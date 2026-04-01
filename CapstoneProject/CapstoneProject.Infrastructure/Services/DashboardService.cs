using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IService;
using CapstoneProject.Infrastructure.Database.AppDbContext;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneProject.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatisticsResponse> GetAdminDashboardStatsAsync()
        {
            var totalStudentsTask = _context.Users.CountAsync(u => u.Role == "Student" && u.Status == "Active");
            var totalMentorsTask = _context.Users.CountAsync(u => u.Role == "Mentor" && u.Status == "Active");
            var totalGroupsTask = _context.Groups.CountAsync();
            var totalTopicsTask = _context.Topics.CountAsync();

            var studentsWithGroupTask = _context.GroupMembers
                .Include(gm => gm.User)
                .Where(gm => gm.User.Role == "Student" && gm.User.Status == "Active")
                .Select(gm => gm.UserId)
                .Distinct()
                .CountAsync();

            var topicStatusTask = _context.Topics
                .GroupBy(t => t.Status)
                .Select(g => new StatusCountResponse { Status = g.Key ?? "Unknown", Count = g.Count() })
                .ToListAsync();

            var groupStatusTask = _context.Groups
                .GroupBy(g => g.Status)
                .Select(g => new StatusCountResponse { Status = g.Key ?? "Unknown", Count = g.Count() })
                .ToListAsync();

            await Task.WhenAll(
                totalStudentsTask, totalMentorsTask, totalGroupsTask, totalTopicsTask,
                studentsWithGroupTask, topicStatusTask, groupStatusTask
            );

            var totalStudents = await totalStudentsTask;
            var studentsWithGroup = await studentsWithGroupTask;

            return new DashboardStatisticsResponse
            {
                KpiCards = new KpiCardsResponse
                {
                    TotalStudents = totalStudents,
                    TotalMentors = await totalMentorsTask,
                    TotalGroups = await totalGroupsTask,
                    TotalTopics = await totalTopicsTask
                },
                Overview = new OverviewStatsResponse
                {
                    StudentsWithGroup = studentsWithGroup,
                    StudentsWithoutGroup = totalStudents - studentsWithGroup,
                    TopicStatus = await topicStatusTask,
                    GroupStatus = await groupStatusTask
                }
            };
        }
    }
}