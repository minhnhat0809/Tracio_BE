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

    public virtual DbSet<Discount> Discounts { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<ProductDiscount> ProductDiscounts { get; set; }

    public virtual DbSet<ProductMedium> ProductMedia { get; set; }

    public virtual DbSet<ProductReview> ProductReviews { get; set; }

    public virtual DbSet<ReviewMediaFile> ReviewMediaFiles { get; set; }

    public virtual DbSet<Shop> Shops { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Discount>(entity =>
        {
            entity.HasKey(e => e.DiscountId).HasName("PRIMARY");

            entity.ToTable("discount");

            entity.HasIndex(e => e.ShopId, "fk_discount_shop");

            entity.Property(e => e.DiscountId).HasColumnName("discount_id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.DiscountCondition).HasColumnName("discount_condition");
            entity.Property(e => e.Percentage).HasColumnName("percentage");
            entity.Property(e => e.ShopId).HasColumnName("shop_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TimeEnd)
                .HasColumnType("datetime")
                .HasColumnName("time_end");
            entity.Property(e => e.TimeStart)
                .HasColumnType("datetime")
                .HasColumnName("time_start");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Shop).WithMany(p => p.Discounts)
                .HasForeignKey(d => d.ShopId)
                .HasConstraintName("fk_discount_shop");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PRIMARY");

            entity.ToTable("order");

            entity.HasIndex(e => e.DiscountId, "fk_order_discount");

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CyclistId).HasColumnName("cyclist_id");
            entity.Property(e => e.DiscountId).HasColumnName("discount_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TotalPrice)
                .HasPrecision(10, 2)
                .HasColumnName("total_price");

            entity.HasOne(d => d.Discount).WithMany(p => p.Orders)
                .HasForeignKey(d => d.DiscountId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_order_discount");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PRIMARY");

            entity.ToTable("order_detail");

            entity.HasIndex(e => e.OrderId, "fk_order_detail_order");

            entity.HasIndex(e => e.ProductId, "fk_order_detail_product");

            entity.HasIndex(e => e.ShopId, "fk_order_detail_shop");

            entity.Property(e => e.OrderDetailId).HasColumnName("order_detail_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.ShopId).HasColumnName("shop_id");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("fk_order_detail_order");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("fk_order_detail_product");

            entity.HasOne(d => d.Shop).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ShopId)
                .HasConstraintName("fk_order_detail_shop");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PRIMARY");

            entity.ToTable("payment");

            entity.HasIndex(e => e.OrderId, "fk_payment_order");

            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaymentMethod)
                .HasColumnType("enum('Cash','Online')")
                .HasColumnName("payment_method");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("fk_payment_order");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PRIMARY");

            entity.ToTable("product");

            entity.HasIndex(e => e.CategoryId, "fk_product_category");

            entity.HasIndex(e => e.ShopId, "fk_product_shop");

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.ShopId).HasColumnName("shop_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.StockQuantity).HasColumnName("stock_quantity");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_product_category");

            entity.HasOne(d => d.Shop).WithMany(p => p.Products)
                .HasForeignKey(d => d.ShopId)
                .HasConstraintName("fk_product_shop");
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PRIMARY");

            entity.ToTable("product_category");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(255)
                .HasColumnName("category_name")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<ProductDiscount>(entity =>
        {
            entity.HasKey(e => new { e.ProductId, e.DiscountId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("product_discount");

            entity.HasIndex(e => e.DiscountId, "fk_product_discount_discount");

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.DiscountId).HasColumnName("discount_id");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");

            entity.HasOne(d => d.Discount).WithMany(p => p.ProductDiscounts)
                .HasForeignKey(d => d.DiscountId)
                .HasConstraintName("fk_product_discount_discount");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductDiscounts)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("fk_product_discount_product");
        });

        modelBuilder.Entity<ProductMedium>(entity =>
        {
            entity.HasKey(e => e.MediaId).HasName("PRIMARY");

            entity.ToTable("product_media");

            entity.HasIndex(e => e.ProductId, "fk_media_product");

            entity.Property(e => e.MediaId).HasColumnName("media_id");
            entity.Property(e => e.MediaUrl)
                .HasMaxLength(2083)
                .HasColumnName("media_url");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("uploaded_at");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductMedia)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("fk_media_product");
        });

        modelBuilder.Entity<ProductReview>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PRIMARY");

            entity
                .ToTable("product_review")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ProductId, "fk_product_review_product");

            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CyclistId).HasColumnName("cyclist_id");
            entity.Property(e => e.CyclistName)
                .HasMaxLength(255)
                .HasColumnName("cyclist_name");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.ReviewContent)
                .HasMaxLength(1000)
                .HasColumnName("review_content");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_product_review_product");
        });

        modelBuilder.Entity<ReviewMediaFile>(entity =>
        {
            entity.HasKey(e => e.ReviewMediaId).HasName("PRIMARY");

            entity.ToTable("review_media_file");

            entity.HasIndex(e => e.ReviewId, "fk_review_media_review");

            entity.Property(e => e.ReviewMediaId).HasColumnName("review_media_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.MediaUrl)
                .HasMaxLength(1000)
                .HasColumnName("media_url");
            entity.Property(e => e.ReviewId).HasColumnName("review_id");

            entity.HasOne(d => d.Review).WithMany(p => p.ReviewMediaFiles)
                .HasForeignKey(d => d.ReviewId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_review_media_review");
        });

        modelBuilder.Entity<Shop>(entity =>
        {
            entity.HasKey(e => e.ShopId).HasName("PRIMARY");

            entity.ToTable("shop");

            entity.Property(e => e.ShopId).HasColumnName("shop_id");
            entity.Property(e => e.Address)
                .HasMaxLength(350)
                .HasColumnName("address")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.ContactNumber)
                .HasMaxLength(10)
                .HasColumnName("contact_number");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.ShopName)
                .HasMaxLength(255)
                .HasColumnName("shop_name")
                .UseCollation("utf8mb4_unicode_ci");
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
