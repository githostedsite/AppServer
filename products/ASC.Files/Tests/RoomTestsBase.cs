using System;
using System.Collections.Generic;
using System.Linq;

using ASC.Api.Documents;
using ASC.Core.Users;

namespace ASC.Files.Tests
{
    public class RoomTestsBase : BaseFilesTests
    {
        protected void AddUserInRoomAsAdmin(int folderId, GroupInfo group, Guid userId)
        {
            VirtualRoomsMembersManager.AddMembers(group, new List<Guid> { userId });

            var shareParam = new FileShareParams { Access = Core.Security.FileShare.RoomAdministrator, ShareTo = userId };
            FilesControllerHelper.SetSecurityInfo(new List<int>(), new List<int> { folderId },
                new List<FileShareParams> { shareParam }, false, string.Empty);
        }

        protected (FolderWrapper<int>, Guid) CreateVirtualRoom(string title)
        {
            var roomFolder = FilesControllerHelper.CreateVirtualRoom(title, false);
            var groupId = FileStorageService.GetSharedInfo(new List<int>(), new List<int> { roomFolder.Id })
                .SingleOrDefault(s => s.SubjectGroup).SubjectId;

            return (roomFolder, groupId);
        }
    }
}
