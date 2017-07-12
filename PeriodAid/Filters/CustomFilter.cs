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

    public class OperationJsonAuthAttribute : ActionFilterAttribute
    {
        public int OperationClass;
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            var _db = new ProjectSchemeModels();
            var param = filterContext.HttpContext.Request.Params["UID"];
            var user = _db.Employee.SingleOrDefault(m => m.UserName == filterContext.HttpContext.User.Identity.Name);
            if (user == null)
            {
                filterContext.Result = new JsonResult
                {
                    Data = new { result = "FAIL", errmsg = "当前用户不可用" },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                filterContext.Result = new JsonResult
                {
                    Data = new { Param = param },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }
    }
}