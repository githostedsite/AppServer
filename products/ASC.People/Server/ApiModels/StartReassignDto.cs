namespace ASC.People.Models
{
    public class StartReassignDto
    {
        public Guid FromUserId { get; set; }
        public Guid ToUserId { get; set; }
        public bool DeleteProfile { get; set; }
    }
}
