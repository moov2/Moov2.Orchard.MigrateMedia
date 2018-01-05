using System.ComponentModel.DataAnnotations;

namespace Moov2.Orchard.MigrateMedia.Models
{
    public class MigrateMediaModel
    {
        public string ConfiguredAzureBlobStorage { get; set; }

        public string CustomAzureBlobStorageConnectionString { get; set; }

        public int MediaItemsCount { get; set; }

        [Required(ErrorMessage = "Migration method is required.")]
        public string MigrationMethod { get; set; }

        public bool IsOverwrite { get; set; }

        public bool UseConfigurationAzureBlobStorage { get; set; }
    }
}