using System;
using System.Collections.Generic;

namespace ShopService.Domain.Entities;

public partial class Product
{
    public int ProductId { get; set; }

    public int ShopId { get; set; }

    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public sbyte Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ProductCategory Category { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<ProductDiscount> ProductDiscounts { get; set; } = new List<ProductDiscount>();

    public virtual ICollection<ProductMedium> ProductMedia { get; set; } = new List<ProductMedium>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    public virtual Shop Shop { get; set; } = null!;
}
