using Microsoft.EntityFrameworkCore;
using RouteService.Domain.Entities;

namespace RouteService.Infrastructure.Contexts;

public partial class TracioRouteDbContext : DbContext
{
    public TracioRouteDbContext()
    {
    }

    public TracioRouteDbContext(DbContextOptions<TracioRouteDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Route> Routes { get; set; }

    public virtual DbSet<RouteBookmark> RouteBookmarks { get; set; }

    public virtual DbSet<RouteMediaFile> RouteMediaFiles { get; set; }

    public virtual DbSet<RouteReaction> RouteReactions { get; set; }

    public virtual DbSet<RouteReview> RouteReviews { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=tracio_route;user=root;password=Nhat2003.", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.39-mysql"), x => x.UseNetTopologySuite());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Route>(entity =>
        {
            entity.HasKey(e => e.RouteId).HasName("PRIMARY");

            entity
                .ToTable("route")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.EndLocation, "end_location")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 32 })
                .HasAnnotation("MySql:SpatialIndex", true);

            entity.HasIndex(e => e.RoutePath, "route_path")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 32 })
                .HasAnnotation("MySql:SpatialIndex", true);

            entity.HasIndex(e => e.StartLocation, "start_location")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 32 })
                .HasAnnotation("MySql:SpatialIndex", true);

            entity.Property(e => e.RouteId).HasColumnName("route_id");
            entity.Property(e => e.AvgSpeed).HasColumnName("avg_speed");
            entity.Property(e => e.Avoid)
                .HasAnnotation("MySql:SpatialReferenceSystemId", 4326)
                .HasColumnName("avoid");
            entity.Property(e => e.AvoidsRoads)
                .HasColumnType("json")
                .HasColumnName("avoids_roads");
            entity.Property(e => e.Calories).HasColumnName("calories");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CyclistAvatar)
                .HasMaxLength(2083)
                .HasColumnName("cyclist_avatar");
            entity.Property(e => e.CyclistId).HasColumnName("cyclist_id");
            entity.Property(e => e.CyclistName)
                .HasMaxLength(255)
                .HasColumnName("cyclist_name");
            entity.Property(e => e.Difficulty).HasColumnName("difficulty");
            entity.Property(e => e.ElevationGain).HasColumnName("elevation_gain");
            entity.Property(e => e.EndLocation)
                .HasAnnotation("MySql:SpatialReferenceSystemId", 4326)
                .HasColumnName("end_location");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_deleted");
            entity.Property(e => e.IsGroup)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_group");
            entity.Property(e => e.IsPublic)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_public");
            entity.Property(e => e.Mood).HasColumnName("mood");
            entity.Property(e => e.MovingTime).HasColumnName("moving_time");
            entity.Property(e => e.OptimizeRoute).HasColumnName("optimize_route");
            entity.Property(e => e.ReactionCounts)
                .HasDefaultValueSql("'0'")
                .HasColumnName("reaction_counts");
            entity.Property(e => e.RouteName)
                .HasMaxLength(255)
                .HasColumnName("route_name");
            entity.Property(e => e.RoutePath)
                .HasAnnotation("MySql:SpatialReferenceSystemId", 4326)
                .HasColumnName("route_path");
            entity.Property(e => e.StartLocation)
                .HasAnnotation("MySql:SpatialReferenceSystemId", 4326)
                .HasColumnName("start_location");
            entity.Property(e => e.TotalDistance).HasColumnName("total_distance");
            entity.Property(e => e.Waypoints)
                .HasAnnotation("MySql:SpatialReferenceSystemId", 4326)
                .HasColumnName("waypoints");
            entity.Property(e => e.Weighting).HasColumnName("weighting");
        });

        modelBuilder.Entity<RouteBookmark>(entity =>
        {
            entity.HasKey(e => e.BookmarkId).HasName("PRIMARY");

            entity
                .ToTable("route_bookmark")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.RouteId, "route_id");

            entity.Property(e => e.BookmarkId).HasColumnName("bookmark_id");
            entity.Property(e => e.CollectionName)
                .HasMaxLength(255)
                .HasColumnName("collection_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.RouteId).HasColumnName("route_id");

            entity.HasOne(d => d.Route).WithMany(p => p.RouteBookmarks)
                .HasForeignKey(d => d.RouteId)
                .HasConstraintName("route_bookmark_ibfk_1");
        });

        modelBuilder.Entity<RouteMediaFile>(entity =>
        {
            entity.HasKey(e => e.MediaId).HasName("PRIMARY");

            entity
                .ToTable("route_media_file")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Location, "location")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 32 })
                .HasAnnotation("MySql:SpatialIndex", true);

            entity.HasIndex(e => e.RouteId, "route_id");

            entity.Property(e => e.MediaId).HasColumnName("media_id");
            entity.Property(e => e.Location)
                .HasAnnotation("MySql:SpatialReferenceSystemId", 4326)
                .HasColumnName("location");
            entity.Property(e => e.MediaUrl)
                .HasMaxLength(2083)
                .HasColumnName("media_url");
            entity.Property(e => e.RouteId).HasColumnName("route_id");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("uploaded_at");

            entity.HasOne(d => d.Route).WithMany(p => p.RouteMediaFiles)
                .HasForeignKey(d => d.RouteId)
                .HasConstraintName("route_media_file_ibfk_1");
        });

        modelBuilder.Entity<RouteReaction>(entity =>
        {
            entity.HasKey(e => e.ReactionId).HasName("PRIMARY");

            entity
                .ToTable("route_reaction")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.RouteId, "route_id");

            entity.Property(e => e.ReactionId).HasColumnName("reaction_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CyclistAvatar)
                .HasMaxLength(2083)
                .HasColumnName("cyclist_avatar");
            entity.Property(e => e.CyclistId).HasColumnName("cyclist_id");
            entity.Property(e => e.CyclistName)
                .HasMaxLength(255)
                .HasColumnName("cyclist_name");
            entity.Property(e => e.RouteId).HasColumnName("route_id");

            entity.HasOne(d => d.Route).WithMany(p => p.RouteReactions)
                .HasForeignKey(d => d.RouteId)
                .HasConstraintName("route_reaction_ibfk_1");
        });

        modelBuilder.Entity<RouteReview>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PRIMARY");

            entity
                .ToTable("route_review")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.RouteId, "route_id");

            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CyclistAvatar)
                .HasMaxLength(2083)
                .HasColumnName("cyclist_avatar");
            entity.Property(e => e.CyclistId).HasColumnName("cyclist_id");
            entity.Property(e => e.CyclistName)
                .HasMaxLength(255)
                .HasColumnName("cyclist_name");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.ReviewContent)
                .HasMaxLength(1000)
                .HasColumnName("review_content");
            entity.Property(e => e.RouteId).HasColumnName("route_id");

            entity.HasOne(d => d.Route).WithMany(p => p.RouteReviews)
                .HasForeignKey(d => d.RouteId)
                .HasConstraintName("route_review_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
