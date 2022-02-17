using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.People.Api;

[Scope(Additional = typeof(BaseLoginProviderExtension))]
[DefaultRoute]
[ApiController]
[ControllerName("people")]
public abstract class BaseApiController : ControllerBase
{
    protected Tenant Tenant => ApiContext.Tenant;

    protected readonly UserManager UserManager;
    protected readonly AuthContext AuthContext;
    protected readonly ApiContext ApiContext;
    protected readonly PermissionContext PermissionContext;
    protected readonly SecurityContext SecurityContext;
    protected readonly DisplayUserSettingsHelper DisplayUserSettingsHelper;
    protected readonly QueueWorkerReassign QueueWorkerReassign;
    protected readonly QueueWorkerRemove QueueWorkerRemove;
    protected readonly EmployeeWraperFullHelper EmployeeWraperFullHelper;
    protected readonly UserPhotoManager UserPhotoManager;
    protected readonly SettingsManager SettingsManager;
    protected readonly MessageService MessageService;
    protected readonly MessageTarget MessageTarget;
    protected readonly IHttpClientFactory HttpClientFactory;
    protected readonly SetupInfo SetupInfo;
    protected readonly ILog Logger;
    protected readonly StudioNotifyService StudioNotifyService;
    protected readonly TenantExtra TenantExtra;
    protected readonly CoreBaseSettings CoreBaseSettings;
    protected readonly CookiesManager CookiesManager;
    protected readonly UserManagerWrapper UserManagerWrapper;

    public BaseApiController(
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
    {
        UserManager = userManager;
        AuthContext = authContext;
        ApiContext = apiContext;
        PermissionContext = permissionContext;
        SecurityContext = securityContext;
        DisplayUserSettingsHelper = displayUserSettingsHelper;
        QueueWorkerReassign = queueWorkerReassign;
        QueueWorkerRemove = queueWorkerRemove;
        EmployeeWraperFullHelper = employeeWraperFullHelper;
        UserPhotoManager = userPhotoManager;
        SettingsManager = settingsManager;
        MessageService = messageService;
        MessageTarget = messageTarget;
        HttpClientFactory = httpClientFactory;
        SetupInfo = setupInfo;
        Logger = option.Get("ASC.Api");
        StudioNotifyService = studioNotifyService;
        TenantExtra = tenantExtra;
        CoreBaseSettings = coreBaseSettings;
        CookiesManager = cookiesManager;
        UserManagerWrapper = userManagerWrapper;
    }

    protected void CheckReassignProccess(IEnumerable<Guid> userIds)
    {
        foreach (var userId in userIds)
{
            var reassignStatus = QueueWorkerReassign.GetProgressItemStatus(Tenant.TenantId, userId);
            if (reassignStatus == null || reassignStatus.IsCompleted)
            {
                continue;
            }

            var userName = UserManager.GetUsers(userId).DisplayUserName(DisplayUserSettingsHelper);

            throw new Exception(string.Format(Resource.ReassignDataRemoveUserError, userName));
        }
    }

    protected UserInfo GetUserInfo(string userNameOrId)
    {
        UserInfo user;
        try
        {
            var userId = new Guid(userNameOrId);
            user = UserManager.GetUsers(userId);
        }
        catch (FormatException)
        {
            user = UserManager.GetUserByUserName(userNameOrId);
        }

        if (user == null || user.ID == Constants.LostUser.ID)
        {
            throw new ItemNotFoundException("user not found");
        }

        return user;
    }

    protected void UpdateContacts(IEnumerable<Contact> contacts, UserInfo user)
    {
        PermissionContext.DemandPermissions(new UserSecurityProvider(user.ID), Constants.Action_EditUser);

        if (contacts == null)
        {
            return;
        }

        var values = contacts.Where(r => !string.IsNullOrEmpty(r.Value)).Select(r => $"{r.Type}|{r.Value}");
        user.Contacts = string.Join('|', values);
    }

    protected void UpdatePhotoUrl(string files, UserInfo user)
    {
        if (string.IsNullOrEmpty(files))
        {
            return;
        }

        PermissionContext.DemandPermissions(new UserSecurityProvider(user.ID), Constants.Action_EditUser);

        if (!files.StartsWith("http://") && !files.StartsWith("https://"))
        {
            files = new Uri(ApiContext.HttpContextAccessor.HttpContext.Request.GetDisplayUrl()).GetLeftPart(UriPartial.Authority) + "/" + files.TrimStart('/');
        }
        var request = new HttpRequestMessage();
        request.RequestUri = new Uri(files);

        var httpClient = HttpClientFactory.CreateClient();
        using var response = httpClient.Send(request);
        using var inputStream = response.Content.ReadAsStream();
        using var br = new BinaryReader(inputStream);
        var imageByteArray = br.ReadBytes((int)inputStream.Length);
        UserPhotoManager.SaveOrUpdatePhoto(user.ID, imageByteArray);
    }
}