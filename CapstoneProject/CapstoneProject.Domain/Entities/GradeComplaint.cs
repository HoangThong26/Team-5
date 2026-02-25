using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Domain.Entities;

public partial class GradeComplaint
{
    [Key]
    public int ComplaintId { get; set; }

    public int? GroupId { get; set; }

    public int? SubmittedBy { get; set; }

    public string? Reason { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    public string? ResolutionNote { get; set; }

    public int? ResolvedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    [ForeignKey("GroupId")]
    [InverseProperty("GradeComplaints")]
    public virtual Group? Group { get; set; }

    [ForeignKey("ResolvedBy")]
    [InverseProperty("GradeComplaintResolvedByNavigations")]
    public virtual User? ResolvedByNavigation { get; set; }

    [ForeignKey("SubmittedBy")]
    [InverseProperty("GradeComplaintSubmittedByNavigations")]
    public virtual User? SubmittedByNavigation { get; set; }
}
