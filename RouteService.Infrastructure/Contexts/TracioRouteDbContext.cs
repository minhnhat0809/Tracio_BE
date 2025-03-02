using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using RouteService.Domain.Entities;
using RouteService.Infrastructure;

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

    public virtual DbSet<Reaction> Reactions { get; set; }

    public virtual DbSet<Route> Routes { get; set; }

    public virtual DbSet<RouteBookmark> RouteBookmarks { get; set; }

    public virtual DbSet<RouteComment> RouteComments { get; set; }

    public virtual DbSet<RouteMediaFile> RouteMediaFiles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=tracio_route;user=root;password=Nhat2003.", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.39-mysql"), x => x.UseNetTopologySuite());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Reaction>(entity =>
        {
            entity.HasKey(e => e.ReactionId).HasName("PRIMARY");

            entity
                .ToTable("reaction")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => new { e.CyclistId, e.TargetId, e.TargetType }, "cyclist_id").IsUnique();

            entity.HasIndex(e => e.CyclistId, "cyclist_id_2");

            entity.HasIndex(e => new { e.TargetId, e.TargetType }, "target_id");

            entity.Property(e => e.ReactionId).HasColumnName("reaction_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CyclistId).HasColumnName("cyclist_id");
            entity.Property(e => e.TargetId).HasColumnName("target_id");
            entity.Property(e => e.TargetType)
                .HasColumnType("enum('route','comment','reply')")
                .HasColumnName("target_type");
        });

        modelBuilder.Entity<Route>(entity =>
        {
            entity.HasKey(e => e.RouteId).HasName("PRIMARY");

            entity
                .ToTable("route")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.City, "city");

            entity.HasIndex(e => e.CyclistId, "cyclist_id");

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
            entity.Property(e => e.AvoidsRoads)
                .HasColumnType("text")
                .HasColumnName("avoids_roads");
            entity.Property(e => e.Calories)
                .HasPrecision(10, 2)
                .HasColumnName("calories");
            entity.Property(e => e.City).HasColumnName("city");
            entity.Property(e => e.CommentCounts).HasColumnName("comment_counts");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CyclistId).HasColumnName("cyclist_id");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Difficulty).HasColumnName("difficulty");
            entity.Property(e => e.DurationTime).HasColumnName("duration_time");
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
            entity.Property(e => e.MaxSpeed).HasColumnName("max_speed");
            entity.Property(e => e.Mood).HasColumnName("mood");
            entity.Property(e => e.MovingTime).HasColumnName("moving_time");
            entity.Property(e => e.OptimizeRoute).HasColumnName("optimize_route");
            entity.Property(e => e.PolylineOverview)
                .HasColumnType("text")
                .HasColumnName("polyline_overview");
            entity.Property(e => e.PrivacyLevel)
                .HasDefaultValueSql("'2'")
                .HasColumnName("privacy_level");
            entity.Property(e => e.ReactionCounts).HasColumnName("reaction_counts");
            entity.Property(e => e.RouteName)
                .HasMaxLength(255)
                .HasColumnName("route_name");
            entity.Property(e => e.RoutePath)
                .HasAnnotation("MySql:SpatialReferenceSystemId", 4326)
                .HasColumnName("route_path");
            entity.Property(e => e.StartLocation)
                .HasAnnotation("MySql:SpatialReferenceSystemId", 4326)
                .HasColumnName("start_location");
            entity.Property(e => e.StoppedTime).HasColumnName("stopped_time");
            entity.Property(e => e.TotalDistance).HasColumnName("total_distance");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.Waypoints)
                .HasAnnotation("MySql:SpatialReferenceSystemId", 4326)
                .HasColumnName("waypoints");
        });

        modelBuilder.Entity<RouteBookmark>(entity =>
        {
            entity.HasKey(e => e.BookmarkId).HasName("PRIMARY");

            entity
                .ToTable("route_bookmark")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => new { e.CyclistId, e.RouteId }, "cyclist_id").IsUnique();

            entity.HasIndex(e => e.CyclistId, "cyclist_id_2");

            entity.HasIndex(e => e.RouteId, "route_id");

            entity.Property(e => e.BookmarkId).HasColumnName("bookmark_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CyclistId).HasColumnName("cyclist_id");
            entity.Property(e => e.RouteId).HasColumnName("route_id");
        });

        modelBuilder.Entity<RouteComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PRIMARY");

            entity
                .ToTable("route_comment")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.CyclistId, "cyclist_id");

            entity.HasIndex(e => e.MentionCyclistId, "mention_cyclist_id");

            entity.HasIndex(e => e.ParentCommentId, "parent_comment_id");

            entity.HasIndex(e => e.RouteId, "route_id");

            entity.Property(e => e.CommentId).HasColumnName("comment_id");
            entity.Property(e => e.CommentContent)
                .HasMaxLength(1000)
                .HasColumnName("comment_content");
            entity.Property(e => e.CommentCounts).HasColumnName("comment_counts");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CyclistId).HasColumnName("cyclist_id");
            entity.Property(e => e.MentionCyclistId).HasColumnName("mention_cyclist_id");
            entity.Property(e => e.ParentCommentId).HasColumnName("parent_comment_id");
            entity.Property(e => e.ReactionCounts).HasColumnName("reaction_counts");
            entity.Property(e => e.RouteId).HasColumnName("route_id");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Route).WithMany(p => p.RouteComments)
                .HasForeignKey(d => d.RouteId)
                .HasConstraintName("route_comment_ibfk_1");
        });

        modelBuilder.Entity<RouteMediaFile>(entity =>
        {
            entity.HasKey(e => e.MediaId).HasName("PRIMARY");

            entity
                .ToTable("route_media_file")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.CyclistId, "cyclist_id");

            entity.HasIndex(e => e.Location, "location")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 32 })
                .HasAnnotation("MySql:SpatialIndex", true);

            entity.HasIndex(e => e.RouteId, "route_id");

            entity.Property(e => e.MediaId).HasColumnName("media_id");
            entity.Property(e => e.CapturedAt)
                .HasColumnType("datetime")
                .HasColumnName("captured_at");
            entity.Property(e => e.CyclistId).HasColumnName("cyclist_id");
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
