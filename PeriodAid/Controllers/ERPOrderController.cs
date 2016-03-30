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
                       where m.type==0
                       orderby m.id descending
                       select m;
            return PartialView(list);
        }

        public ActionResult Download_Member_List()
        {
            return View();
        }

        public ActionResult Download_Member_List_Ajax()
        {
            var list = from m in erpdb.taskstatus
                       where m.type == 1
                       orderby m.id descending
                       select m;
            return PartialView(list);
        }

        [HttpPost]
        public async Task<JsonResult> Download_Order_Start_Ajax(string st, string et)
        {
            ERPOrderUtilities util = new ERPOrderUtilities();
            await util.Download_ErpOrders(st, et);
            return Json(new { result = "SUCCESS" });
        }

        [HttpPost]
        public async Task<JsonResult> Download_Vips_Start_Ajax(string st, string et)
        {
            ERPOrderUtilities util = new ERPOrderUtilities();
            await util.Download_ERPVips(st, et);
            return Json(new { result = "SUCCESS" });
        }

        // 0330 会员标签设定
        public ActionResult Vip_Tag_Setup()
        {
            return View();
        }

        // 0330 会员标签创建
        public ActionResult Vip_Tag_Create()
        {
            return View();
        }

        // 0330 会员标签上传
        public ActionResult Vip_Tag_Upload()
        {
            return View();
        }
    }
}