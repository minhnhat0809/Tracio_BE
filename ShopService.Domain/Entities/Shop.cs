using System;
using System.Collections.Generic;

namespace ShopService.Domain.Entities;

public partial class Shop
{
    public int ShopId { get; set; }

    public int OwnerId { get; set; }

    public string ShopName { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string? ContactNumber { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Discount> Discounts { get; set; } = new List<Discount>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
