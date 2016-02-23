using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PeriodAid.DAL;
using PeriodAid.Models;

namespace PeriodAid.Controllers
{
    public class PeriodAidController : Controller
    {
        // GET: PeriodAid
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private PeriodAidDataContext periodDB = new PeriodAidDataContext();
        public PeriodAidController()
        {
        }

        public PeriodAidController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        /****** 日历页面 **********/
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        /********** AJAX获取当月周期情况 *********/
        [Authorize]
        [HttpPost]
        public JsonResult getPeriodDetails(int year, int month)
        {
            PeriodAidUtilities utilities = new PeriodAidUtilities();
            var setting_item = periodDB.PeriodUserInfo.SingleOrDefault(m => m.UserName == User.Identity.Name);
            if (setting_item != null)
            {
                DateTime first_day = new DateTime(year, month, 1);
                DateTime last_day = first_day.AddMonths(1);
                var history_list = from m in periodDB.PeriodData
                                   where ((m.MC_Begin >= first_day && m.MC_Begin < last_day)
                                   || (m.MC_Finish >= first_day && m.MC_Finish < last_day))
                                   && m.UserName == User.Identity.Name
                                   select m;
                PeriodResult s = new PeriodResult();
                foreach (var item in history_list)
                {
                    s = utilities.RenderPeriodHistory(year, month, item, s);
                }
                s = utilities.getPeriodResult(year, month, setting_item.Last_MC_Begin, setting_item.MC_days, setting_item.MC_Cycle, s);
                return Json(new { result = "SUCCESS", p_day = s.p_days.ToArray(), e_day = s.e_days.ToArray(), prep_day = s.prep_days.ToArray() });
            }
            else
            {
                return Json(new { result = "FAILURE" });
            }
        }
        /********** AJAX日历点击结果判定 *********/
        [Authorize]
        [HttpPost]
        public JsonResult getClickDateResult(int year, int month, int day)
        {
            DateTime clickDate = new DateTime(year, month, day);
            bool it_come = false;
            bool it_gone = false;
            if(clickDate < DateTime.Now.AddDays(1).Date)
            {
                PeriodData pd = getLastPeriodData(User.Identity.Name);
                PeriodData lpd = getLastPeriodData(pd.MC_Begin, User.Identity.Name);
                if(clickDate >= pd.MC_Begin.AddDays(3) && clickDate < pd.MC_Begin.AddDays(13))
                {
                    it_gone = true;
                }
                if (lpd == null)
                {
                    if(clickDate > pd.MC_Begin.AddDays(-45))
                    {
                        it_come = true;
                    }
                }
                else
                {
                    if(clickDate > lpd.MC_Begin.AddDays(14))
                    {
                        it_come = true;
                    }
                }
            }
            if(it_come && it_gone)
            {
                return Json(new { result = "MODAL", message = "1" });
            }
            else if (it_come)
            {
                return Json(new { result = "MODAL", message = "0" });
            }
            else
            {
                return Json(new { result = "INFO", message = "测试" });
            }
        }
        /********** AJAX经期结束设定事件 *********/
        [Authorize]
        [HttpPost]
        public JsonResult setItGone(int year, int month, int day)
        {
            PeriodData pd = getLastPeriodData(User.Identity.Name);
            DateTime periodDone = new DateTime(year, month, day);
            if (pd != null)
            {
                PeriodInfo info = new PeriodInfo()
                {
                    MC_Begin = pd.MC_Begin,
                    MC_Cycle = pd.MC_Cycle,
                    MC_Days = Convert.ToInt32(periodDone.AddDays(1).Subtract(pd.MC_Begin).TotalDays)
                };
                updatePeriodData(info, User.Identity.Name);
                return Json(new { result = "SUCCESS" });
            }
            return Json(new { result = "FAIL" });
        }

        /********** AJAX经期开始设定事件 *********/
        [Authorize]
        [HttpPost]
        public JsonResult setItCome(int year, int month, int day)
        {
            PeriodData pd = getLastPeriodData(User.Identity.Name);
            DateTime periodStart = new DateTime(year, month, day);
            if (pd != null)
            {
                if(periodStart >= pd.MC_Begin.AddDays(14))
                {
                    PeriodInfo info = new PeriodInfo()
                    {
                        MC_Begin = periodStart,
                        MC_Days = pd.MC_Days,
                        MC_Cycle = Convert.ToInt32(periodStart.Subtract(pd.MC_Begin).TotalDays)
                    };
                    addPeriodData(info, User.Identity.Name);
                    return Json(new { result = "SUCCESS" });
                }
                else
                {
                    PeriodInfo info = new PeriodInfo()
                    {
                        MC_Begin = periodStart,
                        MC_Days = pd.MC_Days,
                        MC_Cycle = pd.MC_Cycle
                    };
                    updatePeriodData(info, User.Identity.Name);
                    return Json(new { result = "SUCCESS" });
                }
            }
            return Json(new { result = "FAIL" });
        }

