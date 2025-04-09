using System.ComponentModel.DataAnnotations;

namespace Vm.Models
{
    public class Role
    {
        public Role()
        {
            Users = new HashSet<ApplicationUser>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public ICollection<ApplicationUser> Users { get; set; }
    }
}
