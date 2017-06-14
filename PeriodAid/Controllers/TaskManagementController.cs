using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PeriodAid.Controllers
{
    public class TaskManagementController : Controller
    {
        // GET: TaskManagement
        public ActionResult Index()
        {
            return View();
        }

        //获取星标任务列表
        public PartialViewResult Personal_StarTaskListPartial()
        {
            return PartialView();
        }
        //获取行动任务列表
        public PartialViewResult Personal_ActiveTaskListPartial()
        {
            return PartialView();
        }
        //获取已完成任务列表
        public PartialViewResult Personal_FinishTaskListPartial()
        {
            return PartialView();
        }

        //项目详情
        public ActionResult Task_Detail(string TaskId)
        {
            return View();
        }
    }
}