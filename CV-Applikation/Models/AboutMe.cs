using System.ComponentModel.DataAnnotations.Schema;

namespace CV_Applikation.Models
{
    public class AboutMe
    {

        public string Description { get; set; }

        public int CVId { get; set; }

        [ForeignKey(nameof(CVId))]
        public virtual CV? Cv { get; set; }
    }
}
