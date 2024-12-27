namespace CV_Applikation.Models
{
    public class HomeViewModel
    {
        public Project? ProjectLatest { get; set; }
        public List<CV> CVs { get; set; } = new List<CV>();
    }
}
