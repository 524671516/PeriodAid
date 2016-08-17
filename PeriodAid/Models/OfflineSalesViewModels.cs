using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace PeriodAid.Models
{
    
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

    public class StoreView
    {
        public string StoreName { get; set; }
        public string StoreSystem
        {
            get; set;
        }
        public string Address
        {
            get; set;
        }
        public string Longitude
        {
            get; set;
        }
        public string Latitude
        {
            get; set;
        }
    }
    public class ScheduleList
    {
        public DateTime Subscribe { get; set; }
        public int Count { get; set; }
        public int Unfinished { get; set; }
    }
    public class Excel_DataMessage
    {
        public int line;
        public string message;
        public bool error;
        public Excel_DataMessage(int line, string message, bool error)
        {
            this.line = line;
            this.message = message;
            this.error = error;
        }
    }
    public class Seller_Statistic
    {
        public DateTime Date { get; set; }
        public int? SalesCount { get; set; }
        public decimal? SalesAmount { get; set; }
        public int? StorageCount { get; set; }
        public decimal? AVG_SalesData { get; set; }
        public decimal? AVG_AmountData { get; set; }
    }
    public class StoreSystem_Statistic
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public int? SalesCount { get; set; }
        public decimal? SalesAmount { get; set; }
        public int? StorageCount { get; set; }
    }
    public class StoreSystem_Salary_Statistic
    {
        public DateTime Date { get; set; }
        public string StoreSystem { get; set; }
        public decimal? Salary { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Bonus { get; set; }
    }
    public class StoreSystem_Product_Statistic
    {
        public int ProductId { get; set; }
        public string SimpleName { get; set; }
        public int? SalesCount { get; set; }
        public decimal? SalesAmount { get; set; }
        public int? StorageCount { get; set; }
    }

    public class SellerSalaryExcel
    {
        public string Name { get; set; }
        public string StoreName { get; set; }
        public string Mobile { get; set; }
        public string IdNumber { get; set; }
        public string AccountName { get; set; }
        public string AccountSource { get; set; }
        public string CardName { get; set; }
        public string CardNo { get; set; }
        public decimal? Standard_Salary { get; set; }
        public decimal? Salary { get; set; }
        public decimal? Bonus { get; set; }
        public decimal? Debit { get; set; }
        public int? Att_All { get; set; }
        public int? Att_Delay { get; set; }
        public int? Att_Leave { get; set; }
        public int? Att_Absence { get; set; }
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
    public class AccountSetting_ViewModel
    {
        [Display(Name = "银行列表")]
        public string BankList { get; set; }

        [Display(Name = "上下班时间浮动")]
        public string AttendanceAllow { get; set; }

        [Display(Name = "区域列表")]
        public string AreaList { get; set; }

        [Display(Name = "公司名称")]
        public string CompanyName { get; set; }

        [Display(Name ="公司图标")]
        public string CompanyAvatar { get; set; }
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
        public int ScheduleId { get; set; }

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
    public class Wx_SellerTaskMonthStatistic
    {
        public Off_Seller Off_Seller{ get; set; }

        public int AttendanceCount { get; set; }
    }
    public class Wx_SellerTaskAlert
    {
        public int Id { get; set; }
        public DateTime ApplyDate { get; set; }
        public int? MinStorage { get; set; }
        public string StoreName { get; set; }
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


}