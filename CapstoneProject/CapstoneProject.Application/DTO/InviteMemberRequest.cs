using System.ComponentModel.DataAnnotations;

namespace CapstoneProject.Application.DTO
{
    public class InviteMemberRequest
    {
        [Required(ErrorMessage = "Mã nhóm không được để trống")]
        public int GroupId { get; set; }

        [Required(ErrorMessage = "Email người được mời không được để trống")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        public string InviteeEmail { get; set; } = string.Empty;
    }
}