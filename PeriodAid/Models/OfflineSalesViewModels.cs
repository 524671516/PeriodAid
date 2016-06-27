using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace PeriodAid.Models
{
    public class OfflineSalesViewModels
    {
    }

    public class Store_System_ViewModel
    {
        [Required]
        [StringLength(50, ErrorMessage = "系统名称不能大于50字符")]
        [Display(Name = "系统名称")]
        public string System_Name { get; set; }
    }

    public class Store_ViewModel
    {
        [Display(Name = "门店系统")]
        [Required(ErrorMessage ="门店系统不能为空")]
        public int Store_System_Id { get; set; }

        [Required(ErrorMessage = "门店名称不能为空")]
        [StringLength(50, ErrorMessage = "门店名称不能大于50字符")]
        [Display(Name = "门店名称")]
        public string Store_Name { get; set; }

        [StringLength(128, ErrorMessage = "门店地址不能大于128字符")]
        [Display(Name = "门店地址")]
        public string Address { get; set; }

        [StringLength(32, ErrorMessage = "联系方式不能大于32个字符")]
        [Display(Name = "联系方式")]
        public string Contact { get; set; }
    }

    public class Seller_ViewModel
    {
        [Required]
        [StringLength(16, ErrorMessage = "姓名不得大于16个字符")]
        [Display(Name = "姓名")]
        public string Name { get; set; }

        [StringLength(32, ErrorMessage = "联系方式不得大于32个字符")]
        [Display(Name = "联系方式")]
        public string Contact { get; set; }

        [Display(Name = "性别")]
        [Required(ErrorMessage ="性别不能为空")]
        public bool? Sex { get; set; }
    }

    public class Sales_Data_ViewModel
    {
        [DataType(DataType.Date, ErrorMessage = "日期格式错误")]
        [Required(ErrorMessage = "销售日期不能为空")]
        [Display(Name = "销售日期")]
        public DateTime Sales_Date { get; set; }

        [Display(Name = "门店系统")]
        [Required(ErrorMessage = "门店系统不能为空")]
        public int Store_System_Id { get; set; }

        [Display(Name ="门店")]
        [Required(ErrorMessage ="门店不能为空")]
        public int Store_Id { get; set; }
        

        [Required(ErrorMessage = "试饮数不能为空")]
        [Display(Name ="试饮数")]
        public int? Trial_Count { get; set; }

        [Display(Name = "促销员")]
        [Required(ErrorMessage = "促销员不能为空")]
        public int Seller_Id { get; set; }

        [Display(Name = "最高销量")]
        [Required(ErrorMessage = "最高销量不能为空")]
        public int? Max_Sale { get; set; }

        [Display(Name = "客户反馈")]
        [Required(ErrorMessage = "客户反馈不能为空")]
        public int? Feedback { get; set; }

        [StringLength(32, ErrorMessage ="消费年龄不能大于32字符")]
        [Display(Name = "消费年龄")]
        [Required(ErrorMessage = "消费年龄不能为空")]
        public string Comsumption_Age { get; set; }

        [StringLength(256, ErrorMessage ="总结内容不能大于256个字符")]
        [DataType(DataType.MultilineText)]
        [Display(Name ="意见及建议")]
        public string Summary { get; set; }

        [Display(Name = "参加活动")]
        public bool Event { get; set; }

        [Display(Name = "活动类型")]
        [StringLength(32, ErrorMessage = "活动类型不能大于32字符")]
        public string EventType { get; set; }
    }

    public class Store_Sales_Month_ViewModel
    {
        [Display(Name = "年份")]
        [Required(ErrorMessage = "年份不能为空")]
        [Range(2014, 2018, ErrorMessage = "月份值不规范")]
        public int Sales_Year { get; set; }

        [Display(Name = "月份")]
        [Required(ErrorMessage = "月份不能为空")]
        [Range(1, 12, ErrorMessage = "月份值不规范")]
        public int Sales_Month { get; set; }

        [Required(ErrorMessage = "门店名称不能为空")]
        [Display(Name = "门店名称")]
        public int Store_Id { get; set; }
    }

    public class CustomImage_ViewModel
    {
        [Required(ErrorMessage = "X坐标不能为空")]
        public int crop_x { get; set; }

        [Required(ErrorMessage = "Y坐标不能为空")]
        public int crop_y { get; set; }

        [Required(ErrorMessage = "宽度不能为空")]
        public int crop_w { get; set; }

        [Required(ErrorMessage = "高度不能为空")]
        public int crop_h { get; set; }

        [Required(ErrorMessage = "高度不能为空")]
        [MaxLength(100, ErrorMessage = "文件名不能大于100个字符")]
        public string filename { get; set; }
    }

    public class Benefits_ViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(128)]
        public string OpenId { get; set; }

        [Required]
        public int Status { get; set; }

        [Required]
        [StringLength(32, ErrorMessage ="姓名不得大于32个字符")]
        public string Name { get; set; }

        [Required]
        [StringLength(11)]
        [RegularExpression("1[3|5|7|8|][0-9]{9}", ErrorMessage = "手机号码格式错误")]
        public string Mobile { get; set; }

        [StringLength(20, ErrorMessage ="职位不得大于20个字符")]
        public string JobTitle { get; set; }

        [Required]
        [StringLength(64, ErrorMessage ="公司名字不得大于64个字符")]
        public string Company { get; set; }

        [StringLength(256, ErrorMessage ="申请理由不得大于256个字符")]
        public string Reason { get; set; }

        [Required]
        [RegularExpression("^[0-9]*$",ErrorMessage ="职员人数为数字")]
        [Range(0,100000, ErrorMessage ="职员人数介于0-10万人之间")]
        public int Staff { get; set; }

        [Required]
        [StringLength(32)]
        public string Region { get; set; }

        [Required]
        [StringLength(32)]
        public string Industry { get; set; }

        [Required(ErrorMessage ="地址不能为空")]
        [StringLength(128,ErrorMessage ="地址不能超过128个字符")]
        public string Address { get; set; }
    }
    public class StoreSchedule_ViewModel
    {
        [Required(ErrorMessage ="至少选择一个门店")]
        public string StoreList { get; set; }

        [Required(ErrorMessage="请选择活动开始日期")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "请选择活动开始日期")]
        public DateTime EndDate { get; set; }

        [RegularExpression("[012]\\d:[0-6]\\d", ErrorMessage ="格式错误")]
        [Required(ErrorMessage = "标准上班时间不能为空")]
        public string BeginTime { get; set; }

        [RegularExpression("[012]\\d:[0-6]\\d", ErrorMessage = "格式错误")]
        [Required(ErrorMessage ="标准下班时间不能为空")]
        public string FinishTime { get; set; }

        [Required(ErrorMessage ="标准薪资不能为空")]
        public decimal Salary { get; set; }
        [Required(ErrorMessage = "模板不能为空")]
        public int TemplateId { get; set; }
    }
    public class ConfirmCheckIn_ViewModel
    {
        public int? Rep_Brown { get; set; }

        public int? Rep_Black { get; set; }

        public int? Rep_Honey { get; set; }

        public int? Rep_Lemon { get; set; }

        public int? Rep_Dates { get; set; }

        public int? Rep_Other { get; set; }

        public string Confirm_Remark { get; set; }

        public int CheckIn_Id { get; set; }

        public int AttendanceStatus { get; set; }

        public bool Proxy { get; set; }

        public decimal? Salary { get; set; }

        public decimal? Bonus { get; set; }

        public string Bonus_Remark { get; set; }

        public decimal? Debits { get; set; }

        public string Remark { get; set; }
    }
    public class Wx_ManagerReportListViewModel
    {
        public int Id { get; set; }

        public int Status { get; set; }

        public string StoreName { get; set; }

        public string SellerName { get; set; }

        public int? Rep_Total { get; set; }

        public decimal? AVG_Total { get; set; }

        public int StoreId { get; set; }

        [Required(ErrorMessage = "金额不能为空")]
        [Range(0, 200, ErrorMessage = "奖金金额不能大于200元")]
        public decimal? Bonus { get; set; }

        [Required(ErrorMessage = "红包说明不能为空")]
        [StringLength(128, ErrorMessage = "不超过128个字符")]
        public string Bonus_Remark { get; set; }
    }

    public class Wx_ManagerCreateScheduleViewModel
    {

        [Required(ErrorMessage = "至少选择一个门店")]
        public int Off_Store_Id { get; set; }

        [Required(ErrorMessage ="至少选择一个模板")]
        public int Off_Template_Id { get; set; }

        public DateTime Subscribe { get; set; }

        [RegularExpression("[012]\\d:[0-6]\\d", ErrorMessage = "格式错误")]
        [Required(ErrorMessage = "标准上班时间不能为空")]
        public string Standard_CheckIn { get; set; }

        [RegularExpression("[012]\\d:[0-6]\\d", ErrorMessage = "格式错误")]
        [Required(ErrorMessage = "标准下班时间不能为空")]
        public string Standard_CheckOut { get; set; }

        [Required(ErrorMessage = "标准薪资不能为空")]
        public decimal Salary { get; set; }
        public decimal Standard_Salary { get; set; }
    }

    public class Wx_SellerCreditViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Mobile { get; set; }

        [Required]
        [StringLength(20)]
        public string AccountName { get; set; }

        [Required]
        [StringLength(20)]
        [RegularExpression("(^\\d{18}$)|(^\\d{15}$)|(^\\d{17}(\\d|X|x))", ErrorMessage = "格式错误")]
        public string IdNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string CardName { get; set; }

        [Required]
        [StringLength(50)]
        public string CardNo { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountSource { get; set; }
    }
    public class Wx_ReportItemsViewModel
    {
        public bool StorageRequired { get; set; }

        public bool AmountRequried { get; set; }

        public ICollection<Wx_TemplateProduct> ProductList { get; set; }
    }
    public class Wx_TemplateProduct
    {
        public int ProductId { get; set; }

        public string SimpleName { get; set; }

        public string ItemCode { get; set; }

        public int? SalesCount { get; set; }

        public int? Storage { get; set; }

        public decimal? SalesAmount { get; set; }
    }
    public class ExcelOperation
    {
        public static int? ConvertInt(DataRow dr, string columname)
        {
            if(dr[columname].ToString()== "")
                return null;
            else
            {
                return Convert.ToInt32(dr[columname]);
            }
        }
        public static DateTime? ConvertDateTime(DataRow dr, string columname)
        {
            if (dr[columname].ToString() == "")
                return null;
            else
            {
                return Convert.ToDateTime(dr[columname]);
            }
        }
        public static Decimal? ConvertDecimal(DataRow dr, string columname)
        {
            if (dr[columname].ToString() == "")
                return null;
            else
            {
                return Convert.ToDecimal(dr[columname]);
            }
        }
        public static bool ConvertBoolean(DataRow dr, string columname)
        {
            if (dr[columname].ToString() == "是")
                return true;
            else
            {
                return false;
            }
        }
    }
    public enum ManagerTaskStatus
    {
        Canceled = -1,
        Reported = 0,
        Confirmed = 1
    }
    
    public class ParseStatus
    {
        public static string AttendanceStatus(int? code)
        {
            switch(code){
                case null:
                    return "未确认";
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

        public static string CheckInStatus(int? code)
        {
            switch (code)
            {
                case null:
                    return "未知状态";
                case 0:
                    return "未签到";
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
                case -1:
                    return "作废";
                default:
                    return "其他";
            }
        }
        public static string getManagerTaskStatus(ManagerTaskStatus status)
        {
            string result = String.Empty;
            switch (status)
            {
                case ManagerTaskStatus.Canceled:
                    result = "已作废";
                    break;
                case ManagerTaskStatus.Reported:
                    result = "已提交";
                    break;
                case ManagerTaskStatus.Confirmed:
                    result = "已确认";
                    break;
                default:
                    result = "未知";
                    break;
            }
            return result;
        }

        public static string getManagerRequestStatus(int? code)
        {
            string result = String.Empty;
            switch (code)
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
        public static string getBonusRequestStatus(int? code)
        {
            string result = String.Empty;
            switch (code)
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


    }
}