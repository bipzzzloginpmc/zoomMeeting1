using System.Collections.Generic;
using System.Threading.Tasks;
using ZoomMeetingAPI.DTOs;

namespace ZoomMeetingAPI.Services.Interfaces
{
    public interface IMeetingService
    {
        Task<ZoomMeetingResponseDto> CreateZoomLiveClassAsync(ZoomMeetingDto meetingDto);
        Task<ZoomMeetingResponseDto> GetMeetingAsync(string meetingId);
        // ✅ ADD: Recording toggle
        // Task<bool> ToggleRecordingAsync(long meetingId, string recordingType);
        Task<bool> ToggleRecordingAsync(long meetingId, bool enableRecording);
        Task<List<ZoomMeetingResponseDto>> GetAllMeetingsAsync();
        
        // ✅ ADD: Recurring meetings
        Task<ZoomMeetingResponseDto> CreateRecurringMeetingAsync(RecurringMeetingDto dto);
        Task<List<MeetingOccurrenceDto>> GetMeetingOccurrencesAsync(long meetingId);
        Task DeleteOccurrenceAsync(long meetingId, string occurrenceId);

    }
}