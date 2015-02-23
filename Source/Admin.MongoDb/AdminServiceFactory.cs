using IdentityServer.Core.MongoDb;
using MongoDB.Driver;

namespace IdentityServer.Admin.MongoDb
{
    public static class AdminServiceFactory
    {
        public static IAdminService Create(StoreSettings settings)
        {
            var mongoClient = new MongoClient(settings.ConnectionString);
            var server = mongoClient.GetServer();
            var db = server.GetDatabase(settings.Database);
            return new AdminService(db, settings);
        }
    }
}
