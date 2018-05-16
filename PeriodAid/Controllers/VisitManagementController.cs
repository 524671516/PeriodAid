using CsvHelper;
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
        private VisitManagementModels _vmdb;
        public VisitManagementController()
        {
            _vmdb = new VisitManagementModels();
        }
        public ActionResult Visit_View()
        {
            var visitor = from m in _vmdb.VM_Employee
                          select m;
            ViewBag.Visitor = visitor;
            return View();
        }

        public ActionResult Visit_PartialView(DateTime visit_date,int dep_id,string com_type,string com_name,string vis_name,int? page)
        {
            var _DateTime = visit_date.Year + "-" + visit_date.Month + "-" + visit_date.Day;
            int _page = page ?? 1;
            var record = (from m in _vmdb.VM_VisitRecord
                          where m.VM_Employee.Department_Id == dep_id && m.Visit_Time.Value.Year + "-" + m.Visit_Time.Value.Month + "-" + m.Visit_Time.Value.Day == _DateTime
                          && m.VM_Company.Company_Type == com_type
                          orderby m.Visit_Time descending
                          select m).ToPagedList(_page, 20);
            return PartialView(record);
        }

        //[HttpPost]
        //public JsonResult QueryVisitor(string query)
        //{
        //    var visitor = from m in _vmdb.VM_Employee
        //                  where m.Employee_Name.Contains(query)
        //                  select new { Id = m.Id, VisitorName = m.Employee_Name };
        //    return Json(visitor);
        //}

        //[HttpPost]
        //public JsonResult QueryCompany(string query)
        //{
        //    var company = from m in _vmdb.VM_Company
        //                  where m.Company_Name.Contains(query)
        //                  select new { Id = m.Id, CompanyName = m.Company_Name };
        //    return Json(company);
        //}
    }
}