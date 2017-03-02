using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PeriodAid.Models;
using Microsoft.AspNet.Identity.Owin;

namespace PeriodAid.Controllers
{
    public class WebContentController : Controller
    {
        /*活动列表*/
        // GET: WebContent
        SQZWEBModels _db = new SQZWEBModels();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public WebContentController()
        {

        }

        public WebContentController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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