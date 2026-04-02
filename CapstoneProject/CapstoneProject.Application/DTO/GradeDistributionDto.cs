namespace CapstoneProject.Application.DTOs
{
    public class GradeDistributionDto
    {
        public string GradeLetter { get; set; } = string.Empty; // VD: "A", "B+", "C"
        public int Count { get; set; } // Số lượng nhóm đạt điểm này
    }
}