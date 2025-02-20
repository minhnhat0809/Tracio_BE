namespace UserService.Application.Commands;

using MediatR;
using UserService.Application.DTOs.Auths;
using UserService.Application.DTOs.ResponseModel;

public class VerifyPhoneOtpCommand : IRequest<ResponseModel>
{
    public VerifyPhoneOtpRequestModel? RequestModel { get; set; }
}
