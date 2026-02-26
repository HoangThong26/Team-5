using System;
using System.Collections.Generic;

namespace CapstoneProject.Application.DTO
{
    public class GroupDetailResponse
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Danh sách các thành viên hiện tại trong nhóm
        public List<GroupMemberDto> Members { get; set; } = new List<GroupMemberDto>();
    }

    public class GroupMemberDto
    {
        public int UserId { get; set; }
        public string? RoleInGroup { get; set; }
        public DateTime? JoinedAt { get; set; }
        // Sau này nếu có bảng User đầy đủ, bạn có thể Join thêm FullName, Email vào đây
    }
}