using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface;
using CapstoneProject.Infrastructure.Database; // Thay bằng namespace trỏ tới file DbContext của bạn nếu khác
using CapstoneProject.Infrastructure.Database.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Infrastructure.Services
{
    public class TopicSearchService : ITopicSearchService
    {
        private readonly ApplicationDbContext _context; // Đổi 'CapstoneDbContext' thành tên DbContext thật của team bạn

        public TopicSearchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<object> SearchGlobalAsync(TopicSearchRequest request)
        {
            // 1. Khởi tạo Query kết nối các bảng: Topics -> Groups -> Users (để lấy Leader)
            var query = _context.Topics
                .Include(t => t.Group)
                    .ThenInclude(g => g.Leader)
                .AsNoTracking()
                .AsQueryable();

            // 2. Xử lý logic TÌM KIẾM QUÉT QUA NHIỀU CỘT
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim().ToLower();
                bool isNumeric = int.TryParse(keyword, out int topicIdSearch);

                query = query.Where(t =>
                    // Tìm theo Mã đề tài (chỉ chạy nếu keyword là số)
                    (isNumeric && t.TopicId == topicIdSearch) ||

                    // Tìm theo Tên đề tài (Tiếng Việt/Anh chung 1 cột Title)
                    (t.Title != null && t.Title.ToLower().Contains(keyword)) ||

                    // Tìm theo Tên nhóm trưởng
                    (t.Group != null && t.Group.Leader != null && t.Group.Leader.FullName.ToLower().Contains(keyword))
                );
            }

            // 3. Đếm tổng số lượng để FE làm phân trang
            int totalRecords = await query.CountAsync();

            // 4. Sắp xếp, cắt trang và Map ra DTO
            var topics = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(t => new TopicSearchResponse
                {
                    TopicId = t.TopicId,
                    Title = t.Title ?? "Chưa có tên",
                    Status = t.Status ?? "Draft",
                    GroupId = t.GroupId ?? 0,
                    LeaderName = (t.Group != null && t.Group.Leader != null) ? t.Group.Leader.FullName : "Chưa có nhóm trưởng",
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            // 5. Trả về kết quả
            return new
            {
                Data = topics,
                TotalRecords = totalRecords,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize)
            };
        }
    }
}