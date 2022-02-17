namespace ASC.People.Mapping.TypeConverters;

[Scope]
public class GroupTypeConverter : ITypeConverter<GroupInfo, GroupFullDto>, ITypeConverter<GroupInfo, GroupSimpleDto>
{
    private readonly UserManager _userManager;
    private readonly EmployeeWraperHelper _employeeWraperHelper;

    public GroupTypeConverter(UserManager userManager, EmployeeWraperHelper employeeWraperHelper)
    {
        _userManager = userManager;
        _employeeWraperHelper = employeeWraperHelper;
    }

    public GroupFullDto Convert(GroupInfo source, GroupFullDto destination, ResolutionContext context)
    {
        var result = new GroupFullDto
        {
            Id = source.ID,
            Category = source.CategoryID,
            Parent = source.Parent != null ? source.Parent.ID : Guid.Empty,
            Name = source.Name,
            Manager = _employeeWraperHelper.Get(_userManager.GetUsers(_userManager.GetDepartmentManager(source.ID))),
            Members = new List<EmployeeDto>(_userManager.GetUsersByGroup(source.ID).Select(_employeeWraperHelper.Get))
        };

        return result;
    }

    public GroupSimpleDto Convert(GroupInfo source, GroupSimpleDto destination, ResolutionContext context)
    {
        var result = new GroupSimpleDto
        {
            Id = source.ID,
            Category = source.CategoryID,
            Parent = source.Parent != null ? source.Parent.ID : Guid.Empty,
            Name = source.Name,
            Manager = _employeeWraperHelper.Get(_userManager.GetUsers(_userManager.GetDepartmentManager(source.ID))),
        };

        return result;
    }
}
