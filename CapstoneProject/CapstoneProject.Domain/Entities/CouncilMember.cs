using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Domain.Entities;

public partial class CouncilMember
{
    [Key]
    public int Id { get; set; }

    public int? CouncilId { get; set; }

    public int? UserId { get; set; }

    [ForeignKey("CouncilId")]
    [InverseProperty("CouncilMembers")]
    public virtual Council? Council { get; set; }

    [InverseProperty("CouncilMember")]
    public virtual ICollection<DefenseScore> DefenseScores { get; set; } = new List<DefenseScore>();

    [ForeignKey("UserId")]
    [InverseProperty("CouncilMembers")]
    public virtual User? User { get; set; }
}
