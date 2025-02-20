namespace UserService.Application.Commands;

using MediatR;
using UserService.Application.DTOs.Auths;
using UserService.Application.DTOs.ResponseModel;

public class SendPhoneOtpCommand : IRequest<ResponseModel>
{
    public SendPhoneOtpRequestModel? RequestModel { get; set; }
}
