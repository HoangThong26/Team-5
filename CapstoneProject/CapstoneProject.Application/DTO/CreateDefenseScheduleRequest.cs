namespace CapstoneProject.Application.DTO
{
    public class CreateDefenseScheduleRequest
    {
        public int DefenseId { get; set; }
        public int CouncilId { get; set; }
        public string Room { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
