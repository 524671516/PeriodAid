using PagedList;
using PeriodAid.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PeriodAid.Models;
using Microsoft.AspNet.Identity.Owin;
using PeriodAid.DAL;

namespace PeriodAid.Controllers
{
    public class WebContentController : Controller
    {
        /*活动列表*/
        // GET: WebContent
        SQZWEBModels _db = new SQZWEBModels();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public WebContentController()
        {

        }

        public WebContentController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
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
                var webewebeventslist = (from m in _db.Web_Event
                                         orderby m.Event_Date
                                         select m).ToPagedList(_page, 20);
                return PartialView(webewebeventslist);
            }
            else
            {
                var webewebeventslist = (from m in _db.Web_Event
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
            var files = Request.Files;
            if (ModelState.IsValid)
            {
                Web_Event item = new Web_Event();
                if (TryUpdateModel(item))
                {
                    item.Event_UpdateTime = DateTime.Now;
                    _db.Web_Event.Add(item);
                    _db.SaveChanges();
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
            var web_event = _db.Web_Event.SingleOrDefault(m => m.Id == Eid);
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
                    _db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
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
            var item = _db.Web_Event.SingleOrDefault(m => m.Id == Eid);
            if (item == null)
            {
                return Json(new { result = "FAIL" });
            }
            else
            {
                try
                {
                    _db.Web_Event.Remove(item);
                    _db.SaveChanges();
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
                var checkcodelist = (from m in _db.CheckCode_Group
                                     orderby m.Id
                                     select m).ToPagedList(_page, 20);
                return PartialView(checkcodelist);

            }
            else
            {
                var checkcodelist = (from m in _db.CheckCode_Group
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
                    _db.CheckCode_Group.Add(item);
                    _db.SaveChanges();
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
            var CheckCode_G = _db.CheckCode_Group.SingleOrDefault(m => m.Id == Cid);
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
                    _db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
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
            var item = _db.CheckCode_Group.SingleOrDefault(m => m.Id == Cid);
            if (item == null)
            {
                return Json(new { result = "FAIL" });
            }
            else
            {
                try
                {
                    _db.CheckCode_Group.Remove(item);
                    _db.SaveChanges();
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
            var ccglist = from m in _db.CheckCode_Group
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
                if (_startdate > _enddate)
                {
                    return Json(new { result = "开始时间不能大于结束时间" });
                }
                else
                {
                    string _sql = "SELECT   T2.ViewDate, T1.ClickCount, T2.ViewCount" +
                                       "FROM(SELECT CONVERT(VARCHAR(10), ClickTime, 120) AS ClickDate, COUNT(Id) AS ClickCount " +
                                       "FROM Statistic_PageClick " +
                                       "WHERE(ClickTime > '" + startdate + "') AND(ClickTime < '" + enddate + "') AND(CheckCode_Group_Id = " + ccgid + ") " +
                                       "GROUP BY CONVERT(VARCHAR(10), ClickTime, 120)) AS T1 LEFT OUTER JOIN " +
                                       "(SELECT CONVERT(varchar(10), ViewTime, 120) AS ViewDate, COUNT(Id) AS ViewCount " +
                                       "FROM Statistic_PageView WHERE(ViewTime > '" + startdate + "') AND(ViewTime < '" + enddate + "') AND(CheckCode_Group_Id = " + ccgid + ") " +
                                       "GROUP BY CONVERT(varchar(10), ViewTime, 120)) AS T2 ON T1.ClickDate = T2.ViewDate";
                    var data = _db.Database.SqlQuery<CheckCodeStatistics>(_sql);
                    return Json(new { result = "SUCCESS", cd = data });
                }          
            }
        }

        [HttpPost]
        public ActionResult UpLoadImg(FormCollection form)
        {
            var files = Request.Files;
            string msg = string.Empty;
            string error = string.Empty;
            string imgurl;
            if (files.Count > 0)
            {
                if (files[0].ContentLength > 0 && files[0].ContentType.Contains("image"))
                {
                    string filename = DateTime.Now.ToFileTime().ToString() + ".jpg";
                    //files[0].SaveAs(Server.MapPath("/Content/checkin-img/") + filename);
                    AliOSSUtilities util = new AliOSSUtilities();
                    util.PutObject(files[0].InputStream, "/Content/upload/" + filename);
                    msg = "成功! 文件大小为:" + files[0].ContentLength;
                    imgurl = filename;
                    string res = "{ error:'" + error + "', msg:'" + msg + "',imgurl:'" + imgurl + "'}";
                    return Content(res);
                }
                else
                {
                    error = "文件错误";
                }
            }
            string err_res = "{ error:'" + error + "', msg:'" + msg + "',imgurl:''}";
            return Content(err_res);

        }
       
    }
}