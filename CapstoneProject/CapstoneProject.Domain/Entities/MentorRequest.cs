using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Domain.Entities;

public partial class MentorRequest
{
    [Key]
    public int Id { get; set; }

    public int? GroupId { get; set; }

    public int? MentorId { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    public DateTime? RequestedAt { get; set; }

    public DateTime? RespondedAt { get; set; }

    [ForeignKey("GroupId")]
    [InverseProperty("MentorRequests")]
    public virtual Group? Group { get; set; }

    [ForeignKey("MentorId")]
    [InverseProperty("MentorRequests")]
    public virtual User? Mentor { get; set; }
}
