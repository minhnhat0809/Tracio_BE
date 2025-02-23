namespace ShopService.Domain.Enums;

public enum BookingStatus : sbyte
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    Finished = 3,
    Cancelled = 4
}