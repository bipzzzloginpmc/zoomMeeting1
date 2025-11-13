using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ZoomMeetingAPI.DTOs
{
    // Response DTO for cloud recording
    public class CloudRecordingDto
    {
        public string Uuid { get; set; }
        public long MeetingId { get; set; }
        public string Topic { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public long TotalSize { get; set; }
        public double TotalSizeMB { get; set; }
        public int RecordingCount { get; set; }
        public string ShareUrl { get; set; }
        public List<RecordingFileDto> RecordingFiles { get; set; }
    }

    // Individual recording file DTO
    public class RecordingFileDto
    {
        public string Id { get; set; }
        public string MeetingId { get; set; }
        public DateTime RecordingStart { get; set; }
        public DateTime RecordingEnd { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public double FileSizeMB { get; set; }
        public string PlayUrl { get; set; }
        public string DownloadUrl { get; set; }
        public string Status { get; set; }
        public string RecordingType { get; set; }
    }

    // Request DTO for getting recordings
    public class GetRecordingsRequestDto
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int PageSize { get; set; } = 300;
    }

    // Response DTO for list of recordings
    public class RecordingsListDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int TotalRecords { get; set; }
        public List<CloudRecordingDto> Recordings { get; set; }
    }

    // DTO for download request
    public class DownloadRecordingRequestDto
    {
        public string DownloadUrl { get; set; }
        public string FileName { get; set; }
    }

    // Internal Zoom API Response DTOs (for repository layer)
    public class ZoomCloudRecordingResponse
    {
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("account_id")]
        public string AccountId { get; set; }

        [JsonPropertyName("host_id")]
        public string HostId { get; set; }

        [JsonPropertyName("topic")]
        public string Topic { get; set; }

        [JsonPropertyName("start_time")]
        public DateTime StartTime { get; set; }

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("total_size")]
        public long TotalSize { get; set; }

        [JsonPropertyName("recording_count")]
        public int RecordingCount { get; set; }

        [JsonPropertyName("share_url")]
        public string ShareUrl { get; set; }

        [JsonPropertyName("recording_files")]
        public List<ZoomRecordingFile> RecordingFiles { get; set; }
    }

    public class ZoomRecordingFile
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("meeting_id")]
        public string MeetingId { get; set; }

        [JsonPropertyName("recording_start")]
        public DateTime RecordingStart { get; set; }

        [JsonPropertyName("recording_end")]
        public DateTime RecordingEnd { get; set; }

        [JsonPropertyName("file_type")]
        public string FileType { get; set; }

        [JsonPropertyName("file_size")]
        public long FileSize { get; set; }

        [JsonPropertyName("play_url")]
        public string PlayUrl { get; set; }

        [JsonPropertyName("download_url")]
        public string DownloadUrl { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("recording_type")]
        public string RecordingType { get; set; }
    }

    public class ZoomRecordingsListResponse
    {
        [JsonPropertyName("from")]
        public DateTime From { get; set; }

        [JsonPropertyName("to")]
        public DateTime To { get; set; }

        [JsonPropertyName("page_count")]
        public int PageCount { get; set; }

        [JsonPropertyName("page_size")]
        public int PageSize { get; set; }

        [JsonPropertyName("total_records")]
        public int TotalRecords { get; set; }

        [JsonPropertyName("meetings")]
        public List<ZoomCloudRecordingResponse> Meetings { get; set; }
    }

        /// <summary>
        /// DTO for enabling/disabling recording
        /// </summary>
        public class ToggleRecordingDto
        {
            [Required]
            public bool EnableRecording { get; set; }
            // public string RecordingType { get; set; } 
        }

        public class RecordingToggleResult
            {
                public bool Success { get; set; }
                public string ActualRecordingType { get; set; }  // What Zoom actually has set
                public string Message { get; set; }
            }
}