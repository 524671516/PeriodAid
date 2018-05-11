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
                if (seller.Seller_Type == SellerType.ADMINISTARTOR || seller.Seller_Type == SellerType.SELLERADMIN) //管理员业务主管权限
                {
                    // 管理员&&业务主管权限，直接下推
                }
                else if (seller.Seller_Type == SellerType.PRODUCTDEPARTMENT) // 产品部权限
                {
                    if (OperationGroup == SalesOperationCode.QUOTVIEW)
                    {
                        // 报价单查看权限
                    }
                    else if (OperationGroup == SalesOperationCode.QUOTADD)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.QUOTEDIT)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.SELLERVIEW)
                    {
                        // 查看人员信息权限
                    }
                    else if (OperationGroup == SalesOperationCode.SELLERADD)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.SELLEREDIT)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if(OperationGroup == SalesOperationCode.CLIEVIEW)
                    {
                        // 查看经销商权限
                    }
                    else if (OperationGroup == SalesOperationCode.CLIEADD)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.CLIEEDIT)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.CONTVIEW)
                    {
                        // 查看联系人权限
                    }
                    else if (OperationGroup == SalesOperationCode.CONTADD)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if(OperationGroup == SalesOperationCode.CONTEDIT)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.SALESVIEW)
                    {
                        // 查看渠道权限
                    }
                    else if(OperationGroup == SalesOperationCode.SALESADD)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.SALESEDIT)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.QUOPRIVIEW)
                    {
                        // 查看报价产品权限
                    }
                    else if (OperationGroup == SalesOperationCode.QUOPRIADD)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.QUOPRIEDIT)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if(OperationGroup == SalesOperationCode.PRODVIEW)
                    {
                        // 查看产品权限
                    }
                    else if (OperationGroup == SalesOperationCode.ORDERVIEW)
                    {
                        // 查看订单权限
                    }
                    else if (OperationGroup == SalesOperationCode.ORDERADD)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.ORDEREDIT)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.ORDERCONFIRM)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.ORDERPRICEVIEW)
                    {
                        //
                    }
                    else if (OperationGroup == SalesOperationCode.ORDERPRICEADD)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.ORDERPRICEEDIT)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                }
                else if (seller.Seller_Type == SellerType.FINANCIALDEPARTMENT) // 财务部权限
                {
                    if (OperationGroup == SalesOperationCode.QUOTVIEW)
                    {
                        // 报价单查看权限
                    }
                    else if (OperationGroup == SalesOperationCode.QUOTADD)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.QUOTEDIT)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.SELLERVIEW)
                    {
                        // 查看人员信息权限
                    }
                    else if (OperationGroup == SalesOperationCode.SELLERADD)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.SELLEREDIT)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.CLIEVIEW)
                    {
                        // 查看经销商权限
                    }
                    else if (OperationGroup == SalesOperationCode.CLIEADD)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.CLIEEDIT)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.CONTVIEW)
                    {
                        // 查看联系人权限
                    }
                    else if (OperationGroup == SalesOperationCode.CONTADD)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.CONTEDIT)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.SALESVIEW)
                    {
                        // 查看渠道权限
                    }
                    else if (OperationGroup == SalesOperationCode.SALESADD)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.SALESEDIT)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.QUOPRIVIEW)
                    {
                        // 查看报价产品权限
                    }
                    else if (OperationGroup == SalesOperationCode.QUOPRIADD)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.QUOPRIEDIT)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.PRODVIEW)
                    {
                        // 查看产品权限
                    }
                    else if (OperationGroup == SalesOperationCode.PRODADD)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.PRODEDIT)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.ORDERVIEW)
                    {
                        // 查看订单权限
                    }
                    else if (OperationGroup == SalesOperationCode.ORDERADD)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.ORDEREDIT)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.ORDERCONFIRM)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.ORDERPRICEVIEW)
                    {
                        //
                    }
                    else if (OperationGroup == SalesOperationCode.ORDERPRICEADD)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.ORDERPRICEEDIT)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                }
                else if (seller.Seller_Type == SellerType.SELLER) // 业务员权限
                {
                    if (OperationGroup == SalesOperationCode.SELLERVIEW)
                    {
                        // 查看人员信息权限
                    }
                    else if (OperationGroup == SalesOperationCode.SELLERADD)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.SELLEREDIT)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.PRODVIEW)
                    {
                        // 查看产品权限
                    }
                    else if (OperationGroup == SalesOperationCode.PRODADD)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.PRODEDIT)
                    {
                        setErrorResult(filterContext, "权限不足");
                    }
                    else if (OperationGroup == SalesOperationCode.CLIEVIEW)
                    {
                        // 查看经销商权限
                    }
                    else if (OperationGroup == SalesOperationCode.CLIEADD)
                    {
                       //
                    }
                    else if (OperationGroup == SalesOperationCode.CLIEEDIT)
                    {
                       //
                    }
                    else if (OperationGroup == SalesOperationCode.SALESVIEW)
                    {
                        // 查看渠道权限
                    }
                    else if (OperationGroup == SalesOperationCode.ORDERVIEW)
                    {
                        // 查看产品权限
                    }
                    else if (OperationGroup == SalesOperationCode.ORDERADD)
                    {
                       //
                    }
                    else if (OperationGroup == SalesOperationCode.ORDEREDIT)
                    {
                        //
                    }
                    else if (OperationGroup == SalesOperationCode.ORDERCONFIRM)
                    {
                        //
                    }
                    else if (OperationGroup == SalesOperationCode.ORDERPRICEVIEW)
                    {
                    }
                    else if (OperationGroup == SalesOperationCode.ORDERPRICEADD)
                    {
                    }
                    else if (OperationGroup == SalesOperationCode.ORDERPRICEEDIT)
                    {
                    }
                }
                else
                {
                    // 未知人员
                    setErrorResult(filterContext, "未知人员");
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
                    Data = new { result = "Permission_denied", errmsg = errmsg }
                };
            }
            else
            {
                filterContext.Result = new ContentResult()
                {
                    Content = "<h2>权限不足,请联系管理员注册开通</h2>"
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
        /// <summary>
        /// 修改/删除订单
        /// </summary>
        public static int ORDERCONFIRM = 604;
        /// <summary>
        /// 添加人员信息
        /// </summary>
        public static int SELLERADD = 701;
        /// <summary>
        /// 查看人员信息
        /// </summary>
        public static int SELLERVIEW = 702;
        /// <summary>
        /// 修改/删除人员信息
        /// </summary>
        public static int SELLEREDIT = 703;
        /// <summary>
        /// 新增报价产品
        /// </summary>
        public static int QUOPRIADD = 801;
        /// <summary>
        /// 查看报价产品
        /// </summary>
        public static int QUOPRIVIEW = 802;
        /// <summary>
        /// 修改/删除报价产品
        /// </summary>
        public static int QUOPRIEDIT = 803;
        /// <summary>
        /// 新增报价产品
        /// </summary>
        public static int ORDERPRICEADD = 901;
        /// <summary>
        /// 查看报价产品
        /// </summary>
        public static int ORDERPRICEVIEW = 902;
        /// <summary>
        /// 修改/删除报价产品
        /// </summary>
        public static int ORDERPRICEEDIT = 903;
    }
}