using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using SipahiDomainCore.Models;
using SipahiDomainCore.Validators;
using SipahiDomainCoreML.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;


namespace SipahiDomainCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            TempData["Kullanici"] = HttpContext.Session.GetString("kullanici");

            #region Method1
            //var followups = new List<Followups>()
            //{
            //    new Followups() { Name= "Tim Corey", Link="https://www.youtube.com/user/IAmTimCorey"} ,
            //    new Followups() { Name= "Bora Kaşmer", Link="https://www.youtube.com/channel/UCEQB9Atxfn5AJgX6KfwvTyA"} ,
            //    new Followups() { Name= "dotNET", Link="https://www.youtube.com/channel/UCvtT19MZW8dq5Wwfu6B0oxw"},
            //    new Followups() { Name= "NDC Conferences", Link="https://www.youtube.com/channel/UCTdw38Cw6jcm0atBPA39a0Q"},
            //    new Followups() { Name= "Barış Özcan", Link="https://www.youtube.com/channel/UCv6jcPwFujuTIwFQ11jt1Yw"},
            //    new Followups() { Name= "Özgür Demirtaş", Link="https://twitter.com/ProfDemirtas"},
            //    new Followups() { Name= "Emin Çapa", Link="https://twitter.com/ecapa_aklinizi"}
            //};

            //var educations = new List<Education>()
            //{
            //    new Education() { Name="Medium" , Link="https://www.medium.com/"},
            //    new Education() { Name="Machine Learning" , Link="https://www.coursera.org/learn/machine-learning"},
            //    new Education() { Name="edX-Online Course" , Link="https://www.edx.org/"},
            //    new Education() { Name="Edabit" , Link="https://edabit.com"},
            //    new Education() { Name="Hackerrank" , Link="https://hackerrank.com"},
            //    new Education() { Name="Flutter" , Link="https://www.youtube.com/watch?v=ulg2dpPkulw&list=PLUbFnGajtZlX9ubiLzYz_cw92esraiIBi&index=1"},
            //    new Education() { Name="Microservices" , Link="https://microservices.io/"},
            //    new Education() { Name="Techie Delight" , Link="https://www.techiedelight.com/"},
            //    new Education() { Name="Geeks for Geeks" , Link="https://www.geeksforgeeks.org/"},
            //    new Education() { Name="Mobilhanem" , Link="https://www.mobilhanem.com/"}
            //}; 
            #endregion

            var builder = new ConfigurationBuilder().AddJsonFile("Descriptions.json").Build();
            var model = new ViewModels.ViewModels()
            {
                Followups = builder.GetSection("Followup"),
                Educations = builder.GetSection("Education")
            };

            return View(model);
        }

        [Route("hakkimda")]
        public IActionResult About()
        {
            return View();
        }

        [Route("iletisim")]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Route("iletisim")]
        public IActionResult Contact(MailModels e)
        {
            ContactFormValidator validator = new ContactFormValidator();
            ValidationResult result = validator.Validate(e);

            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }

                return View();
            }


            //if (ModelState.IsValid)
            //{
            CommentPrediction(e);

            #region MailInfo
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Mail");
            string info = config["InfoMail"];

            StringBuilder message = new StringBuilder();
            MailAddress from = new MailAddress(e.Email.ToString());
            message.AppendLine("Email: " + e.Email);
            message.AppendLine(e.Message);

            MailMessage mail = new MailMessage();

            mail.From = from;
            mail.To.Add(info);
            mail.Subject = "Yeni Bir Görüş";
            mail.Body = message.ToString();
            mail.IsBodyHtml = true;
            #endregion

            SendMail(mail, config);
            //}

            ModelState.Clear();
            return View();
        }


        private void CommentPrediction(MailModels e)
        {
            MLContext mlContext = new MLContext();

            ITransformer mlModel = mlContext.Model.Load("MLModel.zip", out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);

            var input = new ModelInput();
            input.Comment = e.Message;

            ModelOutput res = predEngine.Predict(input);

            if (Convert.ToDecimal(res.Prediction) < (decimal)0.5)
            {
                TempData["Tahmin"] = "'Olumsuz' görüş bildirdiniz. Dikkate alacağım.";
                res.Prediction = 0;
            }
            else if (Convert.ToDecimal(res.Prediction) >= (decimal)0.5 && Convert.ToDecimal(res.Prediction) < 1)
            {
                TempData["Tahmin"] = "'Fena Değil' görüşü bildirdiniz. Dikkate alacağım.";
                res.Prediction = (float)0.5;
            }
            else
            {
                TempData["Tahmin"] = "'Olumlu' görüş bildirdiniz. Teşekkürler!";
                res.Prediction = 1;
            }

            using (StreamWriter writer = System.IO.File.AppendText("ML.txt"))
            {
                writer.WriteLine(res.Prediction + "\t" + e.Message);
            }
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
                    TempData["Mail"] = "Mail gönderildi";
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
                        TempData["Mail"] = "Mail gönderilemedi";
                        HttpContext.Response.WriteAsync("Failed to deliver message ");
                    }
                }
            }
            catch (SmtpException ex)
            {
                TempData["Mail"] = "Mail gönderilemedi";
                HttpContext.Response.WriteAsync(ex.ToString());
            }

            catch (Exception ex)
            {
                TempData["Mail"] = "Mail gönderilemedi";
                HttpContext.Response.WriteAsync(ex.ToString());
            }
        }

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}
    }
}
