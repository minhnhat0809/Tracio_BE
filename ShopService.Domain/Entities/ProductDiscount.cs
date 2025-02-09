using System;
using System.Collections.Generic;

namespace ShopService.Domain.Entities;

public partial class ProductDiscount
{
    public int ProductId { get; set; }

    public int DiscountId { get; set; }

    public DateTime Date { get; set; }

    public bool? IsActive { get; set; }

    public virtual Discount Discount { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
