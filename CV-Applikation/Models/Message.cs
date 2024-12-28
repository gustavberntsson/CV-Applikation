using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CV_Applikation.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
		public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Content { get; set; }
        public Boolean IsRead { get; set; }
		public System.DateTime Date { get; set; }

        [ForeignKey(nameof(SenderId))]
        public virtual User Sender { get; set; }

        [ForeignKey(nameof(ReceiverId))]
        public virtual User Receiver { get; set; }

    }
}