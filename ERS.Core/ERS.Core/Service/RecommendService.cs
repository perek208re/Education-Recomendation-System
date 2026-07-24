using ERS.Core.Algorithms;
using ERS.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ERS.Core.Service
{
    public class RecommendService : IRecommendService
    {
        private readonly ICourseRepository course_rep;
        private readonly IUserRepository user_rep;
        private readonly IInteractionRepository iter_rep;
        private readonly TF_IDF tf_idf;
        private readonly IMemoryCache cache;

        private const string VECTORS_CACHE_KEY = "CourseVectors";
        private const string INTERACTIONS_CACHE_KEY = "UserInteractions";
        private const string TEXTS_CACHE_KEY = "CourseTexts";

        public RecommendService(
            ICourseRepository repository
            , IUserRepository user_rep
            , IInteractionRepository iter_rep
            , TF_IDF tfIdf, IMemoryCache cache)
        {
            this.course_rep = repository;
            this.user_rep = user_rep;
            this.iter_rep = iter_rep;
            tf_idf = tfIdf;
            this.cache = cache;
        }

        public async Task WarmUpAsync()
        {
            if (!tf_idf.IsFitted)
            {
                var text = await GetCachedCourseTextsAsync();
                tf_idf.Fit(text.Values);
            }

            await GetCachedCourseVectorsAsync();
        }

        public async Task<List<string>?> GetContentBasedRecAsync(string userId, int n = 5)
        {
            string skillsToLearn = await user_rep.GetUserDesiredSkillsTextAsync(userId);

            if (string.IsNullOrWhiteSpace(skillsToLearn))
                return new List<string>();

            var queryVector = tf_idf.CreateVector(skillsToLearn);       

            if (queryVector.Count == 0)         
                return new List<string>();

            var courseVectors = await GetCachedCourseVectorsAsync();
            List<string> learnedSkills = await user_rep.GetUserLearnedSkillNamesAsync(userId);

            var scoredCourses = new List<(string CourseId, double Score)>();
            foreach (var course in courseVectors)
            {
                double score = Similarities.CosSimilarity(queryVector, course.Value);
                if (score > 0) scoredCourses.Add((course.Key, score));
            }

            var sortedCandidates = scoredCourses
                .OrderByDescending(c => c.Score)
                .Select(c => c.CourseId)
                .ToList();

            if (learnedSkills != null && learnedSkills.Any())
            {
                var courseTexts = await GetCachedCourseTextsAsync();
                var finalrec = new List<string>();

                var skillRegexes = learnedSkills
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(skill => new Regex($@"\b{Regex.Escape(skill)}\b", RegexOptions.IgnoreCase | RegexOptions.Compiled))
                    .ToList();

                foreach (var courseId in sortedCandidates)
                {
                    if (courseTexts.TryGetValue(courseId, out string courseText))
                    {
                        bool containsKnownSkill = skillRegexes.Any(regex => regex.IsMatch(courseText));

                        if (!containsKnownSkill)
                        {
                            finalrec.Add(courseId);
                        }
                    }
                    else
                    {
                        finalrec.Add(courseId);
                    }

                    if (finalrec.Count == n)
                        break;
                }

                return finalrec;
            }

            return sortedCandidates.Take(n).ToList();
        }

        public async Task<List<string>?> GetCollaborativeRecAsync(string userId, int k = 5)
        {
            var allInt = await GetCachedInteractionsAsync();
            if (!allInt.TryGetValue(userId, out var targetRatings) || targetRatings.Count == 0)
                return new List<string>();

            var knn = new KNN();
            return knn.GetRecommendations(userId, allInt, k, topN: 5);
        }

        private async Task<Dictionary<string, Dictionary<int, double>>?> GetCachedCourseVectorsAsync()
        {
            return await cache.GetOrCreateAsync(VECTORS_CACHE_KEY, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);
                return await course_rep.GetAllCourseVectorsAsync();
            });
        }

        private async Task<Dictionary<string, Dictionary<string, double>>?> GetCachedInteractionsAsync()
        {
            return await cache.GetOrCreateAsync(INTERACTIONS_CACHE_KEY, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await iter_rep.GetAllInteractionsAsync();
            });
        }
        private async Task<Dictionary<string, string>?> GetCachedCourseTextsAsync()
        {
            return await cache.GetOrCreateAsync(TEXTS_CACHE_KEY, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);
                return await course_rep.GetAllCourseTextsAsync();
            });
        }
    }
}