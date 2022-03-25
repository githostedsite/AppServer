﻿// (c) Copyright Ascensio System SIA 2010-2022
//
// This program is a free software product.
// You can redistribute it and/or modify it under the terms
// of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
// Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
// to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of
// any third-party rights.
//
// This program is distributed WITHOUT ANY WARRANTY, without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see
// the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
//
// You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
//
// The  interactive user interfaces in modified source and object code versions of the Program must
// display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
//
// Pursuant to Section 7(b) of the License you must retain the original Product logo when
// distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under
// trademark law for use of our trademarks.
//
// All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
// content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
// International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode

namespace ASC.Files.Api;

public class FoldersController : ApiControllerBase
{
    private readonly GlobalFolderHelper _globalFolderHelper;
    private readonly TenantManager _tenantManager;
    private readonly FoldersControllerHelper<int> _foldersControllerHelperInt;
    private readonly FoldersControllerHelper<string> _foldersControllerHelperString;

    public FoldersController(
        GlobalFolderHelper globalFolderHelper,
        TenantManager tenantManager,
        FoldersControllerHelper<int> foldersControllerHelperInt,
        FoldersControllerHelper<string> foldersControllerHelperString)
    {
        _globalFolderHelper = globalFolderHelper;
        _tenantManager = tenantManager;
        _foldersControllerHelperInt = foldersControllerHelperInt;
        _foldersControllerHelperString = foldersControllerHelperString;
    }

    /// <summary>
    /// Creates a new folder with the title sent in the request. The ID of a parent folder can be also specified.
    /// </summary>
    /// <short>
    /// New folder
    /// </short>
    /// <category>Folders</category>
    /// <param name="folderId">Parent folder ID</param>
    /// <param name="title">Title of new folder</param>
    /// <returns>New folder contents</returns>
    [Create("folder/{folderId}", order: int.MaxValue, DisableFormat = true)]
    public Task<FolderDto<string>> CreateFolderFromBodyAsync(string folderId, [FromBody] CreateFolderRequestDto inDto)
    {
        return _foldersControllerHelperString.CreateFolderAsync(folderId, inDto.Title);
    }

    [Create("folder/{folderId:int}", order: int.MaxValue - 1, DisableFormat = true)]
    public Task<FolderDto<int>> CreateFolderFromBodyAsync(int folderId, [FromBody] CreateFolderRequestDto inDto)
    {
        return _foldersControllerHelperInt.CreateFolderAsync(folderId, inDto.Title);
    }

    [Create("folder/{folderId}", order: int.MaxValue, DisableFormat = true)]
    [Consumes("application/x-www-form-urlencoded")]
    public Task<FolderDto<string>> CreateFolderFromFormAsync(string folderId, [FromForm] CreateFolderRequestDto inDto)
    {
        return _foldersControllerHelperString.CreateFolderAsync(folderId, inDto.Title);
    }

    [Create("folder/{folderId:int}", order: int.MaxValue - 1, DisableFormat = true)]
    [Consumes("application/x-www-form-urlencoded")]
    public Task<FolderDto<int>> CreateFolderFromFormAsync(int folderId, [FromForm] CreateFolderRequestDto inDto)
    {
        return _foldersControllerHelperInt.CreateFolderAsync(folderId, inDto.Title);
    }

    /// <summary>
    /// Deletes the folder with the ID specified in the request
    /// </summary>
    /// <short>Delete folder</short>
    /// <category>Folders</category>
    /// <param name="folderId">Folder ID</param>
    /// <param name="deleteAfter">Delete after finished</param>
    /// <param name="immediately">Don't move to the Recycle Bin</param>
    /// <returns>Operation result</returns>
    [Delete("folder/{folderId}", order: int.MaxValue - 1, DisableFormat = true)]
    public Task<IEnumerable<FileOperationDto>> DeleteFolder(string folderId, bool deleteAfter, bool immediately)
    {
        return _foldersControllerHelperString.DeleteFolder(folderId, deleteAfter, immediately);
    }

