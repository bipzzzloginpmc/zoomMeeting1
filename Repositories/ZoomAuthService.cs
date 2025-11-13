using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using ZoomMeetingAPI.DTOs;

namespace ZoomMeetingAPI.Repositories
{
    public class ZoomAuthService
    {
         private readonly HttpClient _httpClient;
        private readonly string _accountId;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private string? _cachedToken;
        private DateTime _tokenExpiry;
        private readonly IConfiguration _configuration;


        public ZoomAuthService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _configuration = configuration;
            _accountId = configuration["Zoom:AccountId"]!;
            _clientId = configuration["Zoom:ClientId"]!;
            _clientSecret = configuration["Zoom:ClientSecret"]!;
        }

        public async Task<string> GetAccessTokenAsync()
        {

            var tokenUrl = $"https://zoom.us/oauth/token?grant_type=account_credentials&account_id={_accountId}";

            var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
            var authBytes = Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}");
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(authBytes)
            );

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("=== Zoom OAuth Error ===");
                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine($"Account ID: {_accountId}");
                Console.WriteLine($"Client ID: {_clientId}");
                Console.WriteLine($"Error Response: {errorContent}");

                throw new HttpRequestException(
                    $"Failed to get Zoom access token: {response.StatusCode} - {errorContent}"
                );
            }
            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<ZoomTokenResponse>(json);

            _cachedToken = tokenResponse!.AccessToken;
            _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 300);

            return _cachedToken;
        }
    }
}