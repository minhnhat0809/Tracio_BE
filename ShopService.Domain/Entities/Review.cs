using System;
using System.Collections.Generic;

namespace ShopService.Domain.Entities;

public partial class Review
{
    public int ReviewId { get; set; }

    public int CyclistId { get; set; }

    public int ServiceId { get; set; }

    public string CyclistName { get; set; } = null!;

    public string CyclistAvatar { get; set; } = null!;

    public string Content { get; set; } = null!;

    public decimal Rating { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();

    public virtual ICollection<Reply> Replies { get; set; } = new List<Reply>();

    public virtual Service Service { get; set; } = null!;
}
