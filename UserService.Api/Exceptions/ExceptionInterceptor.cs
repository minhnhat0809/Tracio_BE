using Grpc.Core;
using Grpc.Core.Interceptors;

namespace UserService.Api.Exceptions;

public class ExceptionInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, 
        ServerCallContext context, 
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🔥 gRPC Exception: {ex.Message}");
            throw new RpcException(new Status(StatusCode.Internal, $"Internal Server Error: {ex.Message}"));
        }
    }
}