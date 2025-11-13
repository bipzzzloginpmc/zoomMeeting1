using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using ZoomMeetingAPI.DTOs;
using ZoomMeetingAPI.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace ZoomMeetingAPI.Repositories
{
    public class ZoomRepository : IZoomRepository
    {
        private readonly HttpClient _httpClient;
        private readonly ZoomAuthService _authService;
        private readonly ILogger<ZoomRepository> _logger;

        public ZoomRepository(ZoomAuthService authService, ILogger<ZoomRepository> logger)
        {
            this._httpClient = new HttpClient();
            this._authService = authService ?? throw new ArgumentNullException(nameof(authService));
            this._httpClient.BaseAddress = new Uri("https://api.zoom.us/v2/");
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


     public async Task<ZoomMeetingResponseDto> CreateMeetingAsync(ZoomMeetingDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Topic))
                    throw new ArgumentException("Topic is required");

                if (dto.Duration <= 0)
                    throw new ArgumentException("Duration must be greater than 0");

                _logger.LogInformation("Creating Zoom meeting for topic: {Topic}", dto.Topic);

                var accessToken = await _authService.GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var startTime = dto.ClassDateTime.ToUniversalTime();
                if (startTime <= DateTime.UtcNow.AddMinutes(5))
                {
                    startTime = DateTime.UtcNow.AddMinutes(10);
                }

                // Determine if registration is required
                bool requireRegistration = dto.Invitees != null && dto.Invitees.Any();

                var body = new
                {
                    topic = dto.Topic,
                    type = 2,
                    start_time = startTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    duration = dto.Duration,
                    agenda = string.IsNullOrWhiteSpace(dto.Agenda) ? dto.Topic : dto.Agenda,
                    timezone = "UTC",
                    settings = new
                    {
                        host_video = true,
                        participant_video = true,
                        join_before_host = true,
                        mute_upon_entry = true,
                        waiting_room = !requireRegistration,
                        approval_type = requireRegistration ? dto.ApprovalType : 2,
                        registration_type = requireRegistration ? 1 : 3,
                        auto_recording = "cloud",
                        recording_authentication = false,
                        require_password_for_all_participants = true
                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("users/me/meetings", content);
                var json = await response.Content.ReadAsStringAsync();

                // ✅✅✅ CRITICAL: Log the RAW response from Zoom
                _logger.LogInformation("================================================");
                _logger.LogInformation("=== RAW ZOOM API RESPONSE ===");
                _logger.LogInformation("{Json}", json);
                _logger.LogInformation("=== END RAW RESPONSE ===");
                _logger.LogInformation("================================================");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Zoom API Error: {Status} - {Response}",
                        response.StatusCode, json);
                    throw new HttpRequestException(
                        $"Zoom API Error: {response.StatusCode} - {json}"
                    );
                }

                var options = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true
                };

                var zoomMeetingResponse = JsonSerializer.Deserialize<ZoomMeetingResponseDto>(json, options);

                if (zoomMeetingResponse == null)
                {
                    throw new InvalidOperationException("Failed to deserialize Zoom response");
                }

                // ✅✅✅ CRITICAL: Log what was deserialized
                _logger.LogInformation("================================================");
                _logger.LogInformation("=== DESERIALIZED VALUES ===");
                _logger.LogInformation("Id (DB): {Id}", zoomMeetingResponse.Id);
                _logger.LogInformation("MeetingId (Zoom): {MeetingId}", zoomMeetingResponse.MeetingId ?? 0);
                _logger.LogInformation("Topic: {Topic}", zoomMeetingResponse.Topic ?? "NULL");
                _logger.LogInformation("JoinUrl: {JoinUrl}", zoomMeetingResponse.JoinUrl ?? "❌ NULL - NOT MAPPED!");
                _logger.LogInformation("StartUrl: {StartUrl}", zoomMeetingResponse.StartUrl ?? "❌ NULL - NOT MAPPED!");
                _logger.LogInformation("Password: {Password}", zoomMeetingResponse.Password ?? "❌ NULL - NOT MAPPED!");
                _logger.LogInformation("RegistrationUrl: {RegistrationUrl}", zoomMeetingResponse.RegistrationUrl ?? "❌ NULL - NOT MAPPED!");
                _logger.LogInformation("=== END DESERIALIZED ===");
                _logger.LogInformation("================================================");

                // Add invitees if provided (will fail on FREE account)
                if (dto.Invitees != null && dto.Invitees.Any())
                {
                    var zoomMeetingId = zoomMeetingResponse.MeetingId?.ToString() 
                        ?? throw new InvalidOperationException("Meeting ID not returned from Zoom");

                    _logger.LogInformation("Adding {Count} invitees to meeting {MeetingId}", 
                        dto.Invitees.Count, zoomMeetingId);
                    
                    try
                    {
                        var registrantUrls = await AddInviteesAsync(zoomMeetingId, dto.Invitees);
                        
                        _logger.LogInformation("Successfully added {Count} invitees. URLs: {Urls}", 
                            registrantUrls.Count, 
                            string.Join(", ", registrantUrls));
                            
                        for (int i = 0; i < dto.Invitees.Count && i < registrantUrls.Count; i++)
                        {
                            _logger.LogInformation("Invitee: {Email} -> Join URL: {Url}", 
                                dto.Invitees[i].Email, 
                                registrantUrls[i]);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "FAILED to add invitees to meeting {MeetingId}", zoomMeetingId);
                    }
                }

                _logger.LogInformation(
                    "Zoom meeting created successfully: ID={MeetingId}, JoinUrl={JoinUrl}, RegistrationUrl={RegistrationUrl}",
                    zoomMeetingResponse.MeetingId,
                    zoomMeetingResponse.JoinUrl ?? "(null)",
                    zoomMeetingResponse.RegistrationUrl ?? "(null)"
                );

                return zoomMeetingResponse;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "Zoom API call failed");
                throw new Exception($"Failed to create Zoom meeting: {httpEx.Message}", httpEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during meeting creation");
                throw new Exception($"Failed to create meeting: {ex.Message}", ex);
            }
        }
       public async Task<List<string>> AddInviteesAsync(string meetingId, List<InviteeDto> invitees)
        {
            var joinUrls = new List<string>();
            
            try
            {
                var accessToken = await _authService.GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                _logger.LogInformation("Starting to add {Count} invitees to meeting {MeetingId}", 
                    invitees.Count, meetingId);

                foreach (var invitee in invitees)
                {
                    try
                    {
                        // Split name into first and last
                        var nameParts = invitee.Name?.Split(' ', 2) ?? new[] { invitee.Email, "" };
                        var firstName = nameParts.Length > 0 ? nameParts[0] : invitee.Email;
                        var lastName = nameParts.Length > 1 ? nameParts[1] : "";

                        var registrantData = new
                        {
                            email = invitee.Email,
                            first_name = firstName,
                            last_name = lastName
                        };

                        var jsonContent = JsonSerializer.Serialize(registrantData);
                        _logger.LogInformation("Sending registrant data: {Data}", jsonContent);

                        var content = new StringContent(
                            jsonContent,
                            Encoding.UTF8,
                            "application/json"
                        );

                        var response = await _httpClient.PostAsync(
                            $"meetings/{meetingId}/registrants",
                            content
                        );

                        var responseJson = await response.Content.ReadAsStringAsync();

                        _logger.LogInformation(
                            "Zoom API Response for {Email}: Status={Status}, Body={Body}",
                            invitee.Email,
                            response.StatusCode,
                            responseJson
                        );

                        if (response.IsSuccessStatusCode)
                        {
                            var options = new JsonSerializerOptions 
                            { 
                                PropertyNameCaseInsensitive = true 
                            };
                            
                            var registrantResponse = JsonSerializer.Deserialize<ZoomRegistrantResponse>(
                                responseJson, 
                                options
                            );

                            if (registrantResponse != null && !string.IsNullOrEmpty(registrantResponse.JoinUrl))
                            {
                                joinUrls.Add(registrantResponse.JoinUrl);
                                
                                _logger.LogInformation(
                                    "✅ SUCCESS: Added invitee {Email}, ID={Id}, JoinUrl={JoinUrl}",
                                    invitee.Email,
                                    registrantResponse.Id,
                                    registrantResponse.JoinUrl
                                );
                            }
                            else
                            {
                                _logger.LogWarning("Response missing join_url for {Email}", invitee.Email);
                            }
                        }
                        else
                        {
                            _logger.LogError(
                                "❌ FAILED to add invitee {Email}: {Status} - {Response}",
                                invitee.Email,
                                response.StatusCode,
                                responseJson
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Exception adding invitee {Email}", invitee.Email);
                        // Continue with next invitee
                    }
                }

                _logger.LogInformation("Completed adding invitees. Success: {Count}/{Total}", 
                    joinUrls.Count, invitees.Count);

                return joinUrls;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error in AddInviteesAsync for meeting {MeetingId}", meetingId);
                throw;
            }
        }
        public async Task<ZoomMeetingResponseDto> GetMeetingAsync(string meetingId)
        {
            var token = await _authService.GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"meetings/{meetingId}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Zoom API Error: {responseContent}");
            }

            return JsonSerializer.Deserialize<ZoomMeetingResponseDto>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            )!;
        }
    
            /// <summary>
        /// Toggle recording type for a meeting
        /// </summary>
        // public async Task<bool> ToggleRecordingAsync(string meetingId, string recordingType)
        // {
        //     try
        //     {
        //         // Validate recording type
        //         var validTypes = new[] { "cloud", "local", "none" };
        //         if (!validTypes.Contains(recordingType.ToLower()))
        //         {
        //             throw new ArgumentException(
        //                 $"Invalid recording type. Must be: {string.Join(", ", validTypes)}"
        //             );
        //         }

        //         _logger.LogInformation(
        //             "Toggling recording for meeting {MeetingId} to {RecordingType}",
        //             meetingId,
        //             recordingType
        //         );

        //         var accessToken = await _authService.GetAccessTokenAsync();
        //         _httpClient.DefaultRequestHeaders.Clear();
        //         _httpClient.DefaultRequestHeaders.Authorization =
        //             new AuthenticationHeaderValue("Bearer", accessToken);

        //         var updateBody = new
        //         {
        //             settings = new
        //             {
        //                 auto_recording = recordingType.ToLower()
        //             }
        //         };

        //         var content = new StringContent(
        //             JsonSerializer.Serialize(updateBody),
        //             Encoding.UTF8,
        //             "application/json"
        //         );

        //         var response = await _httpClient.PatchAsync($"meetings/{meetingId}", content);

        //         if (!response.IsSuccessStatusCode)
        //         {
        //             var error = await response.Content.ReadAsStringAsync();
        //             _logger.LogError(
        //                 "Failed to toggle recording: {Status} - {Error}",
        //                 response.StatusCode,
        //                 error
        //             );
        //             return false;
        //         }

        //         _logger.LogInformation(
        //             "✅ Recording toggled successfully: {MeetingId} -> {RecordingType}",
        //             meetingId,
        //             recordingType
        //         );

        //         return true;
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error toggling recording for meeting {MeetingId}", meetingId);
        //         throw;
        //     }
        // }

