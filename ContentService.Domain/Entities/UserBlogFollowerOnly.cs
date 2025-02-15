using System;
using System.Collections.Generic;

namespace ContentService.Domain.Entities;

public partial class UserBlogFollowerOnly
{
    public int BlogId { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsRead { get; set; }

    public virtual Blog Blog { get; set; } = null!;
}
