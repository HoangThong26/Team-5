using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Domain.Entities;

[Index("GroupId", Name = "UQ__Topics__149AF36BBD8FDC91", IsUnique = true)]
public partial class Topic
{
    [Key]
    public int TopicId { get; set; }

    public int? GroupId { get; set; }

    [StringLength(500)]
    public string? Title { get; set; }

    [NotMapped]
    public string? TopicName => Title;

    public string? Description { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    public int? CurrentVersion { get; set; }

    public DateTime? CreatedAt { get; set; }

    [ForeignKey("GroupId")]
    [InverseProperty("Topic")]
    public virtual Group? Group { get; set; }

    [InverseProperty("Topic")]
    public virtual ICollection<TopicVersion> TopicVersions { get; set; } = new List<TopicVersion>();
}
