using System.ComponentModel.DataAnnotations;

namespace Vm.Models
{
    public class Team
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        [Required]
        public string TeamLeadId { get; set; }
        public ApplicationUser? TeamLead { get; set; }

        // Може да бъде и колекция, така че не е нужно да е задължително
        public ICollection<ApplicationUser> Developers { get; set; } = new List<ApplicationUser>(); // Инициализация по подразбиране

        // Можете да добавите конструктора, ако искате да го направите задължителен
        public Team()
        {
            Developers = new List<ApplicationUser>(); // За да избегнете null стойности
        }
    }

}
