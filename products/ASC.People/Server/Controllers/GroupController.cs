﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;

using ASC.Api.Core;
using ASC.Api.Utils;
using ASC.Common;
using ASC.Common.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Files.Core.Core;
using ASC.Files.Core.VirtualRooms;
using ASC.MessagingSystem;
using ASC.People.Models;
using ASC.Web.Api.Models;
using ASC.Web.Api.Routing;

using Microsoft.AspNetCore.Mvc;

namespace ASC.Employee.Core.Controllers
{
    [Scope]
    [DefaultRoute]
    [ApiController]
    public class GroupController : ControllerBase
    {
        public ApiContext ApiContext { get; }
        private MessageService MessageService { get; }

        private UserManager UserManager { get; }
        private CoreBaseSettings CoreBaseSettings { get; }
        private PermissionContext PermissionContext { get; }
        private MessageTarget MessageTarget { get; }
        private GroupWraperFullHelper GroupWraperFullHelper { get; }
        private VirtualRoomsMembersManager VirtualRoomsMembersManager { get; }

        public GroupController(
            ApiContext apiContext,
            MessageService messageService,
            UserManager userManager,
            PermissionContext permissionContext,
            MessageTarget messageTarget,
            GroupWraperFullHelper groupWraperFullHelper,
            CoreBaseSettings coreBaseSettings,
            VirtualRoomsMembersManager virtualRoomsMembers)
        {
            ApiContext = apiContext;
            MessageService = messageService;
            UserManager = userManager;
            PermissionContext = permissionContext;
            MessageTarget = messageTarget;
            GroupWraperFullHelper = groupWraperFullHelper;
            CoreBaseSettings = coreBaseSettings;
            VirtualRoomsMembersManager = virtualRoomsMembers;
        }

        [Read]
        public IEnumerable<GroupWrapperSummary> GetAll()
        {
            var result = UserManager.GetDepartments().Select(r => r);
            if (!string.IsNullOrEmpty(ApiContext.FilterValue))
            {
                result = result.Where(r => r.Name.Contains(ApiContext.FilterValue, StringComparison.InvariantCultureIgnoreCase));
            }
            return result.Select(x => new GroupWrapperSummary(x, UserManager));
        }

        [Read("full")]
        public IEnumerable<GroupWrapperFull> GetAllWithMembers()
        {
            var result = UserManager.GetDepartments().Select(r => r);
            if (!string.IsNullOrEmpty(ApiContext.FilterValue))
            {
                result = result.Where(r => r.Name.Contains(ApiContext.FilterValue, StringComparison.InvariantCultureIgnoreCase));
            }
            return result.Select(r => GroupWraperFullHelper.Get(r, true));
        }

        [Read("{groupid}")]
        public GroupWrapperFull GetById(Guid groupid)
        {
            return GroupWraperFullHelper.Get(GetGroupInfo(groupid), true);
        }

        [Read("user/{userid}")]
        public IEnumerable<GroupWrapperSummary> GetByUserId(Guid userid)
        {
            return UserManager.GetUserGroups(userid).Select(x => new GroupWrapperSummary(x, UserManager));
        }

        [Create]
        public GroupWrapperFull AddGroupFromBody([FromBody] GroupModel groupModel)
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
            PermissionContext.DemandPermissions(Constants.Action_EditGroups, Constants.Action_AddRemoveUser);

            var group = UserManager.SaveGroupInfo(new GroupInfo { Name = groupModel.GroupName });

            TransferUserToDepartment(groupModel.GroupManager, @group, true);
            if (groupModel.Members != null)
            {
                foreach (var member in groupModel.Members)
                {
                    TransferUserToDepartment(member, group, false);
                }
            }

            MessageService.Send(MessageAction.GroupCreated, MessageTarget.Create(group.ID), group.Name);

            return GroupWraperFullHelper.Get(group, true);
        }

        [Update("{groupid}")]
        public GroupWrapperFull UpdateGroupFromBody(Guid groupid, [FromBody] GroupModel groupModel)
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
            var group = UserManager.GetGroups().SingleOrDefault(x => x.ID == groupid).NotFoundIfNull("group not found");

            if (group.CategoryID == Constants.LinkedGroupCategoryId)
                PermissionContext.DemandPermissions(new GroupSecurityObject(groupid), Constants.Action_EditLinkedGroups);
            else
                PermissionContext.DemandPermissions(Constants.Action_EditGroups, Constants.Action_AddRemoveUser);

            if (groupid == Constants.LostGroupInfo.ID)
                throw new ItemNotFoundException("group not found");

            ErrorIfArchived(group);

            group.Name = groupModel.GroupName ?? group.Name;
            UserManager.SaveGroupInfo(group);

            RemoveMembersFrom(groupid, new GroupModel { Members = UserManager.GetUsersByGroup(groupid, EmployeeStatus.All).Select(u => u.ID).Where(id => !groupModel.Members.Contains(id)) });

            TransferUserToDepartment(groupModel.GroupManager, @group, true);
            if (groupModel.Members != null)
            {
                foreach (var member in groupModel.Members)
                {
                    TransferUserToDepartment(member, group, false);
                }
            }

            MessageService.Send(MessageAction.GroupUpdated, MessageTarget.Create(groupid), group.Name);

