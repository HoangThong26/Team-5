using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Infrastructure.Database.AppDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Infrastructure.Repostitory
{
    public class MentorAssignmentRepository : IMentorAssignmentRepository
    {
        private readonly ApplicationDbContext _context;
        public MentorAssignmentRepository( ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string?> GetMentorEmailByGroupIdAsync(int groupId)
        {
            return await _context.MentorAssignments
                .Where(ma => ma.GroupId == groupId)
                .Select(ma => ma.Mentor.Email)
                .FirstOrDefaultAsync();
        }

    }
}
