namespace CapstoneProject.Application.DTO;

public class DefenseEvaluationRequest
{
    public int DefenseId { get; set; }
    public int CouncilMemberUserId { get; set; } 
    public decimal PresentationScore { get; set; }
    public decimal DemoScore { get; set; }
    public decimal QAScore { get; set; }
    public decimal FinalScore { get; set; } 
    public string? Comment { get; set; }
}