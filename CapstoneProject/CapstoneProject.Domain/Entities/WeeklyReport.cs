using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Domain.Entities;

[Index("GroupId", "WeekId", Name = "UQ_Group_Week", IsUnique = true)]
public partial class WeeklyReport
{
    [Key]
    public int ReportId { get; set; }

    public int? GroupId { get; set; }

    public int? WeekId { get; set; }

    public string? Content { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    public DateTime? SubmittedAt { get; set; }

    [ForeignKey("GroupId")]
    [InverseProperty("WeeklyReports")]
    public virtual Group? Group { get; set; }

    [ForeignKey("WeekId")]
    [InverseProperty("WeeklyReports")]
    public virtual WeekDefinition? Week { get; set; }

    [InverseProperty("Report")]
    public virtual ICollection<WeeklyEvaluation> WeeklyEvaluations { get; set; } = new List<WeeklyEvaluation>();
}
