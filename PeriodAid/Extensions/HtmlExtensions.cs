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
        public static string BonusStatus(this HtmlHelper helper, int status)
        {
            switch (status)
            {
                case -1:
                    return "作废";
                case 0:
                    return "待审核";
                case 1:
                    return "已发送";
                case 2:
                    return "已收款";
                case 3:
                    return "发送失败";
                case 4:
                    return "已退款";
                default:
                    return "未知";
            }
        }
        public static string ManagerRequestStatus(this HtmlHelper helper, int status)
        {
            switch (status)
            {
                case -1:
                    return "作废";
                case 0:
                    return "已提交";
                case 1:
                    return "已审核";
                case 2:
                    return "已完成";
                case 3:
                    return "已驳回";
                default:
                    return "未知";
            }
        }
        public static string AttendanceStatus(this HtmlHelper helper, int status)
        {
            switch (status)
            {
                case 0:
                    return "全勤";
                case 1:
                    return "迟到";
                case 2:
                    return "早退";
                case 3:
                    return "旷工";
                default:
                    return "未确认";
            }
        }
        public static string ManagerTaskStatus(this HtmlHelper helper, int status)
        {
            switch (status)
            {
                case -1:
                    return "已作废";
                case 0:
                    return "已提交";
                case 1:
                    return "已确认";
                default:
                    return "未知";
            }
        }
        public static string CompetitionInfoStatus(this HtmlHelper helper, int status)
        {
            switch (status)
            {
                case -1:
                    return "已作废";
                case 0:
                    return "已提交";
                case 1:
                    return "提报通过";
                case 2:
                    return "已收款";
                case 3:
                    return "发送失败";
                case 4:
                    return "已退款";
                default:
                    return "未知";
            }
        }
        public static string ProductTypeStatus(this HtmlHelper helper, int status)
        {
            switch (status)
            {
                case -1:
                    return "下架";
                case 0:
                    return "在售";
                case 1:
                    return "爆款";
                default:
                    return "未知";
            }
        }
        //CRM
        public static string ContractStatus(this HtmlHelper helper, string status)
        {
            switch (status)
            {
                case "3531567":
                    return "待发货";
                case "3764330":
                    return "已发货";
                case "3531568":
                    return "提交待审核";
                default:
                    return "未知";
            }
        }
        // YYS
        public static string ReceiverStatus(this HtmlHelper helper, int status)
        {
            switch (status)
            {
                case 0:
                    return "待发货";
                case 2:
                    return "已发货";
                default:
                    return "未知";
            }
        }

        public static string SellerStatus(this HtmlHelper helper, int status)
        {
            switch (status)
            {
                case -1:
                    return "离职";
                case 0:
                    return "在职";
                default:
                    return "未知";
            }
        }

        public static string SellerType(this HtmlHelper helper, int status)
        {
            switch (status)
            {
                case 0:
                    return "普通";
                case 1:
                    return "产品操作";
                case 2:
                    return "财务审核";
                case 3:
                    return "部门主管";
                case 4:
                    return "管理员";
                default:
                    return "未知";
            }
        }

        public static string ContactStatus(this HtmlHelper helper, int status)
        {
            switch (status)
            {
                case -1:
                    return "离职";
                case 0:
                    return "正常";
                default:
                    return "未知";
            }
        }
        
        public static string ClientType(this HtmlHelper helper, int status)
        {
            switch (status)
            {
                case 0:
                    return "未知";
                case 1:
                    return "大客户";
                case 2:
                    return "经销商";
                default:
                    return "未知";
            }
        }

        public static string PlattformTypeStatus(this HtmlHelper helper, int status)
        {
            switch (status)
            {
                case -1:
                    return "解约";
                case 0:
                    return "待开发";
                case 1:
                    return "活跃";
                default:
                    return "未知";
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
            List<string> nicknames = new List<string>();
            OfflineSales offlineDB = new OfflineSales();
            foreach(var item in names)
            {
                if (item == null)
                    nicknames.Add("");
                var singlename = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == item && m.Off_System_Id == systemid);
                if (item != null)
                    nicknames.Add(singlename.NickName);
                else
                    nicknames.Add("");
            }
            return string.Join(",", nicknames);
        }

        public static string StringFixLength(this HtmlHelper helper, string str, int str_length)
        {
            int _str_len = str.Length;
            if (_str_len > str_length)
                return str.Substring(0, str_length) + "...";
            else
                return str;
        }
    }
}