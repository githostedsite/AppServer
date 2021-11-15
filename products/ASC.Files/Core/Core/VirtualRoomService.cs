using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;

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
    public class VirtualRoomService
    {
        private const string ModuleName = "files";
        private FileStorageService<int> FileStorageService { get; }
        private GlobalFolderHelper GlobalFolderHelper { get; }
        private FileSecurityCommon FileSecurityCommon { get; }
        private AuthorizationManager AuthorizationManager { get; }
        private SecurityContext SecurityContext { get; }
        private IFolderDao<int> FolderDao { get; }
        private UserManager UserManager { get; }

        public VirtualRoomService(FileStorageService<int> storageService, UserManager userManager,
            IFolderDao<int> folderDao, GlobalFolderHelper globalFolderHelper, 
            AuthorizationManager authorizationManager, FileSecurityCommon fileSecurityCommon,
            SecurityContext securityContext)
        {
            FileStorageService = storageService;
            UserManager = userManager;
            FolderDao = folderDao;
            GlobalFolderHelper = globalFolderHelper;
            AuthorizationManager = authorizationManager;
            FileSecurityCommon = fileSecurityCommon;
            SecurityContext = securityContext;
        }

        public Folder<int> CreateRoom(string title)
        {
            var group = UserManager.SaveGroupInfo(new GroupInfo
            {
                Name = title,
                CategoryID = ASC.Core.Users.Constants.LinkedGroupCategoryId
            });

            var folderId = FolderDao.GetFolderID(ModuleName, "custom", group.ID.ToString(), true, title);

            ShareRoomForGroup(folderId, group.ID);

            return FileStorageService.GetFolder(folderId);
        }

        public List<FileOperationResult> DeleteRoom(int folderId)
        {
            var groupId = GetLinkedGroupId(folderId);

            UserManager.DeleteGroup(groupId);

            return FileStorageService.DeleteFolder("delete", folderId);
        }

        public Folder<int> RenameRoom(int folderId, string title)
        {
            var groupId = GetLinkedGroupId(folderId);
            
            var group = UserManager.GetGroupInfo(groupId);
            group.Name = title;

            UserManager.SaveGroupInfo(group);
            var folder = FileStorageService.FolderRename(folderId, title);

            return folder;
        }

        public List<Folder<int>> GetRooms()
        {
            var folderIDs = GlobalFolderHelper.FoldersCustom;
            return folderIDs.Select(id => FileStorageService.GetFolder(id)).ToList();
        }

        public List<AceWrapper> AddUserIntoRoom(int folderId, Guid userId)
        {
            if (!UserManager.UserExists(userId))
                throw new Exception(); // TODO: Exception

            var groupId = GetLinkedGroupId(folderId);

            UserManager.AddUserIntoLinkedGroup(userId, groupId);

            var store = new Dictionary<Guid, AceWrapper>();

            var sharedInfos = FileStorageService.GetSharedInfo(new List<int>(), new List<int> { folderId });

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

        public bool RemoveUserFromRoom(int folderId, Guid userId)
        {
            var groupId = GetLinkedGroupId(folderId);
            var objectId = new GroupSecurityObject(groupId);

            var record = AuthorizationManager.GetAces(userId, 
                Constants.Action_EditLinkedGroups.ID)
                .FirstOrDefault(r => r.ObjectId == AzObjectIdHelper.GetFullObjectId(objectId));

            if (record == null)
            {
                UserManager.RemoveUserFromLinkedGroup(userId, groupId);
                return true;
            }

            if (!FileSecurityCommon.IsAdministrator(SecurityContext.CurrentAccount.ID))
                throw new SecurityException("Not enough rights"); // TODO: Exception

            RemoveAdminAce(groupId, userId);
            UserManager.RemoveUserFromLinkedGroup(userId, groupId);
            SetRoomSecurity(folderId, userId, FileShare.None);

            return true;
        }

        public bool SetRoomSecurity(int folderId, Guid userId, FileShare share)
        {
            var aceCollection = new AceCollection<int>
            {
                Files = new List<int>(),
                Folders = new List<int>() { folderId },
                Aces = new List<AceWrapper>
                {
                    new AceWrapper
                    {
                        Share = share,
                        SubjectId = userId
                    }
                }
            };

            FileStorageService.SetAceObject(aceCollection, false);

            return true;
        }

        public bool SetRoomAdministartor(int folderId, Guid userId)
        {
            var groupId = GetLinkedGroupId(folderId);

            var record = new AzRecord(userId,
                ASC.Core.Users.Constants.Action_EditLinkedGroups.ID,
                Common.Security.Authorizing.AceType.Allow,
                new GroupSecurityObject(groupId));

            AuthorizationManager.AddAce(record);

            SetRoomSecurity(folderId, userId, FileShare.ReadWrite);

            return true;
        }

        public bool RemoveRoomAdministrator(int folderId, Guid userId)
        {
            var groupId = GetLinkedGroupId(folderId);

            RemoveAdminAce(groupId, userId);
            SetRoomSecurity(folderId, userId, FileShare.None);

            return true;
        }

        private void RemoveAdminAce(Guid groupId, Guid userId)
        {
            var record = new AzRecord(userId,
                ASC.Core.Users.Constants.Action_EditLinkedGroups.ID,
                Common.Security.Authorizing.AceType.Allow,
                new GroupSecurityObject(groupId));

            AuthorizationManager.RemoveAce(record);
        }

        private void ShareRoomForGroup(int folderId, Guid groupId)
        {
            var aceCollection = new AceCollection<int>
            {
                Files = new List<int>(),
                Folders = new List<int> { folderId },
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

        private Guid GetLinkedGroupId(int folderId)
        {
            var folder = FileStorageService.GetFolder(folderId);

            if (folder.FolderType != FolderType.Custom)
                throw new Exception("Entity is not virtual room");

            var ace = FileStorageService.
                GetSharedInfo(new List<int>(), new List<int> { folderId })
                .SingleOrDefault(i => i.SubjectGroup);

            if (ace == null)
                throw new Exception("Virtual room deleted");

            return ace.SubjectId;
        }
    }
}
