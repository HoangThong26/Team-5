using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Domain.Entities;

public partial class GroupInvitation
{
    [Key]
    public int InvitationId { get; set; }

    public int? GroupId { get; set; }

    public int? SenderId { get; set; }

    public int? ReceiverId { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    [ForeignKey("GroupId")]
    [InverseProperty("GroupInvitations")]
    public virtual Group? Group { get; set; }

    [ForeignKey("ReceiverId")]
    [InverseProperty("GroupInvitationReceivers")]
    public virtual User? Receiver { get; set; }

    [ForeignKey("SenderId")]
    [InverseProperty("GroupInvitationSenders")]
    public virtual User? Sender { get; set; }
}
