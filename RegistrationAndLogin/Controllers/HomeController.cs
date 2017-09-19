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
        #region Get Employee Data From Datatable.
        public ActionResult GetEmployees()
        {
            using (MidasEMSEmployeeDataEntities dc = new MidasEMSEmployeeDataEntities())
            {
                var employees = dc.Employees.OrderBy(a => a.FirstName).ToList();
                return Json(new { data = employees }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Save Employee
        [HttpGet]
        public ActionResult Save(int id)
        {
            using (MidasEMSEmployeeDataEntities dc = new MidasEMSEmployeeDataEntities())
            {
                var v = dc.Employees.Where(a => a.EmployeeID == id).FirstOrDefault();
                return View(v);
            }
        }
        #endregion

        #region Check EmployeeID
        [HttpPost]
        public ActionResult Save(Employee emp)
        {
            bool status = false;
            if (ModelState.IsValid)
            {
                using (MidasEMSEmployeeDataEntities dc = new MidasEMSEmployeeDataEntities())
                {
                    if (emp.EmployeeID > 0)
                    {
                        // Edit
                        var v = dc.Employees.Where(a => a.EmployeeID == emp.EmployeeID).FirstOrDefault();
                        if (v != null)
                        {
                            v.FirstName = emp.FirstName;
                            v.LastName = emp.LastName;
                            v.EmailID = emp.EmailID;
                            v.City = emp.City;
                            v.Country = emp.Country;
                        }
                    }
                    else
                    {
                        //Save
                        dc.Employees.Add(emp);
                    }
                    dc.SaveChanges();
                    status = true;
                }
            }
            return new JsonResult { Data = new { status = status } };
        }
        #endregion

        #region Delete Employee
        public ActionResult Delete(int id)
        {
            using (MidasEMSEmployeeDataEntities dc = new MidasEMSEmployeeDataEntities())
            {
                var v = dc.Employees.Where(a => a.EmployeeID == id).FirstOrDefault();
                if (v != null)
                {
                    return View(v);
                }
                else
                {
                    return HttpNotFound();
                }
            }
        }
        #endregion

        #region Confirm Delete
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult ConfirmDelete(int id)
        {
            bool status = false;

            using (MidasEMSEmployeeDataEntities dc = new MidasEMSEmployeeDataEntities())
            {
                var v = dc.Employees.Where(a => a.EmployeeID == id).FirstOrDefault();
                if (v != null)
                {
                    dc.Employees.Remove(v);
                    dc.SaveChanges();
                    status = true;
                }
            }

            return new JsonResult { Data = new { status = status } };
        }
        #endregion
    }
}