namespace NotificationService.Application.Dtos.NotificationDtos.ViewDtos;

public class NotificationDto
{
    public string NotificationId { get; set; } = null!;

    public int RecipientId { get; set; }
    
    public int SenderId { get; set; }
    
    public string SenderName { get; set; } = null!;

    public string SenderAvatar { get; set; } = null!;

    public int EntityId { get; set; }

    public sbyte EntityType { get; set; } 

    public string Message { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }
}