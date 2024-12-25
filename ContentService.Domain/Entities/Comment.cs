using System;
using System.Collections.Generic;

namespace ContentService.Domain.Entities;

public partial class Comment
{
    public int CommentId { get; set; }

    public int UserId { get; set; }

    public int EntityId { get; set; }

    public sbyte EntityType { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsEdited { get; set; }

    public int? LikesCount { get; set; }

    public virtual ICollection<Reply> Replies { get; set; } = new List<Reply>();
}
