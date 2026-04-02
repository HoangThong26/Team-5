using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Domain.Entities;

[Index("SemesterCode", IsUnique = true)]
public partial class Semester
{
    [Key]
    public int SemesterId { get; set; }

    [Required]
    [StringLength(50)]
    public string SemesterCode { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string SemesterName { get; set; } = null!;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsCurrent { get; set; } = false;

    [InverseProperty("Semester")]
    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    [InverseProperty("Semester")]
    public virtual ICollection<WeekDefinition> WeekDefinitions { get; set; } = new List<WeekDefinition>();
}