    [Delete("folder/{folderId:int}")]
    public Task<IEnumerable<FileOperationDto>> DeleteFolder(int folderId, bool deleteAfter, bool immediately)
    {
        return _foldersControllerHelperInt.DeleteFolder(folderId, deleteAfter, immediately);
    }

    /// <summary>
    /// Returns the detailed list of files and folders located in the 'Common Documents' section
    /// </summary>
    /// <short>
    /// Common folder
    /// </short>
    /// <category>Folders</category>
    /// <returns>Common folder contents</returns>
    [Read("@common")]
    public async Task<FolderContentDto<int>> GetCommonFolderAsync(Guid userIdOrGroupId, FilterType filterType, bool withsubfolders)
    {
        return await _foldersControllerHelperInt.GetFolderAsync(await _globalFolderHelper.FolderCommonAsync, userIdOrGroupId, filterType, withsubfolders);
    }

    /// <summary>
    /// Returns the detailed list of favorites files
    /// </summary>
    /// <short>Section Favorite</short>
    /// <category>Folders</category>
    /// <returns>Favorites contents</returns>
    [Read("@favorites")]
    public async Task<FolderContentDto<int>> GetFavoritesFolderAsync(Guid userIdOrGroupId, FilterType filterType, bool withsubfolders)
    {
        return await _foldersControllerHelperInt.GetFolderAsync(await _globalFolderHelper.FolderFavoritesAsync, userIdOrGroupId, filterType, withsubfolders);
    }

    /// <summary>
    /// Returns the detailed list of files and folders located in the folder with the ID specified in the request
    /// </summary>
    /// <short>
    /// Folder by ID
    /// </short>
    /// <category>Folders</category>
    /// <param name="folderId">Folder ID</param>
    /// <param name="userIdOrGroupId" optional="true">User or group ID</param>
    /// <param name="filterType" optional="true" remark="Allowed values: None (0), FilesOnly (1), FoldersOnly (2), DocumentsOnly (3), PresentationsOnly (4), SpreadsheetsOnly (5) or ImagesOnly (7)">Filter type</param>
    /// <returns>Folder contents</returns>
    [Read("{folderId}", order: int.MaxValue, DisableFormat = true)]
    public async Task<FolderContentDto<string>> GetFolderAsync(string folderId, Guid userIdOrGroupId, FilterType filterType, bool withsubfolders)
    {
        var folder = await _foldersControllerHelperString.GetFolderAsync(folderId, userIdOrGroupId, filterType, withsubfolders);

        return folder.NotFoundIfNull();
    }

    [Read("{folderId:int}", order: int.MaxValue - 1, DisableFormat = true)]
    public Task<FolderContentDto<int>> GetFolderAsync(int folderId, Guid userIdOrGroupId, FilterType filterType, bool withsubfolders)
    {
        return _foldersControllerHelperInt.GetFolderAsync(folderId, userIdOrGroupId, filterType, withsubfolders);
    }

    /// <summary>
    /// Returns a detailed information about the folder with the ID specified in the request
    /// </summary>
    /// <short>Folder information</short>
    /// <category>Folders</category>
    /// <returns>Folder info</returns>
    [Read("folder/{folderId}", order: int.MaxValue, DisableFormat = true)]
    public Task<FolderDto<string>> GetFolderInfoAsync(string folderId)
    {
        return _foldersControllerHelperString.GetFolderInfoAsync(folderId);
    }

    [Read("folder/{folderId:int}", order: int.MaxValue - 1, DisableFormat = true)]
    public Task<FolderDto<int>> GetFolderInfoAsync(int folderId)
    {
        return _foldersControllerHelperInt.GetFolderInfoAsync(folderId);
    }

    /// <summary>
    /// Returns parent folders
    /// </summary>
    /// <param name="folderId"></param>
    /// <category>Folders</category>
    /// <returns>Parent folders</returns>
    [Read("folder/{folderId}/path")]
    public IAsyncEnumerable<FileEntryDto> GetFolderPathAsync(string folderId)
    {
        return _foldersControllerHelperString.GetFolderPathAsync(folderId);
    }

