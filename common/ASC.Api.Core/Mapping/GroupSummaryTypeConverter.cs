using GroupInfo = ASC.Core.Users.GroupInfo;

namespace ASC.Api.Core.Mapping;

[Scope]
public class GroupSummaryTypeConverter : ITypeConverter<GroupInfo, GroupSummaryDto>
{
    private readonly UserManager _userManager;

    public GroupSummaryTypeConverter(UserManager userManager)
    {
        _userManager = userManager;
    }

    public GroupSummaryDto Convert(GroupInfo source, GroupSummaryDto destination, ResolutionContext context)
    {
        var result = new GroupSummaryDto();

        result.Id = source.ID;
        result.Name = source.Name;
        result.Manager = _userManager.GetUsers(_userManager.GetDepartmentManager(source.ID)).UserName;

        return result;
    }
}