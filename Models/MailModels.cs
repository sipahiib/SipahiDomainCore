using System.ComponentModel.DataAnnotations;

namespace SipahiDomainCore.Models
{
    public class MailModels
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email giriniz")]
        [EmailAddress(ErrorMessage = "Email formatında giriniz")]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Yorum giriniz")]
        [MaxLength(250)]
        public string Message { get; set; }
    }
}