        /********** 个人设置 **********/
        [Authorize]
        public ActionResult Setting()
        {
            var current_user = UserManager.FindByName(User.Identity.Name);
            ViewBag.User = current_user;
            // 填充年龄下拉框
            List<Object> age = new List<Object>();
            for(int i = 14; i <= 70; i++)
            {
                age.Add(new { name = i, value = i });
            }
            ViewBag.Age = new SelectList(age, "name", "value", 23);
            // 填充经期下拉框
            List<Object> periodday = new List<Object>();
            for (int i = 3; i <= 13; i++)
            {
                periodday.Add(new { name = i, value = i });
            }
            ViewBag.PeriodDay = new SelectList(periodday, "name", "value", 7);
            // 填充周期下拉框
            List<Object> cycle = new List<Object>();
            for (int i = 14; i <= 60; i++)
            {
                cycle.Add(new { name = i, value = i });
            }
            ViewBag.Cycle = new SelectList(age, "name", "value", 28);
            // 判断是否首次设置
            var setting_item = periodDB.PeriodUserInfo.SingleOrDefault(m => m.UserName == User.Identity.Name);
            if (setting_item != null)
            {
                PeriodUserInfoViewModel model = new PeriodUserInfoViewModel()
                {
                    UserName = setting_item.UserName,
                    Pregnancy = setting_item.Pregnancy,
                    Pre_Pregnancy = setting_item.Pre_Pregnancy,
                    Last_MC_Begin = setting_item.Last_MC_Begin,
                    UserAge = setting_item.UserAge,
                    MC_Cycle = setting_item.MC_Cycle,
                    MC_days = setting_item.MC_days
                };
                return View(model);
            }
            else
            {
                PeriodUserInfoViewModel model = new PeriodUserInfoViewModel()
                {
                    UserName = User.Identity.Name,
                    Last_MC_Begin = DateTime.Today,
                    UserAge = 23,
                    MC_days = 7,
                    MC_Cycle = 28,
                    Pregnancy = false,
                    Pre_Pregnancy = false
                };
            return View(model);
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Setting(PeriodUserInfoViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // 判断日期是否合法
                    DateTime currentDate = DateTime.Now.Date;
                    if(model.Last_MC_Begin > currentDate || model.Last_MC_Begin < currentDate.AddDays(-60))
                    {
                        // 判断为不合法
                        #region
                        if (model.Last_MC_Begin > currentDate)
                            ModelState.AddModelError("Last_MC_Begin", "上次经期大于当前日期");
                        else
                            ModelState.AddModelError("Last_MC_Begin", "上次经期小于当前日期前60天");
                        var current_user = UserManager.FindByName(User.Identity.Name);
                        ViewBag.User = current_user;
                        // 填充年龄下拉框
                        List<Object> age = new List<Object>();
                        for (int i = 14; i <= 70; i++)
                        {
                            age.Add(new { name = i, value = i });
                        }
                        ViewBag.Age = new SelectList(age, "name", "value", 23);
                        // 填充经期下拉框
                        List<Object> periodday = new List<Object>();
                        for (int i = 3; i <= 13; i++)
                        {
                            periodday.Add(new { name = i, value = i });
                        }
                        ViewBag.PeriodDay = new SelectList(periodday, "name", "value", 7);
                        // 填充周期下拉框
                        List<Object> cycle = new List<Object>();
                        for (int i = 14; i <= 60; i++)
                        {
                            cycle.Add(new { name = i, value = i });
                        }
                        ViewBag.Cycle = new SelectList(age, "name", "value", 28);
                        //ModelState.AddModelError("", "设置错误");
                        return View(model);
                        #endregion
                    }
                    // 判断是否初次记录
                    var setting_item = periodDB.PeriodUserInfo.SingleOrDefault(m => m.UserName == User.Identity.Name);
                    if (setting_item == null)
                    {
                        // 无设置记录，新建记录
                        PeriodUserInfo info = new PeriodUserInfo()
                        {
                            MC_days = model.MC_days,
                            Last_MC_Begin = model.Last_MC_Begin,
                            MC_Cycle = model.MC_Cycle,
                            Pregnancy = model.Pregnancy,
                            Pre_Pregnancy = model.Pre_Pregnancy,
                            UserAge = model.UserAge,
                            UserName = model.UserName
                        };
                        periodDB.PeriodUserInfo.Add(info);
                        periodDB.SaveChanges();
                        PeriodInfo pi = new PeriodInfo()
                        {
                            MC_Begin = model.Last_MC_Begin,
                            MC_Days = model.MC_days,
                            MC_Cycle = model.MC_Cycle
                        };
                        addPeriodData(pi, User.Identity.Name);
                        return RedirectToAction("UserHome");
                    }
                    if(model.Last_MC_Begin < setting_item.Last_MC_Begin.AddDays(-14))
                    {
                        // 小于上次记录的14天
                        #region
                        ModelState.AddModelError("Last_MC_Begin", "上次经期小于最近经期前14天");
                        var current_user = UserManager.FindByName(User.Identity.Name);
                        ViewBag.User = current_user;
                        // 填充年龄下拉框
                        List<Object> age = new List<Object>();
                        for (int i = 14; i <= 70; i++)
                        {
                            age.Add(new { name = i, value = i });
                        }
                        ViewBag.Age = new SelectList(age, "name", "value", 23);
                        // 填充经期下拉框
                        List<Object> periodday = new List<Object>();
                        for (int i = 3; i <= 13; i++)
                        {
                            periodday.Add(new { name = i, value = i });
                        }
                        ViewBag.PeriodDay = new SelectList(periodday, "name", "value", 7);
                        // 填充周期下拉框
                        List<Object> cycle = new List<Object>();
                        for (int i = 14; i <= 60; i++)
                        {
                            cycle.Add(new { name = i, value = i });
                        }
                        ViewBag.Cycle = new SelectList(age, "name", "value", 28);
                        //ModelState.AddModelError("", "设置错误");
                        return View(model);
                        #endregion
                    }
                    else if(model.Last_MC_Begin >= setting_item.Last_MC_Begin.AddDays(15))
                    {
                        // 大于上次记录的15天，新增记录
                        PeriodInfo pi = new PeriodInfo()
                        {
                            MC_Begin = model.Last_MC_Begin,
                            MC_Days = model.MC_days,
                            MC_Cycle = model.MC_Cycle
                        };
                        addPeriodData(pi, User.Identity.Name);
                    }
                    else
                    {
                        // 中间日期
                        // 判断是否小于再上次记录的前14天
                        var lpd = getLastPeriodData(setting_item.Last_MC_Begin, User.Identity.Name);
                        if (lpd != null)
                        {
                            if (model.Last_MC_Begin < lpd.MC_Begin.AddDays(14))
                            {
                                ModelState.AddModelError("Last_MC_Begin", "上次经期小于最近经期前14天");
                                var current_user = UserManager.FindByName(User.Identity.Name);
                                ViewBag.User = current_user;
                                // 填充年龄下拉框
                                List<Object> age = new List<Object>();
                                for (int i = 14; i <= 70; i++)
                                {
                                    age.Add(new { name = i, value = i });
                                }
                                ViewBag.Age = new SelectList(age, "name", "value", 23);
                                // 填充经期下拉框
                                List<Object> periodday = new List<Object>();
                                for (int i = 3; i <= 13; i++)
                                {
                                    periodday.Add(new { name = i, value = i });
                                }
                                ViewBag.PeriodDay = new SelectList(periodday, "name", "value", 7);
                                // 填充周期下拉框
                                List<Object> cycle = new List<Object>();
                                for (int i = 14; i <= 60; i++)
                                {
                                    cycle.Add(new { name = i, value = i });
                                }
                                ViewBag.Cycle = new SelectList(age, "name", "value", 28);
                                //ModelState.AddModelError("", "设置错误");
                                return View(model);
                            }
                        }
                        PeriodInfo pi = new PeriodInfo()
                        {
                            MC_Begin = model.Last_MC_Begin,
                            MC_Days = model.MC_days,
                            MC_Cycle = model.MC_Cycle
                        };
                        updatePeriodData(pi, User.Identity.Name);
                    }
                    return RedirectToAction("UserHome");
                    #region
                    /*
                    var setting_item = periodDB.PeriodUserInfo.SingleOrDefault(m => m.UserName == User.Identity.Name);
                    if (setting_item != null)
                    {
                        setting_item.MC_days = model.MC_days;
                        setting_item.Last_MC_Begin = model.Last_MC_Begin;
                        setting_item.MC_Cycle = model.MC_Cycle;
                        setting_item.Pregnancy = model.Pregnancy;
                        setting_item.Pre_Pregnancy = model.Pre_Pregnancy;
                        setting_item.UserAge = model.UserAge;

                        var last_item = (from m in periodDB.PeriodData
                                         where m.UserName == User.Identity.Name
                                         orderby m.MC_Begin descending
                                         select m).FirstOrDefault();
                        // 更新历史数据
                        if (last_item != null)
                        {
                            var modify_last = periodDB.PeriodData.SingleOrDefault(m => m.MC_Finish == last_item.MC_Begin);
                            
                            // 调整上一次的月经数据
                            if (modify_last != null)
                            {
                                if (model.Last_MC_Begin < modify_last.MC_Begin.AddDays(14))
                                {
                                    ModelState.AddModelError("Last_MC_Begin", "上次经期日期错误");
                                    var current_user = UserManager.FindByName(User.Identity.Name);
                                    ViewBag.User = current_user;
                                    // 填充年龄下拉框
                                    List<Object> age = new List<Object>();
                                    for (int i = 14; i <= 70; i++)
                                    {
                                        age.Add(new { name = i, value = i });
                                    }
                                    ViewBag.Age = new SelectList(age, "name", "value", 23);
                                    // 填充经期下拉框
                                    List<Object> periodday = new List<Object>();
                                    for (int i = 3; i <= 12; i++)
                                    {
                                        periodday.Add(new { name = i, value = i });
                                    }
                                    ViewBag.PeriodDay = new SelectList(periodday, "name", "value", 7);
                                    // 填充周期下拉框
                                    List<Object> cycle = new List<Object>();
                                    for (int i = 14; i <= 60; i++)
                                    {
                                        cycle.Add(new { name = i, value = i });
                                    }
                                    ViewBag.Cycle = new SelectList(age, "name", "value", 28);
                                    //ModelState.AddModelError("", "设置错误");
                                    return View(model);
                                }
                                else
                                {
                                    last_item.MC_Begin = model.Last_MC_Begin;
                                    last_item.MC_Cycle = model.MC_Cycle;
                                    last_item.MC_Days = model.MC_days;
                                    last_item.MC_Finish = model.Last_MC_Begin.AddDays(model.MC_Cycle);
                                    modify_last.MC_Finish = model.Last_MC_Begin;
                                    modify_last.MC_Cycle = Convert.ToInt32(model.Last_MC_Begin.Subtract(modify_last.MC_Begin).TotalDays);
                                }
                            }
                        }
                        else
                        {
                            PeriodData data = new PeriodData()
                            {
                                MC_Begin = model.Last_MC_Begin,
                                MC_Cycle = model.MC_Cycle,
                                MC_Days = model.MC_days,
                                UserName = User.Identity.Name,
                                MC_Finish = model.Last_MC_Begin.AddDays(model.MC_Cycle),
                                Period_Type = 0 // 默认为普通类型
                            };
                            periodDB.PeriodData.Add(data);
                        }

                        periodDB.SaveChanges();

                        return RedirectToAction("UserHome");
                    }
                    else
                    {
                        PeriodUserInfo info = new PeriodUserInfo()
                        {
                            MC_days = model.MC_days,
                            Last_MC_Begin = model.Last_MC_Begin,
                            MC_Cycle = model.MC_Cycle,
                            Pregnancy = model.Pregnancy,
                            Pre_Pregnancy = model.Pre_Pregnancy,
                            UserAge = model.UserAge,
                            UserName = model.UserName
                        };
                        periodDB.PeriodUserInfo.Add(info);
                        // 首次数据必定添加
                        PeriodData data = new PeriodData()
                        {
                            MC_Begin = model.Last_MC_Begin,
                            MC_Cycle = model.MC_Cycle,
                            MC_Days = model.MC_days, 
                            UserName = User.Identity.Name,
                            MC_Finish = model.Last_MC_Begin.AddDays(model.MC_Cycle),
                            Period_Type = 0 // 默认为普通类型

                        };
                        periodDB.PeriodData.Add(data);
                        periodDB.SaveChanges();
                        return RedirectToAction("UserHome");
                    }
                    */
                    #endregion
                }
                catch
                {
                    return View("Error");
                }
            }
            else
            {
                var current_user = UserManager.FindByName(User.Identity.Name);
                ViewBag.User = current_user;
                // 填充年龄下拉框
                List<Object> age = new List<Object>();
                for (int i = 14; i <= 70; i++)
                {
                    age.Add(new { name = i, value = i });
                }
                ViewBag.Age = new SelectList(age, "name", "value", 23);
                // 填充经期下拉框
                List<Object> periodday = new List<Object>();
                for (int i = 3; i <= 12; i++)
                {
                    periodday.Add(new { name = i, value = i });
                }
                ViewBag.PeriodDay = new SelectList(periodday, "name", "value", 7);
                // 填充周期下拉框
                List<Object> cycle = new List<Object>();
                for (int i = 14; i <= 60; i++)
                {
                    cycle.Add(new { name = i, value = i });
                }
                ViewBag.Cycle = new SelectList(age, "name", "value", 28);
                ModelState.AddModelError("", "设置错误");
                return View(model);
            }
            //return RedirectToAction("UserHome");
        }


