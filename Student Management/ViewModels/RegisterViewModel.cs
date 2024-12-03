using System.ComponentModel.DataAnnotations;

namespace Student_Management.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage ="FirstName is required.")]
       
        public string FirstName { get; set; }

        [Required(ErrorMessage = "LastName is required.")]
		public string LastName { get; set; }

        public string Gender { get; set; }


        [Display(Name = "Date of Birth")]
        public DateOnly DateofBirth { get; set; }


       

        [Display(Name = "Phone Number")]
        public int PhoneNumber { get; set; }

        public string Address { get; set; }


        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }

       
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(40, MinimumLength = 8, ErrorMessage ="The {0} must be at {2} and at max {1} characters long.")]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword" , ErrorMessage =" Password does not match.")]
        public string Password { get; set; }

      
        [Required(ErrorMessage = "ConfirmPaaword is required.")]
        [DataType(DataType.Password)]
        [Display(Name= "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}
