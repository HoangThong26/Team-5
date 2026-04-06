namespace CapstoneProject.Application.DTO
{
    public class CouncilUserDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int CouncilId { get; set; }
        public string CouncilName { get; set; } = string.Empty;
    }
}
