using System;
using System.Collections.Generic;
using System.Linq;

using ASC.Common;
using ASC.Common.Security.Authorizing;
using ASC.Core;
using ASC.Web.Files.Utils;

using Constants = ASC.Core.Users.Constants;

namespace ASC.Files.Core.Helpers
{
    [Scope]
    public class VirtualRoomsHelper
    {
        private FileSharing FileSharing { get; }
        private AuthorizationManager AuthorizationManager { get; }

        public VirtualRoomsHelper(FileSharing fileSharing,
            AuthorizationManager authorizationManager)
        {
            FileSharing = fileSharing;
            AuthorizationManager = authorizationManager;
        }

        public Guid GetLinkedGroupId<T>(Folder<T> folder)
        {
            if (folder.FolderType != FolderType.VirtualRoom
                && folder.FolderType != FolderType.PrivacyVirtualRoom)
                return default(Guid);

            var ace = FileSharing.
                GetSharedInfo(new List<T>(), new List<T> { folder.ID })
                .SingleOrDefault(i => i.SubjectGroup);

            if (ace == null)
                return default(Guid);

            return ace.SubjectId;
        }

        public bool IsRoomAdministrator(Guid userId, Guid groupId)
        {
            var record = AuthorizationManager.GetAces(userId,
                Constants.Action_EditLinkedGroups.ID)
                .FirstOrDefault(r => r.ObjectId == AzObjectIdHelper
                .GetFullObjectId(new GroupSecurityObject(groupId)));

            return record != null;
        }

        public void AddAdminRoomPrivilege(Guid userId, Guid groupId)
        {
            var record = CreateLinkedGroupEditingRecord(userId, groupId);

            AuthorizationManager.AddAce(record);
        }

        public void DeleteAdminRoomPrivilege(Guid userId, Guid groupId)
        {
            var record = CreateLinkedGroupEditingRecord(userId, groupId);

            AuthorizationManager.RemoveAce(record);
        }

        public void ArchiveLinkedGroup<T>(Folder<T> folder, UserManager userManager)
        {
            SetLinkedGroupCategory(Constants.ArchivedLinkedGroupCategoryId,
                folder, userManager);
        }

        public void UnarchiveLinkedGroup<T>(Folder<T> folder, UserManager userManager)
        {
            SetLinkedGroupCategory(Constants.LinkedGroupCategoryId,
                folder, userManager);
        }

        private void SetLinkedGroupCategory<T>(Guid categoryId, Folder<T> folder, UserManager userManager)
        {
            var groupId = GetLinkedGroupId(folder);
            var group = userManager.GetGroupInfo(groupId);
            group.CategoryID = categoryId;
            userManager.SaveLinkedGroupInfo(group);
        }

        private AzRecord CreateLinkedGroupEditingRecord(Guid userId, Guid groupId)
        {
            return new AzRecord(userId,
                Constants.Action_EditLinkedGroups.ID,
                AceType.Allow,
                new GroupSecurityObject(groupId));
        }
    }
}
