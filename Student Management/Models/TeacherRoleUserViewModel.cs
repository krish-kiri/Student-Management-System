namespace Student_Management.Models
{
    public class TeacherRoleUserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }
        public string Email { get; set; }
        public DateOnly DateofBirth { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public int PhoneNumber { get; set; }
    }
}
