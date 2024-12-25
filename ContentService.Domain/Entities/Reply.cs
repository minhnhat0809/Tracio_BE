using System;
using System.Collections.Generic;

namespace ContentService.Domain.Entities;

public partial class Reply
{
    public int ReplyId { get; set; }

    public int UserId { get; set; }

    public int CommentId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public int? LikesCount { get; set; }

    public virtual Comment Comment { get; set; } = null!;
}
