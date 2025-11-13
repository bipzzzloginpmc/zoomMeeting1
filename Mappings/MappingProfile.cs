using AutoMapper;
using ZoomMeetingAPI.DTOs;
using ZoomMeetingAPI.Models;

namespace ZoomMeetingAPI.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ZoomMeeting, ZoomMeetingResponseDto>();
            CreateMap<CreateZoomMeetingDto, ZoomMeeting>();
        }
    }
}