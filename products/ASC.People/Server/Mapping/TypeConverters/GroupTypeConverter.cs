namespace ASC.People.Mapping.TypeConverters;

[Scope]
public class GroupTypeConverter : ITypeConverter<GroupInfo, GroupFullDto>, ITypeConverter<GroupInfo, GroupDto>
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

    public GroupDto Convert(GroupInfo source, GroupDto destination, ResolutionContext context)
    {
        var result = new GroupDto
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
