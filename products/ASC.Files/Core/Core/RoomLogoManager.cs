using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using ASC.Common;
using ASC.Common.Caching;
using ASC.Common.Threading;
using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.Data.Storage;
using ASC.Web.Core;
using ASC.Web.Core.Files;
using ASC.Web.Core.Images;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility;

using SixLabors.ImageSharp;

namespace ASC.Files.Core.Core
{
    [Scope(Additional = typeof(LogoResizeWorkerItemExtension))]
    public class RoomLogoManager
    {
        public static Size OriginalLogoSize => new Size(216, 216);
        public static Size BigLogoSize => new Size(96, 96);
        public static Size MediumLogoSize => new Size(36, 36);
        public static Size SmallLogoSize => new Size(24, 24);

        private const string MODULE_NAME = "roomLogos";
        private const string LOGO_PATH = "{0}\\size_{1}-{2}.{3}";

        private Regex _keyPattern = new Regex(@"\d+\/\S+\/\d+\/\d+", RegexOptions.Compiled);
        private TimeSpan _cacheItemLifeTime = TimeSpan.FromMinutes(30);

        private StorageFactory StorageFactory { get; }
        private TenantManager TenantManager { get; }
        private SettingsManager SettingsManager { get; }
        private ICache Cache { get; }

        private readonly DistributedTaskQueue ResizeQueue;

        private int TenantId => TenantManager.GetCurrentTenant().TenantId;

        private IDataStore _store;
        private IDataStore Store => _store ??= StorageFactory.GetStorage(TenantId.ToString(), MODULE_NAME);

        public RoomLogoManager(
            StorageFactory storageFactory,
            TenantManager tenantManager,
            SettingsManager settingsManager,
            DistributedTaskQueueOptionsManager optionsQueue,
            AscCache cache)
        {
            StorageFactory = storageFactory;
            TenantManager = tenantManager;
            SettingsManager = settingsManager;
            ResizeQueue = optionsQueue.Get<ResizeWorkerItem>();
            Cache = cache;
        }

        public string SaveOrUpdateLogo<T>(T folderId, byte[] imageData)
        {
            return SaveOrUpdateLogo(folderId, imageData, -1, OriginalLogoSize, false, out _);
        }

        public string SaveOrUpdateLogo<T>(T folderId, byte[] imageData, long maxFileSize, Size imageSize, bool saveInCoreContext, out string fileName)
        {
            imageData = ImageHelper.ParseImage(imageData, maxFileSize, imageSize, out var imageFormat, out var width, out var height);

            var imageExtension = CommonPhotoManager.GetImgFormatName(imageFormat);

            fileName = $"{ProcessFolderId(folderId)}\\orig_{width}-{height}.{imageExtension}";

            if (imageData == null && imageData.Length == 0) return string.Empty;

            using var stream = new MemoryStream(imageData);
            var path = Store.Save(fileName, stream).ToString();

            ResizeLogo(folderId, imageData, maxFileSize, BigLogoSize, true);
            ResizeLogo(folderId, imageData, maxFileSize, MediumLogoSize, true);
            ResizeLogo(folderId, imageData, maxFileSize, SmallLogoSize, true);

            Cache.Remove(_keyPattern);

            return path;
        }

        public void DeleteLogo<T>(T folderId)
        {
            Store.DeleteDirectory(ProcessFolderId(folderId));
            Cache.Remove(_keyPattern);
        }

        public void ResizeLogo<T>(T folderId, byte[] imageData, long maxFileSize, Size size, bool now)
        {
            if (imageData == null || imageData.Length <= 0) throw new Web.Core.Users.UnknownImageFormatException();
            if (maxFileSize != -1 && imageData.Length > maxFileSize) throw new ImageWeightLimitException();

            var resizeTask = new LogoResizeWorkerItem<T>(folderId, imageData, maxFileSize, size, Store, SettingsManager.LoadForTenant<RoomLogoSettings>(TenantId));
            var key = $"{folderId}{size}";
            resizeTask.SetProperty("key", key);

            if (now)
            {
                ResizeImage(resizeTask);
            }
            else
            {
                if (!ResizeQueue.GetTasks<ResizeWorkerItem>().Any(t => t.GetProperty<string>("key") == key))
                {
                    ResizeQueue.QueueTask((a, b) => ResizeImage(resizeTask));
                }
            }
        }

        private void ResizeImage<T>(LogoResizeWorkerItem<T> item)
        {
            try
            {
                var data = item.Data;
                using var stream = new MemoryStream(data);
                using var img = Image.Load(stream, out var format);
                var imgFormat = format;
                if (item.Size != img.Size())
                {
                    using var img2 = item.Settings.IsDefault ?
                        CommonPhotoManager.DoThumbnail(img, item.Size, true, true, true) :
                        ImageHelper.GetImage(img, item.Size, item.Settings);
                    data = CommonPhotoManager.SaveToBytes(img2);
                }
                else
                {
                    data = CommonPhotoManager.SaveToBytes(img);
                }

                var extension = CommonPhotoManager.GetImgFormatName(imgFormat);
                var fileName = string.Format(LOGO_PATH, ProcessFolderId(item.FolderId), item.Size.Width, item.Size.Height, extension);

                using var stream2 = new MemoryStream(data);
                item.DataStore.Save(fileName, stream2).ToString();
            }
            catch (ArgumentException error)
            {
                throw new Web.Core.Users.UnknownImageFormatException(error);
            }
        }

        public string GetBigLogoUrl<T>(T folderId)
        {
            return GetSizedLogoUrl(folderId, BigLogoSize);
        }

        public string GetMediumLogoUrl<T>(T folderId)
        {
            return GetSizedLogoUrl(folderId, MediumLogoSize);
        }

        public string GetSmallLogoUrl<T>(T folderId)
        {
            return GetSizedLogoUrl(folderId, SmallLogoSize);
        }

        public string GetSizedLogoUrl<T>(T folderId, Size size)
        {
            var path = Cache.Get<string>(GetKey(folderId, size));

            if (!string.IsNullOrEmpty(path)) return path;

            var logoPath = GetAllSizedLogos(folderId)
                .Where(f => f.Contains($"size_{size.Width}-{size.Height}")).FirstOrDefault();

            if (string.IsNullOrEmpty(logoPath)) return string.Empty;

            Cache.Insert(GetKey(folderId, size), logoPath, _cacheItemLifeTime);

            return logoPath;
        }

        public IEnumerable<string> GetAllSizedLogos<T>(T folderId)
        {
            return Store.ListFiles(ProcessFolderId(folderId), string.Empty, false)
                .Select(u => u.ToString());
        }

        private string GetKey<T>(T folderId, Size size)
        {
            return $"{TenantId}/{folderId}/{size.Width}/{size.Height}";
        }

        private string ProcessFolderId<T>(T folderId)
        {
            if (folderId == null) throw new ArgumentException(null, nameof(folderId));

            return folderId.GetType() != typeof(string)
                ? folderId.ToString()
                : folderId.ToString()?.Replace("-", "");
        }
    }

    public class LogoResizeWorkerItemExtension
    {
        public static void Register(DIHelper services)
        {
            services.AddDistributedTaskQueueService<ResizeWorkerItem>(2);
        }
    }
}