    [Read("folder/{folderId:int}/path")]
    public IAsyncEnumerable<FileEntryDto> GetFolderPathAsync(int folderId)
    {
        return _foldersControllerHelperInt.GetFolderPathAsync(folderId);
    }

    [Read("{folderId}/subfolders")]
    public IAsyncEnumerable<FileEntryDto> GetFoldersAsync(string folderId)
    {
        return _foldersControllerHelperString.GetFoldersAsync(folderId);
    }

    [Read("{folderId:int}/subfolders")]
    public IAsyncEnumerable<FileEntryDto> GetFoldersAsync(int folderId)
    {
        return _foldersControllerHelperInt.GetFoldersAsync(folderId);
    }

    /// <summary>
    /// Returns the detailed list of files and folders located in the current user 'My Documents' section
    /// </summary>
    /// <short>
    /// My folder
    /// </short>
    /// <category>Folders</category>
    /// <returns>My folder contents</returns>
    [Read("@my")]
    public Task<FolderContentDto<int>> GetMyFolderAsync(Guid userIdOrGroupId, FilterType filterType, bool withsubfolders)
    {
        return _foldersControllerHelperInt.GetFolderAsync(_globalFolderHelper.FolderMy, userIdOrGroupId, filterType, withsubfolders);
    }

    [Read("{folderId}/news")]
    public Task<List<FileEntryDto>> GetNewItemsAsync(string folderId)
    {
        return _foldersControllerHelperString.GetNewItemsAsync(folderId);
    }

    [Read("{folderId:int}/news")]
    public Task<List<FileEntryDto>> GetNewItemsAsync(int folderId)
    {
        return _foldersControllerHelperInt.GetNewItemsAsync(folderId);
    }

    [Read("@privacy")]
    public Task<FolderContentDto<int>> GetPrivacyFolderAsync(Guid userIdOrGroupId, FilterType filterType, bool withsubfolders)
    {
        if (PrivacyRoomSettings.IsAvailable(_tenantManager))
        {
            throw new System.Security.SecurityException();
        }

        return InternalGetPrivacyFolderAsync(userIdOrGroupId, filterType, withsubfolders);
    }

    /// <summary>
    /// Returns the detailed list of files and folders located in the current user 'Projects Documents' section
    /// </summary>
    /// <short>
    /// Projects folder
    /// </short>
    /// <category>Folders</category>
    /// <returns>Projects folder contents</returns>
    [Read("@projects")]
    public async Task<FolderContentDto<string>> GetProjectsFolderAsync(Guid userIdOrGroupId, FilterType filterType, bool withsubfolders)
    {
        return await _foldersControllerHelperString.GetFolderAsync(await _globalFolderHelper.GetFolderProjectsAsync<string>(), userIdOrGroupId, filterType, withsubfolders);
    }

    /// <summary>
    /// Returns the detailed list of recent files
    /// </summary>
    /// <short>Section Recent</short>
    /// <category>Folders</category>
    /// <returns>Recent contents</returns>
    [Read("@recent")]
    public async Task<FolderContentDto<int>> GetRecentFolderAsync(Guid userIdOrGroupId, FilterType filterType, bool withsubfolders)
    {
        return await _foldersControllerHelperInt.GetFolderAsync(await _globalFolderHelper.FolderRecentAsync, userIdOrGroupId, filterType, withsubfolders);
    }

