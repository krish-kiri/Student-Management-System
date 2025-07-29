using Microsoft.AspNetCore.Identity;

namespace Student_Management.Models
{
    public class ApplicationRole : IdentityRole
    {
       
        public string? Description { get; set; }
    }
}
