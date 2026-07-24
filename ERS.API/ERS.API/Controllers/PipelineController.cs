using ERS.Core.DataPipeline;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ERS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PipelineController : ControllerBase
    {
        private readonly DataPipelineManager manager;

        public PipelineController(DataPipelineManager manager)
        {
            this.manager = manager;
        }

        [HttpPost("run-import")]
        public async Task<IActionResult> RunImport()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                await manager.RunPipelineAsync();

                stopwatch.Stop();

                return Ok(new
                {
                    Message = "Pipeline executed successfully! Database populated.",
                    ExecutionTimeSeconds = stopwatch.Elapsed.TotalSeconds
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "An error occurred during pipeline execution.",
                    Details = ex.Message
                });
            }
        }
    }
}