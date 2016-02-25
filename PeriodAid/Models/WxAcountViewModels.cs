using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace PeriodAid.Models
{
    public class Wx_RegisterViewModel
    {
        [Required]
        [StringLength(11)]
        [RegularExpression("1[3|5|7|8|][0-9]{9}", ErrorMessage ="手机号码格式错误")]
        [Display(Name ="手机号码")]
        public string Mobile { get; set; }

        [Required]
        [StringLength(6, ErrorMessage = "手机验证码为6位")]
        [Display(Name = "手机验证码")]
        public string CheckCode { get; set; }
        
        [Display(AutoGenerateField = false)]
        public string Open_Id { get; set; }

        [Display(AutoGenerateField = false)]
        public string AccessToken { get; set; }

    }

    public class Wx_OffRegisterViewModel
    {
        [Required]
        [StringLength(11)]
        [RegularExpression("1[3|5|7|8|][0-9]{9}", ErrorMessage = "手机号码格式错误")]
        [Display(Name = "手机号码")]
        public string Mobile { get; set; }

        [Required]
        [StringLength(6, ErrorMessage = "手机验证码为6位")]
        [Display(Name = "手机验证码")]
        public string CheckCode { get; set; }

        [Required(ErrorMessage ="姓名不能为空")]
        [StringLength(6, ErrorMessage =("姓名长度不得超过6个字符"))]
        [Display(Name = "姓名")]
        public string NickName { get; set; }

        [Display(AutoGenerateField = false)]
        public string Open_Id { get; set; }

        [Display(AutoGenerateField = false)]
        public string AccessToken { get; set; }
    }

    public class Wx_SellerRegisterViewModel
    {
        [Required(ErrorMessage = "姓名不能为空")]
        [StringLength(6, ErrorMessage = ("姓名长度不得超过6个字符"))]
        [Display(Name = "姓名")]
        public string NickName { get; set; }
    }
    
    public class Wx_ManagerReportListViewModel
    {
        public int Id { get; set; }

        public int Status { get; set; }

        public string StoreName { get; set; }

        public string SellerName { get; set; }

        public int? Rep_Brown { get; set; }

        public int? Rep_Black { get; set; }
        
        public int? Rep_Lemon { get; set; }

        public int? Rep_Honey { get; set; }

        public int? Rep_Dates { get; set; }

        public int? Rep_Other { get; set; }

        public int? Rep_Total { get; set; }

        [Required(ErrorMessage ="金额不能为空")]
        [Range(0, 200, ErrorMessage ="奖金金额不能大于200元")]
        public decimal? Bonus { get; set; }

        [Required(ErrorMessage ="红包说明不能为空")]
        [StringLength(128, ErrorMessage = "不超过128个字符")]
        public string Bonus_Remark { get; set; }
    }
}