using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PeriodAid.Models;
using PeriodAid.DAL;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using PagedList;

namespace PeriodAid.Controllers
{
    [Authorize(Roles = "Admin")]
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
                       where m.type == 0
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
            var taglist = from m in erpdb.tags
                          select m;
            return View(taglist);
        }

        // 0330 会员标签上传
        public ActionResult Vip_Tag_Upload()
        {
            var taglist = from m in erpdb.tags
                          select new { Key = m.id, Value = m.name };
            ViewBag.SelectList = new SelectList(taglist, "Key", "Value");
            return View();
        }

        public async Task<ActionResult> teststores()
        {
            ERPOrderUtilities util = new ERPOrderUtilities();
            var stores = await util.getERPShops();
            var list = from m in stores
                       select new { Key = m.name, Value = m.name };
            ViewBag.SelectList = new SelectList(list, "Key", "Value");
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> setTagByOrderId(string orderids, int tagid)
        {
            ERPOrderUtilities util = new ERPOrderUtilities();
            var vipidlist = util.getVipIdsByOrderId(orderids);
            //return Content(vipidlist.Count() + "");
            int success = await util.setTags(vipidlist, tagid);
            return Content("成功：" + success);
        }

        [HttpPost]
        public async Task<ActionResult> setTagByVipName(string vipnames, int tagid)
        {
            ERPOrderUtilities util = new ERPOrderUtilities();
            var vipidlist = util.getVipIdsByVipName(vipnames);
            //return Content(vipidlist.Count() + "");
            int success = await util.setTags(vipidlist, tagid);
            return Content("成功：" + success);
        }

        public async Task<ActionResult> GetItems()
        {
            ERPOrderUtilities util = new ERPOrderUtilities();
            await util.Download_ERPItems();
            return Json(new { result = "SUCCESS" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GenericData_List()
        {
            return View();
        }

        public PartialViewResult GenericData_List_Ajax(string storename, int? page)
        {
            int _page = page ?? 1;
            if (storename == null)
            {
                var list = (from m in erpdb.generic_data
                            orderby m.date descending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
            else
            {
                var list = (from m in erpdb.generic_data
                           where m.storename == storename
                           orderby m.date descending
                           select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
        }
        public ActionResult Create_GenericData()
        {
            generic_data data = new generic_data();
            return View(data);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create_GenericData(generic_data model)
        {
            if(ModelState.IsValid)
            {
                generic_data item = new generic_data();
                if (TryUpdateModel(item))
                {
                    erpdb.generic_data.Add(item);
                    erpdb.SaveChanges();
                    return RedirectToAction("GenericData_List");
                }
                else
                {
                    return View("Error");
                }
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return View(model);
            }
        }
        public ActionResult Edit_GenericData(int id)
        {
            try
            {
                var item = erpdb.generic_data.SingleOrDefault(m => m.id == id);
                if (item != null)
                {
                    return View(item);
                }
                else
                    return View("Error");
            }
            catch
            {
                return View("Error");
            }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit_GenericData(generic_data model)
        {
            if (ModelState.IsValid)
            {
                generic_data item = new generic_data();
                if (TryUpdateModel(item))
                {
                    erpdb.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    erpdb.SaveChanges();
                    return RedirectToAction("GenericData_List");
                }
                else
                {
                    return View("Error");
                }
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return View(model);
            }
        }
        [HttpPost]
        public ActionResult Delete_GenericData(int id)
        {
            try
            {
                var item = erpdb.generic_data.SingleOrDefault(m => m.id == id);
                if (item != null)
                {
                    erpdb.generic_data.Remove(item);
                    erpdb.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                else
                    return Json(new { result = "FAIL" });
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
        }

        public ActionResult GenericData_Statistic()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GenericData_Satistic_Ajax(string startdate, string enddate, string storename)
        {
            if (startdate == null || enddate == null)
            {
                var list = from m in erpdb.generic_data
                           where m.storename == storename
                           orderby m.date
                           select m;
                return Json(new { result = "SUCCESS", data = list });
            }
            else
            {
                try
                {
                    DateTime sd = Convert.ToDateTime(startdate);
                    DateTime ed = Convert.ToDateTime(enddate);
                    var list = from m in erpdb.generic_data
                               where m.storename == storename && m.date >= sd && m.date <= ed
                               orderby m.date
                               select m;
                    return Json(new { result = "SUCCESS", data = list });
                }
                catch
                {
                    return Json(new { result = "FAIL" });
                }

            }
        }
    }
}