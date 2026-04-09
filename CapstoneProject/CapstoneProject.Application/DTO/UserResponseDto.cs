namespace CapstoneProject.Application.DTO
{
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public string GroupName { get; set; } // Hiển thị cột GROUP
        public decimal? Grade { get; set; }    // Hiển thị cột GRADE
    }
}
