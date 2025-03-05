namespace RouteService.Api.DIs;


public record RedisConfiguration
{
    public string Host { get; set; } = "redis";
    public int Port { get; set; } = 6379;
    public int Database { get; set; } = 0;
    public string Password { get; set; } = string.Empty;
}