/// <summary>
/// Create a recurring meeting
/// </summary>
        public async Task<ZoomMeetingResponseDto> CreateRecurringMeetingAsync(RecurringMeetingDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Topic))
                    throw new ArgumentException("Topic is required");

                if (dto.Duration <= 0)
                    throw new ArgumentException("Duration must be greater than 0");

                _logger.LogInformation("Creating recurring meeting: {Topic}", dto.Topic);

                var accessToken = await _authService.GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                // Determine meeting type
                int meetingType;
                object bodyObj;

                if (dto.RecurrenceType == RecurrenceType.NoFixedTime)
                {
                    // Type 3: Recurring with no fixed time
                    meetingType = 3;
                    
                    bodyObj = new
                    {
                        topic = dto.Topic,
                        type = meetingType,
                        duration = dto.Duration,
                        timezone = dto.Timezone ?? "UTC",
                        agenda = dto.Agenda,
                        settings = new
                        {
                            host_video = true,
                            participant_video = true,
                            join_before_host = true,
                            mute_upon_entry = true,
                            waiting_room = true,
                            auto_recording = dto.AutoRecording ?? "cloud"
                        }
                    };
                }
                else
                {
                    // Type 8: Recurring with fixed time
                    meetingType = 8;
                    
                    var startTime = dto.StartTime?.ToUniversalTime() ?? DateTime.UtcNow.AddDays(1);
                    if (startTime <= DateTime.UtcNow.AddMinutes(5))
                    {
                        startTime = DateTime.UtcNow.AddMinutes(10);
                    }

                    var recurrenceBody = BuildRecurrenceObject(dto);

                    bodyObj = new
                    {
                        topic = dto.Topic,
                        type = meetingType,
                        start_time = startTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        duration = dto.Duration,
                        timezone = dto.Timezone ?? "UTC",
                        agenda = dto.Agenda,
                        recurrence = recurrenceBody,
                        settings = new
                        {
                            host_video = true,
                            participant_video = true,
                            join_before_host = true,
                            mute_upon_entry = true,
                            waiting_room = true,
                            auto_recording = dto.AutoRecording ?? "cloud"
                        }
                    };
                }

                var content = new StringContent(
                    JsonSerializer.Serialize(bodyObj, new JsonSerializerOptions 
                    { 
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull 
                    }),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("users/me/meetings", content);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Zoom API Error: {Status} - {Response}",
                        response.StatusCode, json);
                    throw new HttpRequestException($"Zoom API Error: {response.StatusCode} - {json}");
                }

                var zoomResponse = JsonSerializer.Deserialize<ZoomMeetingResponseDto>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                _logger.LogInformation(
                    "Recurring meeting created: ID={MeetingId}, Type={Type}",
                    zoomResponse.MeetingId,
                    meetingType
                );

                return zoomResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating recurring meeting");
                throw;
            }
        }

