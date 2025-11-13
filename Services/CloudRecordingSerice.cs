using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ZoomMeetingAPI.DTOs;
using ZoomMeetingAPI.Repositories.Interfaces;
using ZoomMeetingAPI.Services.Interfaces;

namespace ZoomMeetingAPI.Services
{
    public class CloudRecordingService : ICloudRecordingService
    {
        private readonly ICloudRecordingRepository _recordingRepository;
        private readonly ILogger<CloudRecordingService> _logger;

        public CloudRecordingService(
            ICloudRecordingRepository recordingRepository,
            ILogger<CloudRecordingService> logger)
        {
            _recordingRepository = recordingRepository ?? throw new ArgumentNullException(nameof(recordingRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CloudRecordingDto> GetMeetingRecordingsAsync(string meetingId)
        {
            try
            {
                _logger.LogInformation("Getting recordings for meeting {MeetingId}", meetingId);

                var zoomResponse = await _recordingRepository.GetMeetingRecordingsAsync(meetingId);

                if (zoomResponse == null)
                {
                    _logger.LogWarning("No recordings found for meeting {MeetingId}", meetingId);
                    return null;
                }

                // Map Zoom response to DTO
                var dto = MapToCloudRecordingDto(zoomResponse);

                _logger.LogInformation(
                    "Successfully retrieved {Count} recording files for meeting {MeetingId}",
                    dto.RecordingCount,
                    meetingId
                );

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in service getting recordings for meeting {MeetingId}", meetingId);
                throw new Exception($"Failed to get recordings: {ex.Message}", ex);
            }
        }

        public async Task<RecordingsListDto> GetAllRecordingsAsync(GetRecordingsRequestDto request)
        {
            try
            {
                var fromDate = request.From ?? DateTime.UtcNow.AddDays(-30);
                var toDate = request.To ?? DateTime.UtcNow;

                _logger.LogInformation("Getting all recordings from {From} to {To}", fromDate, toDate);

                var zoomResponse = await _recordingRepository.GetAllRecordingsAsync(
                    fromDate,
                    toDate,
                    request.PageSize
                );

                if (zoomResponse == null || zoomResponse.Meetings == null)
                {
                    return new RecordingsListDto
                    {
                        From = fromDate,
                        To = toDate,
                        TotalRecords = 0,
                        Recordings = new List<CloudRecordingDto>()
                    };
                }

                // Map to DTOs
                var recordings = zoomResponse.Meetings
                    .Select(MapToCloudRecordingDto)
                    .ToList();

                var result = new RecordingsListDto
                {
                    From = fromDate,
                    To = toDate,
                    TotalRecords = zoomResponse.TotalRecords,
                    Recordings = recordings
                };

                _logger.LogInformation("Retrieved {Count} recordings", result.TotalRecords);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in service getting all recordings");
                throw new Exception($"Failed to get all recordings: {ex.Message}", ex);
            }
        }

        public async Task<(byte[] fileBytes, string contentType, string fileName)> DownloadRecordingAsync(
            DownloadRecordingRequestDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.DownloadUrl))
                {
                    throw new ArgumentException("Download URL is required");
                }

                _logger.LogInformation("Downloading recording from {Url}", request.DownloadUrl);

                var fileBytes = await _recordingRepository.DownloadRecordingFileAsync(request.DownloadUrl);

                // Determine content type based on file name
                var fileName = request.FileName ?? "recording.mp4";
                var contentType = DetermineContentType(fileName);

                _logger.LogInformation(
                    "Downloaded {Size} MB, FileName: {FileName}",
                    fileBytes.Length / 1024.0 / 1024.0,
                    fileName
                );

                return (fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading recording");
                throw new Exception($"Failed to download recording: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteMeetingRecordingsAsync(string meetingId)
        {
            try
            {
                _logger.LogInformation("Deleting recordings for meeting {MeetingId}", meetingId);

                await _recordingRepository.DeleteMeetingRecordingsAsync(meetingId);

                _logger.LogInformation("Successfully deleted recordings for meeting {MeetingId}", meetingId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting recordings for meeting {MeetingId}", meetingId);
                throw new Exception($"Failed to delete recordings: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteRecordingFileAsync(string meetingId, string recordingId)
        {
            try
            {
                _logger.LogInformation(
                    "Deleting recording file {RecordingId} for meeting {MeetingId}",
                    recordingId,
                    meetingId
                );

                await _recordingRepository.DeleteRecordingFileAsync(meetingId, recordingId);

                _logger.LogInformation("Successfully deleted recording file {RecordingId}", recordingId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting recording file {RecordingId} for meeting {MeetingId}",
                    recordingId,
                    meetingId
                );
                throw new Exception($"Failed to delete recording file: {ex.Message}", ex);
            }
        }

        public async Task<bool> IsRecordingReadyAsync(string meetingId)
        {
            try
            {
                var recording = await _recordingRepository.GetMeetingRecordingsAsync(meetingId);

                if (recording == null || recording.RecordingFiles == null)
                {
                    return false;
                }

                // Check if all files are completed
                var allCompleted = recording.RecordingFiles.All(f => f.Status == "completed");

                _logger.LogInformation(
                    "Recording ready status for meeting {MeetingId}: {IsReady}",
                    meetingId,
                    allCompleted
                );

                return allCompleted;
            }
            catch
            {
                return false;
            }
        }

        public async Task<object> GetRecordingStatsAsync(string meetingId)
        {
            try
            {
                var recording = await _recordingRepository.GetMeetingRecordingsAsync(meetingId);

                if (recording == null)
                {
                    return null;
                }

                var stats = new
                {
                    MeetingId = recording.Id,
                    Topic = recording.Topic,
                    TotalFiles = recording.RecordingCount,
                    TotalSizeMB = Math.Round(recording.TotalSize / 1024.0 / 1024.0, 2),
                    FileTypes = recording.RecordingFiles
                        .GroupBy(f => f.FileType)
                        .Select(g => new
                        {
                            FileType = g.Key,
                            Count = g.Count(),
                            TotalSizeMB = Math.Round(g.Sum(f => f.FileSize) / 1024.0 / 1024.0, 2)
                        })
                        .ToList(),
                    Status = recording.RecordingFiles.All(f => f.Status == "completed") ? "Completed" : "Processing"
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recording stats for meeting {MeetingId}", meetingId);
                throw;
            }
        }

        // Helper method to map Zoom response to DTO
        private CloudRecordingDto MapToCloudRecordingDto(ZoomCloudRecordingResponse zoomResponse)
        {
            return new CloudRecordingDto
            {
                Uuid = zoomResponse.Uuid,
                MeetingId = zoomResponse.Id,
                Topic = zoomResponse.Topic,
                StartTime = zoomResponse.StartTime,
                Duration = zoomResponse.Duration,
                TotalSize = zoomResponse.TotalSize,
                TotalSizeMB = Math.Round(zoomResponse.TotalSize / 1024.0 / 1024.0, 2),
                RecordingCount = zoomResponse.RecordingCount,
                ShareUrl = zoomResponse.ShareUrl,
                RecordingFiles = zoomResponse.RecordingFiles?.Select(f => new RecordingFileDto
                {
                    Id = f.Id,
                    MeetingId = f.MeetingId,
                    RecordingStart = f.RecordingStart,
                    RecordingEnd = f.RecordingEnd,
                    FileType = f.FileType,
                    FileSize = f.FileSize,
                    FileSizeMB = Math.Round(f.FileSize / 1024.0 / 1024.0, 2),
                    PlayUrl = f.PlayUrl,
                    DownloadUrl = f.DownloadUrl,
                    Status = f.Status,
                    RecordingType = f.RecordingType
                }).ToList() ?? new List<RecordingFileDto>()
            };
        }

        private string DetermineContentType(string fileName)
        {
            var extension = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".mp4" => "video/mp4",
                ".m4a" => "audio/mp4",
                ".txt" => "text/plain",
                ".vtt" => "text/vtt",
                ".json" => "application/json",
                _ => "application/octet-stream"
            };
        }
    }
}