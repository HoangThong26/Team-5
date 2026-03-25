namespace CapstoneProject.Application.DTO
{
    public class TopicSearchResponse
    {
        public int TopicId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int GroupId { get; set; }
        public string LeaderName { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }
}