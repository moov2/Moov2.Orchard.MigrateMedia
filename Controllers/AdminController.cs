using Moov2.Orchard.MigrateMedia.Models;
using Moov2.Orchard.MigrateMedia.Services;
using Orchard;
using Orchard.Localization;
using Orchard.MediaLibrary.Services;
using Orchard.UI.Notify;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Moov2.Orchard.MigrateMedia.Controllers
{
    public class AdminController : Controller
    {
        #region Constants

        private const string MediaStorageStorageConnectionStringSettingName = "Orchard.Azure.Media.StorageConnectionString";

        #endregion

        #region Dependencies

        private readonly IMediaLibraryService _mediaLibraryService;
        private readonly IMigrateMediaService _migrateMediaService;
        private readonly IOrchardServices _orchardServices;

        public Localizer T { get; set; }

        #endregion

        #region Constructor

        public AdminController(IMediaLibraryService mediaLibraryService, IMigrateMediaService migrateMediaService, IOrchardServices orchardServices)
        {
            _mediaLibraryService = mediaLibraryService;
            _migrateMediaService = migrateMediaService;
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;
        }   

        #endregion

        #region Actions

        [HttpGet]
        public ActionResult Index()
        {
            var model = new MigrateMediaModel
            {
                IsOverwrite = false,
                UseConfigurationAzureBlobStorage = true
            };

            SetDefaultModelProperties(model);

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(MigrateMediaModel model)
        {
            if (!model.UseConfigurationAzureBlobStorage && string.IsNullOrWhiteSpace(model.CustomAzureBlobStorageConnectionString))
                ModelState.AddModelError("CustomAzureBlobStorageConnectionString", "Azure Blob Storage Connection String is required.");

            SetDefaultModelProperties(model);

            if (!ModelState.IsValid)
                return View(model);

            var connectionString = model.UseConfigurationAzureBlobStorage ? ConfigurationManager.AppSettings[MediaStorageStorageConnectionStringSettingName] : model.CustomAzureBlobStorageConnectionString;

            if (!_migrateMediaService.CanConnectToAzureCloudStorage(connectionString))
            {
                _orchardServices.Notifier.Add(NotifyType.Error, T("Migration failed: Unable to connect to Azure blob storage."));
                return View(model);
            }

            var result = _migrateMediaService.MigrateFileSystemToAzureBlobStorageAsync(connectionString, model.IsOverwrite);

            if (result.SuccessfulTransferCount > 0 || result.UnsuccessfulTransferCount == 0)
                _orchardServices.Notifier.Add(NotifyType.Information, T(string.Format("Successfully migrated {0} media item(s).", result.SuccessfulTransferCount)));

            if (result.IgnoredCount > 0)
                _orchardServices.Notifier.Add(NotifyType.Warning, T(string.Format("Ignored {0} media item(s) during migration.", result.IgnoredCount)));

            if (result.UnsuccessfulTransferCount > 0)
                _orchardServices.Notifier.Add(NotifyType.Error, T(string.Format("Failed to migrate {0} media item(s).", result.UnsuccessfulTransferCount)));

            return RedirectToAction("Index");
        }

        #endregion

        #region HelperMethods

        private string RemoveSensitiveInfoFromConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return string.Empty;

            try
            {
                return connectionString.Split(';')[1].Replace("AccountName=", string.Empty);
            }
            catch
            {
                return connectionString;
            }
        }

        private void SetDefaultModelProperties(MigrateMediaModel model)
        {
            model.ConfiguredAzureBlobStorage = RemoveSensitiveInfoFromConnectionString(ConfigurationManager.AppSettings[MediaStorageStorageConnectionStringSettingName]);
            model.MediaItemsCount = _mediaLibraryService.GetMediaContentItems().Count();
        }

        #endregion
    }
}