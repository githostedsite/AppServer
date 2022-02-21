﻿using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.People.Api;

public class ThirdpartyController : BaseApiController
{
    private readonly IOptionsSnapshot<AccountLinker> _accountLinker;
    private readonly CookiesManager _cookiesManager;
    private readonly CoreBaseSettings _coreBaseSettings;
    private readonly DisplayUserSettingsHelper _displayUserSettingsHelper;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly InstanceCrypto _instanceCrypto;
    private readonly MobileDetector _mobileDetector;
    private readonly PersonalSettingsHelper _personalSettingsHelper;
    private readonly ProviderManager _providerManager;
    private readonly Signature _signature;
    private readonly TenantExtra _tenantExtra;
    private readonly UserHelpTourHelper _userHelpTourHelper;
    private readonly UserManagerWrapper _userManagerWrapper;
    private readonly UserPhotoManager _userPhotoManager;

    public ThirdpartyController(
        UserManager userManager,
        AuthContext authContext,
        ApiContext apiContext,
        PermissionContext permissionContext,
        SecurityContext securityContext,
        MessageService messageService,
        MessageTarget messageTarget,
        StudioNotifyService studioNotifyService,
        IOptionsSnapshot<AccountLinker> optionsSnapshot,
        CookiesManager cookiesManager,
        CoreBaseSettings coreBaseSettings,
        DisplayUserSettingsHelper displayUserSettingsHelper,
        IHttpClientFactory httpClientFactory,
        InstanceCrypto instanceCrypto,
        MobileDetector mobileDetector,
        PersonalSettingsHelper personalSettingsHelper,
        ProviderManager providerManager,
        Signature signature,
        TenantExtra tenantExtra,
        UserHelpTourHelper userHelpTourHelper,
        UserManagerWrapper userManagerWrapper,
        UserPhotoManager userPhotoManager
        ) 
        : base(
            userManager,
            authContext,
            apiContext, 
            permissionContext,
            securityContext,
            messageService,
            messageTarget,
            studioNotifyService)
    {
        _accountLinker = optionsSnapshot;
        _cookiesManager = cookiesManager;
        _coreBaseSettings = coreBaseSettings;
        _displayUserSettingsHelper = displayUserSettingsHelper;
        _httpClientFactory = httpClientFactory;
        _instanceCrypto = instanceCrypto;
        _mobileDetector = mobileDetector;
        _personalSettingsHelper = personalSettingsHelper;
        _providerManager = providerManager;
        _signature = signature;
        _tenantExtra = tenantExtra;
        _userHelpTourHelper = userHelpTourHelper;
        _userManagerWrapper = userManagerWrapper;
        _userPhotoManager = userPhotoManager;
    }

