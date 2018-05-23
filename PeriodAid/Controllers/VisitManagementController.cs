﻿using CsvHelper;
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
    [Authorize(Roles = "SuperAdmin")]
    public class VisitManagementController : Controller
    {
        // GET: VisitManagement
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private VisitManagementModels _vmdb;
        private ApplicationDbContext memdb;
        public VisitManagementController()
        {
            _vmdb = new VisitManagementModels();
            memdb = new ApplicationDbContext();
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
                          where m.Employee_Name != "季锦良"
                          select m;
            ViewBag.Visitor = visitor;
            var company = from m in _vmdb.VM_Company
                          select m;
            ViewBag.Company = company;
            return View();
        }

        public ActionResult Visit_PartialView(DateTime? visit_date,int dep_id,string com_type,string com_name,string vis_name,int? page,int sort)
        {
            int _page = page ?? 1;
            IQueryable<VM_VisitRecord>record;
            if (visit_date != null)
            {
                var _DateTime = visit_date.Value.Year + "-" + visit_date.Value.Month + "-" + visit_date.Value.Day;
                record = from m in _vmdb.VM_VisitRecord
                         where m.Visit_Time.Value.Year + "-" + m.Visit_Time.Value.Month + "-" + m.Visit_Time.Value.Day == _DateTime
                         && m.VM_Company.Company_Name == (com_name != "" ? com_name : m.VM_Company.Company_Name)
                         && m.VM_Employee.Employee_Name == (vis_name != "" ? vis_name : m.VM_Employee.Employee_Name)
                         && m.VM_Company.Company_Type == (com_type != "全部类型" ? com_type : m.VM_Company.Company_Type)
                         && m.VM_Employee.Department_Id == (dep_id != 0 ? dep_id : m.VM_Employee.Department_Id)
                         select m;
            }
            else
            {
               record = from m in _vmdb.VM_VisitRecord
                        where m.Visit_Time.Value < DateTime.Now
                        && m.VM_Company.Company_Name == (com_name != "" ? com_name : m.VM_Company.Company_Name)
                        && m.VM_Employee.Employee_Name == (vis_name != "" ? vis_name : m.VM_Employee.Employee_Name)
                        && m.VM_Company.Company_Type == (com_type != "全部类型" ? com_type : m.VM_Company.Company_Type)
                        && m.VM_Employee.Department_Id == (dep_id != 0 ? dep_id : m.VM_Employee.Department_Id)
                        select m;
            }
            if (sort == 0)
            {
                return PartialView(record.OrderByDescending(m => m.Visit_Time).ToPagedList(_page, 20));
            }
            else if (sort == 1)
            {
                return PartialView(record.OrderByDescending(m => m.Cooperation_Intention).ToPagedList(_page, 20));
            }
            else
            {
                return PartialView(record.OrderByDescending(m => m.VM_Company.VM_VisitRecord.Count()).ToPagedList(_page, 20));
            }
        }
        
        [HttpPost]
        public JsonResult QueryVisitor(string query)
        {
            var visitor = from m in _vmdb.VM_Employee
                          where m.Employee_Name.Contains(query) && m.Employee_Name != "季锦良"
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

        public ActionResult CompanyInfo(int com_id, int rec_id)
        {
            var item = _vmdb.VM_Company.SingleOrDefault(m => m.Id == com_id);
            var record = _vmdb.VM_VisitRecord.SingleOrDefault(m => m.Id == rec_id);
            ViewBag.Record = record;
            return PartialView(item);
        }
        
        public ActionResult Comment_PartialView(int rec_id)
        {
            var comment = from m in _vmdb.VM_Comment
                          where m.VisitRecord_Id == rec_id
                          select m;
            return PartialView(comment);
        }

        public ActionResult ReplyComment_PartialView(int rec_id)
        {
            var Replycomment = from m in _vmdb.VM_ReplyComment
                          where m.VisitRecord_Id == rec_id
                          select m;
            return PartialView(Replycomment);
        }

        public ActionResult CoreComment_PartialView(int rec_id)
        {
            var Corecomment = from m in _vmdb.VM_CoreComment
                               where m.VisitRecord_Id == rec_id
                               select m;
            return PartialView(Corecomment);
        }

        public ApplicationUser getUser(string username)
        {
            var user = memdb.Users.SingleOrDefault(m => m.UserName == username);
            return user;
        }

        [HttpPost]
        public ActionResult CreateComment(string detail,int rec_id)
        {
            var user = getUser(User.Identity.Name);
            var visitor = _vmdb.VM_Employee.SingleOrDefault(m => m.Employee_Name == user.NickName);
            if (visitor!=null)
            {
                VM_Comment comment = new VM_Comment();
                comment.Comment_Time = DateTime.Now;
                comment.Employee_Id = visitor.Id;
                comment.VisitRecord_Id = rec_id;
                comment.Comment_Detail = detail;
                _vmdb.VM_Comment.Add(comment);
                _vmdb.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }else
            {
                return Json(new { result = "FAIL" });
            }
        }

        [HttpPost]
        public ActionResult CreateReplyComment(string detail, int rec_id)
        {
            var user = getUser(User.Identity.Name);
            var visitor = _vmdb.VM_Employee.SingleOrDefault(m => m.Employee_Name == user.NickName);
            if (visitor != null)
            {
                VM_ReplyComment comment = new VM_ReplyComment();
                comment.ReplyComment_Time = DateTime.Now;
                comment.Employee_Id = visitor.Id;
                comment.VisitRecord_Id = rec_id;
                comment.ReplyComment_Detail = detail;
                _vmdb.VM_ReplyComment.Add(comment);
                _vmdb.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }

        [HttpPost]
        public ActionResult CreateCoreComment(string detail, int rec_id)
        {
            var user = getUser(User.Identity.Name);
            var visitor = _vmdb.VM_Employee.SingleOrDefault(m => m.Employee_Name == user.NickName);
            if (visitor != null)
            {
                VM_CoreComment comment = new VM_CoreComment();
                comment.CoreComment_Time = DateTime.Now;
                comment.Employee_Id = visitor.Id;
                comment.VisitRecord_Id = rec_id;
                comment.CoreComment_Detail = detail;
                _vmdb.VM_CoreComment.Add(comment);
                _vmdb.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }

        // 审批
        [HttpPost]
        public JsonResult Supervise_Status(int rec_id,int rec_status, string detail)
        {
            var user = getUser(User.Identity.Name);
            var visitor = _vmdb.VM_Employee.SingleOrDefault(m => m.Employee_Name == user.NickName);
            var record = _vmdb.VM_VisitRecord.SingleOrDefault(m => m.Id == rec_id);
            if (visitor!=null)
            {
                record.Veto_Detail = detail;
                record.status = rec_status;
                _vmdb.Entry(record).State = System.Data.Entity.EntityState.Modified;
                _vmdb.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }else
            {
                return Json(new { result = "FAIL" });
            }
        }
    }
}