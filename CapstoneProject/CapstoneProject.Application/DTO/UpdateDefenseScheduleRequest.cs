namespace CapstoneProject.Application.DTO
{
    public class UpdateDefenseScheduleRequest
    {
        public int CouncilId { get; set; }
        public string Room { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
