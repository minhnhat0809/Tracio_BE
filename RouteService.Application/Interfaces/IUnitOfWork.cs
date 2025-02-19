namespace RouteService.Application.Interfaces;

public interface IUnitOfWork
{
    IRouteRepository RouteRepository { get; }
}