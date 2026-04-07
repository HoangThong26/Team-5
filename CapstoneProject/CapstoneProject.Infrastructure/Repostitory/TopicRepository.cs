using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Infrastructure.Database.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Infrastructure.Repostitory
{
    public class TopicRepository : ITopicRepository
    {
        private readonly ApplicationDbContext _context;

        public TopicRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Topic?> GetByGroupIdAsync(int groupId)
        {
            return await _context.Topics
                .FirstOrDefaultAsync(t => t.GroupId == groupId);
        }

        public async Task<Topic?> GetByIdAsync(int topicId)
        {
            return await _context.Topics
                .FirstOrDefaultAsync(t => t.TopicId == topicId);
        }

        public async Task<TopicVersion?> GetLatestVersionAsync(int topicId)
        {
            return await _context.TopicVersions
                .Where(v => v.TopicId == topicId)
                .OrderByDescending(v => v.VersionNumber)
                .FirstOrDefaultAsync();
        }

        public async Task AddTopicAsync(Topic topic)
        {
            await _context.Topics.AddAsync(topic);
        }

        public async Task AddVersionAsync(TopicVersion version)
        {
            await _context.TopicVersions.AddAsync(version);
        }

        public async Task<bool> GroupExistsAsync(int groupId)
        {
            return await _context.Groups.AnyAsync(g => g.GroupId == groupId);
        }

        public async Task<bool> IsUserInGroupAsync(int groupId, int userId)
        {
            return await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> IsMentorOfGroupAsync(int groupId, int mentorId)
        {
            return await _context.MentorAssignments
                .AnyAsync(ma => ma.GroupId == groupId && ma.MentorId == mentorId);
        }

        public async Task<IEnumerable<TopicVersion>> GetPendingTopicVersionsByMentorAsync(int mentorId)
        {
            return await _context.TopicVersions
                .Include(tv => tv.Topic) 
                    .ThenInclude(t => t.Group) 
                .Where(tv => tv.Status == "Submitted" &&
                             _context.MentorAssignments.Any(ma => ma.MentorId == mentorId && ma.GroupId == tv.Topic.GroupId))
                .OrderByDescending(tv => tv.SubmittedAt)
                .ToListAsync();
        }
        public async Task<TopicVersion?> GetVersionByIdAsync(int id)
        {
            return await _context.TopicVersions.FindAsync(id);
        }
        public async Task<bool> HasMentorAssignedAsync(int groupId)
        {
            return await _context.MentorAssignments.AnyAsync(ma => ma.GroupId == groupId);
        }

        public async Task<IEnumerable<TopicVersion>> GetTopicVersionsByMentorAsync(int mentorId)
        {
            return await _context.TopicVersions
                .AsNoTracking()
                .Include(v => v.Topic)
                    .ThenInclude(t => t.Group)
                .Where(v => _context.MentorAssignments
                    .Any(ma => ma.MentorId == mentorId && ma.GroupId == v.Topic.GroupId))
                // Lọc lấy Version cao nhất của mỗi TopicId ngay tại SQL
                .Where(v => v.VersionNumber == _context.TopicVersions
                    .Where(v2 => v2.TopicId == v.TopicId)
                    .Max(v2 => v2.VersionNumber))
                .OrderByDescending(v => v.SubmittedAt)
                .ToListAsync();
        }

        public async Task<bool> IsGroupLeaderAsync(int groupId, int userId)
        {
            return await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.RoleInGroup == "Leader");
        }

        public async Task<int?> GetMentorIdByGroupIdAsync(int groupId)
        {
            return await _context.MentorAssignments
                .Where(ma => ma.GroupId == groupId)
                .Select(ma => ma.MentorId)
                .FirstOrDefaultAsync();
        }

        public async Task<int?> GetGroupIdByTopicIdAsync(int topicId)
        {
            return await _context.Topics
                .Where(t => t.TopicId == topicId)
                .Select(t => t.GroupId)
                .FirstOrDefaultAsync();
        }

        public async Task<string?> GetMentorEmailByGroupIdAsync(int groupId)
        {
            return await _context.MentorAssignments
                .Where(ma => ma.GroupId == groupId)
                .Include(ma => ma.Mentor)
                .Select(ma => ma.Mentor.Email)
                .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<TopicVersion>> GetMentorBoardVersionsAsync(int mentorId)
        {
            return await _context.TopicVersions
                .AsNoTracking()
                .Include(v => v.Topic)
                    .ThenInclude(t => t.Group)
                .Where(v => _context.MentorAssignments
                    .Any(ma => ma.MentorId == mentorId && ma.GroupId == v.Topic.GroupId))
                .Where(v => v.VersionNumber == _context.TopicVersions
                    .Where(v2 => v2.TopicId == v.TopicId)
                    .Max(v2 => v2.VersionNumber))
                .OrderByDescending(v => v.SubmittedAt)
                .ToListAsync();
        }
        public async Task<bool> IsTopicApprovedForGroupAsync(int groupId)
        {
            return await _context.Topics
                .AnyAsync(t => t.GroupId == groupId &&
                               _context.TopicVersions
                                   .Where(v => v.TopicId == t.TopicId)
                                   .OrderByDescending(v => v.VersionNumber)
                                   .Select(v => v.Status)
                                   .FirstOrDefault() == "Approved");
        }

        public async Task<List<Topic>> SearchTopicsAsync(string? keyword, string? status, int? supervisorId)
        {
            var query = _context.Topics
                .Include(t => t.Group)
                    .ThenInclude(g => g.MentorAssignment)
                        .ThenInclude(ma => ma.Mentor)
                .Include(t => t.TopicVersions)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(t => t.Title.Contains(keyword) || t.Description.Contains(keyword));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status == status);
            }

            if (supervisorId.HasValue)
            {
                query = query.Where(t => t.Group != null && t.Group.MentorAssignment != null && t.Group.MentorAssignment.MentorId == supervisorId);
            }

            return await query.ToListAsync();
        }
    }
}