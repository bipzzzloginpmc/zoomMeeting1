using Microsoft.EntityFrameworkCore;
using ZoomMeetingAPI.Models;

namespace ZoomMeetingAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ZoomMeeting> ZoomMeetings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ZoomMeeting>(entity =>
            {
                entity.ToTable("zoom_meetings");
                
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.MeetingId).HasColumnName("meeting_id");
                entity.Property(e => e.Topic).HasColumnName("topic").HasMaxLength(200);
                entity.Property(e => e.Type).HasColumnName("type");
                entity.Property(e => e.StartTime).HasColumnName("start_time");
                entity.Property(e => e.Duration).HasColumnName("duration");
                entity.Property(e => e.Timezone).HasColumnName("timezone").HasMaxLength(50);
                entity.Property(e => e.Agenda).HasColumnName("agenda").HasMaxLength(500);
                entity.Property(e => e.JoinUrl).HasColumnName("join_url").HasMaxLength(500);
                entity.Property(e => e.StartUrl).HasColumnName("start_url").HasMaxLength(500);
                entity.Property(e => e.Password).HasColumnName("password").HasMaxLength(100);
                entity.Property(e => e.HostEmail).HasColumnName("host_email").HasMaxLength(200);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
                entity.Property(e => e.HostVideo).HasColumnName("host_video").HasDefaultValue(true);
                entity.Property(e => e.ParticipantVideo).HasColumnName("participant_video").HasDefaultValue(true);
                entity.Property(e => e.JoinBeforeHost).HasColumnName("join_before_host").HasDefaultValue(false);
                entity.Property(e => e.MuteUponEntry).HasColumnName("mute_upon_entry").HasDefaultValue(true);
                entity.Property(e => e.WaitingRoom).HasColumnName("waiting_room").HasDefaultValue(true);
                entity.Property(e => e.ApprovalType).HasColumnName("approval_type").HasMaxLength(10);
                entity.Property(e => e.AutoRecording).HasColumnName("auto_recording").HasMaxLength(20);

                entity.HasIndex(e => e.MeetingId).IsUnique();
            });
        }
    }
}