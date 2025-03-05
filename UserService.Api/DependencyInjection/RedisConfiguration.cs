namespace UserService.Api.DependencyInjection;


public record RedisConfiguration
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6379;
    public int Database { get; set; } = 0;
    public string Password { get; set; } = string.Empty;
}
