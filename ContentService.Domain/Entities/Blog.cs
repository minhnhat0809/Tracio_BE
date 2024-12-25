using System;
using System.Collections.Generic;

namespace ContentService.Domain.Entities;

public partial class Blog
{
    public int BlogId { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? LikesCount { get; set; }

    public int? CommentsCount { get; set; }
}
