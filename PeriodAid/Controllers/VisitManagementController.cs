using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

namespace SqzEvent.Controllers
{
    public class VisitManagementController : Controller
    {
        // GET: VisitManagement
        //private VisitManagementModels  _vmdb = new VisitManagementModels();
        public ActionResult Visit_View()
        {
            return View();
        }

        public ActionResult Visit_PartialView(DateTime visit_date)
        {
            return PartialView();
        }

        //[HttpPost]
        //public JsonResult QueryVisitor(string query,int d_id)
        // {
        //    var visitor = from m in _vmdb.VM_Employee
        //                  where m.Employee_Name.Contains(query) && m.VM_Department.Id == d_id
        //                  select new { Id = m.Id, VisitorName = m.Employee_Name };
        //    return Json(visitor);
        //}

        //[HttpPost]
        //public JsonResult QueryCustomer(string query,int cus_type)
        //{
        //    var customer = from m in _vmdb.VM_Customer
        //                   where m.Customer_Name.Contains(query)
        //                   select new { Id = m.Id, CustomerName = m.Customer_Name };
        //    return Json(customer);
        //}
    }
}