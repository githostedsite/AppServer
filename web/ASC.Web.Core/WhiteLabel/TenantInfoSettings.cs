/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.Json.Serialization;

using ASC.Common;
using ASC.Common.Caching;
using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.Data.Storage;
using ASC.Web.Core.Utility.Skins;

using Microsoft.Extensions.Configuration;

namespace ASC.Web.Core.WhiteLabel
{
    [Serializable]
    public class TenantInfoSettings : ISettings, ICacheWrapped<CachedTenantInfoSettings>
    {
        [JsonPropertyName("LogoSize")]
        public Size CompanyLogoSize { get; internal set; }

        [JsonPropertyName("LogoFileName")]
        public string CompanyLogoFileName { get; set; }

        [JsonPropertyName("Default")]
        internal bool IsDefault { get; set; }

        public ISettings GetDefault(IServiceProvider serviceProvider)
        {
            return new TenantInfoSettings()
            {
                IsDefault = true
            };
        }

        public CachedTenantInfoSettings WrapIn()
        {
            return new CachedTenantInfoSettings
            {
                CompanyLogoFileName = this.CompanyLogoFileName,
                Width = CompanyLogoSize.Width,
                Height = CompanyLogoSize.Height,
                IsDefault = this.IsDefault
            };
        }

        public Guid ID
        {
            get { return new Guid("{5116B892-CCDD-4406-98CD-4F18297C0C0A}"); }
        }
    }

    public partial class CachedTenantInfoSettings : ICustomSer<CachedTenantInfoSettings>,
        ICacheWrapped<TenantInfoSettings>
    {
        public void CustomDeSer() { }
        public void CustomSer() { }

        public TenantInfoSettings WrapIn()
        {
            return new TenantInfoSettings
            {
                CompanyLogoFileName = this.CompanyLogoFileName,
                CompanyLogoSize = new Size(Width, Height),
                IsDefault = this.IsDefault
            };
        }
    }

    [Scope]
    public class TenantInfoSettingsHelper
    {
        private WebImageSupplier WebImageSupplier { get; }
        private StorageFactory StorageFactory { get; }
        private TenantManager TenantManager { get; }
        private IConfiguration Configuration { get; }

        public TenantInfoSettingsHelper(
            WebImageSupplier webImageSupplier,
            StorageFactory storageFactory,
            TenantManager tenantManager,
            IConfiguration configuration)
        {
            WebImageSupplier = webImageSupplier;
            StorageFactory = storageFactory;
            TenantManager = tenantManager;
            Configuration = configuration;
        }
        public void RestoreDefault(TenantInfoSettings tenantInfoSettings, TenantLogoManager tenantLogoManager)
        {
            RestoreDefaultTenantName();
            RestoreDefaultLogo(tenantInfoSettings, tenantLogoManager);
        }

        public void RestoreDefaultTenantName()
        {
            var currentTenant = TenantManager.GetCurrentTenant();
            currentTenant.Name = Configuration["web:portal-name"] ?? "Cloud Office Applications";
            TenantManager.SaveTenant(currentTenant);
        }

        public void RestoreDefaultLogo(TenantInfoSettings tenantInfoSettings, TenantLogoManager tenantLogoManager)
        {
            tenantInfoSettings.IsDefault = true;

            var store = StorageFactory.GetStorage(TenantManager.GetCurrentTenant().TenantId.ToString(), "logo");
            try
            {
                store.DeleteFiles("", "*", false);
            }
            catch
            {
            }
            tenantInfoSettings.CompanyLogoSize = default;

            tenantLogoManager.RemoveMailLogoDataFromCache();
        }

        public void SetCompanyLogo(string companyLogoFileName, byte[] data, TenantInfoSettings tenantInfoSettings, TenantLogoManager tenantLogoManager)
        {
            var store = StorageFactory.GetStorage(TenantManager.GetCurrentTenant().TenantId.ToString(), "logo");

            if (!tenantInfoSettings.IsDefault)
            {
                try
                {
                    store.DeleteFiles("", "*", false);
                }
                catch
                {
                }
            }
            using (var memory = new MemoryStream(data))
            using (var image = Image.FromStream(memory))
            {
                tenantInfoSettings.CompanyLogoSize = image.Size;
                memory.Seek(0, SeekOrigin.Begin);
                store.Save(companyLogoFileName, memory);
                tenantInfoSettings.CompanyLogoFileName = companyLogoFileName;
            }
            tenantInfoSettings.IsDefault = false;

            tenantLogoManager.RemoveMailLogoDataFromCache();
        }

        public string GetAbsoluteCompanyLogoPath(TenantInfoSettings tenantInfoSettings)
        {
            if (tenantInfoSettings.IsDefault)
            {
                return WebImageSupplier.GetAbsoluteWebPath("logo/dark_general.png");
            }

            var store = StorageFactory.GetStorage(TenantManager.GetCurrentTenant().TenantId.ToString(), "logo");
            return store.GetUri(tenantInfoSettings.CompanyLogoFileName ?? "").ToString();
        }

        /// <summary>
        /// Get logo stream or null in case of default logo
        /// </summary>
        public Stream GetStorageLogoData(TenantInfoSettings tenantInfoSettings)
        {
            if (tenantInfoSettings.IsDefault) return null;

            var storage = StorageFactory.GetStorage(TenantManager.GetCurrentTenant().TenantId.ToString(CultureInfo.InvariantCulture), "logo");

            if (storage == null) return null;

            var fileName = tenantInfoSettings.CompanyLogoFileName ?? "";

            return storage.IsFile(fileName) ? storage.GetReadStream(fileName) : null;
        }
    }
}