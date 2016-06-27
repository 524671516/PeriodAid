using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace System.Web.Mvc
{
    public static class HtmlExtensions
    {
        public static string ExpensesPaymentType(this HtmlHelper helper, int type)
        {
            switch (type)
            {
                case 0:
                    return "进场费";
                case 1:
                    return "活动费";
                default:
                    return "未知费用";
            }
        }
        public static string ExpensesStatus(this HtmlHelper helper, int status)
        {
            switch (status)
            {
                case -1:
                    return "作废";
                case 0:
                    return "未审核";
                case 1:
                    return "已审核";
                case 2:
                    return "已结算";
                case 3:
                    return "已核销";
                default:
                    return "位置状态";
            }
        }
        public static string ExpensesStatusSpan(this HtmlHelper helper, int status)
        {
            //string value = "";
            switch (status)
            {
                case -1:
                    return "<span class='text-danger'>已作废</span>";
                case 0:
                    return "<span>未审核</span>";
                case 1:
                    return "<span>已审核</span>";
                case 2:
                    return "<span>已结算</span>";
                case 3:
                    return "<span class='text-success'>已核销</span>";
                default:
                    return "<span class='text-danger'>未知状态</span>";
            }
        }
    }
}