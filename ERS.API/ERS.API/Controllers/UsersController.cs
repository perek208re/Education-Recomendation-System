using ERS.API.DTOs;
using ERS.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ERS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository rep;

        public UsersController(IUserRepository rep)
        {
            this.rep = rep;
        }

        [HttpPost("sync")]
        public async Task<IActionResult> SyncUser([FromBody] User request)
        {
            await rep.UpsertUserAsync(request.UserId, request.SkillsToLearn, request.LearnedSkills);
            return Ok(new { Message = "User profile synchronized." });
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserProfile(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("UserId cannot be empty.");

            var exists = await rep.UserExistsAsync(userId);
            if (!exists)
                return NotFound($"User '{userId}' not found.");

            var skillsToLearn = await rep.GetUserDesiredSkillNamesAsync(userId);
            var learnedSkills = await rep.GetUserLearnedSkillNamesAsync(userId);

            return Ok(new
            {
                UserId = userId,
                SkillsToLearn = skillsToLearn,
                LearnedSkills = learnedSkills
            });
        }
    }
}
