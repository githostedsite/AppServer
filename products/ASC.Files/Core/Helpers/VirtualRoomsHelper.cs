using System.Collections.Generic;
using System;

using ASC.Common;
using ASC.Web.Files.Utils;
using System.Linq;
using ASC.Api.Documents;
using ASC.Core;
using Microsoft.AspNetCore.Identity;
using ASC.Core.Users;

namespace ASC.Files.Core.Helpers
{
    [Scope]
    public class VirtualRoomsHelper
    {
        private FileSharing FileSharing { get; }

        public VirtualRoomsHelper(FileSharing fileSharing)
        {
            FileSharing = fileSharing;
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

        public IEnumerable<FolderContentWrapper<string>> FilterThirdPartyStorage(
            IEnumerable<FolderContentWrapper<string>> inputWrappers, IEnumerable<string> virtualRoomsIDs)
        {
            var wrappersByProvider = inputWrappers.ToDictionary(r => r.Current.Id);
            var virtualRoomsIDsByProviderId = virtualRoomsIDs.ToLookup(r => ThirdPartyHelper.GetFullProviderId(r));

            foreach (var wrapper in wrappersByProvider)
            {
                if (!virtualRoomsIDsByProviderId.Contains(wrapper.Key))
                {
                    wrapper.Value.Folders = new List<FileEntryWrapper>();
                    continue;
                };

                var ids = virtualRoomsIDsByProviderId[wrapper.Key];
                wrapper.Value.Folders = wrapper.Value.Folders.Where(f => ids.Contains(((FolderWrapper<string>)f).Id)).ToList();
            }

            return wrappersByProvider.Values;
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
            userManager.SaveGroupInfo(group);
        }
    }
}
