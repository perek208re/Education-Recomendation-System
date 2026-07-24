using ERS.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ERS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DictionariesController : ControllerBase
    {
        private readonly ICourseRepository courseRep;
        private readonly IUserRepository userRep;

        public DictionariesController(ICourseRepository courseRep, IUserRepository userRep)
        {
            this.courseRep = courseRep;
            this.userRep = userRep;
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var cat = await courseRep.GetAllCategoriesAsync();

            var response = cat.Select(c => new { Id = c.Key, Name = c.Value });

            return Ok(response);
        }

        [HttpGet("skills")]
        public async Task<IActionResult> GetSkills()
        {
            var skills = await userRep.GetAllSkillsAsync();
            return Ok(skills);
        }
    }
}