namespace RouteService.Application.DTOs.Bookmarks;

public class BookmarkViewModel
{
    public int BookmarkId { get; set; }

    public int CyclistId { get; set; }

    public int RouteId { get; set; }

    public DateTime CreatedAt { get; set; }
}