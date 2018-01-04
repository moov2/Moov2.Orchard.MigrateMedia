namespace Moov2.Orchard.MigrateMedia.Models
{
    public class MigrateMediaModel
    {
        public string ConfiguredAzureBlobStorage { get; set; }

        public string CustomAzureBlobStorageConnectionString { get; set; }

        public bool UseConfigurationAzureBlobStorage { get; set; }
    }
}