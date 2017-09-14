using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RegistrationAndLogin.Models;

namespace RegistrationAndLogin.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }
        //public ActionResult GetEmployees()
        //{
        //    using (MidasEMSEmployeeDataEntities dc = new MidasEMSEmployeeDataEntities())
        //    {
        //        var employees = dc.Employees.OrderBy(a => a.FirstName).ToList();
        //        return Json(new { data = employees }, JsonRequestBehavior.AllowGet);
        //    }
        //}
    }
}