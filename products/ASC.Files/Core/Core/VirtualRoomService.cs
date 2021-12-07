using System;
using System.Collections.Generic;
using System.Linq;

using ASC.Common;
using ASC.Common.Security.Authorizing;
using ASC.Common.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Files.Core.Helpers;
using ASC.Files.Core.Security;
using ASC.Web.Files.Services.WCFService;

using Constants = ASC.Core.Users.Constants;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Files.Core.Core
{
    [Scope]
    public class VirtualRoomService<T>
    {
        private FileStorageService<T> FileStorageService { get; }
        private FileSecurityCommon FileSecurityCommon { get; }
        private VirtualRoomsHelper VirtualRoomsHelper { get; }
        private AuthorizationManager AuthorizationManager { get; }
        private SecurityContext SecurityContext { get; }
        private UserManager UserManager { get; }

        public VirtualRoomService(FileStorageService<T> storageService,
            UserManager userManager,
            AuthorizationManager authorizationManager,
            FileSecurityCommon fileSecurityCommon,
            SecurityContext securityContext,
            VirtualRoomsHelper virtualRoomsHelper)
        {
            FileStorageService = storageService;
            UserManager = userManager;
            AuthorizationManager = authorizationManager;
            FileSecurityCommon = fileSecurityCommon;
            SecurityContext = securityContext;
            VirtualRoomsHelper = virtualRoomsHelper;
        }

        public Folder<T> CreateRoom(string title, bool privacy, T parentId = default(T))
        {
            var group = UserManager.SaveGroupInfo(new GroupInfo
            {
                Name = title,
                CategoryID = Constants.LinkedGroupCategoryId
            });

            var folder = FileStorageService.CreateNewRootFolder(title, privacy ? FolderType.PrivacyVirtualRoom
                : FolderType.VirtualRoom, group.ID.ToString(), parentId);

            ShareRoomForGroup(folder.ID, group.ID);

            return folder;
        }

        public Folder<T> RenameRoom(T folderId, string title)
        {
            var folder = FileStorageService.GetFolder(folderId);
            var groupId = VirtualRoomsHelper.GetLinkedGroupId(folder);

            ErrorIfEmpty(groupId);

            var group = UserManager.GetGroupInfo(groupId);
            group.Name = title;

            UserManager.SaveGroupInfo(group);
            var renamedFolder = FileStorageService.FolderRename(folderId, title);

            return renamedFolder;
        }

        public void AddMembersIntoRoom(T folderId, IEnumerable<Guid> userIDs)
        {
            var folder = FileStorageService.GetFolder(folderId);
            var groupId = VirtualRoomsHelper.GetLinkedGroupId(folder);

            ErrorIfEmpty(groupId);

            foreach (var id in userIDs)
            {
                if (!UserManager.UserExists(id)) continue;

                UserManager.AddUserIntoLinkedGroup(id, groupId);
            }
        }

        public void RemoveMembersFromRoom(T folderId, IEnumerable<Guid> userIDs)
        {
            var folder = FileStorageService.GetFolder(folderId);
            var groupId = VirtualRoomsHelper.GetLinkedGroupId(folder);

            ErrorIfEmpty(groupId);

            var objectId = new GroupSecurityObject(groupId);
            var result = new List<Guid>();
            var isAdmin = FileSecurityCommon.IsAdministrator(SecurityContext.CurrentAccount.ID);

            foreach (var id in userIDs)
            {
                if (!UserManager.UserExists(id)) continue;

                if (!IsRoomAdministartor(id, groupId))
                {
                    UserManager.RemoveUserFromLinkedGroup(id, groupId);
                    result.Add(id);
                    continue;
                }

                if (isAdmin)
                {
                    RemoveAdminRoomPrivilege(groupId, id);
                    UserManager.RemoveUserFromLinkedGroup(id, groupId);
                    result.Add(id);
                }
            }

            var aces = new List<AceWrapper>(result.Select(r => new AceWrapper
            {
                Share = FileShare.None,
                SubjectId = r
            }));

            var collection = new AceCollection<T>
            {
                Files = new List<T>(),
                Folders = new List<T>() { folderId },
                Aces = aces
            };

            FileStorageService.SetAceObject(collection, false);
        }

        public List<T> ProcessAcesForRooms(IEnumerable<T> folderIds, List<AceWrapper> aces)
        {
            var result = new List<T>();
            var isAdmin = FileSecurityCommon.IsAdministrator(SecurityContext.CurrentAccount.ID);

            foreach (var folderId in folderIds)
            {
                var folder = FileStorageService.GetFolder(folderId);
                var groupId = VirtualRoomsHelper.GetLinkedGroupId(folder);

                if (groupId == Guid.Empty)
                {
                    result.Add(folderId);
                    continue;
                }

                foreach (var ace in aces)
                {
                    if (ace.SubjectGroup)
                        continue;

                    if (!UserManager.IsUserInGroup(ace.SubjectId, groupId))
                        continue;

                    if (ace.Share == FileShare.ReadWrite && isAdmin) // Only the owner or administrator of the portal can assign a room administrator
                    {
                        AddAdminRoomPrivilege(groupId, ace.SubjectId);
                        result.Add(folderId);
                        continue;
                    }

                    if ((ace.Share == FileShare.None || ace.Share == FileShare.Restrict) && isAdmin) // Only the owner or administrator of the portal can remove room administrator privileges
                    {
                        RemoveAdminRoomPrivilege(groupId, ace.SubjectId);
                        result.Add(folderId);
                        continue;
                    }

                    if (IsRoomAdministartor(ace.SubjectId, groupId)) // The room administrator cannot set rights for another room administrator
                        continue;

                    if (ace.Share != FileShare.ReadWrite)
                        result.Add(folderId);
                }
            }

            return result;
        }

        private void AddAdminRoomPrivilege(Guid groupId, Guid userId)
        {
            var record = CreateLinkedGroupEditingRecord(userId, groupId);

            AuthorizationManager.AddAce(record);
        }

        private void RemoveAdminRoomPrivilege(Guid groupId, Guid userId)
        {
            var record = CreateLinkedGroupEditingRecord(userId, groupId);

            AuthorizationManager.RemoveAce(record);
        }

        private void ShareRoomForGroup(T folderId, Guid groupId)
        {
            var aceCollection = new AceCollection<T>
            {
                Files = new List<T>(),
                Folders = new List<T> { folderId },
                Aces = new List<AceWrapper>
                {
                    new AceWrapper
                    {
                        Share = FileShare.Read,
                        SubjectId = groupId,
                        SubjectGroup = true,
                    }
                }
            };

            FileStorageService.SetAceObject(aceCollection, false);
        }

        private bool IsRoomAdministartor(Guid userId, Guid groupId)
        {
            var record = AuthorizationManager.GetAces(userId,
                Constants.Action_EditLinkedGroups.ID)
                .FirstOrDefault(r => r.ObjectId == AzObjectIdHelper
                .GetFullObjectId(new GroupSecurityObject(groupId)));

            return record != null;
        }

        private AzRecord CreateLinkedGroupEditingRecord(Guid userId, Guid groupId)
        {
            return new AzRecord(userId,
                Constants.Action_EditLinkedGroups.ID,
                AceType.Allow,
                new GroupSecurityObject(groupId));
        }

        private void ErrorIfEmpty(Guid guid)
        {
            if (guid == Guid.Empty)
                throw new ItemNotFoundException("Virtual room not found");
        }
    }
}
