using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Infrastructure.Database.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Infrastructure.Repostitory
{
    public class CouncilRepository : ICouncilRepository
    {
        private readonly ApplicationDbContext _context;
        public CouncilRepository(ApplicationDbContext context) => _context = context;

        public async Task<int> CreateCouncilWithMembersAsync(string councilName, List<int> memberIds)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // A. Tạo Hội đồng
                var council = new Council { Name = councilName };
                _context.Councils.Add(council);
                await _context.SaveChangesAsync(); // Lấy được CouncilId tự tăng

                // B. Gán thành viên
                if (memberIds != null && memberIds.Any())
                {
                    var members = memberIds.Select(id => new CouncilMember
                    {
                        CouncilId = council.CouncilId,
                        UserId = id // Đảm bảo kiểu dữ liệu khớp với DB
                    }).ToList();

                    await _context.CouncilMembers.AddRangeAsync(members);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return council.CouncilId;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; // Quăng lỗi để Service bắt và đưa vào ServiceResponse
            }
        }

        // Sửa lại hàm check trùng để an toàn hơn
        public async Task<List<string>> GetStaffAlreadyInCouncilAsync(List<int> userIds)
        {
            if (userIds == null || !userIds.Any()) return new List<string>();

            return await _context.CouncilMembers
                .Include(cm => cm.User)
                .Where(cm => cm.UserId.HasValue && userIds.Contains(cm.UserId.Value))
                .Select(cm => cm.User.FullName)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<User>> GetAvailableStaffsAsync()
        {
            return await _context.Users
                .Where(u => (u.Role == "Council"))
                .ToListAsync();
        }

        public async Task<Council> CreateCouncilAsync(Council council)
        {
            _context.Councils.Add(council);
            await _context.SaveChangesAsync();
            return council;
        }

        public async Task AddMembersAsync(List<CouncilMember> members)
        {
            await _context.CouncilMembers.AddRangeAsync(members);
            await _context.SaveChangesAsync();
        }

        public async Task<List<User>> SearchAvailableStaffAsync(string searchTerm)
        {
            var query = _context.Users
                .Where(u => (u.Role == "Mentor" || u.Role == "Council") && u.Status == "Active")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {

                query = query.Where(u => u.FullName.Contains(searchTerm) || u.Email.Contains(searchTerm));
            }

            return await query.Take(20).ToListAsync();
        }

    }
}
