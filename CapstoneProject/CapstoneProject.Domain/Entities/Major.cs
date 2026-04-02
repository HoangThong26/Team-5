using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Domain.Entities;

[Index("MajorCode", IsUnique = true)]
public partial class Major
{
    [Key]
    public int MajorId { get; set; }

    [Required]
    [StringLength(50)]
    public string MajorCode { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string MajorName { get; set; } = null!;

    [InverseProperty("Major")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();

    [InverseProperty("Major")]
    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();
}