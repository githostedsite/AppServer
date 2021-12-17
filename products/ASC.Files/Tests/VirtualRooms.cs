using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ASC.Api.Documents;
using ASC.Common.Security.Authorizing;
using ASC.Web.Api.Models;

using NUnit.Framework;

namespace ASC.Files.Tests
{
    [TestFixture]
    public class VirtualRooms : BaseFilesTests
    {
        private Guid UserId1 { get; set; }
        private Guid UserId2 { get; set; }
        private (FolderWrapper<int>, Guid) VirtualRoomPair { get; set; }

        [OneTimeSetUp]
        public override void SetUp()
        {
            base.SetUp();

            UserId1 = CreateUser("TestUser1", "testuser1@gmail.com").ID;
            UserId2 = CreateUser("TestUser2", "testuser2@gmail.com").ID;

            var roomFolder = FilesControllerHelper.CreateVirtualRoom("TestVirtualRoom", false);
            var groupId = FileStorageService.GetSharedInfo(new List<int>(), new List<int> { roomFolder.Id })
                .SingleOrDefault(s => s.SubjectGroup).SubjectId;

            VirtualRoomPair = (roomFolder, groupId);
        }

        [Test]
        [Order(1)]
        [Description("Portal owner or administrator can create virtual rooms " +
            "(folder located in the root and associated with the group)")]
        public void CreateVirtualRoom_WithAdminPrivileges_ReturnFolderWrapper()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);

            var title = "TestVirtualRoom";

            var roomFolder = FilesControllerHelper.CreateVirtualRoom(title, false);
            var groupName = FileStorageService.GetSharedInfo(new List<int>(), new List<int> { roomFolder.Id })
                .SingleOrDefault(s => s.SubjectGroup).SubjectName;

