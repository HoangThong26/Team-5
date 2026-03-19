using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IService;
using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;


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


            if (invitee.Role != "Student")
                return $"Invalid request: You can only invite Students. You cannot invite a {invitee.Role}!";

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
            string rejectLink = $"{baseUrl}/api/groups/reject-invite?invitationId={invitationId}";

            string subject = $"Invitation to join Capstone Group: {groupName}";
            string body = $@"
        <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: auto; border: 1px solid #eee; padding: 20px; border-radius: 10px;'>
            <h3 style='color: #0056b3;'>Hello {fullName},</h3>
            <p>You have received an invitation to join the group <strong>{groupName}</strong> for the Capstone Project.</p>
            <p>Please choose one of the options below:</p>
            
            <div style='margin: 30px 0; text-align: center;'>
                <a href='{acceptLink}' style='padding: 12px 25px; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block; margin-right: 10px;'>
                    Confirm Participation
                </a>

                <a href='{rejectLink}' style='padding: 12px 25px; background-color: #dc3545; color: white; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>
                    Reject Invitation
                </a>
            </div>

            <p style='font-size: 0.9em; color: #666;'>If you didn't expect this invitation, you can safely click Reject or ignore this email.</p>
            <hr style='border: none; border-top: 1px solid #eee; margin-top: 20px;' />
            <p style='font-size: 12px; color: #999; text-align: center;'>This is an automated email from the system, please do not reply to this message.</p>
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

        public async Task<string> RejectInviteAsync(int invitationId)
        {
            var invitation = await _groupRepository.GetInvitationByIdAsync(invitationId);

            if (invitation == null)
            {
                return "Invitation not found or has been deleted.";
            }
            if (invitation.Status != "Pending")
            {
                return $"This invitation has already been {invitation.Status.ToLower()}.";
            }
            invitation.Status = "Rejected";
            bool isUpdated = await _groupRepository.UpdateInvitationStatusAsync(invitation);

            if (isUpdated)
            {
                return "You have successfully rejected the invitation.";
            }

            return "An error occurred while processing your request.";
        }

        public async Task<string> KickMemberAsync(int requesterId, int groupId, int targetUserId)
        {
            var group = await _groupRepository.GetGroupByIdAsync(groupId);
            if (group == null)
                throw new Exception("Group not found.");


            if (group.LeaderId != requesterId)
                throw new Exception("Only the Leader can kick members.");

            if (group.IsLocked == true || group.Status != "Forming")
                throw new Exception("The group is locked or no longer in the forming stage.");

            if (requesterId == targetUserId)
                throw new Exception("The Leader cannot kick themselves.");

            var memberToKick = await _groupRepository.GetGroupMemberAsync(groupId, targetUserId);
            if (memberToKick == null)
                throw new Exception("This user is not a member of your group.");
            bool isRemoved = await _groupRepository.RemoveGroupMemberAsync(memberToKick);

            if (!isRemoved)
                throw new Exception("An error occurred while trying to remove the member.");

            return "Member has been successfully kicked from the group.";
        }


        public async Task<List<GroupDetailResponse>> GetAllGroupsForAdminAsync()
        {
            var groups = await _groupRepository.GetAllGroupsWithDetailsAsync();

            return groups.Select(group => new GroupDetailResponse
            {
                GroupId = group.GroupId,
                GroupName = group.GroupName,
                Status = group.Status,
                CreatedAt = group.CreatedAt,
                MentorId = group?.MentorAssignment?.MentorId,
                MentorName = group?.MentorAssignment?.Mentor?.FullName,
                Members = group?.GroupMembers?.Select(m => new GroupMemberDto
                {
                    UserId = m.UserId,
                    FullName = m.User?.FullName,
                    RoleInGroup = m.RoleInGroup,
                    JoinedAt = m.JoinedAt
                }).ToList() ?? new List<GroupMemberDto>()
            }).ToList();
        }

        public async Task<string> DeleteGroupByAdminAsync(int groupId, int currentUserId, string currentUserRole)
        {
            try
            {
                int? leaderId = await _groupRepository.GetGroupLeaderIdAsync(groupId);

                if (leaderId == null)
                    return "Group not found in the system.";

                bool isAdmin = currentUserRole == "Admin";
                bool isLeader = (leaderId == currentUserId);

                if (!isAdmin && !isLeader)
                {
                    return "Access denied. Only Admin or Group Leader can delete this group.";
                }

                bool isDeleted = await _groupRepository.DeleteGroupAsync(groupId);

                if (!isDeleted) return "Failed to delete the group.";

                return "Group and all related data have been successfully deleted.";
            }
            catch (Exception ex)
            {
                return $"An error occurred while deleting the group: {ex.Message}";
            }
        }

        public async Task<string> KickMentorByAdminAsync(int groupId)
        {
            try
            {
                bool isKicked = await _groupRepository.RemoveMentorFromGroupAsync(groupId);
                if (!isKicked)
                {
                    return "This group currently has no Mentor or does not exist.";
                }
                return "Mentor successfully removed from the group! Group status has been changed back to 'Forming'.";
            }
            catch (Exception ex)
            {
                return $"An error occurred while removing the mentor: {ex.Message}";
            }
        }

        public async Task<string> AssignMentorAsync(int groupId, int mentorId)
        {
            var group = await _groupRepository.GetGroupByIdAsync(groupId);

            if (group == null)
            {
                return "Group not found.";
            }

            var memberCount = group.GroupMembers?.Count ?? 0;
            if (memberCount < 5)
            {
                return $"Cannot assign mentor. This group only has {memberCount}/5 members.";
            }

            if (group.MentorAssignment == null)
            {
                group.MentorAssignment = new MentorAssignment
                {
                    GroupId = groupId, 
                    MentorId = mentorId,
                    AssignedAt = DateTime.Now
                };
            }
            else
            {
                group.MentorAssignment.MentorId = mentorId;
                group.MentorAssignment.AssignedAt = DateTime.Now;
            }

            group.Status = "Active";

            try
            {
                await _groupRepository.UpdateGroupAsync(group);
                return "SUCCESS";
            }
            catch (Exception ex)
            {
                return $"Error updating group: {ex.Message}";
            }
        }

      
    }

}
