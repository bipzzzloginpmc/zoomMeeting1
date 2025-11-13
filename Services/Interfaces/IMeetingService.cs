using System.Collections.Generic;
using System.Threading.Tasks;
using ZoomMeetingAPI.DTOs;

namespace ZoomMeetingAPI.Services.Interfaces
{
    public interface IMeetingService
    {
        Task<ZoomMeetingResponseDto> CreateZoomLiveClassAsync(ZoomMeetingDto meetingDto);
        Task<ZoomMeetingResponseDto> GetMeetingAsync(string meetingId);
    }
}