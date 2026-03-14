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
            // Matches the UNIQUE GroupId constraint in your SQL schema
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
            // Retrieves the most recent version based on VersionNumber as defined in your TopicVersions table
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
            // Validates membership against the GroupMembers table
            return await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}