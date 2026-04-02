using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Domain.Entities;

public partial class Group
{
    [Key]
    public int GroupId { get; set; }

    [StringLength(255)]
    public string GroupName { get; set; } = null!;

    public int? LeaderId { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    public bool? IsLocked { get; set; }

    public DateTime? CreatedAt { get; set; }
    // Thêm các dòng này vào class Group của bạn
    public int? SemesterId { get; set; }
    [ForeignKey("SemesterId")]
    public virtual Semester? Semester { get; set; }

    public int? MajorId { get; set; }
    [ForeignKey("MajorId")]
    public virtual Major? Major { get; set; }
    [InverseProperty("Group")]
    public virtual ICollection<DefenseSchedule> DefenseSchedules { get; set; } = new List<DefenseSchedule>();

    [InverseProperty("Group")]
    public virtual FinalGrade? FinalGrade { get; set; }

    [InverseProperty("Group")]
    public virtual ICollection<GradeComplaint> GradeComplaints { get; set; } = new List<GradeComplaint>();

    [InverseProperty("Group")]
    public virtual ICollection<GroupInvitation> GroupInvitations { get; set; } = new List<GroupInvitation>();

    [InverseProperty("Group")]
    public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

    [ForeignKey("LeaderId")]
    [InverseProperty("Groups")]
    public virtual User? Leader { get; set; }

    [InverseProperty("Group")]
    public virtual MentorAssignment? MentorAssignment { get; set; }

    [InverseProperty("Group")]
    public virtual ICollection<MentorRequest> MentorRequests { get; set; } = new List<MentorRequest>();

    [InverseProperty("Group")]
    public virtual Topic? Topic { get; set; }

    [InverseProperty("Group")]
    public virtual ICollection<WeeklyReport> WeeklyReports { get; set; } = new List<WeeklyReport>();
}
