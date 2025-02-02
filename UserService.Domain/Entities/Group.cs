using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace UserService.Domain.Entities;

public partial class Group
{
    public int GroupId { get; set; }

    public int CreatorId { get; set; }

    public int? RouteId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? StartTime { get; set; }

    public Point? StartLocation { get; set; }

    public bool? IsPublic { get; set; }

    public byte[]? Password { get; set; }

    public byte? MaxParticipants { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User Creator { get; set; } = null!;

    public virtual ICollection<GroupInvitation> GroupInvitations { get; set; } = new List<GroupInvitation>();

    public virtual ICollection<GroupParticipant> GroupParticipants { get; set; } = new List<GroupParticipant>();
}
