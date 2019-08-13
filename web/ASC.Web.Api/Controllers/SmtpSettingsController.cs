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
using System.Diagnostics;
using System.Linq;
using ASC.Api.Core;
using ASC.Api.Settings.Smtp;
using ASC.Common.Logging;
using ASC.Common.Threading;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Configuration;
using ASC.Core.Tenants;
using ASC.MessagingSystem;
using ASC.Web.Api.Routing;
using ASC.Web.Core.PublicResources;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace ASC.Api.Settings
{
    [DefaultRoute]
    [ApiController]
    public class SmtpSettingsController : ControllerBase
    {
        private static DistributedTaskQueue SMTPTasks { get; } = new DistributedTaskQueue("smtpOperations");

        public Tenant Tenant { get { return ApiContext.Tenant; } }

        public ApiContext ApiContext { get; }

        public LogManager LogManager { get; }

        public MessageService MessageService { get; }

        public StudioNotifyService StudioNotifyService { get; }

        public IWebHostEnvironment WebHostEnvironment { get; }


        public SmtpSettingsController(LogManager logManager,
            MessageService messageService,
            StudioNotifyService studioNotifyService,
            ApiContext apiContext)
        {
            LogManager = logManager;
            MessageService = messageService;
            StudioNotifyService = studioNotifyService;
            ApiContext = apiContext;
        }


        [Read("smtp")]
        public SmtpSettingsWrapper GetSmtpSettings()
        {
            CheckSmtpPermissions();

            var settings = ToSmtpSettings(CoreContext.Configuration.SmtpSettings, true);

            return settings;
        }

        [Create("smtp")]
        public SmtpSettingsWrapper SaveSmtpSettings(SmtpSettingsWrapper smtpSettings)
        {
            CheckSmtpPermissions();

            //TODO: Add validation check

            if(smtpSettings == null)
                throw new ArgumentNullException("smtpSettings");

            SecurityContext.DemandPermissions(Tenant, SecutiryConstants.EditPortalSettings);

            var settingConfig = ToSmtpSettingsConfig(smtpSettings);

            CoreContext.Configuration.SmtpSettings = settingConfig;

            var settings = ToSmtpSettings(settingConfig, true);

            return settings;
        }

        [Delete("smtp")]
        public SmtpSettingsWrapper ResetSmtpSettings()
        {
            CheckSmtpPermissions();

            if (!CoreContext.Configuration.SmtpSettings.IsDefaultSettings)
            {
                SecurityContext.DemandPermissions(Tenant, SecutiryConstants.EditPortalSettings);
                CoreContext.Configuration.SmtpSettings = null;
            }

            var current = CoreContext.Configuration.Standalone ? CoreContext.Configuration.SmtpSettings : SmtpSettings.Empty;
            
            return ToSmtpSettings(current, true);
        }

        [Read("smtp/test")]
        public SmtpOperationStatus TestSmtpSettings()
        {
            CheckSmtpPermissions();

            var settings = ToSmtpSettings(CoreContext.Configuration.SmtpSettings);

            var smtpTestOp = new SmtpOperation(settings, Tenant.TenantId, SecurityContext.CurrentAccount.ID);

            SMTPTasks.QueueTask(smtpTestOp.RunJob, smtpTestOp.GetDistributedTask());

            return ToSmtpOperationStatus();
        }

        [Read("smtp/test/status")]
        public SmtpOperationStatus GetSmtpOperationStatus()
        {
            CheckSmtpPermissions();

            return ToSmtpOperationStatus();
        }

        private static SmtpOperationStatus ToSmtpOperationStatus()
        {
            var operations = SMTPTasks.GetTasks().ToList();

            foreach (var o in operations)
            {
                if (!string.IsNullOrEmpty(o.InstanseId) &&
                    Process.GetProcesses().Any(p => p.Id == int.Parse(o.InstanseId)))
                    continue;

                o.SetProperty(SmtpOperation.PROGRESS, 100);
                SMTPTasks.RemoveTask(o.Id);
            }

            var operation =
                operations
                    .FirstOrDefault(t => t.GetProperty<int>(SmtpOperation.OWNER) == TenantProvider.CurrentTenantID);

            if (operation == null)
            {
                return null;
            }

            if (DistributedTaskStatus.Running < operation.Status)
            {
                operation.SetProperty(SmtpOperation.PROGRESS, 100);
                SMTPTasks.RemoveTask(operation.Id);
            }

            var result = new SmtpOperationStatus
            {
                Id = operation.Id,
                Completed = operation.GetProperty<bool>(SmtpOperation.FINISHED),
                Percents = operation.GetProperty<int>(SmtpOperation.PROGRESS),
                Status = operation.GetProperty<string>(SmtpOperation.RESULT),
                Error = operation.GetProperty<string>(SmtpOperation.ERROR),
                Source = operation.GetProperty<string>(SmtpOperation.SOURCE)
            };

            return result;
        }

        public static SmtpSettings ToSmtpSettingsConfig(SmtpSettingsWrapper settingsWrapper)
        {
            var settingsConfig = new SmtpSettings(
                settingsWrapper.Host,
                settingsWrapper.Port ?? SmtpSettings.DefaultSmtpPort,
                settingsWrapper.SenderAddress,
                settingsWrapper.SenderDisplayName)
            {
                EnableSSL = settingsWrapper.EnableSSL,
                EnableAuth = settingsWrapper.EnableAuth
            };

            if (settingsWrapper.EnableAuth)
            {
                settingsConfig.SetCredentials(settingsWrapper.CredentialsUserName, settingsWrapper.CredentialsUserPassword);
            }

            return settingsConfig;
        }

        private static SmtpSettingsWrapper ToSmtpSettings(SmtpSettings settingsConfig, bool hidePassword = false)
        {
            return new SmtpSettingsWrapper
            {
                Host = settingsConfig.Host,
                Port = settingsConfig.Port,
                SenderAddress = settingsConfig.SenderAddress,
                SenderDisplayName = settingsConfig.SenderDisplayName,
                CredentialsUserName = settingsConfig.CredentialsUserName,
                CredentialsUserPassword = hidePassword ? "" : settingsConfig.CredentialsUserPassword,
                EnableSSL = settingsConfig.EnableSSL,
                EnableAuth = settingsConfig.EnableAuth
            };
        }

        private static void CheckSmtpPermissions()
        {
            if (!SetupInfo.IsVisibleSettings(ManagementType.SmtpSettings.ToString()))
            {
                throw new BillingException(Resource.ErrorNotAllowedOption, "Smtp");
            }
        }
    }
}
