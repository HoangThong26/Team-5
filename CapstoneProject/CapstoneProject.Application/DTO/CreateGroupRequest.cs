using System.ComponentModel.DataAnnotations;

namespace CapstoneProject.Application.DTO
{
    public class CreateGroupRequest
    {
        [Required(ErrorMessage = "Group name is required")]
        public string GroupName { get; set; } = null!;

        [Required(ErrorMessage = "Please enter the expected number of members")]
        [Range(2, 5, ErrorMessage = "Group size must be between 2 and 5 members!")]
        public int TargetMembers { get; set; }
    }
}