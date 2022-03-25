// (c) Copyright Ascensio System SIA 2010-2022
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

namespace ASC.Web.Api.Models;

public class EmployeeDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; }
    public string Title { get; set; }
    public string AvatarSmall { get; set; }
    public string ProfileUrl { get; set; }

    public static EmployeeDto GetSample()
    {
        return new EmployeeDto
        {
            Id = Guid.Empty,
            DisplayName = "Mike Zanyatski",
            Title = "Manager",
            AvatarSmall = "url to small avatar",
        };
    }
}

[Scope]
public class EmployeeDtoHelper
{
    protected readonly UserPhotoManager UserPhotoManager;
    protected readonly UserManager UserManager;

    private readonly ApiContext _httpContext;
    private readonly DisplayUserSettingsHelper _displayUserSettingsHelper;
    private readonly CommonLinkUtility _commonLinkUtility;

    public EmployeeDtoHelper(
        ApiContext httpContext,
        DisplayUserSettingsHelper displayUserSettingsHelper,
        UserPhotoManager userPhotoManager,
        CommonLinkUtility commonLinkUtility,
        UserManager userManager)
    {
        UserPhotoManager = userPhotoManager;
        UserManager = userManager;
        _httpContext = httpContext;
        _displayUserSettingsHelper = displayUserSettingsHelper;
        _commonLinkUtility = commonLinkUtility;
    }

    public EmployeeDto Get(UserInfo userInfo)
    {
        return Init(new EmployeeDto(), userInfo);
    }

    public EmployeeDto Get(Guid userId)
    {
        try
        {
            return Get(UserManager.GetUsers(userId));
        }
        catch (Exception)
        {
            return Get(Constants.LostUser);
        }
    }

    protected EmployeeDto Init(EmployeeDto result, UserInfo userInfo)
    {
        result.Id = userInfo.Id;
        result.DisplayName = _displayUserSettingsHelper.GetFullUserName(userInfo);

        if (!string.IsNullOrEmpty(userInfo.Title))
        {
            result.Title = userInfo.Title;
        }

        var userInfoLM = userInfo.LastModified.GetHashCode();

        if (_httpContext.Check("avatarSmall"))
        {
            result.AvatarSmall = UserPhotoManager.GetSmallPhotoURL(userInfo.Id, out var isdef) 
                + (isdef ? "" : $"?_={userInfoLM}");
        }     

        if (result.Id != Guid.Empty)
        {
            var profileUrl = _commonLinkUtility.GetUserProfile(userInfo, false);
            result.ProfileUrl = _commonLinkUtility.GetFullAbsolutePath(profileUrl);
        }

        return result;
    }
}