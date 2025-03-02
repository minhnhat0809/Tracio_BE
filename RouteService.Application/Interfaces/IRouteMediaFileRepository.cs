using RouteService.Domain.Entities;

namespace RouteService.Application.Interfaces;

public interface IRouteMediaFileRepository : IRepositoryBase<RouteMediaFile>
{
    Task<IEnumerable<RouteMediaFile>> DeleteAllRouteMediaFilesByRouteIdAsync(int routeId);
}