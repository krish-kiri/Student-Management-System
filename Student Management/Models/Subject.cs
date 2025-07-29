namespace Student_Management.Models
{
    public class Subject
    {
        public Guid Id { get; set; }
        public int SubjectNumber { get; set; }
        public string SubjectName { get; set; }
        
      
        public ICollection<UserSubject> UserSubjects { get; set; }
    }
}