            Assert.IsNotNull(roomFolder);
            Assert.IsTrue(roomFolder.RootFolderType == Core.FolderType.VirtualRoom);
            Assert.IsTrue(roomFolder.ParentId == 0);
            Assert.AreEqual(roomFolder.Title, groupName);
        }

        [Test]
        [Order(2)]
        [Description("Regular user cannot create virtual rooms")]
        public void CreateVirtualRoom_WithoutAdminPrivileges_ThrowException()
        {
            SecurityContext.AuthenticateMe(UserId1);

            Assert.Throws<AuthorizingException>(() =>
                FilesControllerHelper.CreateVirtualRoom("TestVirtualRoom", false));
        }

        [Test]
        [Order(3)]
        [Description("Portal owner or administrator can rename virtual rooms")]
        public void RenameVirtualRoom_WithAdminPrivileges_ReturnUpdatedFolderWrapper()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);

            var newTitle = "RenamedVirtualRoom";
            var roomFolder = FilesControllerHelper.CreateVirtualRoom("TestRoom", false);

            var renamedRoomFolder = FilesControllerHelper.RenameVirtualRoom(roomFolder.Id, newTitle);
            var renamedGroupName = FileStorageService.GetSharedInfo(new List<int>(), new List<int> { renamedRoomFolder.Id })
                .SingleOrDefault(s => s.SubjectGroup).SubjectName;

            Assert.IsNotNull(renamedRoomFolder);
            Assert.IsTrue(renamedRoomFolder.RootFolderType == Core.FolderType.VirtualRoom);
            Assert.IsTrue(renamedRoomFolder.ParentId == 0);
            Assert.AreEqual(renamedRoomFolder.Title, newTitle);
            Assert.AreEqual(renamedGroupName, newTitle);
        }

        [Test]
        [Order(4)]
        [Description("Regular user cannot rename virtual rooms")]
        public void RenameVirtualRoom_WithoutAdminPrivileges_ThrowException()
        {
            SecurityContext.AuthenticateMe(UserId1);

            Assert.Throws<InvalidOperationException>(() =>
                FilesControllerHelper.RenameVirtualRoom(VirtualRoomPair.Item1.Id, "RenamedVirtualRoom"));
        }

        [Test]
        [Order(5)]
        [Description("Portal owner or administrator can add users to virtual room")]
        public void AddUserIntoRoom_WithAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);

            FilesControllerHelper.AddMembersIntoRoom(VirtualRoomPair.Item1.Id, new[] { UserId1 });

            Assert.Contains(VirtualRoomPair.Item2, UserManager.GetUserGroupsId(UserId1).ToArray());
        }

        [Test]
        [Order(6)]
        [Description("Portal owner or administrator can remove users from virtual room")]
        public void RemoveUserFromRoom_WithAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);

            FilesControllerHelper.RemoveMembersFromRoom(VirtualRoomPair.Item1.Id, new[] { UserId1 });

            Assert.IsTrue(UserManager.GetUserGroupsId(UserId1)
                .FirstOrDefault(id => id == VirtualRoomPair.Item2) == Guid.Empty);
        }

        [Test]
        [Order(7)]
        [Description("Virtual room administrator can add users")]
        public void AddUserIntoRoom_WithRoomAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            FilesControllerHelper.AddMembersIntoRoom(VirtualRoomPair.Item1.Id, new[] { UserId1 });

            var shareParam = new FileShareParams { Access = Core.Security.FileShare.ReadWrite, ShareTo = UserId1 };
            FilesControllerHelper.SetSecurityInfo(new List<int>(), new List<int> { VirtualRoomPair.Item1.Id },
                new List<FileShareParams> { shareParam }, false, string.Empty);

            SecurityContext.AuthenticateMe(UserId1);
            FilesControllerHelper.AddMembersIntoRoom(VirtualRoomPair.Item1.Id, new[] { UserId2 });

            Assert.Contains(VirtualRoomPair.Item2, UserManager.GetUserGroupsId(UserId2).ToArray());
        }

        [Test]
        [Order(8)]
        [Description("Virtual room administrator can delete users")]
        public void RemoveUserFromRoom_WithRoomAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(UserId1);

            FilesControllerHelper.RemoveMembersFromRoom(VirtualRoomPair.Item1.Id, new[] { UserId2 });

            Assert.IsTrue(UserManager.GetUserGroupsId(UserId2)
                .FirstOrDefault(id => id == VirtualRoomPair.Item2) == Guid.Empty);
        }

        [Test]
        [Order(9)]
        [Description("Room administrator cannot assign a room administrator")]
        public void SetRoomSecurity_FullAccess_WithRoomAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(UserId1);
            FilesControllerHelper.AddMembersIntoRoom(VirtualRoomPair.Item1.Id, new[] { UserId2 });

            var shareParam = new FileShareParams { Access = Core.Security.FileShare.ReadWrite, ShareTo = UserId2 };
            FilesControllerHelper.SetSecurityInfo(new List<int>(), new List<int> { VirtualRoomPair.Item1.Id },
                new List<FileShareParams> { shareParam }, false, string.Empty);

            Assert.IsTrue(FilesControllerHelper
                .GetSecurityInfo(new List<int>(), new List<int> { VirtualRoomPair.Item1.Id })
                .FirstOrDefault(s => s.SharedTo.Equals(UserId2) && s.Access == Core.Security.FileShare.ReadWrite) == null);
        }

        [Test]
        [Order(9)]
        [Description("Portal owner or administrator can create files in the virtual room")]
        public void CreateFileInRoom_WithAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var roomFolder = FilesControllerHelper.CreateVirtualRoom("TestRoom", false);

            var file = FilesControllerHelper.CreateFile(roomFolder.Id, "TestFile.docx", 0);

            Assert.AreEqual(roomFolder.Id, file.FolderId);
            Assert.AreEqual(roomFolder.Id, file.RootFolderId);
            Assert.AreEqual(roomFolder.RootFolderType, file.RootFolderType);
        }

        [Test]
        [Order(10)]
        [Description("Portal owner or administrator can create folders in the virtual room")]
        public void CreateFolderInRoom_WithAdminPriveleges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var roomFolder = FilesControllerHelper.CreateVirtualRoom("TestRoom", false);

            var folder = FilesControllerHelper.CreateFolder(roomFolder.Id, "TestFolder");

            Assert.AreEqual(roomFolder.Id, folder.RootFolderId);
            Assert.AreEqual(roomFolder.RootFolderType, folder.RootFolderType);
        }

        [Test]
        [Order(11)]
        [Description("The room administrator can create files in the virtual room")]
        public void CreateFileInRoom_WithAdminRoomPriveleges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var roomFolder = FilesControllerHelper.CreateVirtualRoom("TestRoom", false);
            SetRoomAdmin(roomFolder.Id, UserId1);

            SecurityContext.AuthenticateMe(UserId1);

            var file = FilesControllerHelper.CreateFile(roomFolder.Id, "TestFile.docx", 0);

            Assert.AreEqual(roomFolder.Id, file.FolderId);
            Assert.AreEqual(roomFolder.Id, file.RootFolderId);
            Assert.AreEqual(roomFolder.RootFolderType, file.RootFolderType);
        }

        [Test]
        [Order(12)]
        [Description("The room administrator can create folders in the virtual room")]
        public void CreateFolderInRoom_WithAdminRoomPriveleges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var roomFolder = FilesControllerHelper.CreateVirtualRoom("TestRoom", false);
            SetRoomAdmin(roomFolder.Id, UserId1);

            SecurityContext.AuthenticateMe(UserId1);

            var folder = FilesControllerHelper.CreateFolder(roomFolder.Id, "TestFolder");

            Assert.AreEqual(roomFolder.Id, folder.RootFolderId);
            Assert.AreEqual(roomFolder.RootFolderType, folder.RootFolderType);
        }

        [Test]
        [Order(13)]
        [Description("The room administrator can assign rights to room members")]
        public void SetRoomSecurity_WithRoomAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var roomFolder = FilesControllerHelper.CreateVirtualRoom("TestRoom", false);
            SetRoomAdmin(roomFolder.Id, UserId1);
            FilesControllerHelper.AddMembersIntoRoom(roomFolder.Id, new[] { UserId2 });

            SecurityContext.AuthenticateMe(UserId1);
            SetSecurity(roomFolder.Id, UserId2, Core.Security.FileShare.FillForms);

            var info = FilesControllerHelper.GetSecurityInfo(new List<int>(), new List<int> { roomFolder.Id }).ToList();

            Assert.IsTrue(FilesControllerHelper
                .GetSecurityInfo(new List<int>(), new List<int> { roomFolder.Id })
                .FirstOrDefault(s => ((EmployeeWraperFull)s.SharedTo).Id.Equals(UserId2) && s.Access == Core.Security.FileShare.FillForms) != null);
        }

        [Test]
        [Order(13)]
        [Description("Portal owner and administrator can delete virtual room")]
        public void DeleteRoom_WithAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var roomFolder = FilesControllerHelper.CreateVirtualRoom("TestRoom", false);
            var groupId = FileStorageService.GetSharedInfo(new List<int>(), new List<int> { roomFolder.Id })
                .SingleOrDefault(s => s.SubjectGroup).SubjectId;

            FilesControllerHelper.DeleteFolder(roomFolder.Id, false, true);

            while (true)
            {
                var statuses = FileStorageService.GetTasksStatuses();

                if (statuses.TrueForAll(r => r.Finished))
                    break;
                Task.Delay(100);
            }

            Assert.IsTrue(FileStorageService.GetTasksStatuses().TrueForAll(r => string.IsNullOrEmpty(r.Error)));
            Assert.AreEqual(ASC.Core.Users.Constants.LostGroupInfo, UserManager.GetGroupInfo(groupId));
        }

        [Test]
        [Order(14)]
        [Description("Portal owner and administrator can archive virtual room")]
        public void ArchiveRoom_WithAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var roomFolder = FilesControllerHelper.CreateVirtualRoom("TestRoom", false);
            var groupId = FileStorageService.GetSharedInfo(new List<int>(), new List<int> { roomFolder.Id })
                .SingleOrDefault(s => s.SubjectGroup).SubjectId;

            FilesControllerHelper.DeleteFolder(roomFolder.Id, false, false);

            while (true)
            {
                var statuses = FileStorageService.GetTasksStatuses();

                if (statuses.TrueForAll(r => r.Finished))
                    break;
                Task.Delay(100);
            }

            var room = FilesControllerHelper.GetFolder(roomFolder.Id, Guid.Empty, Core.FilterType.None, false);

            Assert.IsTrue(FileStorageService.GetTasksStatuses().TrueForAll(r => string.IsNullOrEmpty(r.Error)));
            Assert.AreEqual(GlobalFolderHelper.FolderArchive, room.Current.ParentId);
            Assert.AreEqual(Core.FolderType.Archive, room.Current.RootFolderType);
            Assert.AreEqual(ASC.Core.Users.Constants.ArchivedLinkedGroupCategoryId, UserManager.GetGroupInfo(groupId).CategoryID);
        }

        [Test]
        [Order(15)]
        [Description("Portal owner and administrator can unarchive the virtual room")]
        public void UnarchiveRoom_WithAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var roomFolder = FilesControllerHelper.CreateVirtualRoom("TestRoom", false);
            var groupId = FileStorageService.GetSharedInfo(new List<int>(), new List<int> { roomFolder.Id })
                .SingleOrDefault(s => s.SubjectGroup).SubjectId;

            FilesControllerHelper.DeleteFolder(roomFolder.Id, false, false);

            while (true)
            {
                var statuses = FileStorageService.GetTasksStatuses();

                if (statuses.TrueForAll(r => r.Finished))
                    break;
                Task.Delay(100);
            }

            var json = " [ { \"folderIds\": [" + roomFolder.Id + "] }, { \"fileIds\": [ ] }, { \"destFolderId\": 0 } ]";

            FilesControllerHelper.MoveBatchItems(GetBatchModel(json));

            while (true)
            {
                var statuses = FileStorageService.GetTasksStatuses();

                if (statuses.TrueForAll(r => r.Finished))
                    break;
                Task.Delay(100);
            }

            var room = FilesControllerHelper.GetFolder(roomFolder.Id, Guid.Empty, Core.FilterType.None, false);

            Assert.AreEqual(0, room.Current.ParentId);
            Assert.AreEqual(Core.FolderType.VirtualRoom, room.Current.RootFolderType);
            Assert.AreEqual(ASC.Core.Users.Constants.LinkedGroupCategoryId, UserManager.GetGroupInfo(groupId).CategoryID);
        }
    }
}
