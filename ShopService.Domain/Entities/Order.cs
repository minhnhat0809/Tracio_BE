using System;
using System.Collections.Generic;

namespace ShopService.Domain.Entities;

public partial class Order
{
    public int OrderId { get; set; }

    public int CyclistId { get; set; }

    public int? DiscountId { get; set; }

    public decimal TotalPrice { get; set; }

    public sbyte Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Discount? Discount { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
