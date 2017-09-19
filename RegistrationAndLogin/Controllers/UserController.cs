using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography.Pkcs;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using RegistrationAndLogin.Models;

namespace RegistrationAndLogin.Controllers
{
    public class UserController : Controller
    {
        #region Registration
              
        //Registration Action   
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }
        #endregion

        #region Registration POST Action

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified,ActivationCode")]User user)
        {
            bool Status = false;
            string message = "";

            //Model Validation
            if (ModelState.IsValid)
            {

                #endregion

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

        #region SetIsEmailVerifiedFalse

                user.IsEmailVerified = false;

                #endregion

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


                

            }
            else
            {
                message = "Invalid Request";
            }

            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(user);
        }
        #endregion

        #region Verify Email Account

        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;
            using (MidasEMSEntities dc = new MidasEMSEntities())
            {
                dc.Configuration.ValidateOnSaveEnabled = false; // Avoid Confirm Password Match 
                var v = dc.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                if (v != null)
                {
                    v.IsEmailVerified = true;
                    dc.SaveChanges();
                    Status = true;
                }
                else
                {
                    ViewBag.Status("Invalid Request");
                }
            }
            ViewBag.Status = Status;
            return View();
        }

        #endregion 

        #region Login Action

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }


        #endregion

        #region Login POST Action

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin login, string ReturnUrl)
        {
            string message = "";

            using (MidasEMSEntities dc =  new MidasEMSEntities())
            {
                var v = dc.Users.Where(a => a.Email == login.Email).FirstOrDefault();
                if (v != null)
                {
                    if (string.Compare(Crypto.Hash(login.Password), v.Password) == 0)
                    {
                        int timeout = login.RememberMe ? 525600 : 20; // 525600 min = 1 year
                        var ticket = new FormsAuthenticationTicket(login.Email, login.RememberMe, timeout);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName,encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);


                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        message = "Invalid Credentials Provided";
                    }
                }
                else
                {
                    message = "Invalid Credentials Provided";
                }            
            }

            ViewBag.Message = message;
            return View();
        }

        #endregion

        #region Logout Action

        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");
        }
        #endregion

        #region IsEmailExist

        [NonAction]
        public bool IsEmailExist(string email)
        {
            using (MidasEMSEntities dc = new MidasEMSEntities())
            {
                var v = dc.Users.Where(a => a.Email == email).FirstOrDefault();
                return v != null;
            }
        }

        #endregion

        #region SendVerificationLinkEmail

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
        
            var fromEmailPassword = "a64cd9fcfd06b67367e54707494c139b";

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
        #endregion
    }
}