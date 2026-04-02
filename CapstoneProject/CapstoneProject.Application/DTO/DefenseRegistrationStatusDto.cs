namespace CapstoneProject.Application.DTO
{
    public class DefenseRegistrationStatusDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int EvaluatedWeeks { get; set; }
        public int PassedWeeks { get; set; }
        public double PassRate { get; set; }
        public bool IsEligibleForDefense { get; set; }
        public bool IsRegistered { get; set; }
    }
}
