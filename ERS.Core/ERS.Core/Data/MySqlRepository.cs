using ERS.Core.Interfaces;
using ERS.Core.Model;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ERS.Core.Data
{
    public class MySqlRepository : ICourseRepository, IUserRepository, IInteractionRepository
    {
        private readonly string connectionString;

        public MySqlRepository(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be empty.");

            this.connectionString = connectionString;
        }

        private async Task<T> ExecuteAsync<T>(Func<MySqlConnection, Task<T>> query)
        {
            using var con = new MySqlConnection(connectionString);
            await con.OpenAsync();
            return await query(con);
        }

        private async Task ExecuteWithTransactionAsync(Func<MySqlConnection, MySqlTransaction, Task> action)
        {
            using var con = new MySqlConnection(connectionString);
            await con.OpenAsync();
            using var trans = await con.BeginTransactionAsync();

            try
            {
                await action(con, trans);
                await trans.CommitAsync();
            }
            catch
            {
                await trans.RollbackAsync();
                throw;
            }
        }

        private async Task<T> ExecuteWithTransactionAsync<T>(Func<MySqlConnection, MySqlTransaction, Task<T>> action)
        {
            using var con = new MySqlConnection(connectionString);
            await con.OpenAsync();
            using var trans = await con.BeginTransactionAsync();

            try
            {
                var result = await action(con, trans);
                await trans.CommitAsync();
                return result;
            }
            catch
            {
                await trans.RollbackAsync();
                throw;
            }
        }

        public async Task InsertCoursesAsync(IEnumerable<Course> courses)
        {
            await ExecuteWithTransactionAsync(async (con, trans) =>
            {
                string sql = @"
                INSERT IGNORE INTO Courses (id, name, title, description, category, url, level) 
                VALUES (@id, @name, @title, @description, @category, @url, @level)";

                using var com = new MySqlCommand(sql, con, trans);

                com.Parameters.Add("@id", MySqlDbType.VarChar, 36);
                com.Parameters.Add("@name", MySqlDbType.VarChar, 255);
                com.Parameters.Add("@title", MySqlDbType.VarChar, 255);
                com.Parameters.Add("@description", MySqlDbType.Text);
                com.Parameters.Add("@category", MySqlDbType.Int32);
                com.Parameters.Add("@url", MySqlDbType.VarChar, 500);
                com.Parameters.Add("@level", MySqlDbType.Int32);

                foreach (var course in courses)
                {
                    com.Parameters["@id"].Value = course.Id;
                    com.Parameters["@name"].Value = course.Name;
                    com.Parameters["@title"].Value = course.Title;
                    com.Parameters["@description"].Value = course.Description;
                    com.Parameters["@category"].Value = course.CategoryId;
                    com.Parameters["@url"].Value = course.Url;
                    com.Parameters["@level"].Value = course.Level;

                    await com.ExecuteNonQueryAsync();
                }
            });
        }

        public async Task UpdateCourseVectorsAsync(Dictionary<string, Dictionary<int, double>> courseVectors)
        {
            await ExecuteWithTransactionAsync(async (con, trans) =>
            {
                string sql = "UPDATE Courses SET vector = @vector WHERE id = @id";
                using var com = new MySqlCommand(sql, con, trans);

                com.CommandTimeout = 3600;

                com.Parameters.Add("@vector", MySqlDbType.JSON);
                com.Parameters.Add("@id", MySqlDbType.VarChar, 36);

                foreach (var kvp in courseVectors)
                {
                    com.Parameters["@id"].Value = kvp.Key;
                    com.Parameters["@vector"].Value = JsonSerializer.Serialize(kvp.Value);
                    await com.ExecuteNonQueryAsync();
                }
            });
        }

        public async Task<Dictionary<string, int>> SyncCategoriesAsync(IEnumerable<string> categoryNames)
        {
            return await ExecuteWithTransactionAsync(async (con, trans) =>
            {
                var result = new Dictionary<string, int>();

                string insertSql = "INSERT IGNORE INTO Categories (name) VALUES (@name)";
                using var insertCmd = new MySqlCommand(insertSql, con, trans);
                insertCmd.Parameters.Add("@name", MySqlDbType.VarChar, 100);

                foreach (var name in categoryNames)
                {
                    insertCmd.Parameters["@name"].Value = name;
                    await insertCmd.ExecuteNonQueryAsync();
                }

                string selectSql = "SELECT id, name FROM Categories";
                using var selectCmd = new MySqlCommand(selectSql, con, trans);
                using var reader = await selectCmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result[reader.GetString("name")] = reader.GetInt32("id");
                }

                return result;
            });
        }

        public async Task<Dictionary<string, string>> GetAllCourseTextsAsync()
        {
            return await ExecuteAsync(async con =>
            {
                var result = new Dictionary<string, string>();
                string sql = "SELECT id, CONCAT(COALESCE(title, ''), ' ', COALESCE(description, '')) AS CourseText FROM Courses";

                using var command = new MySqlCommand(sql, con);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(reader.GetString("id"), reader.GetString("CourseText"));
                }
                return result;
            });
        }

        public async Task<string> GetUserDesiredSkillsTextAsync(string userId)
        {
            return await ExecuteAsync(async con =>
            {
                string sql = @"
                    SELECT GROUP_CONCAT(s.name SEPARATOR ' ') 
                    FROM SkillsToLearn stl
                    JOIN Skills s ON stl.skills = s.id
                    WHERE stl.user_id = @userId";

                using var command = new MySqlCommand(sql, con);
                command.Parameters.AddWithValue("@userId", userId);

                var result = await command.ExecuteScalarAsync();
                return result?.ToString() ?? "";
            });
        }

        public async Task<List<Course>> GetCoursesByIdsAsync(IEnumerable<string> courseIds)
        {
            return await ExecuteAsync(async con =>
            {
                var result = new List<Course>();
                if (!courseIds.Any()) return result;

                var inParameters = string.Join(",", courseIds.Select((s, i) => $"@p{i}"));
                string sql = $"SELECT id, title, url FROM Courses WHERE id IN ({inParameters})";

                using var com = new MySqlCommand(sql, con);
                int index = 0;
                foreach (var id in courseIds)
                {
                    com.Parameters.AddWithValue($"@p{index++}", id);
                }

                using var reader = await com.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new Course
                    {
                        Id = reader.GetString("id"),
                        Title = !reader.IsDBNull(reader.GetOrdinal("title")) ? reader.GetString("title") : "Без назви",
                        Url = !reader.IsDBNull(reader.GetOrdinal("url")) ? reader.GetString("url") : "#"
                    });
                }
                return result;
            });
        }

        public async Task<List<string>> GetUserLearnedSkillNamesAsync(string userId)
        {
            return await ExecuteAsync(async con =>
            {
                var result = new List<string>();
                string sql = @"
                    SELECT s.name 
                    FROM LearnedSkills ls
                    JOIN Skills s ON ls.skills = s.id
                    WHERE ls.user_id = @userId";

                using var command = new MySqlCommand(sql, con);
                command.Parameters.AddWithValue("@userId", userId);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(reader.GetString("name"));
                }
                return result;
            });
        }

        public async Task<Dictionary<string, Dictionary<int, double>>> GetAllCourseVectorsAsync()
        {
            return await ExecuteAsync(async con =>
            {
                var result = new Dictionary<string, Dictionary<int, double>>();
                string sql = "SELECT id, vector FROM Courses WHERE vector IS NOT NULL";

                using var com = new MySqlCommand(sql, con);
                using var reader = await com.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    string id = reader.GetString("id");
                    string jsonVector = reader.GetString("vector");

                    var vector = JsonSerializer.Deserialize<Dictionary<int, double>>(jsonVector);
                    if (vector != null)
                    {
                        result.Add(id, vector);
                    }
                }
                return result;
            });
        }

        public async Task<Dictionary<string, Dictionary<string, double>>> GetAllInteractionsAsync()
        {
            return await ExecuteAsync(async con =>
            {
                var result = new Dictionary<string, Dictionary<string, double>>();
                string sql = "SELECT user_id, course_id, rating FROM Interactions WHERE rating IS NOT NULL";

                using var com = new MySqlCommand(sql, con);
                using var reader = await com.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    string userId = reader.GetString("user_id");
                    string courseId = reader.GetString("course_id");
                    double rating = reader.GetDouble("rating");

                    if (!result.ContainsKey(userId))
                    {
                        result[userId] = new Dictionary<string, double>();
                    }
                    result[userId][courseId] = rating;
                }
                return result;
            });
        }

        public async Task AddOrUpdateInteractionAsync(string userId, string courseId, double rating)
        {
            await ExecuteWithTransactionAsync(async (con, trans) =>
            {
                string ensureUserSql = "INSERT IGNORE INTO Users (id) VALUES (@userId)";
                using var userCmd = new MySqlCommand(ensureUserSql, con, trans);
                userCmd.Parameters.AddWithValue("@userId", userId);
                await userCmd.ExecuteNonQueryAsync();

                string sql = @"
                INSERT INTO Interactions (user_id, course_id, rating) 
                VALUES (@userId, @courseId, @rating)
                ON DUPLICATE KEY UPDATE rating = VALUES(rating), time = CURRENT_TIMESTAMP";

                using var com = new MySqlCommand(sql, con, trans);
                com.Parameters.AddWithValue("@userId", userId);
                com.Parameters.AddWithValue("@courseId", courseId);
                com.Parameters.AddWithValue("@rating", rating);

                await com.ExecuteNonQueryAsync();
            });
        }

        public async Task<bool> UserExistsAsync(string userId)
        {
            return await ExecuteAsync(async con =>
            {
                var sql = "SELECT COUNT(1) FROM Users WHERE id = @UserId";
                using var cmd = new MySqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@UserId", userId);

                var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return count > 0;
            });
        }

        public async Task UpsertUserAsync(string userId, List<string> skillsToLearn, List<string> learnedSkills)
        {
            skillsToLearn ??= new List<string>();
            learnedSkills ??= new List<string>();

            var allUniqueSkills = new HashSet<string>(skillsToLearn.Concat(learnedSkills));

            await ExecuteWithTransactionAsync(async (con, trans) =>
            {
                string ensureUserSql = "INSERT IGNORE INTO Users (id) VALUES (@userId)";
                using var userCmd = new MySqlCommand(ensureUserSql, con, trans);
                userCmd.Parameters.AddWithValue("@userId", userId);
                await userCmd.ExecuteNonQueryAsync();

                if (allUniqueSkills.Count > 0)
                {
                    string insertSkillSql = "INSERT IGNORE INTO Skills (name) VALUES (@name)";
                    using var insertSkillCmd = new MySqlCommand(insertSkillSql, con, trans);
                    insertSkillCmd.Parameters.Add("@name", MySqlDbType.VarChar, 100);

                    foreach (var skill in allUniqueSkills)
                    {
                        insertSkillCmd.Parameters["@name"].Value = skill;
                        await insertSkillCmd.ExecuteNonQueryAsync();
                    }
                }

                var skillIds = new Dictionary<string, int>();
                if (allUniqueSkills.Count > 0)
                {
                    var inClause = string.Join(",", allUniqueSkills.Select((s, i) => $"@p{i}"));
                    string selectSql = $"SELECT id, name FROM Skills WHERE name IN ({inClause})";
                    using var selectCmd = new MySqlCommand(selectSql, con, trans);

                    int pIndex = 0;
                    foreach (var skill in allUniqueSkills)
                    {
                        selectCmd.Parameters.AddWithValue($"@p{pIndex++}", skill);
                    }

                    using var reader = await selectCmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        skillIds[reader.GetString("name")] = reader.GetInt32("id");
                    }
                }

                string deleteSql = "DELETE FROM SkillsToLearn WHERE user_id = @userId; DELETE FROM LearnedSkills WHERE user_id = @userId;";
                using var deleteCmd = new MySqlCommand(deleteSql, con, trans);
                deleteCmd.Parameters.AddWithValue("@userId", userId);
                await deleteCmd.ExecuteNonQueryAsync();

                if (skillsToLearn.Count > 0)
                {
                    string insertS2LSql = "INSERT INTO SkillsToLearn (user_id, skills) VALUES (@userId, @skillId)";
                    using var cmd = new MySqlCommand(insertS2LSql, con, trans);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.Add("@skillId", MySqlDbType.Int32);

                    foreach (var skill in skillsToLearn)
                    {
                        if (skillIds.TryGetValue(skill, out int sId))
                        {
                            cmd.Parameters["@skillId"].Value = sId;
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }

                if (learnedSkills.Count > 0)
                {
                    string insertLSSql = "INSERT INTO LearnedSkills (user_id, skills) VALUES (@userId, @skillId)";
                    using var cmd = new MySqlCommand(insertLSSql, con, trans);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.Add("@skillId", MySqlDbType.Int32);

                    foreach (var skill in learnedSkills)
                    {
                        if (skillIds.TryGetValue(skill, out int sId))
                        {
                            cmd.Parameters["@skillId"].Value = sId;
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            });
        }

        public async Task<List<string>> GetAllSkillsAsync()
        {
            return await ExecuteAsync(async con =>
            {
                var result = new List<string>();
                string sql = "SELECT name FROM Skills ORDER BY name ASC";

                using var com = new MySqlCommand(sql, con);
                using var reader = await com.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(reader.GetString("name"));
                }
                return result;
            });
        }

        public async Task<Dictionary<int, string>> GetAllCategoriesAsync()
        {
            return await ExecuteAsync(async con =>
            {
                var result = new Dictionary<int, string>();
                string sql = "SELECT id, name FROM Categories ORDER BY name ASC";

                using var com = new MySqlCommand(sql, con);
                using var reader = await com.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result[reader.GetInt32("id")] = reader.GetString("name");
                }
                return result;
            });
        }

        public async Task<List<string>> GetUserDesiredSkillNamesAsync(string userId)
        {
            return await ExecuteAsync(async con =>
            {
                var result = new List<string>();
                string sql = @"
                SELECT s.name 
                FROM SkillsToLearn stl
                JOIN Skills s ON stl.skills = s.id
                WHERE stl.user_id = @userId";

                using var command = new MySqlCommand(sql, con);
                command.Parameters.AddWithValue("@userId", userId);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(reader.GetString("name"));
                }
                return result;
            });
        }

        public async Task<Dictionary<string, double>> GetUserInteractionsAsync(string userId)
        {
            return await ExecuteAsync(async con =>
            {
                var result = new Dictionary<string, double>();
                string sql = "SELECT course_id, rating FROM Interactions WHERE user_id = @userId";

                using var com = new MySqlCommand(sql, con);
                com.Parameters.AddWithValue("@userId", userId);

                using var reader = await com.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result[reader.GetString("course_id")] = reader.GetDouble("rating");
                }
                return result;
            });
        }
    }
}