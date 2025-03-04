using MediatR;

namespace UserService.Application.Queries;

public record CheckUserValidQuery(int UserId) : IRequest<bool>;