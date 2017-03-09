using PagedList;
using PeriodAid.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PeriodAid.Controllers
{
    public class WebContentController : Controller
    {
        SQZWEBModels _sqzwebDB = new SQZWEBModels();
        /*******
        官网内容
        ********/
        //活动列表页
        public ActionResult WebContentIndex()
        {
            return View();
        }

        //列表模板页
        public ActionResult WebEventsList_Ajax(int? page, string query)
        {
            int _page = page ?? 1;
            if (query == null || query == "")
            {
                var webewebeventslist = (from m in _sqzwebDB.Web_Event
                                         orderby m.Event_Date
                                         select m).ToPagedList(_page, 20);
                return PartialView(webewebeventslist);
            }
            else
            {
                var webewebeventslist = (from m in _sqzwebDB.Web_Event
                                         where (m.Event_Content.Contains(query) || m.Event_Title.Contains(query))
                                         orderby m.Event_Date
                                         select m).ToPagedList(_page, 20);
                return PartialView(webewebeventslist);
            }
        }

        //创建活动页
        public ActionResult CreateWebEventPartial()
        {
            var web_event = new Web_Event();
            return PartialView(web_event);
        }

        //提交活动
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CreateWebEventPartial(Web_Event model)
        {
            if (ModelState.IsValid)
            {
                Web_Event item = new Web_Event();
                if (TryUpdateModel(item))
                {
                    item.Event_UpdateTime = DateTime.Now;
                    _sqzwebDB.Web_Event.Add(item);
                    _sqzwebDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return PartialView(model);
            }
        }

        //修改活动
        public ActionResult EditWebEventPartial(int Eid)
        {
            var web_event = _sqzwebDB.Web_Event.SingleOrDefault(m => m.Id == Eid);
            return PartialView(web_event);
        }

        //提交修改
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EditWebEventPartial(int id, Web_Event model)
        {
            if (ModelState.IsValid)
            {
                var item = new Web_Event();
                if (TryUpdateModel(item))
                {
                    item.Event_UpdateTime = DateTime.Now;
                    _sqzwebDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    _sqzwebDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return PartialView(model);
            }

        }

        //删除活动
        [HttpPost]
        public ActionResult DeleteWebEvent(int Eid)
        {
            var item = _sqzwebDB.Web_Event.SingleOrDefault(m => m.Id == Eid);
            if (item == null)
            {
                return Json(new { result = "FAIL" });
            }
            else
            {
                try
                {
                    _sqzwebDB.Web_Event.Remove(item);
                    _sqzwebDB.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                catch
                {
                    return Json(new { result = "FAIL" });
                }
            }
        }

        /**********
        券码活动
        **********/
        public ActionResult CheckCodeGroupList()
        {
            return View();
        }

        public ActionResult CheckCodeGroupListPartial(int? page, string query)
        {
            int _page = page ?? 1;
            if (query == "" || query == null)
            {
                var checkcodelist = (from m in _sqzwebDB.CheckCode_Group
                                     orderby m.Id
                                     select m).ToPagedList(_page, 20);
                return PartialView(checkcodelist);

            }
            else
            {
                var checkcodelist = (from m in _sqzwebDB.CheckCode_Group
                                     where m.EventDescription.Contains(query) || m.EventTitle.Contains(query) || m.GroupName.Contains(query)
                                     orderby m.Id
                                     select m).ToPagedList(_page, 20);
                return PartialView(checkcodelist);
            }
        }
        
        public ActionResult CreateCheckCodeGroup()
        {
            var CheckCode_G = new CheckCode_Group();
            return PartialView(CheckCode_G);
        }

        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult CreateCheckCodeGroup(CheckCode_Group model)
        {
            if (ModelState.IsValid)
            {
                CheckCode_Group item = new CheckCode_Group();
                if (TryUpdateModel(item))
                {
                    item.EventStatus = true;
                    _sqzwebDB.CheckCode_Group.Add(item);
                    _sqzwebDB.SaveChanges();
                    return Content("SUCCESS");
                }
                else
                {
                    return Content("FAIL");
                }
                

            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return PartialView(model);
            }
        }
        
        public ActionResult EditCheckCodeGroup(int Cid)
        {
            var CheckCode_G = _sqzwebDB.CheckCode_Group.SingleOrDefault(m => m.Id == Cid);
            return PartialView(CheckCode_G);
        }

        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult EditCheckCodeGroup(int id,CheckCode_Group model)
        {
            if (ModelState.IsValid)
            {
                CheckCode_Group item = new CheckCode_Group();
                if (TryUpdateModel(item))
                {
                    _sqzwebDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    _sqzwebDB.SaveChanges();
                    return Content("SUCCESS");
                }
                else
                {
                    return Content("FAIL");
                }

            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return PartialView(model);
            }
        }
        //删除活动
        [HttpPost]
        public ActionResult DelCheckCodeGroup(int Cid)
        {
            var item = _sqzwebDB.CheckCode_Group.SingleOrDefault(m => m.Id == Cid);
            if (item == null)
            {
                return Json(new { result = "FAIL" });
            }
            else
            {
                try
                {
                    _sqzwebDB.CheckCode_Group.Remove(item);
                    _sqzwebDB.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                catch
                {
                    return Json(new { result = "FAIL" });
                }
            }
        }

        public ActionResult CheckCodeGroupStatistics()
        {
            var ccglist = from m in _sqzwebDB.CheckCode_Group
                          where m.Enable_Statistic==true
                          orderby m.GroupName
                          select m;
            ViewBag.ccg = new SelectList(ccglist, "Id", "GroupName");
            return View();
        }

        [HttpPost]
        public JsonResult SearchCCGStatistics(string startdate,string enddate,int ccgid)
        {
            if (startdate == "" || enddate == "")
            {
                return Json(new { result = "请选择时间" });
            }
            else
            {
                var _startdate = Convert.ToDateTime(startdate);
                var _enddate = Convert.ToDateTime(enddate).AddDays(1);
                string _sql = "SELECT CONVERT(VARCHAR(10),T2.ViewTime,120) AS [ViewTime],COUNT([ViewTime]) AS [ViewNum]" +
           "FROM [Statistic_PageView] AS T2" +
           " WHERE T2.CheckCode_Group_Id=" + ccgid + "AND CONVERT(VARCHAR(10),T2.ViewTime,120)>= '" + _startdate.ToString("yyyy-MM-dd") + "'and CONVERT(VARCHAR(10),T2.ViewTime,120)<= '" + _enddate.ToString("yyyy-MM-dd") + "'"
            + "GROUP BY CONVERT(VARCHAR(10),T2.ViewTime,120)";
                string sql = "SELECT CONVERT(VARCHAR(10),T1.ClickTime,120) AS [ClickTime],T1.ClickId AS [ClickId],COUNT([ClickId]) AS [ClickNum]" +
                            "FROM [Statistic_PageClick] AS T1" +
                            " WHERE T1.CheckCode_Group_Id =" + ccgid + "AND CONVERT(VARCHAR(10), T1.ClickTime, 120)>= '" + _startdate.ToString("yyyy-MM-dd") + "'and CONVERT(VARCHAR(10), T1.ClickTime, 120)<= '" + _enddate.ToString("yyyy-MM-dd") + "'"
                             + "GROUP BY T1.ClickId,CONVERT(VARCHAR(10), T1.ClickTime, 120)";
                var data =_sqzwebDB.Database.SqlQuery<CheckCodeStatistics>(sql);
                var _data = _sqzwebDB.Database.SqlQuery<CheckCodeStatistics>(_sql);
                return Json(new { result = "SUCCESS", cd=data,vd=_data });
            }
        }
    }
}