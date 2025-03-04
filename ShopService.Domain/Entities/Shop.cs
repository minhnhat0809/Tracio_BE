using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace ShopService.Domain.Entities;

public partial class Shop
{
    public int ShopId { get; set; }

    public int OwnerId { get; set; }

    public string TaxCode { get; set; } = null!;

    public string ShopName { get; set; } = null!;

    public string Address { get; set; } = null!;

    public Point Coordinate { get; set; } = null!;

    public string ContactNumber { get; set; } = null!;

    public TimeOnly OpenTime { get; set; }

    public TimeOnly ClosedTime { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();

    public virtual ICollection<Reply> Replies { get; set; } = new List<Reply>();

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
