using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IService;
using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CapstoneProject.Infrastructure.Services
{
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IConfiguration _configuration;

        public GroupService(IGroupRepository groupRepository, IConfiguration configuration)
        {
            _groupRepository = groupRepository;
            _configuration = configuration;
        }

        public async Task<string> CreateGroupAsync(int userId, CreateGroupRequest request)
        {
            if (request.TargetMembers < 2 || request.TargetMembers > 5)
            {
                return "Group size must be between 2 and 5 members!";
            }

            bool hasGroup = await _groupRepository.IsUserInAnyGroupAsync(userId);
            if (hasGroup) return "You are already a member of another group!";

            var group = new Group
            {
                GroupName = request.GroupName,
                LeaderId = userId,
                Status = "Forming",
                IsLocked = false,
                CreatedAt = DateTime.Now
            };

            var member = new GroupMember
            {
                UserId = userId,
                RoleInGroup = "Leader",
                JoinedAt = DateTime.Now
            };

            try
            {
                var result = await _groupRepository.CreateGroupWithLeaderAsync(group, member);
                return "Group created successfully! Invite more members to reach 4 for mentor assignment.";
            }
            catch (Exception)
            {
                return "System error while saving group data.";
            }
        }

        public async Task<GroupDetailResponse?> GetGroupDetailsAsync(int groupId)
        {
            var group = await _groupRepository.GetGroupByIdAsync(groupId);
            if (group == null) return null;

            var response = new GroupDetailResponse
            {
                GroupId = group.GroupId,
                GroupName = group.GroupName,
                Status = group.Status,
                CreatedAt = group.CreatedAt,
                Members = group.GroupMembers.Select(m => new GroupMemberDto
                {
                    UserId = m.UserId,
                    FullName = m.User?.FullName,
                    RoleInGroup = m.RoleInGroup,
                    JoinedAt = m.JoinedAt
                }).ToList()
            };

            return response;
        }

        public async Task<string> AcceptInviteAsync(int invitationId)
        {
            try
            {
                var result = await _groupRepository.AcceptInvitationWithMentorCheckAsync(invitationId);
                return result;
            }
            catch (Exception ex)
            {
                return $"System error while processing participation: {ex.Message}";
            }
        }

        public async Task<string> InviteMemberAsync(int leaderId, InviteMemberRequest request)
        {
            var group = await _groupRepository.GetGroupByIdAsync(request.GroupId);
            if (group == null) return "Group not found!";
            if (group.LeaderId != leaderId) return "You are not the group leader, you do not have permission to invite members!";

            if (group.IsLocked == true || group.Status != "Forming")
                return "The group is locked or no longer in forming status!";

            int currentMembers = await _groupRepository.GetMemberCountAsync(request.GroupId);
            if (currentMembers >= 5) return "The group has reached the maximum capacity (5 members)!";

            var invitee = await _groupRepository.GetUserByEmailAsync(request.InviteeEmail);
            if (invitee == null) return "No student found with this email address in the system!";
            if (invitee.UserId == leaderId) return "You cannot invite yourself!";

            string inviteeName = invitee.FullName ?? "Student";

            bool hasGroup = await _groupRepository.IsUserInAnyGroupAsync(invitee.UserId);
            if (hasGroup) return $"Student {inviteeName} is already a member of another group!";

            bool alreadyInvited = await _groupRepository.HasPendingInvitationAsync(request.GroupId, invitee.UserId);
            if (alreadyInvited) return "You have already sent an invitation to this student, please wait for their confirmation!";

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

                await SendInvitationEmailAsync(invitee.Email, inviteeName, group.GroupName, invitation.InvitationId);

                return "Success: Invitation created and notification email sent!";
            }
            catch (Exception ex)
            {
                return $"System error: {ex.Message}";
            }
        }

        private async Task SendInvitationEmailAsync(string toEmail, string fullName, string groupName, int invitationId)
        {
            string senderEmail = _configuration["EmailSettings:SenderEmail"];
            string senderPassword = _configuration["EmailSettings:AppPassword"];
            string baseUrl = _configuration["AppSettings:BaseUrl"];

            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587;
            string senderName = "Capstone Project System";

            string acceptLink = $"{baseUrl}/api/groups/accept-invite?invitationId={invitationId}";

            string subject = $"Invitation to join Capstone Group: {groupName}";
            string body = $@"
                <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <h3 style='color: #0056b3;'>Hello {fullName},</h3>
                    <p>You have received an invitation to join the group <strong>{groupName}</strong> for the Capstone Project.</p>
                    <p>Please click the button below to confirm your participation:</p>
                    <p style='margin: 20px 0;'>
                        <a href='{acceptLink}' style='padding: 10px 20px; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>Confirm Participation</a>
                    </p>
                    <p>If you do not wish to join, please ignore this email.</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin-top: 20px;' />
                    <p style='font-size: 12px; color: #999;'>This is an automated email from the system, please do not reply to this message.</p>
                </div>";

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
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(mailMessage);
        }


        public async Task<GroupDetailResponse?> GetMyGroupAsync(int userId)
        {
            var group = await _groupRepository.GetGroupWithDetailsByUserIdAsync(userId);

            if (group == null) return null;
            var assignment = group.MentorAssignment;

            var response = new GroupDetailResponse
            {
                GroupId = group.GroupId,
                GroupName = group.GroupName,
                Status = group.Status,
                CreatedAt = group.CreatedAt,
                MentorId = assignment?.MentorId,
                MentorName = assignment?.Mentor?.FullName,

                Members = group.GroupMembers?.Select(m => new GroupMemberDto
                {
                    UserId = m.UserId,
                    FullName = m.User?.FullName,
                    RoleInGroup = m.RoleInGroup,
                    JoinedAt = m.JoinedAt
                }).ToList() ?? new List<GroupMemberDto>()
            };

            return response;
        }

    }
}