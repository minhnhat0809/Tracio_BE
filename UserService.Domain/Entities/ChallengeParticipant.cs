using System;
using System.Collections.Generic;

namespace UserService.Domain.Entities;

public partial class ChallengeParticipant
{
    public int ChallengeId { get; set; }

    public int CyclistId { get; set; }

    public string? CyclistName { get; set; }

    public string? Avatar { get; set; }

    public float? Progress { get; set; }

    public float? Pace { get; set; }

    public bool? IsCompleted { get; set; }

    public DateTime JoinedAt { get; set; }

    public virtual Challenge Challenge { get; set; } = null!;

    public virtual User Cyclist { get; set; } = null!;
}
