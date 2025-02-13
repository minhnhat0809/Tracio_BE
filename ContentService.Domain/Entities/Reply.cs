using System;
using System.Collections.Generic;

namespace ContentService.Domain.Entities;

public partial class Reply
{
    public int ReplyId { get; set; }

    public int CyclistId { get; set; }

    public int CommentId { get; set; }

    public string CyclistName { get; set; } = null!;

    public string CyclistAvatar { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? LikesCount { get; set; }

    public bool? IsEdited { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Comment Comment { get; set; } = null!;

    public virtual ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();

    public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
}
