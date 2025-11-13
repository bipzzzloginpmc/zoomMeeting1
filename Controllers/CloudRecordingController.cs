using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ZoomMeetingAPI.DTOs;
using ZoomMeetingAPI.Services.Interfaces;

namespace ZoomMeetingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CloudRecordingController : ControllerBase
    {
        private readonly ICloudRecordingService _recordingService;
        private readonly ILogger<CloudRecordingController> _logger;

        public CloudRecordingController(
            ICloudRecordingService recordingService,
            ILogger<CloudRecordingController> logger)
        {
            _recordingService = recordingService ?? throw new ArgumentNullException(nameof(recordingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get cloud recordings for a specific meeting
        /// </summary>
        /// <param name="meetingId">Zoom meeting ID</param>
        /// <returns>Cloud recording details with all files</returns>
        [HttpGet("meeting/{meetingId}")]
        [ProducesResponseType(typeof(CloudRecordingDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetMeetingRecordings(string meetingId)
        {
            try
            {
                _logger.LogInformation("API: Getting recordings for meeting {MeetingId}", meetingId);

                var recordings = await _recordingService.GetMeetingRecordingsAsync(meetingId);

                if (recordings == null)
                {
                    return NotFound(new
                    {
                        message = "No recordings found for this meeting",
                        detail = "Recording may still be processing (takes 1-2 hours after meeting ends)"
                    });
                }

                return Ok(recordings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting recordings for meeting {MeetingId}", meetingId);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all cloud recordings for the account
        /// </summary>
        /// <param name="from">Start date (default: 30 days ago)</param>
        /// <param name="to">End date (default: today)</param>
        /// <param name="pageSize">Number of results per page (default: 300)</param>
        /// <returns>List of all recordings</returns>
        [HttpGet]
        [ProducesResponseType(typeof(RecordingsListDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetAllRecordings(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] int pageSize = 300)
        {
            try
            {
                _logger.LogInformation("API: Getting all recordings");

                var request = new GetRecordingsRequestDto
                {
                    From = from,
                    To = to,
                    PageSize = pageSize
                };

                var recordings = await _recordingService.GetAllRecordingsAsync(request);

                return Ok(recordings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting all recordings");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Download a specific recording file
        /// </summary>
        /// <param name="downloadUrl">Download URL from Zoom (from recording file)</param>
        /// <param name="fileName">Desired file name (default: recording.mp4)</param>
        /// <returns>Recording file as download</returns>
        [HttpGet("download")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> DownloadRecording(
            [FromQuery] string downloadUrl,
            [FromQuery] string fileName = "recording.mp4")
        {
            try
            {
                if (string.IsNullOrEmpty(downloadUrl))
                {
                    return BadRequest(new { message = "Download URL is required" });
                }

                _logger.LogInformation("API: Downloading recording: {FileName}", fileName);

                var request = new DownloadRecordingRequestDto
                {
                    DownloadUrl = downloadUrl,
                    FileName = fileName
                };

                var (fileBytes, contentType, finalFileName) = await _recordingService.DownloadRecordingAsync(request);

                return File(fileBytes, contentType, finalFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error downloading recording");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete all recordings for a meeting
        /// </summary>
        /// <param name="meetingId">Zoom meeting ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("meeting/{meetingId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> DeleteMeetingRecordings(string meetingId)
        {
            try
            {
                _logger.LogInformation("API: Deleting recordings for meeting {MeetingId}", meetingId);

                var result = await _recordingService.DeleteMeetingRecordingsAsync(meetingId);

                return Ok(new
                {
                    success = result,
                    message = "All recordings deleted successfully",
                    meetingId = meetingId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error deleting recordings for meeting {MeetingId}", meetingId);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a specific recording file
        /// </summary>
        /// <param name="meetingId">Zoom meeting ID</param>
        /// <param name="recordingId">Recording file ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("meeting/{meetingId}/file/{recordingId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> DeleteRecordingFile(string meetingId, string recordingId)
        {
            try
            {
                _logger.LogInformation(
                    "API: Deleting recording file {RecordingId} for meeting {MeetingId}",
                    recordingId,
                    meetingId
                );

                var result = await _recordingService.DeleteRecordingFileAsync(meetingId, recordingId);

                return Ok(new
                {
                    success = result,
                    message = "Recording file deleted successfully",
                    meetingId = meetingId,
                    recordingId = recordingId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "API: Error deleting recording file {RecordingId} for meeting {MeetingId}",
                    recordingId,
                    meetingId
                );
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Check if recording is ready (processed)
        /// </summary>
        /// <param name="meetingId">Zoom meeting ID</param>
        /// <returns>Recording status</returns>
        [HttpGet("meeting/{meetingId}/status")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> CheckRecordingStatus(string meetingId)
        {
            try
            {
                var isReady = await _recordingService.IsRecordingReadyAsync(meetingId);

                return Ok(new
                {
                    meetingId = meetingId,
                    isReady = isReady,
                    status = isReady ? "completed" : "processing",
                    message = isReady
                        ? "Recording is ready for download"
                        : "Recording is still being processed. Please check back in 1-2 hours."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error checking recording status for meeting {MeetingId}", meetingId);
                return Ok(new
                {
                    meetingId = meetingId,
                    isReady = false,
                    status = "error",
                    message = "Unable to check recording status. Recording may not exist."
                });
            }
        }

        /// <summary>
        /// Get recording statistics
        /// </summary>
        /// <param name="meetingId">Zoom meeting ID</param>
        /// <returns>Recording statistics</returns>
        [HttpGet("meeting/{meetingId}/stats")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetRecordingStats(string meetingId)
        {
            try
            {
                var stats = await _recordingService.GetRecordingStatsAsync(meetingId);

                if (stats == null)
                {
                    return NotFound(new { message = "No recordings found for this meeting" });
                }

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting recording stats for meeting {MeetingId}", meetingId);
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}