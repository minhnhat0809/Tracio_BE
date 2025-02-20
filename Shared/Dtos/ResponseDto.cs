namespace Shared.Dtos;

public class ResponseDto(object? result, string? message, bool isSucceed, int statusCode)
{
    public object? Result { get; set; } = result;
    
    public string? Message { get; set; } = message;
    
    public bool IsSucceed { get; set; } = isSucceed;
    public int StatusCode { get; set; } = statusCode;

    public static ResponseDto GetSuccess(object? result, string? message)
    {
        return new ResponseDto(result, message, true, 200);
    }

    public static ResponseDto CreateSuccess(object? result, string? message)
    {
        return new ResponseDto(result, message, true, 201);
    }

    public static ResponseDto UpdateSuccess(object? result, string? message)
    {
        return new ResponseDto(result, message, true, 200);
    }
    
    public static ResponseDto DeleteSuccess(string? message)
    {
        return new ResponseDto(null, message, true, 204);
    }

    public static ResponseDto InternalError(string? message)
    {
        return new ResponseDto(null, message, false, 500);
    }
    
    public static ResponseDto BadRequest(string? message)
    {
        return new ResponseDto(null, message, false, 400);
    }

    public static ResponseDto NotFound(string? message)
    {
        return new ResponseDto(null, message, false, 404);
    }
}