using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using PeriodAid.Models;
using PeriodAid.Controllers;

namespace PeriodAid.Filters
{
    public class SettingFilter : AuthorizeAttribute
    {
        public string SettingName;
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            var UserManager = filterContext.HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var user = UserManager.FindById(filterContext.HttpContext.User.Identity.GetUserId());
            OfflineSales offlineDb = new OfflineSales();
            var item = offlineDb.Off_System_Setting.SingleOrDefault(m => m.Off_System_Id == user.DefaultSystemId && m.SettingName == SettingName);
            //var s = filterContext.HttpContext.p
            if (item != null)
            {
                if (item.SettingResult == false)
                {
                    filterContext.Result = new RedirectResult("/OffCommon/AuthorizeError");
                }
            }
            else
            {
                filterContext.Result = new RedirectResult("/OffCommon/AuthorizeError");
            }
        }
    }

    public class OperationAuthAttribute : ActionFilterAttribute
    {
        public int OperationGroup;
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            try
            {
                var _db = new ProjectSchemeModels();
                var user = _db.Employee.SingleOrDefault(m => m.UserName == filterContext.HttpContext.User.Identity.Name);
                if (user.Type ==EmployeeType.DEPARTMENTMANAGER)
                {
                    // 管理员权限，直接下推
                }
                else if (user.Type == EmployeeType.ORDINARYEMPLOYEE)
                {
                    // 普通权限
                    if (OperationGroup == OperationGroupCode.PROJADD)
                    {
                        // 添加项目
                        // 所有人员均可以创建项目
                    }
                    else if (OperationGroup == OperationGroupCode.PROJVIEW)
                    {
                        // 查看项目
                        var SubjectId = Convert.ToInt32(filterContext.HttpContext.Request.Params["SubjectId"]);
                        var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId && m.Status > SubjectStatus.DELETED);
                        if (subject == null)
                        {
                            filterContext.Result = new RedirectResult("/TaskManagement/SubjectNotFound");
                        }
                        else
                        {
                            var assignmentList = from m in subject.Assignment
                                                 where m.Status > AssignmentStatus.DELETED
                                                 select m.Id;
                            var cooperater = (from m in user.CollaborateAssignment
                                              where assignmentList.Contains(m.Id)&&m.Status>AssignmentStatus.DELETED
                                              select m).Count();
                            var assignmentHolder = (from m in subject.Assignment
                                                    where m.HolderId == user.Id && m.Status > AssignmentStatus.DELETED
                                                    select m).Count();
                            if (subject.HolderId != user.Id && cooperater == 0 && assignmentHolder == 0)
                            {
                                filterContext.Result = new RedirectResult("/TaskManagement/SubjectNoAuthority");
                            }
                        }
                    }
                    else if (OperationGroup == OperationGroupCode.PROJEDIT)
                    {
                        // 编辑/删除项目
                        var SubjectId = Convert.ToInt32(filterContext.HttpContext.Request.Params["SubjectId"]);
                        var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId&&m.Status>SubjectStatus.DELETED);
                        if (subject == null)
                        {
                            setErrorResult(filterContext, "操作失败,项目已被移除。");
                        }
                        else
                        {
                            if (subject.HolderId != user.Id)
                            {
                                setErrorResult(filterContext, "当前用户没有权限编辑项目。");
                            }
                        }
                    }
                    else if (OperationGroup == OperationGroupCode.ASSIADD)
                    {
                        // 添加任务
                        var SubjectId = Convert.ToInt32(filterContext.HttpContext.Request.Params["SubjectId"]);
                        var ProcedureId= Convert.ToInt32(filterContext.HttpContext.Request.Params["ProcedureId"]);
                        var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId&&m.Status>SubjectStatus.DELETED);
                        if (subject == null)
                        {
                            setErrorResult(filterContext, "操作失败，项目已被移除。");
                        }
                        else
                        {
                            var procedure = _db.Procedure.SingleOrDefault(m => m.Id == ProcedureId && m.Status == ProcedureStatus.NORMAL);
                            if (procedure == null)
                            {
                                setErrorResult(filterContext, "操作失败，任务列表已被移除。");
                            }
                            else
                            {
                                var assignmentList = from m in subject.Assignment
                                                     where m.Status > AssignmentStatus.DELETED
                                                     select m.Id;
                                var cooperater = (from m in user.CollaborateAssignment
                                                  where assignmentList.Contains(m.Id)&& m.Status > AssignmentStatus.DELETED
                                                  select m).Count();
                                var assignmentHolder = (from m in subject.Assignment
                                                        where m.HolderId == user.Id&& m.Status > AssignmentStatus.DELETED
                                                        select m).Count();
                                if (subject.HolderId != user.Id && cooperater == 0 && assignmentHolder == 0)
                                {
                                    setErrorResult(filterContext, "当前用户没有权限添加任务。");
                                }
                            }
                            
                        }
                       
                    }
                    else if (OperationGroup == OperationGroupCode.ASSIVIEW)
                    {
                        // 查看任务
                        var assignmentId = Convert.ToInt32(filterContext.HttpContext.Request.Params["AssignmentId"]);
                        var assignment = _db.Assignment.SingleOrDefault(m => m.Id == assignmentId&&m.Status>AssignmentStatus.DELETED);
                        if (assignment == null)
                        {
                            setErrorResult(filterContext, "操作失败,任务已被移除。");
                        }
                        else
                        {
                            var assignmentlist = from m in assignment.Subject.Assignment
                                                 where m.Status > AssignmentStatus.DELETED
                                                 select m.Id;
                            var cooperater_count = (from m in user.CollaborateAssignment
                                                    where assignmentlist.Contains(m.Id)&& m.Status > AssignmentStatus.DELETED
                                                    select m).Count();
                            var holder_count = (from m in assignment.Subject.Assignment
                                                where m.HolderId == user.Id&& m.Status > AssignmentStatus.DELETED
                                                select m).Count();
                            if (cooperater_count == 0 && holder_count==0 && assignment.Subject.HolderId != user.Id)
                            {
                                setErrorResult(filterContext, "当前用户没有权限访问此任务。");
                            }
                        }
                    }
                    else if (OperationGroup == OperationGroupCode.ASSIEDIT)
                    {
                        // 编辑任务
                        var assignmentId = Convert.ToInt32(filterContext.HttpContext.Request.Params["AssignmentId"]);
                        var assignment = _db.Assignment.SingleOrDefault(m => m.Id == assignmentId && m.Status > AssignmentStatus.DELETED);
                        if (assignment == null)
                        {
                            setErrorResult(filterContext, "操作失败,任务已被移除。");
                        }
                        else
                        {
                            var cooperater_count = (from m in user.CollaborateAssignment
                                                    where m.Id == assignmentId&& m.Status > AssignmentStatus.DELETED
                                                    select m).Count();
                            if (assignment.HolderId != user.Id && assignment.Subject.HolderId != user.Id)
                            {
                                setErrorResult(filterContext, "当前用户没有权限编辑此任务。");
                            }
                        }
                    }
                    else if (OperationGroup == OperationGroupCode.SUBASSIADD)
                    {
                        //添加子任务
                        var assignmentId = Convert.ToInt32(filterContext.HttpContext.Request.Params["AssignmentId"]);
                        var assignment = _db.Assignment.SingleOrDefault(m => m.Id == assignmentId && m.Status > AssignmentStatus.DELETED);
                        if (assignment == null)
                        {
                            setErrorResult(filterContext, "操作失败,任务已被移除。");
                        }
                        else
                        {
                            var cooperater_count = (from m in user.CollaborateAssignment
                                                    where m.Id == assignmentId&& m.Status > AssignmentStatus.DELETED
                                                    select m).Count();
                            if (assignment.HolderId != user.Id && assignment.Subject.HolderId != user.Id && cooperater_count == 0)
                            {
                                setErrorResult(filterContext, "当前用户没有权限执行此操作。");
                            }
                        }
                    }
                    else if (OperationGroup == OperationGroupCode.SUBASSIVIEW)
                    {
                        // 查看子任务
                        var subtaskId = Convert.ToInt32(filterContext.HttpContext.Request.Params["SubTaskId"]);
                        var subtask = _db.SubTask.SingleOrDefault(m => m.Id == subtaskId && m.Status > AssignmentStatus.DELETED);
                        if (subtask == null)
                        {
                            setErrorResult(filterContext, "操作失败,子任务已被移除。");
                        }
                        else
                        {
                            var cooperater_count = (from m in user.CollaborateAssignment
                                                    where m.Id == subtask.Assignment.Id&& m.Status > AssignmentStatus.DELETED
                                                    select m).Count();
                            var holder_count = (from m in subtask.Assignment.Subject.Assignment
                                                where m.HolderId == user.Id&& m.Status > AssignmentStatus.DELETED
                                                select m).Count();
                            if (holder_count == 0&& subtask.Assignment.Subject.HolderId != user.Id && cooperater_count == 0)
                            {
                                setErrorResult(filterContext, "当前用户没有权限查看此子任务。");
                            }
                        }
                    }
                    else if (OperationGroup == OperationGroupCode.SUBASSIEDIT)

                    {
                        //编辑子任务
                        int subtaskId;
                        if (filterContext.HttpContext.Request.Params["Id"]==null|| filterContext.HttpContext.Request.Params["Id"] == "")
                        {
                             subtaskId = Convert.ToInt32(filterContext.HttpContext.Request.Params["SubTaskId"]);
                        }
                        else
                        {
                             subtaskId = Convert.ToInt32(filterContext.HttpContext.Request.Params["Id"]);
                        }                       
                        var subtask = _db.SubTask.SingleOrDefault(m => m.Id == subtaskId && m.Status > AssignmentStatus.DELETED);
                        if (subtask == null)
                        {
                            setErrorResult(filterContext, "操作失败,子任务已被移除。");
                        }
                        else
                        {
                            if (subtask.Assignment.HolderId != user.Id && subtask.Assignment.Subject.HolderId != user.Id && subtask.ExecutorId!= user.Id)
                            {
                                setErrorResult(filterContext, "当前用户没有权限编辑子任务。");
                            }
                        }
                    }                    
                    else
                    {
                        setErrorResult(filterContext, "用户行为未定义。");
                    }
                }
                else
                {
                    // 未知人员
                    setErrorResult(filterContext, "未知人员。");
                }
            }
            catch(Exception)
            {
                setErrorResult(filterContext, "未知错误。");
            }
        }

        private void setErrorResult(ActionExecutingContext filterContext, string errmsg)
        {
            
            if (filterContext.HttpContext.Request.HttpMethod == "POST")
            {
                filterContext.Result = new JsonResult()
                {
                    Data = new { result = "FAIL", errmsg = errmsg }
                };
            }
            else
            {
                filterContext.Result = new ContentResult()
                {
                    Content = "FAIL"
                };
            }
        }
    }

    public static class OperationGroupCode
    {
        /// <summary>
        /// 添加项目
        /// </summary>
        public static int PROJADD = 101;
        /// <summary>
        /// 查看项目
        /// </summary>
        public static int PROJVIEW = 102;
        /// <summary>
        /// 修改/删除项目
        /// </summary>
        public static int PROJEDIT = 103;
        /// <summary>
        /// 添加任务
        /// </summary>
        public static int ASSIADD = 201;
        /// <summary>
        /// 查看任务
        /// </summary>
        public static int ASSIVIEW = 202;
        /// <summary>
        /// 修改/删除任务
        /// </summary>
        public static int ASSIEDIT = 203;
        /// <summary>
        /// 添加子任务
        /// </summary>
        public static int SUBASSIADD = 301;
        /// <summary>
        /// 查看子任务
        /// </summary>
        public static int SUBASSIVIEW = 302;
        /// <summary>
        /// 修改/删除子任务
        /// </summary>
        public static int SUBASSIEDIT = 303;


    }
}