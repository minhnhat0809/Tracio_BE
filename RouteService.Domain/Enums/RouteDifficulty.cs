namespace RouteService.Domain.Enums;

public enum RouteDifficulty : sbyte
{
    Easy = 0,       // Dễ
    Moderate = 1,   // Trung bình
    Hard = 2,       // Khó
    Extreme = 3,    // Cực kỳ khó (tùy chọn nếu có)
    Unknown = -1    // Không xác định (trường hợp người dùng không chọn)
}
