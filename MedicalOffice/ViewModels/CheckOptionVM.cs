namespace MedicalOffice.ViewModels
{
    public class CheckOptionVM
    {
        //Used for a checkbox
        public int ID { get; set; } //Which conditon the checkbox represents
        public string DisplayText { get; set; } = ""; 
        public bool Assigned { get; set; }
    }
}
