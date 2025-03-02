namespace RouteService.Domain.Enums;

public enum RouteMood : sbyte
{
    None = 0,        // Không có mood
    Happy = 1,       // Vui vẻ
    Excited = 2,     // Hào hứng
    Relaxed = 3,     // Thư giãn
    Tired = 4,       // Mệt mỏi
    Challenging = 5, // Đầy thử thách
    Adventurous = 6, // Phiêu lưu
    Angry = 7,       // Bực bội (đường xấu, bị lạc, gặp sự cố)
    Sad = 8,         // Buồn
    Nervous = 9,     // Lo lắng (đường nguy hiểm, sợ tai nạn)
    Focused = 10     // Tập trung (khi thi đấu hoặc luyện tập nghiêm túc)
}
