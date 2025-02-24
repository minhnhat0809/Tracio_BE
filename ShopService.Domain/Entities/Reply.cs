using System;
using System.Collections.Generic;

namespace ShopService.Domain.Entities;

public partial class Reply
{
    public int ReplyId { get; set; }

    public int ShopId { get; set; }

    public int CyclistId { get; set; }

    public int ReviewId { get; set; }

    public string Content { get; set; } = null!;

    public string CyclistName { get; set; } = null!;

    public string CyclistAvatar { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();

    public virtual Review Review { get; set; } = null!;

    public virtual Shop Shop { get; set; } = null!;
}
