using Laparoscope.Utils.Authorization;
using LaparoscopeShared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamJsonRpc;
using System.IO.Pipes;

namespace Laparoscope.Controllers.ServerApi
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Provides access to sync run step results from Get-ADSyncRunStepResult.
    /// Returns detailed information about individual steps within a sync run profile.
    /// </summary>
    [Authorize(AuthenticationSchemes = Global.AuthSchemes, Policy = "HasRoles")]
    public class RunStepResultController : Controller
    {
        /// <summary>
        /// Gets run step results for a specific run history ID.
        /// Executes Get-ADSyncRunStepResult and returns the step-level details.
        /// </summary>
        /// <param name="RunHistoryId">The GUID of the run history to get step results for. Required.</param>
        /// <returns>
        /// Collection of RunHistoryEntry objects containing step-level details including:
        /// - Step counters (adds, updates, deletes, etc.)
        /// - Disconnector information
        /// - Export/Import statistics
        /// - Step-level errors and status
        /// </returns>
        /// <example>
        /// GET /api/RunStepResult?RunHistoryId=8a654dc1-cf23-40ed-86f4-b745bd553c22
        /// 
        /// Returns:
        /// [
        ///   {
        ///     "RunHistoryId": "8a654dc1-cf23-40ed-86f4-b745bd553c22",
        ///     "StepNumber": 1,
        ///     "StepResult": "success",
        ///     "StartDate": "2024-01-15T10:30:00",
        ///     "EndDate": "2024-01-15T10:35:00",
        ///     "StageAdd": 5,
        ///     "StageUpdate": 12,
        ///     "StageDelete": 0,
        ///     ...
        ///   }
        /// ]
        /// </example>
        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] Guid? RunHistoryId = null)
        {
            if (RunHistoryId == null || RunHistoryId == Guid.Empty)
            {
                return BadRequest(new { error = "RunHistoryId is required" });
            }

            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync();
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "GetADSyncRunStepResult";
                    var result = await jsonRpc.InvokeAsync<IEnumerable<RunStepResult>>(function, RunHistoryId);

                    // Apply authorization filtering
                    result = this.WhereAuthorized(result, "RunStepResult");

                    return Ok(result);
                }
            }
        }

        /// <summary>
        /// Gets run step results with optional filtering by step number.
        /// </summary>
        /// <param name="RunHistoryId">The GUID of the run history.</param>
        /// <param name="StepNumber">Filter to a specific step number. Optional.</param>
        /// <returns>
        /// Collection of RunHistoryEntry objects containing step-level details.
        /// </returns>
        /// <example>
        /// GET /api/RunStepResult/Filtered?RunHistoryId=8a654dc1-cf23-40ed-86f4-b745bd553c22&StepNumber=1
        /// </example>
        [HttpGet("Filtered")]
        public async Task<IActionResult> GetFilteredAsync(
            [FromQuery] Guid? RunHistoryId = null,
            [FromQuery] int? StepNumber = null)
        {
            if (RunHistoryId == Guid.Empty)
            {
                return BadRequest(new { error = "Valid RunHistoryId is required" });
            }

            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync();
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "GetADSyncRunStepResult";
                    var result = await jsonRpc.InvokeAsync<IEnumerable<RunStepResult>>(
                        function, 
                        RunHistoryId, 
                        StepNumber, 
                        false);

                    return Ok(result);
                }
            }
        }

        /// <summary>
        /// Gets run step results for a specific run history ID by route parameter.
        /// </summary>
        /// <param name="RunHistoryId">The GUID of the run history to get step results for.</param>
        /// <returns>Collection of RunHistoryEntry objects containing step-level details.</returns>
        /// <example>
        /// GET /api/RunStepResult/8a654dc1-cf23-40ed-86f4-b745bd553c22
        /// </example>
        [HttpGet("{runHistoryId:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid RunHistoryId)
        {
            if (RunHistoryId == Guid.Empty)
            {
                return BadRequest(new { error = "Valid RunHistoryId is required" });
            }

            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync();
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "GetADSyncRunStepResult";
                    var result = await jsonRpc.InvokeAsync<IEnumerable<RunStepResult>>(function, RunHistoryId);

                    return Ok(result);
                }
            }
        }

        /// <summary>
        /// Gets a specific step result by run history ID and step number.
        /// </summary>
        /// <param name="RunHistoryId">The GUID of the run history.</param>
        /// <param name="StepNumber">The specific step number to retrieve.</param>
        /// <returns>Single RunHistoryEntry object for the specified step.</returns>
        /// <example>
        /// GET /api/RunStepResult/8a654dc1-cf23-40ed-86f4-b745bd553c22/step/1
        /// </example>
        [HttpGet("{runHistoryId:guid}/step/{stepNumber:int}")]
        public async Task<IActionResult> GetStepByNumberAsync(Guid RunHistoryId, int StepNumber)
        {
            if (RunHistoryId == Guid.Empty)
            {
                return BadRequest(new { error = "Valid RunHistoryId is required" });
            }

            if (StepNumber < 0)
            {
                return BadRequest(new { error = "StepNumber must be a positive integer" });
            }

            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync();
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "GetADSyncRunStepResult";
                    var result = await jsonRpc.InvokeAsync<IEnumerable<RunStepResult>>(
                        function, 
                        RunHistoryId, 
                        StepNumber, 
                        null);

                    // Apply authorization filtering
                    result = this.WhereAuthorized(result, "RunStepResult");

                    var step = result?.FirstOrDefault();

                    if (step == null)
                    {
                        return NotFound(new { error = $"Step {StepNumber} not found for RunHistoryId {RunHistoryId}" });
                    }

                    return Ok(step);
                }
            }
        }
    }
}
