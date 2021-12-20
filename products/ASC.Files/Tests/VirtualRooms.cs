using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private int _counter;
        private int Counter
        {
            get
            {
                _counter = ++_counter;
                return _counter;
            }
        }

        [OneTimeSetUp]
        public override void SetUp()
        {
            base.SetUp();

            CoreBaseSettings.VDR = true;

            UserId1 = CreateUser("TestUser1", "testuser1@gmail.com").ID;
            UserId2 = CreateUser("TestUser2", "testuser2@gmail.com").ID;
        }

        [Test]
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
        [Description("Regular user cannot create virtual rooms")]
        public void CreateVirtualRoom_WithoutAdminPrivileges_ThrowException()
        {
            SecurityContext.AuthenticateMe(UserId1);

            Assert.Throws<AuthorizingException>(() =>
                FilesControllerHelper.CreateVirtualRoom("TestVirtualRoom", false));
        }

        [Test]
        [Description("Portal owner or administrator can rename virtual rooms")]
        public void RenameVirtualRoom_WithAdminPrivileges_ReturnFolderWrapper()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);

            var newTitle = "RenamedVirtualRoom";
            var roomFolder = FilesControllerHelper.CreateVirtualRoom("TestVirtualRoom", false);

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
        [Description("Regular user cannot rename virtual rooms")]
        public void RenameVirtualRoom_WithoutAdminPrivileges_ThrowException()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var room = FilesControllerHelper.CreateVirtualRoom($"TestRoom{Counter}", false);

            SecurityContext.AuthenticateMe(UserId1);

            Assert.Throws<InvalidOperationException>(() =>
                FilesControllerHelper.RenameVirtualRoom(room.Id, "RenamedVirtualRoom"));
        }

        [Test]
        [Description("Portal owner or administrator can add users to virtual room")]
        public void AddUserIntoRoom_WithAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var room = CreateVirtualRoom($"TestRoom{Counter}");

            FilesControllerHelper.AddMembersIntoRoom(room.Item1.Id, new[] { UserId1 });

            Assert.Contains(room.Item2, UserManager.GetUserGroupsId(UserId1).ToArray());
        }

        [Test]
        [Description("Portal owner or administrator can remove users from virtual room")]
        public void RemoveUserFromRoom_WithAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var room = CreateVirtualRoom($"TestRoom{Counter}");
            FilesControllerHelper.AddMembersIntoRoom(room.Item1.Id, new[] { UserId1 });

            FilesControllerHelper.RemoveMembersFromRoom(room.Item1.Id, new[] { UserId1 });

            Assert.IsTrue(UserManager.GetUserGroupsId(UserId1)
                .FirstOrDefault(id => id == room.Item2) == Guid.Empty);
        }

        [Test]
        [Description("Virtual room administrator can add users")]
        public void AddUserIntoRoom_WithRoomAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var room = CreateVirtualRoom($"TestRoom{Counter}");
            SetRoomAdmin(room.Item1.Id, UserId1);

            SecurityContext.AuthenticateMe(UserId1);
            FilesControllerHelper.AddMembersIntoRoom(room.Item1.Id, new[] { UserId2 });

            Assert.Contains(room.Item2, UserManager.GetUserGroupsId(UserId2).ToArray());
        }

        [Test]
        [Description("Virtual room administrator can delete users")]
        public void RemoveUserFromRoom_WithRoomAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var roomPair = CreateVirtualRoom($"TestRoom{Counter}");
            FilesControllerHelper.AddMembersIntoRoom(roomPair.Item1.Id, new List<Guid> { UserId2 });
            SetRoomAdmin(roomPair.Item1.Id, UserId1);
            SecurityContext.AuthenticateMe(UserId1);

            FilesControllerHelper.RemoveMembersFromRoom(roomPair.Item1.Id, new[] { UserId2 });

            Assert.IsTrue(UserManager.GetUserGroupsId(UserId2)
                .FirstOrDefault(id => id == roomPair.Item2) == Guid.Empty);
        }

        [Test]
        [Description("Room administrator cannot assign a room administrator")]
        public void SetRoomSecurity_FullAccess_WithRoomAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var roomPair = CreateVirtualRoom($"TestRoom{Counter}");
            SetRoomAdmin(roomPair.Item1.Id, UserId1);
            FilesControllerHelper.AddMembersIntoRoom(roomPair.Item1.Id, new[] { UserId2 });
            SecurityContext.AuthenticateMe(UserId1);

            var shareParam = new FileShareParams { Access = Core.Security.FileShare.ReadWrite, ShareTo = UserId2 };
            FilesControllerHelper.SetSecurityInfo(new List<int>(), new List<int> { roomPair.Item1.Id },
                new List<FileShareParams> { shareParam }, false, string.Empty);

            Assert.IsTrue(FilesControllerHelper
                .GetSecurityInfo(new List<int>(), new List<int> { roomPair.Item1.Id })
                .FirstOrDefault(s => s.SharedTo.Equals(UserId2) && s.Access == Core.Security.FileShare.ReadWrite) == null);
        }

        [Test]
        [Description("Portal owner or administrator can create files in the virtual room")]
        public void CreateFileInRoom_WithAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var room = FilesControllerHelper.CreateVirtualRoom($"TestRoom{Counter}", false);

            var file = FilesControllerHelper.CreateFile(room.Id, "TestFile.docx", 0);

            Assert.AreEqual(room.Id, file.FolderId);
            Assert.AreEqual(room.Id, file.RootFolderId);
            Assert.AreEqual(room.RootFolderType, file.RootFolderType);
        }

        [Test]
        [Description("Portal owner or administrator can create folders in the virtual room")]
        public void CreateFolderInRoom_WithAdminPriveleges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var room = FilesControllerHelper.CreateVirtualRoom($"TestRoom{Counter}", false);

            var folder = FilesControllerHelper.CreateFolder(room.Id, "TestFolder");

            Assert.AreEqual(room.Id, folder.RootFolderId);
            Assert.AreEqual(room.RootFolderType, folder.RootFolderType);
        }

        [Test]
        [Description("The room administrator can create files in the virtual room")]
        public void CreateFileInRoom_WithAdminRoomPriveleges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var roomFolder = FilesControllerHelper.CreateVirtualRoom($"TestRoom{Counter}", false);
            SetRoomAdmin(roomFolder.Id, UserId1);

            SecurityContext.AuthenticateMe(UserId1);

            var file = FilesControllerHelper.CreateFile(roomFolder.Id, "TestFile.docx", 0);

            Assert.AreEqual(roomFolder.Id, file.FolderId);
            Assert.AreEqual(roomFolder.Id, file.RootFolderId);
            Assert.AreEqual(roomFolder.RootFolderType, file.RootFolderType);
        }

        [Test]
        [Description("The room administrator can create folders in the virtual room")]
        public void CreateFolderInRoom_WithAdminRoomPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var room = FilesControllerHelper.CreateVirtualRoom($"TestRoom{Counter}", false);
            SetRoomAdmin(room.Id, UserId1);

            SecurityContext.AuthenticateMe(UserId1);

            var folder = FilesControllerHelper.CreateFolder(room.Id, "TestFolder");

            Assert.AreEqual(room.Id, folder.RootFolderId);
            Assert.AreEqual(room.RootFolderType, folder.RootFolderType);
        }

        [Test]
        [Description("The room administrator can assign rights to room members")]
        public void SetRoomSecurity_WithRoomAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var room = FilesControllerHelper.CreateVirtualRoom($"TestRoom{Counter}", false);
            SetRoomAdmin(room.Id, UserId1);
            FilesControllerHelper.AddMembersIntoRoom(room.Id, new[] { UserId2 });

            SecurityContext.AuthenticateMe(UserId1);
            SetSecurity(room.Id, UserId2, Core.Security.FileShare.FillForms);

            var info = FilesControllerHelper.GetSecurityInfo(new List<int>(), new List<int> { room.Id }).ToList();

            Assert.IsTrue(FilesControllerHelper
                .GetSecurityInfo(new List<int>(), new List<int> { room.Id })
                .FirstOrDefault(s => ((EmployeeWraperFull)s.SharedTo).Id.Equals(UserId2) && s.Access == Core.Security.FileShare.FillForms) != null);
        }

        [Test]
        [Description("Portal owner and administrator can delete virtual room")]
        public void DeleteRoom_WithAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var room = CreateVirtualRoom($"TestRoom{Counter}");

            FilesControllerHelper.DeleteFolder(room.Item1.Id, false, true);

            while (true)
            {
                var statuses = FileStorageService.GetTasksStatuses();

                if (statuses.TrueForAll(r => r.Finished))
                    break;

                Task.Delay(100);
            }

            Assert.IsTrue(FileStorageService.GetTasksStatuses().TrueForAll(r => string.IsNullOrEmpty(r.Error)));
            Assert.AreEqual(ASC.Core.Users.Constants.LostGroupInfo, UserManager.GetGroupInfo(room.Item2));
        }

        [Test]
        [Description("The room administrator cannot delete the virtual room")]
        public void DeleteRoom_WithRoomAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var room = CreateVirtualRoom($"TestRoom{Counter}");
            SetRoomAdmin(room.Item1.Id, UserId1);

            SecurityContext.AuthenticateMe(UserId1);

            FilesControllerHelper.DeleteFolder(room.Item1.Id, false, true);

            while (true)
            {
                var statuses = FileStorageService.GetTasksStatuses();

                if (statuses.TrueForAll(r => r.Finished))
                    break;

                Task.Delay(100);
            }

            Assert.IsTrue(FileStorageService.GetTasksStatuses().TrueForAll(r => !string.IsNullOrEmpty(r.Error)));
            Assert.AreEqual(ASC.Core.Users.Constants.LinkedGroupCategoryId, UserManager.GetGroupInfo(room.Item2).CategoryID);
        }

        [Test]
        [Description("Portal owner and administrator can archive virtual room")]
        public void ArchiveRoom_WithAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var room = CreateVirtualRoom($"TestRoom{Counter}");

            FilesControllerHelper.DeleteFolder(room.Item1.Id, false, false);

            while (true)
            {
                var statuses = FileStorageService.GetTasksStatuses();

                if (statuses.TrueForAll(r => r.Finished))
                    break;

                Task.Delay(100);
            }

            var folder = FilesControllerHelper.GetFolderInfo(room.Item1.Id);

            Assert.IsTrue(FileStorageService.GetTasksStatuses().TrueForAll(r => string.IsNullOrEmpty(r.Error)));
            Assert.AreEqual(GlobalFolderHelper.FolderArchive, folder.ParentId);
            Assert.AreEqual(Core.FolderType.Archive, folder.RootFolderType);
            Assert.AreEqual(ASC.Core.Users.Constants.ArchivedLinkedGroupCategoryId, UserManager.GetGroupInfo(room.Item2).CategoryID);
        }

        [Test]
        [Description("Room administrator cannot archive virtual room")]
        public void ArchiveRoom_WithRoomAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var roomPair = CreateVirtualRoom($"TestRoom{Counter}");
            SetRoomAdmin(roomPair.Item1.Id, UserId1);
            SecurityContext.AuthenticateMe(UserId1);

            FilesControllerHelper.DeleteFolder(roomPair.Item1.Id, false, false);

            while (true)
            {
                var statuses = FileStorageService.GetTasksStatuses();

                if (statuses.TrueForAll(r => r.Finished))
                    break;

                Task.Delay(100);
            }

            var room = FilesControllerHelper.GetFolderInfo(roomPair.Item1.Id);

            Assert.AreNotEqual(Core.FolderType.Archive, room.RootFolderType);
            Assert.AreEqual(0, room.ParentId);
            Assert.AreEqual(Core.FolderType.VirtualRoom, room.RootFolderType);
            Assert.AreEqual(ASC.Core.Users.Constants.LinkedGroupCategoryId, UserManager.GetGroupInfo(roomPair.Item2).CategoryID);
        }

        [Test]
        [Description("Portal owner and administrator can unarchive the virtual room")]
        public void UnarchiveRoom_WithAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var roomPair = CreateVirtualRoom($"TestRoom{Counter}");

            FilesControllerHelper.DeleteFolder(roomPair.Item1.Id, false, false);

            while (true)
            {
                var statuses = FileStorageService.GetTasksStatuses();

                if (statuses.TrueForAll(r => r.Finished))
                    break;

                Task.Delay(100);
            }

            var json = " [ { \"folderIds\": [" + roomPair.Item1.Id + "] }, { \"fileIds\": [ ] }, { \"destFolderId\": 0 } ]";

            FilesControllerHelper.MoveBatchItems(GetBatchModel(json));

            while (true)
            {
                var statuses = FileStorageService.GetTasksStatuses();

                if (statuses.TrueForAll(r => r.Finished))
                    break;

                Task.Delay(100);
            }

            var room = FilesControllerHelper.GetFolderInfo(roomPair.Item1.Id);

            Assert.AreEqual(0, room.ParentId);
            Assert.AreEqual(Core.FolderType.VirtualRoom, room.RootFolderType);
            Assert.AreEqual(ASC.Core.Users.Constants.LinkedGroupCategoryId, UserManager.GetGroupInfo(roomPair.Item2).CategoryID);
        }
    }
}
