using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PeriodAid.Models;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using PeriodAid.DAL;

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

        #region 编辑项目
        public ActionResult EditSubject(int id)
        {
            var employee = getEmployee(User.Identity.Name);
            var subject = _db.Subject.SingleOrDefault(m => m.Id == id && m.HolderId == employee.Id);
            if(subject != null)
            {
                var EmployeeList = from m in _db.Employee
                                   where m.Status >= 0
                                   orderby m.Id descending
                                   select m;
                ViewBag.EmployeeDropDown = new SelectList(EmployeeList, "Id", "NickName", employee.Id);
                return PartialView(subject);
            }
            return Content("FAIL");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EditSubject(Subject model)
        {
            if (ModelState.IsValid)
            {
                var employee = getEmployee(User.Identity.Name);
                if (employee == null)
                {
                    return Content("职工不存在。");
                }
                else
                {
                    Subject item = new Subject();
                    if (TryUpdateModel(item))
                    {
                        _db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        _db.SaveChanges();
                        return Content("SUCCESS");
                    }
                    return Content("模型同步失败。");
                }
            }
            else
            {
                return Content("模型验证失败。");
            }
        }
        #endregion

        #region 上传封面

        #endregion

        #region  获取星标项目
        /// <summary>
        /// 获取星标任务
        /// </summary>
        /// <returns>集合</returns>
        public PartialViewResult Personal_StarSubjectListPartial()
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return PartialView("Error");
            }
            else
            {

                // 自己创建的项目
                var ownSubject = employee.Subject.Where(m => m.Status > 99999);
                // 自己的任务列表
                var SubjectList = from m in employee.CollaborateAssignment
                                  where m.Status > 9999
                                  select m.Subject;
                var MergeSubject = ownSubject.Union(SubjectList);

                return PartialView(MergeSubject);

            }
        }
        #endregion

        #region  获取行动中的项目
        /// <summary>
        /// 获取行动中的项目
        /// </summary>
        /// <returns>集合</returns>
        public ActionResult Personal_ActiveSubjectListPartial()
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return PartialView("Error");
            }
            else
            {

                // 自己创建的项目
                var ownSubject = employee.Subject.Where(m => m.Status > 0);
                // 自己的任务列表
                var SubjectList = from m in employee.CollaborateAssignment
                                     where m.Status > 0
                                     select m.Subject;            
                var MergeSubject = ownSubject.Union(SubjectList);

                return PartialView(MergeSubject);

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
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return PartialView("Error");
            }
            else
            {

                // 自己创建的项目
                var ownSubject = employee.Subject.Where(m => m.Status == 0);
                // 自己的任务列表
                var SubjectList = from m in employee.CollaborateAssignment
                                  where m.Status > 0
                                  select m.Subject;
                var FinsihSubjectList = SubjectList.Where(m => m.Status == 0);
                var MergeSubject = ownSubject.Union(FinsihSubjectList);

                return PartialView(MergeSubject);

            }
        }
        #endregion

        #region  获取项目实例
        /// <summary>
        /// 获取项目实例
        /// </summary>
        /// <param name="SubjectId"></param>
        /// <returns>项目实例</returns>
        public ActionResult Subject_Detail(int SubjectId)
        {
            var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId);
            var ProcedureIdList = from m in _db.Procedure
                                      where m.TemplateId == subject.TemplateId && m.Status > 0
                                      orderby m.Sort ascending
                                      select m.Id;
            ViewBag.pid = ProcedureIdList;
            return View(subject);
        }
        #endregion

        #region  获取项目的过程
        /// <summary>
        /// 获取项目的过程
        /// </summary>
        /// <param name="ProcedureId"></param>
        /// <returns>过程</returns>
        public PartialViewResult SubjectProcedure(int ProcedureId)
        {
            var procedure = _db.Procedure.SingleOrDefault(m=>m.Id==ProcedureId);
            return PartialView(procedure);

        }
        #endregion

        #region  获取任务填写表单
        /// <summary>
        /// 获取任务填写表单
        /// </summary>
        /// <param name="ProcedureId"></param>
        /// <param name="SubjectId"></param>
        /// <returns>PartialView</returns>
        public PartialViewResult GetAssignmentForm(int ProcedureId,int SubjectId)
        {
            var employee = getEmployee(User.Identity.Name);
            Assignment item = new Assignment(
             );
            item.ProcedureId = ProcedureId;
            item.SubjectId = SubjectId;
            var EmployeeList = from m in _db.Employee
                               where m.Status >= 0
                               orderby m.Id descending
                               select m;
            item.HolderId = employee.Id;
            ViewBag.EmployeeDropDown = new SelectList(EmployeeList, "Id", "NickName", employee.Id);
            return PartialView(item);
        }
        #endregion

        #region 项目过程中的任务
        /// <summary>
        /// 项目过程中的任务
        /// </summary>
        /// <param name="ProcedureId"></param>
        /// <returns>集合</returns>
        public PartialViewResult SubjectAssignment(int ProcedureId,int SubJectId)
        {
            var assignmentlist = from m in _db.Assignment
                                 where m.ProcedureId == ProcedureId && m.Status >= 0&&m.SubjectId== SubJectId
                                 orderby m.Status ascending
                                 select m;
            ViewBag.ProcedureId = ProcedureId;
            return PartialView(assignmentlist);
        }
        #endregion

        #region 创建任务列表框
        /// <summary>
        /// 创建任务列表框
        /// </summary>
        /// <param name="form"></param>
        /// <param name="model"></param>
        /// <returns>json</returns>
        [HttpPost,ValidateAntiForgeryToken]
        public JsonResult CreateProcedure(FormCollection form,Procedure model)
        {
            if (ModelState.IsValid)
            {
                Procedure item = new Procedure();
                if (TryUpdateModel(item))
                {
                    item.Status = 1;
                    try
                    {
                        _db.Procedure.Add(item);
                        _db.SaveChanges();
                    }
                    catch (Exception)
                    {
                        return Json(new { result = "数据存储失败。" });
                    }
                    return Json(new { result = "SUCCESS", id = item.Id });
                }
                else
                {
                    return Json(new { result = "模型同步错误。" });
                }
            }
            else
            {
                return Json(new { result = "模型错误。" });
            }
                    
        }
        #endregion

        #region 删除任务框
        /// <summary> 
        /// 删除任务框
        /// </summary>
        /// <param name="ProcedureId"></param>
        /// <returns>Content</returns>
        [HttpPost]
        public ActionResult DelectProcedure(int ProcedureId)
        {
            var procedure = _db.Procedure.SingleOrDefault(m => m.Id == ProcedureId);
            if (procedure == null)
            {
                return Content("操作的过程已不存在，请刷新页面!");
            }
            else
            {
                try
                {
                    _db.Procedure.Remove(procedure);
                    _db.SaveChanges();
                }
                catch (Exception)
                {
                    return Content("删除失败，请刷新页面重新尝试。");
                }
                return Content("SUCCESS");
            }
        }
        #endregion

        #region 创建任务
        /// <summary>
        /// 创建任务
        /// </summary>
        /// <param name="form"></param>
        /// <param name="model"></param>
        /// <returns>Content</returns>
        [HttpPost,ValidateAntiForgeryToken]
        public JsonResult CreateAssignment(FormCollection form,Assignment model)
        {
            if (ModelState.IsValid)
            {
                Assignment item = new Assignment();
                if (TryUpdateModel(item))
                {
                    item.Status = 1;
                    item.CreateTime = DateTime.Now;
                    try
                    {
                        _db.Assignment.Add(item);
                        _db.SaveChanges();
                    }
                    catch (Exception)
                    {
                        return Json(new { result = "数据存储失败。" });
                    }
                    return Json(new { result = "SUCCESS", id = item.ProcedureId });

                }
                else
                {
                    return Json(new { result = "模型同步错误。" });
                }
            }
            else
            {
                return Json(new { result = "模型错误。" });
            }
        }
        #endregion


        //任务完成状态回传
        [HttpPost]
        public JsonResult ComfirmFinishAssignment(int AssignmentId)
        {
            var assignment = _db.Assignment.SingleOrDefault(m => m.Id == AssignmentId);
            if (assignment.Status == AssignmentStatus.UNFINISHED)
            {
                assignment.Status = AssignmentStatus.FINISHED;
            }
            else
            {
                assignment.Status = AssignmentStatus.UNFINISHED;
            }
            try
            {
                _db.Entry(assignment).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
            }
            catch (Exception)
            {
                return Json(new { result = "操作失败"});
            }
            return Json(new { result = "操作成功",Id=assignment.ProcedureId });
        }
        //获取任务详情
        public PartialViewResult Assignment_Detail(int AssignmentId)
        {
            var assignment = _db.Assignment.SingleOrDefault(m => m.Id == AssignmentId);
            var EmployeeList = from m in _db.Employee
                               where m.Status > -1
                               select m;
            ViewBag.EmployeeDropDown = new SelectList(EmployeeList, "Id", "NickName", assignment.HolderId);
            return PartialView(assignment);
        }
        //任务修改
        [HttpPost,ValidateAntiForgeryToken]
        public JsonResult Edit_Assignment_Detail(FormCollection form,Assignment model)
        {
            if (ModelState.IsValid)
            {
                Assignment item = new Assignment();
                if (TryUpdateModel(item))
                {
                    try
                    {
                        _db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        _db.SaveChanges();
                    }
                    catch (Exception)
                    {
                        return Json(new { result = "数据存储失败。" });
                    }
                    return Json(new { result = "SUCCESS", id = item.ProcedureId });
                }
                else
                {
                    return Json(new { result = "模型同步错误。" });
                }
            }
            else
            {
                return Json(new { result = "模型错误。" });
            }
        }



















        public Employee getEmployee(string username)
        {
            var user = _db.Employee.SingleOrDefault(m => m.UserName == username);
            return user;
        }

        [HttpPost]
        public ActionResult UploadSubjectCoverFileAjax(FormCollection form)
        {
            var files = Request.Files;
            string msg = string.Empty;
            string error = string.Empty;
            string imgurl;
            if (files.Count > 0)
            {
                if (files[0].ContentLength > 0 && files[0].ContentType.Contains("image"))
                {
                    string filename = DateTime.Now.ToFileTime().ToString() + ".jpg";
                    //files[0].SaveAs(Server.MapPath("/Content/checkin-img/") + filename);
                    AliOSSUtilities util = new AliOSSUtilities();
                    util.PutObject(files[0].InputStream, "Subject/SubjectCover/" + filename);
                    msg = "成功! 文件大小为:" + files[0].ContentLength;
                    imgurl = filename;
                    string res = "{ error:'" + error + "', msg:'" + msg + "',imgurl:'Subject/SubjectCover/" + imgurl + "'}";
                    return Content(res);
                }
                else
                {
                    error = "文件错误";
                }
            }
            string err_res = "{ error:'" + error + "', msg:'" + msg + "',imgurl:''}";
            return Content(err_res);
        }

    }
}