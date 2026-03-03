using System.ComponentModel.DataAnnotations;

namespace CapstoneProject.Application.DTO
{
    public class InviteMemberRequest
    {
        [Required(ErrorMessage = "Group ID is required")]
        public int GroupId { get; set; }

        [Required(ErrorMessage = "Invitee email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string InviteeEmail { get; set; } = string.Empty;
    }
}