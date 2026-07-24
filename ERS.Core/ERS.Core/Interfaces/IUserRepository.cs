using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERS.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> UserExistsAsync(string userId);
        Task UpsertUserAsync(string userId, List<string> skillsToLearn, List<string> learnedSkills);
        Task<List<string>> GetUserDesiredSkillNamesAsync(string userId);
        Task<string> GetUserDesiredSkillsTextAsync(string userId);
        Task<List<string>> GetUserLearnedSkillNamesAsync(string userId);
        Task<List<string>> GetAllSkillsAsync();

    }
}
