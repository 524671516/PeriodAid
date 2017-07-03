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
                return RedirectToAction("TaskManagement", "TaskManagerLogin");
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

        [HttpPost,ValidateAntiForgeryToken]
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
            return Json(new { result = "模型错误。" });
        }

        #region  创建项目
        /// <summary>
        /// 创建项目
        /// </summary>
        /// <returns>ActionResult</returns>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateSubject(FormCollection form, Subject model)
        {
            /* 为统一，最好返回JSON，然后JS里面统一处理 */
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
                        item.Status = SubjectStatus.ACTIVE;
                        var defaultTemplate = _db.ProcedureTemplate.SingleOrDefault(m => m.Id == item.TemplateId);         
                        if (defaultTemplate==null)
                        {
                            defaultTemplate= _db.ProcedureTemplate.SingleOrDefault(m => m.Default==true);
                        }
                        ProcedureTemplate template = new ProcedureTemplate()
                        {
                            Title = defaultTemplate.Title
                        };
                        try
                        {
                            _db.ProcedureTemplate.Add(template);
                            _db.SaveChanges();
                            foreach (var pro in defaultTemplate.Procedure)
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
                            return Content("项目模板建立失败。");
                        }
                        item.TemplateId = template.Id;
                        item.CreateTime = DateTime.Now;
                        try
                        {
                            _db.Subject.Add(item);
                            await _db.SaveChangesAsync();
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

        #region 编辑项目
        public ActionResult EditSubject(int id)
        {
            var employee = getEmployee(User.Identity.Name);
            var subject = _db.Subject.SingleOrDefault(m => m.Id == id && m.HolderId == employee.Id);
            if (subject != null)
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
        public async Task<ActionResult> EditSubject(Subject model)
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
                        await _db.SaveChangesAsync();
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
                    return Json(new { result = "SUCCESS", errmsg = "归档项目成功" });
                }
                return Json(new { result = "FAIL", errmsg = "当前用户没有权限归档项目" });
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
                    return Json(new { result = "SUCCESS", errmsg = "恢复项目成功" });
                }
                return Json(new { result = "FAIL", errmsg = "当前用户没有权限恢复项目" });
            }
        }
        #endregion

        #region 上传封面

        #endregion

        #region  获取星标项目  占时无用
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
                var ownSubject = employee.Subject.Where(m => m.Status == SubjectStatus.ACTIVE);
                // 自己的任务列表
                var SubjectList = (from m in employee.CollaborateAssignment
                                  where m.Status > AssignmentStatus.DELETED
                                  select m.Subject).Where(p=>p.Status==SubjectStatus.ACTIVE);
                var HolderSubject = (from m in _db.Assignment
                                 where m.HolderId == employee.Id && m.Status > AssignmentStatus.DELETED
                                 select m.Subject).Where(p=>p.Status==SubjectStatus.ACTIVE);
                var FirstMerge= ownSubject.Union(SubjectList);
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
                var ownSubject = employee.Subject.Where(m => m.Status == SubjectStatus.ARCHIVED);
                // 自己的任务列表
                var SubjectList = (from m in employee.CollaborateAssignment
                                  where m.Status > AssignmentStatus.DELETED
                                  select m.Subject).Where(p=>p.Status==SubjectStatus.ARCHIVED);
                var HolderSubject = (from m in _db.Assignment
                                    where m.HolderId == employee.Id && m.Status > AssignmentStatus.DELETED
                                    select m.Subject).Where(p=>p.Status==SubjectStatus.ARCHIVED);
                var FirstMerge = ownSubject.Union(SubjectList);
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
            ViewBag.sortprocedure = from m in subject.ProcedureTemplate.Procedure
                                    where m.Status == ProcedureStatus.NORMAL
                                    orderby m.Sort ascending
                                    select m;
            ViewBag.img = getEmployee(User.Identity.Name).ImgUrl;
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
            var procedure = _db.Procedure.SingleOrDefault(m => m.Id == ProcedureId && m.Status == ProcedureStatus.NORMAL);
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
        #endregion

        #region 项目过程中的任务
        /// <summary>
        /// 项目过程中的任务
        /// </summary>
        /// <param name="ProcedureId"></param>
        /// <returns>集合</returns>
        public PartialViewResult SubjectAssignment(int ProcedureId, int SubJectId)
        {
            var assignmentlist = from m in _db.Assignment
                                 where m.ProcedureId == ProcedureId && m.Status >= 0 && m.SubjectId == SubJectId && m.Status > AssignmentStatus.DELETED
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
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult CreateProcedure(FormCollection form, Procedure model)
        {
            if (ModelState.IsValid)
            {
                Procedure item = new Procedure();
                if (TryUpdateModel(item))
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
        public JsonResult Delete_Procedure(int ProcedureId)
        {
            var procedure = _db.Procedure.SingleOrDefault(m => m.Id == ProcedureId && m.Status == ProcedureStatus.NORMAL);
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
                    return Json(new { result = "发生错误。" });
                }
                return Json(new { result = "SUCCESS", Id = procedure.Id });
            }
            else
            {
                return Json(new { result = "请先清空列表中的任务。" });
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
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult CreateAssignment(FormCollection form, Assignment model)
        {
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
                        _db.SaveChanges();
                        //var ass = _db.Assignment.SingleOrDefault(m => m.Id == item.Id && m.Status > AssignmentStatus.DELETED);
                        //var employee = _db.Employee.SingleOrDefault(m => m.Id == item.HolderId);
                        //List<Employee> clist = new List<Employee>();
                        //ass.Collaborator = clist;
                        //ass.Collaborator.Add(employee);
                        //_db.Entry(ass).State = System.Data.Entity.EntityState.Modified;
                        //_db.SaveChanges();

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
            var assignment = _db.Assignment.SingleOrDefault(m => m.Id == AssignmentId && m.Status > AssignmentStatus.DELETED);
            if (assignment.Status == AssignmentStatus.UNFINISHED)
            {
                assignment.Status = AssignmentStatus.FINISHED;
                assignment.CompleteDate = DateTime.Now;
            }
            else
            {
                assignment.Status = AssignmentStatus.UNFINISHED;
                assignment.CompleteDate = null;
            }
            try
            {
                _db.Entry(assignment).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
            }
            catch (Exception)
            {
                return Json(new { result = "操作失败" });
            }
            return Json(new { result = "操作成功", Id = assignment.ProcedureId });
        }


        //获取任务详情
        public PartialViewResult Assignment_Detail(int AssignmentId)
        {
            var assignment = _db.Assignment.SingleOrDefault(m => m.Id == AssignmentId && m.Status > AssignmentStatus.DELETED);
            var EmployeeList = from m in _db.Employee
                               where m.Status > EmployeeStatus.DEVOICE
                               select m;
            ViewBag.EmployeeDropDown = new SelectList(EmployeeList, "Id", "NickName", assignment.HolderId);
            return PartialView(assignment);
        }


        //任务修改
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult Edit_Assignment_Detail(FormCollection form, Assignment model)
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


        //删除任务
        [HttpPost]
        public JsonResult Delete_Assignment(int AssignmentId)
        {
            var assignment = _db.Assignment.SingleOrDefault(m => m.Id == AssignmentId && m.Status > AssignmentStatus.DELETED);
            if (assignment == null)
            {
                return Json(new { result = "此任务已不存在。" });
            }
            else
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
                            return Json(new { result = "发生错误。" });
                        }
                    }
                    _db.SaveChanges();
                }
                try
                {
                    assignment.Status = AssignmentStatus.DELETED;
                    _db.Entry(assignment).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                }
                catch (Exception)
                {
                    return Json(new { result = "发生错误。" });
                }
                return Json(new { result = "SUCCESS", Id = assignment.ProcedureId });
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
            var assignment = _db.Assignment.SingleOrDefault(m => m.Id == AssignmentId && m.Status > AssignmentStatus.DELETED);
            var departmentlist = from m in _db.Department
                                 where m.Status == DepartmentStatus.NORMAL
                                 select m;
            ViewBag.DepartmentList = departmentlist;
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
        public PartialViewResult GetSubtaskForm(int AssignmentId)
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
        //添加和删除参与人
        [HttpPost]
        public JsonResult ControlCollaboratorAjax(bool Remove, int EmployeeId, int AssignmentId)
        {
            var assignment = _db.Assignment.SingleOrDefault(m => m.Id == AssignmentId && m.Status > AssignmentStatus.DELETED);
            var employee = _db.Employee.SingleOrDefault(m => m.Id == EmployeeId);
            if (Remove)
            {
                assignment.Collaborator.Remove(employee);
                _db.SaveChanges();
                return Json(new { result = "SUCCESS" });

            }
            else
            {
                assignment.Collaborator.Add(employee);
                _db.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
        }


        [HttpPost]
        public ActionResult SortProcedure(string ProcedureJson)
        {
            JArray ja = (JArray)JsonConvert.DeserializeObject(ProcedureJson);
            foreach(var item in ja)
            {
                try
                {
                    var objdata = JsonConvert.DeserializeObject <SortProcedureModel>(item.ToString());
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

        [HttpPost]
        public ActionResult DragAssignment(int aid,int nowpid)
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

        [HttpPost]
        public JsonResult GetProcedureJsonInfo(int SubjectId) {
            var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId);
            List<ProcedureJsonModel> plist = new List<ProcedureJsonModel>();
            foreach (var item in subject.ProcedureTemplate.Procedure.Where(p=>p.Status==ProcedureStatus.NORMAL))
            {
                ProcedureJsonModel pinfo = new ProcedureJsonModel()
                {
                    ProcedureName = item.ProcedureTitle,
                    ProcedureId = item.Id,
                    FinishNum = item.Assignment.Count(m => m.Status == AssignmentStatus.FINISHED),
                    TotalNum = item.Assignment.Count(m => m.Status>AssignmentStatus.DELETED)
                };
                plist.Add(pinfo);
            }
            return Json(new { result = plist });
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
            string msg = string.Empty;
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
                    Bitmap CroppedImage = MakeThumbnailImage(sBitmap, 200, 200, section.Width, section.Height, section.X, section.Y,jsonimgsize.rotate);
                    AliOSSUtilities util = new AliOSSUtilities();
                    util.PutObject(BitmapByte(CroppedImage), "Subject/Avatar/" + filename);
                    imgurl = filename;                  
                    return Json(new { result = "SUCCESS",imgurl= "Subject/Avatar/"+ imgurl });
                }
                else
                {
                    error = "文件错误";
                    return Json(new { result = error });
                }
            }
            return Json(new { result = "FAIL"});
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
                    Stream stream = files[0].InputStream;                   
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
        public static Bitmap MakeThumbnailImage(Image originalImage, int maxWidth, int maxHeight, int cropWidth, int cropHeight, int X, int Y,float angle)
        {
            Bitmap _b = new Bitmap(cropWidth, cropHeight);
            var b = KiRotate(originalImage, angle);
            try
            {
                using (Graphics g = Graphics.FromImage(_b))
                {
                    //清空画布并以透明背景色填充
                    g.Clear(Color.Transparent);
                    //在指定位置并且按指定大小绘制原图片的指定部分
                    g.DrawImage(b, new Rectangle(0, 0, maxWidth, maxHeight), X, Y, cropWidth, cropHeight, GraphicsUnit.Pixel);
                    Image displayImage = new Bitmap(_b, maxWidth, maxHeight);
                    Bitmap bit = new Bitmap(_b, maxWidth, maxHeight);                    
                    return bit;
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
                else if(code == LogCode.EDITSUBJECT)
                {
                    logContent = employee.NickName + " 修改了项目 " + subject.SubjectTitle;
                }
                else if(code == LogCode.ARCHIVESUBJECT)
                {
                    logContent = employee.NickName + " 将项目 " + subject.SubjectTitle +" 进行了归档";
                }
                else if(code == LogCode.DELETESUBJECT)
                {
                    logContent = employee.NickName + " 删除了项目 " + subject.SubjectTitle;
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