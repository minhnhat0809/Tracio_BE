using System;
using System.Collections.Generic;

namespace UserService.Domain.Entities;

public partial class ChallengeReward
{
    public int RewardId { get; set; }

    public string RewardName { get; set; } = null!;

    public string RewardIcon { get; set; } = null!;

    public virtual ICollection<Challenge> Challenges { get; set; } = new List<Challenge>();

    public virtual ICollection<User> Cyclists { get; set; } = new List<User>();
}
