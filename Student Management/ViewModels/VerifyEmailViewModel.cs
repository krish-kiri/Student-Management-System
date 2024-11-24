using System.ComponentModel.DataAnnotations;

namespace Student_Management.ViewModels
{
    public class VerifyEmailViewModel
    {

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }

    }
}
