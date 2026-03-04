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

        public List<GroupMemberDto> Members { get; set; } = new List<GroupMemberDto>();
    }

    public class GroupMemberDto
    {
        public int UserId { get; set; }
        public string? RoleInGroup { get; set; }
        public DateTime? JoinedAt { get; set; }

    }
}