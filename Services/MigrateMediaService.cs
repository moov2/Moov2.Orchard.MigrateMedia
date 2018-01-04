using Microsoft.WindowsAzure.Storage;
using Orchard;

namespace Moov2.Orchard.MigrateMedia.Services
{
    public class MigrateMediaService : IMigrateMediaService
    {
        public bool CanConnectToAzureCloudStorage(string connectionString)
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(connectionString);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public interface IMigrateMediaService : IDependency
    {
        bool CanConnectToAzureCloudStorage(string connectionString);
    }
}