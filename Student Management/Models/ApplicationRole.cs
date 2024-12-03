using Microsoft.AspNetCore.Identity;

namespace Student_Management.Models
{
    public class ApplicationRole : IdentityRole
    {
        // Add custom properties here
        public string? Description { get; set; }
    }
}
