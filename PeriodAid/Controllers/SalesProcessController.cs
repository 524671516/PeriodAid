using PagedList;
using PeriodAid.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PeriodAid.Controllers
{
    [Authorize]
    public class SalesProcessController : Controller
    {
        // GET: SalesProcess
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private SalesProcessModel _db;
        public SalesProcessController()
        {
            _db = new SalesProcessModel();
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ProductList()
        {
            return View();
        }

        public ActionResult ProductListPartial(int? page, string query)
        {
            int _page = page ?? 1;
            if (query != null)
            {
                if (query != "")
                {
                    var product = (from m in _db.SP_Product
                                   select m);
                    var SearchResult = (from m in product
                                        where m.Item_Name.Contains(query) || m.Item_Code.Contains(query) || m.System_Code.Contains(query)
                                        orderby m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
                else
                {
                    var SearchResult = (from m in _db.SP_Product
                                        orderby m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }

            }
            else
            {
                var productlist = (from m in _db.SP_Product
                                   orderby m.Id descending
                                   select m).ToPagedList(_page, 15);
                return PartialView(productlist);
            }
        }

        public ActionResult AddProductPartial()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult AddProductPartial(SP_Product model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                var item = new SP_Product();
                item.System_Code = model.System_Code;
                item.Item_Code = model.Item_Code;
                item.Item_Name = model.Item_Name;
                item.Carton_Spec = model.Carton_Spec;
                item.Purchase_Price = model.Purchase_Price;
                _db.SP_Product.Add(item);
                _db.SaveChanges();
                return Content("SUCCESS");

            }
            else
            {
                return PartialView(model);
            }
            //return Content("ERROR1");
        }

        public ActionResult EditProductInfo(int productId)
        {
            var item = _db.SP_Product.SingleOrDefault(m => m.Id == productId);
            return PartialView(item);
        }

        [HttpPost]
        public ActionResult EditProductInfo(SP_Product model)
        {
            if (ModelState.IsValid)
            {
                SP_Product item = new SP_Product();
                if (TryUpdateModel(item))
                {
                    _db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
            }
            return Json(new { result = "FAIL" });
        }

        [HttpPost]
        public ActionResult DeleteProduct(int productId)
        {
            var item = _db.SP_Product.SingleOrDefault(m => m.Id == productId);
            if (item != null)
            {
                try
                {
                    _db.SP_Product.Remove(item);
                    _db.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                catch
                {
                    return Json(new { result = "UNAUTHORIZED" });
                }
            }
            return Json(new { result = "FAIL" });
        }

        public ActionResult ClientList(int plattformType)
        {
            var trafficName = from m in _db.SP_Plattform
                              where m.Plattform_Type == plattformType
                              select m;
            ViewBag.trafficName = trafficName;
            return View();
        }

        public ActionResult ClientListPartial(int plattformId, int? page, string query, int plattformType)
        {
            int _page = page ?? 1;
            if (plattformId == 0)
            {
                if (query != "")
                {
                    var customer = (from m in _db.SP_Contact
                                    where m.SP_Client.SP_Plattform.Plattform_Type == plattformType
                                    select m);
                    var SearchResult = (from m in customer
                                        where m.SP_Client.Client_Name.Contains(query) || m.Contact_Name.Contains(query) || m.SP_Client.SP_Seller.Seller_Name.Contains(query)
                                        orderby m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
                else
                {
                    var SearchResult = (from m in _db.SP_Contact
                                        where m.SP_Client.SP_Plattform.Plattform_Type == plattformType
                                        orderby m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
            }
            else
            {
                if (query != "")
                {
                    var customer = (from m in _db.SP_Contact
                                    where m.SP_Client.SP_Plattform.Plattform_Type == plattformType && m.SP_Client.Plattform_Id == plattformId
                                    select m);
                    var SearchResult = (from m in customer
                                        where m.SP_Client.Client_Name.Contains(query) || m.Contact_Name.Contains(query)|| m.SP_Client.SP_Seller.Seller_Name.Contains(query)
                                        orderby m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
                else
                {
                    var SearchResult = (from m in _db.SP_Contact
                                        where m.SP_Client.SP_Plattform.Plattform_Type == plattformType && m.SP_Client.Plattform_Id == plattformId
                                        orderby m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
            }
        }

        public ActionResult AddClientPartial()
        {
            List<SelectListItem> itemlist = new List<SelectListItem>();
            itemlist.Add(new SelectListItem() { Text = "活跃", Value = "1" });
            itemlist.Add(new SelectListItem() { Text = "正常", Value = "0" });
            itemlist.Add(new SelectListItem() { Text = "待开发", Value = "-1" });
            ViewBag.ClientType = new SelectList(itemlist, "Value", "Text");

            List<SelectListItem> plattformlist = new List<SelectListItem>();
            plattformlist.Add(new SelectListItem() { Text = "分销", Value = "1" });
            plattformlist.Add(new SelectListItem() { Text = "代销", Value = "2" });
            plattformlist.Add(new SelectListItem() { Text = "代发货", Value = "3" });
            ViewBag.PlattformList = new SelectList(plattformlist, "Value", "Text");

            List<SelectListItem> sellerlist = new List<SelectListItem>();
            sellerlist.Add(new SelectListItem() { Text = "孙楠楠", Value = "1" });
            sellerlist.Add(new SelectListItem() { Text = "杨丽萌", Value = "2" });
            ViewBag.SellerName = new SelectList(sellerlist, "Value", "Text");
            return PartialView();
        }

        [HttpPost]
        public ActionResult AddClientPartial(SP_Contact model, FormCollection form)
        {
            ModelState.Remove("Contact_Name");
            ModelState.Remove("Contact_Mobile");
            ModelState.Remove("Contact_Address");
            if (ModelState.IsValid)
            {
                var client = new SP_Client();
                client.Client_Name = model.SP_Client.Client_Name;
                client.Client_Type = model.SP_Client.Client_Type;
                client.Plattform_Id = model.SP_Client.Plattform_Id;
                client.Seller_Id = model.SP_Client.Seller_Id;
                _db.SP_Client.Add(client);
                _db.SaveChanges();
                var clientId = _db.SP_Client.SingleOrDefault( m => m.Client_Name == client.Client_Name);
                var contact = new SP_Contact();
                contact.Client_Id = clientId.Id;
                contact.Contact_Name = model.Contact_Name;
                contact.Contact_Mobile = model.Contact_Mobile;
                contact.Contact_Address = model.Contact_Address;
                _db.SP_Contact.Add(contact);
                _db.SaveChanges();
                return Content("SUCCESS");

            }
            else
            {
                return PartialView(model);
            }
            //return Content("ERROR1");
        }
    }
}