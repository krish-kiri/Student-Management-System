namespace Student_Management.Models
{
    public class UserProfileViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }
        public string Gender { get; set; }

        public string Grade { get; set; }
        public int? PhoneNumber { get; set; }
        public string? Address { get; set; }

        public List<string> Claims { get; set; }

        public IList<string> Roles { get; set; }
        public IList<string> Subject { get; set; }
        public DateOnly? DateofBirth { get; set; }
        public string RoleId { get; set; }
        public string Role { get; set; }
        public string Class { get; set; }
        public string Division { get; set; }
    }
}
