using System;
using System.Collections.Generic;

namespace ContentService.Domain.Entities;

public partial class MediaFile
{
    public int MediaId { get; set; }

    public int? BlogId { get; set; }

    public int? CommentId { get; set; }

    public int? ReplyId { get; set; }

    public string MediaUrl { get; set; } = null!;

    public string MediaType { get; set; } = null!;

    public DateTime UploadedAt { get; set; }

    public virtual Blog? Blog { get; set; }

    public virtual Comment? Comment { get; set; }

    public virtual Reply? Reply { get; set; }
}
