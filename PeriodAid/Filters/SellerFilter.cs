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
    public class SellerFilter : AuthorizeAttribute
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

    public class SellerAttribute : ActionFilterAttribute
    {
        public int OperationGroup;
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            try
            {
                var _db = new SalesProcessModel();
                var seller = _db.SP_Seller.SingleOrDefault(m => m.User_Name == filterContext.HttpContext.User.Identity.Name);
                if (seller.Seller_Type == SellerType.ADMINISTARTOR || seller.Seller_Type == SellerType.SELLERADMIN)
                {
                    // 管理员&&业务主管权限，直接下推
                }
                else if (seller.Seller_Type == SellerType.SELLER)
                {

                }
                else if (seller.Seller_Type == SellerType.FINANCIALDEPARTMENT)
                {
                    if (OperationGroup == SalesOperationCode.QUOTVIEW)
                    {
                        // 报价单查看权限
                    }
                    else if (OperationGroup == SalesOperationCode.QUOTADD)
                    {
                        setErrorResult(filterContext, "没有权限。");
                    }
                    else if (OperationGroup == SalesOperationCode.QUOTEDIT) {
                        setErrorResult(filterContext, "没有权限。");
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
            catch (Exception)
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
                    Content = "权限不足"
                };
            }
        }
    }

    public static class SalesOperationCode
    {
        /// <summary>
        /// 添加产品
        /// </summary>
        public static int PRODADD = 101;
        /// <summary>
        /// 查看产品
        /// </summary>
        public static int PRODVIEW = 102;
        /// <summary>
        /// 修改/删除产品
        /// </summary>
        public static int PRODEDIT = 103;
        /// <summary>
        /// 添加经销商
        /// </summary>
        public static int CLIEADD = 201;
        /// <summary>
        /// 查看经销商
        /// </summary>
        public static int CLIEVIEW = 202;
        /// <summary>
        /// 修改/删除经销商
        /// </summary>
        public static int CLIEEDIT = 203;
        /// <summary>
        /// 添加联系人
        /// </summary>
        public static int CONTADD = 301;
        /// <summary>
        /// 查看联系人
        /// </summary>
        public static int CONTVIEW = 302;
        /// <summary>
        /// 修改/删除联系人
        /// </summary>
        public static int CONTEDIT = 303;
        /// <summary>
        /// 添加渠道
        /// </summary>
        public static int SALESADD = 401;
        /// <summary>
        /// 查看渠道
        /// </summary>
        public static int SALESVIEW = 402;
        /// <summary>
        /// 修改/删除渠道
        /// </summary>
        public static int SALESEDIT = 403;
        /// <summary>
        /// 添加报价单
        /// </summary>
        public static int QUOTADD = 501;
        /// <summary>
        /// 查看报价单
        /// </summary>
        public static int QUOTVIEW = 502;
        /// <summary>
        /// 修改/删除报价单
        /// </summary>
        public static int QUOTEDIT = 503;
        /// <summary>
        /// 添加订单
        /// </summary>
        public static int ORDERADD = 601;
        /// <summary>
        /// 查看订单
        /// </summary>
        public static int ORDERVIEW = 602;
        /// <summary>
        /// 修改/删除订单
        /// </summary>
        public static int ORDEREDIT = 603;


    }
}