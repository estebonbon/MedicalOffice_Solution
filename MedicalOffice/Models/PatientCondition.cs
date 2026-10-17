namespace MedicalOffice.Models
{
    public class PatientCondition
    {

        public int ConditionID { get; set; }

        public Condition? Condition { get; set; } // Stores the Foreign Key (FK) integer value.
        public int PatientID { get; set; }

        public Patient? Patient { get; set; }    // Navigation property: provides a pathway to access the related Patient object.

    }
}
