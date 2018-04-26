using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using PeriodAid.DAL;
using PeriodAid.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace PeriodAid.Controllers
{
    [Authorize(Roles = "Staff")]
    public class MobileTaskManagementController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ProjectSchemeModels _db;
        private TaskManagementLogsUtilities _log;
        public MobileTaskManagementController()
        {
            _db = new ProjectSchemeModels();
            _log = new TaskManagementLogsUtilities();
        }
        public MobileTaskManagementController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        [AllowAnonymous]
        public async Task<ActionResult> TestLogin(string tel)
        {
            var user = UserManager.FindByName(tel);
            //UserManager.Update(user);
            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            return RedirectToAction("Home");
        }

        //登录
        public ActionResult Login()
        {
            return View();
        }

       //注销
       public ActionResult LogOff()
        {
            return View();
        }

        // 主页
        public ActionResult Home()
        {
            return View();
        }


        //任务详情
        public ActionResult AssignmentDetail(int AssignmentId)
        {
            var assignment = _db.Assignment.SingleOrDefault(m => m.Id == AssignmentId && m.Status > AssignmentStatus.DELETED);
            return View(assignment);
        }
        //项目详情过程
        public ActionResult SubjectProcedure(int SubjectId)
        {
            var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId && m.Status > AssignmentStatus.DELETED);
            if (subject == null)
            {
                return View();
            }
            else
            {
                return View(subject);
            }
        }

        //获取项目
        [HttpPost]
        public JsonResult PersonalSubject()
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Json(new { result = "FAIL",errmsg="用户不存在。" });
            }
            else
            {
                try
                {
                    // 自己创建的项目
                    var ownSubject = employee.Subject;
                    var ownSubjectActive = ownSubject.Where(m => m.Status == SubjectStatus.ACTIVE);
                    var ownSubjectArchived = ownSubject.Where(m => m.Status == SubjectStatus.ARCHIVED);
                    // 自己参与任务的项目
                    var ColAssignmentSubject = (from m in employee.CollaborateAssignment
                                                where m.Status > AssignmentStatus.DELETED
                                                select m.Subject);
                    var ColAssignmentSubjectActive = ColAssignmentSubject.Where(p => p.Status == SubjectStatus.ACTIVE);
                    var ColAssignmentSubjectArchived = ColAssignmentSubject.Where(p => p.Status == SubjectStatus.ARCHIVED);
                    //获取负责任务的项目
                    var HolderSubject = (from m in _db.Assignment
                                         where m.HolderId == employee.Id && m.Status > AssignmentStatus.DELETED
                                         select m.Subject);
                    var HolderSubjectActive = HolderSubject.Where(p => p.Status == SubjectStatus.ACTIVE);
                    var HolderSubjectArchived = HolderSubject.Where(p => p.Status == SubjectStatus.ARCHIVED);
                    var FirstMergeAc = ownSubjectActive.Union(ColAssignmentSubjectActive);
                    var SubjectAc = from m in FirstMergeAc.Union(HolderSubjectActive).ToList()
                                         select new { id = m.Id, title = m.SubjectTitle, holderName = m.Holder.NickName,imgUrl=m.PicUrl,status=m.Status };
                    var FirstMergeAr = ownSubjectArchived.Union(ColAssignmentSubjectArchived);
                    var SubjectAr = from m in FirstMergeAr.Union(HolderSubjectArchived).ToList()
                                         select new { id = m.Id, title = m.SubjectTitle,holderName=m.Holder.NickName, imgUrl = m.PicUrl,status=m.Status };
                    var MergeSubjet = SubjectAc.Union(SubjectAr);
                    return Json(new { result = "SUCCESS", subject = MergeSubjet });
                }
                catch (Exception)
                {
                    return Json(new { result = "FAIL",errmsg="内部异常。"});
                }
            }
        }

        //获取任务
        [HttpPost]
        public JsonResult ProcedureAssigemnetAjax(int ProcedureId)
        {
            try
            {
                var assignmentlist = from m in _db.Assignment
                                     where m.ProcedureId == ProcedureId && m.Status >AssignmentStatus.DELETED
                                     select new { id = m.Id, title = m.AssignmentTitle, holderName = m.Holder.NickName, status = m.Status, subCount = m.SubTask.Count(), holderUrl = m.Holder.ImgUrl, deadTime = m.Deadline };
                return Json(new { result = "SUCCESS", data = assignmentlist });
            }
            catch (Exception)
            {
                return Json(new { result = "FAIL",errmsg="内部异常。"});
            }
        }


        //获取我的任务
        public JsonResult PersonalAssigemnetAjax(int datetype,int sorttype,int status)
        {
            return Json(new { });
        }

        //获取我的子任务
        public JsonResult PersonalSubtaskAjax(int datetype,int sorttype,int status)
        {
            return Json(new { });
        }

        public Employee getEmployee(string username)
        {
            var user = _db.Employee.SingleOrDefault(m => m.UserName == username);
            return user;
        }
    }
}