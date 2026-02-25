using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Domain.Entities;

[Index("UserId", Name = "UQ_User_OneGroup", IsUnique = true)]
public partial class GroupMember
{
    [Key]
    public int Id { get; set; }

    public int GroupId { get; set; }

    public int UserId { get; set; }

    [StringLength(50)]
    public string? RoleInGroup { get; set; }

    public DateTime? JoinedAt { get; set; }

    [ForeignKey("GroupId")]
    [InverseProperty("GroupMembers")]
    public virtual Group Group { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("GroupMember")]
    public virtual User User { get; set; } = null!;
}
