using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface;
using CapstoneProject.Infrastructure.Database.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Infrastructure.Services
{
    public class TopicSearchService : ITopicSearchService
    {
        private readonly ApplicationDbContext _context;

        public TopicSearchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<object> SearchGlobalAsync(TopicSearchRequest request)
        {
            // 1. Khởi tạo Query và Join các bảng liên quan
            var query = _context.Topics
                .Include(t => t.Group)
                    .ThenInclude(g => g.Leader)
                .Include(t => t.Group)
                    .ThenInclude(g => g.MentorAssignment)
                        .ThenInclude(ma => ma.Mentor)
                .AsNoTracking()
                .AsQueryable();

            // ================= 2. NGHIỆP VỤ LỌC (FILTER) =================

            // Chỉ lọc những cột chắc chắn có trong DB
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(t => t.Status == request.Status.Trim());
            }

            if (request.MentorId.HasValue)
            {
                query = query.Where(t => t.Group != null
                    && t.Group.MentorAssignment != null
                    && t.Group.MentorAssignment.MentorId == request.MentorId.Value);
            }

            // ĐÃ LƯỢC BỎ: Lọc theo SemesterId và MajorId vì DB chưa có cột này.

            // ================= 3. NGHIỆP VỤ TÌM KIẾM (SEARCH) =================

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var k = request.Keyword.Trim().ToLower();
                bool isNumeric = int.TryParse(k, out int topicId);

                // ĐÃ LƯỢC BỎ: t.TitleEn. Chỉ tìm trên ID, Title (Tiếng Việt) và Tên Nhóm trưởng
                query = query.Where(t =>
                    (isNumeric && t.TopicId == topicId) ||
                    (t.Title != null && t.Title.ToLower().Contains(k)) ||
                    (t.Group != null && t.Group.Leader != null && t.Group.Leader.FullName.ToLower().Contains(k))
                );
            }

            // ================= 4. SẮP XẾP & PHÂN TRANG =================

            int totalRecords = await query.CountAsync();

            var data = await query
                .OrderByDescending(t => t.CreatedAt) // Mới nhất lên đầu
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(t => new TopicSearchResponse
                {
                    TopicId = t.TopicId,
                    Title = t.Title,
                    TitleEn = "", // Trả về chuỗi rỗng tĩnh để không query xuống DB
                    Status = t.Status,
                    GroupId = t.GroupId,
                    LeaderName = t.Group != null && t.Group.Leader != null ? t.Group.Leader.FullName : "N/A",
                    MentorName = t.Group != null && t.Group.MentorAssignment != null && t.Group.MentorAssignment.Mentor != null
                                 ? t.Group.MentorAssignment.Mentor.FullName : "Chưa có Mentor",
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return new
            {
                Data = data,
                TotalRecords = totalRecords,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize)
            };
        }
    }
}