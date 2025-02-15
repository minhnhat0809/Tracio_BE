using System;
using System.Collections.Generic;

namespace ContentService.Domain.Entities;

public partial class Blog
{
    public int BlogId { get; set; }

    public int CreatorId { get; set; }

    public int CategoryId { get; set; }

    public string CreatorName { get; set; } = null!;

    public string CreatorAvatar { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? ReactionsCount { get; set; }

    public int? CommentsCount { get; set; }

    public sbyte PrivacySetting { get; set; }

    public sbyte Status { get; set; }

    public virtual ICollection<BlogBookmark> BlogBookmarks { get; set; } = new List<BlogBookmark>();

    public virtual ICollection<BlogPrivacy> BlogPrivacies { get; set; } = new List<BlogPrivacy>();

    public virtual BlogCategory Category { get; set; } = null!;

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();

    public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();

    public virtual ICollection<UserBlogFollowerOnly> UserBlogFollowerOnlies { get; set; } = new List<UserBlogFollowerOnly>();
}
