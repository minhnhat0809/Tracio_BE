using Microsoft.EntityFrameworkCore;
using ShopService.Domain.Entities;

namespace ShopService.Infrastructure.Contexts;

public partial class TracioShopDbContext : DbContext
{
    public TracioShopDbContext()
    {
    }

    public TracioShopDbContext(DbContextOptions<TracioShopDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingDetail> BookingDetails { get; set; }

    public virtual DbSet<BookingMediaFile> BookingMediaFiles { get; set; }

    public virtual DbSet<MediaFile> MediaFiles { get; set; }

    public virtual DbSet<Reply> Replies { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<ServiceCategory> ServiceCategories { get; set; }

    public virtual DbSet<Shop> Shops { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=tracio_shop;user=root;password=N@hat892003.", Microsoft.EntityFrameworkCore.ServerVersion.Parse("9.2.0-mysql"), x => x.UseNetTopologySuite());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PRIMARY");

            entity.ToTable("booking");

            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CyclistId).HasColumnName("cyclist_id");
        });

        modelBuilder.Entity<BookingDetail>(entity =>
        {
            entity.HasKey(e => e.BookingDetailId).HasName("PRIMARY");

            entity
                .ToTable("booking_detail")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.BookingId, "fk_booking_detail_booking");

            entity.HasIndex(e => e.ServiceId, "fk_booking_detail_service");

            entity.Property(e => e.BookingDetailId).HasColumnName("booking_detail_id");
            entity.Property(e => e.BookedDate)
                .HasColumnType("datetime")
                .HasColumnName("booked_date");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.Note)
                .HasMaxLength(500)
                .HasColumnName("note");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.Reason)
                .HasMaxLength(255)
                .HasColumnName("reason");
            entity.Property(e => e.ReceivedAt)
                .HasColumnType("datetime")
                .HasColumnName("received_at");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingDetails)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("fk_booking_detail_booking");

            entity.HasOne(d => d.Service).WithMany(p => p.BookingDetails)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("fk_booking_detail_service");
        });

        modelBuilder.Entity<BookingMediaFile>(entity =>
        {
            entity.HasKey(e => e.BookingMediaId).HasName("PRIMARY");

            entity.ToTable("booking_media_files");

            entity.HasIndex(e => e.BookingDetailId, "fk_booking_media_files_detail");

            entity.Property(e => e.BookingMediaId).HasColumnName("booking_media_id");
            entity.Property(e => e.BookingDetailId).HasColumnName("booking_detail_id");
            entity.Property(e => e.MediaType).HasColumnName("media_type");
            entity.Property(e => e.MediaUrl)
                .HasMaxLength(500)
                .HasColumnName("media_url");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.BookingDetail).WithMany(p => p.BookingMediaFiles)
                .HasForeignKey(d => d.BookingDetailId)
                .HasConstraintName("fk_booking_media_files_detail");
        });

        modelBuilder.Entity<MediaFile>(entity =>
        {
            entity.HasKey(e => e.MediaFileId).HasName("PRIMARY");

            entity.ToTable("media_files");

            entity.HasIndex(e => e.ReplyId, "fk_media_reply");

            entity.HasIndex(e => e.ReviewId, "fk_media_review");

            entity.HasIndex(e => e.ServiceId, "fk_media_service");

            entity.HasIndex(e => e.ShopId, "fk_media_shop");

            entity.Property(e => e.MediaFileId).HasColumnName("media_file_id");
            entity.Property(e => e.EntityType).HasColumnName("entity_type");
            entity.Property(e => e.MediaUrl)
                .HasMaxLength(500)
                .HasColumnName("media_url");
            entity.Property(e => e.ReplyId).HasColumnName("reply_id");
            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.ShopId).HasColumnName("shop_id");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("uploaded_at");

            entity.HasOne(d => d.Reply).WithMany(p => p.MediaFiles)
                .HasForeignKey(d => d.ReplyId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_media_reply");

            entity.HasOne(d => d.Review).WithMany(p => p.MediaFiles)
                .HasForeignKey(d => d.ReviewId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_media_review");

            entity.HasOne(d => d.Service).WithMany(p => p.MediaFiles)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_media_service");

            entity.HasOne(d => d.Shop).WithMany(p => p.MediaFiles)
                .HasForeignKey(d => d.ShopId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_media_shop");
        });

        modelBuilder.Entity<Reply>(entity =>
        {
            entity.HasKey(e => e.ReplyId).HasName("PRIMARY");

            entity
                .ToTable("reply")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ReviewId, "fk_reply_review");

            entity.HasIndex(e => e.ShopId, "fk_reply_shop");

            entity.Property(e => e.ReplyId).HasColumnName("reply_id");
            entity.Property(e => e.Content)
                .HasMaxLength(1000)
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CyclistAvatar)
                .HasMaxLength(500)
                .HasColumnName("cyclist_avatar");
            entity.Property(e => e.CyclistId).HasColumnName("cyclist_id");
            entity.Property(e => e.CyclistName)
                .HasMaxLength(255)
                .HasColumnName("cyclist_name");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.ShopId).HasColumnName("shop_id");

            entity.HasOne(d => d.Review).WithMany(p => p.Replies)
                .HasForeignKey(d => d.ReviewId)
                .HasConstraintName("fk_reply_review");

            entity.HasOne(d => d.Shop).WithMany(p => p.Replies)
                .HasForeignKey(d => d.ShopId)
                .HasConstraintName("fk_reply_shop");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PRIMARY");

            entity
                .ToTable("review")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ServiceId, "fk_review_service");

            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.Content)
                .HasMaxLength(1000)
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CyclistAvatar)
                .HasMaxLength(500)
                .HasColumnName("cyclist_avatar");
            entity.Property(e => e.CyclistId).HasColumnName("cyclist_id");
            entity.Property(e => e.CyclistName)
                .HasMaxLength(255)
                .HasColumnName("cyclist_name");
            entity.Property(e => e.Rating)
                .HasPrecision(3, 2)
                .HasColumnName("rating");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Service).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("fk_review_service");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PRIMARY");

            entity
                .ToTable("service")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.CategoryId, "fk_service_category");

            entity.HasIndex(e => e.ShopId, "fk_service_shop");

            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.ClosedTime)
                .HasColumnType("time")
                .HasColumnName("closed_time");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.Duration)
                .HasColumnType("time")
                .HasColumnName("duration");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.OpenTime)
                .HasColumnType("time")
                .HasColumnName("open_time");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.ShopId).HasColumnName("shop_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Category).WithMany(p => p.Services)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_service_category");

            entity.HasOne(d => d.Shop).WithMany(p => p.Services)
                .HasForeignKey(d => d.ShopId)
                .HasConstraintName("fk_service_shop");
        });

        modelBuilder.Entity<ServiceCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PRIMARY");

            entity
                .ToTable("service_category")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(255)
                .HasColumnName("category_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Shop>(entity =>
        {
            entity.HasKey(e => e.ShopId).HasName("PRIMARY");

            entity
                .ToTable("shop")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.ShopId).HasColumnName("shop_id");
            entity.Property(e => e.Address)
                .HasMaxLength(500)
                .HasColumnName("address");
            entity.Property(e => e.ClosedTime)
                .HasColumnType("time")
                .HasColumnName("closed_time");
            entity.Property(e => e.ContactNumber)
                .HasMaxLength(20)
                .HasColumnName("contact_number");
            entity.Property(e => e.Coordinate).HasColumnName("coordinate");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.OpenTime)
                .HasColumnType("time")
                .HasColumnName("open_time");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.ShopName)
                .HasMaxLength(255)
                .HasColumnName("shop_name");
            entity.Property(e => e.TaxCode)
                .HasMaxLength(50)
                .HasColumnName("tax_code");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
