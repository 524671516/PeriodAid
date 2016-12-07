using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PeriodAid.Models;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace PeriodAid.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OffSellerTaskController : Controller
    {
        OfflineSales offlineDB = new OfflineSales();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public OffSellerTaskController()
        {

        }

        public OffSellerTaskController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        
        public ActionResult Index()
        {
            return View();
        }

        // 暗促信息首页
        public ActionResult TaskSellerHome()
        {
            return View();
        }
        // 按月份查询（每月每个促销员的）
        public ActionResult TaskSellerQueryMonthPartial(string month)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            DateTime _month = Convert.ToDateTime(month);
            DateTime _month_next = _month.AddMonths(1);
            ViewBag.Month = month;
            var list = from m in offlineDB.Off_SellerTask
                       where m.ApplyDate >= _month && m.ApplyDate < _month_next
                       && m.Off_Seller.Off_System_Id == user.DefaultSystemId
                       group m by m.Off_Seller into g
                       select new Wx_SellerTaskMonthStatistic { Off_Seller = g.Key, AttendanceCount = g.Count() };
            return PartialView(list);
        }
        // 查看个人当月明细
        public ActionResult TaskSellerQueryMonthBySellerPartial(string month, int sellerid)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            DateTime _month = Convert.ToDateTime(month);
            DateTime _month_next = _month.AddMonths(1);
            var list = from m in offlineDB.Off_SellerTask
                       where m.ApplyDate >= _month && m.ApplyDate < _month_next
                       && m.SellerId == sellerid
                       select m;
            return PartialView(list);
        }

        // 查看暗促数据详情
        public ActionResult TaskSellerQueryDetails(int id)
        {
            var item = offlineDB.Off_SellerTask.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                return View(item);
            }
            return View("Error");
        }

        // 查看预警信息
        public ActionResult TaskSellerAlert()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            // 使用SQL查询
            string sql = "SELECT t.Id,t.ApplyDate, Min(t3.StorageCount) as MinStorage, T4.StoreName FROM [dbo].[Off_SellerTask] as t left join dbo.Off_SellerTaskProduct as t3 on t.Id= t3.SellerTaskId left join" +
                " dbo.Off_Store as T4 on t.StoreId = T4.Id where t.Id = (select top 1 t2.Id from [dbo].[Off_SellerTask] t2 where t4.Off_System_Id = " + user.DefaultSystemId + " and t2.StoreId = t.StoreId order by T2.ApplyDate desc) and t3.StorageCount>0" +
                " group by t.Id, T4.StoreName, t.ApplyDate having MIN(t3.StorageCount)<50";
            var tasklist = offlineDB.Database.SqlQuery<Wx_SellerTaskAlert>(sql);
            return PartialView(tasklist);
        }

    }
}