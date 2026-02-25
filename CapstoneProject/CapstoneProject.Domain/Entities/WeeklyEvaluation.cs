using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Domain.Entities;

public partial class WeeklyEvaluation
{
    [Key]
    public int Id { get; set; }

    public int? ReportId { get; set; }

    public int? MentorId { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? Score { get; set; }

    public string? Comment { get; set; }

    public bool? IsPass { get; set; }

    public DateTime? ReviewedAt { get; set; }

    [ForeignKey("MentorId")]
    [InverseProperty("WeeklyEvaluations")]
    public virtual User? Mentor { get; set; }

    [ForeignKey("ReportId")]
    [InverseProperty("WeeklyEvaluations")]
    public virtual WeeklyReport? Report { get; set; }
}
