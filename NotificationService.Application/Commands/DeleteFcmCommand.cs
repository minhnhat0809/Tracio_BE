using MediatR;
using Shared.Dtos;

namespace NotificationService.Application.Commands;

public record DeleteFcmCommand(string DeviceId) : IRequest<ResponseDto>;