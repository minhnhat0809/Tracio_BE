using MongoDB.Bson.Serialization.Attributes;

namespace NotificationService.Domain.Entities;

public class DeviceFcm
{
    [BsonId]
    [BsonElement("device_id")]
    public string DeviceId { get; set; } = null!;

    [BsonElement("fcm_token")]
    public string FcmToken { get; set; } = null!;
    
    [BsonElement("user_id")]
    public int UserId { get; set; }

}