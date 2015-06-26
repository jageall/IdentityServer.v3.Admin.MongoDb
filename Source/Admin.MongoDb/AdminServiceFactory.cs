using IdentityServer.Core.MongoDb;
using MongoDB.Driver;

namespace IdentityServer.Admin.MongoDb
{
    public static class AdminServiceFactory
    {
        public static IAdminService Create(StoreSettings settings)
        {
            var mongoClient = new MongoClient(settings.ConnectionString);
            var db = mongoClient.GetDatabase(settings.Database);
            return new AdminService(mongoClient, db, settings);
        }
    }
}
