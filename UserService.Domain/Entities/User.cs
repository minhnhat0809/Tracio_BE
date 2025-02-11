using System;
using System.Collections.Generic;

namespace UserService.Domain.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string? UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string FirebaseId { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? ProfilePicture { get; set; }

    public string? Bio { get; set; }

    public float? TotalDistance { get; set; }

    public int? TotalPost { get; set; }

    public int? Followers { get; set; }

    public int? Followings { get; set; }

    public int? Level { get; set; }

    public bool? IsActive { get; set; }

    public byte[] Role { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public float? Weight { get; set; }

    public float? Height { get; set; }

    public sbyte Gender { get; set; }

    public string? City { get; set; }

    public string? District { get; set; }

    public virtual ICollection<ChallengeParticipant> ChallengeParticipants { get; set; } = new List<ChallengeParticipant>();

    public virtual ICollection<Challenge> Challenges { get; set; } = new List<Challenge>();

    public virtual ICollection<Follower> FollowerFolloweds { get; set; } = new List<Follower>();

    public virtual ICollection<Follower> FollowerFollowerNavigations { get; set; } = new List<Follower>();

    public virtual ICollection<GroupInvitation> GroupInvitationInvitees { get; set; } = new List<GroupInvitation>();

    public virtual ICollection<GroupInvitation> GroupInvitationInviters { get; set; } = new List<GroupInvitation>();

    public virtual ICollection<GroupParticipant> GroupParticipants { get; set; } = new List<GroupParticipant>();

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();

    public virtual ICollection<ChallengeReward> Rewards { get; set; } = new List<ChallengeReward>();
}
