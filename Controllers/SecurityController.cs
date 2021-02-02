using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SipahiDomainCore.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace SipahiDomainCore.Controllers
{
    public class SecurityController : Controller
    {

        [Route("giris")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Route("giris")]
        public IActionResult Login(Users kullanici)
        {
            HttpContext.Session.SetString("kullanici", string.Empty);

            if (ModelState.IsValid)
            {
                Context sipahiibEntity = new Context();

                var firstLogin = sipahiibEntity.Users.FirstOrDefault(m => m.Email == kullanici.Email);
                String[] parts = kullanici.Email.Split(new[] { '@' });

                if (firstLogin == null && kullanici.Email != null)
                {
                    kullanici.OnaySifre = kullanici.Sifre;
                    HttpContext.Session.SetString("kullanici", parts[0]);

                    sipahiibEntity.Users.Add(kullanici);
                    sipahiibEntity.SaveChanges();
                }

                var validKullanici = sipahiibEntity.Users.FirstOrDefault(m => m.Email == kullanici.Email && m.Sifre == kullanici.Sifre);

                if (validKullanici != null)
                {
                    HttpContext.Session.SetString("kullanici", parts[0]);
                }
                else
                {
                    TempData["Invalid"] = "Geçersiz kullanıcı adı veya şifre";
                    return View();
                }

                TempData["Kullanici"] = HttpContext.Session.GetString("kullanici");
                return RedirectToAction("Index", "Home");
            }

            return View(kullanici);
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            HttpContext.Session.SetString("kullanici", string.Empty);
            return RedirectToAction("Index", "Home");
        }

        [Route("sifrehatirlat")]
        public IActionResult PasswordRemember()
        {
            return View();
        }

        [HttpPost]
        [Route("sifrehatirlat")]
        public IActionResult PasswordRemember(Users e)
        {
            if (e.Email == null)
            {
                TempData["Remember"] = "Email girmeniz gerekmektedir";
            }
            else
            {
                Context sipahiibEntity = new Context();

                var existingEmail = sipahiibEntity.Users.FirstOrDefault(m => m.Email == e.Email);
                if (existingEmail != null && existingEmail.Email == e.Email)
                {
                    StringBuilder message = new StringBuilder();
                    MailAddress fromAdd = new MailAddress(existingEmail.Email.ToString());

                    message.AppendLine(fromAdd + " kullanıcısının şifresi: " + existingEmail.Sifre.ToString());

                    MailMessage mail = new MailMessage();
                    var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Mail");

                    mail.From = fromAdd;
                    mail.To.Add(config["InfoMail"]);
                    mail.Subject = "Şifre Hatırlatma Talebi";
                    mail.Body = message.ToString();
                    mail.IsBodyHtml = true;

                    SendMail(mail, config);
                }
                else
                {
                    TempData["Remember"] = "Kayıtlı olmayan bir email girdiniz";
                }
            }
            ModelState.Clear();
            return View();
        }

        private void SendMail(MailMessage mail, IConfigurationSection config)
        {
            try
            {
                using (var smtpClient = new SmtpClient(config["Smtp"], 587))
                {
                    string password = config["MailPassword"];
                    string domain = config["Domain"];

                    smtpClient.EnableSsl = false;
                    smtpClient.UseDefaultCredentials = false;
                    NetworkCredential cred = new NetworkCredential(config["InfoMail"], password);
                    cred.Domain = domain;
                    smtpClient.Credentials = cred;

                    smtpClient.Send(mail);
                    TempData["Remember"] = "Şifreniz kısa süre sonra mail adresinize gönderilecektir";
                }
            }
            catch (SmtpFailedRecipientsException ex)
            {
                foreach (SmtpFailedRecipientException t in ex.InnerExceptions)
                {
                    var status = t.StatusCode;
                    if (status == SmtpStatusCode.MailboxBusy ||
                        status == SmtpStatusCode.MailboxUnavailable)
                    {
                        HttpContext.Response.WriteAsync("Delivery failed - retrying in 5 seconds.");
                        System.Threading.Thread.Sleep(5000);
                    }
                    else
                    {
                        TempData["Remember"] = "Şifreniz gönderilemedi!";
                        HttpContext.Response.WriteAsync("Failed to deliver message ");
                    }
                }
            }
            catch (SmtpException ex)
            {
                TempData["Remember"] = "Şifreniz gönderilemedi!";
                HttpContext.Response.WriteAsync(ex.ToString());
            }

            catch (Exception ex)
            {
                TempData["Remember"] = "Şifreniz gönderilemedi!";
                HttpContext.Response.WriteAsync(ex.ToString());
            }
        }
    }
}
