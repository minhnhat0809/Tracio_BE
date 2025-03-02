namespace NotificationService.Application.Dtos.FcmDtos;

public class FcmCreateDto
{
    public string DeviceId { get; set; } = null!;

    public string FcmToken { get; set; } = null!;
}