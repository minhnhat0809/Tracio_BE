namespace ContentService.Application.DTOs.CommentDtos;

public class CommentCreateDto
{
    public int BlogId { get; set; }
    public string Content { get; set; } = null!;
}