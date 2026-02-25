using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Domain.Entities;

public partial class DefenseScore
{
    [Key]
    public int Id { get; set; }

    public int? DefenseId { get; set; }

    public int? CouncilMemberId { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? Score { get; set; }

    public string? Comment { get; set; }

    public bool? IsPublished { get; set; }

    [ForeignKey("CouncilMemberId")]
    [InverseProperty("DefenseScores")]
    public virtual CouncilMember? CouncilMember { get; set; }

    [ForeignKey("DefenseId")]
    [InverseProperty("DefenseScores")]
    public virtual DefenseSchedule? Defense { get; set; }
}
