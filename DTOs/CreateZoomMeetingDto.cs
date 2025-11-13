using System;
using System.ComponentModel.DataAnnotations;

namespace ZoomMeetingAPI.DTOs
{
    public class CreateZoomMeetingDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Topic { get; set; } = string.Empty;
        
        public int Type { get; set; } = 2; // Default: Scheduled meeting
        
        public DateTime? StartTime { get; set; }
        
        [Range(1, 1440)]
        public int Duration { get; set; } = 60; // Default: 60 minutes
        
        public string? Timezone { get; set; } = "UTC";
        
        public string? Agenda { get; set; }
        
        public string? Password { get; set; }
        
        [Required]
        [EmailAddress]
        public string HostEmail { get; set; } = string.Empty;
        
        public MeetingSettingsDto Settings { get; set; } = new();
    }
    
    public class MeetingSettingsDto
    {
        public bool HostVideo { get; set; } = true;
        public bool ParticipantVideo { get; set; } = true;
        public bool JoinBeforeHost { get; set; } = false;
        public bool MuteUponEntry { get; set; } = true;
        public bool WaitingRoom { get; set; } = true;
        public int ApprovalType { get; set; } = 2;
        public string AutoRecording { get; set; } = "none";
    }
}