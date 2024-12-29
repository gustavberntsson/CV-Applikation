namespace CV_Applikation.Models
{
    public class ProjectViewModel
    {
        public int ProjectId { get; set; }
        public string Title { get; set; }
        public int ParticipantCount { get; set; }
        public List<ParticipantViewModel> Participants { get; set; } = new List<ParticipantViewModel>();
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
