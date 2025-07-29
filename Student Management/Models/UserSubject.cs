namespace Student_Management.Models
{
    public class UserSubject
    {
        public int? Marks { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public Guid SubjectId { get; set; }
        public Subject Subject { get; set; }
    }
}
