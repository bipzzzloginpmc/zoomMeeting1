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
    }
}
