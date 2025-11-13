using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZoomMeetingAPI.DTOs;
using ZoomMeetingAPI.Services.Interfaces;
using ZoomMeetingAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace ZoomMeetingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingsController : ControllerBase
    {
        private readonly IMeetingService _meetingService;
        private readonly ILogger<MeetingsController> _logger;
        private readonly ApplicationDbContext _context;

        public MeetingsController(IMeetingService meetingService,ILogger<MeetingsController> logger, ApplicationDbContext context)
        {
            _meetingService = meetingService ?? throw new ArgumentNullException(nameof(meetingService));
            _logger = logger;
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Create a new Zoom meeting
        /// </summary>
        // [HttpPost]
        // [ProducesResponseType(typeof(ZoomMeetingResponseDto), 201)]
        // [ProducesResponseType(400)]
        // public async Task<IActionResult> CreateMeeting([FromBody] CreateZoomMeetingDto meetingDto)
        // {
        //     try
        //     {
        //         var result = await _meetingService.CreateMeetingAsync(meetingDto);
        //         return CreatedAtAction(nameof(GetMeeting), new { id = result.Id }, result);
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(new { message = ex.Message });
        //     }
        // }

        /// <summary>
        /// Get a meeting by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ZoomMeetingResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetMeeting(string id)
        {
            var meeting = await _meetingService.GetMeetingAsync(id);

            if (meeting==null)
                return NotFound();

            return Ok(meeting);
        }

        /// <summary>
        /// Get all meetings with recording status
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllMeetings()
        {
            try
            {
                var meetings = await _meetingService.GetAllMeetingsAsync();
                return Ok(meetings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting meetings");
                return BadRequest(new { message = ex.Message });
            }
        }

           /// <summary>
        /// Toggle recording for a meeting
        /// </summary>
        // [HttpPost("{meetingId}/toggle-recordingType")]
        // public async Task<IActionResult> ToggleRecordingType(
        //     long meetingId,
        //     [FromBody] ToggleRecordingDto dto)
        // {
        //     try
        //     {
        //         var result = await _meetingService.ToggleRecordingAsync(meetingId, dto.RecordingType);
                
        //         return Ok(new 
        //         { 
        //             success = result,
        //             meetingId,
        //             recordingType = dto.RecordingType,
        //             message = $"Recording set to {dto.RecordingType}"
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error toggling recording");
        //         return BadRequest(new { message = ex.Message });
        //     }
        // }


        /// <summary>
        /// Enable or disable recording for a meeting
        /// </summary>
        /// <param name="meetingId">Meeting ID</param>
        /// <param name="dto">Enable/disable recording</param>
        [HttpPost("{meetingId}/toggle-recording")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ToggleRecording(
            long meetingId,
            [FromBody] ToggleRecordingDto dto)
        {
            try
            {
                var result = await _meetingService.ToggleRecordingAsync(meetingId, dto.EnableRecording);
                
                return Ok(new 
                { 
                    success = result,
                    meetingId,
                    recordingEnabled = dto.EnableRecording,
                    recordingType = dto.EnableRecording ? "cloud" : "none",
                    message = dto.EnableRecording 
                        ? "Recording enabled for this meeting" 
                        : "Recording disabled for this meeting"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling recording for meeting {MeetingId}", meetingId);
                return BadRequest(new { message = ex.Message });
            }
        }

           /// <summary>
        /// Create recurring meeting
        /// </summary>
        [HttpPost("recurring")]
        public async Task<IActionResult> CreateRecurringMeeting([FromBody] RecurringMeetingDto dto)
        {
            try
            {
                var result = await _meetingService.CreateRecurringMeetingAsync(dto);
                return CreatedAtAction(nameof(GetMeetingOccurrences), 
                    new { meetingId = result.MeetingId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating recurring meeting");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all occurrences of a recurring meeting
        /// </summary>
        [HttpGet("{meetingId}/occurrences")]
        public async Task<IActionResult> GetMeetingOccurrences(long meetingId)
        {
            try
            {
                var occurrences = await _meetingService.GetMeetingOccurrencesAsync(meetingId);
                return Ok(new 
                { 
                    meetingId, 
                    count = occurrences.Count,
                    occurrences 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting occurrences");
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a specific occurrence
        /// </summary>
        [HttpDelete("{meetingId}/occurrences/{occurrenceId}")]
        public async Task<IActionResult> DeleteOccurrence(long meetingId, string occurrenceId)
        {
            try
            {
                await _meetingService.DeleteOccurrenceAsync(meetingId, occurrenceId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting occurrence");
                return BadRequest(new { message = ex.Message });
            }
        }
        /// <summary>
        /// Update a meeting
        /// </summary>
        // [HttpPut("{id}")]
        // [ProducesResponseType(typeof(ZoomMeetingResponseDto), 200)]
        // [ProducesResponseType(404)]
        // public async Task<IActionResult> UpdateMeeting(int id, [FromBody] CreateZoomMeetingDto meetingDto)
        // {
        //     try
        //     {
        //         var result = await _meetingService.UpdateMeetingAsync(id, meetingDto);
        //         return Ok(result);
        //     }
        //     catch (KeyNotFoundException)
        //     {
        //         return NotFound();
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(new { message = ex.Message });
        //     }
        // }

        /// <summary>
        /// Delete a meeting
        /// </summary>
        // [HttpDelete("{id}")]
        // [ProducesResponseType(204)]
        // [ProducesResponseType(404)]
        // public async Task<IActionResult> DeleteMeeting(int id)
        // {
        //     var result = await _meetingService.DeleteMeetingAsync(id);

        //     if (!result)
        //         return NotFound();

        //     return NoContent();
        // }

        /// <summary>
        /// Test database connection
        /// </summary>
        [HttpGet("test-connection")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                // Test database connectivity
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    return StatusCode(500, new { message = "Could not connect to the database" });
                }

                // Get database provider and connection string (masked)
                var connection = _context.Database.GetConnectionString();
                var provider = _context.Database.ProviderName;

                return Ok(new { 
                    message = "Successfully connected to the database",
                    provider = provider,
                    connection = "Connection string masked for security"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Database connection test failed: {ex.Message}" });
            }
        }
        ///For ZOOM PLATEFORM MEETING CREATION------------------------------------------------------
        /// <summary>
        /// Create a new Zoom live class meeting
        /// </summary>
        [HttpPost("live-class")]
        [ProducesResponseType(typeof(ZoomMeetingResponseDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateLiveClassMeeting([FromBody] ZoomMeetingDto meetingDto)
        {
            try
            {
                if (meetingDto == null)
                    return BadRequest(new { message = "Meeting data is required" });

                if (string.IsNullOrWhiteSpace(meetingDto.Topic))
                    return BadRequest(new { message = "Topic is required" });

                if (meetingDto.Duration <= 0)
                    return BadRequest(new { message = "Duration must be greater than 0" });

                var result = await _meetingService.CreateZoomLiveClassAsync(meetingDto);
                // return CreatedAtAction(nameof(GetMeeting), new { id = result.Id }, result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to create live class meeting: {ex.Message}" });
            }
        }
    }
}