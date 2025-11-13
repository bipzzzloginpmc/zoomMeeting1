using System.Collections.Generic;
using System.Threading.Tasks;
using ZoomMeetingAPI.DTOs;

namespace ZoomMeetingAPI.Repositories.Interfaces
{
    public interface IZoomRepository
    {
        Task<ZoomMeetingResponseDto> CreateMeetingAsync(ZoomMeetingDto dto);
        Task<ZoomMeetingResponseDto> GetMeetingAsync(string meetingId);
        Task<List<string>> AddInviteesAsync(string meetingId, List<InviteeDto> invitees);

        // ✅ ADD: Recording toggle
        // Task<bool> ToggleRecordingAsync(string meetingId, string recordingType);
        
        // ✅ ADD: Recurring meetings
        Task<ZoomMeetingResponseDto> CreateRecurringMeetingAsync(RecurringMeetingDto dto);
        Task<List<MeetingOccurrenceDto>> GetRecurringMeetingOccurrencesAsync(string meetingId);
        Task DeleteMeetingOccurrenceAsync(string meetingId, string occurrenceId);
         Task<RecordingToggleResult> ToggleRecordingAsync(string meetingId, bool enableRecording);
    }
}
