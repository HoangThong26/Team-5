using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Domain.Entities;

public partial class LoginHistory
{
    [Key]
    public int Id { get; set; }

    public int? UserId { get; set; }

    public DateTime? LoginTime { get; set; }

    [Column("IPAddress")]
    [StringLength(50)]
    public string? Ipaddress { get; set; }

    public bool? IsSuccess { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("LoginHistories")]
    public virtual User? User { get; set; }
}
