﻿using System.Collections.Generic;
using System;

using ASC.Common;
using ASC.Web.Files.Utils;
using System.Linq;
using ASC.Api.Documents;

namespace ASC.Files.Core.Helpers
{
    [Scope]
    public class VirtualRoomsHelper
    {
        public FileSharing FileSharing { get; set; }

        public VirtualRoomsHelper(FileSharing fileSharing)
        {
            FileSharing = fileSharing;
        }

        public Guid GetLinkedGroupId<T>(Folder<T> folder)
        {
            if (folder.FolderType != FolderType.Custom
                && folder.FolderType != FolderType.CustomPrivacy)
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
                if (!virtualRoomsIDsByProviderId.Contains(wrapper.Key)) continue;

                var ids = virtualRoomsIDsByProviderId[wrapper.Key];
                wrapper.Value.Folders = wrapper.Value.Folders.Where(f => ids.Contains(((FolderWrapper<string>)f).Id)).ToList();
            }

            return wrappersByProvider.Values;
        }
    }
}