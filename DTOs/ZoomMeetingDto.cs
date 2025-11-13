using System;
using System.ComponentModel.DataAnnotations;

namespace ZoomMeetingAPI.DTOs
{
    public class ZoomMeetingDto
    {
        public string Topic { get; set; } = string.Empty;
        public DateTime ClassDateTime { get; set; }
        public int Duration { get; set; }
        public string? Agenda { get; set; }
        public int? MainSubjectID { get; set; }
        public int? ProgramID { get; set; }
        public int? BatchID { get; set; }
        public int? SemesterID { get; set; }
        public int? SectionID { get; set; }
        public int? ShiftID { get; set; }
        public int? LMSID { get; set; }
        public string? Category { get; set; }
        // Add at the bottom of your existing ZoomMeetingDto class
        public List<InviteeDto>? Invitees { get; set; }
        public int ApprovalType { get; set; } = 0; // 0=auto approve
    }
    public class InviteeDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string? Name { get; set; }
    }

}
