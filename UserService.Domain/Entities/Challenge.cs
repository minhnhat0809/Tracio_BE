using System;
using System.Collections.Generic;

namespace UserService.Domain.Entities;

public partial class Challenge
{
    public int ChallengeId { get; set; }

    public int CreatorId { get; set; }

    public int? RewardId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string ChallengeType { get; set; } = null!;

    public float GoalValue { get; set; }

    public string? Unit { get; set; }

    public float? Mission { get; set; }

    public string? MissionType { get; set; }

    public bool? IsSystem { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsPublic { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<ChallengeParticipant> ChallengeParticipants { get; set; } = new List<ChallengeParticipant>();

    public virtual User Creator { get; set; } = null!;

    public virtual ChallengeReward? Reward { get; set; }
}
