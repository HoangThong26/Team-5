namespace CapstoneProject.Application.DTO
{
    public class CouncilEligibilityDto
    {
        public int GroupId { get; set; }
        public int TotalWeeks { get; set; }
        public decimal TotalScore { get; set; }
        public decimal MaxScore { get; set; }
        public double Percentage { get; set; }
        public bool IsEligibleForCouncil { get; set; }
        public int EvaluatedWeeks { get; set; }
    }
}
