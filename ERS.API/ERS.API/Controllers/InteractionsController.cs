using ERS.API.Controllers;
using ERS.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class InteractionsController : ControllerBase
{
    private readonly IInteractionRepository rep;   

    public InteractionsController(IInteractionRepository repository)
    {
        rep = repository;
    }

    [HttpPost("rate")]
    public async Task<IActionResult> RateCourse([FromBody] Interaction request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.CourseId))
            return BadRequest("UserId and CourseId are required.");

        if (request.Rating < 1.0 || request.Rating > 5.0)
            return BadRequest("The score must be between 1.0 and 5.0.");

        await rep.AddOrUpdateInteractionAsync(request.UserId, request.CourseId, request.Rating);

        return Ok(new { Message = "The rating has been successfully saved!" });
    }
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserInteractions(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("UserId cannot be empty.");

        var interaction = await rep.GetUserInteractionsAsync(userId);

        var response = interaction.Select(i => new { CourseId = i.Key, Rating = i.Value });
        return Ok(response);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllInteractions()
    {
        var allInteractions = await rep.GetAllInteractionsAsync();
        return Ok(allInteractions);
    }
}