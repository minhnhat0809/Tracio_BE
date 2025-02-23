using System;
using System.Collections.Generic;

namespace ShopService.Domain.Entities;

public partial class MediaFile
{
    public int MediaFileId { get; set; }

    public int? ReviewId { get; set; }

    public int? ServiceId { get; set; }

    public int? ReplyId { get; set; }

    public int? ShopId { get; set; }

    public sbyte EntityType { get; set; }

    public string MediaUrl { get; set; } = null!;

    public DateTime? UploadedAt { get; set; }

    public virtual Reply? Reply { get; set; }

    public virtual Review? Review { get; set; }

    public virtual Service? Service { get; set; }

    public virtual Shop? Shop { get; set; }
}
