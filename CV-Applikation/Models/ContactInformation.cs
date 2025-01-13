using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CV_Applikation.Models
{
    public class ContactInformation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string LastName { get; set; }

        [Required]
        [StringLength(255)]  
        public string Adress { get; set; }

        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Telefonnummer måste vara 10 siffror.")]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Range(0, 10000)]
        public int? ViewCount { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}