    [AllowAnonymous]
    [Read("thirdparty/providers")]
    public ICollection<AccountInfoDto> GetAuthProviders(bool inviteView, bool settingsView, string clientCallback, string fromOnly)
    {
        ICollection<AccountInfoDto> infos = new List<AccountInfoDto>();
        IEnumerable<LoginProfile> linkedAccounts = new List<LoginProfile>();

        if (AuthContext.IsAuthenticated)
        {
            linkedAccounts = _accountLinker.Get("webstudio").GetLinkedProfiles(AuthContext.CurrentAccount.ID.ToString());
        }

        fromOnly = string.IsNullOrWhiteSpace(fromOnly) ? string.Empty : fromOnly.ToLower();

        foreach (var provider in ProviderManager.AuthProviders.Where(provider => string.IsNullOrEmpty(fromOnly) || fromOnly == provider || (provider == "google" && fromOnly == "openid")))
        {
            if (inviteView && provider.Equals("twitter", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            var loginProvider = _providerManager.GetLoginProvider(provider);
            if (loginProvider != null && loginProvider.IsEnabled)
            {

                var url = VirtualPathUtility.ToAbsolute("~/login.ashx") + $"?auth={provider}";
                var mode = settingsView || inviteView || (!_mobileDetector.IsMobile() && !Request.DesktopApp())
                        ? $"&mode=popup&callback={clientCallback}"
                        : "&mode=Redirect&desktop=true";

                infos.Add(new AccountInfoDto
                {
                    Linked = linkedAccounts.Any(x => x.Provider == provider),
                    Provider = provider,
                    Url = url + mode
                });
            }
        }

        return infos;
    }

    [Update("thirdparty/linkaccount")]
    public void LinkAccountFromBody([FromBody] LinkAccountRequestDto model)
    {
        LinkAccount(model);
    }

    [Update("thirdparty/linkaccount")]
    [Consumes("application/x-www-form-urlencoded")]
    public void LinkAccountFromForm([FromForm] LinkAccountRequestDto model)
    {
        LinkAccount(model);
    }

    [AllowAnonymous]
    [Create("thirdparty/signup")]
    public void SignupAccountFromBody([FromBody] SignupAccountModel model)
    {
        SignupAccount(model);
    }

    [AllowAnonymous]
    [Create("thirdparty/signup")]
    [Consumes("application/x-www-form-urlencoded")]
    public void SignupAccountFromForm([FromForm] SignupAccountModel model)
    {
        SignupAccount(model);
    }

    [Delete("thirdparty/unlinkaccount")]
    public void UnlinkAccount(string provider)
    {
        GetLinker().RemoveProvider(SecurityContext.CurrentAccount.ID.ToString(), provider);
        MessageService.Send(MessageAction.UserUnlinkedSocialAccount, GetMeaningfulProviderName(provider));
    }

    private static string GetMeaningfulProviderName(string providerName)
    {
        switch (providerName)
        {
            case "google":
            case "openid":
                return "Google";
            case "facebook":
                return "Facebook";
            case "twitter":
                return "Twitter";
            case "linkedin":
                return "LinkedIn";
            default:
                return "Unknown Provider";
        }
    }

    private UserInfo CreateNewUser(string firstName, string lastName, string email, string passwordHash, EmployeeType employeeType, bool fromInviteLink)
    {
        var isVisitor = employeeType == EmployeeType.Visitor;

        if (SetupInfo.IsSecretEmail(email))
        {
            fromInviteLink = false;
        }

        var userInfo = new UserInfo
        {
            FirstName = string.IsNullOrEmpty(firstName) ? UserControlsCommonResource.UnknownFirstName : firstName,
            LastName = string.IsNullOrEmpty(lastName) ? UserControlsCommonResource.UnknownLastName : lastName,
            Email = email,
        };

        if (_coreBaseSettings.Personal)
        {
            userInfo.ActivationStatus = EmployeeActivationStatus.Activated;
            userInfo.CultureName = _coreBaseSettings.CustomMode ? "ru-RU" : Thread.CurrentThread.CurrentUICulture.Name;
        }

        return _userManagerWrapper.AddUser(userInfo, passwordHash, true, true, isVisitor, fromInviteLink);
    }

    private string GetEmailAddress(SignupAccountModel model)
    {
        if (!string.IsNullOrEmpty(model.Email))
        {
            return model.Email.Trim();
        }

        return string.Empty;
    }

    private string GetEmailAddress(SignupAccountModel model, LoginProfile account)
    {
        var value = GetEmailAddress(model);

        return string.IsNullOrEmpty(value) ? account.EMail : value;
    }

    private string GetFirstName(SignupAccountModel model)
    {
        var value = string.Empty;
        if (!string.IsNullOrEmpty(model.FirstName))
        {
            value = model.FirstName.Trim();
        }

        return HtmlUtil.GetText(value);
    }

    private string GetFirstName(SignupAccountModel model, LoginProfile account)
    {
        var value = GetFirstName(model);

        return string.IsNullOrEmpty(value) ? account.FirstName : value;
    }

    private string GetLastName(SignupAccountModel model)
    {
        var value = string.Empty;
        if (!string.IsNullOrEmpty(model.LastName))
        {
            value = model.LastName.Trim();
        }

        return HtmlUtil.GetText(value);
    }

    private string GetLastName(SignupAccountModel model, LoginProfile account)
    {
        var value = GetLastName(model);

        return string.IsNullOrEmpty(value) ? account.LastName : value;
    }

    private AccountLinker GetLinker()
    {
        return _accountLinker.Get("webstudio");
    }

    private void LinkAccount(LinkAccountRequestDto model)
    {
        var profile = new LoginProfile(_signature, _instanceCrypto, model.SerializedProfile);

        if (!(_coreBaseSettings.Standalone || _tenantExtra.GetTenantQuota().Oauth))
        {
            throw new Exception("ErrorNotAllowedOption");
        }

        if (string.IsNullOrEmpty(profile.AuthorizationError))
        {
            GetLinker().AddLink(SecurityContext.CurrentAccount.ID.ToString(), profile);
            MessageService.Send(MessageAction.UserLinkedSocialAccount, GetMeaningfulProviderName(profile.Provider));
        }
        else
        {
            // ignore cancellation
            if (profile.AuthorizationError != "Canceled at provider")
            {
                throw new Exception(profile.AuthorizationError);
            }
        }
    }

    private void SaveContactImage(Guid userID, string url)
    {
        using (var memstream = new MemoryStream())
        {
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(url);

            var httpClient = _httpClientFactory.CreateClient();
            using (var response = httpClient.Send(request))
            using (var stream = response.Content.ReadAsStream())
            {
                var buffer = new byte[512];
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    memstream.Write(buffer, 0, bytesRead);
                }

                var bytes = memstream.ToArray();

                _userPhotoManager.SaveOrUpdatePhoto(userID, bytes);
            }
        }
    }

    private void SignupAccount(SignupAccountModel model)
    {
        var employeeType = model.EmplType ?? EmployeeType.User;
        var passwordHash = model.PasswordHash;
        var mustChangePassword = false;
        if (string.IsNullOrEmpty(passwordHash))
        {
            passwordHash = UserManagerWrapper.GeneratePassword();
            mustChangePassword = true;
        }

        var thirdPartyProfile = new LoginProfile(_signature, _instanceCrypto, model.SerializedProfile);
        if (!string.IsNullOrEmpty(thirdPartyProfile.AuthorizationError))
        {
            // ignore cancellation
            if (thirdPartyProfile.AuthorizationError != "Canceled at provider")
            {
                throw new Exception(thirdPartyProfile.AuthorizationError);
            }

            return;
        }

        if (string.IsNullOrEmpty(thirdPartyProfile.EMail))
        {
            throw new Exception(Resource.ErrorNotCorrectEmail);
        }

        var userID = Guid.Empty;
        try
        {
            SecurityContext.AuthenticateMeWithoutCookie(ASC.Core.Configuration.Constants.CoreSystem);
            var newUser = CreateNewUser(GetFirstName(model, thirdPartyProfile), GetLastName(model, thirdPartyProfile), GetEmailAddress(model, thirdPartyProfile), passwordHash, employeeType, false);
            var messageAction = employeeType == EmployeeType.User ? MessageAction.UserCreatedViaInvite : MessageAction.GuestCreatedViaInvite;
            MessageService.Send(MessageInitiator.System, messageAction, MessageTarget.Create(newUser.ID), newUser.DisplayUserName(false, _displayUserSettingsHelper));
            userID = newUser.ID;
            if (!string.IsNullOrEmpty(thirdPartyProfile.Avatar))
            {
                SaveContactImage(userID, thirdPartyProfile.Avatar);
            }

            GetLinker().AddLink(userID.ToString(), thirdPartyProfile);
        }
        finally
        {
            SecurityContext.Logout();
        }

        var user = UserManager.GetUsers(userID);
        var cookiesKey = SecurityContext.AuthenticateMe(user.Email, passwordHash);
        _cookiesManager.SetCookies(CookiesType.AuthKey, cookiesKey);
        MessageService.Send(MessageAction.LoginSuccess);
        StudioNotifyService.UserHasJoin();

        if (mustChangePassword)
        {
            StudioNotifyService.UserPasswordChange(user);
        }

        _userHelpTourHelper.IsNewUser = true;
        if (_coreBaseSettings.Personal)
        {
            _personalSettingsHelper.IsNewUser = true;
        }
    }
}
