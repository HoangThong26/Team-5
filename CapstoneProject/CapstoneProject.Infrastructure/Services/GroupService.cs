using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IService;
using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Domain.Entities;
using System;
using System.Threading.Tasks;
using System.Linq; // Nhớ đảm bảo có using này ở trên cùng để dùng được .Select()

namespace CapstoneProject.Infrastructure.Services
{
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _groupRepository;

        public GroupService(IGroupRepository groupRepository)
        {
            _groupRepository = groupRepository;
        }

        public async Task<string> CreateGroupAsync(int userId, CreateGroupRequest request)
        {
            // 1. Validate: Kiểm tra số lượng cấu hình (2-5 người)
            // (Mặc dù DTO đã chặn ở DataAnnotations, nhưng check thêm ở Service cho chắc chắn)
            if (request.TargetMembers < 2 || request.TargetMembers > 5)
            {
                return "Số lượng thành viên nhóm phải từ 2 đến 5 người!";
            }

            // 2. Validate: Kiểm tra sinh viên có nhóm chưa
            bool hasGroup = await _groupRepository.IsUserInAnyGroupAsync(userId);
            if (hasGroup) return "Bạn đã tham gia một nhóm khác!";

            // 3. Khởi tạo đối tượng Group
            var group = new Group
            {
                GroupName = request.GroupName,
                LeaderId = userId,
                Status = "Forming",
                IsLocked = false,
                CreatedAt = DateTime.Now
                // Lưu ý: Nếu muốn lưu lại TargetMembers xuống SQL, bạn cần vào DB tạo thêm cột MaxMembers cho bảng Groups nhé!
            };

            // 4. Khởi tạo đối tượng Leader
            var member = new GroupMember
            {
                UserId = userId,
                RoleInGroup = "Leader",
                JoinedAt = DateTime.Now
            };

            // 5. Gọi Repository để lưu xuống DB
            try
            {
                await _groupRepository.CreateGroupWithLeaderAsync(group, member);
                return "Tạo nhóm thành công!";
            }
            catch (Exception)
            {
                return "Lỗi hệ thống khi lưu dữ liệu tạo nhóm.";
            }
        }
        public async Task<GroupDetailResponse?> GetGroupDetailsAsync(int groupId)
        {
            // 1. Gọi Repository lấy dữ liệu thô
            var group = await _groupRepository.GetGroupByIdAsync(groupId);
            if (group == null) return null;

            // 2. Map (Chuyển đổi) từ Entity sang DTO để trả về cho API
            var response = new GroupDetailResponse
            {
                GroupId = group.GroupId,
                GroupName = group.GroupName,
                Status = group.Status,
                CreatedAt = group.CreatedAt,
                Members = group.GroupMembers.Select(m => new GroupMemberDto
                {
                    UserId = m.UserId,
                    RoleInGroup = m.RoleInGroup,
                    JoinedAt = m.JoinedAt
                }).ToList()
            };

            return response;
        }

    }
}