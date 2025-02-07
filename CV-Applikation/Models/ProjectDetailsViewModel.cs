namespace CV_Applikation.Models
{
    public class ProjectDetailsViewModel
    {
        public int ProjectId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ParticipantViewModel> Participants { get; set; }
        public bool IsUserInProject { get; set; }
    }
}
