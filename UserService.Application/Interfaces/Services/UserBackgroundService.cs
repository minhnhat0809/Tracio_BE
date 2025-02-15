using FirebaseAdmin.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace UserService.Application.Interfaces.Services;

public class UserBackgroundService: BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24); // Runs every 24 hours

    public UserBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                // get all users
                var users = await unitOfWork.UserRepository!.GetAllAsync();

                // we do nothing in here
            }

            await Task.Delay(_cleanupInterval, stoppingToken); // ✅ Wait before running again
        }
    }
}