        #region 经期助手辅助程序
        /******** 经期助手辅助程序 ********/
        /// <summary>
        /// 更新最近一次的经期数据，并同步到PeriodUserInfo
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userName"></param>
        private void updatePeriodData(PeriodInfo data, string userName)
        {
            // 获取最近一次的经期数据
            PeriodData pd = (from m in periodDB.PeriodData
                                   where m.UserName == userName
                                   orderby m.MC_Begin descending
                                   select m).FirstOrDefault();
            if (pd != null)
            {
                
                // 修改上一次经期结束时间
                PeriodData lpd = getLastPeriodData(pd.MC_Begin, userName);
                if (lpd != null)
                {
                    lpd.MC_Finish = data.MC_Begin;
                    lpd.MC_Cycle = Convert.ToInt32(lpd.MC_Finish.Subtract(lpd.MC_Begin).TotalDays);
                }
                // 修改记录
                pd.MC_Begin = data.MC_Begin;
                pd.MC_Cycle = data.MC_Cycle;
                pd.MC_Days = data.MC_Days;
                pd.MC_Finish = data.MC_Begin.AddDays(data.MC_Cycle);
                // 更新PeriodUserInfo
                var pui = periodDB.PeriodUserInfo.SingleOrDefault(m => m.UserName == userName);
                if (pui != null)
                {
                    pui.Last_MC_Begin = data.MC_Begin;
                    pui.MC_Cycle = data.MC_Cycle;
                    pui.MC_days = data.MC_Days;
                    periodDB.SaveChanges();
                }
                // 保存数据
                periodDB.SaveChanges();
            }
        }
        /// <summary>
        /// 新增PeriodData数据并且更新到PeriodUserInfo
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userName"></param>
        private void addPeriodData(PeriodInfo data, string userName)
        {
            PeriodData pd = new PeriodData()
            {
                MC_Begin = data.MC_Begin,
                MC_Days = data.MC_Days,
                MC_Cycle = data.MC_Cycle,
                MC_Finish = data.MC_Begin.AddDays(data.MC_Cycle),
                Period_Type = 1,
                UserName = userName
            };
            periodDB.PeriodData.Add(pd);
            // 修改再上次经期的结束时间
            PeriodData lpd = getLastPeriodData(userName);
            if (lpd != null)
            {
                lpd.MC_Finish = data.MC_Begin;
                lpd.MC_Cycle = Convert.ToInt32(lpd.MC_Finish.Subtract(lpd.MC_Begin).TotalDays);
            }
            var pui = periodDB.PeriodUserInfo.SingleOrDefault(m => m.UserName == userName);
            if (pui != null)
            {
                pui.Last_MC_Begin = data.MC_Begin;
                pui.MC_Cycle = data.MC_Cycle;
                pui.MC_days = data.MC_Days;
                periodDB.SaveChanges();
            }
            // 保存数据
            periodDB.SaveChanges();
        }
        private PeriodData getLastPeriodData(DateTime date, string userName)
        {
            var lpd = periodDB.PeriodData.SingleOrDefault(m => m.MC_Finish == date && m.UserName == userName);
            return lpd;
        }
        private PeriodData getLastPeriodData(string userName)
        {
            var lpd = (from m in periodDB.PeriodData
                       where m.UserName == userName
                       orderby m.MC_Begin descending
                       select m).FirstOrDefault();
            return lpd;
        }
        #endregion


