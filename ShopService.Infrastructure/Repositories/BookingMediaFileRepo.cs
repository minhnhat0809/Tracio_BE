using ShopService.Application.Interfaces;
using ShopService.Domain;
using ShopService.Domain.Entities;
using ShopService.Infrastructure.Contexts;

namespace ShopService.Infrastructure.Repositories;

public class BookingMediaFileRepo(TracioShopDbContext context) : RepositoryBase<BookingMediaFile>(context), IBookingMediaFileRepo
{
    
}