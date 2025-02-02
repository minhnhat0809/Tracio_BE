using System;
using System.Collections.Generic;

namespace UserService.Domain.Entities;

public partial class GroupInvitation
{
    public int InvitationId { get; set; }

    public int GroupId { get; set; }

    public int InviterId { get; set; }

    public int InviteeId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime InvitedAt { get; set; }

    public DateTime? RespondedAt { get; set; }

    public virtual Group Group { get; set; } = null!;

    public virtual User Invitee { get; set; } = null!;

    public virtual User Inviter { get; set; } = null!;
}
