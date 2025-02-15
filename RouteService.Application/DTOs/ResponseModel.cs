namespace RouteService.Application.DTOs;

public class ResponseModel(object? result, string? message, bool isSucceed, int statusCode)
{
    public object? Result { get; set; } = result;
    
    public string? Message { get; set; } = message;
    
    public bool IsSucceed { get; set; } = isSucceed;
    public int StatusCode { get; set; } = statusCode;
        
}