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
        private ProjectSchemeModels ar_db;
        //private IdentityUser ac_db;
        public AccountRightsController()
        {
            ar_db = new ProjectSchemeModels();
            //ac_db = new IdentityUser();
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
        public ActionResult WebContentIndex()
        {
            return View();
        }

        public ActionResult RightsList()
        {
            return View();
        }

        public PartialViewResult RightsListPartial(int? page, string query)
        {
            int _page = page ?? 1;
            //var user = UserManager.FindById(User.Identity.GetUserId());
            if (query == null || query.Trim() == "")
            {
                var list = (from m in ar_db.Employee
                            where m.Status >= 0
                            orderby m.Id ascending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
            else
            {
                var list = (from m in ar_db.Employee
                            where m.NickName.Contains(query)
                            orderby m.Id ascending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
        }

        // 显示权限
        public ActionResult AccountRights_View(int userId)
        {
            var employee = ar_db.Employee.SingleOrDefault(m => m.Id == userId);
            ViewBag.Nickname = employee;
            var user = UserManager.FindByName(employee.UserName);
            var Roles = UserManager.GetRoles(user.Id);
            ViewBag.Roles = Roles;
            return View();
        }

        public ActionResult AccountRights_PartialView(int userId)
        {
            var employee = ar_db.Employee.SingleOrDefault(m => m.Id == userId);
            var user = UserManager.FindByName(employee.UserName);
            var Roles = UserManager.GetRoles(user.Id);
            ViewBag.Roles = Roles;
            return PartialView();
        }
        [HttpPost]
        public JsonResult Add_RoleAjax(int userId,string Role)
        {
            var seller = User.Identity.Name;
            var sellerType = ar_db.Employee.SingleOrDefault(m => m.UserName == seller);
            if(sellerType.Type == 2)
            {
                var employee = ar_db.Employee.SingleOrDefault(m => m.Id == userId);
                var user = UserManager.FindByName(employee.UserName);
                if (Role != "")
                {
                    try
                    {
                        var add_role = UserManager.AddToRole(user.Id, Role);
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
            }else
            {
                return Json(new { result = "Permission_denied" });
            }
            
        }
        [HttpPost]
        public JsonResult Remove_RoleAjax(int userId, string Role)
        {
            var seller = User.Identity.Name;
            var sellerType = ar_db.Employee.SingleOrDefault(m => m.UserName == seller);
            if (sellerType.Type == 2)
            {
                var employee = ar_db.Employee.SingleOrDefault(m => m.Id == userId);
                var user = UserManager.FindByName(employee.UserName);
                if (Role != "")
                {
                    try
                    {
                        var remove_role = UserManager.RemoveFromRole(user.Id, Role);
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
            else
            {
                return Json(new { result = "Permission_denied" });
            }
        }
    }
}