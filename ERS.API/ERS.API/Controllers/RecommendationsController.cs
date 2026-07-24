using ERS.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ERS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationsController : ControllerBase
    {
        private readonly IRecommendService service;
        private readonly ICourseRepository courseRep;

        public RecommendationsController(IRecommendService service, ICourseRepository courseRep)
        {
            this.service = service;
            this.courseRep = courseRep;
        }

        [HttpGet("content/{userId}")]
        public async Task<IActionResult> GetContentRecommendations(string userId, [FromQuery] int count = 5)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("UserId cannot be empty.");

            var recCourseIds = await service.GetContentBasedRecAsync(userId, count);

            if (recCourseIds == null || !recCourseIds.Any())
            {
                return NotFound(new { message = "No recommendations found for this user's skills." });
            }

            var coursesDetails = await courseRep.GetCoursesByIdsAsync(recCourseIds);

            var response = recCourseIds
                .Select(id => coursesDetails.FirstOrDefault(c => c.Id == id))
                .Where(c => c != null)
                .Select(c => new
                {
                    CourseId = c.Id,
                    Title = c.Title,
                    Url = c.Url
                });

            return Ok(response);
        }

        [HttpGet("collaborative/{userId}")]
        public async Task<IActionResult> GetCollaborativeRecommendations(string userId, [FromQuery] int k = 5)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("UserId cannot be empty.");

            var recCourseIds = await service.GetCollaborativeRecAsync(userId, k);

            if (recCourseIds == null || !recCourseIds.Any())
            {
                return Ok(new
                {
                    Message = "Cold start detected. Please use the /content endpoint.",
                    Recommendations = new List<object>()      
                });
            }

            var coursesDetails = await courseRep.GetCoursesByIdsAsync(recCourseIds);

            var response = recCourseIds
                .Select(id => coursesDetails.FirstOrDefault(c => c.Id == id))
                .Where(c => c != null)
                .Select(c => new
                {
                    CourseId = c.Id,
                    Title = c.Title,
                    Url = c.Url
                });

            return Ok(response);
        }
    }
}