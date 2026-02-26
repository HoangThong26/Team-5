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
        public async Task<string> InviteMemberAsync(int leaderId, InviteMemberRequest request)
        {
            // TRẠM 1: Lấy thông tin nhóm & Kiểm tra quyền
            var group = await _groupRepository.GetGroupByIdAsync(request.GroupId);
            if (group == null) return "Không tìm thấy nhóm!";
            if (group.LeaderId != leaderId) return "Bạn không phải trưởng nhóm, bạn không có quyền mời thành viên!";

            // TRẠM 2: Kiểm tra trạng thái nhóm
            if (group.IsLocked == true || group.Status != "Forming")
                return "Nhóm đã khóa hoặc không còn ở trạng thái gom người!";

            // TRẠM 3: Kiểm tra sức chứa (Tối đa 5 người)
            int currentMembers = await _groupRepository.GetMemberCountAsync(request.GroupId);
            if (currentMembers >= 5) return "Nhóm đã đạt số lượng tối đa (5 người)!";

            // TRẠM 4: Kiểm tra người được mời
            var invitee = await _groupRepository.GetUserByEmailAsync(request.InviteeEmail);
            if (invitee == null) return "Không tìm thấy sinh viên nào sử dụng Email này trong hệ thống!";
            if (invitee.UserId == leaderId) return "Bạn không thể tự mời chính mình!";

            // TRẠM 5: Kiểm tra tính độc quyền
            bool hasGroup = await _groupRepository.IsUserInAnyGroupAsync(invitee.UserId);
            if (hasGroup) return $"Sinh viên {invitee.FullName} đã tham gia một nhóm khác!";

            // TRẠM 6: Tránh spam lời mời
            bool alreadyInvited = await _groupRepository.HasPendingInvitationAsync(request.GroupId, invitee.UserId);
            if (alreadyInvited) return "Bạn đã gửi lời mời cho sinh viên này rồi, vui lòng chờ họ xác nhận!";

            // VƯỢT QUA MỌI TRẠM -> TẠO LỜI MỜI
            try
            {
                var invitation = new GroupInvitation
                {
                    GroupId = request.GroupId,
                    SenderId = leaderId,
                    ReceiverId = invitee.UserId,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };

                await _groupRepository.AddInvitationAsync(invitation);

                // TODO: GỌI HÀM GỬI EMAIL TẠI ĐÂY (Mình sẽ setup ở bước sau để bạn test code API chạy mượt trước)

                return "Thành công: Đã tạo lời mời gia nhập nhóm!";
            }
            catch (Exception)
            {
                return "Lỗi hệ thống khi lưu lời mời.";
            }
        }

    }
}