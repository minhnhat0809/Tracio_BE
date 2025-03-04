using Grpc.Net.Client;
using userservice;
using RouteService.Application.Interfaces;
using Grpc.Core;
using UserResponse = RouteService.Application.DTOs.Users.UserResponse;

namespace RouteService.Api.grpcClient;

public class UserGrpcClient : IUserRepository
{
    private readonly UserService.UserServiceClient _client;

    public UserGrpcClient(IConfiguration config)
    {
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator; // Allows self-signed certs

        var channel = GrpcChannel.ForAddress(config["GrpcSettings:UserServiceUrl"] ?? throw new InvalidOperationException(),
            new GrpcChannelOptions { HttpHandler = handler });

        _client = new UserService.UserServiceClient(channel);
    }

    /*public async Task<UserResponse> GetUserByIdAsync(int userId)
    {
        try
        {
            var request = new UserRequest { UserId = userId }; // ✅ FIXED field name
            var exist = await _client.GetUserByIdAsync(request);
            return new UserResponse()
            {
                Avatar = exist.Avatar,
                Username = exist.UserName,
                IsUserValid = exist.IsValid,
            };
        }
        catch (RpcException ex)
        {
            throw new Exception($"gRPC error: {ex.Status.StatusCode} - {ex.Status.Detail}");
        }
    }*/

    public async Task<bool> ValidateUserAsync(int userId)
    {
        try
        {
            var request = new UserRequest { UserId = userId }; // ✅ FIXED field name
            var response = await _client.ValidateUserAsync(request);
            return response.IsValid;
        }
        catch (RpcException ex)
        {
            throw new Exception($"gRPC error: {ex.Status.StatusCode} - {ex.Status.Detail}");
        }
    }
}