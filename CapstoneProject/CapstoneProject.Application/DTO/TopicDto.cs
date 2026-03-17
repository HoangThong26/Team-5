namespace CapstoneProject.Application.DTO
{
    public class TopicDto
    {
        public int TopicId { get; set; }
        public int GroupId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int VersionId { get; set; }
        public string? GroupName { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? ReviewComment { get; set; }
    }
}