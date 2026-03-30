namespace CapstoneProject.Application.DTO
{
    public class TopicSearchRequest
    {
        // 1. Tìm kiếm chung (Global Search)
        public string? Keyword { get; set; }

        // 2. Các tiêu chí lọc (Filter)
        public int? SemesterId { get; set; }
        public int? MajorId { get; set; }
        public int? MentorId { get; set; }
        public string? Status { get; set; }

        // 3. Phân trang (Pagination)
        private int _pageSize = 10;
        private const int MaxPageSize = 50; // Chặn hacker kéo quá nhiều data

        public int PageIndex { get; set; } = 1;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }
    }
}