/// <summary>
/// Get all occurrences of a recurring meeting
/// </summary>
        public async Task<List<MeetingOccurrenceDto>> GetRecurringMeetingOccurrencesAsync(string meetingId)
        {
            try
            {
                var accessToken = await _authService.GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.GetAsync($"meetings/{meetingId}");
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to get meeting: {response.StatusCode}");
                }

                // Check if response has occurrences
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                
                if (!root.TryGetProperty("occurrences", out var occurrencesElement))
                {
                    _logger.LogInformation("Meeting {MeetingId} has no occurrences", meetingId);
                    return new List<MeetingOccurrenceDto>();
                }

                var occurrences = JsonSerializer.Deserialize<List<MeetingOccurrenceDto>>(
                    occurrencesElement.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                return occurrences ?? new List<MeetingOccurrenceDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting occurrences for meeting {MeetingId}", meetingId);
                throw;
            }
        }

/// <summary>
/// Delete a specific occurrence
/// </summary>
        public async Task DeleteMeetingOccurrenceAsync(string meetingId, string occurrenceId)
{
    try
    {
        var accessToken = await _authService.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.DeleteAsync(
            $"meetings/{meetingId}?occurrence_id={occurrenceId}"
        );

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Failed to delete occurrence: {error}");
        }

        _logger.LogInformation("Deleted occurrence {OccurrenceId}", occurrenceId);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error deleting occurrence");
        throw;
    }
}

