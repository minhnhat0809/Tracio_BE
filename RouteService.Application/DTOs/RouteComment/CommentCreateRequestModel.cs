using System.ComponentModel.DataAnnotations;

namespace RouteService.Application.DTOs.RouteComment;


public class CommentCreateRequestModel
{
    //public int? ParentCommentId { get; set; } // If null, it's a top-level comment

    [Required(ErrorMessage = "Comment content cannot be empty.")]
    [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
    public required string Content { get; set; }
}
