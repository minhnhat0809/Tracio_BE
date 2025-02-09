using System;
using System.Collections.Generic;

namespace ShopService.Domain.Entities;

public partial class ReviewMediaFile
{
    public int ReviewMediaId { get; set; }

    public int ReviewId { get; set; }

    public string MediaUrl { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ProductReview Review { get; set; } = null!;
}
