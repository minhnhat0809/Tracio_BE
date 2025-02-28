using MongoDB.Driver;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Repositories;

public class NotificationRepo(IMongoDatabase database, string collectionName) : RepositoryBase<Notification>(database, collectionName), INotificationRepo
{
    
}