namespace ASC.People.Api;

public class RemoveUserDataController : BaseApiController
{
    public RemoveUserDataController(UserManager userManager,
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
        : base(userManager,
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

    [Read(@"remove/progress")]
    public RemoveProgressItem GetRemoveProgress(Guid userId)
    {
        PermissionContext.DemandPermissions(Constants.Action_EditUser);

        return QueueWorkerRemove.GetProgressItemStatus(Tenant.TenantId, userId);
    }

    [Create(@"remove/start")]
    public RemoveProgressItem StartRemoveFromBody([FromBody] TerminateRequestDto model)
    {
        return StartRemove(model);
    }

    [Create(@"remove/start")]
    [Consumes("application/x-www-form-urlencoded")]
    public RemoveProgressItem StartRemoveFromForm([FromForm] TerminateRequestDto model)
    {
        return StartRemove(model);
    }

    [Update(@"remove/terminate")]
    public void TerminateRemoveFromBody([FromBody] TerminateRequestDto model)
{
        PermissionContext.DemandPermissions(Constants.Action_EditUser);

        QueueWorkerRemove.Terminate(Tenant.TenantId, model.UserId);
    }

    [Update(@"remove/terminate")]
    [Consumes("application/x-www-form-urlencoded")]
    public void TerminateRemoveFromForm([FromForm] TerminateRequestDto model)
    {
        PermissionContext.DemandPermissions(Constants.Action_EditUser);

        QueueWorkerRemove.Terminate(Tenant.TenantId, model.UserId);
    }

    private RemoveProgressItem StartRemove(TerminateRequestDto model)
    {
        PermissionContext.DemandPermissions(Constants.Action_EditUser);

        var user = UserManager.GetUsers(model.UserId);

        if (user == null || user.ID == Constants.LostUser.ID)
        {
            throw new ArgumentException("User with id = " + model.UserId + " not found");
        }

        if (user.IsOwner(Tenant) || user.IsMe(AuthContext) || user.Status != EmployeeStatus.Terminated)
        {
            throw new ArgumentException("Can not delete user with id = " + model.UserId);
        }

        return QueueWorkerRemove.Start(Tenant.TenantId, user, SecurityContext.CurrentAccount.ID, true);
    }
}
