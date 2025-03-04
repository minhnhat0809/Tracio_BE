using System;
using System.Collections.Generic;

namespace ShopService.Domain.Entities;

public partial class BookingDetail
{
    public int BookingDetailId { get; set; }

    public int BookingId { get; set; }

    public int ServiceId { get; set; }

    public DateTime BookedDate { get; set; }

    public string Reason { get; set; } = null!;

    public sbyte Status { get; set; }

    public string Note { get; set; } = null!;

    public decimal Price { get; set; }

    public DateTime? ReceivedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual ICollection<BookingMediaFile> BookingMediaFiles { get; set; } = new List<BookingMediaFile>();

    public virtual Service Service { get; set; } = null!;
}
