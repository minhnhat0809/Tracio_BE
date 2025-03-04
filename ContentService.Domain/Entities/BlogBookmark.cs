using System;
using System.Collections.Generic;

namespace ContentService.Domain.Entities;

public partial class BlogBookmark
{
    public int BookmarkId { get; set; }

    public int OwnerId { get; set; }

    public int BlogId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Blog Blog { get; set; } = null!;
}
