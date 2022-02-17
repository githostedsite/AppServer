using Module = ASC.Api.Core.Module;

namespace ASC.People.Api;

public class InfoController : BaseApiController
{
    public InfoController(UserManager userManager,
        AuthContext authContext,
        ApiContext apiContext,
        PermissionContext permissionContext,
        Core.SecurityContext securityContext,
        DisplayUserSettingsHelper displayUserSettingsHelper,
        QueueWorkerReassign queueWorkerReassign,
        QueueWorkerRemove queueWorkerRemove,
        EmployeeWraperFullHelper employeeWraperFullHelper,
        UserPhotoManager userPhotoManager,
        SettingsManager settingsManager,
        MessageService messageService,
        MessageTarget messageTarget,
        IHttpClientFactory httpClientFactory,
        SetupInfo setupInfo,
        IOptionsMonitor<ILog> option,
        StudioNotifyService studioNotifyService,
        TenantExtra tenantExtra,
        CoreBaseSettings coreBaseSettings,
        CookiesManager cookiesManager,
        UserManagerWrapper userManagerWrapper) 
        : base(
            userManager,
            authContext,
            apiContext,
            permissionContext,
            securityContext,
            displayUserSettingsHelper,
            queueWorkerReassign,
            queueWorkerRemove,
            employeeWraperFullHelper,
            userPhotoManager,
            settingsManager,
            messageService,
            messageTarget,
            httpClientFactory,
            setupInfo,
            option,
            studioNotifyService,
            tenantExtra,
            coreBaseSettings,
            cookiesManager,
            userManagerWrapper)
    {
    }

    [Read("info")]
    public Module GetModule()
    {
        var product = new PeopleProduct();
        product.Init();

        return new Module(product);
    }


    [Update("self/delete")]
    public object SendInstructionsToDelete()
    {
        var user = UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

        if (user.IsLDAP())
        {
            throw new SecurityException();
        }

        StudioNotifyService.SendMsgProfileDeletion(user);
        MessageService.Send(MessageAction.UserSentDeleteInstructions);

        return string.Format(Resource.SuccessfullySentNotificationDeleteUserInfoMessage, "<b>" + user.Email + "</b>");
    }
}
