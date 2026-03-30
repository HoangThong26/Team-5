namespace CapstoneProject.Application.DTO
{
    public class TopicSearchResponse
    {
        public int TopicId { get; set; }
        public string? Title { get; set; }
        public string? TitleEn { get; set; } // Tên tiếng Anh
        public string? Status { get; set; }
        public int? GroupId { get; set; }
        public string? LeaderName { get; set; }
        public string? MentorName { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}