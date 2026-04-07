namespace CapstoneProject.Application.DTO
{
    public class MentorRequestDto
    {
        public int Id { get; set; }
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public int? MentorId { get; set; }
        public string? Status { get; set; }
        public DateTime? RequestedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public string? GroupLeaderName { get; set; }
    }
}