using System;
using System.Collections.Generic;

namespace UserService.Domain.Entities;

public partial class Follower
{
    public int FollowerId { get; set; }

    public int FollowedId { get; set; }

    public sbyte Status { get; set; }

    public virtual User Followed { get; set; } = null!;

    public virtual User FollowerNavigation { get; set; } = null!;
}
