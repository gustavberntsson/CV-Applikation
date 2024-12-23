using System.ComponentModel.DataAnnotations.Schema;

namespace CV_Applikation.Models
{
    public class Languages
    {
        public string LanguageName { get; set; }
        public int Level { get; set; }
        public int CVId { get; set; }

        [ForeignKey(nameof(CVId))]
        public virtual CV? Cv { get; set; }
    }
}