            return GetById(groupid);
        }

        [Delete("{groupid}")]
        public GroupWrapperFull DeleteGroup(Guid groupid)
        {
            PermissionContext.DemandPermissions(Constants.Action_EditGroups, Constants.Action_AddRemoveUser);
            var @group = GetGroupInfo(groupid);
            var groupWrapperFull = GroupWraperFullHelper.Get(group, false);

            if (CoreBaseSettings.VDR && group.CategoryID == Constants.LinkedGroupCategoryId ||
                group.CategoryID == Constants.ArchivedLinkedGroupCategoryId)
                throw new SecurityException("Unable to delete linked group");

            UserManager.DeleteGroup(groupid);

            MessageService.Send(MessageAction.GroupDeleted, MessageTarget.Create(group.ID), group.Name);

            return groupWrapperFull;
        }

        private GroupInfo GetGroupInfo(Guid groupid)
        {
            var group = UserManager.GetGroups().SingleOrDefault(x => x.ID == groupid).NotFoundIfNull("group not found");
            if (group.ID == Constants.LostGroupInfo.ID)
                throw new ItemNotFoundException("group not found");
            return @group;
        }

        [Update("{groupid}/members/{newgroupid}")]
        public GroupWrapperFull TransferMembersTo(Guid groupid, Guid newgroupid)
        {
            PermissionContext.DemandPermissions(Constants.Action_EditGroups, Constants.Action_AddRemoveUser);
            var oldgroup = GetGroupInfo(groupid);

            var newgroup = GetGroupInfo(newgroupid);

            ErrorIfArchived(oldgroup);
            ErrorIfArchived(newgroup);

            var users = UserManager.GetUsersByGroup(oldgroup.ID);
            foreach (var userInfo in users)
            {
                TransferUserToDepartment(userInfo.ID, newgroup, false);
            }
            return GetById(newgroupid);
        }

        [Create("{groupid}/members")]
        public GroupWrapperFull SetMembersToFromBody(Guid groupid, [FromBody] GroupModel groupModel)
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
            RemoveMembersFrom(groupid, new GroupModel { Members = UserManager.GetUsersByGroup(groupid).Select(x => x.ID) });
            AddMembersTo(groupid, groupModel);
            return GetById(groupid);
        }

        [Update("{groupid}/members")]
        public GroupWrapperFull AddMembersToFromBody(Guid groupid, [FromBody] GroupModel groupModel)
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
            var group = GetGroupInfo(groupid);

            ErrorIfArchived(group);

            if (group.CategoryID == Constants.LinkedGroupCategoryId)
            {
                PermissionContext.DemandPermissions(new GroupSecurityObject(groupid), Constants.Action_EditLinkedGroups);
                VirtualRoomsMembersManager.AddMembers(group, groupModel.Members);
            }
            else
            {
                PermissionContext.DemandPermissions(Constants.Action_EditGroups, Constants.Action_AddRemoveUser);
                foreach (var userId in groupModel.Members)
                {
                    TransferUserToDepartment(userId, group, false);
                }
            }

            return GetById(group.ID);
        }

        [Update("{groupid}/manager")]
        public GroupWrapperFull SetManagerFromBody(Guid groupid, [FromBody] SetManagerModel setManagerModel)
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

            ErrorIfArchived(group);

            if (UserManager.UserExists(setManagerModel.UserId))
            {
                UserManager.SetDepartmentManager(group.ID, setManagerModel.UserId);
            }
            else
            {
                throw new ItemNotFoundException("user not found");
            }
            return GetById(groupid);
        }

        [Delete("{groupid}/members")]
        public GroupWrapperFull RemoveMembersFromFromBody(Guid groupid, [FromBody] GroupModel groupModel)
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
            var group = GetGroupInfo(groupid);

            ErrorIfArchived(group);

            if (group.CategoryID == Constants.LinkedGroupCategoryId)
            {
                PermissionContext.DemandPermissions(new GroupSecurityObject(groupid), Constants.Action_EditLinkedGroups);
                VirtualRoomsMembersManager.RemoveMembers(group, groupModel.Members);
            }
            else
            {
                PermissionContext.DemandPermissions(Constants.Action_EditGroups, Constants.Action_AddRemoveUser);
                foreach (var userId in groupModel.Members)
                {
                    RemoveUserFromDepartment(userId, group);
                }
            }

            return GetById(group.ID);
        }

        private void RemoveUserFromDepartment(Guid userId, GroupInfo @group)
        {
            if (!UserManager.UserExists(userId)) return;

            var user = UserManager.GetUsers(userId);
            UserManager.RemoveUserFromGroup(user.ID, @group.ID);
            UserManager.SaveUserInfo(user);
        }

        private void TransferUserToDepartment(Guid userId, GroupInfo group, bool setAsManager)
        {
            if (!UserManager.UserExists(userId) && userId != Guid.Empty) return;

            if (setAsManager)
            {
                UserManager.SetDepartmentManager(@group.ID, userId);
            }
            UserManager.AddUserIntoGroup(userId, @group.ID);
        }

        private void ErrorIfArchived(GroupInfo group)
        {
            if (group.CategoryID == Constants.ArchivedLinkedGroupCategoryId)
                throw new SecurityException("The archived group cannot be changed");
        }
    }
}