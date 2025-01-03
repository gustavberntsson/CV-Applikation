namespace CV_Applikation.Models
{
	public class MessageViewModel
	{
		public bool IsRead { get; set; }
        public List<Message> Messages { get; set; } = new List<Message>();
		public List<int> SelectedMessages { get; set; }
    }
}
