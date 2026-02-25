using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Domain.Entities;

[Index("Email", Name = "UQ__Users__A9D10534693D73B8", IsUnique = true)]
public partial class User
{
    [Key]
    public int UserId { get; set; }

    [StringLength(255)]
    public string Email { get; set; } = null!;

    [StringLength(500)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(255)]
    public string FullName { get; set; } = null!;

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(500)]
    public string? AvatarUrl { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }
    public string? Role { get; set; }
    public string? VerifyToken { get; set; }

    public bool? EmailVerified { get; set; }

    public DateTime? VerifyTokenExpire { get; set; }

    public int? FailedLoginCount { get; set; }

    public DateTime? LockUntil { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }


    [InverseProperty("User")]
    public virtual ICollection<CouncilMember> CouncilMembers { get; set; } = new List<CouncilMember>();

    [InverseProperty("ResolvedByNavigation")]
    public virtual ICollection<GradeComplaint> GradeComplaintResolvedByNavigations { get; set; } = new List<GradeComplaint>();

    [InverseProperty("SubmittedByNavigation")]
    public virtual ICollection<GradeComplaint> GradeComplaintSubmittedByNavigations { get; set; } = new List<GradeComplaint>();

    [InverseProperty("Receiver")]
    public virtual ICollection<GroupInvitation> GroupInvitationReceivers { get; set; } = new List<GroupInvitation>();

    [InverseProperty("Sender")]
    public virtual ICollection<GroupInvitation> GroupInvitationSenders { get; set; } = new List<GroupInvitation>();

    [InverseProperty("User")]
    public virtual GroupMember? GroupMember { get; set; }

    [InverseProperty("Leader")]
    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    [InverseProperty("User")]
    public virtual ICollection<LoginHistory> LoginHistories { get; set; } = new List<LoginHistory>();

    [InverseProperty("Mentor")]
    public virtual ICollection<MentorAssignment> MentorAssignments { get; set; } = new List<MentorAssignment>();

    [InverseProperty("Mentor")]
    public virtual ICollection<MentorRequest> MentorRequests { get; set; } = new List<MentorRequest>();

    [InverseProperty("User")]
    public virtual ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();

    [InverseProperty("User")]
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    [InverseProperty("ReviewedByNavigation")]
    public virtual ICollection<TopicVersion> TopicVersions { get; set; } = new List<TopicVersion>();

    [InverseProperty("Mentor")]
    public virtual ICollection<WeeklyEvaluation> WeeklyEvaluations { get; set; } = new List<WeeklyEvaluation>();
}
