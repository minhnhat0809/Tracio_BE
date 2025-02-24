using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Contexts;

public partial class TracioUserDbContext : DbContext
{
    public TracioUserDbContext()
    {
        
    }

    public TracioUserDbContext(DbContextOptions<TracioUserDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Challenge> Challenges { get; set; }

    public virtual DbSet<ChallengeParticipant> ChallengeParticipants { get; set; }

    public virtual DbSet<ChallengeReward> ChallengeRewards { get; set; }

    public virtual DbSet<Follower> Followers { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<GroupInvitation> GroupInvitations { get; set; }

    public virtual DbSet<GroupParticipant> GroupParticipants { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserSession> UserSessions { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql("server=localhost;database=tracio_activity;user=root;password=N@hat892003.",
            new MySqlServerVersion(new Version(9, 0, 0)),
            mySqlOptions => mySqlOptions.UseNetTopologySuite()
        );
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Challenge>(entity =>
        {
            entity.HasKey(e => e.ChallengeId).HasName("PRIMARY");

            entity.ToTable("challenge");

            entity.HasIndex(e => e.CreatorId, "fk_challenge_creator");

            entity.HasIndex(e => e.RewardId, "fk_challenge_reward");

            entity.Property(e => e.ChallengeId).HasColumnName("challenge_id");
            entity.Property(e => e.ChallengeType).HasColumnName("challenge_type");
            entity.Property(e => e.CreatorId).HasColumnName("creator_id");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("end_date");
            entity.Property(e => e.GoalValue).HasColumnName("goal_value");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_deleted");
            entity.Property(e => e.IsPublic)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_public");
            entity.Property(e => e.IsSystem)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_system");
            entity.Property(e => e.Mission).HasColumnName("mission");
            entity.Property(e => e.MissionType).HasColumnName("mission_type");
            entity.Property(e => e.RewardId).HasColumnName("reward_id");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("start_date");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.Unit).HasColumnName("unit");

            entity.HasOne(d => d.Creator).WithMany(p => p.Challenges)
                .HasForeignKey(d => d.CreatorId)
                .HasConstraintName("fk_challenge_creator");

            entity.HasOne(d => d.Reward).WithMany(p => p.Challenges)
                .HasForeignKey(d => d.RewardId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_challenge_reward");
        });

        modelBuilder.Entity<ChallengeParticipant>(entity =>
        {
            entity.HasKey(e => new { e.ChallengeId, e.CyclistId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("challenge_participant");

            entity.HasIndex(e => e.CyclistId, "fk_challenge_participant_cyclist");

            entity.Property(e => e.ChallengeId).HasColumnName("challenge_id");
            entity.Property(e => e.CyclistId).HasColumnName("cyclist_id");
            entity.Property(e => e.Avatar)
                .HasMaxLength(255)
                .HasColumnName("avatar");
            entity.Property(e => e.CyclistName)
                .HasMaxLength(255)
                .HasColumnName("cyclist_name")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.IsCompleted)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_completed");
            entity.Property(e => e.JoinedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("joined_at");
            entity.Property(e => e.Pace)
                .HasDefaultValueSql("'0'")
                .HasColumnName("pace");
            entity.Property(e => e.Progress)
                .HasDefaultValueSql("'0'")
                .HasColumnName("progress");

            entity.HasOne(d => d.Challenge).WithMany(p => p.ChallengeParticipants)
                .HasForeignKey(d => d.ChallengeId)
                .HasConstraintName("fk_challenge_participant_challenge");

            entity.HasOne(d => d.Cyclist).WithMany(p => p.ChallengeParticipants)
                .HasForeignKey(d => d.CyclistId)
                .HasConstraintName("fk_challenge_participant_cyclist");
        });

        modelBuilder.Entity<ChallengeReward>(entity =>
        {
            entity.HasKey(e => e.RewardId).HasName("PRIMARY");

            entity.ToTable("challenge_reward");

            entity.Property(e => e.RewardId).HasColumnName("reward_id");
            entity.Property(e => e.RewardIcon)
                .HasMaxLength(2083)
                .HasColumnName("reward_icon");
            entity.Property(e => e.RewardName)
                .HasMaxLength(255)
                .HasColumnName("reward_name")
                .UseCollation("utf8mb4_unicode_ci");
        });

        modelBuilder.Entity<Follower>(entity =>
        {
            entity.HasKey(e => new { e.FollowerId, e.FollowedId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("follower");

            entity.HasIndex(e => e.FollowedId, "fk_follower_followed");

            entity.Property(e => e.FollowerId).HasColumnName("follower_id");
            entity.Property(e => e.FollowedId).HasColumnName("followed_id");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Followed).WithMany(p => p.FollowerFolloweds)
                .HasForeignKey(d => d.FollowedId)
                .HasConstraintName("fk_follower_followed");

            entity.HasOne(d => d.FollowerNavigation).WithMany(p => p.FollowerFollowerNavigations)
                .HasForeignKey(d => d.FollowerId)
                .HasConstraintName("fk_follower_follower");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PRIMARY");

            entity.ToTable("group");

            entity.HasIndex(e => e.CreatorId, "fk_group_creator");

            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatorId).HasColumnName("creator_id");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.IsPublic)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_public");
            entity.Property(e => e.MaxParticipants)
                .HasDefaultValueSql("'0'")
                .HasColumnName("max_participants");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.Password)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("password");
            entity.Property(e => e.RouteId).HasColumnName("route_id");
            entity.Property(e => e.StartLocation).HasColumnName("start_location");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("start_time");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Creator).WithMany(p => p.Groups)
                .HasForeignKey(d => d.CreatorId)
                .HasConstraintName("fk_group_creator");
        });

        modelBuilder.Entity<GroupInvitation>(entity =>
        {
            entity.HasKey(e => e.InvitationId).HasName("PRIMARY");

            entity.ToTable("group_invitation");

            entity.HasIndex(e => e.GroupId, "fk_group_invitation_group");

            entity.HasIndex(e => e.InviteeId, "fk_group_invitation_invitee");

            entity.HasIndex(e => e.InviterId, "fk_group_invitation_inviter");

            entity.Property(e => e.InvitationId).HasColumnName("invitation_id");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.InvitedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("invited_at");
            entity.Property(e => e.InviteeId).HasColumnName("invitee_id");
            entity.Property(e => e.InviterId).HasColumnName("inviter_id");
            entity.Property(e => e.RespondedAt)
                .HasColumnType("datetime")
                .HasColumnName("responded_at");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Group).WithMany(p => p.GroupInvitations)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("fk_group_invitation_group");

            entity.HasOne(d => d.Invitee).WithMany(p => p.GroupInvitationInvitees)
                .HasForeignKey(d => d.InviteeId)
                .HasConstraintName("fk_group_invitation_invitee");

            entity.HasOne(d => d.Inviter).WithMany(p => p.GroupInvitationInviters)
                .HasForeignKey(d => d.InviterId)
                .HasConstraintName("fk_group_invitation_inviter");
        });

        modelBuilder.Entity<GroupParticipant>(entity =>
        {
            entity.HasKey(e => new { e.GroupId, e.CyclistId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("group_participant");

            entity.HasIndex(e => e.CyclistId, "fk_group_participant_cyclist");

            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.CyclistId).HasColumnName("cyclist_id");
            entity.Property(e => e.CheckIn)
                .HasColumnType("datetime")
                .HasColumnName("check_in");
            entity.Property(e => e.IsCheckin)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_checkin");
            entity.Property(e => e.IsOrganizer)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_organizer");

            entity.HasOne(d => d.Cyclist).WithMany(p => p.GroupParticipants)
                .HasForeignKey(d => d.CyclistId)
                .HasConstraintName("fk_group_participant_cyclist");

            entity.HasOne(d => d.Group).WithMany(p => p.GroupParticipants)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("fk_group_participant_group");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity
                .ToTable("user")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.HasIndex(e => e.FirebaseId, "firebase_id").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Bio)
                .HasMaxLength(1000)
                .HasColumnName("bio");
            entity.Property(e => e.City)
                .HasMaxLength(255)
                .HasColumnName("city");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.District)
                .HasMaxLength(255)
                .HasColumnName("district");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.FirebaseId).HasColumnName("firebase_id");
            entity.Property(e => e.Followers)
                .HasDefaultValueSql("'0'")
                .HasColumnName("followers");
            entity.Property(e => e.Followings)
                .HasDefaultValueSql("'0'")
                .HasColumnName("followings");
            entity.Property(e => e.Gender).HasColumnName("gender");
            entity.Property(e => e.Height)
                .HasDefaultValueSql("'0'")
                .HasColumnName("height");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.Level)
                .HasDefaultValueSql("'0'")
                .HasColumnName("level");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .HasColumnName("phone_number");
            entity.Property(e => e.ProfilePicture)
                .HasMaxLength(2083)
                .HasColumnName("profile_picture");
            entity.Property(e => e.Role)
                .HasMaxLength(8)
                .IsFixedLength()
                .HasColumnName("role");
            entity.Property(e => e.TotalDistance)
                .HasDefaultValueSql("'0'")
                .HasColumnName("total_distance");
            entity.Property(e => e.TotalPost)
                .HasDefaultValueSql("'0'")
                .HasColumnName("total_post");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserName)
                .HasMaxLength(255)
                .HasColumnName("user_name");
            entity.Property(e => e.Weight)
                .HasDefaultValueSql("'0'")
                .HasColumnName("weight");

