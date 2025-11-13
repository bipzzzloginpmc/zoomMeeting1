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
    }
}