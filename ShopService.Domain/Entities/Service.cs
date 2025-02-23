using System;
using System.Collections.Generic;

namespace ShopService.Domain.Entities;

public partial class Service
{
    public int ServiceId { get; set; }

    public int ShopId { get; set; }

    public int? CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public TimeOnly OpenTime { get; set; }

    public TimeOnly ClosedTime { get; set; }

    public bool IsDeleted { get; set; }

    public sbyte Status { get; set; }

    public TimeOnly Duration { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();

    public virtual ServiceCategory? Category { get; set; }

    public virtual ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Shop Shop { get; set; } = null!;
}
