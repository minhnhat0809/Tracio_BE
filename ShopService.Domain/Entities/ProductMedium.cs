using System;
using System.Collections.Generic;

namespace ShopService.Domain.Entities;

public partial class ProductMedium
{
    public int MediaId { get; set; }

    public int ProductId { get; set; }

    public string MediaUrl { get; set; } = null!;

    public DateTime UploadedAt { get; set; }

    public virtual Product Product { get; set; } = null!;
}