            entity.HasMany(d => d.Rewards).WithMany(p => p.Cyclists)
                .UsingEntity<Dictionary<string, object>>(
                    "CyclistReward",
                    r => r.HasOne<ChallengeReward>().WithMany()
                        .HasForeignKey("RewardId")
                        .HasConstraintName("fk_cyclist_reward_reward"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("CyclistId")
                        .HasConstraintName("fk_cyclist_reward_cyclist"),
                    j =>
                    {
                        j.HasKey("CyclistId", "RewardId")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j.ToTable("cyclist_reward");
                        j.HasIndex(new[] { "RewardId" }, "fk_cyclist_reward_reward");
                        j.IndexerProperty<int>("CyclistId").HasColumnName("cyclist_id");
                        j.IndexerProperty<int>("RewardId").HasColumnName("reward_id");
                    });
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PRIMARY");

            entity.ToTable("user_sessions");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.AccessToken)
                .HasColumnType("text")
                .HasColumnName("access_token");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("expires_at");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(50)
                .HasColumnName("ip_address");
            entity.Property(e => e.RefreshToken)
                .HasColumnType("text")
                .HasColumnName("refresh_token");
            entity.Property(e => e.UserAgent)
                .HasColumnType("text")
                .HasColumnName("user_agent");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserSessions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_sessions_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
