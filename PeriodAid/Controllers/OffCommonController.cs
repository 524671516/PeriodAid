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
using PeriodAid.DAL;

namespace PeriodAid.Controllers
{
    public class OffCommonController : Controller
    {
        OfflineSales _offlineDB = new OfflineSales();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public OffCommonController()
        {

        }

        public OffCommonController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: Common
        public ActionResult Index()
        {
            return View();
        }
        
        // Origin: ajax_StoreSystem
        [HttpPost]
        public JsonResult StoreSystemListAjax()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var storesystem = from m in _offlineDB.Off_Store
                              where m.Off_System_Id == user.DefaultSystemId
                              group m by m.StoreSystem into g
                              select g.Key;
            return Json(new { storesystem = storesystem });
        }
    }
}