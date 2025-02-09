using System;
using System.Collections.Generic;

namespace ShopService.Domain.Entities;

public partial class ProductReview
{
    public int ReviewId { get; set; }

    public int ProductId { get; set; }

    public int CyclistId { get; set; }

    public string CyclistName { get; set; } = null!;

    public sbyte? Rating { get; set; }

    public string? ReviewContent { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ReviewMediaFile> ReviewMediaFiles { get; set; } = new List<ReviewMediaFile>();
}
