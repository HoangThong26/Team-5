using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Domain.Entities;

public partial class TopicVersion
{
    [Key]
    public int Id { get; set; }

    public int? TopicId { get; set; }

    public int? VersionNumber { get; set; }

    [StringLength(500)]
    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public int? ReviewedBy { get; set; }

    public string? ReviewComment { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [ForeignKey("ReviewedBy")]
    [InverseProperty("TopicVersions")]
    public virtual User? ReviewedByNavigation { get; set; }

    [ForeignKey("TopicId")]
    [InverseProperty("TopicVersions")]
    public virtual Topic? Topic { get; set; }
}
