namespace ContentService.Domain.Enums;

public enum BlogStatus : sbyte
{
    Draft = 0,
    Published = 1,
    Pending = 2,
    Archived = 3,
    Rejected = 4,
    Deleted = 5
}