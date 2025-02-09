using System;
using System.Collections.Generic;

namespace ShopService.Domain.Entities;

public partial class Discount
{
    public int DiscountId { get; set; }

    public int ShopId { get; set; }

    public string? Description { get; set; }

    public float? DiscountCondition { get; set; }

    public float? Amount { get; set; }

    public float? Percentage { get; set; }

    public sbyte Status { get; set; }

    public DateTime TimeStart { get; set; }

    public DateTime TimeEnd { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ProductDiscount> ProductDiscounts { get; set; } = new List<ProductDiscount>();

    public virtual Shop Shop { get; set; } = null!;
}
