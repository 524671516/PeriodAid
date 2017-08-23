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
using Newtonsoft.Json.Linq;
using PeriodAid.Filters;
using System.Data;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

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


        public ActionResult SubjectNotFound()
        {
            return View();
        }


        public ActionResult SubjectNoAuthority()
        {
            return View();
        }
        //修改个人信息
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

        //获取登录记录
        public ActionResult LoginLog(string username) {
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

        //安全设置
        public ActionResult SecuritySetting(string username)
        {
            var employee = getEmployee(User.Identity.Name);
            
            if (employee == null)
            {
                return View("Error");
            }
            else
            {
                ViewBag.img = employee.ImgUrl;
                return View();
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
                    return Json(new { result = "SUCCESS"});
                }
                return Json(new { result = "模型同步错误。" });
            }
            return Json(new { result = "格式错误。" });
        }

        #region 项目操作

        //进行中的项目
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

        //已完成的项目
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

        //获取项目实例
        [OperationAuth(OperationGroup = 102)]
        public ActionResult Subject_Detail(int SubjectId)
        {
            var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId && m.Status == SubjectStatus.ACTIVE);
            if (subject == null)
            {
                return RedirectToAction("Index");
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

        //获取项目填写表单
        [OperationAuth(OperationGroup = 101)]
        public PartialViewResult GetSubjectForm()
        {
            Subject model = new Subject()
            {
                TemplateId = 1,
            };
            return PartialView(model);
        }

        //创建项目
        [HttpPost, ValidateAntiForgeryToken]
        [OperationAuth(OperationGroup = 101)]
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
                            await AddLogAsync(LogCode.CREATESUBJECT, employee, item.Id, "创建了项目。");
                        }
                        catch (Exception)
                        {
                            return Json(new { result = "FAIL", errmsg = "数据存储失败。" });
                        }
                        return Json(new { result = "SUCCESS", msg = "" });
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


        //获取项目修改信息
        [OperationAuth(OperationGroup = 101)]
        public ActionResult EditSubject(int SubjectId)
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Content("FAIL");
            }
            else
            {
                var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId && m.Status > SubjectStatus.DELETED);
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

        //修改项目
        [HttpPost, ValidateAntiForgeryToken]
        [OperationAuth(OperationGroup = 103)]
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
                        await AddLogAsync(LogCode.EDITSUBJECT, employee, item.Id, "修改了项目的基本信息。");
                        return Json(new { result = "SUCCESS", msg = "项目基本信息修改成功。" });
                    }
                    return Json(new { result = "FAIL", errmsg = "模型同步失败。" });
                }
            }
            else
            {
                return Json(new { result = "FAIL", errmsg = "模型验证失败。" });
            }
        }

        //归档项目
        [HttpPost]
        [OperationAuth(OperationGroup = 103)]
        public async Task<JsonResult> ArchiveSubject(int SubjectId)
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Json(new { result = "FAIL", errmsg = "员工不存在。" });
            }
            else
            {
                var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId);
                if (subject != null)
                {
                    if (subject.HolderId == employee.Id || employee.Type == EmployeeType.DEPARTMENTMANAGER)
                    {
                        subject.Status = SubjectStatus.ARCHIVED;
                        _db.Entry(subject).State = System.Data.Entity.EntityState.Modified;
                        await _db.SaveChangesAsync();
                        await AddLogAsync(LogCode.ARCHIVESUBJECT, employee, subject.Id, "将项目状态设置为：归档。");
                        return Json(new { result = "SUCCESS", msg = "归档项目成功。" });
                    }
                    else
                    {
                        return Json(new { result = "FAIL", errmsg = "当前用户没有权限归档项目。" });
                    }
                }
                return Json(new { result = "FAIL", errmsg = "操作的项目不存在。" });
            }
        }


        //删除项目
        [HttpPost]
        [OperationAuth(OperationGroup = 103)]
        public async Task<JsonResult> DeleteSubject(int SubjectId)
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Json(new { result = "FAIL", errmsg = "员工不存在。" });
            }
            else
            {
                var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId);
                if (subject != null)
                {
                    if (subject.HolderId == employee.Id || employee.Type == EmployeeType.DEPARTMENTMANAGER) {
                        subject.Status = SubjectStatus.DELETED;
                        _db.Entry(subject).State = System.Data.Entity.EntityState.Modified;
                        await _db.SaveChangesAsync();
                        await AddLogAsync(LogCode.DELETESUBJECT, employee, subject.Id, "将项目状态设置为：删除。");
                        return Json(new { result = "SUCCESS", msg = "删除项目成功。" });
                    }
                    return Json(new { result = "FAIL", errmsg = "当前用户没有权限删除项目。" });
                }
                return Json(new { result = "FAIL", errmsg = "操作的项目不存在。" });
            }
        }

        //重置项目
        [HttpPost]
        [OperationAuth(OperationGroup = 103)]
        public async Task<JsonResult> ResetSubject(int SubjectId)
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Json(new { result = "FAIL", errmsg = "员工不存在。" });
            }
            else
            {
                var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId);
                if (subject != null)
                {
                    if (subject.HolderId == employee.Id || employee.Type == EmployeeType.DEPARTMENTMANAGER) {
                        subject.Status = SubjectStatus.ACTIVE;
                        _db.Entry(subject).State = System.Data.Entity.EntityState.Modified;
                        await _db.SaveChangesAsync();
                        await AddLogAsync(LogCode.RESETSUBJECT, employee, subject.Id, "将项目状态设置为：进行中。");
                        return Json(new { result = "SUCCESS", msg = "恢复项目成功。" });
                    }
                    return Json(new { result = "FAIL", errmsg = "当前用户没有权限恢复项目。" });
                }
                return Json(new { result = "FAIL", errmsg = "操作的项目不存在。" });
            }
        }

        //上传项目封面
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


        #region  项目任务列表操作
        //获取任务列表
        public ActionResult SubjectProcedure(int ProcedureId)
        {
            var procedure = _db.Procedure.SingleOrDefault(m => m.Id == ProcedureId && m.Status == ProcedureStatus.NORMAL);
            if (procedure != null)
            {
                return PartialView(procedure);
            }
            return Content("FAIL");
        }


        //创建任务列表
        [HttpPost, ValidateAntiForgeryToken]
        [OperationAuth(OperationGroup = 103)]
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
                        return Json(new { result = "FAIL", errmsg = "当前用户没有权限添加任务列表。" });
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

        //删除任务列表
        [HttpPost]
        [OperationAuth(OperationGroup = 103)]
        public async Task<JsonResult> Delete_Procedure(int ProcedureId, int SubjectId)
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
                        await _db.SaveChangesAsync();
                        await AddLogAsync(LogCode.EDITSUBJECT, employee, SubjectId, "删除了任务列表：" + procedure.ProcedureTitle + "。");
                    }
                    catch (Exception)
                    {
                        return Json(new { result = "FAIL", errmsg = "删除任务列表失败。" });
                    }
                    return Json(new { result = "SUCCESS", Id = procedure.Id });
                }
                else
                {
                    return Json(new { result = "FAIL", errmsg = "操作失败,请先清空列表中的任务。" });
                }
            }
            else
            {
                return Json(new { result = "FAIL", errmsg = "你没有权限删除任务列表。" });
            }
        }


        //获取任务列表表单
        public PartialViewResult GetProcedureForm(int ProcedureId,int SubjectId)
        {
            var procedure = _db.Procedure.SingleOrDefault(m => m.Id == ProcedureId);
            ViewBag.sid = SubjectId;
            return PartialView(procedure);
        }
        //修改任务列表
        [HttpPost]
        [OperationAuth(OperationGroup = 103)]
        public async Task<JsonResult> Edit_Procedure(Procedure model,FormCollection form)
        {
            var employee = getEmployee(User.Identity.Name);
            if (ModelState.IsValid)
            {
                try
                {
                    if (TryUpdateModel(model))
                    {
                        _db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                        await _db.SaveChangesAsync();
                        await AddLogAsync(LogCode.EDITSUBJECT, employee, Convert.ToInt32(form["SubjectId"]), "将任务列表【" + form["OldTitle"] + "】修改为【" + model.ProcedureTitle + "】。");
                        return Json(new { result = "SUCCESS", msg = "修改成功。" });
                    }
                    else
                    {
                        return Json(new { result = "FAIL", errmsg = "模型同步失败。" });
                    }
                }
                catch (Exception)
                {
                    return Json(new { result = "FAIL", errmsg = "内部异常。" });
                }
            }
            else
            {
                return Json(new { result = "FAIL", errmsg = "模型验证失败。" });
            }
        }

        //列表排序
        [HttpPost]
        [OperationAuth(OperationGroup = 103)]
        public async Task<JsonResult>SortProcedure(string ProcedureJson,int SubjectId)
        {
            var employee = getEmployee(User.Identity.Name);
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
                    await _db.SaveChangesAsync();                   
                }
                catch (Exception)
                {
                    return Json(new { result = "FAIL", errmsg = "数据上传失败。" });
                }
            }
            await AddLogAsync(LogCode.EDITSUBJECT, employee, SubjectId, "调整了任务列表的顺序。");
            return Json(new { result = "SUCCESS", msg = "" });
        }
        #endregion


        #region  任务操作
        //获取任务
        public PartialViewResult SubjectAssignment(int ProcedureId, int SubjectId)
        {
            var assignmentlist = from m in _db.Assignment
                                 where m.ProcedureId == ProcedureId && m.SubjectId == SubjectId && m.Status > AssignmentStatus.DELETED
                                 orderby m.Status ascending
                                 select m;
            ViewBag.ProcedureId = ProcedureId;
            return PartialView(assignmentlist);
        }

        //获取任务填写表单
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

        //创建任务
        [HttpPost, ValidateAntiForgeryToken]
        [OperationAuth(OperationGroup = 201)]
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
                        await AddLogAsync(LogCode.CREATETASK, employee, item.SubjectId, "创建了任务【" + item.AssignmentTitle + "】。");
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

        //获取任务详情
        [OperationAuth(OperationGroup = 202)]
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

        //任务修改
        [HttpPost, ValidateAntiForgeryToken]
        [OperationAuth(OperationGroup = 203)]
        public async Task<JsonResult> Edit_Assignment_Detail(FormCollection form, Assignment model)
        {
            var employee = getEmployee(User.Identity.Name);
            if (ModelState.IsValid)
            {
                var item = _db.Assignment.SingleOrDefault(m => m.Id == model.Id);         
                if (employee.Subject.Contains(item.Subject) || item.Holder == employee || employee.Type == EmployeeType.DEPARTMENTMANAGER)
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
                            await AddLogAsync(LogCode.EDITTASK, employee, item.SubjectId, "修改了任务【" + item.AssignmentTitle + "】。");
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
                    return Json(new { result = "FAIL", errmsg = "当前用户没有权限修改此任务。" });
                }
            }
            else
            {
                return Json(new { result = "FAIL", errmsg = "模型错误。" });
            }
        }

        //任务完成状态回传
        [HttpPost]
        [OperationAuth(OperationGroup = 203)]
        public async Task<JsonResult> ComfirmFinishAssignment(int AssignmentId)
        {
            var employee = getEmployee(User.Identity.Name);
            var assignment = _db.Assignment.SingleOrDefault(m => m.Id == AssignmentId && m.Status > AssignmentStatus.DELETED);
            var subtaskFinsh_count = assignment.SubTask.Count(m => m.Status == AssignmentStatus.UNFINISHED);
            if (subtaskFinsh_count != 0)
            {
                return Json(new { result = "FAIL", errmsg = "请先完成子任务。" });
            }
            else
            {
                if (assignment.Status == AssignmentStatus.UNFINISHED)
                {
                    assignment.Status = AssignmentStatus.FINISHED;
                    assignment.CompleteDate = DateTime.Now;
                    await AddLogAsync(LogCode.EDITTASK, employee, assignment.SubjectId, "将任务【" + assignment.AssignmentTitle + "】的状态修改为【完成】。");
                }
                else
                {
                    assignment.Status = AssignmentStatus.UNFINISHED;
                    assignment.CompleteDate = null;
                    await AddLogAsync(LogCode.EDITTASK, employee, assignment.SubjectId, "将任务【" + assignment.AssignmentTitle + "】的状态修改为【未完成】。");
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

        //删除任务
        [HttpPost]
        [OperationAuth(OperationGroup = 203)]
        public async Task<JsonResult> Delete_Assignment(int AssignmentId)
        {
            var employee = getEmployee(User.Identity.Name);
            var assignment = _db.Assignment.SingleOrDefault(m => m.Id == AssignmentId && m.Status > AssignmentStatus.DELETED);
                if (employee.Subject.Contains(assignment.Subject) || assignment.Holder == employee || employee.Type == EmployeeType.DEPARTMENTMANAGER)
                {
                    try
                    {
                        assignment.Status = AssignmentStatus.DELETED;
                        _db.Entry(assignment).State = System.Data.Entity.EntityState.Modified;
                        await _db.SaveChangesAsync();
                        await AddLogAsync(LogCode.EDITTASK, employee, assignment.SubjectId, "删除了任务【" + assignment.AssignmentTitle + "】。");
                    }
                    catch (Exception)
                    {
                        return Json(new { result = "FAIL", errmsg = "保存数据失败。" });
                    }
                    return Json(new { result = "SUCCESS", Id = assignment.ProcedureId });
                }
                else
                {
                    return Json(new { result = "FAIL", errmsg = "当前用户没有权限删除此任务。" });
                }
        }

        //任务拖拽改变
        [HttpPost]
        [OperationAuth(OperationGroup = 203)]
        public async Task<JsonResult> DragAssignment(int AssignmentId, int nowpid)
        {
            var employee = getEmployee(User.Identity.Name);
            try
            {
                var original = _db.Assignment.SingleOrDefault(m => m.Id == AssignmentId);
                var nowprocedure = _db.Procedure.SingleOrDefault(m => m.Id == nowpid);
                original.ProcedureId = nowpid;
                _db.Entry(original).State = System.Data.Entity.EntityState.Modified;
                await _db.SaveChangesAsync();
                await AddLogAsync(LogCode.EDITTASK, employee, original.SubjectId, "将任务【" + original.AssignmentTitle + "】从"+original.Procedure.ProcedureTitle+"移动到了"+nowprocedure.ProcedureTitle+"。");
            }
            catch (Exception)
            {
                return Json(new { result = "FAIL", errmsg = "内部异常。" });
            }
            return Json(new { result = "SUCCESS" });

        }
        #endregion


        #region  任务参与人操作
        //获取任务的参与者模板
        [OperationAuth(OperationGroup = 202)]
        public PartialViewResult Assignment_CollaboratorPartial(int AssignmentId)
        {
            var assignment = _db.Assignment.SingleOrDefault(m => m.Id == AssignmentId && m.Status > AssignmentStatus.DELETED);
            return PartialView(assignment);
        }

        //添加参与者模板
        [OperationAuth(OperationGroup = 202)]
        public ActionResult Assignmnet_CollaboratorAddPartial(int AssignmentId)
        {
            try
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
            catch (Exception)
            {
                return Content("FAIL");
            }
        }

        //添加参与人
        [HttpPost]
        [OperationAuth(OperationGroup = 203)]
        public async Task<JsonResult> AddAssignmentCollaborator(int AssignmentId, string colvalue)
        {
            try
            {
                var employee = getEmployee(User.Identity.Name);
                var assignment = _db.Assignment.SingleOrDefault(m => m.Id == AssignmentId && m.Status > AssignmentStatus.DELETED);
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
                            await _db.SaveChangesAsync();
                        }
                    }
                    catch (Exception)
                    {
                        return Json(new { result = "FAIL", errmsg = "数据保存失败。" });
                    }
                }
                await AddLogAsync(LogCode.EDITTASK, employee, assignment.SubjectId, "为任务【" + assignment.AssignmentTitle + "】添加"+ colarray.Count()+"位参与人。");
                return Json(new { result = "SUCCESS", errmsg = "" });
            }
            catch (Exception)
            {
                return Json(new { result = "FAIL", errmsg = "内部异常。" });
            }
        }

        //删除参与人
        [HttpPost]
        [OperationAuth(OperationGroup = 203)]
        public async Task<JsonResult> DeleteAssignmentCollaborator(int AssignmentId, int EmployeeId)
        {
            var employee = getEmployee(User.Identity.Name);
            var assignment = _db.Assignment.SingleOrDefault(m => m.Id == AssignmentId && m.Status > AssignmentStatus.DELETED);
            if (employee.HolderAssignment.Select(p => p.Id).Contains(AssignmentId) || employee.Subject.Select(m => m.Id).Contains(assignment.SubjectId) ||employee.Type==EmployeeType.DEPARTMENTMANAGER)
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
                        await _db.SaveChangesAsync();
                        await AddLogAsync(LogCode.EDITTASK, employee, assignment.SubjectId, "为任务【" + assignment.AssignmentTitle + "】删除了1位参与人。");
                    }
                        catch (Exception)
                        {
                            return Json(new { result = "FAIL", errmsg = "存储失败。" });
                        }
                        return Json(new { result = "SUCCESS", errmsg = "" });
                    }
            }
            else
            {
                return Json(new { result = "FAIL", errmsg = "当前用户没有权限删除参与人。" });
            }

        }
        #endregion


        #region 子任务操作
        //获取子任务模板
        [OperationAuth(OperationGroup = 202)]
        public PartialViewResult Assignment_SubtaskPartial(int AssignmentId)
        {
            var subtasklist = from m in _db.SubTask
                              where m.AssignmentId == AssignmentId && m.Status > AssignmentStatus.DELETED
                              orderby m.Status
                              select m;
            return PartialView(subtasklist);
        }

        //获取子任务表单
        [OperationAuth(OperationGroup = 202)]
        public ActionResult GetSubtaskForm(int AssignmentId)
        {
            var assignment = _db.Assignment.SingleOrDefault(m => m.Id == AssignmentId && m.Status > AssignmentStatus.DELETED);
            if (assignment == null)
            {
                return Content("FAIL");
            }
            try
            {
                var collaborator = assignment.Collaborator;
                List<Employee> emlist = new List<Employee>();
                emlist.Add(assignment.Holder);
                emlist.Add(assignment.Subject.Holder);
                foreach (var i in collaborator)
                {
                    emlist.Add(i);
                }

                SubTask model = new SubTask()
                {
                    AssignmentId = AssignmentId
                };
                ViewBag.EmployeeDropDown = new SelectList(emlist.Distinct().ToList(), "Id", "NickName", assignment.HolderId);
                return PartialView(model);
            }
            catch (Exception)
            {
                return Content("FAIL");
            }
        }

        //创建子任务
        [HttpPost, ValidateAntiForgeryToken]
        [OperationAuth(OperationGroup = 301)]
        public async Task<JsonResult> CreateSubtask(SubTask model)
        {
            var employee = getEmployee(User.Identity.Name);
            if (ModelState.IsValid)
            {
                SubTask item = new SubTask();
                if (TryUpdateModel(item))
                {
                    item.CreateTime = DateTime.Now;
                    item.Status = AssignmentStatus.UNFINISHED;
                    try
                    {

                        var assignment = _db.Assignment.SingleOrDefault(m => m.Id == item.AssignmentId);
                        assignment.Status = AssignmentStatus.UNFINISHED;
                        _db.Entry(assignment).State = System.Data.Entity.EntityState.Modified;
                        _db.SubTask.Add(item);
                        await _db.SaveChangesAsync();
                        await AddLogAsync(LogCode.EDITTASK, employee, assignment.SubjectId, "为任务【" + assignment.AssignmentTitle + "】创建了子任务【"+item.TaskTitle+"】。");
                    }
                    catch (Exception)
                    {
                        return Json(new { result = "FAIL",errmsg="内部异常。" });
                    }
                    var ProcedureId = _db.Assignment.SingleOrDefault(m => m.Id == item.AssignmentId).ProcedureId;
                    return Json(new { result = "SUCCESS", Id = ProcedureId });

                }
                else
                {
                    return Json(new { result = "FAIL", errmsg = "模型同步错误。" });
                }
            }
            else
            {
                return Json(new {result = "FAIL", errmsg = "模型错误。" });
            }
        }

        //修改子任务
        [HttpPost]
        [OperationAuth(OperationGroup = 303)]
        public async Task<JsonResult> EditSubtask(SubTask model)
        {
            var employee = getEmployee(User.Identity.Name);
            if (ModelState.IsValid)
            {
                var item = _db.SubTask.SingleOrDefault(m => m.Id == model.Id);
                if (TryUpdateModel(item))
                {
                    try
                    {
                        _db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        await _db.SaveChangesAsync();
                        await AddLogAsync(LogCode.EDITTASK, employee, item.Assignment.SubjectId, "修改了任务【" + item.Assignment.AssignmentTitle + "】中的子任务【" + item.TaskTitle + "】。");

                    }
                    catch (Exception)
                    {
                        return Json(new { result = "FAIL",errmsg="内部异常。" });
                    }
                    return Json(new { result = "SUCCESS", id = item.AssignmentId });
                }
                else
                {
                    return Json(new {result = "FAIL", errmsg = "模型同步错误。" });
                }
            }
            else
            {
                return Json(new {result = "FAIL", errmsg = "模型错误。" });
            }
        }

        //子任务详情
        [OperationAuth(OperationGroup = 302)]
        public PartialViewResult Subtask_Detail(int SubTaskId)
        {
            var subtask = _db.SubTask.SingleOrDefault(m => m.Id == SubTaskId && m.Status > AssignmentStatus.DELETED);
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
        [OperationAuth(OperationGroup = 303)]
        public async Task<JsonResult> Delete_Subtask(int SubTaskId)
        {
            var employee = getEmployee(User.Identity.Name);
            var subtask = _db.SubTask.SingleOrDefault(m => m.Id == SubTaskId && m.Status > AssignmentStatus.DELETED);
                try
                {
                    subtask.Status = AssignmentStatus.DELETED;
                    _db.Entry(subtask).State = System.Data.Entity.EntityState.Modified;
                    await _db.SaveChangesAsync();
                    await AddLogAsync(LogCode.EDITTASK, employee, subtask.Assignment.SubjectId, "删除了任务【" + subtask.Assignment.AssignmentTitle + "】中的子任务【" + subtask.TaskTitle + "】。");
                }
                catch (Exception)
                {
                    return Json(new { result = "FAIL",errmsg="内部异常。" });
                }
                return Json(new { result = "SUCCESS", Id = subtask.Assignment.ProcedureId });
        }

        //子任务完成状态回传
        [HttpPost]
        [OperationAuth(OperationGroup = 303)]
        public async Task<JsonResult> ComfirmFinishSubtask(int SubTaskId)
        {
            var employee = getEmployee(User.Identity.Name);
            var subtask = _db.SubTask.SingleOrDefault(m => m.Id == SubTaskId && m.Status > AssignmentStatus.DELETED);
            if (subtask.Status == AssignmentStatus.UNFINISHED)
            {
                subtask.Status = AssignmentStatus.FINISHED;
                subtask.CompleteDate = DateTime.Now;
                await AddLogAsync(LogCode.EDITTASK, employee, subtask.Assignment.SubjectId, "将任务【" + subtask.Assignment.AssignmentTitle + "】中的子任务【" + subtask.TaskTitle + "】的状态修改为完成。");
            }
            else
            {
                subtask.Assignment.Status = AssignmentStatus.UNFINISHED;
                subtask.Status = AssignmentStatus.UNFINISHED;
                subtask.CompleteDate = null;
                await AddLogAsync(LogCode.EDITTASK, employee, subtask.Assignment.SubjectId, "将任务【" + subtask.Assignment.AssignmentTitle + "】中的子任务【" + subtask.TaskTitle + "】的状态修改为未完成。");
            }
            try
            {
                _db.Entry(subtask).State = System.Data.Entity.EntityState.Modified;
                await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                return Json(new { result = "FAIL",errmsg="内部异常。" });
            }
            return Json(new { result = "SUCCESS", Id = subtask.Assignment.ProcedureId,ParentStatus=subtask.Assignment.Status });
        }
        #endregion

        #region 任务评论操作
        //任务评论模板
        [OperationAuth(OperationGroup = 202)]
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
        [OperationAuth(OperationGroup = 202)]
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
                            await AddLogAsync(LogCode.EDITTASK, employee, assignment.SubjectId, "添加了一条评论为任务【" + assignment.AssignmentTitle + "】。");
                        }
                        catch (Exception)
                        {
                            return Json(new { result = "FAIL", errmsg = "内部异常。" });
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
                            return Json(new { result = "FAIL", errmsg = "内部异常。" });
                        }
                    }
                    else
                    {
                        return Json(new { result = "FAIL", errmsg = "当前用户没有权限操作此评论。" });
                    }


                    return Json(new { result = "SUCCESS", msg = "评论删除成功。" });
                }

            }
        }
        #endregion


        #region  数据获取操作
        //获取任务列表数字更新数据
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
        #endregion


        #region 日志操作
        //获取项目日志
        [OperationAuth(OperationGroup = 102)]
        public PartialViewResult SubjectLogsPartial(int SubjectId)
        {
            var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId);
            var EmployeeList = from m in _db.Employee
                               where m.Status > EmployeeStatus.DEVOICE
                               select m;
            ViewBag.EmployeeDropDown = EmployeeList;
            return PartialView(subject);
        }

        //项目日志分页获取
        [OperationAuth(OperationGroup = 102)]
        public PartialViewResult SubjectLogsAjaxPartial(int? page, int SubjectId)
        {
            int _page = page ?? 0;
            var LogList = (from m in _db.OperationLogs
                           where m.SubjectId == SubjectId
                           orderby m.LogTime descending
                           select m).Skip(_page * 20).Take(20);
            return PartialView(LogList);
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
                    logContent = employee.NickName + info;
                }
                else if (code == LogCode.EDITSUBJECT)
                {
                    logContent = employee.NickName + info;
                }
                else if (code == LogCode.ARCHIVESUBJECT)
                {
                    logContent = employee.NickName + info;
                }
                else if (code == LogCode.DELETESUBJECT)
                {
                    logContent = employee.NickName + info;
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
        #endregion


        #region 文件操作
        //项目文件页
        [OperationAuth(OperationGroup = 102)]
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

        //获取项目文件模板
        [OperationAuth(OperationGroup = 102)]
        public ActionResult Subject_FilesPartial(int SubjectId, int TypeCode)
        {
            var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId);
            if (subject.Status == SubjectStatus.ARCHIVED || subject.Status == SubjectStatus.DELETED)
            {
                return Content("FAIL");
            }
            else
            {
                var subjectattachment = from m in _db.SubjectAttachment
                                        where m.SubjectId == subject.Id && m.AttachmentType == TypeCode&&m.Status>AttachmentStatus.DELETE
                                        orderby m.UploadTime descending
                                        select m;              
                ContentTypeClass ctc = GetContentType(TypeCode);
                ViewBag.CTC = ctc;
                return PartialView(subjectattachment);
            }
        }

        //修改文件
        [HttpPost]
        public async Task<JsonResult> Subject_EditFile(int FileId,string Filename)
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Json(new { result = "FAIL", errmsg = "员工不存在。" });
            }
            else
            {
                var file = _db.SubjectAttachment.SingleOrDefault(m => m.Id == FileId && m.Status > AttachmentStatus.DELETE);
                if (file != null)
                {
                    if (file.UploaderId == employee.Id || file.Subject.HolderId == employee.Id)
                    {
                        file.AttachmentTitle = Filename;
                        _db.Entry(file).State = System.Data.Entity.EntityState.Modified;
                        await _db.SaveChangesAsync();
                        await AddLogAsync(LogCode.EDITSUBJECT, employee, file.SubjectId, "把文件【" + file.AttachmentTitle + "】的名称修改为【"+Filename+"】。");
                        return Json(new { result = "SUCCESS", errmsg = "" });
                    }
                    else
                    {
                        return Json(new { result = "FAIL", errmsg = "当前用户没有权限修改此文件。" });
                    }
                }
                else
                {
                    return Json(new { result = "FAIL", errmsg = "操作失败，此文件已经被删除。" });
                }
            }
        }

        //上传文件
        [HttpPost]
        [OperationAuth(OperationGroup = 102)]
        public async Task<JsonResult> TM_UpLoadFile()
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Json(new { result = "FAIL", errmsg = "用户不存在。" });
            }
            else
            {
                var _file = Request.Files[0];
                var _fileLength = Request.Files[0].ContentLength;
                var _fileType = Request.Files[0].ContentType;
                var _fileName = Request.Files[0].FileName;
                if (_fileName.Contains("\\"))
                {
                    _fileName = _fileName.Substring(_fileName.LastIndexOf("\\") + 1);
                }
                int maxFileLength = 1024 * 1024 * 1024;
                if (_fileLength <= 0)
                {
                    return Json(new { result = "FAIL", errmsg = "文件大小不能为0。" });
                }
                if (_fileLength > maxFileLength)
                {
                    return Json(new { result = "FAIL", errmsg = "请上传大小少于1G的文件。" });
                }
                ContentTypeClass type = GetContentType(_fileType);
                var ServerFileName = "Subject/Files/" + employee.Id + "1001" + DateTime.Now.ToFileTime().ToString() + "." + _fileName.Substring(_fileName.LastIndexOf(".") + 1, (_fileName.Length - _fileName.LastIndexOf(".") - 1));
                try
                {
                    SubjectAttachment SA = new SubjectAttachment()
                    {
                        AttachmentTitle = _fileName,
                        AttachmentSource = ServerFileName,
                        AttachmentSize = _fileLength,
                        ContentType = _fileType,
                        AttachmentType = Convert.ToInt32(Request["AttachmentType"]),
                        UploadTime = DateTime.Now,
                        UploaderId = employee.Id,
                        SubjectId = Convert.ToInt32(Request["SubjectId"])
                    };
                    AliOSSUtilities util = new AliOSSUtilities();
                    util.PutObject(_file.InputStream, ServerFileName);
                    _db.SubjectAttachment.Add(SA);
                    await _db.SaveChangesAsync();
                    await AddLogAsync(LogCode.EDITSUBJECT, employee, Convert.ToInt32(Request["SubjectId"]), "为项目添加了文件【" + _fileName + "】。");
                }
                catch (Exception)
                {
                    return Json(new { result = "FAIL", errmsg = "请确认文件标题不超过32个字符。" });
                }
                return Json(new { result = "SUCCESS", fileurl = ServerFileName });
            }

        }

        //文件删除
        [HttpPost]
        public async Task<JsonResult> Subject_DeleteFile(int FileId)
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Json(new { result = "FAIL", errmsg = "员工不存在。" });
            }
            else
            {
                var file = _db.SubjectAttachment.SingleOrDefault(m => m.Id == FileId && m.Status>AttachmentStatus.DELETE);
                if (file!=null)
                {
                    if (file.UploaderId == employee.Id || file.Subject.HolderId == employee.Id)
                    {
                        file.Status = AttachmentStatus.DELETE;
                        _db.Entry(file).State = System.Data.Entity.EntityState.Modified;
                        await _db.SaveChangesAsync();
                        await AddLogAsync(LogCode.EDITSUBJECT, employee, file.SubjectId, "删除了文件【" + file.AttachmentTitle + "】。");
                        return Json(new { result = "SUCCESS", errmsg = "" });
                    }
                    else
                    {
                        return Json(new { result = "FAIL", errmsg = "当前用户没有权限删除此文件。" });
                    }
                }
                else
                {
                    return Json(new { result = "FAIL", errmsg = "操作失败，此文件已经被删除。" });
                }
            }
        }

        //文件json数据更新
        [HttpPost]
        [OperationAuth(OperationGroup = 102)]
        public JsonResult Subject_FilesAjax(int SubjectId)
        {
            var codelist = (from m in _db.SubjectAttachment
                            where m.SubjectId == SubjectId && m.Status > AttachmentStatus.DELETE
                            select m.AttachmentType).Distinct().ToList();
            List<SubjectFolder> sflist = new List<SubjectFolder>();
            foreach(var code in codelist)
            {
                
                var filenum = (from m in _db.SubjectAttachment
                                where m.SubjectId == SubjectId && m.AttachmentType == code && m.Status > AttachmentStatus.DELETE
                                select m).Count();
                var lastfile = (from m in _db.SubjectAttachment
                                where m.SubjectId == SubjectId && m.AttachmentType == code && m.Status > AttachmentStatus.DELETE
                                orderby m.UploadTime descending
                                select m).First();
                var ctc = GetContentType(code);
                SubjectFolder sf = new SubjectFolder()
                {
                    FolderName=ctc.Name,
                    FolderCode = code,
                    FileNum = filenum,
                    LastUpLoadUser = lastfile.Uploader.NickName,
                    LastUpLoadTime = lastfile.UploadTime.ToString("yyyy-MM-dd HH:ss")
                };
                sflist.Add(sf);
            }
            return Json(new { result="SUCCESS",data = sflist });
        }



        //文件类型
        private ContentTypeClass GetContentType(string ContentType)
        {
            ContentTypeClass item = new ContentTypeClass()
            {
                Code = ContentTypeCode.UNKNOWN.Code,
                Key = ContentTypeCode.UNKNOWN.Key,
                Name= ContentTypeCode.UNKNOWN.TypeName
            };
            int _code = ContentTypeCode.UNKNOWN.Code;
            if (ContentType.Contains(ContentTypeCode.IMAGE.Key))
            {
                item.Code = ContentTypeCode.IMAGE.Code;
                item.Key = ContentTypeCode.IMAGE.Key;
                item.Name = ContentTypeCode.IMAGE.TypeName;
            }
            else if (ContentType.Contains(ContentTypeCode.VIDEO.Key))
            {
                item.Code = ContentTypeCode.VIDEO.Code;
                item.Key = ContentTypeCode.VIDEO.Key;
                item.Name = ContentTypeCode.VIDEO.TypeName;
            }
            else if (ContentType.Contains(ContentTypeCode.AUDIO.Key))
            {
                item.Code = ContentTypeCode.AUDIO.Code;
                item.Key = ContentTypeCode.AUDIO.Key;
                item.Name = ContentTypeCode.AUDIO.TypeName;
            }
            else if (ContentType.Contains(ContentTypeCode.TEXT.EXCEL.Key))
            {
                item.Code = ContentTypeCode.TEXT.Code;
                item.Key = ContentTypeCode.TEXT.Key;
                item.Name = ContentTypeCode.TEXT.TypeName;
            }
            else if (ContentType.Contains(ContentTypeCode.TEXT.PPT.Key))
            {
                item.Code = ContentTypeCode.TEXT.Code;
                item.Key = ContentTypeCode.TEXT.Key;
                item.Name = ContentTypeCode.TEXT.TypeName;
            }
            else if (ContentType.Contains(ContentTypeCode.TEXT.WORD.Key))
            {
                item.Code = ContentTypeCode.TEXT.Code;
                item.Key = ContentTypeCode.TEXT.Key;
                item.Name = ContentTypeCode.TEXT.TypeName;
            }
            else if (ContentType.Contains(ContentTypeCode.TEXT.Key))
            {
                item.Code = ContentTypeCode.TEXT.Code;
                item.Key = ContentTypeCode.TEXT.Key;
                item.Name = ContentTypeCode.TEXT.TypeName;
            }
            else if (ContentType.Contains(ContentTypeCode.UNKNOWN.PDF.Key))
            {
                item.Code = ContentTypeCode.UNKNOWN.Code;
                item.Key = ContentTypeCode.UNKNOWN.Key;
                item.Name = ContentTypeCode.UNKNOWN.TypeName;
            }
            return item;
        }

        private ContentTypeClass GetContentType(int Code)
        {
            ContentTypeClass item = new ContentTypeClass()
            {
                Code = ContentTypeCode.UNKNOWN.Code,
                Key = ContentTypeCode.UNKNOWN.Key,
                Name = ContentTypeCode.UNKNOWN.TypeName
            };
            if (ContentTypeCode.IMAGE.Code==Code)
            {
                item.Code = ContentTypeCode.IMAGE.Code;
                item.Key = ContentTypeCode.IMAGE.Key;
                item.Name = ContentTypeCode.IMAGE.TypeName;
            }
            else if (ContentTypeCode.VIDEO.Code==Code)
            {
                item.Code = ContentTypeCode.VIDEO.Code;
                item.Key = ContentTypeCode.VIDEO.Key;
                item.Name = ContentTypeCode.VIDEO.TypeName;
            }
            else if (ContentTypeCode.AUDIO.Code==Code)
            {
                item.Code = ContentTypeCode.AUDIO.Code;
                item.Key = ContentTypeCode.AUDIO.Key;
                item.Name = ContentTypeCode.AUDIO.TypeName;
            }
            else if (ContentTypeCode.TEXT.Code==Code)
            {
                item.Code = ContentTypeCode.TEXT.Code;
                item.Key = ContentTypeCode.TEXT.Key;
                item.Name = ContentTypeCode.TEXT.TypeName;
            }            
            return item;
        }
        #endregion

        #region 日程操作  预留
        public ViewResult Subject_Agendas(int SubjectId)
        {
            var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId && m.Status > SubjectStatus.DELETED);
            if (subject == null)
            {
                return View("Error");
            }
            else
            {
                ViewBag.img = getEmployee(User.Identity.Name).ImgUrl;
                return View(subject);
            }
        }
        #endregion

        #region 我的帮助操作
        //获取我的帮助页面
        public ActionResult PersonalHelp() {
            return View();
        }
        #endregion
        #region 我的任务操作
        //获取我的任务页面
        public ActionResult PersonalActionPanel()
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Content("FAIL");
            }
            else
            {
                return PartialView(employee);
            }

        }

        //个人近期的事情
        public ActionResult PersonlRecentPartial()
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Content("FAIL");
            }
            else
            {
                return PartialView(employee);
            }
        }

        //近期任务获取
        public ActionResult PersonalRecentAssignmentPartial(int? page, string datarange, string sorttype)
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Content("FAIL");
            }
            else
            {
                int _page = page ?? 0;
                ViewBag.currentpage = _page;
                if (datarange == null|| datarange =="")
                {
                    if (sorttype == null || sorttype == "")
                    {
                        var recentassignmentlist = (from m in _db.Assignment
                                                    where m.HolderId == employee.Id&&m.Status==AssignmentStatus.UNFINISHED&&m.Subject.Status>SubjectStatus.DELETED
                                                    orderby m.CreateTime descending
                                                    select m).Skip(_page * 20).Take(20);
                        return PartialView(recentassignmentlist);
                    }
                    else
                    {
                        if (sorttype == GetDataType.DEADTIMESORTDATA)
                        {
                            var recentassignmentlist = (from m in _db.Assignment
                                                        where m.HolderId == employee.Id && m.Status == AssignmentStatus.UNFINISHED && m.Subject.Status > SubjectStatus.DELETED
                                                        orderby m.Deadline ascending
                                                        select m).Skip(_page * 20).Take(20);
                            return PartialView(recentassignmentlist);
                        }
                        else if (sorttype == GetDataType.SUBJECTSORTDATA)
                        {
                            var recentassignmentlist = (from m in _db.Assignment
                                                        where m.HolderId == employee.Id && m.Status == AssignmentStatus.UNFINISHED && m.Subject.Status > SubjectStatus.DELETED
                                                        orderby m.SubjectId ascending
                                                        select m).Skip(_page * 20).Take(20);
                            return PartialView(recentassignmentlist);
                        }
                        else
                        {
                            var recentassignmentlist = (from m in _db.Assignment
                                                        where m.HolderId == employee.Id && m.Status == AssignmentStatus.UNFINISHED && m.Subject.Status > SubjectStatus.DELETED
                                                        orderby m.CreateTime descending
                                                        select m).Skip(_page * 20).Take(20);
                            return PartialView(recentassignmentlist);
                        }
                    }
                }
                else
                {
                    TimeRangeClass _timerange = getTimeRange(datarange);
                    var starttime = _timerange.StartTime;
                    var endtime = _timerange.EndTime;
                    if (sorttype == null|| sorttype =="")
                    {
                        var recentassignmentlist = (from m in _db.Assignment
                                                    where m.HolderId == employee.Id && m.Status == AssignmentStatus.UNFINISHED&&m.Deadline>= starttime && m.Deadline<endtime && m.Subject.Status > SubjectStatus.DELETED
                                                    orderby m.CreateTime descending
                                                    select m).Skip(_page * 20).Take(20);
                        return PartialView(recentassignmentlist);
                    }
                    else
                    {
                        if (sorttype == GetDataType.DEADTIMESORTDATA)
                        {
                            var recentassignmentlist = (from m in _db.Assignment
                                                        where m.HolderId == employee.Id && m.Status == AssignmentStatus.UNFINISHED &&m.Deadline >= starttime && m.Deadline < endtime && m.Subject.Status > SubjectStatus.DELETED
                                                        orderby m.Deadline ascending
                                                        select m).Skip(_page * 20).Take(20);
                            return PartialView(recentassignmentlist);
                        }
                        else if (sorttype == GetDataType.SUBJECTSORTDATA)
                        {
                            var recentassignmentlist = (from m in _db.Assignment
                                                        where m.HolderId == employee.Id && m.Status == AssignmentStatus.UNFINISHED && m.Deadline >= starttime && m.Deadline < endtime && m.Subject.Status > SubjectStatus.DELETED
                                                        orderby m.SubjectId ascending
                                                        select m).Skip(_page * 20).Take(20);
                            return PartialView(recentassignmentlist);
                        }
                        else
                        {
                            var recentassignmentlist = (from m in _db.Assignment
                                                        where m.HolderId == employee.Id && m.Status == AssignmentStatus.UNFINISHED && m.Deadline >= starttime && m.Deadline < endtime && m.Subject.Status > SubjectStatus.DELETED
                                                        orderby m.CreateTime descending
                                                        select m).Skip(_page * 20).Take(20);
                            return PartialView(recentassignmentlist);
                        }
                    }
                }
            }
        }
        //近期子任务获取
        public ActionResult PersonalRecentSubtaskPartial(int? page, string datarange, string sorttype)
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Content("FAIL");
            }
            else
            {
                int _page = page ?? 0;
                ViewBag.currentpage = _page;
                if (datarange == null || datarange == "")
                {
                    if (sorttype == null || sorttype == "")
                    {
                        var recentassignmentlist = (from m in _db.SubTask
                                                    where m.ExecutorId== employee.Id && m.Status == AssignmentStatus.UNFINISHED && m.Assignment.Status > AssignmentStatus.DELETED&&m.Assignment.Subject.Status>SubjectStatus.DELETED
                                                    orderby m.CreateTime descending
                                                    select m).Skip(_page * 20).Take(20);
                        return PartialView(recentassignmentlist);
                    }
                    else
                    {
                        if (sorttype == GetDataType.DEADTIMESORTDATA)
                        {
                            var recentassignmentlist = (from m in _db.SubTask
                                                        where m.ExecutorId == employee.Id && m.Status == AssignmentStatus.UNFINISHED && m.Assignment.Status > AssignmentStatus.DELETED && m.Assignment.Subject.Status > SubjectStatus.DELETED
                                                        orderby m.Deadline ascending
                                                        select m).Skip(_page * 20).Take(20);
                            return PartialView(recentassignmentlist);
                        }
                        else if (sorttype == GetDataType.SUBJECTSORTDATA)
                        {
                            var recentassignmentlist = (from m in _db.SubTask
                                                        where m.ExecutorId== employee.Id && m.Status == AssignmentStatus.UNFINISHED && m.Assignment.Status > AssignmentStatus.DELETED && m.Assignment.Subject.Status > SubjectStatus.DELETED
                                                        orderby m.Assignment.SubjectId ascending
                                                        select m).Skip(_page * 20).Take(20);
                            return PartialView(recentassignmentlist);
                        }
                        else
                        {
                            var recentassignmentlist = (from m in _db.SubTask
                                                        where m.ExecutorId == employee.Id && m.Status == AssignmentStatus.UNFINISHED && m.Assignment.Status > AssignmentStatus.DELETED && m.Assignment.Subject.Status > SubjectStatus.DELETED
                                                        orderby m.CreateTime descending
                                                        select m).Skip(_page * 20).Take(20);
                            return PartialView(recentassignmentlist);
                        }
                    }
                }
                else
                {
                    TimeRangeClass _timerange = getTimeRange(datarange);
                    var starttime = _timerange.StartTime;
                    var endtime = _timerange.EndTime;
                    if (sorttype == null || sorttype == "")
                    {
                        var recentassignmentlist = (from m in _db.SubTask
                                                    where m.ExecutorId == employee.Id && m.Status == AssignmentStatus.UNFINISHED && m.Deadline >= starttime && m.Deadline < endtime && m.Assignment.Status > AssignmentStatus.DELETED && m.Assignment.Subject.Status > SubjectStatus.DELETED
                                                    orderby m.CreateTime descending
                                                    select m).Skip(_page * 20).Take(20);
                        return PartialView(recentassignmentlist);
                    }
                    else
                    {
                        if (sorttype == GetDataType.DEADTIMESORTDATA)
                        {
                            var recentassignmentlist = (from m in _db.SubTask
                                                        where m.ExecutorId == employee.Id && m.Status == AssignmentStatus.UNFINISHED && m.Deadline >= starttime && m.Deadline < endtime && m.Assignment.Status > AssignmentStatus.DELETED && m.Assignment.Subject.Status > SubjectStatus.DELETED
                                                        orderby m.Deadline ascending
                                                        select m).Skip(_page * 20).Take(20);
                            return PartialView(recentassignmentlist);
                        }
                        else if (sorttype == GetDataType.SUBJECTSORTDATA)
                        {
                            var recentassignmentlist = (from m in _db.SubTask
                                                        where m.ExecutorId == employee.Id && m.Status == AssignmentStatus.UNFINISHED && m.Deadline >= starttime && m.Deadline < endtime && m.Assignment.Status > AssignmentStatus.DELETED && m.Assignment.Subject.Status > SubjectStatus.DELETED
                                                        orderby m.Assignment.SubjectId ascending
                                                        select m).Skip(_page * 20).Take(20);
                            return PartialView(recentassignmentlist);
                        }
                        else
                        {
                            var recentassignmentlist = (from m in _db.SubTask
                                                        where m.ExecutorId == employee.Id && m.Status == AssignmentStatus.UNFINISHED && m.Deadline >= starttime && m.Deadline < endtime && m.Assignment.Status > AssignmentStatus.DELETED && m.Assignment.Subject.Status > SubjectStatus.DELETED
                                                        orderby m.CreateTime descending
                                                        select m).Skip(_page * 20).Take(20);
                            return PartialView(recentassignmentlist);
                        }
                    }
                }
            }
        }

        //参与的任务获取
        public ActionResult PersonalInvolvedAssignmentPartial(int?page,string datarange,string sorttype)
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Content("FAIL");
            }
            else
            {
                int _page = page ?? 0;
                ViewBag.currentpage = _page;
                var HolderAssignment = (from m in _db.Assignment
                                       where m.HolderId == employee.Id&&m.Status>AssignmentStatus.DELETED&&m.Subject.Status>SubjectStatus.DELETED
                                       select m).ToList();
                var ColAssigenment = from m in employee.CollaborateAssignment
                                     where m.Status > AssignmentStatus.DELETED && m.Subject.Status > SubjectStatus.DELETED
                                     select m;
                var ConsolidationList = HolderAssignment.Union(ColAssigenment);
                if (datarange == null || datarange == "")
                {
                    if (sorttype == null || sorttype == "")
                    {
                        var ResultList = (from m in ConsolidationList
                                          orderby m.CreateTime ascending
                                          select m).Skip(_page * 20).Take(20);
                        return PartialView(ResultList);

                    }
                    else
                    {
                        if (sorttype == GetDataType.DEADTIMESORTDATA)
                        {
                            var ResultList = (from m in ConsolidationList
                                              orderby m.Deadline ascending
                                              select m).Skip(_page * 20).Take(20);
                            return PartialView(ResultList);

                        }
                        else if (sorttype == GetDataType.SUBJECTSORTDATA)
                        {
                            var ResultList = (from m in ConsolidationList
                                              orderby m.SubjectId ascending
                                              select m).Skip(_page * 20).Take(20);
                            return PartialView(ResultList);
                        }
                        else
                        {
                            var ResultList = (from m in ConsolidationList
                                              orderby m.Id ascending
                                              select m).Skip(_page * 20).Take(20);
                            return PartialView(ResultList);
                        }


                    }
                }
                else
                {
                    if (sorttype == null || sorttype == "")
                    {
                        if (datarange == GetDataType.UNFINISHDATA)
                        {
                            var ResultList = (from m in ConsolidationList
                                              where m.Status==AssignmentStatus.UNFINISHED
                                              orderby m.CreateTime ascending
                                              select m).Skip(_page * 20).Take(20);
                            return PartialView(ResultList);
                        }
                        else
                        {
                            var ResultList = (from m in ConsolidationList
                                              where m.Status == AssignmentStatus.FINISHED
                                              orderby m.CreateTime ascending
                                              select m).Skip(_page * 20).Take(20);
                            return PartialView(ResultList);
                        }
                    }
                    else
                    {
                        if (datarange == GetDataType.UNFINISHDATA)
                        {
                            if (sorttype == GetDataType.DEADTIMESORTDATA)
                            {
                                var ResultList = (from m in ConsolidationList
                                                  where m.Status == AssignmentStatus.UNFINISHED
                                                  orderby m.Deadline ascending
                                                  select m).Skip(_page * 20).Take(20);
                                return PartialView(ResultList);

                            }
                            else if (sorttype == GetDataType.SUBJECTSORTDATA)
                            {
                                var ResultList = (from m in ConsolidationList
                                                  where m.Status == AssignmentStatus.UNFINISHED
                                                  orderby m.SubjectId ascending
                                                  select m).Skip(_page * 20).Take(20);
                                return PartialView(ResultList);
                            }
                            else
                            {
                                var ResultList = (from m in ConsolidationList
                                                  where m.Status == AssignmentStatus.UNFINISHED
                                                  orderby m.Id ascending
                                                  select m).Skip(_page * 20).Take(20);
                                return PartialView(ResultList);
                            }
                        }
                        else
                        {
                            if (sorttype == GetDataType.DEADTIMESORTDATA)
                            {
                                var ResultList = (from m in ConsolidationList
                                                  where m.Status == AssignmentStatus.FINISHED
                                                  orderby m.Deadline ascending
                                                  select m).Skip(_page * 20).Take(20);
                                return PartialView(ResultList);

                            }
                            else if (sorttype == GetDataType.SUBJECTSORTDATA)
                            {
                                var ResultList = (from m in ConsolidationList
                                                  where m.Status == AssignmentStatus.FINISHED
                                                  orderby m.SubjectId ascending
                                                  select m).Skip(_page * 20).Take(20);
                                return PartialView(ResultList);
                            }
                            else
                            {
                                var ResultList = (from m in ConsolidationList
                                                  where m.Status == AssignmentStatus.FINISHED
                                                  orderby m.Id ascending
                                                  select m).Skip(_page * 20).Take(20);
                                return PartialView(ResultList);
                            }
                        }
                    }

                }

            }
        }
        //负责的任务获取
        public ActionResult PersonalResponsibleAssignmentPartial(int?page,string datarange,string sorttype)
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Content("FAIL");
            }
            else
            {
                int _page = page ?? 0;
                ViewBag.currentpage = _page;
                if (datarange == null || datarange == "")
                {
                    if (sorttype == null || sorttype == "")
                    {
                        var ResultList = (from m in _db.Assignment
                                          where m.HolderId == employee.Id && m.Subject.Status > SubjectStatus.DELETED && m.Status > AssignmentStatus.DELETED
                                          orderby m.CreateTime ascending
                                          select m).Skip(_page * 20).Take(20);
                        return PartialView(ResultList);

                    }
                    else
                    {
                        if (sorttype == GetDataType.DEADTIMESORTDATA)
                        {
                            var ResultList = (from m in _db.Assignment
                                              where m.HolderId == employee.Id && m.Subject.Status > SubjectStatus.DELETED && m.Status > AssignmentStatus.DELETED
                                              orderby m.Deadline ascending
                                              select m).Skip(_page * 20).Take(20);
                            return PartialView(ResultList);

                        }
                        else if (sorttype == GetDataType.SUBJECTSORTDATA)
                        {
                            var ResultList = (from m in _db.Assignment
                                              where m.HolderId == employee.Id && m.Subject.Status > SubjectStatus.DELETED && m.Status > AssignmentStatus.DELETED
                                              orderby m.SubjectId ascending
                                              select m).Skip(_page * 20).Take(20);
                            return PartialView(ResultList);
                        }
                        else
                        {
                            var ResultList = (from m in _db.Assignment
                                              where m.HolderId == employee.Id && m.Subject.Status > SubjectStatus.DELETED && m.Status > AssignmentStatus.DELETED
                                              orderby m.Id ascending
                                              select m).Skip(_page * 20).Take(20);
                            return PartialView(ResultList);
                        }


                    }
                }
                else
                {
                    if (sorttype == null || sorttype == "")
                    {
                        if (datarange == GetDataType.UNFINISHDATA)
                        {
                            var ResultList = (from m in _db.Assignment
                                              where m.HolderId == employee.Id&&m.Status == AssignmentStatus.UNFINISHED && m.Subject.Status > SubjectStatus.DELETED
                                              orderby m.CreateTime ascending
                                              select m).Skip(_page * 20).Take(20);
                            return PartialView(ResultList);
                        }
                        else
                        {
                            var ResultList = (from m in _db.Assignment
                                              where m.HolderId == employee.Id&&m.Status == AssignmentStatus.FINISHED && m.Subject.Status > SubjectStatus.DELETED
                                              orderby m.CreateTime ascending
                                              select m).Skip(_page * 20).Take(20);
                            return PartialView(ResultList);
                        }
                    }
                    else
                    {
                        if (datarange == GetDataType.UNFINISHDATA)
                        {
                            if (sorttype == GetDataType.DEADTIMESORTDATA)
                            {
                                var ResultList = (from m in _db.Assignment
                                                  where m.HolderId == employee.Id && m.Status == AssignmentStatus.UNFINISHED && m.Subject.Status > SubjectStatus.DELETED
                                                  orderby m.Deadline ascending
                                                  select m).Skip(_page * 20).Take(20);
                                return PartialView(ResultList);

                            }
                            else if (sorttype == GetDataType.SUBJECTSORTDATA)
                            {
                                var ResultList = (from m in _db.Assignment
                                                  where m.HolderId == employee.Id&& m.Status == AssignmentStatus.UNFINISHED && m.Subject.Status > SubjectStatus.DELETED
                                                  orderby m.SubjectId ascending
                                                  select m).Skip(_page * 20).Take(20);
                                return PartialView(ResultList);
                            }
                            else
                            {
                                var ResultList = (from m in _db.Assignment
                                                  where m.HolderId == employee.Id && m.Status == AssignmentStatus.UNFINISHED && m.Subject.Status > SubjectStatus.DELETED
                                                  orderby m.Id ascending
                                                  select m).Skip(_page * 20).Take(20);
                                return PartialView(ResultList);
                            }
                        }
                        else
                        {
                            if (sorttype == GetDataType.DEADTIMESORTDATA)
                            {
                                var ResultList = (from m in _db.Assignment
                                                  where m.HolderId == employee.Id && m.Status == AssignmentStatus.FINISHED && m.Subject.Status > SubjectStatus.DELETED
                                                  orderby m.Deadline ascending
                                                  select m).Skip(_page * 20).Take(20);
                                return PartialView(ResultList);

                            }
                            else if (sorttype == GetDataType.SUBJECTSORTDATA)
                            {
                                var ResultList = (from m in _db.Assignment
                                                  where m.HolderId == employee.Id && m.Status == AssignmentStatus.FINISHED && m.Subject.Status > SubjectStatus.DELETED
                                                  orderby m.SubjectId ascending
                                                  select m).Skip(_page * 20).Take(20);
                                return PartialView(ResultList);
                            }
                            else
                            {
                                var ResultList = (from m in _db.Assignment
                                                  where m.HolderId == employee.Id && m.Status == AssignmentStatus.FINISHED && m.Subject.Status > SubjectStatus.DELETED
                                                  orderby m.Id ascending
                                                  select m).Skip(_page * 20).Take(20);
                                return PartialView(ResultList);
                            }
                        }
                    }

                }

            }
        }
        //执行的任务获取
        public ActionResult PersonalExecutableTaskPartial(int?page,string datarange,string sorttype)
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Content("FAIL");
            }
            else
            {
                int _page = page ?? 0;
                ViewBag.currentpage = _page;
                if (datarange == null || datarange == "")
                {
                    if (sorttype == null || sorttype == "")
                    {
                        var ResultList = (from m in _db.SubTask
                                          where m.ExecutorId == employee.Id&&m.Assignment.Status>AssignmentStatus.DELETED&&m.Assignment.Subject.Status>SubjectStatus.DELETED
                                          orderby m.CreateTime ascending
                                          select m).Skip(_page * 20).Take(20);
                        return PartialView(ResultList);

                    }
                    else
                    {
                        if (sorttype == GetDataType.DEADTIMESORTDATA)
                        {
                            var ResultList = (from m in _db.SubTask
                                              where m.ExecutorId== employee.Id && m.Assignment.Status > AssignmentStatus.DELETED && m.Assignment.Subject.Status > SubjectStatus.DELETED
                                              orderby m.Deadline ascending
                                              select m).Skip(_page * 20).Take(20);
                            return PartialView(ResultList);

                        }
                        else if (sorttype == GetDataType.SUBJECTSORTDATA)
                        {
                            var ResultList = (from m in _db.SubTask
                                              where m.ExecutorId == employee.Id && m.Assignment.Status > AssignmentStatus.DELETED && m.Assignment.Subject.Status > SubjectStatus.DELETED
                                              orderby m.Assignment.SubjectId ascending
                                              select m).Skip(_page * 20).Take(20);
                            return PartialView(ResultList);
                        }
                        else
                        {
                            var ResultList = (from m in _db.SubTask
                                              where m.ExecutorId == employee.Id && m.Assignment.Status > AssignmentStatus.DELETED && m.Assignment.Subject.Status > SubjectStatus.DELETED
                                              orderby m.CreateTime ascending
                                              select m).Skip(_page * 20).Take(20);
                            return PartialView(ResultList);
                        }


                    }
                }
                else
                {
                    if (sorttype == null || sorttype == "")
                    {
                        if (datarange == GetDataType.UNFINISHDATA)
                        {
                            var ResultList = (from m in _db.SubTask
                                              where m.ExecutorId== employee.Id && m.Status == AssignmentStatus.UNFINISHED && m.Assignment.Status > AssignmentStatus.DELETED && m.Assignment.Subject.Status > SubjectStatus.DELETED
                                              orderby m.CreateTime ascending
                                              select m).Skip(_page * 20).Take(20);
                            return PartialView(ResultList);
                        }
                        else
                        {
                            var ResultList = (from m in _db.SubTask
                                              where m.ExecutorId == employee.Id && m.Status == AssignmentStatus.FINISHED && m.Assignment.Status > AssignmentStatus.DELETED && m.Assignment.Subject.Status > SubjectStatus.DELETED
                                              orderby m.CreateTime ascending
                                              select m).Skip(_page * 20).Take(20);
                            return PartialView(ResultList);
                        }
                    }
                    else
                    {
                        if (datarange == GetDataType.UNFINISHDATA)
                        {
                            if (sorttype == GetDataType.DEADTIMESORTDATA)
                            {
                                var ResultList = (from m in _db.SubTask
                                                  where m.ExecutorId== employee.Id && m.Status == AssignmentStatus.UNFINISHED && m.Assignment.Status > AssignmentStatus.DELETED && m.Assignment.Subject.Status > SubjectStatus.DELETED
                                                  orderby m.Deadline ascending
                                                  select m).Skip(_page * 20).Take(20);
                                return PartialView(ResultList);

                            }
                            else if (sorttype == GetDataType.SUBJECTSORTDATA)
                            {
                                var ResultList = (from m in _db.SubTask
                                                  where m.ExecutorId == employee.Id && m.Status == AssignmentStatus.UNFINISHED && m.Assignment.Status > AssignmentStatus.DELETED && m.Assignment.Subject.Status > SubjectStatus.DELETED
                                                  orderby m.Assignment.SubjectId ascending
                                                  select m).Skip(_page * 20).Take(20);
                                return PartialView(ResultList);
                            }
                            else
                            {
                                var ResultList = (from m in _db.SubTask
                                                  where m.ExecutorId== employee.Id && m.Status == AssignmentStatus.UNFINISHED && m.Assignment.Status > AssignmentStatus.DELETED && m.Assignment.Subject.Status > SubjectStatus.DELETED
                                                  orderby m.AssignmentId ascending
                                                  select m).Skip(_page * 20).Take(20);
                                return PartialView(ResultList);
                            }
                        }
                        else
                        {
                            if (sorttype == GetDataType.DEADTIMESORTDATA)
                            {
                                var ResultList = (from m in _db.SubTask
                                                  where m.ExecutorId== employee.Id && m.Status == AssignmentStatus.FINISHED && m.Assignment.Status > AssignmentStatus.DELETED && m.Assignment.Subject.Status > SubjectStatus.DELETED
                                                  orderby m.Deadline ascending
                                                  select m).Skip(_page * 20).Take(20);
                                return PartialView(ResultList);

                            }
                            else if (sorttype == GetDataType.SUBJECTSORTDATA)
                            {
                                var ResultList = (from m in _db.SubTask
                                                  where m.ExecutorId == employee.Id && m.Status == AssignmentStatus.FINISHED && m.Assignment.Status > AssignmentStatus.DELETED && m.Assignment.Subject.Status > SubjectStatus.DELETED
                                                  orderby m.Assignment.SubjectId ascending
                                                  select m).Skip(_page * 20).Take(20);
                                return PartialView(ResultList);
                            }
                            else
                            {
                                var ResultList = (from m in _db.SubTask
                                                  where m.ExecutorId == employee.Id && m.Status == AssignmentStatus.FINISHED && m.Assignment.Status > AssignmentStatus.DELETED && m.Assignment.Subject.Status > SubjectStatus.DELETED
                                                  orderby m.AssignmentId ascending
                                                  select m).Skip(_page * 20).Take(20);
                                return PartialView(ResultList);
                            }
                        }
                    }

                }

            }
        }
        //文件获取

        //个人任务
        public ActionResult PersonalTaskPartial()
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Content("FAIL");
            }
            else
            {
                return PartialView(employee);
            }
        }

        //个人文件
        public ActionResult PersonalFilePartial()
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Content("FAIL");
            }
            else
            {
                return PartialView(employee);
            }
        }

        //个人收藏
        public ActionResult PersonalCollectionPartial()
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Content("FAIL");
            }
            else
            {
                return PartialView(employee);
            }
        }
        #endregion


        #region  日历
        //日历页面
        public ActionResult SubjectCalendarPanel()
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Content("FAIL");
            }
            else
            {
                return PartialView(employee);
            }
        }

        //获取用户每月的所有任务数据
        [HttpPost]
        public JsonResult  EmployeeMonthDataOfTask(int month,int year)
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return Json(new { result = "FAIL", errmsg="获取月数据失败,用户不存在!" });
            }
            else
            {
                DateTime starttime = new DateTime(year, month, 1);
                DateTime endtime = starttime.AddMonths(1);
                var holderassignmentlist = (from m in _db.Assignment
                                            where m.HolderId == employee.Id && m.Status == AssignmentStatus.UNFINISHED && m.Subject.Status == SubjectStatus.ACTIVE && m.Deadline >= starttime && m.Deadline < endtime
                                            select new { Id = m.Id, Name = m.AssignmentTitle, Type = "任务", TypeCode = 1, DeadTime = m.Deadline}).ToList();
                var subtasklist = (from m in _db.SubTask
                                   where m.ExecutorId == employee.Id && m.Status == AssignmentStatus.UNFINISHED && m.Assignment.Status == AssignmentStatus.UNFINISHED && m.Assignment.Subject.Status == SubjectStatus.ACTIVE && m.Deadline >= starttime && m.Deadline < endtime
                                   select new { Id = m.Id, Name = m.TaskTitle, Type = "子任务", TypeCode = 2,DeadTime=m.Deadline }).ToList();
                var resultlist = holderassignmentlist.Concat(subtasklist).ToList();
                List<AssignmentInfoClass> hal = new List<AssignmentInfoClass>();
                foreach(var item in resultlist)
                {
                    AssignmentInfoClass t = new AssignmentInfoClass()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Type = item.Type,
                        TypeCode = item.TypeCode,
                        DeadTime = Convert.ToDateTime(item.DeadTime).ToString("yyyy-MM-dd"),
                        Hour=Convert.ToDateTime(item.DeadTime).Hour,
                        Minute= Convert.ToDateTime(item.DeadTime).Minute,

                    };
                    hal.Add(t);
                }
                return Json(new { result = "SUCCESS", msg = "获取月数据成功。",data= hal });
            }
        }

        #endregion


        #region  数据EXCEL导出操作
        public ActionResult GetExcelDataForAssignment()
        {
            var employee = getEmployee(User.Identity.Name);
            if (employee == null)
            {
                return View("Error");
            }
            else
            {
                DataTable dt = new DataTable();
                DataColumn dc = null;
                dc = dt.Columns.Add("序号", Type.GetType("System.Int32"));
                dc.AutoIncrement = true;//自动增加
                dc.AutoIncrementSeed = 1;//起始为1
                dc.AutoIncrementStep = 1;//步长为1
                dc.AllowDBNull = false;//
                dc = dt.Columns.Add("名称", Type.GetType("System.String"));
                dc = dt.Columns.Add("任务状态", Type.GetType("System.String"));
                dc = dt.Columns.Add("创建时间", Type.GetType("System.String"));
                dc = dt.Columns.Add("截止时间", Type.GetType("System.String"));
                dc = dt.Columns.Add("完成时间", Type.GetType("System.String"));
                dc = dt.Columns.Add("提醒时间", Type.GetType("System.String"));
                dc = dt.Columns.Add("备注", Type.GetType("System.String"));
                dc = dt.Columns.Add("负责人", Type.GetType("System.String"));
                dc = dt.Columns.Add("项目归属", Type.GetType("System.String"));
                dc = dt.Columns.Add("项目状态", Type.GetType("System.String"));
                dc = dt.Columns.Add("任务类型", Type.GetType("System.String"));
                DataRow newRow;
                var assignementlist = (from m in _db.Assignment
                                      where m.HolderId == employee.Id
                                      select m).ToList();
                var collist = from m in employee.CollaborateAssignment
                              select m;
                var asslist = assignementlist.Union(collist);
                foreach(var item in asslist)
                {
                    newRow = dt.NewRow();
                    newRow["名称"] = item.AssignmentTitle;
                    newRow["任务状态"] = (item.Status == -1 ? "已删除" : item.Status == 0 ? "已归档" : item.Status == 1 ? "未完成" : "已完成");
                    newRow["创建时间"] = item.CreateTime;
                    newRow["截止时间"] = item.Deadline;
                    newRow["完成时间"] = item.CompleteDate;
                    newRow["提醒时间"] = item.RemindDate;
                    newRow["备注"] = item.Remarks;
                    newRow["负责人"] = item.Holder.NickName;
                    newRow["项目归属"] = item.Subject.SubjectTitle;
                    newRow["项目状态"] = (item.Subject.Status == -1 ? "已删除" : item.Subject.Status == 0 ? "已归档" : item.Subject.Status == 1 ? "进行中" : "未知");
                    newRow["任务类型"] = "任务";
                    dt.Rows.Add(newRow);
                }
                var subtasklist = from m in _db.SubTask
                                  where m.ExecutorId == employee.Id
                                  select m;
                foreach (var item in subtasklist)
                {
                    newRow = dt.NewRow();
                    newRow["名称"] = item.TaskTitle;
                    newRow["任务状态"] = (item.Status == -1 ? "已删除" : item.Status == 0 ? "已归档" : item.Status == 1 ? "未完成" : "已完成");
                    newRow["创建时间"] = item.CreateTime;
                    newRow["截止时间"] = item.Deadline;
                    newRow["完成时间"] = item.CompleteDate;
                    newRow["提醒时间"] = item.RemindDate;
                    newRow["备注"] = item.Remarks;
                    newRow["负责人"] = item.Executor.NickName;
                    newRow["项目归属"] = item.Assignment.Subject.SubjectTitle;
                    newRow["项目状态"] = (item.Assignment.Subject.Status == -1 ? "已删除" : item.Assignment.Subject.Status == 0 ? "已归档" : item.Assignment.Subject.Status == 1 ? "进行中" : "未知");
                    newRow["任务类型"] = "子任务";
                    dt.Rows.Add(newRow);
                }
                HSSFWorkbook workbook = new HSSFWorkbook();
                dt.TableName = "任务列表";
                ISheet sheet = string.IsNullOrEmpty(dt.TableName) ? workbook.CreateSheet("Sheet1") : workbook.CreateSheet(dt.TableName);
                IRow row = sheet.CreateRow(0);
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    ICell cell = row.CreateCell(i);
                    cell.SetCellValue(dt.Columns[i].ColumnName);
                }

                //数据  
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    IRow row1 = sheet.CreateRow(i + 1);
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        ICell cell = row1.CreateCell(j);
                        cell.SetCellValue(dt.Rows[i][j].ToString());
                    }
                }

                //转为字节数组  
                MemoryStream _stream = new MemoryStream();
                workbook.Write(_stream);
                _stream.Flush();
                _stream.Seek(0, SeekOrigin.Begin);
                return File(_stream, "application/ms-excel", employee.NickName+"的任务.xls");
            };
        }
        #endregion


        //获取项目侧边栏
        [OperationAuth(OperationGroup = 102)]
        public PartialViewResult SubjectMenuPanelPartial(int SubjectId)
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
        [OperationAuth(OperationGroup = 102)]
        public PartialViewResult SubjectProgressPartial(int SubjectId)
        {
            var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId);
            return PartialView(subject);
        }


        //获取项目今日完成进度
        [OperationAuth(OperationGroup = 102)]
        public PartialViewResult SubjectFinishProgressPartial(int SubjectId)
        {
            var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId);
            var AssignmentList = from m in subject.Assignment
                                 where m.Status == AssignmentStatus.FINISHED && m.CompleteDate >= DateTime.Today && m.CompleteDate < DateTime.Today.AddDays(1)
                                 select m;
            return PartialView(AssignmentList);
        }

        //获取项目今日待完成进度
        [OperationAuth(OperationGroup = 102)]
        public PartialViewResult SubjectUntreatedProgressPartial(int SubjectId)
        {
            var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId);
            var AssignmentList = from m in subject.Assignment
                                 where m.Status == AssignmentStatus.UNFINISHED && (m.RemindDate >= DateTime.Today && m.RemindDate < DateTime.Today.AddDays(1) || m.Deadline >= DateTime.Today && m.Deadline < DateTime.Today.AddDays(1))
                                 select m;
            return PartialView(AssignmentList);
        }












        [HttpPost]
        public JsonResult QuickSearch(string input)
        {
            var employee = getEmployee(User.Identity.Name);
            var subjectlist = (from m in _db.Subject
                               where m.SubjectTitle.Contains(input) && m.HolderId == employee.Id && m.Status > SubjectStatus.ARCHIVED
                               select new { id = m.Id, name = m.SubjectTitle, type = 0 }).ToList();
            var tasklist = (from m in _db.Assignment
                            where m.AssignmentTitle.Contains(input) && m.HolderId == employee.Id && m.Status > AssignmentStatus.ARCHIVED && m.Subject.Status > SubjectStatus.ARCHIVED
                            select new { id = m.Id, name = m.AssignmentTitle, type = 1,taskbelong = m.Subject.SubjectTitle }).ToList();
            var subtasklist = (from m in _db.SubTask
                               where m.TaskTitle.Contains(input) && m.ExecutorId == employee.Id && m.Status > AssignmentStatus.DELETED && m.Assignment.Status> AssignmentStatus.DELETED && m.Assignment.Subject.Status> SubjectStatus.ARCHIVED
                               select new { id = m.Id, name = m.TaskTitle, type = 2 ,subtbelong=m.Assignment.AssignmentTitle}).ToList();
            return Json(new { subject = subjectlist, task = tasklist, subtask = subtasklist });
        }










        public ActionResult GetLogs(int? employeeId, int? SubjectId, string logType, string logTime, int _page)
        {
            int _subjectId = SubjectId ?? 0;
            int _employeeId = employeeId ?? 0;
            ViewBag.currentshow = _page;
            if (_employeeId!=0)
            {
                if (_subjectId!=0)
                {
                    var a = getTypeRange(logType);
                    var b = getTimeRange(logTime);
                    var getLogsByTime = (from m in _db.OperationLogs
                                         where m.UserId == _employeeId && m.SubjectId==_subjectId && m.LogCode <= a.MaxValue && m.LogCode >= a.MinValue && m.LogTime >= b.StartTime && m.LogTime < b.EndTime
                                         orderby m.LogTime descending
                                         select m).Skip(_page * 9).Take(9);
                    return PartialView(getLogsByTime);

                }
                else {
                    return Content("项目不明确！");
                }
            }
            else {
                return Content("负责人不明确！");
            }
            

        }

        public TypeRangeClass getTypeRange(string type) {
            TypeRangeClass item = new TypeRangeClass();
            if (type == GetDataType.SUBJECTDATA)
            {
                item.MinValue = 101;
                item.MaxValue = 112;
            }
            else if (type == GetDataType.MISSIONTDATA)
            {
                item.MinValue = 105;
                item.MaxValue = 108;
            }
            else if (type == GetDataType.SUBMISSIONDATA) {
                item.MinValue = 109;
                item.MaxValue = 112;
            }
            return item;
        }

        public TimeRangeClass getTimeRange(string timetype)
        {
            TimeRangeClass item = new TimeRangeClass();
            var _today = DateTime.Today;
            if (timetype == GetDataType.WEEKDATA)
            {
                var startweek = _today.AddDays(1 - Convert.ToInt32(_today.DayOfWeek.ToString("d")));
                var endweek = startweek.AddDays(7);
                item.StartTime = startweek;
                item.EndTime = endweek;
            }
            else if (timetype == GetDataType.MONTHDATA)
            {
                var startmonth = _today.AddDays(1 - _today.Day);
                var endmonth = startmonth.AddMonths(1);
                item.StartTime = startmonth;
                item.EndTime = endmonth;

            }
            else if (timetype == GetDataType.YEARDATA)
            {
                var startyear = new DateTime(_today.Year, 1, 1);
                var endyear = startyear.AddYears(1);
                item.StartTime = startyear;
                item.EndTime = endyear;
            }
            else if (timetype == GetDataType.DAYDATA)
            {
                item.StartTime = _today;
                item.EndTime = _today.AddDays(1);
            }
            else if(timetype == GetDataType.ALLDATA)
            {
                var startyear = new DateTime(2016, 1, 1);
                var currentyear = new DateTime(_today.Year, 1, 1);           
                var endyear = currentyear.AddYears(1);
            }
            return item;
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







    }

}