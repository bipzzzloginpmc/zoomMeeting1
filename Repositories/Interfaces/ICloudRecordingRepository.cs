using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZoomMeetingAPI.DTOs;

namespace ZoomMeetingAPI.Repositories.Interfaces
{
    public interface ICloudRecordingRepository
    {
        /// <summary>
        /// Get cloud recordings for a specific meeting from Zoom API
        /// </summary>
        Task<ZoomCloudRecordingResponse> GetMeetingRecordingsAsync(string meetingId);

        /// <summary>
        /// Get all cloud recordings for the account from Zoom API
        /// </summary>
        Task<ZoomRecordingsListResponse> GetAllRecordingsAsync(DateTime from, DateTime to, int pageSize = 300);

        /// <summary>
        /// Download recording file from Zoom
        /// </summary>
        Task<byte[]> DownloadRecordingFileAsync(string downloadUrl);

        /// <summary>
        /// Delete all recordings for a meeting in Zoom
        /// </summary>
        Task DeleteMeetingRecordingsAsync(string meetingId);

        /// <summary>
        /// Delete a specific recording file in Zoom
        /// </summary>
        Task DeleteRecordingFileAsync(string meetingId, string recordingId);

        /// <summary>
        /// Get recording settings for a meeting
        /// </summary>
        Task<object> GetRecordingSettingsAsync(string meetingId);
    }
}