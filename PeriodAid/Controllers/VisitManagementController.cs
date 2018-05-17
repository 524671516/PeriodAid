using CsvHelper;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PagedList;
using PeriodAid.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml;


namespace PeriodAid.Controllers
{
    public class VisitManagementController : Controller
    {
        // GET: VisitManagement
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private VisitManagementModels _vmdb;
        public VisitManagementController()
        {
            _vmdb = new VisitManagementModels();
        }
        public VisitManagementController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }


        public ActionResult Visit_View()
        {
            var visitor = from m in _vmdb.VM_Employee
                          select m;
            ViewBag.Visitor = visitor;
            var company = from m in _vmdb.VM_Company
                          select m;
            ViewBag.Company = company;
            return View();
        }

        public ActionResult Visit_PartialView(DateTime visit_date,int dep_id,string com_type,string com_name,string vis_name,int? page)
        {
            var _DateTime = visit_date.Year + "-" + visit_date.Month + "-" + visit_date.Day;
            int _page = page ?? 1;
            var Record = from m in _vmdb.VM_VisitRecord
                         where com_name != "" ? m.VM_Company.Company_Name == com_name : m.VM_Company.Company_Status == 0
                         select m;
            if (vis_name!= "")
            {
                var record = (from m in Record
                              where m.VM_Employee.Department_Id == dep_id && m.Visit_Time.Value.Year + "-" + m.Visit_Time.Value.Month + "-" + m.Visit_Time.Value.Day == _DateTime
                              && m.VM_Company.Company_Type == com_type && m.VM_Employee.Employee_Name == vis_name
                              orderby m.Visit_Time descending
                              select m).ToPagedList(_page, 20);
                return PartialView(record);
            }else
            {
                var record = (from m in Record
                              where m.VM_Employee.Department_Id == dep_id && m.Visit_Time.Value.Year + "-" + m.Visit_Time.Value.Month + "-" + m.Visit_Time.Value.Day == _DateTime
                              && m.VM_Company.Company_Type == com_type
                              orderby m.Visit_Time descending
                              select m).ToPagedList(_page, 20);
                return PartialView(record);
            }
            
        }

        [HttpPost]
        public JsonResult QueryVisitor(string query)
        {
            var visitor = from m in _vmdb.VM_Employee
                          where m.Employee_Name.Contains(query)
                          select new { Id = m.Id, VisitorName = m.Employee_Name };
            return Json(visitor);
        }

        [HttpPost]
        public JsonResult QueryCompany(string query)
        {
            var company = from m in _vmdb.VM_Company
                          where m.Company_Name.Contains(query)
                          select new { Id = m.Id, CompanyName = m.Company_Name };
            return Json(company);
        }

        public ActionResult CompanyInfo(int com_id)
        {
            var item = _vmdb.VM_Company.SingleOrDefault(m => m.Id == com_id);
            return PartialView(item);
        }

        public ActionResult Comment_View(int rec_id)
        {
            var record = _vmdb.VM_Comment.FirstOrDefault(m => m.VisitRecord_Id == rec_id);
            ViewBag.Record = record.VisitRecord_Id;
            return View();
        }

        public ActionResult Comment_PartialView(int rec_id)
        {
            var comment = from m in _vmdb.VM_Comment
                          where m.VisitRecord_Id == rec_id
                          select m;
            return PartialView(comment);
        }

        [HttpPost]
        public ActionResult CreateComment(string detail)
        {
            VM_Comment comment = new VM_Comment();
            comment.Comment_Time = DateTime.Now;
            _vmdb.VM_Comment.Add(comment);
            _vmdb.SaveChanges();
            return Json(new { result = "SUCCESS" });
        }
    }
}