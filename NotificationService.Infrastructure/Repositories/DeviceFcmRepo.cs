using MongoDB.Driver;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Repositories;

public class DeviceFcmRepo(IMongoDatabase database) : RepositoryBase<DeviceFcm>(database, "device_fcm"), IDeviceFcmRepo
{
    
}