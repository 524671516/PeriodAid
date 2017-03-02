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
        活动列表
        ********/
        //活动列表页
        public ActionResult WebContentIndex()
        {
            return View();
        }

        //列表模板页
        public ActionResult WebEventsList_Ajax(int?page,string query)
        {
            int _page=page ?? 1;
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
                                         where (m.Event_Content.Contains(query)||m.Event_Title.Contains(query))
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
        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult EditWebEventPartial(int id,Web_Event model)
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
       数据统计
       **********/
       public ActionResult WebContentStatistics()
        {
            return View();
        }


    }
}