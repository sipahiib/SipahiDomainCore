using System.ComponentModel.DataAnnotations;

namespace SipahiDomainCore.Models
{
    public class Users
    {
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Email giriniz")]
        [EmailAddress(ErrorMessage = "Email formatında giriniz")]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Şifrenizi giriniz")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Şifreniz min 8 karakter olmalıdır")]
        public string Sifre { get; set; }
        //[Required(AllowEmptyStrings = false, ErrorMessage = "Şifrenizi tekrar giriniz")]
        //[DataType(DataType.Password)]
        //[Compare("Sifre", ErrorMessage = "Şifreniz yukarıdaki şifre ile uyuşmamaktadır")]
        public string OnaySifre { get; set; }
    }
}
