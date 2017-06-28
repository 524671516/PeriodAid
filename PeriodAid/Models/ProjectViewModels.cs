using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PeriodAid.Models
{
    public class ProjectViewModels
    {
    }
    //图片编辑参数
    public class CutImgModel
    {
        public double x { get; set; }
        public double y { get; set; }
        public double height { get; set; }
        public double width { get; set; }
        public float rotate { get; set; }
        public int ToInt(double doubleValue)
        {
            return Convert.ToInt32(doubleValue);
        }
    }
}