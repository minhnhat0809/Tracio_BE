using System;
using System.Collections.Generic;
namespace RouteService.Domain.Entities;

public partial class RouteComment
{
    public int CommentId { get; set; }

    public int RouteId { get; set; }

    public int? ParentCommentId { get; set; }

    public int CyclistId { get; set; }

    public int? MentionCyclistId { get; set; }

    public string CommentContent { get; set; } = null!;

    public int ReactionCounts { get; set; }

    public int CommentCounts { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Route Route { get; set; } = null!;
}
