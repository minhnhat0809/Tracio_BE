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

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Shop Shop { get; set; } = null!;
}
