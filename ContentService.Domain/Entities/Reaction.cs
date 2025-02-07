using System;
using System.Collections.Generic;

namespace ContentService.Domain.Entities;

public partial class Reaction
{
    public int ReactionId { get; set; }

    public int CyclistId { get; set; }

    public string CyclistName { get; set; } = null!;

    public int? BlogId { get; set; }

    public int? CommentId { get; set; }

    public int? ReplyId { get; set; }

    public sbyte ReactionType { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Blog? Blog { get; set; }

    public virtual Comment? Comment { get; set; }

    public virtual Reply? Reply { get; set; }
}
