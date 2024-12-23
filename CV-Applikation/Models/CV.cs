using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CV_Applikation.Models
{
    public class CV
    {
        [Key]
        public int CVId { get; set; }
        public string OwnerId { get; set; }
        public Boolean IsPrivate { get; set; }

        public int UserId { get; set; }
        
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
