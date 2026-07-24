using ERS.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERS.Core.DataPipeline.Udemy
{
    public class UdemyCourseAdapter
    {
        public static Course? Adapt(UdemyCsvRecord record)
        {
            if (record == null) return null;

            string fullDescription = $"{record.headline} {record.objectives} {record.curriculum}";

            return new Course
            {
                Id = $"udemy_{record.id}",
                Name = TextLimiter(record.title, 255) ?? "Untitled",
                Title = TextLimiter(record.title, 255) ?? "",
                Description = $"{record.title} {record.headline} {record.objectives} {record.curriculum}",
                CategoryName = TextLimiter(record.category, 100) ?? "General",
                Url = record.url,
                Level = ParseLevel(record.instructional_level)
            };

        }
        private static int ParseLevel(string level)
        {
            return level?.ToLower() switch
            {
                "all levels" => 0,
                "beginner level" => 1,
                "intermediate level" => 2,
                "expert level" => 3,
                _ => 1
            };
        }

        private static string TextLimiter(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}