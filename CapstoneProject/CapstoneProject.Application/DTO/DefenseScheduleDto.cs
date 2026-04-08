namespace CapstoneProject.Application.DTO
{
    public class DefenseScheduleDto
    {
        public int DefenseId { get; set; }
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public int? CouncilId { get; set; }
        public string? Room { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Status { get; set; }
    }
}