using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;

using ASC.Common;
using ASC.Core;
using ASC.Core.Users;
using ASC.Files.Core.Helpers;
using ASC.Files.Core.Security;
using ASC.MessagingSystem;
using ASC.Web.Files.Helpers;
using ASC.Web.Files.Services.WCFService;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace ASC.Files.Core.VirtualRooms
{
    [Scope]
    public class VirtualRoomsMembersManager
    {
        private readonly UserManager _userManager;
        private readonly FileSecurity _fileSecurity;
        private readonly AuthContext _authContext;
        private readonly FileSecurityCommon _fileSecurityCommon;
        private readonly FileStorageService<int> _fileStorageServiceInt;
        private readonly FileStorageService<string> _fileStorageServiceString;
        private readonly FilesMessageService _filesMessageService;
        private readonly VirtualRoomsHelper _virtualRoomsHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private IDictionary<string, StringValues> Headers => _httpContextAccessor?.HttpContext?.Request?.Headers;

        public VirtualRoomsMembersManager(
            UserManager userManager,
            FileSecurity fileSecurity,
            AuthContext authContext,
            FileSecurityCommon fileSecurityCommon,
            FileStorageService<int> fileStorageServiceInt,
            FileStorageService<string> fileStorageServiceString,
            FilesMessageService filesMessageService,
            VirtualRoomsHelper virtualRoomsHelper,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _fileSecurity = fileSecurity;
            _authContext = authContext;
            _fileSecurityCommon = fileSecurityCommon;
            _fileStorageServiceInt = fileStorageServiceInt;
            _fileStorageServiceString = fileStorageServiceString;
            _filesMessageService = filesMessageService;
            _virtualRoomsHelper = virtualRoomsHelper;
            _httpContextAccessor = httpContextAccessor;
        }

        public void AddMembers(GroupInfo group, IEnumerable<Guid> userIDs)
        {
            ErrorIfArchived(group);

            var addedUsers = new List<string>(userIDs.Count());

            foreach (var userId in userIDs)
            {
                if (!_userManager.UserExists(userId) && userId != Guid.Empty) return;

                _userManager.AddUserIntoLinkedGroup(userId, group.ID);
                addedUsers.Add(userId.ToString());
            }

            if (addedUsers.Any())
            {
                var record = GetRecord(group.ID);
                SendMessage(record.EntryId, MessageAction.AddedUserIntoVirtualRoom, addedUsers.ToArray());
            }
        }

        public void RemoveMembers(GroupInfo group, IEnumerable<Guid> userIDs)
        {
            ErrorIfArchived(group);

            var record = GetRecord(group.ID);

            if (record == null) return;

            var objectId = new GroupSecurityObject(group.ID);
            var removedUsers = new List<Guid>();
            var isAdmin = _fileSecurityCommon.IsAdministrator(_authContext.CurrentAccount.ID);

            foreach (var id in userIDs)
            {
                if (!_userManager.UserExists(id)) continue;

                if (!_virtualRoomsHelper.IsRoomAdministrator(id, group.ID))
                {
                    _userManager.RemoveUserFromLinkedGroup(id, group.ID);
                    removedUsers.Add(id);

                    continue;
                }

                if (isAdmin)
                {
                    _virtualRoomsHelper.DeleteAdminRoomPrivilege(group.ID, id);
                    _userManager.RemoveUserFromLinkedGroup(id, group.ID);

                    removedUsers.Add(id);
                }
            }

            if (removedUsers.Any())
            {
                DeleteFolderAces(record.EntryId, userIDs);
                SendMessage(record.EntryId, MessageAction.DeletedUserFromVirtualRoom, 
                    removedUsers.Select(id => id.ToString()).ToArray());
            }
        }

        private void DeleteFolderAces(object entryId, IEnumerable<Guid> usersIds)
        {
            var aces = new List<AceWrapper>(usersIds.Select(r => new AceWrapper
            {
                Share = FileShare.None,
                SubjectId = r
            }));

            if (entryId is int)
            {
                var folderId = (int)entryId;

                var collection = new AceCollection<int>
                {
                    Files = new List<int>(),
                    Folders = new List<int>() { folderId },
                    Aces = aces
                };

                _fileStorageServiceInt.SetAceObject(collection, false);
            }
            else
            {
                var collection = new AceCollection<string>
                {
                    Files = new List<string>(),
                    Folders = new List<string>() { (string)entryId },
                    Aces = aces
                };

                _fileStorageServiceString.SetAceObject(collection, false);
            }
        }

        private void SendMessage(object entryId, MessageAction action, string[] usersIds)
        {
            if (entryId is int)
            {
                var entry = _fileStorageServiceInt.GetFolder((int)entryId);
                _filesMessageService.Send(entry, Headers, action, usersIds);
            }
            else
            {
                var entry = _fileStorageServiceString.GetFolder((string)entryId);
                _filesMessageService.Send(entry, Headers, action, usersIds);
            }
        }
        
        private FileShareRecord GetRecord(Guid groupId)
        {
            return _fileSecurity.GetShares<int>(new List<Guid> { groupId })
                    .FirstOrDefault(r => r.EntryType == FileEntryType.Folder);
        }

        private void ErrorIfArchived(GroupInfo group)
        {
            if (group.CategoryID == Constants.ArchivedLinkedGroupCategoryId)
                throw new SecurityException();
        }
    }
}
