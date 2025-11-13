using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZoomMeetingAPI.Models
{
    public class ZoomMeeting
    {
        [Key]
        public int Id { get; set; }
        
        public long MeetingId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Topic { get; set; } = string.Empty;
        
        public int Type { get; set; }
        
        public DateTime? StartTime { get; set; }
        
        public int Duration { get; set; }
        
        [StringLength(50)]
        public string? Timezone { get; set; }
        
        [StringLength(500)]
        public string? Agenda { get; set; }
        
        [StringLength(500)]
        public string? JoinUrl { get; set; }
        
        [StringLength(500)]
        public string? StartUrl { get; set; }
        
        [StringLength(100)]
        public string? Password { get; set; }
        
        [StringLength(200)]
        public string HostEmail { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsDeleted { get; set; }
        
        public bool HostVideo { get; set; }
        
        public bool ParticipantVideo { get; set; }
        
        public bool JoinBeforeHost { get; set; }
        
        public bool MuteUponEntry { get; set; }
        
        public bool WaitingRoom { get; set; }
        
        [StringLength(10)]
        public string? ApprovalType { get; set; }
        
        [StringLength(20)]
        public string? AutoRecording { get; set; }

        // Add to existing ZoomMeeting class
        public bool IsRecurring { get; set; }
        public string? RecurrencePattern { get; set; } // "Daily", "Weekly", "Monthly"
    }
}