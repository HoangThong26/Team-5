namespace CapstoneProject.Application.DTO
{
    public class FinalGradeResponseDto
    {
        public int GroupId { get; set; }
        public decimal? AverageScore { get; set; }
        public string? GradeLetter { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public FinalGradeGroupDto? Group { get; set; }
    }

    public class FinalGradeGroupDto
    {
        public string? GroupName { get; set; }
        public FinalGradeTopicDto? Topic { get; set; }
    }

    public class FinalGradeTopicDto
    {
        public string? TopicName { get; set; }
    }
}
