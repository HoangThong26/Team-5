using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Infrastructure.Database.AppDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Infrastructure.Repostitory
{
    public class GroupMemberRepository : IGroupMemberRepository
    {
        private readonly ApplicationDbContext _context;
        public GroupMemberRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<string>> GetMemberEmailsByGroupIdAsync(int groupId)
        {
            return await _context.GroupMembers
                .Where(gm => gm.GroupId == groupId)
                .Select(gm => gm.User.Email)
                .ToListAsync();
        }

    }
}
