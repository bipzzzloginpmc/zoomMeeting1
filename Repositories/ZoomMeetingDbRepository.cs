using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZoomMeetingAPI.Data;
using ZoomMeetingAPI.Models;
using ZoomMeetingAPI.Repositories.Interfaces;

namespace ZoomMeetingAPI.Repositories
{
    public class ZoomMeetingRepository : IZoomMeetingRepository
    {
        private readonly ApplicationDbContext _context;

        public ZoomMeetingRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ZoomMeeting> GetByIdAsync(int id)
        {
            return await _context.ZoomMeetings.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<ZoomMeeting> GetByMeetingIdAsync(long meetingId)
        {
            return await _context.ZoomMeetings.FirstOrDefaultAsync(m => m.MeetingId == meetingId);
        }

        public async Task<IEnumerable<ZoomMeeting>> GetAllAsync()
        {
            var result = await _context.ZoomMeetings
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
            return result;
        }

        public async Task<IEnumerable<ZoomMeeting>> FindAsync(Expression<Func<ZoomMeeting, bool>> predicate)
        {
            return await _context.ZoomMeetings
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<ZoomMeeting> AddAsync(ZoomMeeting meeting)
        {
            meeting.CreatedAt = DateTime.UtcNow;
            await _context.ZoomMeetings.AddAsync(meeting);
            await _context.SaveChangesAsync();
            return meeting;
        }

        public async Task<ZoomMeeting> UpdateAsync(ZoomMeeting meeting)
        {
            meeting.UpdatedAt = DateTime.UtcNow;
            _context.ZoomMeetings.Update(meeting);
            await _context.SaveChangesAsync();
            return meeting;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var meeting = await GetByIdAsync(id);
            if (meeting == null)
                return false;

            // Soft delete
            meeting.IsDeleted = true;
            meeting.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(long meetingId)
        {
            return await _context.ZoomMeetings
                .AnyAsync(m => m.MeetingId == meetingId);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}