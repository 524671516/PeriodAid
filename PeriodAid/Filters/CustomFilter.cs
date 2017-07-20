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
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            try
            {
                var _db = new ProjectSchemeModels();
                var user = _db.Employee.SingleOrDefault(m => m.UserName == filterContext.HttpContext.User.Identity.Name);
                if (user.Type == 1)
                {
                    // 管理员权限，直接下推
                }
                else if (user.Type == 0)
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
                        var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId);
                        var assignmentList = from m in subject.Assignment
                                             select m.Id;
                        var cooperater = (from m in user.CollaborateAssignment
                                         where assignmentList.Contains(m.Id)
                                         select m).Count();
                        var assignmentHolder = (from m in subject.Assignment
                                                where m.HolderId == user.Id
                                                select m).Count();
                        if (subject.HolderId != user.Id && cooperater == 0 && assignmentHolder==0)
                        {
                            setErrorResult(filterContext, "当前用户无权访问");
                        }
                    }
                    else if (OperationGroup == OperationGroupCode.PROJEDIT)
                    {
                        // 编辑/删除项目
                        var SubjectId = Convert.ToInt32(filterContext.HttpContext.Request.Params["SubjectId"]);
                        var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId);
                        if (subject.HolderId != user.Id)
                        {
                            setErrorResult(filterContext, "当前用户无权访问");
                        }
                    }
                    else if (OperationGroup == OperationGroupCode.ASSIADD)
                    {
                        // 添加任务
                        var SubjectId = Convert.ToInt32(filterContext.HttpContext.Request.Params["SubjectId"]);
                        var subject = _db.Subject.SingleOrDefault(m => m.Id == SubjectId);
                        var assignmentList = from m in subject.Assignment
                                             select m.Id;
                        var cooperater = (from m in user.CollaborateAssignment
                                          where assignmentList.Contains(m.Id)
                                          select m).Count();
                        var assignmentHolder = (from m in subject.Assignment
                                                where m.HolderId == user.Id
                                                select m).Count();
                        if (subject.HolderId != user.Id && cooperater == 0 && assignmentHolder == 0)
                        {
                            setErrorResult(filterContext, "当前用户无权访问");
                        }
                    }
                    else if (OperationGroup == OperationGroupCode.ASSIVIEW)
                    {
                        // 查看任务
                        var assigmentId = Convert.ToInt32(filterContext.HttpContext.Request.Params["AssignmentId"]);
                        var assigment = _db.Assignment.SingleOrDefault(m => m.Id == assigmentId);
                        var cooperater_count = (from m in user.CollaborateAssignment
                                                where m.Id == assigment.Id
                                                select m).Count();
                        if(cooperater_count==0&&assigment.HolderId != user.Id && assigment.Subject.HolderId != user.Id)
                        {
                            setErrorResult(filterContext, "当前用户无权访问");
                        }
                    }
                    else if (OperationGroup == OperationGroupCode.ASSIEDIT)
                    {
                        // 编辑任务
                    }
                    else if (OperationGroup == OperationGroupCode.SUBASSIADD)
                    {
                        // 添加子任务
                    }
                    else if (OperationGroup == OperationGroupCode.SUBASSIVIEW)
                    {
                        // 查看子任务
                    }
                    else if (OperationGroup == OperationGroupCode.SUBASSIEDIT)
                    {
                        // 编辑子任务
                    }
                    else
                    {
                        setErrorResult(filterContext, "未知行为");
                    }
                }
                else
                {
                    // 未知人员
                    setErrorResult(filterContext, "未知人员");
                }
            }
            catch(Exception)
            {
                setErrorResult(filterContext, "未知错误");
            }
        }

        private void setErrorResult(ActionExecutedContext filterContext, string errmsg)
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