namespace CapstoneProject.Application.DTO
{
    public class DefenseScoreDto
    {
        public decimal Score { get; set; }
        public string? Comment { get; set; }

        // Bạn có thể bổ sung thêm các trường này nếu muốn UI hiển thị chi tiết hơn
        public decimal? PresentationScore { get; set; }
        public decimal? DemoScore { get; set; }
        public decimal? QAScore { get; set; }
    }
}