using System.Collections.Generic;
using System;

using ASC.Common;
using ASC.Web.Files.Utils;
using System.Linq;

namespace ASC.Files.Core.Helpers
{
    [Scope]
    public class LinkedFolderHelper
    {
        public FileSharing FileSharing { get; set; }

        public LinkedFolderHelper(FileSharing fileSharing)
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
    }
}
