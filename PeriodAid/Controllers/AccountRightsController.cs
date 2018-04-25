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
    [Authorize]
    public class AccountRightsController : Controller
    {
        // 权限管理
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public AccountRightsController()
        {
            //ar_db = new AccountRightsModel();
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
        
        public async Task<ActionResult> RightsListPartial(int? page, string query)
        {
            var ApplicationUsers = new List<ApplicationUser>();
            var UserList = await UserManager.Users.ToListAsync();
            foreach (var user in UserList)
            {
                if (user.UserName.Contains("@"))
                {
                    if (UserManager.IsInRole(user.Id, "Staff"))
                    {
                        ApplicationUsers.Add(user);
                    }
                }
            }
            int _page = page ?? 1;
            if (query == null || query.Trim() == "")
            {
                var list = (from m in ApplicationUsers
                            orderby m.Id ascending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
            else
            {
                var list = (from m in ApplicationUsers
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
        //[Authorize(Roles = "Admin")]
        public JsonResult Add_UserInfo(ApplicationUser model, FormCollection form)
        {
            var UserInfo = UserManager.Users.SingleOrDefault(m => m.UserName == model.UserName);
            if (UserInfo != null)
            {
                var add_role = UserManager.AddToRole(UserInfo.Id, "Staff");
                return Json(new { result = "SUCCESS" });
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }
        [HttpPost]
        //[Authorize(Roles = "Admin")]
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
            var AcUser = UserManager.Users.SingleOrDefault(m => m.Id == userId);
            var Roles = UserManager.GetRoles(userId);
            ViewBag.Roles = Roles;
            return PartialView(AcUser);
        }
        [HttpPost]
        //[Authorize(Roles = "Admin")]
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
        //[Authorize(Roles = "Admin")]
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

        public class UserInfoList
        {
            public string Id { get; set; }

            public string Email { get; set; }

            public string UserName { get; set; }

            public string PhoneNumber { get; set; }

            public string NickName { get; set; }
        }

    }
}