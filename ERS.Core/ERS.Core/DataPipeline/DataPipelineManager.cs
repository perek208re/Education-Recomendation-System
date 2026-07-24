using ERS.Core.Algorithms;
using ERS.Core.Interfaces;
using ERS.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERS.Core.DataPipeline
{
    public class DataPipelineManager
    {
        private readonly IEnumerable<ICourseExtractor> extractors;
        private readonly ICourseRepository rep;
        private readonly TF_IDF tf_idf;

        public DataPipelineManager(
            IEnumerable<ICourseExtractor> extractors,
            ICourseRepository repository,
            TF_IDF tfIdf)
        {
            this.extractors = extractors;
            rep = repository;
            tf_idf = tfIdf;
        }

        public async Task RunPipelineAsync(CancellationToken cancellationToken = default)
        {
            var allCourses = new List<Course>();
            var uniqueCategories = new HashSet<string>();

            foreach (var extractor in extractors)
            {
                await foreach (var course in extractor.ExtractAsync(cancellationToken))
                {
                    allCourses.Add(course);
                    if (!string.IsNullOrWhiteSpace(course.CategoryName))
                        uniqueCategories.Add(course.CategoryName);
                }
            }

            var categoryLookup = await rep.SyncCategoriesAsync(uniqueCategories);
            foreach (var course in allCourses)
            {
                if (categoryLookup.TryGetValue(course.CategoryName, out int catId))
                    course.CategoryId = catId;
            }

            await rep.InsertCoursesAsync(allCourses);

            var dbCourseTexts = await rep.GetAllCourseTextsAsync();

            tf_idf.Fit(dbCourseTexts.Values);

            var courseVectors = new Dictionary<string, Dictionary<int, double>>();
            foreach (var kvp in dbCourseTexts)
            {
                courseVectors[kvp.Key] = tf_idf.CreateVector(kvp.Value ?? "");
            }

            await rep.UpdateCourseVectorsAsync(courseVectors);
        }
    }
}