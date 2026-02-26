using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Domain.Entities;

public partial class DefenseSchedule
{
    [Key]
    public int DefenseId { get; set; }

    public int? GroupId { get; set; }

    public int? CouncilId { get; set; }

    [StringLength(100)]
    public string? Room { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [ForeignKey("CouncilId")]
    [InverseProperty("DefenseSchedules")]
    public virtual Council? Council { get; set; }

    [InverseProperty("Defense")]
    public virtual ICollection<DefenseScore> DefenseScores { get; set; } = new List<DefenseScore>();

    [ForeignKey("GroupId")]
    [InverseProperty("DefenseSchedules")]
    public virtual Group? Group { get; set; }
}