    [Read("@root")]
    public async Task<IEnumerable<FolderContentDto<int>>> GetRootFoldersAsync(Guid userIdOrGroupId, FilterType filterType, bool withsubfolders, bool withoutTrash, bool withoutAdditionalFolder)
    {
        var foldersIds = await _foldersControllerHelperInt.GetRootFoldersIdsAsync(withoutTrash, withoutAdditionalFolder);

        var result = new List<FolderContentDto<int>>();

        foreach (var folder in foldersIds)
        {
            result.Add(await _foldersControllerHelperInt.GetFolderAsync(folder, userIdOrGroupId, filterType, withsubfolders));
        }

        return result;
    }
    /// <summary>
    /// Returns the detailed list of files and folders located in the 'Shared with Me' section
    /// </summary>
    /// <short>
    /// Shared folder
    /// </short>
    /// <category>Folders</category>
    /// <returns>Shared folder contents</returns>
    [Read("@share")]
    public async Task<FolderContentDto<int>> GetShareFolderAsync(Guid userIdOrGroupId, FilterType filterType, bool withsubfolders)
    {
        return await _foldersControllerHelperInt.GetFolderAsync(await _globalFolderHelper.FolderShareAsync, userIdOrGroupId, filterType, withsubfolders);
    }

    /// <summary>
    /// Returns the detailed list of templates files
    /// </summary>
    /// <short>Section Template</short>
    /// <category>Folders</category>
    /// <returns>Templates contents</returns>
    [Read("@templates")]
    public async Task<FolderContentDto<int>> GetTemplatesFolderAsync(Guid userIdOrGroupId, FilterType filterType, bool withsubfolders)
    {
        return await _foldersControllerHelperInt.GetFolderAsync(await _globalFolderHelper.FolderTemplatesAsync, userIdOrGroupId, filterType, withsubfolders);
    }

    /// <summary>
    /// Returns the detailed list of files and folders located in the 'Recycle Bin' section
    /// </summary>
    /// <short>
    /// Trash folder
    /// </short>
    /// <category>Folders</category>
    /// <returns>Trash folder contents</returns>
    [Read("@trash")]
    public Task<FolderContentDto<int>> GetTrashFolderAsync(Guid userIdOrGroupId, FilterType filterType, bool withsubfolders)
    {
        return _foldersControllerHelperInt.GetFolderAsync(Convert.ToInt32(_globalFolderHelper.FolderTrash), userIdOrGroupId, filterType, withsubfolders);
    }

    /// <summary>
    /// Renames the selected folder to the new title specified in the request
    /// </summary>
    /// <short>
    /// Rename folder
    /// </short>
    /// <category>Folders</category>
    /// <param name="folderId">Folder ID</param>
    /// <param name="title">New title</param>
    /// <returns>Folder contents</returns>
    [Update("folder/{folderId}", order: int.MaxValue, DisableFormat = true)]
    public Task<FolderDto<string>> RenameFolderFromBodyAsync(string folderId, [FromBody] CreateFolderRequestDto inDto)
    {
        return _foldersControllerHelperString.RenameFolderAsync(folderId, inDto.Title);
    }

    [Update("folder/{folderId:int}", order: int.MaxValue - 1, DisableFormat = true)]
    public Task<FolderDto<int>> RenameFolderFromBodyAsync(int folderId, [FromBody] CreateFolderRequestDto inDto)
    {
        return _foldersControllerHelperInt.RenameFolderAsync(folderId, inDto.Title);
    }

    [Update("folder/{folderId}", order: int.MaxValue, DisableFormat = true)]
    [Consumes("application/x-www-form-urlencoded")]
    public Task<FolderDto<string>> RenameFolderFromFormAsync(string folderId, [FromForm] CreateFolderRequestDto inDto)
    {
        return _foldersControllerHelperString.RenameFolderAsync(folderId, inDto.Title);
    }

    [Update("folder/{folderId:int}", order: int.MaxValue - 1, DisableFormat = true)]
    [Consumes("application/x-www-form-urlencoded")]
    public Task<FolderDto<int>> RenameFolderFromFormAsync(int folderId, [FromForm] CreateFolderRequestDto inDto)
    {
        return _foldersControllerHelperInt.RenameFolderAsync(folderId, inDto.Title);
    }

    private async Task<FolderContentDto<int>> InternalGetPrivacyFolderAsync(Guid userIdOrGroupId, FilterType filterType, bool withsubfolders)
    {
        return await _foldersControllerHelperInt.GetFolderAsync(await _globalFolderHelper.FolderPrivacyAsync, userIdOrGroupId, filterType, withsubfolders);
    }
}