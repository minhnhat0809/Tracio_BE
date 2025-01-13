using System;
using System.Collections.Generic;

namespace ContentService.Domain.Entities;

public partial class Reaction
{
    public int ReactionId { get; set; }

    public int UserId { get; set; }

    public int EntityId { get; set; }

    public sbyte EntityType { get; set; }

    public sbyte ReactionType { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Blog Entity { get; set; } = null!;

    public virtual Reply Entity1 { get; set; } = null!;

    public virtual Comment EntityNavigation { get; set; } = null!;
}
