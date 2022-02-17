namespace ASC.People.ApiModels
{
    public class GroupDto
    {
        public Guid GroupManager { get; set; }
        public string GroupName { get; set; }
        public IEnumerable<Guid> Members { get; set; }
    }
}