// Helper method to build recurrence object
        private object BuildRecurrenceObject(RecurringMeetingDto dto)
        {
            var recurrence = new Dictionary<string, object>
            {
                ["type"] = (int)dto.RecurrencePattern
            };

            switch (dto.RecurrencePattern)
            {
                case RecurrencePattern.Daily:
                    recurrence["repeat_interval"] = dto.RepeatInterval;
                    if (dto.EndDate.HasValue)
                    {
                        recurrence["end_date_time"] = dto.EndDate.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");
                    }
                    if (dto.Occurrences.HasValue)
                    {
                        recurrence["end_times"] = dto.Occurrences.Value;
                    }
                    break;

                case RecurrencePattern.Weekly:
                    recurrence["repeat_interval"] = dto.RepeatInterval;
                    recurrence["weekly_days"] = dto.WeeklyDays; // e.g., "1,3,5"
                    if (dto.EndDate.HasValue)
                    {
                        recurrence["end_date_time"] = dto.EndDate.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");
                    }
                    if (dto.Occurrences.HasValue)
                    {
                        recurrence["end_times"] = dto.Occurrences.Value;
                    }
                    break;

                case RecurrencePattern.Monthly:
                    recurrence["repeat_interval"] = dto.RepeatInterval;
                    if (dto.MonthlyDay.HasValue)
                    {
                        recurrence["monthly_day"] = dto.MonthlyDay.Value;
                    }
                    else if (dto.MonthlyWeek.HasValue && dto.MonthlyWeekDay.HasValue)
                    {
                        recurrence["monthly_week"] = dto.MonthlyWeek.Value;
                        recurrence["monthly_week_day"] = dto.MonthlyWeekDay.Value;
                    }
                    if (dto.EndDate.HasValue)
                    {
                        recurrence["end_date_time"] = dto.EndDate.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");
                    }
                    if (dto.Occurrences.HasValue)
                    {
                        recurrence["end_times"] = dto.Occurrences.Value;
                    }
                    break;
            }

            return recurrence;
        }
   
         /// <summary>
        /// Enable or disable recording for a meeting
        /// </summary>
        public async Task<bool> ToggleRecordingAsync(string meetingId, bool enableRecording)
        {
            try
            {
                // Determine recording type based on boolean
                string recordingType = enableRecording ? "cloud" : "none";
                // Note: Use "local" instead of "cloud" for free accounts

                _logger.LogInformation(
                    "Setting recording for meeting {MeetingId} to {Enabled} (type: {RecordingType})",
                    meetingId,
                    enableRecording ? "ENABLED" : "DISABLED",
                    recordingType
                );

                var accessToken = await _authService.GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var updateBody = new
                {
                    settings = new
                    {
                        auto_recording = recordingType
                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(updateBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PatchAsync($"meetings/{meetingId}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Failed to toggle recording: {Status} - {Error}",
                        response.StatusCode,
                        error
                    );
                    return false;
                }

                _logger.LogInformation(
                    "✅ Recording {Status} for meeting {MeetingId}",
                    enableRecording ? "ENABLED" : "DISABLED",
                    meetingId
                );

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling recording for meeting {MeetingId}", meetingId);
                throw;
            }
        }
   
    }
}