using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Domain.Entities;

public partial class RefreshToken
{
    [Key]
    public int TokenId { get; set; }

    public int UserId { get; set; }

    [StringLength(500)]
    public string Token { get; set; } = null!;

    public DateTime ExpiryDate { get; set; }

    public bool? IsRevoked { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("RefreshTokens")]
    public virtual User User { get; set; } = null!;
}
