using SecurityContext = ASC.Core.SecurityContext;
using Module = ASC.Api.Core.Module;

namespace ASC.Employee.Core.Controllers
{
    [Scope(Additional = typeof(BaseLoginProviderExtension))]
    [DefaultRoute]
    [ApiController]
    public class PeopleController : ControllerBase
    {
        public Tenant Tenant => _apiContext.Tenant;

        private readonly ApiContext _apiContext;
        private readonly MessageService _messageService;
        private readonly QueueWorkerReassign _queueWorkerReassign;
        private readonly QueueWorkerRemove _queueWorkerRemove;
        private readonly StudioNotifyService _studioNotifyService;
        private readonly UserManagerWrapper _userManagerWrapper;
        private readonly UserManager _userManager;
        private readonly TenantExtra _tenantExtra;
        private readonly TenantStatisticsProvider _tenantStatisticsProvider;
        private readonly UserPhotoManager _userPhotoManager;
        private readonly SecurityContext _securityContext;
        private readonly CookiesManager _cookiesManager;
        private readonly WebItemSecurity _webItemSecurity;
        private readonly PermissionContext _permissionContext;
        private readonly AuthContext _authContext;
        private readonly WebItemManager _webItemManager;
        private readonly CustomNamingPeople _customNamingPeople;
        private readonly TenantUtil _tenantUtil;
        private readonly CoreBaseSettings _coreBaseSettings;
        private readonly SetupInfo _setupInfo;
        private readonly FileSizeComment _fileSizeComment;
        private readonly DisplayUserSettingsHelper _displayUserSettingsHelper;
        private readonly Signature _signature;
        private readonly InstanceCrypto _instanceCrypto;
        private readonly WebItemSecurityCache _webItemSecurityCache;
        private readonly MessageTarget _messageTarget;
        private readonly SettingsManager _settingsManager;
        private readonly IOptionsSnapshot<AccountLinker> _accountLinker;
        private readonly EmployeeWraperFullHelper _employeeWraperFullHelper;
        private readonly EmployeeWraperHelper _employeeWraperHelper;
        private readonly UserFormatter _userFormatter;
        private readonly PasswordHasher _passwordHasher;
        private readonly UserHelpTourHelper _userHelpTourHelper;
        private readonly PersonalSettingsHelper _personalSettingsHelper;
        private readonly CommonLinkUtility _commonLinkUtility;
        private readonly MobileDetector _mobileDetector;
        private readonly ProviderManager _providerManager;
        private readonly Constants _constants;
        private readonly Recaptcha _recaptcha;
        private readonly ILog _logger;
        private readonly IHttpClientFactory _clientFactory;

        public PeopleController(
            MessageService messageService,
            QueueWorkerReassign queueWorkerReassign,
            QueueWorkerRemove queueWorkerRemove,
            StudioNotifyService studioNotifyService,
            UserManagerWrapper userManagerWrapper,
            ApiContext apiContext,
            UserManager userManager,
            TenantExtra tenantExtra,
            TenantStatisticsProvider tenantStatisticsProvider,
            UserPhotoManager userPhotoManager,
            SecurityContext securityContext,
            CookiesManager cookiesManager,
            WebItemSecurity webItemSecurity,
            PermissionContext permissionContext,
            AuthContext authContext,
            WebItemManager webItemManager,
            CustomNamingPeople customNamingPeople,
            TenantUtil tenantUtil,
            CoreBaseSettings coreBaseSettings,
            SetupInfo setupInfo,
            FileSizeComment fileSizeComment,
            DisplayUserSettingsHelper displayUserSettingsHelper,
            Signature signature,
            InstanceCrypto instanceCrypto,
            WebItemSecurityCache webItemSecurityCache,
            MessageTarget messageTarget,
            SettingsManager settingsManager,
            IOptionsMonitor<ILog> option,
            IOptionsSnapshot<AccountLinker> accountLinker,
            EmployeeWraperFullHelper employeeWraperFullHelper,
            EmployeeWraperHelper employeeWraperHelper,
            UserFormatter userFormatter,
            PasswordHasher passwordHasher,
            UserHelpTourHelper userHelpTourHelper,
            PersonalSettingsHelper personalSettingsHelper,
            CommonLinkUtility commonLinkUtility,
            MobileDetector mobileDetector,
            ProviderManager providerManager,
            Constants constants,
            Recaptcha recaptcha,
            IHttpClientFactory clientFactory
            )
        {
            _logger = option.Get("ASC.Api");
            _messageService = messageService;
            _queueWorkerReassign = queueWorkerReassign;
            _queueWorkerRemove = queueWorkerRemove;
            _studioNotifyService = studioNotifyService;
            _userManagerWrapper = userManagerWrapper;
            _apiContext = apiContext;
            _userManager = userManager;
            _tenantExtra = tenantExtra;
            _tenantStatisticsProvider = tenantStatisticsProvider;
            _userPhotoManager = userPhotoManager;
            _securityContext = securityContext;
            _cookiesManager = cookiesManager;
            _webItemSecurity = webItemSecurity;
            _permissionContext = permissionContext;
            _authContext = authContext;
            _webItemManager = webItemManager;
            _customNamingPeople = customNamingPeople;
            _tenantUtil = tenantUtil;
            _coreBaseSettings = coreBaseSettings;
            _setupInfo = setupInfo;
            _fileSizeComment = fileSizeComment;
            _displayUserSettingsHelper = displayUserSettingsHelper;
            _signature = signature;
            _instanceCrypto = instanceCrypto;
            _webItemSecurityCache = webItemSecurityCache;
            _messageTarget = messageTarget;
            _settingsManager = settingsManager;
            _accountLinker = accountLinker;
            _employeeWraperFullHelper = employeeWraperFullHelper;
            _employeeWraperHelper = employeeWraperHelper;
            _userFormatter = userFormatter;
            _passwordHasher = passwordHasher;
            _userHelpTourHelper = userHelpTourHelper;
            _personalSettingsHelper = personalSettingsHelper;
            _commonLinkUtility = commonLinkUtility;
            _mobileDetector = mobileDetector;
            _providerManager = providerManager;
            _constants = constants;
            _recaptcha = recaptcha;
            _clientFactory = clientFactory;
        }

        [Read("info")]
        public Module GetModule()
        {
            var product = new PeopleProduct();
            product.Init();

            return new Module(product);
        }

        [Read]
        public IQueryable<EmployeeWraper> GetAll()
        {
            return GetByStatus(EmployeeStatus.Active);
        }

