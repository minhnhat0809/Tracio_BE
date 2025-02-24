using System;
using System.Collections.Generic;

namespace ShopService.Domain.Entities;

public partial class BookingMediaFile
{
    public int BookingMediaId { get; set; }

    public int BookingDetailId { get; set; }

    public string MediaUrl { get; set; } = null!;

    public sbyte MediaType { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual BookingDetail BookingDetail { get; set; } = null!;
}
