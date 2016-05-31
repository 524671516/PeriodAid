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

        public int SystemId { get; set; }
    }

    public class Wx_SellerRegisterViewModel
    {
        [Required(ErrorMessage = "姓名不能为空")]
        [StringLength(6, ErrorMessage = ("姓名长度不得超过6个字符"))]
        [Display(Name = "姓名")]
        public string NickName { get; set; }

        public int Systemid { get; set; }
    }

    public class Promotion_TJH_ViewModel
    {
        [Required]
        [StringLength(32)]
        public string openId { get; set; }

        [Required(ErrorMessage = "请填写姓名")]
        [StringLength(10, ErrorMessage = "姓名不得大于10个字符")]
        [Display(Name = "姓名")]
        public string name { get; set; }

        [Required(ErrorMessage = "请填写电话号码")]
        [StringLength(32, ErrorMessage = "电话号码不得大于32个字符")]
        [RegularExpression("1[3|5|7|8|][0-9]{9}", ErrorMessage = "手机号码格式错误")]
        [Display(Name = "手机号码")]
        public string mobile { get; set; }

        [Required(ErrorMessage = "请选择您的职业")]
        [StringLength(32, ErrorMessage = "职位不得大于32个字符")]
        [Display(Name = "职业")]
        public string branch { get; set; }
    }

}