using FluentValidation;
using SipahiDomainCore.Models;

namespace SipahiDomainCore.Validators
{
    public class ContactFormValidator : AbstractValidator<MailModels>
    {
        public ContactFormValidator()
        {
            RuleFor(s => s.Email).NotEmpty().WithMessage("Email adresi giriniz").EmailAddress().WithMessage("Email formatında giriniz");
            RuleFor(x => x.Message).NotEmpty().WithMessage("Yorum giriniz").Length(1, 250).WithMessage("Max 250 karakter giriniz.");
        }
    }
}
