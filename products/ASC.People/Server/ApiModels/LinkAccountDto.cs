namespace ASC.People.Models
{
    public class LinkAccountDto
    {
        public string SerializedProfile { get; set; }
    }

    public class SignupAccountModel: LinkAccountDto
    {
        public EmployeeType? EmplType { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
}
