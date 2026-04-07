using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Domain.Entities;

public partial class PasswordResetToken
{
    [Key]
    public int Id { get; set; }

    public int? UserId { get; set; }

    [StringLength(500)]
    public string? Token { get; set; }

    public DateTime? ExpiryTime { get; set; }

    public bool? IsUsed { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("PasswordResetTokens")]
    public virtual User? User { get; set; }
}
