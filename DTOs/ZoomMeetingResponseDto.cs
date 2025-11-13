using System;
using System.Text.Json.Serialization;

namespace ZoomMeetingAPI.DTOs
{
    public class ZoomMeetingResponseDto
    {
        // ✅ FIX: Ignore this during JSON deserialization (database-only field)
        [JsonIgnore]
        public long Id { get; set; }

        // ✅ This maps to Zoom's "id" field
        [JsonPropertyName("id")]
        public long? MeetingId { get; set; }

        [JsonPropertyName("uuid")]
        public string? Uuid { get; set; }

        [JsonPropertyName("host_id")]
        public string? HostId { get; set; }

        [JsonPropertyName("host_email")]
        public string? HostEmail { get; set; } = string.Empty;

        [JsonPropertyName("topic")]
        public string? Topic { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public int? Type { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("start_time")]
        public DateTime? StartTime { get; set; }

        [JsonPropertyName("duration")]
        public int? Duration { get; set; }

        [JsonPropertyName("timezone")]
        public string? Timezone { get; set; }

        [JsonPropertyName("agenda")]
        public string? Agenda { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        // ✅ Critical mappings
        [JsonPropertyName("join_url")]
        public string? JoinUrl { get; set; }

        [JsonPropertyName("start_url")]
        public string? StartUrl { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }

        [JsonPropertyName("encrypted_password")]
        public string? EncryptedPassword { get; set; }

        [JsonPropertyName("h323_password")]
        public string? H323Password { get; set; }

        [JsonPropertyName("pstn_password")]
        public string? PstnPassword { get; set; }

        [JsonPropertyName("registration_url")]
        public string? RegistrationUrl { get; set; }

        [JsonPropertyName("settings")]
        public ZoomMeetingSettings? Settings { get; set; }

        // Database-only fields (ignored during JSON serialization/deserialization)
        [JsonIgnore]
        public DateTime? UpdatedAt { get; set; }
        
        [JsonIgnore]
        public bool IsDeleted { get; set; }

        // Backward compatibility fields (can be populated from Settings)
        [JsonIgnore]
        public bool HostVideo { get; set; }
        
        [JsonIgnore]
        public bool ParticipantVideo { get; set; }
        
        [JsonIgnore]
        public bool JoinBeforeHost { get; set; }
        
        [JsonIgnore]
        public bool MuteUponEntry { get; set; }
        
        [JsonIgnore]
        public bool WaitingRoom { get; set; }
        
        [JsonIgnore]
        public string? ApprovalType { get; set; }
        
        [JsonIgnore]
        public string? AutoRecording { get; set; }
    }

    public class ZoomMeetingSettings
    {
        [JsonPropertyName("host_video")]
        public bool HostVideo { get; set; }

        [JsonPropertyName("participant_video")]
        public bool ParticipantVideo { get; set; }

        [JsonPropertyName("join_before_host")]
        public bool JoinBeforeHost { get; set; }

        [JsonPropertyName("mute_upon_entry")]
        public bool MuteUponEntry { get; set; }

        [JsonPropertyName("waiting_room")]
        public bool WaitingRoom { get; set; }

        [JsonPropertyName("approval_type")]
        public int ApprovalType { get; set; }

        [JsonPropertyName("registration_type")]
        public int? RegistrationType { get; set; }

        [JsonPropertyName("auto_recording")]
        public string? AutoRecording { get; set; }

        [JsonPropertyName("meeting_authentication")]
        public bool MeetingAuthentication { get; set; }

        [JsonPropertyName("encryption_type")]
        public string? EncryptionType { get; set; }
    }

    public class ZoomRegistrantResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("registrant_id")]
        public string RegistrantId { get; set; }

        [JsonPropertyName("join_url")]
        public string JoinUrl { get; set; }

        [JsonPropertyName("topic")]
        public string Topic { get; set; }
    }
}