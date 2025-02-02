﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContentService.Domain.Entities;

public partial class Blog
{
    public int BlogId { get; set; }

    public int CreatorId { get; set; }

    public int CategoryId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? ReactionsCount { get; set; }

    public int? CommentsCount { get; set; }
    
    [MaxLength(10)]
    [Column(TypeName = "ENUM('Public', 'Private', 'Friends')")]
    public string PrivacySetting { get; set; } = null!;

    [MaxLength(10)]
    [Column(TypeName = "ENUM('Draft', 'Published', 'Archived', 'Deleted')")]
    public string Status { get; set; } = null!;

    public virtual ICollection<BlogBookmark> BlogBookmarks { get; set; } = new List<BlogBookmark>();

    public virtual ICollection<BlogPrivacy> BlogPrivacies { get; set; } = new List<BlogPrivacy>();

    public virtual BlogCategory Category { get; set; } = null!;

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();

    public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
}
