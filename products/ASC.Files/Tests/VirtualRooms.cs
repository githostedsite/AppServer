using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using ASC.Api.Documents;
using ASC.Files.Core;
using ASC.Web.Api.Models;

using NUnit.Framework;

namespace ASC.Files.Tests
{
    [TestFixture]
    public class VirtualRooms : RoomTestsBase
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
                .SingleOrDefault(s => s.SubjectGroup)?.SubjectName;

            Assert.IsNotNull(roomFolder);
            Assert.IsTrue(roomFolder.RootFolderType == FolderType.RoomsStorage);
            Assert.AreEqual(GlobalFolderHelper.FolderVirtualRooms, roomFolder.ParentId);
            Assert.AreEqual(roomFolder.Title, groupName);
        }

        [Test]
        [Description("Regular user cannot create virtual rooms")]
        public void CreateVirtualRoom_WithoutAdminPrivileges_ThrowException()
        {
            SecurityContext.AuthenticateMe(UserId1);

            Assert.Throws<InvalidOperationException>(() =>
                FilesControllerHelper.CreateVirtualRoom("TestVirtualRoom", false));
        }

        [Test]
        [Description("Portal owner or administrator can rename virtual rooms")]
        public void RenameVirtualRoom_WithAdminPrivileges_ReturnFolderWrapper()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);

            var newTitle = "RenamedVirtualRoom";
            var roomFolder = FilesControllerHelper.CreateVirtualRoom("TestVirtualRoom", false);

            var renamedRoomFolder = FilesControllerHelper.RenameFolder(roomFolder.Id, newTitle);
            var renamedGroupName = FileStorageService.GetSharedInfo(new List<int>(), new List<int> { renamedRoomFolder.Id })
                .SingleOrDefault(s => s.SubjectGroup)?.SubjectName;

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
                FilesControllerHelper.RenameFolder(room.Id, "RenamedVirtualRoom"));
        }

        [Test]
        [Description("Portal owner or administrator can add users to virtual room")]
        public void AddUserIntoRoom_WithAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var room = CreateVirtualRoom($"TestRoom{Counter}");
            var group = UserManager.GetGroupInfo(room.Item2);

            VirtualRoomsMembersManager.AddMembers(group, new[] { UserId1 });

            Assert.IsTrue(UserManager.IsUserInGroup(UserId1, group.ID));
        }

        [Test]
        [Description("Portal owner or administrator can remove users from virtual room")]
        public void RemoveUserFromRoom_WithAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var room = CreateVirtualRoom($"TestRoom{Counter}");
            var group = UserManager.GetGroupInfo(room.Item2);

            VirtualRoomsMembersManager.AddMembers(group, new[] { UserId1 });

            VirtualRoomsMembersManager.RemoveMembers(group, new[] { UserId1 });

            Assert.IsTrue(!UserManager.IsUserInGroup(UserId1, group.ID));
        }

        [Test]
        [Description("Virtual room administrator can add users")]
        public void AddUserIntoRoom_WithRoomAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var (folder, groupId) = CreateVirtualRoom($"TestRoom{Counter}");
            var group = UserManager.GetGroupInfo(groupId);
            AddUserInRoomAsAdmin(folder.Id, group ,UserId1);

            SecurityContext.AuthenticateMe(UserId1);
            VirtualRoomsMembersManager.AddMembers(group, new[] { UserId2 });

            Assert.IsTrue(UserManager.IsUserInGroup(UserId2, group.ID));
        }

        [Test]
        [Description("Virtual room administrator can delete users")]
        public void RemoveUserFromRoom_WithRoomAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var (folder, groupId) = CreateVirtualRoom($"TestRoom{Counter}");
            var group = UserManager.GetGroupInfo(groupId);
            VirtualRoomsMembersManager.AddMembers(group, new List<Guid> { UserId2 });
            AddUserInRoomAsAdmin(folder.Id, group, UserId1);
            SecurityContext.AuthenticateMe(UserId1);

            VirtualRoomsMembersManager.RemoveMembers(group, new[] { UserId2 });

            Assert.IsTrue(!UserManager.IsUserInGroup(UserId2, group.ID));
        }

        [Test]
        [Description("Room administrator cannot assign a room administrator")]
        public void SetRoomSecurity_FullAccess_WithRoomAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var (folder, groupId) = CreateVirtualRoom($"TestRoom{Counter}");
            var group = UserManager.GetGroupInfo(groupId);
            AddUserInRoomAsAdmin(folder.Id, group ,UserId1);
            VirtualRoomsMembersManager.AddMembers(group, new[] { UserId2 });
            SecurityContext.AuthenticateMe(UserId1);

            var shareParam = new FileShareParams { Access = Core.Security.FileShare.RoomAdministrator, ShareTo = UserId2 };
            FilesControllerHelper.SetSecurityInfo(new List<int>(), new List<int> { folder.Id },
                new List<FileShareParams> { shareParam }, false, string.Empty);

            Assert.IsTrue(FilesControllerHelper
                .GetSecurityInfo(new List<int>(), new List<int> { folder.Id })
                .FirstOrDefault(s => s.SharedTo.Equals(UserId2) && s.Access == Core.Security.FileShare.RoomAdministrator) == null);
        }

        [Test]
        [Description("Portal owner or administrator can create files in the virtual room")]
        public void CreateFileInRoom_WithAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var room = FilesControllerHelper.CreateVirtualRoom($"TestRoom{Counter}", false);
            var templateId = JsonSerializer.SerializeToElement(0);
            
            var file = FilesControllerHelper.CreateFile(room.Id, "TestFile.docx", templateId);

            Assert.AreEqual(room.Id, file.FolderId);
            Assert.AreEqual(room.RootFolderType, FolderType.RoomsStorage);
        }

        //[Test]
        //[Description("Portal owner or administrator can create folders in the virtual room")]
        //public void CreateFolderInRoom_WithAdminPrivileges()
        //{
        //    SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
        //    var room = FilesControllerHelper.CreateVirtualRoom($"TestRoom{Counter}", false);

        //    var folder = FilesControllerHelper.CreateFolder(room.Id, "TestFolder");

        //    Assert.AreEqual(room.Id, folder.RootFolderId);
        //    Assert.AreEqual(room.RootFolderType, folder.RootFolderType);
        //}

        //[Test]
        //[Description("The room administrator can create files in the virtual room")]
        //public void CreateFileInRoom_WithAdminRoomPrivileges()
        //{
        //    SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
        //    var (folder, groupId) = CreateVirtualRoom($"TestRoom{Counter}");
        //    var group = UserManager.GetGroupInfo(groupId);
        //    AddUserInRoomAsAdmin(folder.Id, group, UserId1);
        //    SecurityContext.AuthenticateMe(UserId1);
        //    var templateId = JsonSerializer.SerializeToElement(0);
            
        //    var file = FilesControllerHelper.CreateFile(folder.Id, "TestFile.docx", templateId);

        //    Assert.AreEqual(folder.Id, file.FolderId);
        //    Assert.AreEqual(folder.Id, file.RootFolderId);
        //    Assert.AreEqual(folder.RootFolderType, file.RootFolderType);
        //}

        //[Test]
        //[Description("The room administrator can create folders in the virtual room")]
        //public void CreateFolderInRoom_WithAdminRoomPrivileges()
        //{
        //    SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
        //    var (roomFolder, groupId) = CreateVirtualRoom($"TestRoom{Counter}");
        //    var group = UserManager.GetGroupInfo(groupId);
        //    AddUserInRoomAsAdmin(roomFolder.Id, group, UserId1);

        //    SecurityContext.AuthenticateMe(UserId1);

        //    var folder = FilesControllerHelper.CreateFolder(roomFolder.Id, "TestFolder");

        //    Assert.AreEqual(roomFolder.Id, folder.RootFolderId);
        //    Assert.AreEqual(roomFolder.RootFolderType, folder.RootFolderType);
        //}

        [Test]
        [Description("The room administrator can assign rights to room members")]
        public void SetRoomSecurity_WithRoomAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var (folder, groupId) = CreateVirtualRoom($"TestRoom{Counter}");
            var group = UserManager.GetGroupInfo(groupId);

            AddUserInRoomAsAdmin(folder.Id, group, UserId1);
            VirtualRoomsMembersManager.AddMembers(group, new[] { UserId2 });

            SecurityContext.AuthenticateMe(UserId1);
            SetSecurity(folder.Id, UserId2, Core.Security.FileShare.FillForms);

            var info = FilesControllerHelper.GetSecurityInfo(new List<int>(), new List<int> { folder.Id }).ToList();

            Assert.IsTrue(FilesControllerHelper
                .GetSecurityInfo(new List<int>(), new List<int> { folder.Id })
                .FirstOrDefault(s => ((EmployeeWraperFull)s.SharedTo).Id.Equals(UserId2) && s.Access == Core.Security.FileShare.FillForms) != null);
        }

        [Test]
        [Description("Portal owner and administrator can delete virtual room")]
        public void DeleteRoom_WithAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var (folder, groupId) = CreateVirtualRoom($"TestRoom{Counter}");

            FilesControllerHelper.DeleteFolder(folder.Id, false, true);

            while (true)
            {
                var statuses = FileStorageService.GetTasksStatuses();

                if (statuses.TrueForAll(r => r.Finished))
                    break;

                Task.Delay(100);
            }

            Assert.IsTrue(FileStorageService.GetTasksStatuses().TrueForAll(r => string.IsNullOrEmpty(r.Error)));
            Assert.Throws<InvalidOperationException>(() =>
            {
                FilesControllerHelper.GetFolder(folder.Id, Guid.Empty, FilterType.None, false);
            });
        }

        [Test]
        [Description("The room administrator cannot delete the virtual room")]
        public void DeleteRoom_WithRoomAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var (folder, groupId) = CreateVirtualRoom($"TestRoom{Counter}");
            var group = UserManager.GetGroupInfo(groupId);
            AddUserInRoomAsAdmin(folder.Id, group, UserId1);

            SecurityContext.AuthenticateMe(UserId1);

            FilesControllerHelper.DeleteFolder(folder.Id, false, true);

            while (true)
            {
                var statuses = FileStorageService.GetTasksStatuses();

                if (statuses.TrueForAll(r => r.Finished))
                    break;

                Task.Delay(100);
            }

            Assert.IsTrue(FileStorageService.GetTasksStatuses().TrueForAll(r => !string.IsNullOrEmpty(r.Error)));
            Assert.AreEqual(ASC.Core.Users.Constants.LinkedGroupCategoryId, UserManager.GetGroupInfo(groupId).CategoryID);
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
        [Description("Room administrator can archive virtual room")]
        public void ArchiveRoom_WithRoomAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var (folder, groupId) = CreateVirtualRoom($"TestRoom{Counter}");
            var group = UserManager.GetGroupInfo(groupId);
            AddUserInRoomAsAdmin(folder.Id, group, UserId1);
            SecurityContext.AuthenticateMe(UserId1);

            FilesControllerHelper.ArchiveRoom(new List<JsonElement> { JsonSerializer.SerializeToElement(folder.Id) });

            while (true)
            {
                var statuses = FileStorageService.GetTasksStatuses();

                if (statuses.TrueForAll(r => r.Finished))
                    break;

                Task.Delay(100);
            }

            var roomFolder = FilesControllerHelper.GetFolderInfo(folder.Id);

            Assert.AreEqual(FolderType.Archive, roomFolder.RootFolderType);
            Assert.AreEqual(GlobalFolderHelper.FolderArchive, roomFolder.ParentId);
            Assert.AreEqual(ASC.Core.Users.Constants.ArchivedLinkedGroupCategoryId, UserManager.GetGroupInfo(groupId).CategoryID);
        }

        [Test]
        [Description("Room administartor can unarchive the virtual room")]
        public void UnarchiveRoom_WithRoomAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var (folder, groupId) = CreateVirtualRoom($"TestRoom{Counter}");
            var group = UserManager.GetGroupInfo(groupId);
            AddUserInRoomAsAdmin(folder.Id, group, UserId1);

            FilesControllerHelper.ArchiveRoom(new List<JsonElement> { JsonSerializer.SerializeToElement(folder.Id) });

            while (true)
            {
                var statuses = FileStorageService.GetTasksStatuses();

                if (statuses.TrueForAll(r => r.Finished))
                    break;

                Task.Delay(100);
            }

            var json = " [ { \"folderIds\": [" + folder.Id + "] }, { \"fileIds\": [ ] }, { \"destFolderId\": 0 } ]";

            SecurityContext.AuthenticateMe(UserId1);

            FilesControllerHelper.UnarchiveRoom(new List<JsonElement> { JsonSerializer.SerializeToElement(folder.Id) });

            while (true)
            {
                var statuses = FileStorageService.GetTasksStatuses();

                if (statuses.TrueForAll(r => r.Finished))
                    break;

                Task.Delay(100);
            }

            var room = FilesControllerHelper.GetFolderInfo(folder.Id);

            Assert.AreEqual(GlobalFolderHelper.FolderVirtualRooms, room.ParentId);
            Assert.AreEqual(FolderType.RoomsStorage, room.RootFolderType);
            Assert.AreEqual(ASC.Core.Users.Constants.LinkedGroupCategoryId, UserManager.GetGroupInfo(groupId).CategoryID);
        }

        [Test]
        [Description("Portal owner and administrator can unarchive the virtual room")]
        public void UnarchiveRoom_WithAdminPrivileges()
        {
            SecurityContext.AuthenticateMe(CurrentTenant.OwnerId);
            var (folder, groupId) = CreateVirtualRoom($"TestRoom{Counter}");

            FilesControllerHelper.ArchiveRoom(new List<JsonElement> { JsonSerializer.SerializeToElement(folder.Id) });

            while (true)
            {
                var statuses = FileStorageService.GetTasksStatuses();

                if (statuses.TrueForAll(r => r.Finished))
                    break;

                Task.Delay(100);
            }

            var json = " [ { \"folderIds\": [" + folder.Id + "] }, { \"fileIds\": [ ] }, { \"destFolderId\": 0 } ]";

            FilesControllerHelper.UnarchiveRoom(new List<JsonElement> { JsonSerializer.SerializeToElement(folder.Id) });

            while (true)
            {
                var statuses = FileStorageService.GetTasksStatuses();

                if (statuses.TrueForAll(r => r.Finished))
                    break;

                Task.Delay(100);
            }

            var room = FilesControllerHelper.GetFolderInfo(folder.Id);

            Assert.AreEqual(GlobalFolderHelper.FolderVirtualRooms, room.ParentId);
            Assert.AreEqual(FolderType.RoomsStorage, room.RootFolderType);
            Assert.AreEqual(ASC.Core.Users.Constants.LinkedGroupCategoryId, UserManager.GetGroupInfo(groupId).CategoryID);
        }
    }
}
