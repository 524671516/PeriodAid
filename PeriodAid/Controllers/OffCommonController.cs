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
using System.Reflection;

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
        // Origin: updateSystem
        public ActionResult ChangeSystem(int id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            user.DefaultSystemId = id;
            UserManager.Update(user);
            return RedirectToAction("Index", "OffCommon");
        }
        public ActionResult AuthorizeError()
        {
            return View("AuthorizeError");
        }
        [Authorize(Roles = "Admin")]
        public ActionResult AccountSetting()
        {
            AccountSetting_ViewModel model = new AccountSetting_ViewModel();
            Type t = model.GetType();
            var props = t.GetProperties();
            var user = UserManager.FindById(User.Identity.GetUserId());
            for (int i = 0; i < props.Length; i++)
            {
                string settingName = props[i].Name;
                var settingitem = _offlineDB.Off_System_Setting.SingleOrDefault(m => m.SettingName == settingName && m.Off_System_Id == user.DefaultSystemId);
                if (settingitem != null)
                {
                    props[i].SetValue(model, settingitem.SettingValue);
                }
            }
            OfflineSales offdb = new OfflineSales();
            List<int> systemlistid = new List<int>();
            string[] temp = user.OffSalesSystem.Split(',');
            foreach (var item in temp)
            {
                systemlistid.Add(Convert.ToInt32(item));
            }
            var systemlist = from m in offdb.Off_System
                             where systemlistid.Contains(m.Id)
                             select m;
            ViewBag.SystemList = systemlist;
            return View(model);
        }
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken,HttpPost]
        public ActionResult AccountSettingAjax(FormCollection form)
        {
            if (ModelState.IsValid)
            {
                //form.AllKeys;
                AccountSetting_ViewModel model = new AccountSetting_ViewModel();
                if (TryUpdateModel(model))
                {
                    Type t = model.GetType();
                    var s = t.GetProperties();
                    //var result = "";
                    var user = UserManager.FindById(User.Identity.GetUserId());
                    for (int i = 0; i < s.Length; i++)
                    {
                        string settingName = s[i].Name;
                        var settingitem = _offlineDB.Off_System_Setting.SingleOrDefault(m => m.SettingName == settingName && m.Off_System_Id == user.DefaultSystemId);
                        if (settingitem != null)
                        {
                            settingitem.SettingValue = (string)s[i].GetValue(model);
                            _offlineDB.Entry(settingitem).State = System.Data.Entity.EntityState.Modified;
                        }
                        else
                        {
                            var setting = new Off_System_Setting()
                            {
                                Off_System_Id = user.DefaultSystemId,
                                SettingName = settingName,
                                SettingResult = true,
                                SettingValue = (string)s[i].GetValue(model)
                            };
                            _offlineDB.Off_System_Setting.Add(setting);
                        }
                    }
                    _offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                return Content("FAIL");
            }
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

        // Origin: Off_Add_Schedule_StoreList
        [HttpPost]
        public JsonResult StoreListAjax(string storesystem)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in _offlineDB.Off_Store
                       where m.StoreSystem == storesystem && m.Off_System_Id == user.DefaultSystemId
                       orderby m.StoreName
                       select new { ID = m.Id, StoreName = m.StoreName };
            return Json(new { StoreList = list });
        }
    }
}