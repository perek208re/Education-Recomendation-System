using ERS.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERS.Core.Interfaces
{
    public interface ICourseRepository
    {
        Task InsertCoursesAsync(IEnumerable<Course> courses);
        Task UpdateCourseVectorsAsync(Dictionary<string, Dictionary<int, double>> courseVectors);
        Task<Dictionary<string, int>> SyncCategoriesAsync(IEnumerable<string> names);
        Task<Dictionary<string, string>> GetAllCourseTextsAsync();
        Task<List<Course>> GetCoursesByIdsAsync(IEnumerable<string> courseIds);
        Task<Dictionary<string, Dictionary<int, double>>> GetAllCourseVectorsAsync();
        Task<Dictionary<int, string>> GetAllCategoriesAsync();

    }
}
