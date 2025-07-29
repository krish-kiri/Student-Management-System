namespace Student_Management.Models
{
    public class AssignSubjectViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Grade { get; set; }
        public DateOnly DateofBirth { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public int PhoneNumber { get; set; }
        public bool IsActive { get; set; }


        public List<SubjectViewModel> Subjects { get; set; } = new List<SubjectViewModel>();
        public List<Guid> SelectedSubjectIds { get; set; } = new List<Guid>();
        public string Class { get; set; }
        public string Division { get; set; }
    }


    public class SubjectViewModel
    {
        public Guid SubjectId { get; set; }
        public int SujectNumber { get; set; }
        public string SubjectName { get; set; }
       public string SubjectType { get; set; }
        public bool IsSelected { get; set; }
         public int? Marks { get; set; }


    }
}
