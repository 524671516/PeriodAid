using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

using PeriodAid.Models;
using PeriodAid.Filters;
using PagedList;

namespace PeriodAid.Controllers
{
    [Authorize(Roles ="Admin")]
    public class OffStatisticController : Controller
    {
        OfflineSales _offlineDB = new OfflineSales();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public OffStatisticController()
        {

        }

        public OffStatisticController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: OffStatistic
        public ActionResult Index()
        {
            return View();
        }

        // Origin: Off_Statistic_StoreSystem
        // 0413 统计数据-渠道数据
        public ActionResult StoreSystemStatistic()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var storesystem = from m in _offlineDB.Off_StoreSystem
                              where m.Off_System_Id == user.DefaultSystemId
                              select m;
            ViewBag.SystemList = new SelectList(storesystem, "Id", "SystemName", storesystem.FirstOrDefault().Id);
            return View();
        }
        // Origin: Off_Statistic_StoreSystem_Ajax
        [HttpPost]
        public JsonResult StoreSystemStatisticAjax(string startdate, string enddate, int storesystemid, string type)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            DateTime st = Convert.ToDateTime(startdate);
            DateTime et = Convert.ToDateTime(enddate);
            if (type == "day")
            {
                string sql = "SELECT T4.SystemName,[Date], count([Date]) as Count, SUM(T3.SalesCount) as SalesCount, SUM(T3.SalesAmount) as SalesAmount, SUM(T3.StorageCount) as StorageCount " +
                    "FROM [Off_SalesInfo_Daily] as T1 left join Off_Store as T2 on T1.StoreId = T2.Id left join Off_Daily_Product as T3 on T1.Id = T3.DailyId left join Off_StoreSystem as T4 on T4.Id=T2.Off_StoreSystemId " +
                    "where Date >= '" + st.ToString("yyyy-MM-dd") + "' and Date < '" + et.ToString("yyyy-MM-dd") + "' and T2.Off_StoreSystemId =" + storesystemid + " " +
                    "and T4.Off_System_Id = " + user.DefaultSystemId + " group by T1.Date, T4.SystemName order by T1.Date";
                var data = _offlineDB.Database.SqlQuery<StoreSystem_Statistic>(sql);
                return Json(new { result = "SUCCESS", data = data });
            }
            else if (type == "month")
            {
                string sql = "SELECT T4.SystemName,CONVERT(datetime, CONVERT(char(7), T1.Date, 120)+'-01') as Date, count(CONVERT(char(7), T1.Date, 120)) as Count, SUM(T3.SalesCount) as SalesCount, SUM(T3.SalesAmount) as SalesAmount, SUM(T3.StorageCount) as StorageCount " +
                    "FROM [Off_SalesInfo_Daily] as T1 left join Off_Store as T2 on T1.StoreId = T2.Id left join Off_Daily_Product as T3 on T1.Id = T3.DailyId left join Off_StoreSystem as T4 on T4.Id=T2.Off_StoreSystemId " +
                    "where Date >= '" + st.ToString("yyyy-MM-01") + "' and Date < '" + et.AddMonths(1).ToString("yyyy-MM-01") + "' and T2.Off_StoreSystemId =" + storesystemid + "" +
                    "and T4.Off_System_Id = " + user.DefaultSystemId + " group by CONVERT(char(7), T1.Date, 120), T4.SystemName order by CONVERT(char(7), T1.Date, 120)";
                var data = _offlineDB.Database.SqlQuery<StoreSystem_Statistic>(sql);
                return Json(new { result = "SUCCESS", data = data });
            }
            return Json(new { result = "FAIL" });
        }
        // Origin: Off_Statistic_StoreSystem_Product_Ajax
        [HttpPost]
        public JsonResult StoreSystemProductStatisticAjax(string startdate, string enddate, int storesystemid)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            DateTime st = Convert.ToDateTime(startdate);
            DateTime et = Convert.ToDateTime(enddate);
            string sql = "SELECT T3.ProductId, T4.SimpleName, sum(T3.SalesCount) as SalesCount, SUM(T3.SalesAmount) as SalesAmount, SUM(T3.StorageCount) as StorageCount " +
                "FROM [Off_SalesInfo_Daily] as T1 left join Off_Store as T2 on T1.StoreId = T2.Id left join Off_Daily_Product as T3 on T1.Id = T3.DailyId left join Off_Product as T4 on T4.Id = T3.ProductId " +
                "where Date >= '" + st.ToString("yyyy-MM-dd") + "' and Date< '" + et.ToString("yyyy-MM-dd") + "' and T2.Off_StoreSystemId =" + storesystemid + " " +
                "and T4.Off_System_Id = " + user.DefaultSystemId + " and ProductId is not NULL group by T3.ProductId,T4.SimpleName order by T4.SimpleName";
            var data = _offlineDB.Database.SqlQuery<StoreSystem_Product_Statistic>(sql);
            return Json(new { result = "SUCCESS", data = data });
        }
        // Origin: Off_Statistic_StoreSystem_Salary_Ajax
        [HttpPost]
        public JsonResult StoreSystemSalaryStatisticAjax(string startdate, string enddate, int storesystemid, string type)
        {
            try
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                DateTime st = Convert.ToDateTime(startdate);
                DateTime et = Convert.ToDateTime(enddate);
                if (type == "month")
                {
                    et = et.AddMonths(1);
                    string sql = "SELECT CONVERT(datetime, CONVERT(char(7), T1.Date, 120)+'-01') as Date, T4.SystemName, SUM(T1.Salary) as Salary, SUM(T1.Debit) as Debit, SUM(T1.Bonus) as Bonus FROM [Off_SalesInfo_Daily] as T1 left join [Off_Store] as T2 on T1.StoreId = T2.Id "+
                        "left join Off_StoreSystem as T4 on T4.Id=T2.Off_StoreSystemId " +
                        "where Date>= '" + st.ToString("yyyy-MM-01") + "' and Date< '" + et.ToString("yyyy-MM-01") + "' and T2.Off_StoreSystemId =" + storesystemid + " and T4.Off_System_Id = " + user.DefaultSystemId + " " +
                        "group by T4.SystemName, CONVERT(char(7), T1.Date, 120) order by CONVERT(char(7), T1.Date, 120)";
                    var data = _offlineDB.Database.SqlQuery<StoreSystem_Salary_Statistic>(sql);
                    return Json(new { result = "SUCCESS", data = data });
                }
                else if (type == "day")
                {

                    string sql = "SELECT T1.Date, T4.SystemName, SUM(T1.Salary) as Salary, SUM(T1.Debit) as Debit, SUM(T1.Bonus) as Bonus FROM [Off_SalesInfo_Daily] as T1 left join [Off_Store] as T2 on T1.StoreId = T2.Id " +
                        "left join Off_StoreSystem as T4 on T4.Id = T2.Off_StoreSystemId "+
                        "where Date>= '" + st.ToString("yyyy-MM-dd") + "' and Date< '" + et.ToString("yyyy-MM-dd") + "' and T2.Off_StoreSystemId =" + storesystemid + " and T4.Off_System_Id = " + user.DefaultSystemId + " " +
                        "group by T4.SystemName, T1.Date";
                    var data = _offlineDB.Database.SqlQuery<StoreSystem_Salary_Statistic>(sql);
                    return Json(new { result = "SUCCESS", data = data }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { result = "FAIL" });
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
        }
        // Origin: Off_Satistic_Store
        // 0413 统计数据-门店数据
        public ActionResult StoreStatistic()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var storesystem = from m in _offlineDB.Off_StoreSystem
                              where m.Off_System_Id == user.DefaultSystemId
                              select m;
            ViewBag.SystemList = new SelectList(storesystem, "Id", "SystemName", storesystem.FirstOrDefault().Id);
            return View();
        }

        // Origin: Off_Statistic_Store_Ajax
        [HttpPost]
        public JsonResult StoreStatisticAjax(string startdate, string enddate, int storeid, string type)
        {
            DateTime st = Convert.ToDateTime(startdate);
            DateTime et = Convert.ToDateTime(enddate);
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (type == "day")
            {
                string sql = "SELECT T2.StoreName,[Date], count([Date]) as Count, SUM(T3.SalesCount) as SalesCount, SUM(T3.SalesAmount) as SalesAmount, SUM(T3.StorageCount) as StorageCount " +
                    "FROM [Off_SalesInfo_Daily] as T1 left join Off_Store as T2 on T1.StoreId = T2.Id left join Off_Daily_Product as T3 on T1.Id = T3.DailyId left join Off_StoreSystem as T4 on T4.Id = T2.Off_StoreSystemId " +
                    "where Date >= '" + st.ToString("yyyy-MM-dd") + "' and Date < '" + et.ToString("yyyy-MM-dd") + "' and T1.StoreId =" + storeid + " " +
                    "and T4.Off_System_Id = " + user.DefaultSystemId + " group by T1.Date, T2.StoreName order by T1.Date";
                var data = _offlineDB.Database.SqlQuery<StoreSystem_Statistic>(sql);
                return Json(new { result = "SUCCESS", data = data });
            }
            else if (type == "month")
            {
                string sql = "SELECT T2.StoreName,CONVERT(datetime, CONVERT(char(7), T1.Date, 120)+'-01') as Date, count(CONVERT(char(7), T1.Date, 120)) as Count, SUM(T3.SalesCount) as SalesCount, SUM(T3.SalesAmount) as SalesAmount, SUM(T3.StorageCount) as StorageCount " +
                    "FROM [Off_SalesInfo_Daily] as T1 left join Off_Store as T2 on T1.StoreId = T2.Id left join Off_Daily_Product as T3 on T1.Id = T3.DailyId left join Off_StoreSystem as T4 on T4.Id = T2.Off_StoreSystemId " +
                    "where Date >= '" + st.ToString("yyyy-MM-01") + "' and Date < '" + et.AddMonths(1).ToString("yyyy-MM-01") + "' and T1.StoreId =" + storeid + " " +
                    "and T4.Off_System_Id = " + user.DefaultSystemId + " group by CONVERT(char(7), T1.Date, 120), T2.StoreName order by CONVERT(char(7), T1.Date, 120)";
                var data = _offlineDB.Database.SqlQuery<StoreSystem_Statistic>(sql);
                return Json(new { result = "SUCCESS", data = data });
            }
            else
                return Json(new { result = "FAIL" });
        }

        // Origin: Off_Statistic_Store_Product_Ajax
        [HttpPost]
        public JsonResult StoreProductStatisticAjax(string startdate, string enddate, int storeid)
        {
            DateTime st = Convert.ToDateTime(startdate);
            DateTime et = Convert.ToDateTime(enddate);
            var user = UserManager.FindById(User.Identity.GetUserId());
            string sql = "SELECT T3.ProductId, T4.SimpleName, sum(T3.SalesCount) as SalesCount, SUM(T3.SalesAmount) as SalesAmount, SUM(T3.StorageCount) as StorageCount " +
                "FROM [Off_SalesInfo_Daily] as T1 left join Off_Store as T2 on T1.StoreId = T2.Id left join Off_Daily_Product as T3 on T1.Id = T3.DailyId left join Off_Product as T4 on T4.Id = T3.ProductId " +
                "where Date >= '" + st.ToString("yyyy-MM-dd") + "' and Date< '" + et.ToString("yyyy-MM-dd") + "' and T1.StoreId like '" + storeid + "'" +
                "and T4.Off_System_Id = " + user.DefaultSystemId + " and ProductId is not NULL group by T3.ProductId,T4.SimpleName order by T4.SimpleName";
            var data = _offlineDB.Database.SqlQuery<StoreSystem_Product_Statistic>(sql);
            return Json(new { result = "SUCCESS", data = data });
        }

        // Origin: Off_Statistic_Store_Salary_Ajax
        [HttpPost]
        public JsonResult StoreSalaryStatisticAjax(string startdate, string enddate, int storeid, string type)
        {
            try
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                DateTime st = Convert.ToDateTime(startdate);
                DateTime et = Convert.ToDateTime(enddate);
                if (type == "month")
                {
                    et = et.AddMonths(1);
                    string sql = "SELECT CONVERT(datetime, CONVERT(char(7), T1.Date, 120)+'-01') as Date, T4.SystemName, SUM(T1.Salary) as Salary, SUM(T1.Debit) as Debit, SUM(T1.Bonus) as Bonus FROM [Off_SalesInfo_Daily] as T1 left join [Off_Store] as T2 on T1.StoreId = T2.Id left join Off_StoreSystem as T4 on T4.Id= T2.Off_StoreSystemId " +
                        "where Date>= '" + st.ToString("yyyy-MM-01") + "' and Date< '" + et.ToString("yyyy-MM-01") + "' and T1.StoreId = " + storeid + " and T4.Off_System_Id = " + user.DefaultSystemId + " " +
                        "group by T4.SystemName, CONVERT(char(7), T1.Date, 120)";
                    var data = _offlineDB.Database.SqlQuery<StoreSystem_Salary_Statistic>(sql);
                    return Json(new { result = "SUCCESS", data = data });
                }
                else if (type == "day")
                {
                    //et = Convert.ToDateTime(enddate);
                    string sql = "SELECT T1.Date, T1.StoreId, SUM(T1.Salary) as Salary, SUM(T1.Debit) as Debit, SUM(T1.Bonus) as Bonus FROM [Off_SalesInfo_Daily] as T1 left join [Off_Store] as T2 on T1.StoreId = T2.Id left join Off_StoreSystem as T4 on T4.Id= T2.Off_StoreSystemId " +
                        "where Date>= '" + st.ToString("yyyy-MM-dd") + "' and Date< '" + et.ToString("yyyy-MM-dd") + "' and T1.StoreId =" + storeid + " and T4.Off_System_Id = " + user.DefaultSystemId + " " +
                        "group by T1.StoreId, T1.Date";
                    var data = _offlineDB.Database.SqlQuery<StoreSystem_Salary_Statistic>(sql);
                    return Json(new { result = "SUCCESS", data = data });
                }
                return Json(new { result = "FAIL" });
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
        }
        // 0413 统计数据-促销员数据
        // Origin: Off_Statistic_Seller
        public ActionResult SellerStatistic()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var storesystem = from m in _offlineDB.Off_StoreSystem
                              where m.Off_System_Id == user.DefaultSystemId
                              select m;
            ViewBag.SystemList = new SelectList(storesystem, "Id", "SystemName", storesystem.FirstOrDefault().Id);
            return View();
        }
        // Origin: Off_Statistic_Seller_Ajax
        [HttpPost]
        public JsonResult SellerStatisticAjax(string startdate, string enddate, int sellerid)
        {
            string st = Convert.ToDateTime(startdate).ToString("yyyy-MM-dd");
            string et = Convert.ToDateTime(enddate).ToString("yyyy-MM-dd");
            string sql = "Select T3.Date, T3.SalesCount, T3.SalesAmount, T3.StorageCount, T4.AVG_SalesData, T4.AVG_AmountData from (select T1.Date,T1.StoreId, SUM(T2.SalesCount) as SalesCount, SUM(T2.SalesAmount) as SalesAmount, SUM(T2.StorageCount) as StorageCount from Off_SalesInfo_Daily as T1 left join " +
                "[Off_Daily_Product] as T2 on T1.Id = T2.DailyId" +
                " where T1.SellerId = " + sellerid + " and Date>= '" + startdate + "' and Date<='" + enddate + "' " +
                "group by T1.Date, T1.StoreId) as T3 left join Off_AVG_Info as T4 on DatePart(DW, T3.Date) = T4.DayOfWeek and T3.StoreId = T4.StoreId order by T3.Date";
            var data = _offlineDB.Database.SqlQuery<Seller_Statistic>(sql);
            return Json(new { result = "SUCCESS", data = data });
        }
        [HttpPost]
        public JsonResult Off_Statistic_QuerySeller_Ajax(string query)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = (from m in _offlineDB.Off_Seller
                        where (m.Name.Contains(query) || m.Off_Store.StoreName.Contains(query)) && m.Off_System_Id == user.DefaultSystemId
                        select new { value = m.Id, label = m.Name, desc = m.Off_Store.StoreName }).Take(5);
            return Json(new { result = "SUCCESS", data = list });
        }
    }
}