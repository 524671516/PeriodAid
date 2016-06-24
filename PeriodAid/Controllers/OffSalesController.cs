using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

using PagedList;
using PeriodAid.Models;
using PeriodAid.Filters;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;

namespace PeriodAid.Controllers
{
    [Authorize(Roles ="Admin")]
    public class OffSalesController : Controller
    {
        // GET: OffStore
        OfflineSales _offlineDB = new OfflineSales();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public OffSalesController()
        {

        }

        public OffSalesController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        }// GET: Sales
        public ActionResult Index()
        {
            return View();
        }

        // Origin:Off_Sales_main
        public ActionResult DailySalesIndex()
        {
            return View();
        }

        // Origin:Off_DailySalesInfo_ajaxlist
        public ActionResult DailySalesListPartial(int? page, string query)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            int _page = page ?? 1;
            if (query == null || query == "")
            {
                var list = (from m in _offlineDB.Off_SalesInfo_Daily
                            where m.Off_Store.Off_System_Id == user.DefaultSystemId
                            orderby m.Date descending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
            else
            {
                var list = (from m in _offlineDB.Off_SalesInfo_Daily
                            where m.Off_Store.StoreName.Contains(query) && m.Off_Store.Off_System_Id == user.DefaultSystemId
                            orderby m.Date descending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
        }
        // Origin:Ajax_EditDailyInfo
        public PartialViewResult EditDailySalesPartial(int id)
        {
            var item = _offlineDB.Off_SalesInfo_Daily.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_Store.Off_System_Id == user.DefaultSystemId)
                {
                    var sellerlist = from m in _offlineDB.Off_Seller
                                     where m.StoreId == item.StoreId
                                     select m;
                    ViewBag.Sellerlist = new SelectList(sellerlist, "Id", "Name");
                    List<Object> attendance = new List<Object>();
                    attendance.Add(new { Key = 0, Value = "全勤" });
                    attendance.Add(new { Key = 1, Value = "迟到" });
                    attendance.Add(new { Key = 2, Value = "早退" });
                    attendance.Add(new { Key = 3, Value = "旷工" });
                    ViewBag.Attendancelist = new SelectList(attendance, "Key", "Value", item.Attendance);
                    return PartialView(item);
                }
                else
                {
                    return PartialView("AuthorizeErrorPartial");
                }
            }
            else
            {
                return PartialView("ErrorPartial");
            }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EditDailySalesPartial(int id, FormCollection form)
        {
            var dailysales = _offlineDB.Off_SalesInfo_Daily.SingleOrDefault(m => m.Id == id);
            if (dailysales != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (dailysales.Off_Store.Off_System_Id == user.DefaultSystemId)
                {
                    var item = new Off_SalesInfo_Daily();
                    if (TryUpdateModel(item))
                    {
                        List<int> plist = new List<int>();
                        var productlist = from m in _offlineDB.Off_Product
                                          where m.Off_System_Id == user.DefaultSystemId
                                          && m.status >= 0
                                          select m;
                        // 添加或修改销售列表
                        foreach (var product in productlist)
                        {
                            // 获取单品数据
                            int? sales = null;
                            if (form["sales_" + product.Id] != "")
                                sales = Convert.ToInt32(form["sales_" + product.Id]);
                            int? storage = null;
                            if (form["storage_" + product.Id] != "")
                                storage = Convert.ToInt32(form["storage_" + product.Id]);
                            decimal? amount = null;
                            if (form["amount_" + product.Id] != "")
                                amount = Convert.ToDecimal(form["amount_" + product.Id]);
                            // 判断是否已有数据
                            var checkinproductlist = _offlineDB.Off_Daily_Product.Where(m => m.DailyId == id);
                            var existdata = checkinproductlist.SingleOrDefault(m => m.ProductId == product.Id);
                            if (existdata != null)
                            {
                                if (sales == null && storage == null && amount == null)
                                {
                                    // 无数据则删除
                                    _offlineDB.Off_Daily_Product.Remove(existdata);
                                }
                                else if (sales == 0 && storage == 0 && amount == 0)
                                {
                                    _offlineDB.Off_Daily_Product.Remove(existdata);
                                }
                                else
                                {
                                    // 修改数据
                                    existdata.SalesAmount = amount;
                                    existdata.SalesCount = sales;
                                    existdata.StorageCount = storage;
                                }
                            }
                            else
                            {
                                // 添加数据
                                // 如果三项数据不为空，则添加
                                if (sales == null && storage == null && amount == null)
                                { }
                                else if (sales == 0 && storage == 0 && amount == 0)
                                {

                                }
                                else
                                {
                                    existdata = new Off_Daily_Product()
                                    {
                                        DailyId = id,
                                        ItemCode = product.ItemCode,
                                        ProductId = product.Id,
                                        SalesAmount = amount,
                                        SalesCount = sales,
                                        StorageCount = storage
                                    };
                                    _offlineDB.Off_Daily_Product.Add(existdata);
                                    //offlineDB.SaveChanges();
                                }
                            }
                        }
                        item.UploadTime = DateTime.Now;
                        item.UploadUser = User.Identity.Name;
                        _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        _offlineDB.SaveChanges();
                        // 该代码需要重写
                        
                        
                        //update_Sales_AVGINfo(item.StoreId, (int)item.Date.DayOfWeek + 1);
                        return Content("SUCCESS");
                    }
                    else
                    {
                        ModelState.AddModelError("", "错误");
                        var sellerlist = from m in _offlineDB.Off_Seller
                                         where m.StoreId == item.StoreId
                                         select m;
                        ViewBag.Sellerlist = new SelectList(sellerlist, "Id", "Name");
                        List<Object> attendance = new List<Object>();
                        attendance.Add(new { Key = 0, Value = "全勤" });
                        attendance.Add(new { Key = 1, Value = "迟到" });
                        attendance.Add(new { Key = 2, Value = "早退" });
                        attendance.Add(new { Key = 3, Value = "旷工" });
                        ViewBag.Attendance = new SelectList(attendance, "Key", "Value", item.Attendance);
                        return PartialView(item);
                    }
                }
                else
                {
                    return PartialView("AuthorizeErrorPartial");
                }
            }
            else
            {
                return PartialView("ErrorPartial");
            }

        }

        // Origin:Off_DailyProductList
        public PartialViewResult DailySalesProductListPartial(int DailyId)
        {
            var item = _offlineDB.Off_SalesInfo_Daily.SingleOrDefault(m => m.Id == DailyId);
            var model = item.Off_Daily_Product;
            return PartialView(model);
        }
        // Origin:Off_DailyInfo_Add_ProductList
        public JsonResult DailySalesAllProductAjax()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var product = from m in _offlineDB.Off_Product
                          where m.Off_System_Id == user.DefaultSystemId && m.status >= 0
                          select new { Id = m.Id, ItemCode = m.ItemCode, SimpleName = m.SimpleName };
            return Json(new { result = "SUCCESS", data = product }, JsonRequestBehavior.AllowGet);
        }

        // Origin:Off_Sales_Month
        [SettingFilter(SettingName = "GENERAL")]
        public ActionResult MonthSalesList()
        {
            return View();
        }

        // Origin: Off_MonthSalesInfo_ajaxlist
        [SettingFilter(SettingName = "GENERAL")]
        public ActionResult MonthSalesListPartial(int? page, string query)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            int _page = page ?? 1;
            if (query == null || query == "")
            {
                var list = (from m in _offlineDB.Off_SalesInfo_Month
                            where m.Off_Store.Off_System_Id == user.DefaultSystemId
                            orderby m.Date descending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
            else
            {
                var list = (from m in _offlineDB.Off_SalesInfo_Month
                            where m.Off_Store.StoreName.Contains(query) && m.Off_Store.Off_System_Id == user.DefaultSystemId
                            orderby m.Date descending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
        }
        // Orgin:Ajax_EditMonthInfo
        public PartialViewResult EditMonthSalesPartial(int id)
        {
            var item = _offlineDB.Off_SalesInfo_Month.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_Store.Off_System_Id == user.DefaultSystemId)
                {
                    return PartialView(item);
                }
                else
                {
                    return PartialView("AuthorizeErrorPartial");
                }
            }
            else
            {
                return PartialView("ErrorPartial");
            }
        }
        [HttpPost]
        public ActionResult EditMonthSalesPartial(int id, FormCollection form)
        {
            var monthsales = _offlineDB.Off_SalesInfo_Month.SingleOrDefault(m => m.Id == id);
            if (monthsales != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (monthsales.Off_Store.Off_System_Id == user.DefaultSystemId)
                {
                    var item = new Off_SalesInfo_Month();
                    if (TryUpdateModel(item))
                    {
                        item.UploadTime = DateTime.Now;
                        item.UploadUser = User.Identity.Name;
                        _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        _offlineDB.SaveChanges();
                        return Content("SUCCESS");
                    }
                    else
                    {
                        ModelState.AddModelError("", "错误");
                        return PartialView(item);
                    }
                }
                else
                {
                    return PartialView("AuthorizeErrorPartial");
                }
            }
            else
            {
                return PartialView("ErrorPartial");
            }
        }
        // Origin: Off_ScheduleList(原本是单页面，现在做成ajax+partial的形式)
        public ActionResult ScheduleList()
        {
            return View();
        }
        public ActionResult ScheduleListPartial(bool? history, int? page)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            bool _history = history ?? true;
            int _page = page ?? 1;
            var currentTime = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            if (_history)
            {
                var list = (from m in _offlineDB.Off_Checkin_Schedule
                           where m.Subscribe <= currentTime && m.Off_System_Id == user.DefaultSystemId
                           group m by m.Subscribe into g
                           orderby g.Key descending
                           select new ScheduleList { Subscribe = g.Key, Count = g.Count(), Unfinished = g.Count(m => m.Off_Checkin.Any(p => p.Status >= 3)) }).ToPagedList(_page, 20);
                return View(list);
            }
            else
            {
                var list = (from m in _offlineDB.Off_Checkin_Schedule
                           where m.Subscribe > currentTime && m.Off_System_Id == user.DefaultSystemId
                           group m by m.Subscribe into g
                           orderby g.Key
                           select new ScheduleList { Subscribe = g.Key, Count = g.Count(), Unfinished = g.Count(m => m.Off_Checkin.Any(p => p.Status >= 3)) }).ToPagedList(_page, 20);
                return View(list);
            }
        }
        // Origin: Off_ScheduleDetails(原本是单页面，现在做成ajax+partial的形式)
        public ActionResult ViewScheduleDetails(string date)
        {
            ViewBag.Date = date;
            return View();
        }
        public PartialViewResult ViewScheduleDetailsPartial(string date, string query, int? page)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            DateTime day = DateTime.Parse(date);
            int _page = page ?? 1;
            if (query != null)
            {
                var list = (from m in _offlineDB.Off_Checkin_Schedule
                           where m.Subscribe == day && m.Off_Store.StoreName.Contains(query) && m.Off_System_Id == user.DefaultSystemId
                           orderby m.Off_Store.StoreName
                           select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
            else
            {
                var list = (from m in _offlineDB.Off_Checkin_Schedule
                           where m.Subscribe == day && m.Off_System_Id == user.DefaultSystemId
                           orderby m.Off_Store.StoreName
                           select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
        }
        
        // Origin: Ajax_EditSchedule
        public ActionResult EditSchedulePartial(int id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var item = _offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                if (item.Off_System_Id == user.DefaultSystemId)
                {
                    ViewBag.StoreName = item.Off_Store.StoreName;
                    var TemplateList = from m in _offlineDB.Off_Sales_Template
                                       where m.Off_System_Id == user.DefaultSystemId && m.Status >= 0
                                       orderby m.TemplateName
                                       select new { Key = m.Id, Value = m.TemplateName };
                    ViewBag.TemplateList = new SelectList(TemplateList, "Key", "Value");
                    return PartialView(item);
                }
                else
                {
                    return PartialView("AuthorizeErrorPartial");
                }
            }
            else
            {
                return PartialView("ErrorPartial");
            }
        }
        [ValidateAntiForgeryToken, HttpPost]
        public ActionResult EditSchedulePartial(Off_Checkin_Schedule model)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin_Schedule item = new Off_Checkin_Schedule();
                if (TryUpdateModel(item))
                {
                    _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    _offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "请重试");
                var user = UserManager.FindById(User.Identity.GetUserId());
                ViewBag.StoreName = _offlineDB.Off_Store.SingleOrDefault(m => m.Id == model.Off_Store_Id).StoreName;
                var TemplateList = from m in _offlineDB.Off_Sales_Template
                                   where m.Off_System_Id == user.DefaultSystemId && m.Status >= 0
                                   orderby m.TemplateName
                                   select new { Key = m.Id, Value = m.TemplateName };
                ViewBag.TemplateList = new SelectList(TemplateList, "Key", "Value");
                return PartialView(model);
            }
        }

        // Origin: Ajax_DeleteSchedule(原来直接返回success,现在返回json)
        [HttpPost]
        public ActionResult Ajax_DeleteSchedule(int id)
        {
            var item = _offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_System_Id == user.DefaultSystemId)
                {
                    _offlineDB.Off_Checkin_Schedule.Remove(item);
                    _offlineDB.SaveChanges();
                    return Json(new { data = "SUCCESS" });
                }
                else
                    return Json(new { data = "UNAUTHORIZED" });
            }
            else
                return Json(new { data = "FAIL" });
        }

        // Origin: Off_Add_Schedule
        public ActionResult AddSchedule()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var storesystem = from m in _offlineDB.Off_Store
                              where m.Off_System_Id == user.DefaultSystemId
                              group m by m.StoreSystem into g
                              orderby g.Key
                              select new { Key = g.Key, Value = g.Key };
            var TemplateList = from m in _offlineDB.Off_Sales_Template
                               where m.Off_System_Id == user.DefaultSystemId && m.Status >= 0
                               orderby m.TemplateName
                               select new { Key = m.Id, Value = m.TemplateName };
            ViewBag.TemplateList = new SelectList(TemplateList, "Key", "Value");
            ViewBag.SystemList = new SelectList(storesystem, "Key", "Value", storesystem.FirstOrDefault().Value);
            return View();
        }
        [HttpPost]
        public ActionResult Add_Schedule(FormCollection form)
        {
            StoreSchedule_ViewModel model = new StoreSchedule_ViewModel();
            if (TryUpdateModel(model))
            {
                if (ModelState.IsValid)
                {
                    if (model.StartDate > model.EndDate)
                    {
                        var storesystem = from m in _offlineDB.Off_Store
                                          group m by m.StoreSystem into g
                                          orderby g.Key
                                          select new { Key = g.Key, Value = g.Key };
                        ViewBag.SystemList = new SelectList(storesystem, "Key", "Value", storesystem.FirstOrDefault().Value);
                        ModelState.AddModelError("", "开始日期不得大于结束日期");
                        return View(model);
                    }
                    if (form["StoreList"].ToString().Trim() == "")
                    {
                        var storesystem = from m in _offlineDB.Off_Store
                                          group m by m.StoreSystem into g
                                          orderby g.Key
                                          select new { Key = g.Key, Value = g.Key };
                        ViewBag.SystemList = new SelectList(storesystem, "Key", "Value", storesystem.FirstOrDefault().Value);
                        ModelState.AddModelError("", "请至少选择一个店铺");
                        return View(model);
                    }
                    var datelength = Convert.ToInt32(model.EndDate.Subtract(model.StartDate).TotalDays);
                    // 每天循环
                    for (int i = 0; i <= datelength; i++)
                    {
                        var user = UserManager.FindById(User.Identity.GetUserId());
                        string[] storelist = form["StoreList"].ToString().Split(',');
                        for (int j = 0; j < storelist.Length; j++)
                        {
                            string[] begintime = model.BeginTime.Split(':');
                            string[] finishtime = model.FinishTime.Split(':');
                            int year = model.StartDate.AddDays(i).Year;
                            int month = model.StartDate.AddDays(i).Month;
                            int day = model.StartDate.AddDays(i).Day;
                            int storeid = Convert.ToInt32(storelist[j]);
                            DateTime subscribe = model.StartDate.AddDays(i);
                            var schedule = _offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Off_Store_Id == storeid && m.Subscribe == subscribe);
                            if (schedule == null)
                            {
                                schedule = new Off_Checkin_Schedule()
                                {
                                    Off_Store_Id = storeid,
                                    Subscribe = subscribe,
                                    Standard_CheckIn = new DateTime(year, month, day, Convert.ToInt32(begintime[0]), Convert.ToInt32(begintime[1]), 0),
                                    Standard_CheckOut = new DateTime(year, month, day, Convert.ToInt32(finishtime[0]), Convert.ToInt32(finishtime[1]), 0),
                                    Standard_Salary = model.Salary,
                                    TemplateId = model.TemplateId,
                                    Off_System_Id = user.DefaultSystemId
                                };
                                _offlineDB.Off_Checkin_Schedule.Add(schedule);
                            }
                            else
                            {
                                schedule.Standard_CheckIn = new DateTime(year, month, day, Convert.ToInt32(begintime[0]), Convert.ToInt32(begintime[1]), 0);
                                schedule.Standard_CheckOut = new DateTime(year, month, day, Convert.ToInt32(finishtime[0]), Convert.ToInt32(finishtime[1]), 0);
                                schedule.Standard_Salary = model.Salary;
                                schedule.TemplateId = model.TemplateId;
                                _offlineDB.Entry(schedule).State = System.Data.Entity.EntityState.Modified;
                            }
                        }
                    }
                    _offlineDB.SaveChanges();
                    return RedirectToAction("ScheduleList");
                }
                else
                {
                    var user = UserManager.FindById(User.Identity.GetUserId());
                    var storesystem = from m in _offlineDB.Off_Store
                                      group m by m.StoreSystem into g
                                      orderby g.Key
                                      select new { Key = g.Key, Value = g.Key };
                    ViewBag.SystemList = new SelectList(storesystem, "Key", "Value", storesystem.FirstOrDefault().Value);
                    var TemplateList = from m in _offlineDB.Off_Sales_Template
                                       where m.Off_System_Id == user.DefaultSystemId && m.Status >= 0
                                       orderby m.TemplateName
                                       select new { Key = m.Id, Value = m.TemplateName };
                    ViewBag.TemplateList = new SelectList(TemplateList, "Key", "Value");
                    return View(model);
                }
            }
            return RedirectToAction("ScheduleList");
        }

        // Origin: Off_Add_Schedule_StoreList
        [HttpPost]
        public JsonResult ScheduleStoreListAjax(string storesystem)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in _offlineDB.Off_Store
                       where m.StoreSystem == storesystem && m.Off_System_Id == user.DefaultSystemId
                       orderby m.StoreName
                       select new { ID = m.Id, StoreName = m.StoreName };
            return Json(new { StoreList = list });
        }
       


        // Origin:UploadDailyInfo
        [SettingFilter(SettingName = "GENERAL")]
        public ActionResult UploadDailySales()
        {
            return View();
        }
        [HttpPost]
        [SettingFilter(SettingName = "GENERAL")]
        public async Task<ActionResult> UploadDailySales(FormCollection form)
        {
            var file = Request.Files[0];
            List<Excel_DataMessage> messageList = new List<Excel_DataMessage>();
            string time_ticks = DateTime.Now.Ticks.ToString();
            if (file != null)
            {
                //文件不得大于500K
                if (file.ContentLength > 1024 * 500)
                {
                    messageList.Add(new Excel_DataMessage(0, "文件大于500K", true));
                }
                else if (file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    messageList.Add(new Excel_DataMessage(0, "文件类型错误", true));
                }
                else
                {
                    string folder = HttpContext.Server.MapPath("~/Content/xlsx/");
                    string filename = time_ticks + file.FileName.Substring(file.FileName.LastIndexOf('.'));
                    file.SaveAs(folder + filename);
                    List<Excel_DataMessage> result = await UploadDailySalesByExcelAsync(filename, messageList);
                }
            }
            else
            {
                messageList.Add(new Excel_DataMessage(0, "文件上传错误", true));
            }
            return View("UploadResult", messageList);
        }


        

        
        // Origin:UploadMonthInfo
        [SettingFilter(SettingName = "GENERAL")]
        public ActionResult UploadMonthSales()
        {
            return View();
        }
        [HttpPost]
        [SettingFilter(SettingName = "GENERAL")]
        public async Task<ActionResult> UploadMonthSales(FormCollection form)
        {
            var file = Request.Files[0];
            List<Excel_DataMessage> messageList = new List<Excel_DataMessage>();
            string time_ticks = DateTime.Now.Ticks.ToString();
            if (file != null)
            {
                //文件不得大于500K
                if (file.ContentLength > 1024 * 500)
                {
                    messageList.Add(new Excel_DataMessage(0, "文件大于500K", true));
                }
                else if (file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    messageList.Add(new Excel_DataMessage(0, "文件类型错误", true));
                }
                else
                {
                    string folder = HttpContext.Server.MapPath("~/Content/xlsx/");
                    string filename = time_ticks + file.FileName.Substring(file.FileName.LastIndexOf('.'));
                    file.SaveAs(folder + filename);
                    List<Excel_DataMessage> result =await UploadMonthSalesByExcelAsync(filename, messageList);
                }
            }
            else
            {
                messageList.Add(new Excel_DataMessage(0, "文件上传错误", true));
            }
            return View("UploadResult", messageList);
        }
        
        // Origin:analyseExcel_MonthInfoTable
        public async Task<List<Excel_DataMessage>> UploadMonthSalesByExcelAsync(string filename, List<Excel_DataMessage> messageList)
        {
            try
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                string folder = HttpContext.Server.MapPath("~/Content/xlsx/");
                string strConn = "Provider=Microsoft.Ace.OleDb.12.0;" + "data source=" + folder + filename + ";Extended Properties='Excel 12.0; HDR=1; IMEX=1'"; //此连接可以操作.xls与.xlsx文件
                OleDbConnection conn = new OleDbConnection(strConn);
                conn.Open();
                DataSet ds = new DataSet();
                OleDbDataAdapter odda = new OleDbDataAdapter(string.Format("SELECT * FROM [{0}]", "Sheet1$"), conn);                    //("select * from [Sheet1$]", conn);
                odda.Fill(ds, "[Sheet1$]");
                conn.Close();
                DataTable dt = ds.Tables[0];
                int i = 0;
                bool result_flag = true;
                foreach (DataRow dr in dt.Rows)
                {
                    i++;
                    try
                    {
                        // 判断是否存在店铺
                        string storename = dr["店铺名称"].ToString();
                        var exist_store = _offlineDB.Off_Store.SingleOrDefault(m => m.StoreName == storename && m.Off_System_Id == user.DefaultSystemId);
                        if (exist_store == null)
                        {
                            messageList.Add(new Excel_DataMessage(i, "店铺不存在", true));
                            result_flag = false;
                            continue;

                        }
                        // 判断是否含已有数据
                        DateTime info_date = Convert.ToDateTime(dr["月份"]);
                        info_date = new DateTime(info_date.Year, info_date.Month, 1);
                        var exist_dailyinfo = _offlineDB.Off_SalesInfo_Daily.SingleOrDefault(m => m.Date.Year == info_date.Year && m.Date.Month == info_date.Month && m.StoreId == exist_store.Id);
                        if (exist_dailyinfo != null)
                        {
                            messageList.Add(new Excel_DataMessage(i, "当月数据已存在", true));
                            result_flag = false;
                            continue;
                        }
                        else
                        {
                            Off_SalesInfo_Month monthinfo = new Off_SalesInfo_Month()
                            {
                                StoreId = exist_store.Id,
                                Date = info_date,
                                Item_Brown = ExcelOperation.ConvertInt(dr, "红糖姜茶"),
                                Item_Black = ExcelOperation.ConvertInt(dr, "黑糖姜茶"),
                                Item_Lemon = ExcelOperation.ConvertInt(dr, "柠檬姜茶"),
                                Item_Honey = ExcelOperation.ConvertInt(dr, "蜂蜜姜茶"),
                                Item_Dates = ExcelOperation.ConvertInt(dr, "红枣姜茶"),
                                UploadTime = DateTime.Now,
                                UploadUser = User.Identity.Name
                            };
                            _offlineDB.Off_SalesInfo_Month.Add(monthinfo);
                            messageList.Add(new Excel_DataMessage(i, "数据类型验证成功", false));
                        }
                    }
                    catch (Exception e)
                    {
                        result_flag = false;
                        messageList.Add(new Excel_DataMessage(i, "表格格式错误," + e.ToString(), true));
                    }

                }
                if (result_flag)
                {
                    await _offlineDB.SaveChangesAsync();
                    messageList.Add(new Excel_DataMessage(0, "保存成功", false));
                }
                else
                {
                    messageList.Add(new Excel_DataMessage(0, "数据行发生错误，未保存", true));
                }
            }
            catch (Exception e)
            {
                messageList.Add(new Excel_DataMessage(-1, "表格格式错误" + e.ToString(), true));
            }
            return messageList;
        }

        
        

        // Origin: analyseExcel_DailyInfoTable
        public async Task<List<Excel_DataMessage>> UploadDailySalesByExcelAsync(string filename, List<Excel_DataMessage> messageList)
        {
            try
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                string folder = HttpContext.Server.MapPath("~/Content/xlsx/");
                string strConn = "Provider=Microsoft.Ace.OleDb.12.0;" + "data source=" + folder + filename + ";Extended Properties='Excel 12.0; HDR=1; IMEX=1'"; //此连接可以操作.xls与.xlsx文件
                OleDbConnection conn = new OleDbConnection(strConn);
                conn.Open();
                DataSet ds = new DataSet();
                OleDbDataAdapter odda = new OleDbDataAdapter(string.Format("SELECT * FROM [{0}]", "Sheet1$"), conn);                    //("select * from [Sheet1$]", conn);
                odda.Fill(ds, "[Sheet1$]");
                conn.Close();
                DataTable dt = ds.Tables[0];
                int i = 0;
                bool result_flag = true;
                foreach (DataRow dr in dt.Rows)
                {
                    i++;
                    try
                    {
                        // 判断是否存在店铺
                        string storename = dr["店铺名称"].ToString();
                        var exist_store = _offlineDB.Off_Store.SingleOrDefault(m => m.StoreName == storename && m.Off_System_Id == user.DefaultSystemId);
                        if (exist_store == null)
                        {
                            messageList.Add(new Excel_DataMessage(i, "店铺不存在", true));
                            result_flag = false;
                            continue;
                        }
                        // 判断是否有促销员，如有促销员，判断存在销售员
                        string sellername = dr["促销员"].ToString();
                        Off_Seller exist_seller = null;
                        if (sellername != "")
                        {
                            exist_seller = _offlineDB.Off_Seller.SingleOrDefault(m => m.Name == sellername);
                            if (exist_seller == null)
                            {
                                messageList.Add(new Excel_DataMessage(i, "销售员不存在", true));
                                result_flag = false;
                                continue;
                            }
                        }
                        // 判断是否含已有数据
                        DateTime info_date = Convert.ToDateTime(dr["日期"]);
                        var exist_dailyinfo = _offlineDB.Off_SalesInfo_Daily.SingleOrDefault(m => m.Date == info_date && m.StoreId == exist_store.Id && m.isMultiple == false);
                        if (exist_dailyinfo != null)
                        {
                            messageList.Add(new Excel_DataMessage(i, "当日数据已存在", true));
                            result_flag = false;
                            continue;
                        }
                        else
                        {
                            int? attendance = null;
                            string attendance_info = dr["考勤"].ToString();
                            switch (attendance_info)
                            {
                                case "全勤":
                                    attendance = 0;
                                    break;
                                case "迟到":
                                    attendance = 1;
                                    break;
                                case "早退":
                                    attendance = 2;
                                    break;
                                case "旷工":
                                    attendance = 3;
                                    break;
                                default:
                                    attendance = null;
                                    break;
                            }
                            Off_SalesInfo_Daily dailyinfo = new Off_SalesInfo_Daily()
                            {
                                StoreId = exist_store.Id,
                                Date = info_date,
                                Item_Brown = ExcelOperation.ConvertInt(dr, "红糖姜茶"),
                                Item_Black = ExcelOperation.ConvertInt(dr, "黑糖姜茶"),
                                Item_Lemon = ExcelOperation.ConvertInt(dr, "柠檬姜茶"),
                                Item_Honey = ExcelOperation.ConvertInt(dr, "蜂蜜姜茶"),
                                Item_Dates = ExcelOperation.ConvertInt(dr, "红枣姜茶"),
                                Off_Seller = exist_seller,
                                Attendance = attendance,
                                Salary = ExcelOperation.ConvertDecimal(dr, "工资"),
                                Bonus = ExcelOperation.ConvertDecimal(dr, "奖金"),
                                Debit = ExcelOperation.ConvertDecimal(dr, "扣款"),
                                isMultiple = ExcelOperation.ConvertBoolean(dr, "多人"),
                                remarks = dr["备注"].ToString(),
                                UploadTime = DateTime.Now,
                                UploadUser = User.Identity.Name
                            };
                            _offlineDB.Off_SalesInfo_Daily.Add(dailyinfo);
                            messageList.Add(new Excel_DataMessage(i, "数据类型验证成功", false));
                        }
                    }
                    catch (Exception e)
                    {
                        result_flag = false;
                        messageList.Add(new Excel_DataMessage(i, "表格格式错误," + e.ToString(), true));
                    }
                }
                if (result_flag)
                {
                    await _offlineDB.SaveChangesAsync();
                    messageList.Add(new Excel_DataMessage(0, "保存成功", false));
                }
                else
                {
                    messageList.Add(new Excel_DataMessage(0, "数据行发生错误，未保存", true));
                }
            }
            catch (Exception e)
            {
                messageList.Add(new Excel_DataMessage(-1, "表格格式错误" + e.ToString(), true));
            }
            return messageList;
        }
        [SettingFilter(SettingName = "GENERAL")]
        public ActionResult UploadResult()
        {
            return View();
        }
    }
}