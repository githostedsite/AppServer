namespace ASC.People.ApiModels.ResponseDto;

public class GroupSimpleDto
{
    public EmployeeDto Manager { get; set; }
    public Guid Category { get; set; }
    public Guid Id { get; set; }
    public Guid? Parent { get; set; }
    public string Description { get; set; }
    public string Name { get; set; }
}
