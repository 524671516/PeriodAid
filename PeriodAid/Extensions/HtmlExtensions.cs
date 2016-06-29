using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PeriodAid.Models;

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
        public static string CheckinStatus(this HtmlHelper helper, int status)
        {
            switch (status)
            {
                case -1:
                    return "已作废";
                case 0:
                    return "无数据";
                case 1:
                    return "已签到";
                case 2:
                    return "已签退";
                case 3:
                    return "已提报";
                case 4:
                    return "已确认";
                case 5:
                    return "已结算";
                default:
                    return "位置状态";
            }
        }

        public static string ManagerNickName(this HtmlHelper helper, string username, int systemid)
        {
            if (username == null)
                return "";
            OfflineSales offlineDB = new OfflineSales();
            var item = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == username && m.Off_System_Id == systemid);
            if (item != null)
                return item.NickName;
            else
                return username;
        }
        
        public static string ManagerNickNameCollection(this HtmlHelper helper, string usernames, int systemid)
        {
            string[] names = usernames.Split(',');
            string[] nicknames = new string[names.Length];
            OfflineSales offlineDB = new OfflineSales();
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i] == null)
                    nicknames[i]= "";
                var item = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == usernames[i].ToString() && m.Off_System_Id == systemid);
                if (item != null)
                    nicknames[i] = item.NickName;
                else
                    nicknames[i] = "";
            }
            return string.Join(",", nicknames);
        }
    }
}