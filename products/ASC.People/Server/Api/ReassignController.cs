﻿using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.People.Api;

public class ReassignController : BaseApiController
{
    public ReassignController(
        UserManager userManager,
        AuthContext authContext,
        ApiContext apiContext,
        PermissionContext permissionContext,
        SecurityContext securityContext,
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

    [Read(@"reassign/progress")]
    public ReassignProgressItem GetReassignProgress(Guid userId)
    {
        PermissionContext.DemandPermissions(Constants.Action_EditUser);

        return QueueWorkerReassign.GetProgressItemStatus(Tenant.TenantId, userId);
    }

    [Create(@"reassign/start")]
    public ReassignProgressItem StartReassignFromBody([FromBody] StartReassignRequestDto model)
    {
        return StartReassign(model);
    }

    [Create(@"reassign/start")]
    [Consumes("application/x-www-form-urlencoded")]
    public ReassignProgressItem StartReassignFromForm([FromForm] StartReassignRequestDto model)
    {
        return StartReassign(model);
    }

    [Update(@"reassign/terminate")]
    public void TerminateReassignFromBody([FromBody] TerminateRequestDto model)
    {
        PermissionContext.DemandPermissions(Constants.Action_EditUser);

        QueueWorkerReassign.Terminate(Tenant.TenantId, model.UserId);
    }

    [Update(@"reassign/terminate")]
    [Consumes("application/x-www-form-urlencoded")]
    public void TerminateReassignFromForm([FromForm] TerminateRequestDto model)
    {
        PermissionContext.DemandPermissions(Constants.Action_EditUser);

        QueueWorkerReassign.Terminate(Tenant.TenantId, model.UserId);
    }

    private ReassignProgressItem StartReassign(StartReassignRequestDto model)
    {
        PermissionContext.DemandPermissions(Constants.Action_EditUser);

        var fromUser = UserManager.GetUsers(model.FromUserId);

        if (fromUser == null || fromUser.ID == Constants.LostUser.ID)
        {
            throw new ArgumentException("User with id = " + model.FromUserId + " not found");
        }

        if (fromUser.IsOwner(Tenant) || fromUser.IsMe(AuthContext) || fromUser.Status != EmployeeStatus.Terminated)
        {
            throw new ArgumentException("Can not delete user with id = " + model.FromUserId);
        }

        var toUser = UserManager.GetUsers(model.ToUserId);

        if (toUser == null || toUser.ID == Constants.LostUser.ID)
        {
            throw new ArgumentException("User with id = " + model.ToUserId + " not found");
        }

        if (toUser.IsVisitor(UserManager) || toUser.Status == EmployeeStatus.Terminated)
        {
            throw new ArgumentException("Can not reassign data to user with id = " + model.ToUserId);
        }

        return QueueWorkerReassign.Start(Tenant.TenantId, model.FromUserId, model.ToUserId, SecurityContext.CurrentAccount.ID, model.DeleteProfile);
    }
}