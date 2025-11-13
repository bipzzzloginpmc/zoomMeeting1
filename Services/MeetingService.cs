using AutoMapper;
using Microsoft.Extensions.Logging;
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
        private ILogger<MeetingService> _logger;

        public MeetingService(
            IZoomMeetingRepository repository,
            IZoomRepository repo,
            IMapper mapper,
            ILogger<MeetingService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _repo=repo??throw new ArgumentNullException(nameof(repo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

        // public async Task<bool> ToggleRecordingAsync(long meetingId, string recordingType)
        // {
        //     try
        //     {
        //         // Update in Zoom
        //         var result = await _repo.ToggleRecordingAsync(meetingId.ToString(), recordingType);

        //         if (result)
        //         {
        //             // Update in database
        //             var dbMeeting = await _repository.GetByMeetingIdAsync(meetingId);
        //             if (dbMeeting != null)
        //             {
        //                 dbMeeting.AutoRecording = recordingType;
        //                 dbMeeting.UpdatedAt = DateTime.UtcNow;
        //                 await _repository.UpdateAsync(dbMeeting);
        //             }
        //         }

        //         return result;
        //     }
        //     catch (Exception ex)
        //     {
        //         throw new Exception($"Failed to toggle recording: {ex.Message}", ex);
        //     }
        // }

        public async Task<List<ZoomMeetingResponseDto>> GetAllMeetingsAsync()
        {
            try
            {
                var meetings = await _repository.GetAllAsync();
                
                return meetings.Select(m => new ZoomMeetingResponseDto
                {
                    Id = m.Id,
                    MeetingId = m.MeetingId,
                    Topic = m.Topic,
                    Type = m.Type,
                    StartTime = m.StartTime,
                    Duration = m.Duration,
                    JoinUrl = m.JoinUrl,
                    StartUrl = m.StartUrl,
                    Password = m.Password,
                    AutoRecording = m.AutoRecording,
                    CreatedAt = m.CreatedAt
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get meetings: {ex.Message}", ex);
            }
        }

        public async Task<ZoomMeetingResponseDto> CreateRecurringMeetingAsync(RecurringMeetingDto dto)
        {
            try
            {
                // Validate
                ValidateRecurringMeeting(dto);

                // Create in Zoom
                var zoomResponse = await _repo.CreateRecurringMeetingAsync(dto);

                // Save to database
                var meeting = new ZoomMeeting
                {
                    MeetingId = zoomResponse.MeetingId ?? 0,
                    Topic = zoomResponse.Topic ?? dto.Topic,
                    Type = zoomResponse.Type ?? (dto.RecurrenceType == RecurrenceType.FixedTime ? 8 : 3),
                    StartTime = zoomResponse.StartTime ?? dto.StartTime,
                    Duration = dto.Duration,
                    Timezone = dto.Timezone ?? "UTC",
                    Agenda = dto.Agenda,
                    JoinUrl = zoomResponse.JoinUrl,
                    StartUrl = zoomResponse.StartUrl,
                    Password = zoomResponse.Password,
                    HostEmail = zoomResponse.HostEmail ?? string.Empty,
                    AutoRecording = dto.AutoRecording ?? "cloud",
                    CreatedAt = DateTime.UtcNow,
                    // Add recurring metadata
                    IsRecurring = true,
                    RecurrencePattern = dto.RecurrencePattern.ToString()
                };

                await _repository.AddAsync(meeting);

                return new ZoomMeetingResponseDto
                {
                    Id = meeting.Id,
                    MeetingId = meeting.MeetingId,
                    Topic = meeting.Topic,
                    Type = meeting.Type,
                    StartTime = meeting.StartTime,
                    Duration = meeting.Duration,
                    JoinUrl = meeting.JoinUrl,
                    StartUrl = meeting.StartUrl,
                    Password = meeting.Password,
                    CreatedAt = meeting.CreatedAt
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create recurring meeting: {ex.Message}", ex);
            }
        }

        public async Task<List<MeetingOccurrenceDto>> GetMeetingOccurrencesAsync(long meetingId)
        {
            try
            {
                return await _repo.GetRecurringMeetingOccurrencesAsync(meetingId.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get occurrences: {ex.Message}", ex);
            }
        }

        public async Task DeleteOccurrenceAsync(long meetingId, string occurrenceId)
        {
            try
            {
                await _repo.DeleteMeetingOccurrenceAsync(meetingId.ToString(), occurrenceId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete occurrence: {ex.Message}", ex);
            }
        }

        private void ValidateRecurringMeeting(RecurringMeetingDto dto)
        {
            if (dto.RecurrenceType == RecurrenceType.FixedTime && !dto.StartTime.HasValue)
            {
                throw new ArgumentException("StartTime is required for fixed time recurring meetings");
            }

            if (dto.RecurrencePattern == RecurrencePattern.Weekly && string.IsNullOrEmpty(dto.WeeklyDays))
            {
                throw new ArgumentException("WeeklyDays is required for weekly recurrence");
            }

            if (dto.RecurrencePattern == RecurrencePattern.Monthly && 
                !dto.MonthlyDay.HasValue && !dto.MonthlyWeek.HasValue)
            {
                throw new ArgumentException("MonthlyDay or MonthlyWeek is required for monthly recurrence");
            }
        }


       public async Task<RecordingToggleResult> ToggleRecordingAsync(long meetingId, bool enableRecording)
        {
            try
            {
                // Update in Zoom and get ACTUAL type that was set
                var result = await _repo.ToggleRecordingAsync(meetingId.ToString(), enableRecording);

                if (result.Success)
                {
                    // Update database with VERIFIED recording type from Zoom
                    var dbMeeting = await _repository.GetByMeetingIdAsync(meetingId);
                    if (dbMeeting != null)
                    {
                        // ✅ Store ACTUAL verified type, not requested type
                        dbMeeting.AutoRecording = result.ActualRecordingType;
                        dbMeeting.UpdatedAt = DateTime.UtcNow;
                        await _repository.UpdateAsync(dbMeeting);
                        
                        _logger.LogInformation(
                            "✅ Database updated: Meeting {MeetingId} recording = '{ActualType}' (verified from Zoom)",
                            meetingId,
                            result.ActualRecordingType
                        );
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Meeting {MeetingId} not found in database",
                            meetingId
                        );
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "Recording toggle failed for meeting {MeetingId}, database not updated",
                        meetingId
                    );
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle recording for meeting {MeetingId}", meetingId);
                throw new Exception($"Failed to toggle recording: {ex.Message}", ex);
            }
        }
    }
}