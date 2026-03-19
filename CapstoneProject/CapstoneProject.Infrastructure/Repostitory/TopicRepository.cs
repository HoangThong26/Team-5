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
            var allVersions = await _context.TopicVersions
                .AsNoTracking()
                .Include(v => v.Topic)
                    .ThenInclude(t => t.Group)
                .Where(v => _context.MentorAssignments
                    .Any(ma => ma.MentorId == mentorId && ma.GroupId == v.Topic.GroupId))
                .ToListAsync();

            return allVersions
                .GroupBy(v => v.TopicId)
                .Select(g => g.OrderByDescending(v => v.SubmittedAt).First())
                .OrderByDescending(v => v.SubmittedAt)
                .ToList();
        }

        public async Task<bool> IsGroupLeaderAsync(int groupId, int userId)
        {
            return await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.RoleInGroup == "Leader");
        }
    }
}