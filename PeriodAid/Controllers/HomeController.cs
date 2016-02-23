using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PeriodAid.DAL;
using System.IO;

namespace PeriodAid.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult generateQRCode()
        {
            return View();
        }
        [HttpPost]
        public ActionResult generateQRCode(FormCollection form)
        {
            string url = form["url"];
            MemoryStream ms = CommonUtilities.generate_QR_Code(url);
            return File(ms.ToArray(), @"image/png");
        }
    }
}