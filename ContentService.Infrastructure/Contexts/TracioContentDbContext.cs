using ContentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContentService.Infrastructure.Contexts;

public partial class TracioContentDbContext : DbContext
{
    public TracioContentDbContext()
    {
    }

    public TracioContentDbContext(DbContextOptions<TracioContentDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<BlogBookmark> BlogBookmarks { get; set; }

    public virtual DbSet<BlogCategory> BlogCategories { get; set; }

    public virtual DbSet<BlogPrivacy> BlogPrivacies { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<MediaFile> MediaFiles { get; set; }

    public virtual DbSet<Reaction> Reactions { get; set; }

    public virtual DbSet<Reply> Replies { get; set; }

    public virtual DbSet<UserBlogFollowerOnly> UserBlogFollowerOnlies { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.BlogId).HasName("PRIMARY");

            entity.ToTable("blog");

            entity.HasIndex(e => e.CategoryId, "fk_blog_category");

            entity.Property(e => e.BlogId).HasColumnName("blog_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CommentsCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("comments_count");
            entity.Property(e => e.Content)
                .HasMaxLength(1000)
                .HasColumnName("content")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatorAvatar)
                .HasMaxLength(2083)
                .HasColumnName("creator_avatar");
            entity.Property(e => e.CreatorId).HasColumnName("creator_id");
            entity.Property(e => e.CreatorName)
                .HasMaxLength(255)
                .HasColumnName("creator_name")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.PrivacySetting).HasColumnName("privacy_setting");
            entity.Property(e => e.ReactionsCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("reactions_count");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Category).WithMany(p => p.Blogs)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_blog_category");
        });

        modelBuilder.Entity<BlogBookmark>(entity =>
        {
            entity.HasKey(e => e.BookmarkId).HasName("PRIMARY");

            entity
                .ToTable("blog_bookmark")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.BlogId, "fk_blog_bookmark_blog");

            entity.Property(e => e.BookmarkId).HasColumnName("bookmark_id");
            entity.Property(e => e.BlogId).HasColumnName("blog_id");
            entity.Property(e => e.CollectionName)
                .HasMaxLength(255)
                .HasColumnName("collection_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");

            entity.HasOne(d => d.Blog).WithMany(p => p.BlogBookmarks)
                .HasForeignKey(d => d.BlogId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_blog_bookmark_blog");
        });

        modelBuilder.Entity<BlogCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PRIMARY");

            entity.ToTable("blog_category");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(255)
                .HasColumnName("category_name")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<BlogPrivacy>(entity =>
        {
            entity.HasKey(e => new { e.BlogId, e.CyclistId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("blog_privacy");

            entity.Property(e => e.BlogId).HasColumnName("blog_id");
            entity.Property(e => e.CyclistId).HasColumnName("cyclist_id");
            entity.Property(e => e.CyclistName)
                .HasMaxLength(255)
                .HasColumnName("cyclist_name")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasOne(d => d.Blog).WithMany(p => p.BlogPrivacies)
                .HasForeignKey(d => d.BlogId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_blog_privacy_blog");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PRIMARY");

            entity.ToTable("comment");

            entity.HasIndex(e => e.BlogId, "fk_comment_blog");

            entity.Property(e => e.CommentId).HasColumnName("comment_id");
            entity.Property(e => e.BlogId).HasColumnName("blog_id");
            entity.Property(e => e.Content)
                .HasMaxLength(1000)
                .HasColumnName("content")
                .UseCollation("utf8mb4_unicode_ci");
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
                .HasColumnName("cyclist_name")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.DeletedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_deleted");
            entity.Property(e => e.IsEdited)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_edited");
            entity.Property(e => e.LikesCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("likes_count");
            entity.Property(e => e.RepliesCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("replies_count");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Blog).WithMany(p => p.Comments)
                .HasForeignKey(d => d.BlogId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_comment_blog");
        });

        modelBuilder.Entity<MediaFile>(entity =>
        {
            entity.HasKey(e => e.MediaId).HasName("PRIMARY");

            entity.ToTable("media_files");

            entity.HasIndex(e => e.BlogId, "fk_media_blog");

            entity.HasIndex(e => e.CommentId, "fk_media_comment");

            entity.HasIndex(e => e.ReplyId, "fk_media_reply");

            entity.Property(e => e.MediaId).HasColumnName("media_id");
            entity.Property(e => e.BlogId).HasColumnName("blog_id");
            entity.Property(e => e.CommentId).HasColumnName("comment_id");
            entity.Property(e => e.MediaUrl)
                .HasMaxLength(2083)
                .HasColumnName("media_url");
            entity.Property(e => e.ReplyId).HasColumnName("reply_id");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("uploaded_at");

            entity.HasOne(d => d.Blog).WithMany(p => p.MediaFiles)
                .HasForeignKey(d => d.BlogId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_media_blog");

            entity.HasOne(d => d.Comment).WithMany(p => p.MediaFiles)
                .HasForeignKey(d => d.CommentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_media_comment");

            entity.HasOne(d => d.Reply).WithMany(p => p.MediaFiles)
                .HasForeignKey(d => d.ReplyId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_media_reply");
        });

        modelBuilder.Entity<Reaction>(entity =>
        {
            entity.HasKey(e => e.ReactionId).HasName("PRIMARY");

            entity.ToTable("reaction");

            entity.HasIndex(e => e.BlogId, "fk_reaction_blog");

            entity.HasIndex(e => e.CommentId, "fk_reaction_comment");

            entity.HasIndex(e => e.ReplyId, "fk_reaction_reply");

            entity.Property(e => e.ReactionId).HasColumnName("reaction_id");
            entity.Property(e => e.BlogId).HasColumnName("blog_id");
            entity.Property(e => e.CommentId).HasColumnName("comment_id");
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
                .HasColumnName("cyclist_name")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.ReplyId).HasColumnName("reply_id");

            entity.HasOne(d => d.Blog).WithMany(p => p.Reactions)
                .HasForeignKey(d => d.BlogId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_reaction_blog");

            entity.HasOne(d => d.Comment).WithMany(p => p.Reactions)
                .HasForeignKey(d => d.CommentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_reaction_comment");

            entity.HasOne(d => d.Reply).WithMany(p => p.Reactions)
                .HasForeignKey(d => d.ReplyId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_reaction_reply");
        });

        modelBuilder.Entity<Reply>(entity =>
        {
            entity.HasKey(e => e.ReplyId).HasName("PRIMARY");

            entity.ToTable("reply");

            entity.HasIndex(e => e.CommentId, "fk_reply_comment");

            entity.Property(e => e.ReplyId).HasColumnName("reply_id");
            entity.Property(e => e.CommentId).HasColumnName("comment_id");
            entity.Property(e => e.Content)
                .HasMaxLength(1000)
                .HasColumnName("content")
                .UseCollation("utf8mb4_unicode_ci");
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
                .HasColumnName("cyclist_name")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.DeletedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_deleted");
            entity.Property(e => e.IsEdited)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_edited");
            entity.Property(e => e.LikesCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("likes_count");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Comment).WithMany(p => p.Replies)
                .HasForeignKey(d => d.CommentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_reply_comment");
        });

        modelBuilder.Entity<UserBlogFollowerOnly>(entity =>
        {
            entity.HasKey(e => new { e.BlogId, e.UserId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("user_blog_follower_only");

            entity.Property(e => e.BlogId).HasColumnName("blog_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Blog).WithMany(p => p.UserBlogFollowerOnlies)
                .HasForeignKey(d => d.BlogId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_user_blog_follower_only_blog");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
