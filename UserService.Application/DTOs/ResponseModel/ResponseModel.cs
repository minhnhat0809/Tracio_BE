namespace UserService.Application.DTOs.ResponseModel;

public class ResponseModel(string? status, int statusCode, string? message, object? result)
{
    public string? Status { get; set; } = status; // ['success' || 'error']
    public int StatusCode { get; set; } = statusCode;
    public string? Message { get; set; } = message;
    public object? Result { get; set; } = result;
}