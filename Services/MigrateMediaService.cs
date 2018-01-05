using Microsoft.WindowsAzure.Storage;
using Orchard;
using Orchard.Azure;
using Orchard.Azure.Services.Environment.Configuration;
using Orchard.Azure.Services.FileSystems;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.Media;
using Orchard.Logging;
using Orchard.MediaLibrary.Services;
using System;
using System.IO;

namespace Moov2.Orchard.MigrateMedia.Services
{
    public class MigrateMediaService : IMigrateMediaService
    {
        #region Dependencies

        private readonly IMediaLibraryService _mediaLibraryService;
        private readonly IMimeTypeProvider _mimeTypeProvider;
        private readonly IPlatformConfigurationAccessor _platformConfigurationAccessor;
        private readonly IStorageProvider _storageProvider;
        private readonly ShellSettings _shellSettings;

        public ILogger Logger { get; set; }

        #endregion

        #region Constructor

        public MigrateMediaService(IMediaLibraryService mediaLibraryService, IMimeTypeProvider mimeTypeProvider, IPlatformConfigurationAccessor platformConfigurationAccessor, IStorageProvider storageProvider, ShellSettings shellSettings)
        {
            _mediaLibraryService = mediaLibraryService;
            _mimeTypeProvider = mimeTypeProvider;
            _platformConfigurationAccessor = platformConfigurationAccessor;
            _storageProvider = storageProvider;
            _shellSettings = shellSettings;

            Logger = NullLogger.Instance;
        }

        #endregion

        #region Implementation

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

        public MigrateMediaResult MigrateFileSystemToAzureBlobStorageAsync(string connectionString, bool isOverwrite)
        {
            var mediaItems = _mediaLibraryService.GetMediaContentItems()
                .List();

            var result = new MigrateMediaResult();
            var azureFileSystem = InitializeAzureFileSystem(connectionString);

            foreach (var mediaItem in mediaItems)
            {
                var path = Path.Combine(mediaItem.FolderPath, mediaItem.FileName);

                if (!_storageProvider.FileExists(path) || (!isOverwrite && azureFileSystem.FileExists(path)))
                {
                    result.IgnoredCount++;
                    continue;
                }

                try
                {
                    if (isOverwrite && azureFileSystem.FileExists(path))
                        azureFileSystem.DeleteFile(path);

                    var file = _storageProvider.GetFile(path);
                    var azureFile = azureFileSystem.CreateFile(path);

                    using (var inputStream = file.OpenRead())
                    {
                        using (var outputStream = azureFile.OpenWrite())
                        {
                            var buffer = new byte[8192];
                            while (true)
                            {
                                var length = inputStream.Read(buffer, 0, buffer.Length);
                                if (length <= 0)
                                    break;
                                outputStream.Write(buffer, 0, length);
                            }
                        }
                    }

                    result.SuccessfulTransferCount++;
                }
                catch (Exception ex)
                { 
                    Logger.Error(ex, string.Format("Failed to transfer media {0}.", path));
                    result.UnsuccessfulTransferCount++;

                    azureFileSystem.DeleteFile(path);
                }
            }

            return result;
        }

        #endregion

        #region HelperMethods

        private AzureFileSystem InitializeAzureFileSystem(string connectionString)
        {
            return new AzureFileSystem(
                connectionString,
                _platformConfigurationAccessor.GetSetting(Constants.MediaStorageContainerNameSettingName, _shellSettings.Name, null) ?? Constants.MediaStorageDefaultContainerName,
                _platformConfigurationAccessor.GetSetting(Constants.MediaStorageRootFolderPathSettingName, _shellSettings.Name, null) ?? _shellSettings.Name,
                false,
                _mimeTypeProvider
            );
        }

        #endregion
    }

    public interface IMigrateMediaService : IDependency
    {
        bool CanConnectToAzureCloudStorage(string connectionString);

        MigrateMediaResult MigrateFileSystemToAzureBlobStorageAsync(string connectionString, bool isOverwrite);
    }
}