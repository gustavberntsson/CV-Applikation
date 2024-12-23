using System.ComponentModel.DataAnnotations.Schema;

namespace CV_Applikation.Models
{
    public class Message
    {
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Content { get; set; }
        public System.DateTime Date { get; set; }

        [ForeignKey(nameof(Sender))]
        public virtual User? User { get; set; }
            
        [ForeignKey(nameof(Receiver))]
        public virtual ContactInformation? ContactInformation { get; set; }
    }
}