using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PeriodAid.Models;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace PeriodAid.Controllers
{
    [Authorize]
    public class TaskManagementController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ProjectSchemeModels _db;
        public TaskManagementController()
        {
            _db = new ProjectSchemeModels();
        }
        public TaskManagementController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        public async Task<ActionResult> TestLogin()
        {
            var user = UserManager.FindByName("13916209621");
            //UserManager.Update(user);
            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            return RedirectToAction("Index");
        }
        // GET: TaskManagement
        public ActionResult Index()
        {
            var a = User.Identity.Name;
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return View("Error");
            }
            else
            {
                return View();
            }
        }
        #region  创建项目
        /// <summary>
        /// 创建项目
        /// </summary>
        /// <returns>ActionResult</returns>
        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult CreateSubject(FormCollection form,Subject model)
        {
            if (ModelState.IsValid)
            {
                Subject item = new Subject();
                if (TryUpdateModel(item))
                {
                    var employee = getEmployee(User.Identity.Name);
                    if (employee == null)
                    {
                        return Content("职工不存在。");                 
                    }
                    else
                    {

                        item.HolderId = employee.Id;
                        item.Status = 1;
                        if (item.TemplateId == 0)
                        {
                            item.TemplateId = 1;
                        }
                        item.CreateTime = DateTime.Now;
                        try
                        {
                            _db.Subject.Add(item);
                            _db.SaveChanges();
                        }
                        catch (Exception)
                        {

                            return Content("数据存储失败。");
                        }
                        return Content("SUCCESS");
                    }
                }
                else
                {

                    return Content("模型同步失败。");
                }
            }
            else
            {

                return Content("模型验证失败。");
            }
        }
        #endregion


        #region  获取星标项目
        /// <summary>
        /// 获取星标任务
        /// </summary>
        /// <returns>集合</returns>
        public PartialViewResult Personal_StarSubjectListPartial()
        {
            return PartialView();
        }
        #endregion


        #region  获取行动中的项目
        /// <summary>
        /// 获取行动中的项目
        /// </summary>
        /// <returns>集合</returns>
        public PartialViewResult Personal_ActiveSubjectListPartial()
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return PartialView("Error");
            }
            else
            {
                var assignlist = from m in _db.Assignment
                                 where m.HolderId == employee.Id &&m.Status!=-1
                                 select m.SubjectId;
                var subtask = from m in _db.SubTask
                              where m.ExecutorId == employee.Id && m.Status != -1
                              select m.Assignment.SubjectId;
            }
        }
        #endregion

        #region 获取已完成项目列表
        /// <summary>
        /// 获取已完成项目列表
        /// </summary>
        /// <returns>集合</returns>
        public PartialViewResult Personal_FinishSubjectListPartial()
        {
            return PartialView();
        }
        #endregion


        //项目详情
        public ActionResult Subject_Detail(string SubjectId)
        {
            return View();
        }

        public Employee getEmployee(string username)
        {
            var user = _db.Employee.SingleOrDefault(m => m.UserName == username);
            return user;
        }
    }
}