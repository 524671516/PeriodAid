using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;

using PeriodAid.Models;
using PeriodAid.Filters;
using PagedList;
using PeriodAid.DAL;

namespace PeriodAid.Controllers
{

    [Authorize(Roles ="Admin")]
    public class BonusController : Controller
    {
        OfflineSales _offlineDB = new OfflineSales();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public BonusController()
        {

        }

        public BonusController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: Bonus
        public ActionResult Index()
        {
            return View();
        }

        // Origin: Off_RedPack_List
        //0614
        [SettingFilter(SettingName = "BONUS")]
        public ActionResult BonusList()
        {
            return View();
        }

        // Origin: Off_RedPack_List_Ajax
        [SettingFilter(SettingName = "BONUS")]
        public PartialViewResult BonusListPartial(string query, int? page)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            int _page = page ?? 1;
            if (query == null || query == "")
            {
                var list = (from m in _offlineDB.Off_BonusRequest
                            where m.Off_Checkin.Off_Checkin_Schedule.Off_System_Id == user.DefaultSystemId && m.Status > 0
                            orderby m.RequestTime descending
                            select m).ToPagedList(_page, 30);
                return PartialView(list);
            }
            else
            {
                var list = (from m in _offlineDB.Off_BonusRequest
                            where m.Off_Checkin.Off_Checkin_Schedule.Off_System_Id == user.DefaultSystemId && m.Status > 0
                            && (m.Off_Checkin.Off_Seller.Name.Contains(query) || m.Off_Checkin.Off_Checkin_Schedule.Off_Store.StoreName.Contains(query))
                            orderby m.RequestTime descending
                            select m).ToPagedList(_page, 30);
                return PartialView(list);
            }
        }

        // Origin: Off_RedPack_Refresh_Status
        [SettingFilter(SettingName = "BONUS")]
        public async Task<JsonResult> RefreshBonusStatus(int id)
        {
            var request = _offlineDB.Off_BonusRequest.SingleOrDefault(m => m.Id == id && m.Status == 1);
            if (request != null)
            {
                AppPayUtilities pay = new AppPayUtilities();
                string result = await pay.WxRedPackQuery(request.Mch_BillNo);
                switch (result)
                {
                    case "SENT":
                        request.Status = 1;
                        break;
                    case "RECEIVED":
                        request.Status = 2;
                        break;
                    case "FAIL":
                        request.Status = 3;
                        break;
                    case "REFUND":
                        request.Status = 4;
                        break;
                    default:
                        request.Status = 1;
                        break;
                }
                _offlineDB.Entry(request).State = System.Data.Entity.EntityState.Modified;
                await _offlineDB.SaveChangesAsync();
                return Json(new { result = "SUCCESS" });
            }
            else
            {
                return Json(new { result = "FAIL" });
            }

        }
    }
}