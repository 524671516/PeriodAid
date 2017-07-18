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
using Newtonsoft.Json;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using PagedList;

namespace PeriodAid.Controllers
{
    [Authorize]
    public class TaskManagementController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ProjectSchemeModels _db;
        private TaskManagementLogsUtilities _log;
        public TaskManagementController()
        {
            _db = new ProjectSchemeModels();
            _log = new TaskManagementLogsUtilities();
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
        public async Task<ActionResult> TestLogin(string tel)
        {
            var user = UserManager.FindByName(tel);
            //UserManager.Update(user);
            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            return RedirectToAction("Index");
        }

        // GET: TaskManagement
        public async Task<ActionResult> Index()
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                //return RedirectToAction("TaskManagement", "TaskManagerLogin");
                return PartialView("EmployeeError");
            }
            else
            {
                /* 更新最后刷新时间 */
                employee.LastLoginDate = DateTime.Now;
                _db.Entry(employee).State = System.Data.Entity.EntityState.Modified;
                await _db.SaveChangesAsync();

                return View(employee);
            }
        }

        /* 修改个人信息 */
        public ActionResult PersonalSetting()
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return View("Error");
            }
            else
            {
                return View(employee);
            }
        }


        //修改个人信息
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> EditPersonalInfo(Employee model)
        {
            if (ModelState.IsValid)
            {
                Employee item = new Employee();
                if (TryUpdateModel(item))
                {
                    try
                    {
                        _db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        await _db.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        return Json(new { result = "数据存储失败。" });
                    }
                    return Json(new { result = "SUCCESS" });
                }
                return Json(new { result = "模型同步错误。" });
            }
            return Json(new { result = "模型错误。" });
        }

        #region  创建项目
        /// <summary>
        /// 创建项目
        /// </summary>
        /// <returns>ActionResult</returns>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateSubject(FormCollection form, Subject model)
        {
            if (ModelState.IsValid)
            {
                Subject item = new Subject();
                if (TryUpdateModel(item))
                {
                    var employee = getEmployee(User.Identity.Name);
                    if (employee == null)
                    {
                        return Json(new { result = "FAIL", errmsg = "模型错误。" });
                    }
                    else
                    {
                        item.HolderId = employee.Id;
                        item.Status = SubjectStatus.ACTIVE;
                        var ItemTemplate = _db.ProcedureTemplate.SingleOrDefault(m => m.Id == item.TemplateId);
                        if (ItemTemplate == null)
                        {
                            ItemTemplate = _db.ProcedureTemplate.SingleOrDefault(m => m.Default == true);
                        }
                        ProcedureTemplate template = new ProcedureTemplate()
                        {
                            Title = ItemTemplate.Title
                        };
                        try
                        {
                            _db.ProcedureTemplate.Add(template);
                            _db.SaveChanges();
                            foreach (var pro in ItemTemplate.Procedure)
                            {
                                Procedure procedure = new Procedure()
                                {
                                    TemplateId = template.Id,
                                    Sort = pro.Sort,
                                    Status = pro.Status,
                                    ProcedureTitle = pro.ProcedureTitle
                                };
                                _db.Procedure.Add(procedure);
                                await _db.SaveChangesAsync();
                            }
                        }
                        catch (Exception)
                        {
                            return Json(new { result = "FAIL", errmsg = "项目模板建立失败。" });
                        }
                        item.TemplateId = template.Id;
                        item.CreateTime = DateTime.Now;
                        try
                        {
                            _db.Subject.Add(item);
                            await _db.SaveChangesAsync();
                            await AddLogAsync(LogCode.CREATESUBJECT, employee, item.Id, "");
                        }
                        catch (Exception)
                        {
                            return Json(new { result = "FAIL", errmsg = "数据存储失败。" });
                        }
                        return Json(new { result = "SUCCESS", errmsg = "" });
                    }
                }
                else
                {
                    return Json(new { result = "FAIL", errmsg = "模型同步失败。" });
                }
            }
            else
            {
                return Json(new { result = "FAIL", errmsg = "模型验证失败。" });
            }
        }
        #endregion

        #region  获取创建项目表单 
        /// <summary>
        /// 获取创建项目表单
        /// </summary>
        /// <returns>partialview</returns>
        public PartialViewResult GetSubjectForm()
        {
            Subject model = new Subject()
            {
                TemplateId = 1,
            };
            return PartialView(model);
        }
        #endregion

        #region 编辑项目和提交
        public ActionResult EditSubject(int id)
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Content("FAIL");
            }
            else
            {
                var subject = _db.Subject.SingleOrDefault(m => m.Id == id && m.HolderId == employee.Id);
                if (subject != null)
                {
                    var EmployeeList = from m in _db.Employee
                                       where m.Status == EmployeeStatus.NORMAL
                                       orderby m.Id descending
                                       select m;
                    ViewBag.EmployeeDropDown = new SelectList(EmployeeList, "Id", "NickName", employee.Id);
                    return PartialView(subject);
                }
                return Content("FAIL");
            }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> EditSubject(Subject model)
        {
            if (ModelState.IsValid)
            {
                var employee = getEmployee(User.Identity.Name);
                if (employee == null)
                {
                    return Json(new { result = "FAIL", errmsg = "职工不存在。" });
                }
                else
                {
                    Subject item = new Subject();
                    if (TryUpdateModel(item))
                    {
                        _db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        await _db.SaveChangesAsync();
                        await AddLogAsync(LogCode.EDITSUBJECT, employee, item.Id, "");
                        return Json(new { result = "SUCCESS", errmsg = "" });
                    }
                    return Json(new { result = "FAIL", errmsg = "模型同步失败。" });
                }
            }
            else
            {
                return Json(new { result = "FAIL", errmsg = "模型验证失败。" });
            }
        }
        #endregion

        #region 归档项目
        [HttpPost]
        public async Task<JsonResult> ArchiveSubject(int id)
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Json(new { result = "FAIL", errmsg = "员工不存在" });
            }
            else
            {
                var subject = _db.Subject.SingleOrDefault(m => m.Id == id && m.HolderId == employee.Id);
                if (subject != null)
                {
                    subject.Status = SubjectStatus.ARCHIVED;
                    _db.Entry(subject).State = System.Data.Entity.EntityState.Modified;
                    await _db.SaveChangesAsync();
                    await AddLogAsync(LogCode.ARCHIVESUBJECT, employee, subject.Id, "");
                    return Json(new { result = "SUCCESS", errmsg = "归档项目成功" });
                }
                return Json(new { result = "FAIL", errmsg = "当前用户没有权限归档项目。" });
            }
        }
        #endregion

        #region 删除项目
        [HttpPost]
        public async Task<JsonResult> DeleteSubject(int id)
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Json(new { result = "FAIL", errmsg = "员工不存在" });
            }
            else
            {
                var subject = _db.Subject.SingleOrDefault(m => m.Id == id && m.HolderId == employee.Id);
                if (subject != null)
                {
                    subject.Status = SubjectStatus.DELETED;
                    _db.Entry(subject).State = System.Data.Entity.EntityState.Modified;
                    await _db.SaveChangesAsync();
                    await AddLogAsync(LogCode.DELETESUBJECT, employee, subject.Id, "");
                    return Json(new { result = "SUCCESS", errmsg = "删除项目成功" });
                }
                return Json(new { result = "FAIL", errmsg = "当前用户没有权限删除项目" });
            }
        }
        #endregion

        #region 重置项目
        [HttpPost]
        public async Task<JsonResult> ResetSubject(int id)
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Json(new { result = "FAIL", errmsg = "员工不存在" });
            }
            else
            {
                var subject = _db.Subject.SingleOrDefault(m => m.Id == id && m.HolderId == employee.Id);
                if (subject != null)
                {
                    subject.Status = SubjectStatus.ACTIVE;
                    _db.Entry(subject).State = System.Data.Entity.EntityState.Modified;
                    await _db.SaveChangesAsync();
                    string info = "恢复了项目:" + subject.SubjectTitle + "。";
                    await AddLogAsync(LogCode.RESETSUBJECT, employee, subject.Id, info);
                    return Json(new { result = "SUCCESS", errmsg = "恢复项目成功" });
                }
                return Json(new { result = "FAIL", errmsg = "当前用户没有权限恢复项目" });
            }
        }
        #endregion

        #region 上传封面
        [HttpPost]
        public ActionResult UploadSubjectCoverFileAjax(FormCollection form)
        {
            var files = Request.Files;
            string errmsg = string.Empty;
            string error = string.Empty;
            string imgurl;

            if (files.Count > 0)
            {
                if (files[0].ContentLength > 0 && files[0].ContentType.Contains("image"))
                {
                    string filename = DateTime.Now.ToFileTime().ToString() + ".jpg";
                    Stream stream = files[0].InputStream;
                    //files[0].SaveAs(Server.MapPath("/Content/checkin-img/") + filename);
                    AliOSSUtilities util = new AliOSSUtilities();
                    util.PutObject(files[0].InputStream, "Subject/SubjectCover/" + filename);
                    errmsg = "成功! 文件大小为:" + files[0].ContentLength;
                    imgurl = filename;
                    string res = "{ error:'" + error + "', errmsg:'" + errmsg + "',imgurl:'Subject/SubjectCover/" + imgurl + "'}";
                    return Content(res);
                }
                else
                {
                    error = "文件错误";
                }
            }
            string err_res = "{ error:'" + error + "', errmsg:'" + errmsg + "',imgurl:''}";
            return Content(err_res);
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
                return Content("FAIL");
            }
            else
            {

                // 自己创建的项目
                var ownSubject = employee.Subject.Where(m => m.Status == SubjectStatus.ACTIVE);
                // 自己参与任务的项目
                var ColAssignmentSubject = (from m in employee.CollaborateAssignment
                                            where m.Status > AssignmentStatus.DELETED
                                            select m.Subject).Where(p => p.Status == SubjectStatus.ACTIVE);
                //获取负责任务的项目
                var HolderSubject = (from m in _db.Assignment
                                     where m.HolderId == employee.Id && m.Status > AssignmentStatus.DELETED
                                     select m.Subject).Where(p => p.Status == SubjectStatus.ACTIVE);
                var FirstMerge = ownSubject.Union(ColAssignmentSubject);
                var MergeSubject = FirstMerge.Union(HolderSubject);
                return PartialView(MergeSubject);
            }
        }
        #endregion

        #region 获取已完成项目列表
        /// <summary>
        /// 获取已完成项目列表
        /// </summary>
        /// <returns>集合</returns>
        public ActionResult Personal_FinishSubjectListPartial()
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Content("FAIL");
            }
            else
            {

                // 自己创建的项目
                var ownSubject = employee.Subject.Where(m => m.Status == SubjectStatus.ARCHIVED);
                // 自己参与任务的项目
                var ColAssignmentSubject = (from m in employee.CollaborateAssignment
                                            where m.Status > AssignmentStatus.DELETED
                                            select m.Subject).Where(p => p.Status == SubjectStatus.ARCHIVED);
                //自己负责任务的项目
                var HolderSubject = (from m in _db.Assignment
                                     where m.HolderId == employee.Id && m.Status > AssignmentStatus.DELETED
                                     select m.Subject).Where(p => p.Status == SubjectStatus.ARCHIVED);
                var FirstMerge = ownSubject.Union(ColAssignmentSubject);
                var MergeSubject = FirstMerge.Union(HolderSubject);

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
            if (subject.Status == SubjectStatus.ARCHIVED || subject.Status == SubjectStatus.DELETED)
            {
                return View("Error");
            }
            else
            {
                ViewBag.sortprocedure = from m in subject.ProcedureTemplate.Procedure
                                        where m.Status == ProcedureStatus.NORMAL
                                        orderby m.Sort ascending
                                        select m;
                ViewBag.img = getEmployee(User.Identity.Name).ImgUrl;
                return View(subject);
            }
        }
        #endregion

        #region  获取项目的过程
        /// <summary>
        /// 获取项目的过程
        /// </summary>
        /// <param name="ProcedureId"></param>
        /// <returns>过程</returns>
        public ActionResult SubjectProcedure(int ProcedureId)
        {
            var procedure = _db.Procedure.SingleOrDefault(m => m.Id == ProcedureId && m.Status == ProcedureStatus.NORMAL);
            if (procedure != null)
            {
                return PartialView(procedure);
            }
            return Content("FAIL");
        }
        #endregion

        #region 创建任务列表框
        /// <summary>
        /// 创建任务列表框
        /// </summary>
        /// <param name="form"></param>
        /// <param name="model"></param>
        /// <returns>json</returns>
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult CreateProcedure(FormCollection form, Procedure model)
        {
            if (ModelState.IsValid)
            {
                Procedure item = new Procedure();
                if (TryUpdateModel(item))
                {
                    var employee = getEmployee(User.Identity.Name);
                    if (employee.Subject.Select(p => p.TemplateId).Contains(item.TemplateId))
                    {
                        var template = _db.ProcedureTemplate.SingleOrDefault(m => m.Id == item.TemplateId);
                        var maxsort = template.Procedure.Max(m => m.Sort);
                        item.Sort = maxsort + 1;
                        item.Status = ProcedureStatus.NORMAL;
                        try
                        {
                            _db.Procedure.Add(item);
                            _db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            return Json(new { result = "FAIL", errmsg = "数据存储失败。" });
                        }
                        return Json(new { result = "SUCCESS", id = item.Id });
                    }
                    else
                    {
                        return Json(new { result = "FAIL", errmsg = "请联系项目管理员添加。" });
                    }
                }
                else
                {
                    return Json(new { result = "FAIL", errmsg = "模型同步错误。" });
                }
            }
            else
            {
                return Json(new { result = "FAIL", errmsg = "模型错误。" });
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
        public JsonResult Delete_Procedure(int ProcedureId)
        {
            var procedure = _db.Procedure.SingleOrDefault(m => m.Id == ProcedureId && m.Status == ProcedureStatus.NORMAL);
            var employee = getEmployee(User.Identity.Name);
            if (employee.Subject.Select(p => p.TemplateId).Contains(procedure.TemplateId))
            {
                var assignmentlist = (from m in _db.Assignment
                                      where m.ProcedureId == ProcedureId && m.Status > AssignmentStatus.DELETED
                                      select m).Count();
                if (assignmentlist == 0)
                {
                    try
                    {
                        procedure.Status = ProcedureStatus.REMOVED;
                        _db.Entry(procedure).State = System.Data.Entity.EntityState.Modified;
                        _db.SaveChanges();
                    }
                    catch (Exception)
                    {
                        return Json(new { result = "FAIL", errmsg = "删除过程失败。" });
                    }
                    return Json(new { result = "SUCCESS", Id = procedure.Id });
                }
                else
                {
                    return Json(new { result = "FAIL", errmsg = "请先清空列表中的任务。" });
                }
            }
            else
            {
                return Json(new { result = "FAIL", errmsg = "你没有权限添加任务列表。" });
            }
        }
        #endregion

        #region  创建任务
        /// <summary>
        ///创建任务
        /// </summary>
        /// <param name="ProcedureId"></param>
        /// <param name="SubjectId"></param>
        /// <returns>PartialView</returns>
        public PartialViewResult GetAssignmentForm(int ProcedureId, int SubjectId)
        {
            var employee = getEmployee(User.Identity.Name);
            Assignment item = new Assignment()
            {
                ProcedureId = ProcedureId,
                SubjectId = SubjectId,
                HolderId = employee.Id
            };
            var EmployeeList = from m in _db.Employee
                               where m.Status > EmployeeStatus.DEVOICE
                               orderby m.Id descending
                               select m;
            ViewBag.EmployeeDropDown = new SelectList(EmployeeList, "Id", "NickName", employee.Id);
            return PartialView(item);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateAssignment(FormCollection form, Assignment model)
        {
            var employee = getEmployee(User.Identity.Name);
            if (ModelState.IsValid)
            {
                Assignment item = new Assignment();
                if (TryUpdateModel(item))
                {
                    item.Status = AssignmentStatus.UNFINISHED;
                    item.CreateTime = DateTime.Now;
                    try
                    {
                        _db.Assignment.Add(item);
                        await _db.SaveChangesAsync();
                        await AddLogAsync(LogCode.CREATETASK, employee, item.SubjectId, "创建了项目:" + item.AssignmentTitle + "。");
                    }
                    catch (Exception)
                    {
                        return Json(new { result = "FAIL", errmsg = "数据存储失败。" });
                    }
                    return Json(new { result = "SUCCESS", id = item.ProcedureId });

                }
                else
                {
                    return Json(new { result = "FAIL", errmsg = "模型同步错误。" });
                }
            }
            else
            {
                return Json(new { result = "FAIL", errmsg = "模型错误。" });
            }
        }
        #endregion

        #region 获取任务列表
        /// <summary>
        /// 获取任务列表
        /// </summary>
        /// <param name="ProcedureId"></param>
        /// <returns>集合</returns>
        public PartialViewResult SubjectAssignment(int ProcedureId, int SubJectId)
        {
            var assignmentlist = from m in _db.Assignment
                                 where m.ProcedureId == ProcedureId && m.SubjectId == SubJectId && m.Status > AssignmentStatus.DELETED
                                 orderby m.Status ascending
                                 select m;
            ViewBag.ProcedureId = ProcedureId;
            return PartialView(assignmentlist);
        }
        #endregion

        //任务修改
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> Edit_Assignment_Detail(FormCollection form, Assignment model)
        {
            var employee = getEmployee(User.Identity.Name);
            if (ModelState.IsValid)
            {
                var item = _db.Assignment.SingleOrDefault(m => m.Id == model.Id);
                if (employee.Subject.Contains(item.Subject) || item.Holder == employee)
                {
                    int oldholderid = item.HolderId;
                    if (TryUpdateModel(item))
                    {

                        if (item.HolderId != oldholderid)
                        {
                            var subtasklist = from m in _db.SubTask
                                              where m.Status > AssignmentStatus.DELETED && m.ExecutorId == oldholderid && m.AssignmentId == item.Id
                                              select m.ExecutorId;
                            var col = _db.Employee.SingleOrDefault(m => m.Id == oldholderid);
                            if (item.Collaborator.Select(p => p.Id).Contains(item.HolderId))
                            {
                                var newholder = _db.Employee.SingleOrDefault(m => m.Id == item.HolderId);
                                item.Collaborator.Remove(newholder);
                            }
                            if (subtasklist.Contains(oldholderid))
                            {
                                item.Collaborator.Add(col);
                            }
                        }
                        try
                        {
                            _db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                            await _db.SaveChangesAsync();
                            await AddLogAsync(LogCode.EDITTASK, employee, item.SubjectId, "修改了任务:" + item.AssignmentTitle + "。");
                        }
                        catch (Exception)
                        {
                            return Json(new { result = "FAIL", errmsg = "数据存储失败。" });
                        }
                        return Json(new { result = "SUCCESS", id = item.ProcedureId });
                    }
                    else
                    {
                        return Json(new { result = "FAIL", errmsg = "模型同步错误。" });
                    }
                }
                else
                {
                    return Json(new { result = "FAIL", errmsg = "你没有权限修改此任务。" });
                }
            }
            else
            {
                return Json(new { result = "FAIL", errmsg = "模型错误。" });
            }
        }


        //任务完成状态回传
        [HttpPost]
        public async Task<JsonResult> ComfirmFinishAssignment(int AssignmentId)
        {
            var employee = getEmployee(User.Identity.Name);
            var assignment = _db.Assignment.SingleOrDefault(m => m.Id == AssignmentId && m.Status > AssignmentStatus.DELETED);
            if (assignment == null)
            {
                return Json(new { result = "FAIL", errmsg = "任务已被移除请刷新页面。" });
            }
            else
            {
                var subtasklist = assignment.SubTask;
                foreach (var sub in subtasklist)
                {
                    if (sub.Status == AssignmentStatus.UNFINISHED)
                    {
                        return Json(new { result = "FAIL", errmsg = "请先完成子任务。" });
                    }
                }
                if (assignment.Status == AssignmentStatus.UNFINISHED)
                {
                    assignment.Status = AssignmentStatus.FINISHED;
                    assignment.CompleteDate = DateTime.Now;
                    await AddLogAsync(LogCode.EDITTASK, employee, assignment.SubjectId, "确认完成了任务:" + assignment.AssignmentTitle + "。");
                }
                else
                {
                    assignment.Status = AssignmentStatus.UNFINISHED;
                    assignment.CompleteDate = null;
                    await AddLogAsync(LogCode.EDITTASK, employee, assignment.SubjectId, "修改了任务:" + assignment.AssignmentTitle + "。");
                }
                try
                {
                    _db.Entry(assignment).State = System.Data.Entity.EntityState.Modified;
                    await _db.SaveChangesAsync();

                }
                catch (Exception)
                {
                    return Json(new { result = "FAIL", errmsg = "数据存储失败。" });
                }
                return Json(new { result = "SUCCESS", errmsg = "", Id = assignment.ProcedureId });
            }
        }


        //获取任务详情
        public ActionResult Assignment_Detail(int AssignmentId)
        {
            var assignment = _db.Assignment.SingleOrDefault(m => m.Id == AssignmentId && m.Status > AssignmentStatus.DELETED);
            if (assignment == null)
            {
                return Content("FAIL");
            }
            else
            {
                var EmployeeList = from m in _db.Employee
                                   where m.Status > EmployeeStatus.DEVOICE
                                   select m;
                ViewBag.EmployeeDropDown = new SelectList(EmployeeList, "Id", "NickName", assignment.HolderId);
                return PartialView(assignment);
            }
        }

        //删除任务
        [HttpPost]
        public async Task<JsonResult> Delete_Assignment(int AssignmentId)
        {
            var employee = getEmployee(User.Identity.Name);
            var assignment = _db.Assignment.SingleOrDefault(m => m.Id == AssignmentId && m.Status > AssignmentStatus.DELETED);
            if (assignment == null)
            {
                return Json(new { result = "FAIL", errmsg = "此任务已不存在。" });
            }
            else
            {
                if (employee.Subject.Contains(assignment.Subject) || assignment.Holder == employee)
                {
                    var subtasklist = from m in _db.SubTask
                                      where m.AssignmentId == assignment.Id && m.Status > AssignmentStatus.DELETED
                                      select m;
                    if (subtasklist.Count() != 0)
                    {
                        foreach (var item in subtasklist)
                        {
                            try
                            {
                                item.Status = AssignmentStatus.DELETED;
                                _db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                            }
                            catch (Exception)
                            {
                                return Json(new { result = "FAIL", errmsg = "发生错误。" });
                            }
                        }
                        await _db.SaveChangesAsync();
                    }
                    try
                    {
                        assignment.Status = AssignmentStatus.DELETED;
                        _db.Entry(assignment).State = System.Data.Entity.EntityState.Modified;
                        await _db.SaveChangesAsync();
                        await AddLogAsync(LogCode.EDITTASK, employee, assignment.SubjectId, "删除了任务:" + assignment.AssignmentTitle + "。");
                    }
                    catch (Exception)
                    {
                        return Json(new { result = "FAIL", errmsg = "保存数据失败。" });
                    }
                    return Json(new { result = "SUCCESS", Id = assignment.ProcedureId });
                }
                else
                {
                    return Json(new { result = "FAIL", errmsg = "你没有权限删除此任务。" });
                }
            }
        }

        //获取任务的参与者模板
        public PartialViewResult Assignment_CollaboratorPartial(int AssignmentId)
        {
            var assignment = _db.Assignment.SingleOrDefault(m => m.Id == AssignmentId && m.Status > AssignmentStatus.DELETED);
            return PartialView(assignment);
        }

        //添加参与者模板
        public PartialViewResult Assignmnet_CollaboratorAddPartial(int AssignmentId)
        {
            var assignment = _db.Assignment.SingleOrDefault(m => m.Id == AssignmentId);
            List<CollaboratorModel> collist = new List<CollaboratorModel>();
            List<Employee> existem = new List<Employee>();
            existem.Add(assignment.Holder);
            foreach (var col in assignment.Collaborator)
            {
                existem.Add(col);
            }
            var departemt = from m in _db.Department
                            where m.Status == DepartmentStatus.NORMAL
                            select m;
            foreach (var depart in departemt)
            {
                List<Employee> newlist = new List<Employee>();
                foreach (var item in depart.Employee)
                {
                    if (!existem.Contains(item))
                    {
                        newlist.Add(item);
                    }
                }
                CollaboratorModel colmodel = new CollaboratorModel()
                {
                    DepartmentName = depart.DepartmentName,
                    EmployeeList = newlist
                };
                collist.Add(colmodel);
            }
            ViewBag.DepartmentList = collist;
            return PartialView(assignment);
        }

        //获取子任务模板
        public PartialViewResult Assignment_SubtaskPartial(int AssignmentId)
        {
            var subtasklist = from m in _db.SubTask
                              where m.AssignmentId == AssignmentId && m.Status > AssignmentStatus.DELETED
                              orderby m.Status
                              select m;
            return PartialView(subtasklist);
        }

        //获取子任务表单
        public ActionResult GetSubtaskForm(int AssignmentId)
        {
            var assignment = _db.Assignment.SingleOrDefault(m => m.Id == AssignmentId && m.Status > AssignmentStatus.DELETED);
            if (assignment == null)
            {
                return Content("FAIL");
            }
            var collaborator = assignment.Collaborator;
            List<Employee> emlist = new List<Employee>();
            emlist.Add(assignment.Holder);
            foreach (var i in collaborator)
            {
                emlist.Add(i);
            }

            SubTask model = new SubTask()
            {
                AssignmentId = AssignmentId
            };
            ViewBag.EmployeeDropDown = new SelectList(emlist, "Id", "NickName", assignment.HolderId);
            return PartialView(model);
        }


        //获取子任务表单已有内容
        public PartialViewResult GetSubtaskFilledForm(int AssignmentId, int SubtaskId)
        {
            var assignment = _db.Assignment.SingleOrDefault(m => m.Id == AssignmentId && m.Status > AssignmentStatus.DELETED);
            if (assignment == null)
            {
                return PartialView("Error");
            }
            var collaborator = assignment.Collaborator;
            List<Employee> emlist = new List<Employee>();
            emlist.Add(assignment.Holder);
            foreach (var i in collaborator)
            {
                emlist.Add(i);
            }

            var subtask = _db.SubTask.SingleOrDefault(m => m.Id == SubtaskId);
            ViewBag.EmployeeDropDown = new SelectList(emlist, "Id", "NickName", subtask.ExecutorId);
            return PartialView(subtask);
        }

        //创建子任务
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult CreateSubtask(SubTask model)
        {
            if (ModelState.IsValid)
            {
                SubTask item = new SubTask();
                if (TryUpdateModel(item))
                {
                    item.CreateTime = DateTime.Now;
                    item.Status = AssignmentStatus.UNFINISHED;
                    try
                    {
                        _db.SubTask.Add(item);
                        _db.SaveChanges();
                    }
                    catch (Exception)
                    {
                        return Json(new { result = "存储失败。" });
                    }
                    var ProcedureId = _db.Assignment.SingleOrDefault(m => m.Id == item.AssignmentId).ProcedureId;
                    return Json(new { result = "SUCCESS", Id = ProcedureId });

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

        //修改子任务
        [HttpPost]
        public JsonResult EditSubtask(SubTask model)
        {
            if (ModelState.IsValid)
            {
                SubTask item = new SubTask();
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
                    return Json(new { result = "SUCCESS", id = item.AssignmentId });
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

        //子任务详情
        public PartialViewResult Subtask_Detail(int SubtaskId)
        {
            var subtask = _db.SubTask.SingleOrDefault(m => m.Id == SubtaskId && m.Status > AssignmentStatus.DELETED);
            List<Employee> emlist = new List<Employee>();
            foreach (var item in subtask.Assignment.Collaborator)
            {
                emlist.Add(item);
            };
            emlist.Add(subtask.Assignment.Holder);
            ViewBag.EmployeeDropDown = new SelectList(emlist, "Id", "NickName", subtask.ExecutorId);
            return PartialView(subtask);
        }

        //删除子任务
        [HttpPost]
        public JsonResult Delete_Subtask(int SubtaskId)
        {
            var subtask = _db.SubTask.SingleOrDefault(m => m.Id == SubtaskId && m.Status > AssignmentStatus.DELETED);
            if (subtask == null)
            {
                return Json(new { result = "此任务已不存在。" });
            }
            else
            {
                try
                {
                    subtask.Status = AssignmentStatus.DELETED;
                    _db.Entry(subtask).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                }
                catch (Exception)
                {
                    return Json(new { result = "发生错误。" });
                }
                return Json(new { result = "SUCCESS", Id = subtask.Assignment.ProcedureId });
            }
        }

        //子任务完成状态回传
        [HttpPost]
        public JsonResult ComfirmFinishSubtask(int SubtaskId)
        {
            var subtask = _db.SubTask.SingleOrDefault(m => m.Id == SubtaskId && m.Status > AssignmentStatus.DELETED);

            if (subtask.Status == AssignmentStatus.UNFINISHED)
            {
                subtask.Status = AssignmentStatus.FINISHED;
                subtask.CompleteDate = DateTime.Now;
            }
            else
            {
                subtask.Status = AssignmentStatus.UNFINISHED;
                subtask.CompleteDate = null;
            }
            try
            {
                _db.Entry(subtask).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
            }
            catch (Exception)
            {
                return Json(new { result = "操作失败" });
            }
            return Json(new { result = "SUCCESS", Id = subtask.Assignment.ProcedureId });
        }

        //添加参与人
        [HttpPost]
        public JsonResult AddAssignmentCollaborator(int AssignmentId, string colvalue)
        {
            var assignment = _db.Assignment.SingleOrDefault(m => m.Id == AssignmentId && m.Status > AssignmentStatus.DELETED);
            if (assignment == null)
            {
                return Json(new { result = "FAIL", errmsg = "此任务已不存在。" });
            }
            else
            {
                var colarray = colvalue.Split(',');
                foreach (var i in colarray)
                {
                    try
                    {
                        var ci = Convert.ToInt32(i);
                        var col = _db.Employee.SingleOrDefault(m => m.Id == ci && m.Status > EmployeeStatus.DEVOICE);
                        if (!assignment.Collaborator.Contains(col))
                        {
                            assignment.Collaborator.Add(col);
                            _db.Entry(assignment).State = System.Data.Entity.EntityState.Modified;
                            _db.SaveChanges();
                        }
                    }
                    catch (Exception)
                    {
                        return Json(new { result = "FAIL", errmsg = "数据保存失败。" });
                    }
                }
                return Json(new { result = "SUCCESS", errmsg = "" });

            }
        }

        //删除参与人
        [HttpPost]
        public JsonResult DeleteAssignmentCollaborator(int AssignmentId, int EmployeeId)
        {
            var employee = getEmployee(User.Identity.Name);
            var assignment = _db.Assignment.SingleOrDefault(m => m.Id == AssignmentId && m.Status > AssignmentStatus.DELETED);
            if (employee.HolderAssignment.Select(p => p.Id).Contains(AssignmentId) || employee.Subject.Select(m => m.Id).Contains(assignment.SubjectId))
            {
                if (assignment == null)
                {
                    return Json(new { result = "FAIL", errmsg = "此任务已不存在。" });
                }
                else
                {
                    var colstraff = _db.Employee.SingleOrDefault(m => m.Id == EmployeeId && m.Status > EmployeeStatus.DEVOICE);
                    var colNum = (from m in _db.SubTask
                                  where m.ExecutorId == EmployeeId && m.AssignmentId == AssignmentId && m.Status > AssignmentStatus.DELETED
                                  select m).Count();
                    if (colNum > 0)
                    {
                        return Json(new { result = "FAIL", errmsg = "无法删除请确认参与人不负责任何子任务。" });
                    }
                    else
                    {
                        try
                        {
                            assignment.Collaborator.Remove(colstraff);
                            _db.Entry(assignment).State = System.Data.Entity.EntityState.Modified;
                            _db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            return Json(new { result = "FAIL", errmsg = "存储失败。" });
                        }
                        return Json(new { result = "SUCCESS", errmsg = "" });
                    }
                }
            }
            else
            {
                return Json(new { result = "FAIL", errmsg = "当前用户没有权限删除参与人。" });
            }

        }


        //列表排序
        [HttpPost]
        public ActionResult SortProcedure(string ProcedureJson)
        {
            JArray ja = (JArray)JsonConvert.DeserializeObject(ProcedureJson);
            foreach (var item in ja)
            {
                try
                {
                    var objdata = JsonConvert.DeserializeObject<SortProcedureModel>(item.ToString());
                    var procedureid = Convert.ToInt32(objdata.procedureid);
                    var procedure = _db.Procedure.SingleOrDefault(m => m.Id == procedureid);
                    procedure.Sort = objdata.sort;
                    _db.Entry(procedure).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                }
                catch (Exception)
                {
                    return Content("FAIL");
                }
            }
            return Content("SUCCESS");
        }

        //任务拖拽改变
        [HttpPost]
        public ActionResult DragAssignment(int aid, int nowpid)
        {
            try
            {
                var original = _db.Assignment.SingleOrDefault(m => m.Id == aid);
                original.ProcedureId = nowpid;
                _db.Entry(original).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
            }
            catch (Exception)
            {
                return Content("FAIL");
            }
            return Content("SUCCESS");

        }

        //获取任务数字更新数据
        [HttpPost]
        public JsonResult GetProcedureJsonInfo(int SubjectId)
        {
            var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId);
            List<ProcedureJsonModel> plist = new List<ProcedureJsonModel>();
            foreach (var item in subject.ProcedureTemplate.Procedure.Where(p => p.Status == ProcedureStatus.NORMAL))
            {
                ProcedureJsonModel pinfo = new ProcedureJsonModel()
                {
                    ProcedureName = item.ProcedureTitle,
                    ProcedureId = item.Id,
                    FinishNum = item.Assignment.Count(m => m.Status == AssignmentStatus.FINISHED),
                    TotalNum = item.Assignment.Count(m => m.Status > AssignmentStatus.DELETED)
                };
                plist.Add(pinfo);
            }
            return Json(new { result = "SUCCESS", data = plist });
        }

        //获取项目侧边栏
        public PartialViewResult SubjectMenuPannelPartial(int SubjectId)
        {
            var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId);
            var loglist = (from m in _db.OperationLogs
                           where m.SubjectId == SubjectId
                           orderby m.LogTime descending
                           select m).Take(6).ToList();
            ViewBag.finishnum = (from m in subject.Assignment
                                 where m.Status == AssignmentStatus.FINISHED && m.CompleteDate >= DateTime.Today && m.CompleteDate < DateTime.Today.AddDays(1)
                                 select m).Count();
            ViewBag.waitnum = (from m in subject.Assignment
                               where m.Status == AssignmentStatus.UNFINISHED && (m.RemindDate >= DateTime.Today && m.RemindDate < DateTime.Today.AddDays(1) || m.Deadline >= DateTime.Today && m.Deadline < DateTime.Today.AddDays(1))
                               select m).Count();
            ViewBag.RecentEvent = loglist;
            return PartialView(subject);
        }

        //获取项目进度
        public PartialViewResult SubjectProgressPartial(int SubjectId)
        {
            var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId);
            return PartialView(subject);
        }


        //获取项目今日完成进度
        public PartialViewResult SubjectFinishProgressPartial(int SubjectId)
        {
            var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId);
            var AssignmentList = from m in subject.Assignment
                                 where m.Status == AssignmentStatus.FINISHED && m.CompleteDate >= DateTime.Today && m.CompleteDate < DateTime.Today.AddDays(1)
                                 select m;
            return PartialView(AssignmentList);
        }

        //获取项目今日待完成进度
        public PartialViewResult SubjectUntreatedProgressPartial(int SubjectId)
        {
            var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId);
            var AssignmentList = from m in subject.Assignment
                                 where m.Status == AssignmentStatus.UNFINISHED && (m.RemindDate >= DateTime.Today && m.RemindDate < DateTime.Today.AddDays(1) || m.Deadline >= DateTime.Today && m.Deadline < DateTime.Today.AddDays(1))
                                 select m;
            return PartialView(AssignmentList);
        }
        //获取项目日志
        public PartialViewResult SubjectLogsPartial(int SubjectId)
        {
            var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId);
            return PartialView(subject);
        }

        //项目日志分页获取
        public PartialViewResult SubjectLogsAjaxPartial(int? page, int SubjectId)
        {
            int _page = page ?? 0;
            var LogList = (from m in _db.OperationLogs
                           where m.SubjectId == SubjectId
                           orderby m.LogTime descending
                           select m).Skip(_page * 20).Take(20);
            return PartialView(LogList);
        }

        //任务评论模板
        public PartialViewResult Assignment_CommentPartial(int AssignmentId)
        {
            var commentlist = from m in _db.AssignmentComment
                              where m.Status > CommentStatus.REMOVED && m.AssignmentId == AssignmentId
                              orderby m.CreateTime descending
                              select m;
            return PartialView(commentlist);
        }

        //任务评论添加
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> Add_AssignmentComment(AssignmentComment model)
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Json(new { result = "FAIL", errmsg = "员工不存在。" });
            }
            else
            {
                if (ModelState.IsValid)
                {
                    AssignmentComment item = new AssignmentComment();
                    if (TryUpdateModel(item))
                    {
                        try
                        {
                            item.ComposerId = employee.Id;
                            item.Status = CommentStatus.NORMAL;
                            item.CreateTime = DateTime.Now;
                            var assignment = _db.Assignment.SingleOrDefault(m => m.Id == item.AssignmentId && m.Status > AssignmentStatus.DELETED);
                            _db.AssignmentComment.Add(item);
                            await _db.SaveChangesAsync();
                            await AddLogAsync(LogCode.EDITTASK, employee, assignment.SubjectId, "添加了一条评论为任务:" + assignment.AssignmentTitle + "。");
                        }
                        catch (Exception)
                        {
                            return Json(new { result = "FAIL", errmsg = "存储失败。" });
                        }

                        return Json(new { result = "SUCCESS", msg = "添加评论成功。" });
                    }
                    else
                    {
                        return Json(new { result = "FAIL", errmsg = "模型同步失败。" });
                    }
                }
                else
                {
                    return Json(new { result = "FAIL", errmsg = "模型错误。" });
                }
            }
        }


        //删除评论
        [HttpPost]
        public async Task<JsonResult> Delete_AssignmentComment(int CommentId)
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Json(new { result = "FAIL", errmsg = "员工不存在。" });
            }
            else
            {
                var comment = _db.AssignmentComment.SingleOrDefault(m => m.Id == CommentId && m.Status == CommentStatus.NORMAL);
                if (comment == null)
                {
                    return Json(new { result = "FAIL", errmsg = "评论已不存在请刷新页面。" });
                }
                else
                {

                    if (employee.Id == comment.Assignment.HolderId || employee.Id == comment.Assignment.Subject.HolderId || employee.Id == comment.ComposerId)
                    {
                        try
                        {
                            comment.Status = CommentStatus.REMOVED;
                            _db.Entry(comment).State = System.Data.Entity.EntityState.Modified;
                            await _db.SaveChangesAsync();
                        }
                        catch (Exception)
                        {
                            return Json(new { result = "FAIL", errmsg = "评论修改失败。" });
                        }
                    }
                    else
                    {
                        return Json(new { result = "FAIL", msg = "你没有权限操作此评论。" });
                    }


                    return Json(new { result = "SUCCESS", msg = "评论删除成功。" });
                }

            }
        }

        //项目文件
        public ActionResult Subject_Files(int SubjectId)
        {
            var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId);
            if (subject.Status == SubjectStatus.ARCHIVED || subject.Status == SubjectStatus.DELETED)
            {
                return View("Error");
            }
            else
            {
                ViewBag.img = getEmployee(User.Identity.Name).ImgUrl;
                return View(subject);
            }
        }

























        public Employee getEmployee(string username)
        {
            var user = _db.Employee.SingleOrDefault(m => m.UserName == username);
            return user;
        }


        [HttpPost]
        public JsonResult UploadSubjectCutFileAjax(FormCollection form)
        {
            var files = Request.Files;
            string errmsg = string.Empty;
            string error = string.Empty;
            string imgurl;

            if (files.Count > 0)
            {
                if (files[0].ContentLength > 0 && files[0].ContentType.Contains("image"))
                {
                    string filename = DateTime.Now.ToFileTime().ToString() + ".jpg";

                    Stream stream = files[0].InputStream;
                    HttpPostedFileBase NeedByteFile = files[0];   //需要转换的文件
                    var ByteArray = SetFileToByteArray(NeedByteFile);   //转换
                    var jsonimgsize = JsonConvert.DeserializeObject<CutImgModel>(form["avatar_data"]);  //获取截图参数                      
                    string strtExtension = Path.GetExtension(files[0].FileName);//图片格式
                    MemoryStream ms1 = new MemoryStream(ByteArray);
                    Bitmap sBitmap = (Bitmap)Image.FromStream(ms1);
                    Rectangle section = new Rectangle(new Point(jsonimgsize.ToInt(jsonimgsize.x), jsonimgsize.ToInt(jsonimgsize.y)), new Size(jsonimgsize.ToInt(jsonimgsize.width), jsonimgsize.ToInt(jsonimgsize.height)));
                    Bitmap CroppedImage = MakeThumbnailImage(sBitmap, 200, 200, section.X, section.Y, section.Width, section.Height, jsonimgsize.rotate);
                    AliOSSUtilities util = new AliOSSUtilities();
                    util.PutObject(BitmapByte(CroppedImage), "Subject/Avatar/" + filename);
                    imgurl = filename;
                    return Json(new { result = "SUCCESS", imgurl = "Subject/Avatar/" + imgurl });
                }
                else
                {
                    error = "文件错误";
                    return Json(new { result = error });
                }
            }
            return Json(new { result = "FAIL" });
        }




        //旋转图片
        public static Bitmap KiRotate(Image originalImage, float angle)
        {
            int w = originalImage.Width + 2;
            int h = originalImage.Height + 2;
            Bitmap tmp = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(tmp);
            g.DrawImageUnscaled(originalImage, 1, 1);
            g.Dispose();

            GraphicsPath path = new GraphicsPath();
            path.AddRectangle(new RectangleF(0f, 0f, w, h));
            Matrix mtrx = new Matrix();
            mtrx.Rotate(angle);
            RectangleF rct = path.GetBounds(mtrx);

            Bitmap dst = new Bitmap((int)rct.Width, (int)rct.Height);
            g = Graphics.FromImage(dst);
            g.TranslateTransform(-rct.X, -rct.Y);
            g.RotateTransform(angle);
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            g.DrawImageUnscaled(tmp, 0, 0);
            g.Dispose();

            tmp.Dispose();

            return dst;
        }
        //bitmap转byte[] 
        public static byte[] BitmapByte(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Jpeg);
                byte[] data = new byte[stream.Length];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(data, 0, Convert.ToInt32(stream.Length));
                return data;
            }
        }


        /// <summary>
        /// 将From表单file文件转化为byte数组
        /// </summary>
        /// <param name="File">from提交文件流</param>
        /// <returns></returns>
        private byte[] SetFileToByteArray(HttpPostedFileBase File)
        {
            Stream stream = File.InputStream;
            byte[] ArrayByte = new byte[File.ContentLength];
            stream.Read(ArrayByte, 0, File.ContentLength);
            stream.Close();
            return ArrayByte;
        }

        /// <summary>
        /// 裁剪图片并保存
        /// </summary>
        /// <param name="Image">图片信息</param>
        /// <param name="maxWidth">缩略图宽度</param>
        /// <param name="maxHeight">缩略图高度</param>
        /// <param name="cropWidth">裁剪宽度</param>
        /// <param name="cropHeight">裁剪高度</param>
        /// <param name="X">X轴</param>
        /// <param name="Y">Y轴</param>
        public static Bitmap MakeThumbnailImage(Image originalImage, int width, int height, int X, int Y, int cropWidth, int cropHeight, float angle)
        {
            Bitmap _b = new Bitmap(width, height);
            var b = KiRotate(originalImage, angle);
            try
            {
                using (Graphics g = Graphics.FromImage(_b))
                {
                    //清空画布并以透明背景色填充
                    g.Clear(Color.Transparent);
                    //在指定位置并且按指定大小绘制原图片的指定部分
                    g.DrawImage(b, new Rectangle(0, 0, width, height), new Rectangle(X, Y, cropWidth, cropHeight), GraphicsUnit.Pixel);
                    return _b;
                }
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                originalImage.Dispose();
                b.Dispose();
            }
        }







        //日志
        private async Task<bool> AddLogAsync(int code, int employeeId, int subjectId, string info)
        {
            var employee = _db.Employee.SingleOrDefault(m => m.Id == employeeId);
            if (employee != null)
            {
                bool result = await AddLogAsync(code, employee, subjectId, info);
                return result;
            }
            return false;
        }
        private async Task<bool> AddLogAsync(int code, Employee employee, int subjectId, string info)
        {
            var subject = _db.Subject.SingleOrDefault(m => m.Id == subjectId);
            if (subject != null)
            {
                string logContent = "";
                if (code == LogCode.CREATESUBJECT)
                {
                    logContent = employee.NickName + " 添加了项目 " + subject.SubjectTitle;
                }
                else if (code == LogCode.EDITSUBJECT)
                {
                    logContent = employee.NickName + " 修改了项目 " + subject.SubjectTitle;
                }
                else if (code == LogCode.ARCHIVESUBJECT)
                {
                    logContent = employee.NickName + " 将项目 " + subject.SubjectTitle + " 进行了归档";
                }
                else if (code == LogCode.DELETESUBJECT)
                {
                    logContent = employee.NickName + " 删除了项目 " + subject.SubjectTitle;
                }
                else if (code == LogCode.CREATETASK)
                {
                    logContent = employee.NickName + info;
                }
                else if (code == LogCode.EDITTASK)
                {
                    logContent = employee.NickName + info;
                }
                else if (code == LogCode.DELETETASK)
                {
                    logContent = employee.NickName + info;
                }
                else if (code == LogCode.CREATESUBTASK)
                {
                    logContent = employee.NickName + info;
                }
                else if (code == LogCode.EDITSUBTASK)
                {
                    logContent = employee.NickName + info;
                }
                else if (code == LogCode.DELETESUBTASK)
                {
                    logContent = employee.NickName + info;
                }
                else if (code == LogCode.RESETSUBJECT)
                {
                    logContent = employee.NickName + info;
                }
                OperationLogs log = new OperationLogs()
                {
                    LogCode = code,
                    LogContent = logContent,
                    LogTime = DateTime.Now,
                    SubjectId = subjectId,
                    UserId = employee.Id
                };
                _db.OperationLogs.Add(log);
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }

}