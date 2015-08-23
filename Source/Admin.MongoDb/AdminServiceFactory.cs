using IdentityServer3.MongoDb;
using MongoDB.Driver;

namespace IdentityServer3.Admin.MongoDb
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
