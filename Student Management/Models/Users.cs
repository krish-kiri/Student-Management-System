using Microsoft.AspNetCore.Identity;

namespace Student_Management.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; }
    }
}
