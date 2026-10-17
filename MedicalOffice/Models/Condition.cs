using System.ComponentModel.DataAnnotations;

namespace MedicalOffice.Models
{
    public class Condition
    {
        public int ID { get; set; }

        [Display(Name = "Medical Condition")]
        [Required(ErrorMessage = "You cannot leave the name of the condition blank.")]
        [StringLength(50, ErrorMessage = "Condition name cannot be more than 50 characters long.")]
        public string ConditionName { get; set; } = "";

        [Display(Name = "History")]
        public ICollection<PatientCondition> PatientConditions { get; set; } = new HashSet<PatientCondition>();
        // ICollection<PatientCondition> allows a Condition object to hold multiple related PatientCondition objects in C#.
        // Each row in the Condition table becomes a Condition object when loaded.
        // Each Condition object has its own collection of PatientCondition objects.
        // For example, the Asthma object holds all PatientCondition records
        // that connect patients to Asthma.
        // Essentially, one object contains a collection of other objects.

    }
}
