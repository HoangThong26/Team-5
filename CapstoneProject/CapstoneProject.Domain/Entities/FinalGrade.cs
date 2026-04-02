using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Domain.Entities;

[Index("GroupId", Name = "UQ__FinalGra__149AF36B5AEB251B", IsUnique = true)]
public partial class FinalGrade
{
    [Key]
    public int Id { get; set; }
    // Thêm dòng này vào class FinalGrade
    public bool? IsPass { get; set; }
    public int? GroupId { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? AverageScore { get; set; }

    [StringLength(5)]
    public string? GradeLetter { get; set; }

    public bool? IsPublished { get; set; }

    public DateTime? PublishedAt { get; set; }

    [ForeignKey("GroupId")]
    [InverseProperty("FinalGrade")]
    public virtual Group? Group { get; set; }
}
