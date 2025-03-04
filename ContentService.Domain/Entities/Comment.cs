using System;
using System.Collections.Generic;

namespace ContentService.Domain.Entities;

public partial class Comment
{
    public int CommentId { get; set; }

    public int CyclistId { get; set; }

    public string CyclistName { get; set; } = null!;

    public string CyclistAvatar { get; set; } = null!;

    public int BlogId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int LikesCount { get; set; }

    public int RepliesCount { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Blog Blog { get; set; } = null!;

    public virtual ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();

    public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();

    public virtual ICollection<Reply> Replies { get; set; } = new List<Reply>();
}
