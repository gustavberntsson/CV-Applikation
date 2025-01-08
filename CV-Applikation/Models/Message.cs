using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CV_Applikation.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "AvsändarID är obligatoriskt.")]
		public string SenderId { get; set; }

        [Required(ErrorMessage = "MottagarID är obligatoriskt.")]
        public string ReceiverId { get; set; }

        [Required(ErrorMessage = "Meddelandeinnehåll är obligatoriskt.")]
        [StringLength(1000, ErrorMessage = "Meddelandeinnehållet får inte överskrida 1000 tecken.")]
        public string Content { get; set; }

        [Required]
        public Boolean IsRead { get; set; }

        [Required]
		public System.DateTime Date { get; set; }

        [ForeignKey(nameof(ReceiverId))]
        public virtual User Receiver { get; set; }

    }
}