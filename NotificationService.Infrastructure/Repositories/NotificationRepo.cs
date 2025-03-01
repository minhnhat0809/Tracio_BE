using MongoDB.Driver;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Repositories;

public class NotificationRepo(IMongoDatabase database) : RepositoryBase<Notification>(database, "notification"), INotificationRepo
{
    
}