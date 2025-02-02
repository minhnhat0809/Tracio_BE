using System;
using System.Collections.Generic;

namespace UserService.Domain.Entities;

public partial class GroupParticipant
{
    public int GroupId { get; set; }

    public int CyclistId { get; set; }

    public bool? IsCheckin { get; set; }

    public DateTime? CheckIn { get; set; }

    public bool? IsOrganizer { get; set; }

    public virtual User Cyclist { get; set; } = null!;

    public virtual Group Group { get; set; } = null!;
}
