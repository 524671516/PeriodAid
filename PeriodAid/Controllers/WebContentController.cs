using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PeriodAid.Controllers
{
    public class WebContentController : Controller
    {
        /*活动列表*/
        // GET: WebContent
        public ActionResult WebContentIndex()
        {
            return View();
        }
        //GET:WebEventList_Ajax
        public ActionResult WebEventsList_Ajax()
        {
            return PartialView();
        }
        //GET:CreateWebEventPartial
        public ActionResult CreateWebEventPartial()
        {
            return PartialView();
        }
        //[HttpPost,ValidateAntiForgeryToken]
        //public ActionResult CreateWebEventPartial()
        //{

        //}
    }
}