using System;
using System.Collections.Generic;

namespace ShopService.Domain.Entities;

public partial class Booking
{
    public int BookingId { get; set; }

    public int CyclistId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();
}
