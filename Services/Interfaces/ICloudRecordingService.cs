using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZoomMeetingAPI.DTOs;

namespace ZoomMeetingAPI.Services.Interfaces
{
    public interface ICloudRecordingService
    {
        /// <summary>
        /// Get cloud recordings for a specific meeting with business logic
        /// </summary>
        Task<CloudRecordingDto> GetMeetingRecordingsAsync(string meetingId);

        /// <summary>
        /// Get all cloud recordings with filtering and mapping
        /// </summary>
        Task<RecordingsListDto> GetAllRecordingsAsync(GetRecordingsRequestDto request);

        /// <summary>
        /// Download a recording file
        /// </summary>
        Task<(byte[] fileBytes, string contentType, string fileName)> DownloadRecordingAsync(
            DownloadRecordingRequestDto request);

        /// <summary>
        /// Delete all recordings for a meeting
        /// </summary>
        Task<bool> DeleteMeetingRecordingsAsync(string meetingId);

        /// <summary>
        /// Delete a specific recording file
        /// </summary>
        Task<bool> DeleteRecordingFileAsync(string meetingId, string recordingId);

        /// <summary>
        /// Check if recording is ready (processed)
        /// </summary>
        Task<bool> IsRecordingReadyAsync(string meetingId);

        /// <summary>
        /// Get recording statistics
        /// </summary>
        Task<object> GetRecordingStatsAsync(string meetingId);
    }
}