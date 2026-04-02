namespace CapstoneProject.Application.DTO
{
    public class DefenseRegistrationItemDto
    {
        public int DefenseId { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? CouncilId { get; set; }
        public string? CouncilName { get; set; }
        public string? Room { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
