using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PeriodAid.Models
{
    public class PeriodUserInfoViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string UserName { get; set; }

        [Required(ErrorMessage ="用户年龄不能为空")]
        [Range(14,70,ErrorMessage ="年龄范围不得超过14-70")]
        public int UserAge { get; set; }

        [Required(ErrorMessage = "经期日期不得为空")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DateRange("2010-01-01", "2030-01-01")]
        public DateTime Last_MC_Begin { get; set; }

        [Required(ErrorMessage = "用户年龄不能为空")]
        [Range(3, 12, ErrorMessage = "年龄范围不得超过3-12")]
        public int MC_days { get; set; }

        [Required(ErrorMessage = "用户年龄不能为空")]
        [Range(14, 60, ErrorMessage = "年龄范围不得超过14-60")]
        public int MC_Cycle { get; set; }

        public bool Pregnancy { get; set; }

        public bool Pre_Pregnancy { get; set; }
    }

    public class DateRangeAttribute : ValidationAttribute
    {
        private readonly DateTime _begin;
        private readonly DateTime _end;
        public DateRangeAttribute(string begin, string end)
        {
            _begin = Convert.ToDateTime(begin);
            _end = Convert.ToDateTime(end);
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            try
            {
                DateTime current = Convert.ToDateTime(value);
                if(current< _end && current >= _begin)
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult("日期不属于指定区间");
                }
            }
            catch
            {
                return new ValidationResult("未知错误");
            }
        }
    }
}