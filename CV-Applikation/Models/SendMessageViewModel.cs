namespace CV_Applikation.Models
{
    public class SendMessageViewModel
    {
        public string ProfileName { get; set; }
        public List<Message> Messages { get; set; } = new List<Message>();
    }
}
