using Moov2.Orchard.MigrateMedia.Models;
using Moov2.Orchard.MigrateMedia.Services;
using Orchard;
using Orchard.Localization;
using Orchard.UI.Notify;
using System.Configuration;
using System.Web.Mvc;

namespace Moov2.Orchard.MigrateMedia.Controllers
{
    public class AdminController : Controller
    {
        #region Constants

        private const string MediaStorageStorageConnectionStringSettingName = "Orchard.Azure.Media.StorageConnectionString";

        #endregion

        #region Dependencies

        private readonly IMigrateMediaService _migrateMediaService;
        private readonly IOrchardServices _orchardServices;

        public Localizer T { get; set; }

        #endregion

        #region Constructor

        public AdminController(IMigrateMediaService migrateMediaService, IOrchardServices orchardServices)
        {
            _migrateMediaService = migrateMediaService;
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;
        }

        #endregion

        #region Actions

        [HttpGet]
        public ActionResult Index()
        {
            var model = new MigrateMediaModel();
            model.ConfiguredAzureBlobStorage = RemoveSensitiveInfoFromConnectionString(ConfigurationManager.AppSettings[MediaStorageStorageConnectionStringSettingName]);
            model.UseConfigurationAzureBlobStorage = true;
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(MigrateMediaModel model)
        {
            if (!model.UseConfigurationAzureBlobStorage && string.IsNullOrWhiteSpace(model.CustomAzureBlobStorageConnectionString))
                ModelState.AddModelError("CustomAzureBlobStorageConnectionString", "Azure Blob Storage Connection String is required.");

            model.ConfiguredAzureBlobStorage = RemoveSensitiveInfoFromConnectionString(ConfigurationManager.AppSettings[MediaStorageStorageConnectionStringSettingName]);

            if (!ModelState.IsValid)
                return View(model);

            var connectionString = model.UseConfigurationAzureBlobStorage ? ConfigurationManager.AppSettings[MediaStorageStorageConnectionStringSettingName] : model.CustomAzureBlobStorageConnectionString;

            if (!_migrateMediaService.CanConnectToAzureCloudStorage(connectionString))
            {
                _orchardServices.Notifier.Add(NotifyType.Error, T("Migration failed: Unable to connect to Azure blob storage."));
                return View(model);
            }

            _orchardServices.Notifier.Add(NotifyType.Information, T("Successfully migrated media."));
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

        #endregion
    }
}