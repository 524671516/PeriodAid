using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

using PeriodAid.Models;
using PeriodAid.Filters;
using PagedList;

namespace PeriodAid.Controllers
{
    [Authorize(Roles ="Admin")]
    public class OffProductController : Controller
    {
        OfflineSales _offlineDB = new OfflineSales();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public OffProductController()
        {

        }

        public OffProductController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: OffProduct
        public ActionResult Index()
        {
            return View();
        }
        // 产品 查增改删
        // 0509 产品列表
        // Origin: Off_Product_List
        public ActionResult ProductList()
        {
            return View();
        }
        // Origin: Off_Product_List_Ajax
        public ActionResult ProductListPartial(int? page)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            int _page = page ?? 1;
            var list = (from m in _offlineDB.Off_Product
                        where m.Off_System_Id == user.DefaultSystemId && m.status >= 0
                        orderby m.ItemCode
                        select m).ToPagedList(_page, 20);
            return PartialView(list);
        }
        // 0509 添加产品
        // Origin: Off_Product_Create
        public ActionResult CreateProduct()
        {
            Off_Product model = new Off_Product();
            return PartialView(model);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CreateProduct(Off_Product model)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                Off_Product item = new Off_Product();
                if (TryUpdateModel(item))
                {
                    item.Off_System_Id = user.DefaultSystemId;
                    _offlineDB.Off_Product.Add(item);
                    _offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return PartialView("Error");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return PartialView(model);
            }
        }

        // Origin: Off_Product_Edit
        // 0509 修改产品
        public ActionResult EditProduct(int id)
        {
            Off_Product product = _offlineDB.Off_Product.SingleOrDefault(m => m.Id == id);
            if (product != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (product.Off_System_Id == user.DefaultSystemId)
                {
                    return PartialView(product);
                }
                else
                    return PartialView("AuthorizeErrorPartial");
            }                
            else
                return PartialView("ErrorPartial");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EditProduct(Off_Product model)
        {
            if (ModelState.IsValid)
            {
                Off_Product item = new Off_Product();
                if (TryUpdateModel(item))
                {
                    _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    _offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return PartialView("Error");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return PartialView(model);
            }
        }


        // Origin: Off_Product_Delete_Ajax
        // 0509 删除产品
        [HttpPost]
        public ActionResult CancelProductAjax(int id)
        {
            Off_Product product = _offlineDB.Off_Product.SingleOrDefault(m => m.Id == id);
            if (product != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (product.Off_System_Id == user.DefaultSystemId)
                {
                    product.status = -1;
                    _offlineDB.Entry(product).State = System.Data.Entity.EntityState.Modified;
                    _offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                else
                    return Json(new { result = "UNAUTHORIZED" });
            }
            return Json(new { result = "FAIL" });
        }

        // 模板 查增改删
        // 0511 产品列表
        // Origin: Off_Template_List
        public ActionResult TemplateList()
        {
            return View();
        }
        // Origin:Off_Template_List_Ajax
        public ActionResult TemplateListPartial(int? page)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            int _page = page ?? 1;
            var list = (from m in _offlineDB.Off_Sales_Template
                        where m.Off_System_Id == user.DefaultSystemId && m.Status >= 0
                        orderby m.TemplateName
                        select m).ToPagedList(_page, 20);
            return PartialView(list);
        }
        // 0511 添加模板
        // Origin: Off_Template_Creat
        public ActionResult CreateTemplate()
        {
            Off_Sales_Template model = new Off_Sales_Template();
            return PartialView(model);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CreateTemplate(Off_Sales_Template model)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                Off_Sales_Template item = new Off_Sales_Template();
                if (TryUpdateModel(item))
                {
                    item.Off_System_Id = user.DefaultSystemId;
                    _offlineDB.Off_Sales_Template.Add(item);
                    _offlineDB.SaveChanges();
                    return RedirectToAction("Off_Template_List");
                }
                return PartialView("Error");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return PartialView(model);
            }
        }
        // 0511 获取产品列表
        // Origin: Off_Template_ProductList_Ajax
        [HttpPost]
        public ActionResult TemplateProductListAjax()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var productlist = from m in _offlineDB.Off_Product
                              where m.Off_System_Id == user.DefaultSystemId
                              select new { Id = m.Id, ItemCode = m.ItemCode, ItemName = m.ItemName, SimpleName = m.SimpleName, SalesPrice = m.SalesPrice, Spec = m.Spec, Status = m.status };
            return Json(new { result = "SUCCESS", list = productlist });
        }
        // 0511 修改模板
        // Origin: Off_Template_Edit
        public ActionResult EditTemplate(int id)
        {
            Off_Sales_Template template = _offlineDB.Off_Sales_Template.SingleOrDefault(m => m.Id == id);
            if (template != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (template.Off_System_Id == user.DefaultSystemId)
                {
                    return View(template);
                }
                else
                    return View("AuthorizeError");
            } 
            else
                return View("Error");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EditTemplate(Off_Sales_Template model)
        {
            if (ModelState.IsValid)
            {
                Off_Sales_Template item = new Off_Sales_Template();
                if (TryUpdateModel(item))
                {
                    _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    _offlineDB.SaveChanges();
                    return RedirectToAction("TemplateList");
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return View(model);
            }
        }

        // Off_Template_Delete_Ajax
        // 0511 删除模板
        [HttpPost]
        public ActionResult CancelTemplateAjax(int id)
        {
            Off_Sales_Template model = _offlineDB.Off_Sales_Template.SingleOrDefault(m => m.Id == id);
            if (model != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (user.DefaultSystemId == model.Off_System_Id)
                {
                    model.Status = -1;
                    _offlineDB.Entry(model).State = System.Data.Entity.EntityState.Modified;
                    _offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                else
                    return Json(new { result = "UNAUTHORIZED" });
            }
            return Json(new { result = "FAIL" });
        }
    }
}