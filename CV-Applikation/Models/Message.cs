namespace CV_Applikation.Models
{
    public class Message
    {
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Content { get; set; }
        public System.DateTime Date { get; set; }
    }
}