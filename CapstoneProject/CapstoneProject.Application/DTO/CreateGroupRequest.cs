using System.ComponentModel.DataAnnotations;

namespace CapstoneProject.Application.DTO
{
    public class CreateGroupRequest
    {
        [Required(ErrorMessage = "Tên nhóm không được để trống")]
        public string GroupName { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập số lượng thành viên dự kiến")]
        [Range(2, 5, ErrorMessage = "Số lượng thành viên nhóm phải từ 2 đến 5 người!")]
        public int TargetMembers { get; set; }
    }
}