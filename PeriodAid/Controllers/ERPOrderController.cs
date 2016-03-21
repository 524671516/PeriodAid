using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PeriodAid.Models;
using PeriodAid.DAL;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;

namespace PeriodAid.Controllers
{
    public class ERPOrderController : Controller
    {
        //OfflineSales offlineDB = new OfflineSales();
        ERPOrderDataContext erpdb = new ERPOrderDataContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public ERPOrderController()
        {

        }

        public ERPOrderController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: ERPOrder
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Download_Order_List()
        {
            return View();
        }
        public ActionResult Download_Order_List_Ajax()
        {
            var list = from m in erpdb.taskstatus
                       orderby m.id descending
                       select m;
            return PartialView(list);
        }

        public ActionResult Download_Member_List()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> Download_Order_Start_Ajax(string st, string et)
        {
            ERPOrderUtilities util = new ERPOrderUtilities();
            await util.Download_ErpOrders(st, et);
            return Json(new { result = "SUCCESS" });
        }

        public JsonResult Download_Member_Status_Ajax(int id)
        {
            return Json(new { });
        }
    }
}