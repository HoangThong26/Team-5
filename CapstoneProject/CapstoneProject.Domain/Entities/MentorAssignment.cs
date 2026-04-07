using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Domain.Entities;

[Index("GroupId", Name = "UQ__MentorAs__149AF36B3C559856", IsUnique = true)]
public partial class MentorAssignment
{
    [Key]
    public int Id { get; set; }

    public int? GroupId { get; set; }

    public int? MentorId { get; set; }

    public DateTime? AssignedAt { get; set; }

    [ForeignKey("GroupId")]
    [InverseProperty("MentorAssignment")]
    public virtual Group? Group { get; set; }

    [ForeignKey("MentorId")]
    [InverseProperty("MentorAssignments")]
    public virtual User? Mentor { get; set; }
}
