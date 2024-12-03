using System.ComponentModel.DataAnnotations;

namespace Student_Management.Models
{
    public class EditRoleViewModel
    {
        [Key]
        [Required]
        public string Id { get; set; }
        [Required(ErrorMessage = "Role Name is Required")]
        public string RoleName { get; set; }
        public string? Description { get; set; }
        public List<string>? Users { get; set; }
    }
}
