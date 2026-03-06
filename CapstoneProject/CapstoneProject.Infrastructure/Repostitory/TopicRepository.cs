using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Infrastructure.Database.AppDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

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
                .Include(t => t.TopicVersions)
                .FirstOrDefaultAsync(t => t.GroupId == groupId);
        }

        public async Task AddTopicAsync(Topic topic)
        {
            await _context.Topics.AddAsync(topic);
        }

        public async Task AddVersionAsync(TopicVersion version)
        {
            await _context.TopicVersions.AddAsync(version);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
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
    }
}

