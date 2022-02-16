namespace ASC.Employee.Core.Controllers
{
    [Scope]
    [DefaultRoute]
    [ApiController]
    public class GroupController : ControllerBase
    {
        public readonly ApiContext _apiContext;
        private readonly MessageService _messageService;
        private readonly UserManager _userManager;
        private readonly PermissionContext _permissionContext;
        private readonly MessageTarget _messageTarget;
        private readonly GroupWraperFullHelper _groupWraperFullHelper;

        public GroupController(
            ApiContext apiContext,
            MessageService messageService,
            UserManager userManager,
            PermissionContext permissionContext,
            MessageTarget messageTarget,
            GroupWraperFullHelper groupWraperFullHelper)
        {
            _apiContext = apiContext;
            _messageService = messageService;
            _userManager = userManager;
            _permissionContext = permissionContext;
            _messageTarget = messageTarget;
            _groupWraperFullHelper = groupWraperFullHelper;
        }

        [Read]
        public IEnumerable<GroupWrapperSummary> GetAll()
        {
            var result = _userManager.GetDepartments().Select(r => r);
            if (!string.IsNullOrEmpty(_apiContext.FilterValue))
            {
                result = result.Where(r => r.Name.Contains(_apiContext.FilterValue, StringComparison.InvariantCultureIgnoreCase));
            }

            return result.Select(x => new GroupWrapperSummary(x, _userManager));
        }

        [Read("full")]
        public IEnumerable<GroupWrapperFull> GetAllWithMembers()
        {
            var result = _userManager.GetDepartments().Select(r => r);
            if (!string.IsNullOrEmpty(_apiContext.FilterValue))
            {
                result = result.Where(r => r.Name.Contains(_apiContext.FilterValue, StringComparison.InvariantCultureIgnoreCase));
            }

            return result.Select(r=> _groupWraperFullHelper.Get(r, true));
        }

        [Read("{groupid}")]
        public GroupWrapperFull GetById(Guid groupid)
        {
            return _groupWraperFullHelper.Get(GetGroupInfo(groupid), true);
        }

        [Read("user/{userid}")]
        public IEnumerable<GroupWrapperSummary> GetByUserId(Guid userid)
        {
            return _userManager.GetUserGroups(userid).Select(x => new GroupWrapperSummary(x, _userManager));
        }

        [Create]
        public GroupWrapperFull AddGroupFromBody([FromBody]GroupModel groupModel)
        {
            return AddGroup(groupModel);
        }

        [Create]
        [Consumes("application/x-www-form-urlencoded")]
        public GroupWrapperFull AddGroupFromForm([FromForm] GroupModel groupModel)
        {
            return AddGroup(groupModel);
        }

        private GroupWrapperFull AddGroup(GroupModel groupModel)
        {
            _permissionContext.DemandPermissions(Constants.Action_EditGroups, Constants.Action_AddRemoveUser);

            var group = _userManager.SaveGroupInfo(new GroupInfo { Name = groupModel.GroupName });

            TransferUserToDepartment(groupModel.GroupManager, @group, true);
            if (groupModel.Members != null)
            {
                foreach (var member in groupModel.Members)
                {
                    TransferUserToDepartment(member, group, false);
                }
            }

            _messageService.Send(MessageAction.GroupCreated, _messageTarget.Create(group.ID), group.Name);

            return _groupWraperFullHelper.Get(group, true);
        }

        [Update("{groupid}")]
        public GroupWrapperFull UpdateGroupFromBody(Guid groupid, [FromBody]GroupModel groupModel)
        {
            return UpdateGroup(groupid, groupModel);
        }

        [Update("{groupid}")]
        [Consumes("application/x-www-form-urlencoded")]
        public GroupWrapperFull UpdateGroupFromForm(Guid groupid, [FromForm] GroupModel groupModel)
        {
            return UpdateGroup(groupid, groupModel);
        }

        private GroupWrapperFull UpdateGroup(Guid groupid, GroupModel groupModel)
        {
            _permissionContext.DemandPermissions(Constants.Action_EditGroups, Constants.Action_AddRemoveUser);
            var group = _userManager.GetGroups().SingleOrDefault(x => x.ID == groupid).NotFoundIfNull("group not found");
            if (groupid == Constants.LostGroupInfo.ID)
            {
                throw new ItemNotFoundException("group not found");
            }

            group.Name = groupModel.GroupName ?? group.Name;
            _userManager.SaveGroupInfo(group);

            RemoveMembersFrom(groupid, new GroupModel {Members = _userManager.GetUsersByGroup(groupid, EmployeeStatus.All).Select(u => u.ID).Where(id => !groupModel.Members.Contains(id)) });

            TransferUserToDepartment(groupModel.GroupManager, @group, true);
            if (groupModel.Members != null)
            {
                foreach (var member in groupModel.Members)
                {
                    TransferUserToDepartment(member, group, false);
                }
            }

            _messageService.Send(MessageAction.GroupUpdated, _messageTarget.Create(groupid), group.Name);

            return GetById(groupid);
        }

        [Delete("{groupid}")]
        public GroupWrapperFull DeleteGroup(Guid groupid)
        {
            _permissionContext.DemandPermissions(Constants.Action_EditGroups, Constants.Action_AddRemoveUser);
            var @group = GetGroupInfo(groupid);
            var groupWrapperFull = _groupWraperFullHelper.Get(group, false);

            _userManager.DeleteGroup(groupid);

            _messageService.Send(MessageAction.GroupDeleted, _messageTarget.Create(group.ID), group.Name);

            return groupWrapperFull;
        }

        private GroupInfo GetGroupInfo(Guid groupid)
        {
            var group = _userManager.GetGroups().SingleOrDefault(x => x.ID == groupid).NotFoundIfNull("group not found");
            if (group.ID == Constants.LostGroupInfo.ID)
            {
                throw new ItemNotFoundException("group not found");
            }

            return @group;
        }

        [Update("{groupid}/members/{newgroupid}")]
        public GroupWrapperFull TransferMembersTo(Guid groupid, Guid newgroupid)
        {
            _permissionContext.DemandPermissions(Constants.Action_EditGroups, Constants.Action_AddRemoveUser);
            var oldgroup = GetGroupInfo(groupid);

            var newgroup = GetGroupInfo(newgroupid);

            var users = _userManager.GetUsersByGroup(oldgroup.ID);
            foreach (var userInfo in users)
            {
                TransferUserToDepartment(userInfo.ID, newgroup, false);
            }

            return GetById(newgroupid);
        }

        [Create("{groupid}/members")]
        public GroupWrapperFull SetMembersToFromBody(Guid groupid, [FromBody]GroupModel groupModel)
        {
            return SetMembersTo(groupid, groupModel);
        }

        [Create("{groupid}/members")]
        [Consumes("application/x-www-form-urlencoded")]
        public GroupWrapperFull SetMembersToFromForm(Guid groupid, [FromForm] GroupModel groupModel)
        {
            return SetMembersTo(groupid, groupModel);
        }

        private GroupWrapperFull SetMembersTo(Guid groupid, GroupModel groupModel)
        {
            RemoveMembersFrom(groupid, new GroupModel {Members = _userManager.GetUsersByGroup(groupid).Select(x => x.ID) });
            AddMembersTo(groupid, groupModel);

            return GetById(groupid);
        }

        [Update("{groupid}/members")]
        public GroupWrapperFull AddMembersToFromBody(Guid groupid, [FromBody]GroupModel groupModel)
        {
            return AddMembersTo(groupid, groupModel);
        }

        [Update("{groupid}/members")]
        [Consumes("application/x-www-form-urlencoded")]
        public GroupWrapperFull AddMembersToFromForm(Guid groupid, [FromForm] GroupModel groupModel)
        {
            return AddMembersTo(groupid, groupModel);
        }

        private GroupWrapperFull AddMembersTo(Guid groupid, GroupModel groupModel)
        {
            _permissionContext.DemandPermissions(Constants.Action_EditGroups, Constants.Action_AddRemoveUser);
            var group = GetGroupInfo(groupid);

            foreach (var userId in groupModel.Members)
            {
                TransferUserToDepartment(userId, group, false);
            }

            return GetById(group.ID);
        }

        [Update("{groupid}/manager")]
        public GroupWrapperFull SetManagerFromBody(Guid groupid, [FromBody]SetManagerModel setManagerModel)
        {
            return SetManager(groupid, setManagerModel);
        }

        [Update("{groupid}/manager")]
        [Consumes("application/x-www-form-urlencoded")]
        public GroupWrapperFull SetManagerFromForm(Guid groupid, [FromForm] SetManagerModel setManagerModel)
        {
            return SetManager(groupid, setManagerModel);
        }

        private GroupWrapperFull SetManager(Guid groupid, SetManagerModel setManagerModel)
        {
            var group = GetGroupInfo(groupid);
            if (_userManager.UserExists(setManagerModel.UserId))
            {
                _userManager.SetDepartmentManager(group.ID, setManagerModel.UserId);
            }
            else
            {
                throw new ItemNotFoundException("user not found");
            }

            return GetById(groupid);
        }

        [Delete("{groupid}/members")]
        public GroupWrapperFull RemoveMembersFromFromBody(Guid groupid, [FromBody]GroupModel groupModel)
        {
            return RemoveMembersFrom(groupid, groupModel);
        }

        [Delete("{groupid}/members")]
        [Consumes("application/x-www-form-urlencoded")]
        public GroupWrapperFull RemoveMembersFromFromForm(Guid groupid, [FromForm] GroupModel groupModel)
        {
            return RemoveMembersFrom(groupid, groupModel);
        }

        private GroupWrapperFull RemoveMembersFrom(Guid groupid, GroupModel groupModel)
        {
            _permissionContext.DemandPermissions(Constants.Action_EditGroups, Constants.Action_AddRemoveUser);
            var group = GetGroupInfo(groupid);

            foreach (var userId in groupModel.Members)
            {
                RemoveUserFromDepartment(userId, group);
            }

            return GetById(group.ID);
        }

        private void RemoveUserFromDepartment(Guid userId, GroupInfo @group)
        {
            if (!_userManager.UserExists(userId))
            {
                return;
            }

            var user = _userManager.GetUsers(userId);
            _userManager.RemoveUserFromGroup(user.ID, @group.ID);
            _userManager.SaveUserInfo(user);
        }

        private void TransferUserToDepartment(Guid userId, GroupInfo group, bool setAsManager)
        {
            if (!_userManager.UserExists(userId) && userId != Guid.Empty)
            {
                return;
            }

            if (setAsManager)
            {
                _userManager.SetDepartmentManager(@group.ID, userId);
            }
            _userManager.AddUserIntoGroup(userId, @group.ID);
        }
    }
}