        [Read("status/{status}")]
        public IQueryable<EmployeeWraper> GetByStatus(EmployeeStatus status)
        {
            if (_coreBaseSettings.Personal)
            {
                throw new Exception("Method not available");
            }

            Guid? groupId = null;
            if ("group".Equals(_apiContext.FilterBy, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(_apiContext.FilterValue))
            {
                groupId = new Guid(_apiContext.FilterValue);
                _apiContext.SetDataFiltered();
            }

            return GetFullByFilter(status, groupId, null, null, null);
        }

        [Read("@self")]
        public EmployeeWraper Self()
        {
            return _employeeWraperFullHelper.GetFull(_userManager.GetUser(_securityContext.CurrentAccount.ID, EmployeeWraperFullHelper.GetExpression(_apiContext)));
        }

        [Read("email")]
        public EmployeeWraperFull GetByEmail([FromQuery] string email)
        {
            if (_coreBaseSettings.Personal && !_userManager.GetUsers(_securityContext.CurrentAccount.ID).IsOwner(Tenant))
            {
                throw new MethodAccessException("Method not available");
            }

            var user = _userManager.GetUserByEmail(email);
            if (user.ID == Constants.LostUser.ID)
            {
                throw new ItemNotFoundException("User not found");
            }

            return _employeeWraperFullHelper.GetFull(user);
        }

        [Read("{username}", order: int.MaxValue)]
        public EmployeeWraperFull GetById(string username)
        {
            if (_coreBaseSettings.Personal)
            {
                throw new MethodAccessException("Method not available");
            }

            var user = _userManager.GetUserByUserName(username);
            if (user.ID == Constants.LostUser.ID)
            {
                if (Guid.TryParse(username, out var userId))
                {
                    user = _userManager.GetUsers(userId);
                }
                else
                {
                    _logger.Error(string.Format("Account {0} сould not get user by name {1}", _securityContext.CurrentAccount.ID, username));
                }
            }

            if (user.ID == Constants.LostUser.ID)
            {
                throw new ItemNotFoundException("User not found");
            }

            return _employeeWraperFullHelper.GetFull(user);
        }

        [Read("@search/{query}")]
        public IEnumerable<EmployeeWraperFull> GetSearch(string query)
        {
            if (_coreBaseSettings.Personal)
            {
                throw new MethodAccessException("Method not available");
            }

            try
            {
                var groupId = Guid.Empty;
                if ("group".Equals(_apiContext.FilterBy, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(_apiContext.FilterValue))
                {
                    groupId = new Guid(_apiContext.FilterValue);
                }

                return _userManager.Search(query, EmployeeStatus.Active, groupId).Select(_employeeWraperFullHelper.GetFull);
            }
            catch (Exception error)
            {
                _logger.Error(error);
            }

            return null;
        }

        [Read("search")]
        public IEnumerable<EmployeeWraperFull> GetPeopleSearch([FromQuery] string query)
        {
            return GetSearch(query);
        }

        [Read("status/{status}/search")]
        public IEnumerable<EmployeeWraperFull> GetAdvanced(EmployeeStatus status, [FromQuery] string query)
        {
            if (_coreBaseSettings.Personal)
            {
                throw new MethodAccessException("Method not available");
            }

            try
            {
                var list = _userManager.GetUsers(status).AsEnumerable();

                if ("group".Equals(_apiContext.FilterBy, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(_apiContext.FilterValue))
                {
                    var groupId = new Guid(_apiContext.FilterValue);
                    //Filter by group
                    list = list.Where(x => _userManager.IsUserInGroup(x.ID, groupId));
                    _apiContext.SetDataFiltered();
                }

                list = list.Where(x => x.FirstName != null && x.FirstName.IndexOf(query, StringComparison.OrdinalIgnoreCase) > -1 || (x.LastName != null && x.LastName.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1) ||
                                       (x.UserName != null && x.UserName.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1) || (x.Email != null && x.Email.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1) || (x.ContactsList != null && x.ContactsList.Any(y => y.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1)));

                return list.Select(_employeeWraperFullHelper.GetFull);
            }
            catch (Exception error)
            {
                _logger.Error(error);
            }

            return null;
        }

        ///// <summary>
        ///// Adds a new portal user from import with the first and last name, email address
        ///// </summary>
        ///// <short>
        ///// Add new import user
        ///// </short>
        ///// <param name="userList">The list of users to add</param>
        ///// <param name="importUsersAsCollaborators" optional="true">Add users as guests (bool type: false|true)</param>
        ///// <returns>Newly created users</returns>
        //[Create("import/save")]
        //public void SaveUsers(string userList, bool importUsersAsCollaborators)
        //{
        //    lock (progressQueue.SynchRoot)
        //    {
        //        var task = progressQueue.GetItems().OfType<ImportUsersTask>().FirstOrDefault(t => (int)t.Id == TenantProvider.CurrentTenantID);
        //        if (task != null && task.IsCompleted)
        //        {
        //            progressQueue.Remove(task);
        //            task = null;
        //        }
        //        if (task == null)
        //        {
        //            progressQueue.Add(new ImportUsersTask(userList, importUsersAsCollaborators, GetHttpHeaders(HttpContext.Current.Request))
        //            {
        //                Id = TenantProvider.CurrentTenantID,
        //                UserId = SecurityContext.CurrentAccount.ID,
        //                Percentage = 0
        //            });
        //        }
        //    }
        //}

        //[Read("import/status")]
        //public object GetStatus()
        //{
        //    lock (progressQueue.SynchRoot)
        //    {
        //        var task = progressQueue.GetItems().OfType<ImportUsersTask>().FirstOrDefault(t => (int)t.Id == TenantProvider.CurrentTenantID);
        //        if (task == null) return null;

        //        return new
        //        {
        //            Completed = task.IsCompleted,
        //            Percents = (int)task.Percentage,
        //            UserCounter = task.GetUserCounter,
        //            Status = (int)task.Status,
        //            Error = (string)task.Error,
        //            task.Data
        //        };
        //    }
        //}


        [Read("filter")]
        public IQueryable<EmployeeWraperFull> GetFullByFilter(EmployeeStatus? employeeStatus, Guid? groupId, EmployeeActivationStatus? activationStatus, EmployeeType? employeeType, bool? isAdministrator)
        {
            var users = GetByFilter(employeeStatus, groupId, activationStatus, employeeType, isAdministrator);

            return users.Select(r => _employeeWraperFullHelper.GetFull(r));
        }

        [Read("simple/filter")]
        public IEnumerable<EmployeeWraper> GetSimpleByFilter(EmployeeStatus? employeeStatus, Guid? groupId, EmployeeActivationStatus? activationStatus, EmployeeType? employeeType, bool? isAdministrator)
        {
            var users = GetByFilter(employeeStatus, groupId, activationStatus, employeeType, isAdministrator);

            return users.Select(_employeeWraperHelper.Get);
        }

        private IQueryable<UserInfo> GetByFilter(EmployeeStatus? employeeStatus, Guid? groupId, EmployeeActivationStatus? activationStatus, EmployeeType? employeeType, bool? isAdministrator)
        {
            if (_coreBaseSettings.Personal)
            {
                throw new MethodAccessException("Method not available");
            }

            var isAdmin = _userManager.GetUsers(_securityContext.CurrentAccount.ID).IsAdmin(_userManager) ||
                          _webItemSecurity.IsProductAdministrator(WebItemManager.PeopleProductID, _securityContext.CurrentAccount.ID);

            var includeGroups = new List<List<Guid>>();
            if (groupId.HasValue)
            {
                includeGroups.Add(new List<Guid> { groupId.Value });
            }

            var excludeGroups = new List<Guid>();

            if (employeeType != null)
            {
                switch (employeeType)
                {
                    case EmployeeType.User:
                        excludeGroups.Add(Constants.GroupVisitor.ID);
                        break;
                    case EmployeeType.Visitor:
                        includeGroups.Add(new List<Guid> { Constants.GroupVisitor.ID });
                        break;
                }
            }

            if (isAdministrator.HasValue && isAdministrator.Value)
            {
                var adminGroups = new List<Guid>
                {
                    Constants.GroupAdmin.ID
                };

                var products = _webItemManager.GetItemsAll().Where(i => i is IProduct || i.ID == WebItemManager.MailProductID);
                adminGroups.AddRange(products.Select(r => r.ID));

                includeGroups.Add(adminGroups);
            }

            var users = _userManager.GetUsers(isAdmin, employeeStatus, includeGroups, excludeGroups, activationStatus, _apiContext.FilterValue, _apiContext.SortBy, !_apiContext.SortDescending, _apiContext.Count, _apiContext.StartIndex, out var total, out var count);

            _apiContext.SetTotalCount(total).SetCount(count);

            return users;
        }

        [AllowAnonymous]
        [Create(@"register")]
        public string RegisterUserOnPersonal(RegisterPersonalUserDto model)
        {
            if (!_coreBaseSettings.Personal)
            {
                throw new MethodAccessException("Method is only available on personal.onlyoffice.com");
            }

            try
            {
                if (_coreBaseSettings.CustomMode)
                {
                    model.Lang = "ru-RU";
                }

                var cultureInfo = _setupInfo.GetPersonalCulture(model.Lang).Value;

                if (cultureInfo != null)
                {
                    Thread.CurrentThread.CurrentUICulture = cultureInfo;
                }

                model.Email.ThrowIfNull(new ArgumentException(Resource.ErrorEmailEmpty, "email"));

                if (!model.Email.TestEmailRegex())
                {
                    throw new ArgumentException(Resource.ErrorNotCorrectEmail, "email");
                }

                if (!SetupInfo.IsSecretEmail(model.Email)
                    && !string.IsNullOrEmpty(_setupInfo.RecaptchaPublicKey) && !string.IsNullOrEmpty(_setupInfo.RecaptchaPrivateKey))
                {
                    var ip = Request.Headers["X-Forwarded-For"].ToString() ?? Request.GetUserHostAddress();

                    if (string.IsNullOrEmpty(model.RecaptchaResponse)
                        || !_recaptcha.ValidateRecaptcha(model.RecaptchaResponse, ip))
                    {
                        throw new RecaptchaException(Resource.RecaptchaInvalid);
                    }
                }

                var newUserInfo = _userManager.GetUserByEmail(model.Email);

                if (_userManager.UserExists(newUserInfo.ID))
                {
                    if (!SetupInfo.IsSecretEmail(model.Email) || _securityContext.IsAuthenticated)
                    {
                        _studioNotifyService.SendAlreadyExist(model.Email);
                        return string.Empty;
                    }

                    try
                    {
                        _securityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                        _userManager.DeleteUser(newUserInfo.ID);
                    }
                    finally
                    {
                        _securityContext.Logout();
                    }
                }
                if (!model.Spam)
                {
                    try
                    {
                        //TODO
                        //const string _databaseID = "com";
                        //using (var db = DbManager.FromHttpContext(_databaseID))
                        //{
                        //    db.ExecuteNonQuery(new SqlInsert("template_unsubscribe", false)
                        //                           .InColumnValue("email", email.ToLowerInvariant())
                        //                           .InColumnValue("reason", "personal")
                        //        );
                        //    Log.Debug(String.Format("Write to template_unsubscribe {0}", email.ToLowerInvariant()));
                        //}
                    }
                    catch (Exception ex)
                    {
                        _logger.Debug($"ERROR write to template_unsubscribe {ex.Message}, email:{model.Email.ToLowerInvariant()}");
                    }
                }

                _studioNotifyService.SendInvitePersonal(model.Email);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return string.Empty;
        }

        [Create]
        [Authorize(AuthenticationSchemes = "confirm", Roles = "LinkInvite,Everyone")]
        public EmployeeWraperFull AddMemberFromBody([FromBody] MemberDto memberModel)
        {
            return AddMember(memberModel);
        }

        [Create]
        [Authorize(AuthenticationSchemes = "confirm", Roles = "LinkInvite,Everyone")]
        [Consumes("application/x-www-form-urlencoded")]
        public EmployeeWraperFull AddMemberFromForm([FromForm] MemberDto memberModel)
        {
            return AddMember(memberModel);
        }

        private EmployeeWraperFull AddMember(MemberDto memberModel)
        {
            _apiContext.AuthByClaim();

            _permissionContext.DemandPermissions(Constants.Action_AddRemoveUser);

            memberModel.PasswordHash = (memberModel.PasswordHash ?? "").Trim();
            if (string.IsNullOrEmpty(memberModel.PasswordHash))
            {
                memberModel.Password = (memberModel.Password ?? "").Trim();

                if (string.IsNullOrEmpty(memberModel.Password))
                {
                    memberModel.Password = UserManagerWrapper.GeneratePassword();
                }
                else
                {
                    _userManagerWrapper.CheckPasswordPolicy(memberModel.Password);
                }

                memberModel.PasswordHash = _passwordHasher.GetClientPassword(memberModel.Password);
            }

            var user = new UserInfo();

            //Validate email
            var address = new MailAddress(memberModel.Email);
            user.Email = address.Address;
            //Set common fields
            user.FirstName = memberModel.Firstname;
            user.LastName = memberModel.Lastname;
            user.Title = memberModel.Title;
            user.Location = memberModel.Location;
            user.Notes = memberModel.Comment;
            user.Sex = "male".Equals(memberModel.Sex, StringComparison.OrdinalIgnoreCase)
                           ? true
                           : ("female".Equals(memberModel.Sex, StringComparison.OrdinalIgnoreCase) ? (bool?)false : null);

            user.BirthDate = memberModel.Birthday != null && memberModel.Birthday != DateTime.MinValue ? _tenantUtil.DateTimeFromUtc(memberModel.Birthday) : null;
            user.WorkFromDate = memberModel.Worksfrom != null && memberModel.Worksfrom != DateTime.MinValue ? _tenantUtil.DateTimeFromUtc(memberModel.Worksfrom) : DateTime.UtcNow.Date;

            UpdateContacts(memberModel.Contacts, user);

            user = _userManagerWrapper.AddUser(user, memberModel.PasswordHash, memberModel.FromInviteLink, true, memberModel.IsVisitor, memberModel.FromInviteLink);

            var messageAction = memberModel.IsVisitor ? MessageAction.GuestCreated : MessageAction.UserCreated;
            _messageService.Send(messageAction, _messageTarget.Create(user.ID), user.DisplayUserName(false, _displayUserSettingsHelper));

            UpdateDepartments(memberModel.Department, user);

            if (memberModel.Files != _userPhotoManager.GetDefaultPhotoAbsoluteWebPath())
            {
                UpdatePhotoUrl(memberModel.Files, user);
            }

            return _employeeWraperFullHelper.GetFull(user);
        }

        [Create("active")]
        public EmployeeWraperFull AddMemberAsActivatedFromBody([FromBody] MemberDto memberModel)
        {
            return AddMemberAsActivated(memberModel);
        }

        [Create("active")]
        [Consumes("application/x-www-form-urlencoded")]
        public EmployeeWraperFull AddMemberAsActivatedFromForm([FromForm] MemberDto memberModel)
        {
            return AddMemberAsActivated(memberModel);
        }

        private EmployeeWraperFull AddMemberAsActivated(MemberDto memberModel)
        {
            _permissionContext.DemandPermissions(Constants.Action_AddRemoveUser);

            var user = new UserInfo();

            memberModel.PasswordHash = (memberModel.PasswordHash ?? "").Trim();
            if (string.IsNullOrEmpty(memberModel.PasswordHash))
            {
                memberModel.Password = (memberModel.Password ?? "").Trim();

                if (string.IsNullOrEmpty(memberModel.Password))
                {
                    memberModel.Password = UserManagerWrapper.GeneratePassword();
                }
                else
                {
                    _userManagerWrapper.CheckPasswordPolicy(memberModel.Password);
                }

                memberModel.PasswordHash = _passwordHasher.GetClientPassword(memberModel.Password);
            }

            //Validate email
            var address = new MailAddress(memberModel.Email);
            user.Email = address.Address;
            //Set common fields
            user.FirstName = memberModel.Firstname;
            user.LastName = memberModel.Lastname;
            user.Title = memberModel.Title;
            user.Location = memberModel.Location;
            user.Notes = memberModel.Comment;
            user.Sex = "male".Equals(memberModel.Sex, StringComparison.OrdinalIgnoreCase)
                           ? true
                           : ("female".Equals(memberModel.Sex, StringComparison.OrdinalIgnoreCase) ? (bool?)false : null);

            user.BirthDate = memberModel.Birthday != null ? _tenantUtil.DateTimeFromUtc(memberModel.Birthday) : null;
            user.WorkFromDate = memberModel.Worksfrom != null ? _tenantUtil.DateTimeFromUtc(memberModel.Worksfrom) : DateTime.UtcNow.Date;

            UpdateContacts(memberModel.Contacts, user);

            user = _userManagerWrapper.AddUser(user, memberModel.PasswordHash, false, false, memberModel.IsVisitor);

            user.ActivationStatus = EmployeeActivationStatus.Activated;

            UpdateDepartments(memberModel.Department, user);

            if (memberModel.Files != _userPhotoManager.GetDefaultPhotoAbsoluteWebPath())
            {
                UpdatePhotoUrl(memberModel.Files, user);
            }

            return _employeeWraperFullHelper.GetFull(user);
        }

        [Update("{userid}/culture")]
        public EmployeeWraperFull UpdateMemberCultureFromBody(string userid, [FromBody] UpdateMemberModel memberModel)
        {
            return UpdateMemberCulture(userid, memberModel);
        }

        [Update("{userid}/culture")]
        [Consumes("application/x-www-form-urlencoded")]
        public EmployeeWraperFull UpdateMemberCultureFromForm(string userid, [FromForm] UpdateMemberModel memberModel)
        {
            return UpdateMemberCulture(userid, memberModel);
        }

        private EmployeeWraperFull UpdateMemberCulture(string userid, UpdateMemberModel memberModel)
        {
            var user = GetUserInfo(userid);

            if (_userManager.IsSystemUser(user.ID))
            {
                throw new SecurityException();
            }

            _permissionContext.DemandPermissions(new UserSecurityProvider(user.ID), Constants.Action_EditUser);

            var curLng = user.CultureName;

            if (_setupInfo.EnabledCultures.Find(c => string.Equals(c.Name, memberModel.CultureName, StringComparison.InvariantCultureIgnoreCase)) != null)
            {
                if (curLng != memberModel.CultureName)
                {
                    user.CultureName = memberModel.CultureName;

                    try
                    {
                        _userManager.SaveUserInfo(user);
                    }
                    catch
                    {
                        user.CultureName = curLng;
                        throw;
                    }

                    _messageService.Send(MessageAction.UserUpdatedLanguage, _messageTarget.Create(user.ID), user.DisplayUserName(false, _displayUserSettingsHelper));

                }
            }

            return _employeeWraperFullHelper.GetFull(user);
        }

        [Update("{userid}")]
        public EmployeeWraperFull UpdateMemberFromBody(string userid, [FromBody] UpdateMemberModel memberModel)
        {
            return UpdateMember(userid, memberModel);
        }

        [Update("{userid}")]
        [Consumes("application/x-www-form-urlencoded")]
        public EmployeeWraperFull UpdateMemberFromForm(string userid, [FromForm] UpdateMemberModel memberModel)
        {
            return UpdateMember(userid, memberModel);
        }

        private EmployeeWraperFull UpdateMember(string userid, UpdateMemberModel memberModel)
        {
            var user = GetUserInfo(userid);

            if (_userManager.IsSystemUser(user.ID))
            {
                throw new SecurityException();
            }

            _permissionContext.DemandPermissions(new UserSecurityProvider(user.ID), Constants.Action_EditUser);

            var self = _securityContext.CurrentAccount.ID.Equals(user.ID);
            var resetDate = new DateTime(1900, 01, 01);

            //Update it

            var isLdap = user.IsLDAP();
            var isSso = user.IsSSO();
            var isAdmin = _webItemSecurity.IsProductAdministrator(WebItemManager.PeopleProductID, _securityContext.CurrentAccount.ID);

            if (!isLdap && !isSso)
            {
                //Set common fields

                user.FirstName = memberModel.Firstname ?? user.FirstName;
                user.LastName = memberModel.Lastname ?? user.LastName;
                user.Location = memberModel.Location ?? user.Location;

                if (isAdmin)
                {
                    user.Title = memberModel.Title ?? user.Title;
                }
            }

            if (!_userFormatter.IsValidUserName(user.FirstName, user.LastName))
            {
                throw new Exception(Resource.ErrorIncorrectUserName);
            }

            user.Notes = memberModel.Comment ?? user.Notes;
            user.Sex = ("male".Equals(memberModel.Sex, StringComparison.OrdinalIgnoreCase)
                            ? true
                            : ("female".Equals(memberModel.Sex, StringComparison.OrdinalIgnoreCase) ? (bool?)false : null)) ?? user.Sex;

            user.BirthDate = memberModel.Birthday != null ? _tenantUtil.DateTimeFromUtc(memberModel.Birthday) : user.BirthDate;

            if (user.BirthDate == resetDate)
            {
                user.BirthDate = null;
            }

            user.WorkFromDate = memberModel.Worksfrom != null ? _tenantUtil.DateTimeFromUtc(memberModel.Worksfrom) : user.WorkFromDate;

            if (user.WorkFromDate == resetDate)
            {
                user.WorkFromDate = null;
            }

            //Update contacts
            UpdateContacts(memberModel.Contacts, user);
            UpdateDepartments(memberModel.Department, user);

            if (memberModel.Files != _userPhotoManager.GetPhotoAbsoluteWebPath(user.ID))
            {
                UpdatePhotoUrl(memberModel.Files, user);
            }
            if (memberModel.Disable.HasValue)
            {
                user.Status = memberModel.Disable.Value ? EmployeeStatus.Terminated : EmployeeStatus.Active;
                user.TerminatedDate = memberModel.Disable.Value ? DateTime.UtcNow : null;
            }

            if (self && !isAdmin)
            {
                _studioNotifyService.SendMsgToAdminAboutProfileUpdated();
            }

            // change user type
            var canBeGuestFlag = !user.IsOwner(Tenant) && !user.IsAdmin(_userManager) && user.GetListAdminModules(_webItemSecurity).Count == 0 && !user.IsMe(_authContext);

            if (memberModel.IsVisitor && !user.IsVisitor(_userManager) && canBeGuestFlag)
            {
                _userManager.AddUserIntoGroup(user.ID, Constants.GroupVisitor.ID);
                _webItemSecurityCache.ClearCache(Tenant.TenantId);
            }

            if (!self && !memberModel.IsVisitor && user.IsVisitor(_userManager))
            {
                var usersQuota = _tenantExtra.GetTenantQuota().ActiveUsers;
                if (_tenantStatisticsProvider.GetUsersCount() < usersQuota)
                {
                    _userManager.RemoveUserFromGroup(user.ID, Constants.GroupVisitor.ID);
                    _webItemSecurityCache.ClearCache(Tenant.TenantId);
                }
                else
                {
                    throw new TenantQuotaException(string.Format("Exceeds the maximum active users ({0})", usersQuota));
                }
            }

            _userManager.SaveUserInfo(user);
            _messageService.Send(MessageAction.UserUpdated, _messageTarget.Create(user.ID), user.DisplayUserName(false, _displayUserSettingsHelper));

            if (memberModel.Disable.HasValue && memberModel.Disable.Value)
            {
                _cookiesManager.ResetUserCookie(user.ID);
                _messageService.Send(MessageAction.CookieSettingsUpdated);
            }

            return _employeeWraperFullHelper.GetFull(user);
        }

        [Delete("{userid}")]
        public EmployeeWraperFull DeleteMember(string userid)
        {
            _permissionContext.DemandPermissions(Constants.Action_AddRemoveUser);

            var user = GetUserInfo(userid);

            if (_userManager.IsSystemUser(user.ID) || user.IsLDAP())
            {
                throw new SecurityException();
            }

            if (user.Status != EmployeeStatus.Terminated)
            {
                throw new Exception("The user is not suspended");
            }

            CheckReassignProccess(new[] { user.ID });

            var userName = user.DisplayUserName(false, _displayUserSettingsHelper);

            _userPhotoManager.RemovePhoto(user.ID);
            _userManager.DeleteUser(user.ID);
            _queueWorkerRemove.Start(Tenant.TenantId, user, _securityContext.CurrentAccount.ID, false);

            _messageService.Send(MessageAction.UserDeleted, _messageTarget.Create(user.ID), userName);

            return _employeeWraperFullHelper.GetFull(user);
        }

        [Delete("@self")]
        [Authorize(AuthenticationSchemes = "confirm", Roles = "ProfileRemove")]
        public EmployeeWraperFull DeleteProfile()
        {
            _apiContext.AuthByClaim();

            if (_userManager.IsSystemUser(_securityContext.CurrentAccount.ID))
            {
                throw new SecurityException();
            }

            var user = GetUserInfo(_securityContext.CurrentAccount.ID.ToString());

            if (!_userManager.UserExists(user))
            {
                throw new Exception(Resource.ErrorUserNotFound);
            }

            if (user.IsLDAP())
            {
                throw new SecurityException();
            }

            _securityContext.AuthenticateMeWithoutCookie(ASC.Core.Configuration.Constants.CoreSystem);

            user.Status = EmployeeStatus.Terminated;

            _userManager.SaveUserInfo(user);

            var userName = user.DisplayUserName(false, _displayUserSettingsHelper);
            _messageService.Send(MessageAction.UsersUpdatedStatus, _messageTarget.Create(user.ID), userName);

            _cookiesManager.ResetUserCookie(user.ID);
            _messageService.Send(MessageAction.CookieSettingsUpdated);

            if (_coreBaseSettings.Personal)
            {
                _userPhotoManager.RemovePhoto(user.ID);
                _userManager.DeleteUser(user.ID);
                _messageService.Send(MessageAction.UserDeleted, _messageTarget.Create(user.ID), userName);
            }
            else
            {
                //StudioNotifyService.Instance.SendMsgProfileHasDeletedItself(user);
                //StudioNotifyService.SendMsgProfileDeletion(Tenant.TenantId, user);
            }

            return _employeeWraperFullHelper.GetFull(user);
        }

        [Update("{userid}/contacts")]
        public EmployeeWraperFull UpdateMemberContactsFromBody(string userid, [FromBody] UpdateMemberModel memberModel)
        {
            return UpdateMemberContacts(userid, memberModel);
        }

        [Update("{userid}/contacts")]
        [Consumes("application/x-www-form-urlencoded")]
        public EmployeeWraperFull UpdateMemberContactsFromForm(string userid, [FromForm] UpdateMemberModel memberModel)
        {
            return UpdateMemberContacts(userid, memberModel);
        }

        private EmployeeWraperFull UpdateMemberContacts(string userid, UpdateMemberModel memberModel)
        {
            var user = GetUserInfo(userid);

            if (_userManager.IsSystemUser(user.ID))
            {
                throw new SecurityException();
            }

            UpdateContacts(memberModel.Contacts, user);
            _userManager.SaveUserInfo(user);

            return _employeeWraperFullHelper.GetFull(user);
        }

        [Create("{userid}/contacts")]
        public EmployeeWraperFull SetMemberContactsFromBody(string userid, [FromBody] UpdateMemberModel memberModel)
        {
            return SetMemberContacts(userid, memberModel);
        }

        [Create("{userid}/contacts")]
        [Consumes("application/x-www-form-urlencoded")]
        public EmployeeWraperFull SetMemberContactsFromForm(string userid, [FromForm] UpdateMemberModel memberModel)
        {
            return SetMemberContacts(userid, memberModel);
        }

        private EmployeeWraperFull SetMemberContacts(string userid, UpdateMemberModel memberModel)
        {
            var user = GetUserInfo(userid);

            if (_userManager.IsSystemUser(user.ID))
            {
                throw new SecurityException();
            }

            user.ContactsList.Clear();
            UpdateContacts(memberModel.Contacts, user);
            _userManager.SaveUserInfo(user);

            return _employeeWraperFullHelper.GetFull(user);
        }

        [Delete("{userid}/contacts")]
        public EmployeeWraperFull DeleteMemberContactsFromBody(string userid, [FromBody] UpdateMemberModel memberModel)
        {
            return DeleteMemberContacts(userid, memberModel);
        }

        [Delete("{userid}/contacts")]
        [Consumes("application/x-www-form-urlencoded")]
        public EmployeeWraperFull DeleteMemberContactsFromForm(string userid, [FromForm] UpdateMemberModel memberModel)
        {
            return DeleteMemberContacts(userid, memberModel);
        }

        private EmployeeWraperFull DeleteMemberContacts(string userid, UpdateMemberModel memberModel)
        {
            var user = GetUserInfo(userid);

            if (_userManager.IsSystemUser(user.ID))
            {
                throw new SecurityException();
            }

            DeleteContacts(memberModel.Contacts, user);
            _userManager.SaveUserInfo(user);

            return _employeeWraperFullHelper.GetFull(user);
        }

        [Read("{userid}/photo")]
        public ThumbnailsDataDto GetMemberPhoto(string userid)
        {
            var user = GetUserInfo(userid);

            if (_userManager.IsSystemUser(user.ID))
            {
                throw new SecurityException();
            }

            return new ThumbnailsDataDto(user.ID, _userPhotoManager);
        }

        [Create("{userid}/photo")]
        public FileUploadResult UploadMemberPhoto(string userid, IFormCollection model)
        {
            var result = new People.Models.FileUploadResult();
            var autosave = bool.Parse(model["Autosave"]);

            try
            {
                if (model.Files.Count != 0)
                {
                    Guid userId;
                    try
                    {
                        userId = new Guid(userid);
                    }
                    catch
                    {
                        userId = _securityContext.CurrentAccount.ID;
                    }

                    _permissionContext.DemandPermissions(new UserSecurityProvider(userId), Constants.Action_EditUser);

                    var userPhoto = model.Files[0];

                    if (userPhoto.Length > _setupInfo.MaxImageUploadSize)
                    {
                        result.Success = false;
                        result.Message = _fileSizeComment.FileImageSizeExceptionString;

                        return result;
                    }

                    var data = new byte[userPhoto.Length];
                    using var inputStream = userPhoto.OpenReadStream();

                    var br = new BinaryReader(inputStream);
                    br.Read(data, 0, (int)userPhoto.Length);
                    br.Close();

                    CheckImgFormat(data);

                    if (autosave)
                    {
                        if (data.Length > _setupInfo.MaxImageUploadSize)
                        {
                            throw new ImageSizeLimitException();
                        }

                        var mainPhoto = _userPhotoManager.SaveOrUpdatePhoto(userId, data);

                        result.Data =
                            new
                            {
                                main = mainPhoto,
                                retina = _userPhotoManager.GetRetinaPhotoURL(userId),
                                max = _userPhotoManager.GetMaxPhotoURL(userId),
                                big = _userPhotoManager.GetBigPhotoURL(userId),
                                medium = _userPhotoManager.GetMediumPhotoURL(userId),
                                small = _userPhotoManager.GetSmallPhotoURL(userId),
                            };
                    }
                    else
                    {
                        result.Data = _userPhotoManager.SaveTempPhoto(data, _setupInfo.MaxImageUploadSize, UserPhotoManager.OriginalFotoSize.Width, UserPhotoManager.OriginalFotoSize.Height);
                    }

                    result.Success = true;
                }
                else
                {
                    result.Success = false;
                    result.Message = PeopleResource.ErrorEmptyUploadFileSelected;
                }

            }
            catch (Web.Core.Users.UnknownImageFormatException)
            {
                result.Success = false;
                result.Message = PeopleResource.ErrorUnknownFileImageType;
            }
            catch (ImageWeightLimitException)
            {
                result.Success = false;
                result.Message = PeopleResource.ErrorImageWeightLimit;
            }
            catch (ImageSizeLimitException)
            {
                result.Success = false;
                result.Message = PeopleResource.ErrorImageSizetLimit;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message.HtmlEncode();
            }

            return result;
        }

        [Update("{userid}/photo")]
        public ThumbnailsDataDto UpdateMemberPhotoFromBody(string userid, [FromBody] UpdateMemberModel model)
        {
            return UpdateMemberPhoto(userid, model);
        }

        [Update("{userid}/photo")]
        [Consumes("application/x-www-form-urlencoded")]
        public ThumbnailsDataDto UpdateMemberPhotoFromForm(string userid, [FromForm] UpdateMemberModel model)
        {
            return UpdateMemberPhoto(userid, model);
        }

        private ThumbnailsDataDto UpdateMemberPhoto(string userid, UpdateMemberModel model)
        {
            var user = GetUserInfo(userid);

            if (_userManager.IsSystemUser(user.ID))
            {
                throw new SecurityException();
            }

            if (model.Files != _userPhotoManager.GetPhotoAbsoluteWebPath(user.ID))
            {
                UpdatePhotoUrl(model.Files, user);
            }

            _userManager.SaveUserInfo(user);
            _messageService.Send(MessageAction.UserAddedAvatar, _messageTarget.Create(user.ID), user.DisplayUserName(false, _displayUserSettingsHelper));

            return new ThumbnailsDataDto(user.ID, _userPhotoManager);
        }

        [Delete("{userid}/photo")]
        public ThumbnailsDataDto DeleteMemberPhoto(string userid)
        {
            var user = GetUserInfo(userid);

            if (_userManager.IsSystemUser(user.ID))
            {
                throw new SecurityException();
            }

            _permissionContext.DemandPermissions(new UserSecurityProvider(user.ID), Constants.Action_EditUser);

            _userPhotoManager.RemovePhoto(user.ID);

            _userManager.SaveUserInfo(user);
            _messageService.Send(MessageAction.UserDeletedAvatar, _messageTarget.Create(user.ID), user.DisplayUserName(false, _displayUserSettingsHelper));

            return new ThumbnailsDataDto(user.ID, _userPhotoManager);
        }


        [Create("{userid}/photo/thumbnails")]
        public ThumbnailsDataDto CreateMemberPhotoThumbnailsFromBody(string userid, [FromBody] ThumbnailsDto thumbnailsModel)
        {
            return CreateMemberPhotoThumbnails(userid, thumbnailsModel);
        }

        [Create("{userid}/photo/thumbnails")]
        [Consumes("application/x-www-form-urlencoded")]
        public ThumbnailsDataDto CreateMemberPhotoThumbnailsFromForm(string userid, [FromForm] ThumbnailsDto thumbnailsModel)
        {
            return CreateMemberPhotoThumbnails(userid, thumbnailsModel);
        }

        private ThumbnailsDataDto CreateMemberPhotoThumbnails(string userid, ThumbnailsDto thumbnailsModel)
        {
            var user = GetUserInfo(userid);

            if (_userManager.IsSystemUser(user.ID))
            {
                throw new SecurityException();
            }

            _permissionContext.DemandPermissions(new UserSecurityProvider(user.ID), Constants.Action_EditUser);

            if (!string.IsNullOrEmpty(thumbnailsModel.TmpFile))
            {
                var fileName = Path.GetFileName(thumbnailsModel.TmpFile);
                var data = _userPhotoManager.GetTempPhotoData(fileName);

                var settings = new UserPhotoThumbnailSettings(thumbnailsModel.X, thumbnailsModel.Y, thumbnailsModel.Width, thumbnailsModel.Height);
                _settingsManager.SaveForUser(settings, user.ID);

                _userPhotoManager.RemovePhoto(user.ID);
                _userPhotoManager.SaveOrUpdatePhoto(user.ID, data);
                _userPhotoManager.RemoveTempPhoto(fileName);
            }
            else
            {
                UserPhotoThumbnailManager.SaveThumbnails(_userPhotoManager, _settingsManager, thumbnailsModel.X, thumbnailsModel.Y, thumbnailsModel.Width, thumbnailsModel.Height, user.ID);
            }

            _userManager.SaveUserInfo(user);
            _messageService.Send(MessageAction.UserUpdatedAvatarThumbnails, _messageTarget.Create(user.ID), user.DisplayUserName(false, _displayUserSettingsHelper));

            return new ThumbnailsDataDto(user.ID, _userPhotoManager);
        }


        [AllowAnonymous]
        [Create("password", false)]
        public object SendUserPasswordFromBody([FromBody] MemberDto memberModel)
        {
            return SendUserPassword(memberModel);
        }

        [AllowAnonymous]
        [Create("password", false)]
        [Consumes("application/x-www-form-urlencoded")]
        public object SendUserPasswordFromForm([FromForm] MemberDto memberModel)
        {
            return SendUserPassword(memberModel);
        }

        private object SendUserPassword(MemberDto memberModel)
        {
            string error = _userManagerWrapper.SendUserPassword(memberModel.Email);
            if (!string.IsNullOrEmpty(error))
            {
                _logger.ErrorFormat("Password recovery ({0}): {1}", memberModel.Email, error);
            }

            return string.Format(Resource.MessageYourPasswordSendedToEmail, memberModel.Email);
        }

        [Update("{userid}/password")]
        [Authorize(AuthenticationSchemes = "confirm", Roles = "PasswordChange,EmailChange,Activation,EmailActivation,Everyone")]
        public EmployeeWraperFull ChangeUserPasswordFromBody(Guid userid, [FromBody] MemberDto memberModel)
        {
            return ChangeUserPassword(userid, memberModel);
        }

        [Update("{userid}/password")]
        [Authorize(AuthenticationSchemes = "confirm", Roles = "PasswordChange,EmailChange,Activation,EmailActivation,Everyone")]
        [Consumes("application/x-www-form-urlencoded")]
        public EmployeeWraperFull ChangeUserPasswordFromForm(Guid userid, [FromForm] MemberDto memberModel)
        {
            return ChangeUserPassword(userid, memberModel);
        }

        private EmployeeWraperFull ChangeUserPassword(Guid userid, MemberDto memberModel)
        {
            _apiContext.AuthByClaim();
            _permissionContext.DemandPermissions(new UserSecurityProvider(userid), Constants.Action_EditUser);

            var user = _userManager.GetUsers(userid);

            if (!_userManager.UserExists(user))
            {
                return null;
            }

            if (_userManager.IsSystemUser(user.ID))
            {
                throw new SecurityException();
            }

            if (!string.IsNullOrEmpty(memberModel.Email))
            {
                var address = new MailAddress(memberModel.Email);
                if (!string.Equals(address.Address, user.Email, StringComparison.OrdinalIgnoreCase))
                {
                    user.Email = address.Address.ToLowerInvariant();
                    user.ActivationStatus = EmployeeActivationStatus.Activated;
                    _userManager.SaveUserInfo(user);
                }
            }

            memberModel.PasswordHash = (memberModel.PasswordHash ?? "").Trim();
            if (string.IsNullOrEmpty(memberModel.PasswordHash))
            {
                memberModel.Password = (memberModel.Password ?? "").Trim();

                if (!string.IsNullOrEmpty(memberModel.Password))
                {
                    memberModel.PasswordHash = _passwordHasher.GetClientPassword(memberModel.Password);
                }
            }

            if (!string.IsNullOrEmpty(memberModel.PasswordHash))
            {
                _securityContext.SetUserPasswordHash(userid, memberModel.PasswordHash);
                _messageService.Send(MessageAction.UserUpdatedPassword);

                _cookiesManager.ResetUserCookie(userid);
                _messageService.Send(MessageAction.CookieSettingsUpdated);
            }

            return _employeeWraperFullHelper.GetFull(GetUserInfo(userid.ToString()));
        }


        [Create("email", false)]
        public object SendEmailChangeInstructionsFromBody([FromBody] UpdateMemberModel model)
        {
            return SendEmailChangeInstructions(model);
        }

        [Create("email", false)]
        [Consumes("application/x-www-form-urlencoded")]
        public object SendEmailChangeInstructionsFromForm([FromForm] UpdateMemberModel model)
        {
            return SendEmailChangeInstructions(model);
        }

        private object SendEmailChangeInstructions(UpdateMemberModel model)
        {
            Guid.TryParse(model.UserId, out var userid);

            if (userid == Guid.Empty)
            {
                throw new ArgumentNullException("userid");
            }

            var email = (model.Email ?? "").Trim();

            if (string.IsNullOrEmpty(email))
            {
                throw new Exception(Resource.ErrorEmailEmpty);
            }

            if (!email.TestEmailRegex())
            {
                throw new Exception(Resource.ErrorNotCorrectEmail);
            }

            var viewer = _userManager.GetUsers(_securityContext.CurrentAccount.ID);
            var user = _userManager.GetUsers(userid);

            if (user == null)
            {
                throw new Exception(Resource.ErrorUserNotFound);
            }

            if (viewer == null || (user.IsOwner(Tenant) && viewer.ID != user.ID))
            {
                throw new Exception(Resource.ErrorAccessDenied);
            }

            var existentUser = _userManager.GetUserByEmail(email);

            if (existentUser.ID != Constants.LostUser.ID)
            {
                throw new Exception(_customNamingPeople.Substitute<Resource>("ErrorEmailAlreadyExists"));
            }

            if (!viewer.IsAdmin(_userManager))
            {
                _studioNotifyService.SendEmailChangeInstructions(user, email);
            }
            else
            {
                if (email == user.Email)
                {
                    throw new Exception(Resource.ErrorEmailsAreTheSame);
                }

                user.Email = email;
                user.ActivationStatus = EmployeeActivationStatus.NotActivated;
                _userManager.SaveUserInfo(user);
                _studioNotifyService.SendEmailActivationInstructions(user, email);
            }

            _messageService.Send(MessageAction.UserSentEmailChangeInstructions, user.DisplayUserName(false, _displayUserSettingsHelper));

            return string.Format(Resource.MessageEmailChangeInstuctionsSentOnEmail, email);
        }

        private UserInfo GetUserInfo(string userNameOrId)
        {
            UserInfo user;
            try
            {
                var userId = new Guid(userNameOrId);
                user = _userManager.GetUsers(userId);
            }
            catch (FormatException)
            {
                user = _userManager.GetUserByUserName(userNameOrId);
            }

            if (user == null || user.ID == Constants.LostUser.ID)
            {
                throw new ItemNotFoundException("user not found");
            }

            return user;
        }

        [Update("activationstatus/{activationstatus}")]
        [Authorize(AuthenticationSchemes = "confirm", Roles = "Activation,Everyone")]
        public IEnumerable<EmployeeWraperFull> UpdateEmployeeActivationStatusFromBody(EmployeeActivationStatus activationstatus, [FromBody] UpdateMembersDto model)
        {
            return UpdateEmployeeActivationStatus(activationstatus, model);
        }

        [Update("activationstatus/{activationstatus}")]
        [Authorize(AuthenticationSchemes = "confirm", Roles = "Activation,Everyone")]
        [Consumes("application/x-www-form-urlencoded")]
        public IEnumerable<EmployeeWraperFull> UpdateEmployeeActivationStatusFromForm(EmployeeActivationStatus activationstatus, [FromForm] UpdateMembersDto model)
        {
            return UpdateEmployeeActivationStatus(activationstatus, model);
        }

        private IEnumerable<EmployeeWraperFull> UpdateEmployeeActivationStatus(EmployeeActivationStatus activationstatus, UpdateMembersDto model)
        {
            _apiContext.AuthByClaim();

            var retuls = new List<EmployeeWraperFull>();
            foreach (var id in model.UserIds.Where(userId => !_userManager.IsSystemUser(userId)))
            {
                _permissionContext.DemandPermissions(new UserSecurityProvider(id), Constants.Action_EditUser);
                var u = _userManager.GetUsers(id);
                if (u.ID == Constants.LostUser.ID || u.IsLDAP())
                {
                    continue;
                }

                u.ActivationStatus = activationstatus;
                _userManager.SaveUserInfo(u);
                retuls.Add(_employeeWraperFullHelper.GetFull(u));
            }

            return retuls;
        }

        [Update("type/{type}")]
        public IEnumerable<EmployeeWraperFull> UpdateUserTypeFromBody(EmployeeType type, [FromBody] UpdateMembersDto model)
        {
            return UpdateUserType(type, model);
        }

        [Update("type/{type}")]
        [Consumes("application/x-www-form-urlencoded")]
        public IEnumerable<EmployeeWraperFull> UpdateUserTypeFromForm(EmployeeType type, [FromForm] UpdateMembersDto model)
        {
            return UpdateUserType(type, model);
        }

        private IEnumerable<EmployeeWraperFull> UpdateUserType(EmployeeType type, UpdateMembersDto model)
        {
            var users = model.UserIds
                .Where(userId => !_userManager.IsSystemUser(userId))
                .Select(userId => _userManager.GetUsers(userId))
                .ToList();

            foreach (var user in users)
            {
                if (user.IsOwner(Tenant) || user.IsAdmin(_userManager) 
                    || user.IsMe(_authContext) || user.GetListAdminModules(_webItemSecurity).Count > 0)
                {
                    continue;
                }

                switch (type)
                {
                    case EmployeeType.User:
                        if (user.IsVisitor(_userManager))
                        {
                            if (_tenantStatisticsProvider.GetUsersCount() < _tenantExtra.GetTenantQuota().ActiveUsers)
                            {
                                _userManager.RemoveUserFromGroup(user.ID, Constants.GroupVisitor.ID);
                                _webItemSecurityCache.ClearCache(Tenant.TenantId);
                            }
                        }
                        break;
                    case EmployeeType.Visitor:
                        if (_coreBaseSettings.Standalone || _tenantStatisticsProvider.GetVisitorsCount() < _tenantExtra.GetTenantQuota().ActiveUsers * _constants.CoefficientOfVisitors)
                        {
                            _userManager.AddUserIntoGroup(user.ID, Constants.GroupVisitor.ID);
                            _webItemSecurityCache.ClearCache(Tenant.TenantId);
                        }
                        break;
                }
            }

            _messageService.Send(MessageAction.UsersUpdatedType, _messageTarget.Create(users.Select(x => x.ID)), users.Select(x => x.DisplayUserName(false, _displayUserSettingsHelper)));

            return users.Select(_employeeWraperFullHelper.GetFull);
        }

        [Update("status/{status}")]
        public IEnumerable<EmployeeWraperFull> UpdateUserStatusFromBody(EmployeeStatus status, [FromBody] UpdateMembersDto model)
        {
            return UpdateUserStatus(status, model);
        }

        [Update("status/{status}")]
        [Consumes("application/x-www-form-urlencoded")]
        public IEnumerable<EmployeeWraperFull> UpdateUserStatusFromForm(EmployeeStatus status, [FromForm] UpdateMembersDto model)
        {
            return UpdateUserStatus(status, model);
        }

        private IEnumerable<EmployeeWraperFull> UpdateUserStatus(EmployeeStatus status, UpdateMembersDto model)
        {
            _permissionContext.DemandPermissions(Constants.Action_EditUser);

            var users = model.UserIds.Select(userId => _userManager.GetUsers(userId))
                .Where(u => !_userManager.IsSystemUser(u.ID) && !u.IsLDAP())
                .ToList();

            foreach (var user in users)
            {
                if (user.IsOwner(Tenant) || user.IsMe(_authContext))
                {
                    continue;
                }

                switch (status)
                {
                    case EmployeeStatus.Active:
                        if (user.Status == EmployeeStatus.Terminated)
                        {
                            if (_tenantStatisticsProvider.GetUsersCount() < _tenantExtra.GetTenantQuota().ActiveUsers || user.IsVisitor(_userManager))
                            {
                                user.Status = EmployeeStatus.Active;
                                _userManager.SaveUserInfo(user);
                            }
                        }
                        break;
                    case EmployeeStatus.Terminated:
                        user.Status = EmployeeStatus.Terminated;
                        _userManager.SaveUserInfo(user);

                        _cookiesManager.ResetUserCookie(user.ID);
                        _messageService.Send(MessageAction.CookieSettingsUpdated);
                        break;
                }
            }

            _messageService.Send(MessageAction.UsersUpdatedStatus, _messageTarget.Create(users.Select(x => x.ID)), users.Select(x => x.DisplayUserName(false, _displayUserSettingsHelper)));

            return users.Select(_employeeWraperFullHelper.GetFull);
        }


        [Update("invite")]
        public IEnumerable<EmployeeWraperFull> ResendUserInvitesFromBody([FromBody] UpdateMembersDto model)
        {
            return ResendUserInvites(model);
        }

        [Update("invite")]
        [Consumes("application/x-www-form-urlencoded")]
        public IEnumerable<EmployeeWraperFull> ResendUserInvitesFromForm([FromForm] UpdateMembersDto model)
        {
            return ResendUserInvites(model);
        }

        private IEnumerable<EmployeeWraperFull> ResendUserInvites(UpdateMembersDto model)
        {
            var users = model.UserIds
                .Where(userId => !_userManager.IsSystemUser(userId))
                .Select(userId => _userManager.GetUsers(userId))
                .ToList();

            foreach (var user in users)
            {
                if (user.IsActive)
                {
                    continue;
                }

                var viewer = _userManager.GetUsers(_securityContext.CurrentAccount.ID);

                if (viewer == null)
                {
                    throw new Exception(Resource.ErrorAccessDenied);
                }

                if (viewer.IsAdmin(_userManager) || viewer.ID == user.ID)
                {
                    if (user.ActivationStatus == EmployeeActivationStatus.Activated)
                    {
                        user.ActivationStatus = EmployeeActivationStatus.NotActivated;
                    }
                    if (user.ActivationStatus == (EmployeeActivationStatus.AutoGenerated | EmployeeActivationStatus.Activated))
                    {
                        user.ActivationStatus = EmployeeActivationStatus.AutoGenerated;
                    }
                    _userManager.SaveUserInfo(user);
                }

                if (user.ActivationStatus == EmployeeActivationStatus.Pending)
                {
                    if (user.IsVisitor(_userManager))
                    {
                        _studioNotifyService.GuestInfoActivation(user);
                    }
                    else
                    {
                        _studioNotifyService.UserInfoActivation(user);
                    }
                }
                else
                {
                    _studioNotifyService.SendEmailActivationInstructions(user, user.Email);
                }
            }

            _messageService.Send(MessageAction.UsersSentActivationInstructions, _messageTarget.Create(users.Select(x => x.ID)), users.Select(x => x.DisplayUserName(false, _displayUserSettingsHelper)));

            return users.Select(_employeeWraperFullHelper.GetFull);
        }

        [Update("delete", Order = -1)]
        public IEnumerable<EmployeeWraperFull> RemoveUsersFromBody([FromBody] UpdateMembersDto model)
        {
            return RemoveUsers(model);
        }

        [Update("delete", Order = -1)]
        [Consumes("application/x-www-form-urlencoded")]
        public IEnumerable<EmployeeWraperFull> RemoveUsersFromForm([FromForm] UpdateMembersDto model)
        {
            return RemoveUsers(model);
        }

        private IEnumerable<EmployeeWraperFull> RemoveUsers(UpdateMembersDto model)
        {
            _permissionContext.DemandPermissions(Constants.Action_AddRemoveUser);

            CheckReassignProccess(model.UserIds);

            var users = model.UserIds.Select(userId => _userManager.GetUsers(userId))
                .Where(u => !_userManager.IsSystemUser(u.ID) && !u.IsLDAP())
                .ToList();

            var userNames = users.Select(x => x.DisplayUserName(false, _displayUserSettingsHelper)).ToList();

            foreach (var user in users)
            {
                if (user.Status != EmployeeStatus.Terminated)
                {
                    continue;
                }

                _userPhotoManager.RemovePhoto(user.ID);
                _userManager.DeleteUser(user.ID);
                _queueWorkerRemove.Start(Tenant.TenantId, user, _securityContext.CurrentAccount.ID, false);
            }

            _messageService.Send(MessageAction.UsersDeleted, _messageTarget.Create(users.Select(x => x.ID)), userNames);

            return users.Select(_employeeWraperFullHelper.GetFull);
        }


        [Update("self/delete")]
        public object SendInstructionsToDelete()
        {
            var user = _userManager.GetUsers(_securityContext.CurrentAccount.ID);

            if (user.IsLDAP())
            {
                throw new SecurityException();
            }

            _studioNotifyService.SendMsgProfileDeletion(user);
            _messageService.Send(MessageAction.UserSentDeleteInstructions);

            return string.Format(Resource.SuccessfullySentNotificationDeleteUserInfoMessage, "<b>" + user.Email + "</b>");
        }

        [AllowAnonymous]
        [Read("thirdparty/providers")]
        public ICollection<AccountInfoDto> GetAuthProviders(bool inviteView, bool settingsView, string clientCallback, string fromOnly)
        {
            ICollection<AccountInfoDto> infos = new List<AccountInfoDto>();
            IEnumerable<LoginProfile> linkedAccounts = new List<LoginProfile>();

            if (_authContext.IsAuthenticated)
            {
                linkedAccounts = _accountLinker.Get("webstudio").GetLinkedProfiles(_authContext.CurrentAccount.ID.ToString());
            }

            fromOnly = string.IsNullOrWhiteSpace(fromOnly) ? string.Empty : fromOnly.ToLower();

            foreach (var provider in ProviderManager.AuthProviders.Where(provider => string.IsNullOrEmpty(fromOnly) || fromOnly == provider || (provider == "google" && fromOnly == "openid")))
            {
                if (inviteView && provider.Equals("twitter", StringComparison.OrdinalIgnoreCase)) continue;

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
        public void LinkAccountFromBody([FromBody] LinkAccountDto model)
        {
            LinkAccount(model);
        }

        [Update("thirdparty/linkaccount")]
        [Consumes("application/x-www-form-urlencoded")]
        public void LinkAccountFromForm([FromForm] LinkAccountDto model)
        {
            LinkAccount(model);
        }

        public void LinkAccount(LinkAccountDto model)
        {
            var profile = new LoginProfile(_signature, _instanceCrypto, model.SerializedProfile);

            if (!(_coreBaseSettings.Standalone || _tenantExtra.GetTenantQuota().Oauth))
            {
                throw new Exception("ErrorNotAllowedOption");
            }

            if (string.IsNullOrEmpty(profile.AuthorizationError))
            {
                GetLinker().AddLink(_securityContext.CurrentAccount.ID.ToString(), profile);
                _messageService.Send(MessageAction.UserLinkedSocialAccount, GetMeaningfulProviderName(profile.Provider));
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

        [Delete("thirdparty/unlinkaccount")]
        public void UnlinkAccount(string provider)
        {
            GetLinker().RemoveProvider(_securityContext.CurrentAccount.ID.ToString(), provider);
            _messageService.Send(MessageAction.UserUnlinkedSocialAccount, GetMeaningfulProviderName(provider));
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

        public void SignupAccount(SignupAccountModel model)
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
                _securityContext.AuthenticateMeWithoutCookie(ASC.Core.Configuration.Constants.CoreSystem);
                var newUser = CreateNewUser(GetFirstName(model, thirdPartyProfile), GetLastName(model, thirdPartyProfile), GetEmailAddress(model, thirdPartyProfile), passwordHash, employeeType, false);

                var messageAction = employeeType == EmployeeType.User ? MessageAction.UserCreatedViaInvite : MessageAction.GuestCreatedViaInvite;
                _messageService.Send(MessageInitiator.System, messageAction, _messageTarget.Create(newUser.ID), newUser.DisplayUserName(false, _displayUserSettingsHelper));

                userID = newUser.ID;
                if (!string.IsNullOrEmpty(thirdPartyProfile.Avatar))
                {
                    SaveContactImage(userID, thirdPartyProfile.Avatar);
                }

                GetLinker().AddLink(userID.ToString(), thirdPartyProfile);
            }
            finally
            {
                _securityContext.Logout();
            }

            var user = _userManager.GetUsers(userID);
            var cookiesKey = _securityContext.AuthenticateMe(user.Email, passwordHash);
            _cookiesManager.SetCookies(CookiesType.AuthKey, cookiesKey);
            _messageService.Send(MessageAction.LoginSuccess);
            _studioNotifyService.UserHasJoin();

            if (mustChangePassword)
            {
                _studioNotifyService.UserPasswordChange(user);
            }

            _userHelpTourHelper.IsNewUser = true;
            if (_coreBaseSettings.Personal)
            {
                _personalSettingsHelper.IsNewUser = true;
            }
        }

        [Create("phone")]
        public object SendNotificationToChangeFromBody([FromBody] UpdateMemberModel model)
        {
            return SendNotificationToChange(model.UserId);
        }

        [Create("phone")]
        [Consumes("application/x-www-form-urlencoded")]
        public object SendNotificationToChangeFromForm([FromForm] UpdateMemberModel model)
        {
            return SendNotificationToChange(model.UserId);
        }

        public object SendNotificationToChange(string userId)
        {
            var user = _userManager.GetUsers(
                string.IsNullOrEmpty(userId)
                    ? _securityContext.CurrentAccount.ID
                    : new Guid(userId));

            var canChange =
                user.IsMe(_authContext)
                || _permissionContext.CheckPermissions(new UserSecurityProvider(user.ID), Constants.Action_EditUser);

            if (!canChange)
            {
                throw new SecurityAccessDeniedException(Resource.ErrorAccessDenied);
            }

            user.MobilePhoneActivationStatus = MobilePhoneActivationStatus.NotActivated;
            _userManager.SaveUserInfo(user);

            if (user.IsMe(_authContext))
            {
                return _commonLinkUtility.GetConfirmationUrl(user.Email, ConfirmType.PhoneActivation);
            }

            _studioNotifyService.SendMsgMobilePhoneChange(user);

            return string.Empty;
        }

        protected string GetEmailAddress(SignupAccountModel model)
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

        protected string GetFirstName(SignupAccountModel model)
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

        protected string GetLastName(SignupAccountModel model)
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

        private void SaveContactImage(Guid userID, string url)
        {
            using (var memstream = new MemoryStream())
            {
                var request = new HttpRequestMessage();
                request.RequestUri = new Uri(url);

                var httpClient = _clientFactory.CreateClient();
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

        private AccountLinker GetLinker()
        {
            return _accountLinker.Get("webstudio");
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


        [Read(@"reassign/progress")]
        public ReassignProgressItem GetReassignProgress(Guid userId)
        {
            _permissionContext.DemandPermissions(Constants.Action_EditUser);

            return _queueWorkerReassign.GetProgressItemStatus(Tenant.TenantId, userId);
        }

        [Update(@"reassign/terminate")]
        public void TerminateReassignFromBody([FromBody] TerminateDto model)
        {
            _permissionContext.DemandPermissions(Constants.Action_EditUser);

            _queueWorkerReassign.Terminate(Tenant.TenantId, model.UserId);
        }

        [Update(@"reassign/terminate")]
        [Consumes("application/x-www-form-urlencoded")]
        public void TerminateReassignFromForm([FromForm] TerminateDto model)
        {
            _permissionContext.DemandPermissions(Constants.Action_EditUser);

            _queueWorkerReassign.Terminate(Tenant.TenantId, model.UserId);
        }

        [Create(@"reassign/start")]
        public ReassignProgressItem StartReassignFromBody([FromBody] StartReassignDto model)
        {
            return StartReassign(model);
        }

        [Create(@"reassign/start")]
        [Consumes("application/x-www-form-urlencoded")]
        public ReassignProgressItem StartReassignFromForm([FromForm] StartReassignDto model)
        {
            return StartReassign(model);
        }

        private ReassignProgressItem StartReassign(StartReassignDto model)
        {
            _permissionContext.DemandPermissions(Constants.Action_EditUser);

            var fromUser = _userManager.GetUsers(model.FromUserId);

            if (fromUser == null || fromUser.ID == Constants.LostUser.ID)
            {
                throw new ArgumentException("User with id = " + model.FromUserId + " not found");
            }

            if (fromUser.IsOwner(Tenant) || fromUser.IsMe(_authContext) || fromUser.Status != EmployeeStatus.Terminated)
            {
                throw new ArgumentException("Can not delete user with id = " + model.FromUserId);
            }

            var toUser = _userManager.GetUsers(model.ToUserId);

            if (toUser == null || toUser.ID == Constants.LostUser.ID)
            {
                throw new ArgumentException("User with id = " + model.ToUserId + " not found");
            }

            if (toUser.IsVisitor(_userManager) || toUser.Status == EmployeeStatus.Terminated)
            {
                throw new ArgumentException("Can not reassign data to user with id = " + model.ToUserId);
            }

            return _queueWorkerReassign.Start(Tenant.TenantId, model.FromUserId, model.ToUserId, _securityContext.CurrentAccount.ID, model.DeleteProfile);
        }

        private void CheckReassignProccess(IEnumerable<Guid> userIds)
        {
            foreach (var userId in userIds)
            {
                var reassignStatus = _queueWorkerReassign.GetProgressItemStatus(Tenant.TenantId, userId);
                if (reassignStatus == null || reassignStatus.IsCompleted)
                {
                    continue;
                }

                var userName = _userManager.GetUsers(userId).DisplayUserName(_displayUserSettingsHelper);
                throw new Exception(string.Format(Resource.ReassignDataRemoveUserError, userName));
            }
        }

        //#endregion

        #region Remove user data


        [Read(@"remove/progress")]
        public RemoveProgressItem GetRemoveProgress(Guid userId)
        {
            _permissionContext.DemandPermissions(Constants.Action_EditUser);

            return _queueWorkerRemove.GetProgressItemStatus(Tenant.TenantId, userId);
        }

        [Update(@"remove/terminate")]
        public void TerminateRemoveFromBody([FromBody] TerminateDto model)
        {
            _permissionContext.DemandPermissions(Constants.Action_EditUser);

            _queueWorkerRemove.Terminate(Tenant.TenantId, model.UserId);
        }

        [Update(@"remove/terminate")]
        [Consumes("application/x-www-form-urlencoded")]
        public void TerminateRemoveFromForm([FromForm] TerminateDto model)
        {
            _permissionContext.DemandPermissions(Constants.Action_EditUser);

            _queueWorkerRemove.Terminate(Tenant.TenantId, model.UserId);
        }

        [Create(@"remove/start")]
        public RemoveProgressItem StartRemoveFromBody([FromBody] TerminateDto model)
        {
            return StartRemove(model);
        }

        [Create(@"remove/start")]
        [Consumes("application/x-www-form-urlencoded")]
        public RemoveProgressItem StartRemoveFromForm([FromForm] TerminateDto model)
        {
            return StartRemove(model);
        }

        private RemoveProgressItem StartRemove(TerminateDto model)
        {
            _permissionContext.DemandPermissions(Constants.Action_EditUser);

            var user = _userManager.GetUsers(model.UserId);

            if (user == null || user.ID == Constants.LostUser.ID)
            {
                throw new ArgumentException("User with id = " + model.UserId + " not found");
            }

            if (user.IsOwner(Tenant) || user.IsMe(_authContext) || user.Status != EmployeeStatus.Terminated)
            {
                throw new ArgumentException("Can not delete user with id = " + model.UserId);
            }

            return _queueWorkerRemove.Start(Tenant.TenantId, user, _securityContext.CurrentAccount.ID, true);
        }

        #endregion

        private void UpdateDepartments(IEnumerable<Guid> department, UserInfo user)
        {
            if (!_permissionContext.CheckPermissions(Constants.Action_EditGroups))
            {
                return;
            }

            if (department == null)
            {
                return;
            }

            var groups = _userManager.GetUserGroups(user.ID);
            var managerGroups = new List<Guid>();
            foreach (var groupInfo in groups)
            {
                _userManager.RemoveUserFromGroup(user.ID, groupInfo.ID);
                var managerId = _userManager.GetDepartmentManager(groupInfo.ID);
                if (managerId == user.ID)
                {
                    managerGroups.Add(groupInfo.ID);
                    _userManager.SetDepartmentManager(groupInfo.ID, Guid.Empty);
                }
            }
            foreach (var guid in department)
            {
                var userDepartment = _userManager.GetGroupInfo(guid);
                if (userDepartment != Constants.LostGroupInfo)
                {
                    _userManager.AddUserIntoGroup(user.ID, guid);
                    if (managerGroups.Contains(guid))
                    {
                        _userManager.SetDepartmentManager(guid, user.ID);
                    }
                }
            }
        }

        private void UpdateContacts(IEnumerable<Contact> contacts, UserInfo user)
        {
            _permissionContext.DemandPermissions(new UserSecurityProvider(user.ID), Constants.Action_EditUser);

            if (contacts == null)
            {
                return;
            }

            var values = contacts.Where(r => !string.IsNullOrEmpty(r.Value)).Select(r => $"{r.Type}|{r.Value}");
            user.Contacts = string.Join('|', values);
        }

        private void DeleteContacts(IEnumerable<Contact> contacts, UserInfo user)
        {
            _permissionContext.DemandPermissions(new UserSecurityProvider(user.ID), Constants.Action_EditUser);
            if (contacts == null)
            {
                return;
            }

            if (user.ContactsList == null)
            {
                user.ContactsList = new List<string>();
            }

            foreach (var contact in contacts)
            {
                var index = user.ContactsList.IndexOf(contact.Type);
                if (index != -1)
                {
                    //Remove existing
                    user.ContactsList.RemoveRange(index, 2);
                }
            }
        }

        private void UpdatePhotoUrl(string files, UserInfo user)
        {
            if (string.IsNullOrEmpty(files))
            {
                return;
            }

            _permissionContext.DemandPermissions(new UserSecurityProvider(user.ID), Constants.Action_EditUser);

            if (!files.StartsWith("http://") && !files.StartsWith("https://"))
            {
                files = new Uri(_apiContext.HttpContextAccessor.HttpContext.Request.GetDisplayUrl()).GetLeftPart(UriPartial.Authority) + "/" + files.TrimStart('/');
            }
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(files);

            var httpClient = _clientFactory.CreateClient();
            using var response = httpClient.Send(request);
            using var inputStream = response.Content.ReadAsStream();
            using var br = new BinaryReader(inputStream);
            var imageByteArray = br.ReadBytes((int)inputStream.Length);
            _userPhotoManager.SaveOrUpdatePhoto(user.ID, imageByteArray);
        }

        private static void CheckImgFormat(byte[] data)
        {
            IImageFormat imgFormat;
            try
            {
                using var img = Image.Load(data, out var format);
                imgFormat = format;
            }
            catch (OutOfMemoryException)
            {
                throw new ImageSizeLimitException();
            }
            catch (ArgumentException error)
            {
                throw new Web.Core.Users.UnknownImageFormatException(error);
            }

            if (imgFormat.Name != "PNG" && imgFormat.Name != "JPEG")
            {
                throw new Web.Core.Users.UnknownImageFormatException();
            }
        }
    }
}
