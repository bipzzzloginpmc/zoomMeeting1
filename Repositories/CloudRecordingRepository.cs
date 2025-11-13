using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ZoomMeetingAPI.DTOs;
using ZoomMeetingAPI.Repositories.Interfaces;

namespace ZoomMeetingAPI.Repositories
{
    public class CloudRecordingRepository : ICloudRecordingRepository
    {
        private readonly HttpClient _httpClient;
        private readonly ZoomAuthService _authService;
        private readonly ILogger<CloudRecordingRepository> _logger;
        private const string BaseUrl = "https://api.zoom.us/v2";

        public CloudRecordingRepository(
            ZoomAuthService authService,
            ILogger<CloudRecordingRepository> logger)
        {
            _httpClient = new HttpClient();
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ZoomCloudRecordingResponse> GetMeetingRecordingsAsync(string meetingId)
        {
            try
            {
                _logger.LogInformation("Fetching recordings for meeting {MeetingId} from Zoom API", meetingId);

                var token = await _authService.GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{BaseUrl}/meetings/{meetingId}/recordings");
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        _logger.LogWarning("No recordings found for meeting {MeetingId}", meetingId);
                        return null;
                    }

                    _logger.LogError(
                        "Zoom API error getting recordings: {Status} - {Response}",
                        response.StatusCode,
                        json
                    );
                    throw new HttpRequestException($"Zoom API Error: {response.StatusCode} - {json}");
                }

                var result = JsonSerializer.Deserialize<ZoomCloudRecordingResponse>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                _logger.LogInformation(
                    "Retrieved {Count} recording files for meeting {MeetingId}",
                    result?.RecordingFiles?.Count ?? 0,
                    meetingId
                );

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recordings for meeting {MeetingId}", meetingId);
                throw;
            }
        }

        public async Task<ZoomRecordingsListResponse> GetAllRecordingsAsync(
            DateTime from,
            DateTime to,
            int pageSize = 300)
        {
            try
            {
                _logger.LogInformation(
                    "Fetching all recordings from {From} to {To}",
                    from,
                    to
                );

                var token = await _authService.GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var url = $"{BaseUrl}/users/me/recordings" +
                          $"?from={from:yyyy-MM-dd}" +
                          $"&to={to:yyyy-MM-dd}" +
                          $"&page_size={pageSize}";

                var response = await _httpClient.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Zoom API error getting all recordings: {Status} - {Response}",
                        response.StatusCode,
                        json
                    );
                    throw new HttpRequestException($"Zoom API Error: {response.StatusCode}");
                }

                var result = JsonSerializer.Deserialize<ZoomRecordingsListResponse>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                _logger.LogInformation(
                    "Retrieved {Count} total recordings",
                    result?.TotalRecords ?? 0
                );

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all recordings");
                throw;
            }
        }

        public async Task<byte[]> DownloadRecordingFileAsync(string downloadUrl)
        {
            try
            {
                _logger.LogInformation("Downloading recording from {Url}", downloadUrl);

                var token = await _authService.GetAccessTokenAsync();

                using (var downloadClient = new HttpClient())
                {
                    downloadClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    var response = await downloadClient.GetAsync(downloadUrl);

                    if (!response.IsSuccessStatusCode)
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        _logger.LogError(
                            "Failed to download recording: {Status} - {Error}",
                            response.StatusCode,
                            error
                        );
                        throw new HttpRequestException($"Download failed: {response.StatusCode}");
                    }

                    var fileBytes = await response.Content.ReadAsByteArrayAsync();

                    _logger.LogInformation(
                        "Downloaded {Size} MB from {Url}",
                        fileBytes.Length / 1024.0 / 1024.0,
                        downloadUrl
                    );

                    return fileBytes;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading recording from {Url}", downloadUrl);
                throw;
            }
        }

        public async Task DeleteMeetingRecordingsAsync(string meetingId)
        {
            try
            {
                _logger.LogInformation("Deleting all recordings for meeting {MeetingId}", meetingId);

                var token = await _authService.GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.DeleteAsync(
                    $"{BaseUrl}/meetings/{meetingId}/recordings"
                );

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Failed to delete recordings: {Status} - {Error}",
                        response.StatusCode,
                        error
                    );
                    throw new HttpRequestException($"Delete failed: {response.StatusCode} - {error}");
                }

                _logger.LogInformation("Successfully deleted recordings for meeting {MeetingId}", meetingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting recordings for meeting {MeetingId}", meetingId);
                throw;
            }
        }

        public async Task DeleteRecordingFileAsync(string meetingId, string recordingId)
        {
            try
            {
                _logger.LogInformation(
                    "Deleting recording file {RecordingId} for meeting {MeetingId}",
                    recordingId,
                    meetingId
                );

                var token = await _authService.GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.DeleteAsync(
                    $"{BaseUrl}/meetings/{meetingId}/recordings/{recordingId}"
                );

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Failed to delete recording file: {Status} - {Error}",
                        response.StatusCode,
                        error
                    );
                    throw new HttpRequestException($"Delete failed: {response.StatusCode} - {error}");
                }

                _logger.LogInformation(
                    "Successfully deleted recording file {RecordingId}",
                    recordingId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting recording file {RecordingId} for meeting {MeetingId}",
                    recordingId,
                    meetingId
                );
                throw;
            }
        }

        public async Task<object> GetRecordingSettingsAsync(string meetingId)
        {
            try
            {
                var token = await _authService.GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync(
                    $"{BaseUrl}/meetings/{meetingId}/recordings/settings"
                );

                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Failed to get recording settings: {Status} - {Response}",
                        response.StatusCode,
                        json
                    );
                    throw new HttpRequestException($"Failed to get settings: {response.StatusCode}");
                }

                return JsonSerializer.Deserialize<object>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recording settings for meeting {MeetingId}", meetingId);
                throw;
            }
        }
    }
}