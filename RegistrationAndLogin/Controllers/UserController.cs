using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography.Pkcs;
using System.Web;
using System.Web.Mvc;
using RegistrationAndLogin.Models;

namespace RegistrationAndLogin.Controllers
{
    public class UserController : Controller
    {
        //Registration Action   
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        //Registration POST Action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified,ActivationCode")]User user)
        {
            bool Status = false;
            string message = "";

            //Model Validation
            if (ModelState.IsValid)
            {
                #region Email Already Exists
                var isExist = IsEmailExist(user.Email);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email already exist");
                    return View();
                }
                #endregion

                #region Generate Activation Code

                user.ActivationCode = Guid.NewGuid();

                #endregion

                #region Password Hashing

                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword);

                #endregion

                user.IsEmailVerified = false;

                #region Save Data To DB

                using (MidasEMSEntities dc = new MidasEMSEntities())
                {
                    dc.Users.Add(user);
                    dc.SaveChanges();

                    //Send Email To User
                    SendVerificationLinkEmail(user.Email, user.ActivationCode.ToString());
                    message = "Registration successfully done. Account activation link " +
                              " has been sent to your email id:" + user.Email;
                    Status = true;
                }


                #endregion

            }
            else
            {
                message = "Invalid Request";
            }

            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(user);
        }

        //Verify Email Account Action

        //Verify Email Link Action

        //Login Action

        //Login POST Action

        //Logout Action

        [NonAction]
        public bool IsEmailExist(string email)
        {
            using (MidasEMSEntities dc = new MidasEMSEntities())
            {
                var v = dc.Users.Where(a => a.Email == email).FirstOrDefault();
                return v != null;
            }
        }

        [NonAction]
        public void SendVerificationLinkEmail(string emailID, string activationCode)
        {
            //var scheme = Request.Url.Scheme;
            //var host = Request.Url.Host;
            //var port = Request.Url.Port;

            //string url = scheme + "://" + host + "://";

            var verifyUrl = "/User/VerifyAccount" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("sappakane@gmail.com", "Midas Safety IT Consultant Trainee");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "sappakane@123";
            string subject = "Your account is successfully created!";
            string body = "<br/><br/>We are excited to tell you that your account Midas Safety IT Consultant Trainee is" +
                " successfully created. Please click on the below link to verify your account" +
                "<br/><br/><a href='"+link+"'>"+link+"</a>";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })

            smtp.Send(message);

        }
    }
}