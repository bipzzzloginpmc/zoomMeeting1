using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZoomMeetingAPI.Models;

namespace ZoomMeetingAPI.Repositories.Interfaces
{
    public interface IZoomMeetingRepository
    {
        Task<ZoomMeeting> GetByIdAsync(int id);
        Task<ZoomMeeting> GetByMeetingIdAsync(long meetingId);
        Task<IEnumerable<ZoomMeeting>> GetAllAsync();
        Task<IEnumerable<ZoomMeeting>> FindAsync(Expression<Func<ZoomMeeting, bool>> predicate);
        Task<ZoomMeeting> AddAsync(ZoomMeeting meeting);
        Task<ZoomMeeting> UpdateAsync(ZoomMeeting meeting);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(long meetingId);
        Task<int> SaveChangesAsync();
    }
}