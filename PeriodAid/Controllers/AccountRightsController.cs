using CsvHelper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using PagedList;
using PeriodAid.DAL;
using PeriodAid.Filters;
using PeriodAid.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml;

namespace PeriodAid.Controllers
{
    [Authorize(Roles = "Staff")]
    public class AccountRightsController : Controller
    {
        // 权限管理
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private AccountRightsModel ar_db;
        public AccountRightsController()
        {
            ar_db = new AccountRightsModel();
        }

        public AccountRightsController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        
        public ActionResult RightsList()
        {
            return View();
        }
        
        public ActionResult RightsListPartial(int? page, string query)
        {
            int _page = page ?? 1;
            var memdb = new ApplicationDbContext();
            var roles = memdb.Roles.SingleOrDefault(m => m.Name == "Staff");
            var usersInfo = from m in roles.Users
                            select m.UserId;
            var users = from m in memdb.Users
                        where usersInfo.Contains(m.Id)
                        select m;
            if (query == null || query.Trim() == "")
            {
                var list = (from m in users
                            orderby m.Id ascending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
            else
            {
                var list = (from m in users
                            where m.UserName.Contains(query) || m.NickName.Contains(query)
                            orderby m.Id ascending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
        }
        
        public ActionResult Add_UserInfo()
        {
            return PartialView();
        }
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public JsonResult Add_UserInfo(ApplicationUser model, FormCollection form)
        {
            var UserInfo = UserManager.Users.SingleOrDefault(m => m.UserName == model.UserName);
            if (UserInfo != null)
            {
                if (UserManager.IsInRole(UserInfo.Id, "Staff"))
                {
                    return Json(new { result = "ERROR" });
                }
                else
                {
                    var add_role = UserManager.AddToRole(UserInfo.Id, "Staff");
                    return Json(new { result = "SUCCESS" });
                }
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public JsonResult Remove_UserInfo(string userId)
        {
            var remove_roles = UserManager.RemoveFromRole(userId, "Staff");
            return Json(new { result = "SUCCESS" });
        }
        //显示权限
        public ActionResult AccountRights_View(string userId)
        {
            var AcUser = UserManager.Users.SingleOrDefault(m => m.Id == userId);
            ViewBag.Nickname = AcUser;
            return View();
        }

        public PartialViewResult AccountRights_PartialView(string userId)
        {
            var rolelist = from m in ar_db.AR_Roles
                           select m;
            var AcUser = UserManager.Users.SingleOrDefault(m => m.Id == userId);
            var Roles = UserManager.GetRoles(userId);
            var sameList = rolelist.Where(m => Roles.Contains(m.Key)).ToArray();
            ViewBag.Roles = sameList;
            ViewBag.UserRoles = rolelist;
            return PartialView(AcUser);
        }
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public JsonResult Add_RoleAjax(string userId, string Role)
        {
            if (Role != "")
            {
                try
                {
                    var add_role = UserManager.AddToRole(userId, Role);
                    return Json(new { result = "SUCCESS" });
                }
                catch
                {
                    return Json(new { result = "ERROR" });
                }
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public JsonResult Remove_RoleAjax(string userId, string Role)
        {
            if (Role != "")
            {
                try
                {
                    var remove_role = UserManager.RemoveFromRole(userId, Role);
                    return Json(new { result = "SUCCESS" });
                }
                catch
                {
                    return Json(new { result = "ERROR" });
                }
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }
        
    }
}