        /******** 活动中心 ********/
        [Authorize]
        public ActionResult SaleOff()
        {
            var setting_item = periodDB.PeriodUserInfo.SingleOrDefault(m => m.UserName == User.Identity.Name);
            WeChatUtilities utilities = new WeChatUtilities();
            string _url = ViewBag.Url = Request.Url.ToString();
            ViewBag.AppId = utilities.getAppId();
            string _nonce = CommonUtilities.generateNonce();
            ViewBag.Nonce = _nonce;
            string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
            ViewBag.TimeStamp = _timeStamp;
            ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url);
            if (setting_item != null)
            {
                DateTime today = DateTime.Now.Date;
                var SignInRecord = periodDB.CreditsRecord.SingleOrDefault(m => m.RecordDate > today && m.CreditsType_Id == 1 && m.UserName == User.Identity.Name);
                if (SignInRecord == null)
                {
                    ViewBag.enableSignIn = true;
                }
                else
                {
                    ViewBag.enableSignIn = false;
                }
                var current_user = UserManager.FindByName(User.Identity.Name);
                return View(current_user);
            }
            else
            {
                return RedirectToAction("Setting");
            }
        }

        [HttpPost]
        [Authorize]
        public JsonResult CreditsSignIn()
        {
            try {
                DateTime today = DateTime.Now.Date;
                var SignInRecord = periodDB.CreditsRecord.SingleOrDefault(m => m.RecordDate > today && m.CreditsType_Id == 1 && m.UserName == User.Identity.Name);
                if (SignInRecord == null)
                {
                    CreditsRecord record = new CreditsRecord()
                    {
                        Credits = 2,
                        RecordDate = DateTime.Now,
                        CreditsType_Id = 1,
                        UserName = User.Identity.Name
                    };
                    periodDB.CreditsRecord.Add(record);
                    periodDB.SaveChanges();
                    var current_user = UserManager.FindByName(User.Identity.Name);
                    current_user.Credits += 2;
                    UserManager.Update(current_user);
                    return Json(new { result = "SUCCESS", message = current_user.Credits });
                }
                else
                {
                    return Json(new { result = "FAIL" });
                }
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
        }

        [HttpPost]
        [Authorize]
        public JsonResult ShareTimeline()
        {
            try {
                CreditsRecord record = new CreditsRecord()
                {
                    Credits = 20,
                    RecordDate = DateTime.Now,
                    CreditsType_Id = 2,
                    UserName = User.Identity.Name
                };
                periodDB.CreditsRecord.Add(record);
                periodDB.SaveChanges();
                var current_user = UserManager.FindByName(User.Identity.Name);
                current_user.Credits += 20;
                UserManager.Update(current_user);
                return Json(new { result = "SUCCESS", message = current_user.Credits });
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
        }

        [HttpPost]
        [Authorize]
        public JsonResult ShareAppMessage()
        {
            try
            {
                CreditsRecord record = new CreditsRecord()
                {
                    Credits = 2,
                    RecordDate = DateTime.Now,
                    CreditsType_Id = 3,
                    UserName = User.Identity.Name
                };
                periodDB.CreditsRecord.Add(record);
                periodDB.SaveChanges();
                var current_user = UserManager.FindByName(User.Identity.Name);
                current_user.Credits += 2;
                UserManager.Update(current_user);
                return Json(new { result = "SUCCESS", message = current_user.Credits });
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
        }

        #region 个人首页
        /******** 个人首页 ********/
        [Authorize]
        public ActionResult UserHome()
        {
            var setting_item = periodDB.PeriodUserInfo.SingleOrDefault(m => m.UserName == User.Identity.Name);
            if (setting_item != null)
            {
                var current_user = UserManager.FindByName(User.Identity.Name);
                return View(current_user);
            }
            else
            {
                return RedirectToAction("Setting");
            }
        }
        #endregion
        
        #region 帮助文档
        [Authorize]
        public PartialViewResult Version()
        {
            return PartialView();
        }

        [Authorize]
        public PartialViewResult Privacy()
        {
            return PartialView();
        }

        [Authorize]
        public PartialViewResult About()
        {
            return PartialView();
        }
        #endregion
    }
}
