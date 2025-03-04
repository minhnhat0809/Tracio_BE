using FirebaseAdmin.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using UserService.Application.Commands;
using UserService.Application.DTOs.Users;

namespace UserService.Application.Interfaces.Services
{
    using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using UserService.Application.Interfaces;
using MediatR;

    public class UserBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UserBackgroundService> _logger;

        public UserBackgroundService(IServiceProvider serviceProvider, ILogger<UserBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("UserBackgroundService is starting.");

            using (var scope = _serviceProvider.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var firebaseAuthRepo = scope.ServiceProvider.GetRequiredService<IFirebaseAuthenticationRepository>();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                string adminEmail = "huydocon1@gmail.com";
                string adminPassword = "admin1";

                try
                {
                    // check in db
                    if (await repository.UserRepository.GetUserByPropertyAsync(adminEmail) != null)
                    {
                        _logger.LogInformation("Admin account already exists in database, skipping creation.");
                        return;
                    }

                    string firebaseUid;
                    try
                    {
                        // check Firebase Auth
                        var firebaseUser =
                            await firebaseAuthRepo.GetFirebaseUserByEmailAsync(adminEmail, stoppingToken);
                        firebaseUid = firebaseUser.Uid;
                        _logger.LogInformation("Admin account found in Firebase.");
                    }
                    catch (FirebaseAuthException)
                    {
                        _logger.LogWarning("Admin not found in Firebase. Creating a new admin account.");

                        // create user record
                        var firebaseUser = await CreateFirebaseAdminUserAsync(adminEmail,
                            adminPassword, stoppingToken);
                        firebaseUid = firebaseUser.Uid;
                    }

                    // register to db
                    await RegisterAdminAccount(mediator, firebaseUid, adminEmail, stoppingToken);

                    // claim role
                    var claims = new Dictionary<string, object> { { "role", "admin" } };
                    await firebaseAuthRepo.SetCustomClaimsAsync(firebaseUid, claims, stoppingToken);
                    _logger.LogInformation("Admin role has been assigned.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Unexpected error in UserBackgroundService: {ex.Message}");
                }
            }

            _logger.LogInformation("UserBackgroundService has finished execution.");
        }

        /// <summary>
        /// Create User Record In FBAuth
        /// </summary>
        private async Task<UserRecord> CreateFirebaseAdminUserAsync(
            string email,
            string password,
            CancellationToken cancellationToken)
        {
            var userArgs = new UserRecordArgs
            {
                Email = email,
                EmailVerified = true,
                Password = password,
                DisplayName = "Admin",
                Disabled = false
            };

            try
            {
                var userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(userArgs, cancellationToken);
                _logger.LogInformation("Admin account successfully created in Firebase.");
                return userRecord;
            }
            catch (FirebaseAuthException ex)
            {
                _logger.LogError($"Failed to create admin account in Firebase: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Background Service: Register admin in database if not exist
        /// </summary>
        private async Task RegisterAdminAccount(IMediator mediator, string firebaseUid, string email,
            CancellationToken cancellationToken)
        {
            var adminRegisterCommand = new UserRegisterCommand
            {
                RegisterModel = new UserRegisterModel()
                {
                    FirebaseUid = firebaseUid,
                    Email = email,
                    UserName = "Admin"
                }
            };

            var response = await mediator.Send(adminRegisterCommand, cancellationToken);

            if (response.Status == "success")
            {
                _logger.LogInformation("Admin account successfully registered in the database.");
            }
            else
            {
                _logger.LogError($"Failed to register admin account: {response.Message}");
            }
        }
    }

}
