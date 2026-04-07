using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Domain.Entities;

public partial class WeekDefinition
{
    [Key]
    public int WeekId { get; set; }

    public int? WeekNumber { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [InverseProperty("Week")]
    public virtual ICollection<WeeklyReport> WeeklyReports { get; set; } = new List<WeeklyReport>();
}
