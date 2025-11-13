using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ZoomMeetingAPI.DTOs
{
    /// <summary>
    /// Recurrence type for meetings
    /// </summary>
    public enum RecurrenceType
    {
        NoFixedTime = 0,  // Type 3 meeting - same ID, use anytime
        FixedTime = 1     // Type 8 meeting - scheduled recurring
    }

    /// <summary>
    /// Recurrence pattern
    /// </summary>
    public enum RecurrencePattern
    {
        Daily = 1,
        Weekly = 2,
        Monthly = 3
    }

    /// <summary>
    /// DTO for creating a recurring meeting
    /// </summary>
    public class RecurringMeetingDto
    {
        [Required]
        [StringLength(200)]
        public string Topic { get; set; }

        public string? Agenda { get; set; }

        [Required]
        [Range(1, 1440)]
        public int Duration { get; set; }

        /// <summary>
        /// Type: NoFixedTime (Type 3) or FixedTime (Type 8)
        /// </summary>
        public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.FixedTime;

        /// <summary>
        /// Pattern: Daily, Weekly, or Monthly
        /// </summary>
        [Required]
        public RecurrencePattern RecurrencePattern { get; set; }

        /// <summary>
        /// Start time (required for FixedTime type)
        /// </summary>
        public DateTime? StartTime { get; set; }

        public string? Timezone { get; set; } = "UTC";

        /// <summary>
        /// Repeat interval (e.g., every 1 day, every 2 weeks)
        /// </summary>
        [Range(1, 90)]
        public int RepeatInterval { get; set; } = 1;

        /// <summary>
        /// For weekly: Days of week to repeat (e.g., "1,3,5" for Sun, Tue, Thu)
        /// 1=Sunday, 2=Monday, 3=Tuesday, 4=Wednesday, 5=Thursday, 6=Friday, 7=Saturday
        /// </summary>
        public string? WeeklyDays { get; set; }

        /// <summary>
        /// For monthly: Day of month (1-31) or -1 for last day
        /// </summary>
        [Range(-1, 31)]
        public int? MonthlyDay { get; set; }

        /// <summary>
        /// For monthly: Week of month (-1=Last, 1=First, 2=Second, 3=Third, 4=Fourth)
        /// </summary>
        [Range(-1, 4)]
        public int? MonthlyWeek { get; set; }

        /// <summary>
        /// For monthly: Day of week (1=Sunday through 7=Saturday)
        /// </summary>
        [Range(1, 7)]
        public int? MonthlyWeekDay { get; set; }

        /// <summary>
        /// End date for the recurrence
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Number of occurrences (alternative to EndDate)
        /// </summary>
        [Range(1, 60)]
        public int? Occurrences { get; set; }

        /// <summary>
        /// Recording type: "cloud", "local", or "none"
        /// </summary>
        public string? AutoRecording { get; set; } = "cloud";

        // Additional fields from your existing system
        public int? MainSubjectID { get; set; }
        public int? ProgramID { get; set; }
        public int? BatchID { get; set; }
        public int? SemesterID { get; set; }
        public int? SectionID { get; set; }
        public int? ShiftID { get; set; }
        public int? LMSID { get; set; }
        public string? Category { get; set; }
    }

    /// <summary>
    /// DTO for meeting occurrence (from Zoom API)
    /// </summary>
    public class MeetingOccurrenceDto
    {
        [JsonPropertyName("occurrence_id")]
        public string OccurrenceId { get; set; }

        [JsonPropertyName("start_time")]
        public DateTime StartTime { get; set; }

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } // "available", "deleted"
    }

}