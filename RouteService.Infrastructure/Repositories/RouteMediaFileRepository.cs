using Microsoft.EntityFrameworkCore;
using RouteService.Application.Interfaces;
using RouteService.Domain.Entities;
using RouteService.Infrastructure.Contexts;

namespace RouteService.Infrastructure.Repositories;

public class RouteMediaFileRepository(TracioRouteDbContext context) : RepositoryBase<RouteMediaFile>(context), IRouteMediaFileRepository
{
   public async Task<IEnumerable<RouteMediaFile>> DeleteAllRouteMediaFilesByRouteIdAsync(int routeId)
   {
      var mediaFiles = await _context.RouteMediaFiles.Where(r => r.RouteId == routeId).ToListAsync();

      if (mediaFiles.Any())
      {
         _context.RouteMediaFiles.RemoveRange(mediaFiles); 
         await _context.SaveChangesAsync();
      }

      return mediaFiles;
   }

}