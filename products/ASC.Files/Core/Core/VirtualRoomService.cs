using System;
using System.Collections.Generic;
using System.Linq;

using ASC.Api.Documents;
using ASC.Common;
using ASC.Common.Security.Authorizing;
using ASC.Core;
using ASC.Core.Users;
using ASC.Files.Core.Security;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Services.WCFService;
using ASC.Web.Files.Services.WCFService.FileOperations;

using Constants = ASC.Core.Users.Constants;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Files.Core.Core
{
    [Scope]
    public class VirtualRoomService<T>
    {
        private FileStorageService<T> FileStorageService { get; }
        private FileSecurityCommon FileSecurityCommon { get; }
        private FileShareParamsHelper FileShareParamsHelper { get; }
        private AuthorizationManager AuthorizationManager { get; }
        private SecurityContext SecurityContext { get; }
        private UserManager UserManager { get; }

        public VirtualRoomService(FileStorageService<T> storageService, 
            UserManager userManager, 
            AuthorizationManager authorizationManager, 
            FileSecurityCommon fileSecurityCommon,
            SecurityContext securityContext,
            FileShareParamsHelper fileShareParamsHelper)
        {
            FileStorageService = storageService;
            UserManager = userManager;
            AuthorizationManager = authorizationManager;
            FileSecurityCommon = fileSecurityCommon;
            SecurityContext = securityContext;
            FileShareParamsHelper= fileShareParamsHelper;
        }

        public Folder<T> CreateRoom(string title, bool privacy)
        {
            var group = UserManager.SaveGroupInfo(new GroupInfo
            {
                Name = title,
                CategoryID = Constants.LinkedGroupCategoryId
            });

            var folder = FileStorageService.CreateNewRootFolder(title, privacy ? FolderType.CustomPrivacy
                : FolderType.Custom, group.ID.ToString());

            ShareRoomForGroup(folder.ID, group.ID);

            return folder;
        }

        public List<FileOperationResult> DeleteRoom(T folderId)
        {
            var groupId = GetLinkedGroupId(folderId);

            UserManager.DeleteGroup(groupId);

            return FileStorageService.DeleteFolder("delete", folderId);
        }

        public Folder<T> RenameRoom(T folderId, string title)
        {
            var groupId = GetLinkedGroupId(folderId);
            
            var group = UserManager.GetGroupInfo(groupId);
            group.Name = title;

            UserManager.SaveGroupInfo(group);
            var folder = FileStorageService.FolderRename(folderId, title);

            return folder;
        }

        public List<AceWrapper> AddUsersIntoRoom(T folderId, IEnumerable<Guid> userIDs)
        {
            var groupId = GetLinkedGroupId(folderId);

            foreach (var id in userIDs)
            {
                if (!UserManager.UserExists(id)) continue;

                UserManager.AddUserIntoLinkedGroup(id, groupId);
            }

            return GetSharedInfo(folderId, groupId);
        }

        public List<AceWrapper> RemoveUsersFromRoom(T folderId, IEnumerable<Guid> userIDs)
        {
            var groupId = GetLinkedGroupId(folderId);
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

            return GetSharedInfo(folderId, groupId);
        }

        public List<AceWrapper> SetRoomSecurityInfo(T folderId, IEnumerable<FileShareParams> shareParams, bool notify,
            string sharingMessage)
        {
            var groupId = GetLinkedGroupId(folderId);
            var isAdmin = FileSecurityCommon.IsAdministrator(SecurityContext.CurrentAccount.ID);
            List<AceWrapper> result = new List<AceWrapper>();

            foreach (var ace in shareParams.Select(FileShareParamsHelper.ToAceObject))
            {
                if (ace.SubjectGroup)
                    continue;

                if (ace.Share == FileShare.ReadWrite && isAdmin)
                    AddAdminRoomPrivilege(groupId, ace.SubjectId);

                if (ace.Share == FileShare.None && isAdmin)
                    RemoveAdminRoomPrivilege(groupId, ace.SubjectId);

                if ((ace.Share == FileShare.None || ace.Share == FileShare.Restrict) 
                    && IsRoomAdministartor(ace.SubjectId, groupId))
                    continue;

                result.Add(ace);
            }

            var aceCollection = new AceCollection<T>
            {
                Files = new List<T>(),
                Folders = new List<T> { folderId },
                Aces = result,
                Message = sharingMessage
            };

            FileStorageService.SetAceObject(aceCollection, notify);

            return GetSharedInfo(folderId, groupId);
        }

        private void AddAdminRoomPrivilege(Guid groupId, Guid userId)
        {
            var record = new AzRecord(userId,
                Constants.Action_EditLinkedGroups.ID,
                AceType.Allow,
                new GroupSecurityObject(groupId));

            AuthorizationManager.AddAce(record);
        }

        private void RemoveAdminRoomPrivilege(Guid groupId, Guid userId)
        {
            var record = new AzRecord(userId,
                Constants.Action_EditLinkedGroups.ID,
                AceType.Allow,
                new GroupSecurityObject(groupId));

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

        private Guid GetLinkedGroupId(T folderId)
        {
            var folder = FileStorageService.GetFolder(folderId);

            if (folder.FolderType != FolderType.Custom 
                && folder.FolderType != FolderType.CustomPrivacy)
                throw new Exception("Entity is not virtual room");

            var ace = FileStorageService.
                GetSharedInfo(new List<T>(), new List<T> { folderId })
                .SingleOrDefault(i => i.SubjectGroup);

            if (ace == null)
                throw new Exception("Virtual room deleted");

            return ace.SubjectId;
        }
        
        private bool IsRoomAdministartor(Guid userId, Guid groupId)
        {
            var record = AuthorizationManager.GetAces(userId,
                Constants.Action_EditLinkedGroups.ID)
                .FirstOrDefault(r => r.ObjectId == AzObjectIdHelper
                .GetFullObjectId(new GroupSecurityObject(groupId)));

            return record != null;
        }

        private List<AceWrapper> GetSharedInfo(T folderId, Guid groupId)
        {
            var store = new Dictionary<Guid, AceWrapper>();

            var sharedInfos = FileStorageService.GetSharedInfo(new List<T>(), new List<T> { folderId });

            foreach (var info in sharedInfos)
            {
                if (info.SubjectGroup)
                    continue;

                store.Add(info.SubjectId, info);
            }

            var users = UserManager.GetUsersByGroup(groupId);

            foreach (var user in users)
            {
                if (store.ContainsKey(user.ID))
                    continue;

                store.Add(user.ID, new AceWrapper
                {
                    SubjectId = user.ID,
                    Share = FileShare.Read
                });
            }

            return store.Values.ToList();
        }
    }
}
