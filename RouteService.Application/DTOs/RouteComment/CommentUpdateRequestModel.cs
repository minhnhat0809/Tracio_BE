using System.ComponentModel.DataAnnotations;

namespace RouteService.Application.DTOs.RouteComment;

public class CommentUpdateRequestModel
{
    [Required(ErrorMessage = "Updated content is required.")]
    [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
    public required string Content { get; set; }
}
