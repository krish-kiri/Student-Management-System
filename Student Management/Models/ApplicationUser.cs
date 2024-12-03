using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Student_Management.Models
{
    public class ApplicationUser : IdentityUser
    {
        

		public string LastName { get; set; }

		public string FirstName { get; set; }

		public string Gender { get; set; }


		[Display(Name = "Date of Birth")]
		public DateOnly DateofBirth { get; set; }




		[Display(Name = "Phone Number")]
        public int PhoneNumber { get; set; }

		public string Address { get; set; }



	}
}
