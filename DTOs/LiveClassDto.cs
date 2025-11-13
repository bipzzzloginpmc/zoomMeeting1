using System;

namespace ZoomMeetingAPI.DTOs
{
    public class LiveClassDto
    {
        public string? Category { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime ClassDateTime { get; set; }
        public int? MainSubjectID { get; set; }
        public int? ProgramID { get; set; }
        public int? BatchID { get; set; }
        public int? SemesterID { get; set; }
        public int? SectionID { get; set; }
        public int? ShiftID { get; set; }
        public int? LMSID { get; set; }
        public int Duration { get; set; }
        public long MeetingID { get; set; }
        public string? Join_Url { get; set; }
        public string? Start_Url { get; set; }
        public string? Agenda { get; set; }
        public string? TimeZone { get; set; }
    }

    public class GetLiveClassDto
    {
        public int LiveClassID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Category { get; set; }
        public DateTime ClassDateTime { get; set; }
        public MainSub? MainSubject { get; set; }
        public Program? Program { get; set; }
        public Batch? Batch { get; set; }
        public Semester? Semester { get; set; }
        public Section? Section { get; set; }
        public Shift? Shift { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class MainSub
    {
        public int MainSubjectID { get; set; }
        public string? subject { get; set; }
        public string? code { get; set; }
    }

    public class Program
    {
        public int programID { get; set; }
        public string? program { get; set; }
        public string? programCode { get; set; }
    }

    public class Batch
    {
        public int batchID { get; set; }
        public string? batchTitle { get; set; }
    }

    public class Semester
    {
        public int semesterID { get; set; }
        public string? semester { get; set; }
    }

    public class Section
    {
        public int sectionID { get; set; }
        public string? section { get; set; }
    }

    public class Shift
    {
        public int ShiftID { get; set; }
        public string? shiftName { get; set; }
    }
}
