using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PeriodAid.Controllers
{
    public class ERPOrderController : Controller
    {
        // GET: ERPOrder
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Download_Order_List()
        {
            return View();
        }

        public ActionResult Download_Member_List()
        {
            return View();
        }

        public JsonResult Download_Order_Status_Ajax(int id)
        {
            return Json(new { });
        }

        public JsonResult Download_Member_Status_Ajax(int id)
        {
            return Json(new { });
        }
    }
}