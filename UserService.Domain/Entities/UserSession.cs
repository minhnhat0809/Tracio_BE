using System;
using System.Collections.Generic;

namespace UserService.Domain.Entities;

public partial class UserSession
{
    public int SessionId { get; set; }

    public int UserId { get; set; }

    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
