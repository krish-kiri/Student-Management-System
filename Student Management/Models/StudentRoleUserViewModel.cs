namespace Student_Management.Models
{
    public class StudentRoleUserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }
        public string Email { get; set; }
        public DateOnly DateofBirth { get; set; }
        public string Gender { get; set; }

        public string Grade { get; set; }
        public string Address { get; set; }
        public int PhoneNumber { get; set; }

        public string Class { get; set; }
        public string Division { get; set; }

       
        public List<Subject> AllSubjects { get; set; }
        public List<Guid> SelectedSubjectIds { get; set; }
        public List<Subject> AssignedSubjects { get; set; }
        public List<string> Subject { get; set; }
   
    }

}



