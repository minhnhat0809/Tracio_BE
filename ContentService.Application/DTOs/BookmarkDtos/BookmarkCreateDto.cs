namespace ContentService.Application.DTOs.BookmarkDtos;

public class BookmarkCreateDto
{
    public int OwnerId { get; set; }

    public int BlogId { get; set; }

    public string? CollectionName { get; set; }
}