using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IService;
using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Domain.Entities;
using Microsoft.Extensions.Configuration; // Dùng để đọc appsettings.json
using System;
using System.Linq;
using System.Net; // Dùng cho NetworkCredential
using System.Net.Mail; // Dùng cho SmtpClient và MailMessage
using System.Threading.Tasks;

namespace CapstoneProject.Infrastructure.Services
{
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IConfiguration _configuration; // Khai báo IConfiguration

        // Cập nhật Constructor
        public GroupService(IGroupRepository groupRepository, IConfiguration configuration)
        {
            _groupRepository = groupRepository;
            _configuration = configuration;
        }

        public async Task<string> CreateGroupAsync(int userId, CreateGroupRequest request)
        {
            // 1. Validate: Kiểm tra số lượng cấu hình (2-5 người)
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

            // Lấy tên sinh viên (Nếu User của bạn dùng cột khác cho tên thì hãy đổi invitee.FullName thành cột tương ứng)
            string inviteeName = invitee.FullName ?? "Sinh viên";

            // TRẠM 5: Kiểm tra tính độc quyền
            bool hasGroup = await _groupRepository.IsUserInAnyGroupAsync(invitee.UserId);
            if (hasGroup) return $"Sinh viên {inviteeName} đã tham gia một nhóm khác!";

            // TRẠM 6: Tránh spam lời mời
            bool alreadyInvited = await _groupRepository.HasPendingInvitationAsync(request.GroupId, invitee.UserId);
            if (alreadyInvited) return "Bạn đã gửi lời mời cho sinh viên này rồi, vui lòng chờ họ xác nhận!";

            // VƯỢT QUA MỌI TRẠM -> TẠO LỜI MỜI VÀ GỬI EMAIL
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

                // Lưu vào Database
                await _groupRepository.AddInvitationAsync(invitation);

                // GỌI HÀM GỬI EMAIL
                await SendInvitationEmailAsync(invitee.Email, inviteeName, group.GroupName, invitation.InvitationId);

                return "Thành công: Đã tạo lời mời và gửi Email thông báo!";
            }
            catch (Exception ex)
            {
                // Trả về ex.Message để dễ dàng debug lỗi nếu có
                return $"Lỗi hệ thống: {ex.Message}";
            }
        }

        // =========================================================================
        // HÀM PRIVATE: CHỈ CHỊU TRÁCH NHIỆM GỬI EMAIL THÔNG BÁO
        // =========================================================================
        private async Task SendInvitationEmailAsync(string toEmail, string fullName, string groupName, int invitationId)
        {
            // 1. Đọc tài khoản Gmail từ file appsettings.json gốc của bạn
            string senderEmail = _configuration["EmailSettings:Email"];
            string senderPassword = _configuration["EmailSettings:Password"];
            string baseUrl = _configuration["AppSettings:BaseUrl"];

            // 2. Gắn cứng cấu hình máy chủ của Google
            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587;
            string senderName = "Hệ thống Capstone Project";

            // 3. Chuẩn bị Link và Nội dung Email (Có trang trí CSS một chút cho đẹp)
            string acceptLink = $"{baseUrl}/api/groups/accept-invite?invitationId={invitationId}";

            string subject = $"Lời mời tham gia nhóm Capstone: {groupName}";
            string body = $@"
                <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <h3 style='color: #0056b3;'>Chào {fullName},</h3>
                    <p>Bạn vừa nhận được lời mời tham gia nhóm <strong>{groupName}</strong> để thực hiện Đồ án Capstone.</p>
                    <p>Vui lòng click vào nút bên dưới để xác nhận tham gia:</p>
                    <p style='margin: 20px 0;'>
                        <a href='{acceptLink}' style='padding: 10px 20px; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>Xác nhận tham gia</a>
                    </p>
                    <p>Nếu bạn không muốn tham gia, vui lòng bỏ qua email này.</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin-top: 20px;' />
                    <p style='font-size: 12px; color: #999;'>Đây là email tự động từ hệ thống, vui lòng không trả lời thư này.</p>
                </div>";

            // 4. Tiến hành gửi mail qua SmtpClient
            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            using var smtpClient = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true // Rất quan trọng: Gmail bắt buộc dùng SSL
            };

            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task<string> AcceptInviteAsync(int invitationId)
        {
            // 1. Lấy thông tin lời mời
            var invitation = await _groupRepository.GetInvitationByIdAsync(invitationId);
            if (invitation == null) return "Không tìm thấy lời mời này trong hệ thống.";

            // BẢO VỆ CHỐNG LỖI INT? -> INT
            if (invitation.GroupId == null || invitation.ReceiverId == null)
                return "Dữ liệu lời mời bị lỗi (thiếu ID nhóm hoặc người nhận).";

            // 2. Kiểm tra trạng thái lời mời
            if (invitation.Status != "Pending")
                return "Lời mời này đã được xác nhận hoặc đã hết hạn/bị hủy.";

            // 3. Kiểm tra xem nhóm có bị đầy không (trước khi cho vào)
            // Đã thêm .Value để chuyển từ int? sang int
            int currentMembers = await _groupRepository.GetMemberCountAsync(invitation.GroupId.Value);
            if (currentMembers >= 5) return "Rất tiếc, nhóm này đã đủ 5 thành viên.";

            // 4. Kiểm tra xem sinh viên này có vô tình join nhóm khác trong lúc chờ mail không
            // Đã thêm .Value
            bool hasGroup = await _groupRepository.IsUserInAnyGroupAsync(invitation.ReceiverId.Value);
            if (hasGroup) return "Bạn đã tham gia một nhóm khác rồi.";

            try
            {
                // 5. Cập nhật trạng thái lời mời thành "Accepted"
                invitation.Status = "Accepted";
                await _groupRepository.UpdateInvitationAsync(invitation);

                // 6. Đưa sinh viên vào nhóm
                var newMember = new GroupMember
                {
                    GroupId = invitation.GroupId.Value,    // Đã thêm .Value
                    UserId = invitation.ReceiverId.Value,  // Đã thêm .Value
                    RoleInGroup = "Member",
                    JoinedAt = DateTime.Now
                };
                await _groupRepository.AddGroupMemberAsync(newMember);

                return "Xác nhận tham gia nhóm thành công! Chào mừng bạn đến với nhóm.";
            }
            catch (Exception ex)
            {
                return $"Lỗi hệ thống khi xử lý tham gia nhóm: {ex.Message}";
            }
        }
    }
}