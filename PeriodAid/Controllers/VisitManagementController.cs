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
                          select m;
            ViewBag.Visitor = visitor;
            var company = from m in _vmdb.VM_Company
                          select m;
            ViewBag.Company = company;
            var Record = from m in _vmdb.VM_VisitRecord
                         select m;
            ViewBag.Recore = Record.Count();
            return View();
        }

        public ActionResult Visit_PartialView(DateTime? visit_date, string dep_id, string com_type, string com_name, string vis_name, int? page, int sort,int status,int page_count)  
        {
            int _page = page ?? 1;
            ViewBag.CurrentPage = _page;
            var config = from m in _vmdb.VM_ContentConfig
                         where m.Content_Type == 5
                         select m;
            ViewBag.Config = config;
            IQueryable<VM_VisitRecord> record;
            if (visit_date != null)
            {
                var _DateTime = visit_date.Value.Year + "-" + visit_date.Value.Month + "-" + visit_date.Value.Day;
                record = from m in _vmdb.VM_VisitRecord
                         where m.Visit_Time.Value.Year + "-" + m.Visit_Time.Value.Month + "-" + m.Visit_Time.Value.Day == _DateTime
                         && m.VM_Company.Company_Name == (com_name != "" ? com_name : m.VM_Company.Company_Name)
                         && m.VM_Employee.Employee_Name == (vis_name != "" ? vis_name : m.VM_Employee.Employee_Name)
                         && m.VM_Company.Company_Type == (com_type != "全部" ? com_type : m.VM_Company.Company_Type)
                         && m.VM_Employee.VM_Department.Department_Name == (dep_id != "" ? dep_id : m.VM_Employee.VM_Department.Department_Name)
                         && m.status == (status != 2 ? status :m.status)
                         select m;
            }
            else
            {
                record = from m in _vmdb.VM_VisitRecord
                         where m.Visit_Time.Value < DateTime.Now
                         && m.VM_Company.Company_Name == (com_name != "" ? com_name : m.VM_Company.Company_Name)
                         && m.VM_Employee.Employee_Name == (vis_name != "" ? vis_name : m.VM_Employee.Employee_Name)
                         && m.VM_Company.Company_Type == (com_type != "全部" ? com_type : m.VM_Company.Company_Type)
                         && m.VM_Employee.VM_Department.Department_Name == (dep_id != "" ? dep_id : m.VM_Employee.VM_Department.Department_Name)
                         && m.status == (status != 2 ? status : m.status)
                         select m;
            }
            if (sort == 0)
            {
                return PartialView(record.OrderByDescending(m => m.Visit_Time).OrderByDescending(m => m.Id).ToPagedList(_page, page_count));
            }
            else if (sort == 1)
            {
                return PartialView(record.OrderByDescending(m => m.Cooperation_Intention).OrderByDescending(m => m.Id).ToPagedList(_page, page_count));
            }
            else
            {
                return PartialView(record.OrderByDescending(m => m.VM_Company.VM_VisitRecord.Count()).OrderByDescending(m => m.Id).ToPagedList(_page, page_count));
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

        public ActionResult Comment_PartialView(int rec_id)
        {
            var comment = from m in _vmdb.VM_Comment
                          where m.VisitRecord_Id == rec_id && m.Comment_Type == 0
                          select m;
            return PartialView(comment);
        }
        // 评论数量
        [HttpPost]
        public JsonResult Comment_Count(int rec_id)
        {
            var com_count = from m in _vmdb.VM_Comment
                            where m.VisitRecord_Id == rec_id && m.Comment_Type == 0
                            select m;
            var replyCom_count = from m in _vmdb.VM_Comment
                                 where m.VisitRecord_Id == rec_id && m.Comment_Type == 3
                                 select m;
            var coreCom_count = from m in _vmdb.VM_Comment
                                where m.VisitRecord_Id == rec_id && m.Comment_Type == 1
                                select m;
            var supportCom_count = from m in _vmdb.VM_Comment
                                   where m.VisitRecord_Id == rec_id && m.Comment_Type == 2
                                   select m;
            return Json(new { result = "SUCCESS", comment = com_count.Count(), reply = replyCom_count.Count(), core = coreCom_count.Count(), support = supportCom_count.Count(), rid = rec_id });
        }

        public ActionResult ReplyComment_PartialView(int rec_id)
        {
            var Replycomment = from m in _vmdb.VM_Comment
                               where m.VisitRecord_Id == rec_id && m.Comment_Type == 3
                               select m;
            return PartialView(Replycomment);
        }

        public ActionResult CoreComment_PartialView(int rec_id)
        {
            var Corecomment = from m in _vmdb.VM_Comment
                              where m.VisitRecord_Id == rec_id && m.Comment_Type == 1
                              select m;
            return PartialView(Corecomment);
        }

        public ActionResult SupportComment_PartialView(int rec_id)
        {
            var Supportcomment = from m in _vmdb.VM_Comment
                                 where m.VisitRecord_Id == rec_id && m.Comment_Type == 2
                                 select m;
            return PartialView(Supportcomment);
        }

        public ApplicationUser getUser(string username)
        {
            var user = memdb.Users.SingleOrDefault(m => m.UserName == username);
            return user;
        }

        [HttpPost]
        public ActionResult CreateComment(string detail, int rec_id)
        {
            var user = getUser(User.Identity.Name);
            VM_Comment comment = new VM_Comment();
            comment.Comment_Time = DateTime.Now;
            comment.Nick_Name = user.NickName;
            comment.User_Name = user.UserName;
            comment.VisitRecord_Id = rec_id;
            comment.Comment_Detail = detail;
            comment.Comment_Type = 0;
            comment.Comment_Status = 0;
            _vmdb.VM_Comment.Add(comment);
            _vmdb.SaveChanges();
            return Json(new { result = "SUCCESS" });
        }

        [HttpPost]
        public ActionResult CreateReplyComment(string detail, int rec_id)
        {
            var user = getUser(User.Identity.Name);
            VM_Comment comment = new VM_Comment();
            comment.Comment_Time = DateTime.Now;
            comment.Nick_Name = user.NickName;
            comment.User_Name = user.UserName;
            comment.VisitRecord_Id = rec_id;
            comment.Comment_Detail = detail;
            comment.Comment_Type = 3;
            comment.Comment_Status = 0;
            _vmdb.VM_Comment.Add(comment);
            _vmdb.SaveChanges();
            return Json(new { result = "SUCCESS" });
        }

        [HttpPost]
        public ActionResult CreateCoreComment(string detail, int rec_id)
        {
            var user = getUser(User.Identity.Name);
            VM_Comment comment = new VM_Comment();
            comment.Comment_Time = DateTime.Now;
            comment.Nick_Name = user.NickName;
            comment.User_Name = user.UserName;
            comment.VisitRecord_Id = rec_id;
            comment.Comment_Detail = detail;
            comment.Comment_Type = 1;
            comment.Comment_Status = 0;
            _vmdb.VM_Comment.Add(comment);
            _vmdb.SaveChanges();
            return Json(new { result = "SUCCESS" });
        }

        [HttpPost]
        public ActionResult CreateSupportComment(string detail, int rec_id)
        {
            var user = getUser(User.Identity.Name);
            VM_Comment comment = new VM_Comment();
            comment.Comment_Time = DateTime.Now;
            comment.Nick_Name = user.NickName;
            comment.User_Name = user.UserName;
            comment.VisitRecord_Id = rec_id;
            comment.Comment_Detail = detail;
            comment.Comment_Type = 2;
            comment.Comment_Status = 0;
            _vmdb.VM_Comment.Add(comment);
            _vmdb.SaveChanges();
            return Json(new { result = "SUCCESS" });
        }

        // 审批
        [HttpPost]
        public JsonResult Supervise_Status(int rec_id, int rec_status, string detail,string cause)
        {
            try
            {
                var user = getUser(User.Identity.Name);
                var record = _vmdb.VM_VisitRecord.SingleOrDefault(m => m.Id == rec_id);
                record.Veto_Detail = detail;
                record.status = rec_status;
                record.Veto_Cause = cause;
                _vmdb.Entry(record).State = System.Data.Entity.EntityState.Modified;
                _vmdb.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
        }

        // 公司
        public ActionResult Company_View()
        {
            var visitor = from m in _vmdb.VM_Employee
                          select m;
            ViewBag.Visitor = visitor;
            var company = from m in _vmdb.VM_Company
                          select m;
            ViewBag.Company = company;
            var count = from m in _vmdb.VM_Company
                        select m;
            ViewBag.Count = count.Count();
            return View();
        }

        public ActionResult Company_PartialView(int dep_id, string com_type, string com_name, string vis_name, int? page,int page_count)
        {
            int _page = page ?? 1;
            ViewBag.CurrentPage = _page;
            var company = (from m in _vmdb.VM_Company
                           where m.VM_Employee.Department_Id == (dep_id != 0 ? dep_id : m.VM_Employee.Department_Id)
                           && m.Company_Type == (com_type != "全部" ? com_type : m.Company_Type)
                           && m.Company_Name == (com_name != "" ? com_name : m.Company_Name)
                           && m.VM_Employee.Employee_Name == (vis_name != "" ? vis_name : m.VM_Employee.Employee_Name)
                           orderby m.Id descending
                           select m).ToPagedList(_page, page_count);
            return PartialView(company);
        }

    }
}