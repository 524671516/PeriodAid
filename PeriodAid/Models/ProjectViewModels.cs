using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PeriodAid.Models
{
    public class ProjectViewModels
    {
    }
    public class ContentTypeClass
    {
        public int Code { get; set; }
        public string Key { get; set; }
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

    //过程排序
    public class SortProcedureModel
    {
        public string procedureid { get; set; }
        public int sort { get; set; }
    }

    //SubjetJson数据
    public class ProcedureJsonModel
    {
        public string ProcedureName { get; set; }
        public int ProcedureId { get; set; }
        public int FinishNum { get; set; }
        public int TotalNum { get; set; }
    }

    //参与人筛选
    public class CollaboratorModel
    {
        public string DepartmentName { get; set; }
        public List<Employee> EmployeeList { get; set; }
    }
}