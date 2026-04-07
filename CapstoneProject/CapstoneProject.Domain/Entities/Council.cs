using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Domain.Entities;

public partial class Council
{
    [Key]
    public int CouncilId { get; set; }

    [StringLength(255)]
    public string? Name { get; set; }

    [InverseProperty("Council")]
    public virtual ICollection<CouncilMember> CouncilMembers { get; set; } = new List<CouncilMember>();

    [InverseProperty("Council")]
    public virtual ICollection<DefenseSchedule> DefenseSchedules { get; set; } = new List<DefenseSchedule>();
}
