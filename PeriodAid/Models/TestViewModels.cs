using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PeriodAid.Models
{
    public class TestViewModels
    {
    }

    public class CreateQuestionViewModel
    {
        [Display(Name = "题库类型")]
        [Required(ErrorMessage = "题库类型不能为空")]
        public int TestTypeID { get; set; }

        [Display(Name = "题库问题")]
        [Required(ErrorMessage = "问题内容不能为空")]
        [StringLength(256, ErrorMessage ="问题内容超过256个字符")]
        public string questionArea { get; set; }

        [Display(Name = "答案A")]
        [Required(ErrorMessage = "答案内容不能为空")]
        [StringLength(50, ErrorMessage = "答案内容超过50个字符")]
        public string answerA { get; set; }

        [Display(Name = "答案B")]
        [Required(ErrorMessage = "答案内容不能为空")]
        [StringLength(50, ErrorMessage = "答案内容超过50个字符")]
        public string answerB { get; set; }

        [Display(Name = "答案C")]
        [Required(ErrorMessage = "答案内容不能为空")]
        [StringLength(50, ErrorMessage = "答案内容超过50个字符")]
        public string answerC { get; set; }

        [Display(Name = "答案D")]
        [Required(ErrorMessage = "答案内容不能为空")]
        [StringLength(50, ErrorMessage = "答案内容超过50个字符")]
        public string answerD { get; set; }

        public int answerRight { get; set; }
    }
}