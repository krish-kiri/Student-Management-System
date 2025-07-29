using System.ComponentModel.DataAnnotations;

namespace Student_Management.Models
{
    public class EditUserViewModel
    {
        
        public EditUserViewModel()
        {
            Claims = new List<string>();
            Roles = new List<string>();
            Subject = new List<string>();
        }
        [Required]
        public string Id { get; set; }
        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Grade { get; set; }

        [Display(Name = "Date of Birth")]
        public DateOnly DateofBirth { get; set; }
        public int PhoneNumber { get; set; }
        public string Address { get; set; }
        public List<string> Claims { get; set; }
        public IList<string> Roles { get; set; }
        public IList<string> Subject { get; set; }
    }
}
