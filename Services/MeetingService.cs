using AutoMapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZoomMeetingAPI.DTOs;
using ZoomMeetingAPI.Models;
using ZoomMeetingAPI.Repositories.Interfaces;
using ZoomMeetingAPI.Services.Interfaces;

namespace ZoomMeetingAPI.Services
{
    public class MeetingService : IMeetingService
    {
        private readonly IZoomMeetingRepository _repository;
        private readonly IZoomRepository _repo;
        private readonly IMapper _mapper;

        public MeetingService(
            IZoomMeetingRepository repository,
            IZoomRepository repo,
            IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _repo=repo??throw new ArgumentNullException(nameof(repo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
       public async Task<ZoomMeetingResponseDto> CreateZoomLiveClassAsync(ZoomMeetingDto meetingDto)
            {
                // 1. Create meeting in Zoom and get response
                var zoomResponse = await _repo.CreateMeetingAsync(meetingDto);
                
                // 2. Save to database
                var meeting = new ZoomMeeting
                {
                    MeetingId = zoomResponse.MeetingId ?? 0,
                    Topic = zoomResponse.Topic ?? string.Empty,
                    Type = zoomResponse.Type ?? 2,
                    StartTime = zoomResponse.StartTime ?? meetingDto.ClassDateTime,
                    Duration = zoomResponse.Duration ?? meetingDto.Duration,
                    Timezone = zoomResponse.Timezone ?? "UTC",
                    Agenda = meetingDto.Agenda ?? zoomResponse.Agenda,
                    JoinUrl = zoomResponse.JoinUrl,
                    StartUrl = zoomResponse.StartUrl,
                    Password = zoomResponse.Password,
                    HostEmail = zoomResponse.HostEmail ?? string.Empty,
                    HostVideo = zoomResponse.Settings?.HostVideo ?? true,
                    ParticipantVideo = zoomResponse.Settings?.ParticipantVideo ?? true,
                    JoinBeforeHost = zoomResponse.Settings?.JoinBeforeHost ?? true,
                    MuteUponEntry = zoomResponse.Settings?.MuteUponEntry ?? true,
                    WaitingRoom = zoomResponse.Settings?.WaitingRoom ?? false,
                    AutoRecording = zoomResponse.Settings?.AutoRecording ?? "cloud",
                    CreatedAt = DateTime.UtcNow
                };

                var savedMeeting = await _repository.AddAsync(meeting);
                
                // 3. ✅ Return response with DB Id populated
                return new ZoomMeetingResponseDto
                {
                    Id = savedMeeting.Id, // Database ID (will be serialized to JSON as "id" when returning)
                    MeetingId = savedMeeting.MeetingId, // Zoom meeting ID (ignored in JSON output due to [JsonIgnore])
                    Topic = savedMeeting.Topic,
                    Type = savedMeeting.Type,
                    StartTime = savedMeeting.StartTime,
                    Duration = savedMeeting.Duration,
                    Timezone = savedMeeting.Timezone,
                    Agenda = savedMeeting.Agenda,
                    JoinUrl = savedMeeting.JoinUrl,
                    StartUrl = savedMeeting.StartUrl,
                    Password = savedMeeting.Password,
                    HostEmail = savedMeeting.HostEmail,
                    CreatedAt = savedMeeting.CreatedAt,
                    HostVideo = savedMeeting.HostVideo,
                    ParticipantVideo = savedMeeting.ParticipantVideo,
                    JoinBeforeHost = savedMeeting.JoinBeforeHost,
                    MuteUponEntry = savedMeeting.MuteUponEntry,
                    WaitingRoom = savedMeeting.WaitingRoom,
                    AutoRecording = savedMeeting.AutoRecording
                };
            }

        public async Task<ZoomMeetingResponseDto> GetMeetingAsync(string meetingId)
        {
            return await _repo.GetMeetingAsync(meetingId);
        }
    }
}