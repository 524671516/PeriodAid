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
            var P_Type = from m in _db.SP_ProductType
                         select m;
            ViewBag.ProductType = P_Type;
            return View();
        }

        public ActionResult ProductListPartial(int? page, string query,int productType)
        {
            int _page = page ?? 1;
            if (productType == 0)
            {
                if (query != "")
                {
                    var product = (from m in _db.SP_Product
                                   select m);
                    var SearchResult = (from m in product
                                        where m.Item_Name.Contains(query) || m.Item_Code.Contains(query) || m.System_Code.Contains(query)
                                        || m.SP_ProductType.Type_Name.Contains(query) || m.Brand_Name.Contains(query)
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
            }else
            {
                if (query != "")
                {
                    var product = (from m in _db.SP_Product
                                   where m.Type_Id == productType
                                   select m);
                    var SearchResult = (from m in product
                                        where m.Item_Name.Contains(query) || m.Item_Code.Contains(query) || m.System_Code.Contains(query) 
                                        || m.SP_ProductType.Type_Name.Contains(query) || m.Brand_Name.Contains(query)
                                        orderby m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
                else
                {
                    var SearchResult = (from m in _db.SP_Product
                                        where m.Type_Id == productType
                                        orderby m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
            }
        }

        public ActionResult AddProductPartial()
        {
            List<SelectListItem> productType = new List<SelectListItem>();
            productType.Add(new SelectListItem() { Text = "姜茶", Value = "1" });
            productType.Add(new SelectListItem() { Text = "花草茶", Value = "2" });
            productType.Add(new SelectListItem() { Text = "糕点", Value = "3" });
            productType.Add(new SelectListItem() { Text = "其它", Value = "4" });
            ViewBag.productType = new SelectList(productType, "Value", "Text");
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
                item.Brand_Name = model.Brand_Name;
                item.Item_ShortName = model.Item_ShortName;
                item.Supplier_Name = model.Supplier_Name;
                item.Bar_Code = model.Bar_Code;
                item.Product_Weight = model.Product_Weight;
                item.Carton_Spec = model.Carton_Spec;
                item.Purchase_Price = model.Purchase_Price;
                item.Supply_Price = model.Supply_Price;
                item.Type_Id = model.Type_Id;
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

        public ActionResult ClientList()
        {
            return View();
        }

        public ActionResult ClientListPartial(int? page, string query)
        {
            int _page = page ?? 1;
            
                if (query != "")
                {
                    var customer = (from m in _db.SP_Client
                                    select m);
                    var SearchResult = (from m in customer
                                        where m.Client_Name.Contains(query) || m.SP_Seller.Seller_Name.Contains(query) || m.Client_Area.Contains(query)
                                        orderby m.Client_Name descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
                else
                {
                    var SearchResult = (from m in _db.SP_Client
                                        orderby m.Client_Name descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
            }

        public ActionResult AddClientPartial()
        {
            List<SelectListItem> itemlist = new List<SelectListItem>();
            itemlist.Add(new SelectListItem() { Text = "活跃", Value = "1" });
            itemlist.Add(new SelectListItem() { Text = "待开发", Value = "0" });
            itemlist.Add(new SelectListItem() { Text = "解约", Value = "-1" });
            ViewBag.ClientType = new SelectList(itemlist, "Value", "Text");

            List<SelectListItem> salessystem = new List<SelectListItem>();
            salessystem.Add(new SelectListItem() { Text = "华东", Value = "华东" });
            salessystem.Add(new SelectListItem() { Text = "外区", Value = "外区" });
            salessystem.Add(new SelectListItem() { Text = "华南", Value = "华南" });
            salessystem.Add(new SelectListItem() { Text = "全国", Value = "全国" });
            salessystem.Add(new SelectListItem() { Text = "西南", Value = "西南" });
            salessystem.Add(new SelectListItem() { Text = "华中", Value = "华中" });
            salessystem.Add(new SelectListItem() { Text = "东北", Value = "东北" });
            salessystem.Add(new SelectListItem() { Text = "西北", Value = "西北" });
            salessystem.Add(new SelectListItem() { Text = "华北", Value = "华北" });
            ViewBag.SalesList = new SelectList(salessystem, "Value", "Text");

            List<SelectListItem> sellerlist = new List<SelectListItem>();
            sellerlist.Add(new SelectListItem() { Text = "孙楠楠", Value = "1" });
            sellerlist.Add(new SelectListItem() { Text = "杨丽萌", Value = "2" });
            ViewBag.SellerName = new SelectList(sellerlist, "Value", "Text");
            return PartialView();
        }
        [HttpPost]
        public ActionResult AddClientPartial(SP_Client model, FormCollection form)
        {
            bool Client = _db.SP_Client.Any(m => m.Client_Name == model.Client_Name && m.Client_Area == model.Client_Area);
            if (ModelState.IsValid)
            {
                if (Client)
                {
                    return Content("FALL");
                }
                else
                {
                    var client = new SP_Client();
                    client.Client_Name = model.Client_Name;
                    client.Client_Type = model.Client_Type;
                    client.Seller_Id = model.Seller_Id;
                    client.Client_Area = model.Client_Area;
                    _db.SP_Client.Add(client);
                    _db.SaveChanges();
                    return Content("SUCCESS");
                };

            }
            else
            {
                return PartialView(model);
            }
            //return Content("ERROR1");
        }

        public ActionResult EditClientInfo(int clientId)
        {
            var item = _db.SP_Client.SingleOrDefault(m => m.Id == clientId);
            List<SelectListItem> sellerlist = new List<SelectListItem>();
            sellerlist.Add(new SelectListItem() { Text = "孙楠楠", Value = "1" });
            sellerlist.Add(new SelectListItem() { Text = "杨丽萌", Value = "2" });
            ViewBag.SellerName = new SelectList(sellerlist, "Value", "Text");

            List<SelectListItem> itemlist = new List<SelectListItem>();
            itemlist.Add(new SelectListItem() { Text = "活跃", Value = "1" });
            itemlist.Add(new SelectListItem() { Text = "待开发", Value = "0" });
            itemlist.Add(new SelectListItem() { Text = "解约", Value = "-1" });
            ViewBag.ClientType = new SelectList(itemlist, "Value", "Text");

            List<SelectListItem> salessystem = new List<SelectListItem>();
            salessystem.Add(new SelectListItem() { Text = "华东", Value = "华东" });
            salessystem.Add(new SelectListItem() { Text = "外区", Value = "外区" });
            salessystem.Add(new SelectListItem() { Text = "华南", Value = "华南" });
            salessystem.Add(new SelectListItem() { Text = "全国", Value = "全国" });
            salessystem.Add(new SelectListItem() { Text = "西南", Value = "西南" });
            salessystem.Add(new SelectListItem() { Text = "华中", Value = "华中" });
            salessystem.Add(new SelectListItem() { Text = "东北", Value = "东北" });
            salessystem.Add(new SelectListItem() { Text = "西北", Value = "西北" });
            salessystem.Add(new SelectListItem() { Text = "华北", Value = "华北" });
            ViewBag.SalesList = new SelectList(salessystem, "Value", "Text");
            return PartialView(item);
        }
        [HttpPost]
        public ActionResult EditClientInfo(SP_Client model)
        {
            bool Client = _db.SP_Client.Any(m => m.Client_Name == model.Client_Name && m.Client_Area == model.Client_Area);
            if (ModelState.IsValid)
            {
                if (Client)
                {
                    return Json(new { result = "FAIL" });
                }
                else
                {
                    SP_Client client = new SP_Client();
                    if (TryUpdateModel(client))
                    {
                        _db.Entry(client).State = System.Data.Entity.EntityState.Modified;
                        _db.SaveChanges();
                        return Json(new { result = "SUCCESS" });
                    }
                }
                
            }
            return Json(new { result = "FAIL" });
        }
        [HttpPost]
        public ActionResult DeleteClient(int clientId)
        {
            var Client = _db.SP_Client.AsNoTracking().SingleOrDefault(m => m.Id == clientId);
            SP_Client client = new SP_Client();
            client.Id = Client.Id;
            client.Client_Name = Client.Client_Name;
            client.Seller_Id = Client.Seller_Id;
            client.Client_Type = -1;
            client.Client_Area = Client.Client_Area;
            if (TryUpdateModel(client))
            {
                _db.Entry(client).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            return Json(new { result = "FALL" });

        }

        public ActionResult ContactList(int clientId)
        {
            var client = from m in _db.SP_Client
                         where m.Id ==  clientId
                         select m;
            ViewBag.ClientName = client;
            return View();
        }

        public ActionResult ContactListPartial(int clientId,int? page ,string query)
        {
            int _page = page ?? 1;
            if (query != "")
            {
                var contact = (from m in _db.SP_Contact
                               where m.Client_Id == clientId
                               select m);
                var SearchResult = (from m in contact
                                    where m.Contact_Name.Contains(query) || m.Contact_Mobile.Contains(query)
                                    orderby m.Id descending
                                    select m).ToPagedList(_page, 15);
                return PartialView(SearchResult);
            }
            else
            {
                var SearchResult = (from m in _db.SP_Contact
                                    where m.Client_Id == clientId
                                    orderby m.Id descending
                                    select m).ToPagedList(_page, 15);
                return PartialView(SearchResult);
            }
            
        }

        public ActionResult AddContactPartial(int clientId)
        {
            var client = from m in _db.SP_Client
                         where m.Id == clientId
                         select m;
            ViewBag.ClientName = client;
            return PartialView();
        }
        [HttpPost]
        public ActionResult AddContactPartial(SP_Contact model, FormCollection form)
        {
            ModelState.Remove("Contact_Mobile");
            if (ModelState.IsValid)
            {
                var contact = new SP_Contact();
                contact.Contact_Name = model.Contact_Name;
                contact.Contact_Mobile = model.Contact_Mobile;
                contact.Contact_Address = model.Contact_Address;
                contact.Contact_Type = 0;
                contact.Client_Id = model.Client_Id;
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

        public ActionResult EditContactInfo(int contactId)
        {
            var item = _db.SP_Contact.SingleOrDefault(m => m.Id == contactId);
            var contact = from m in _db.SP_Contact
                         where m.Id == contactId
                         select m;
            ViewBag.ClientName = contact;
            return PartialView(item);
        }
        [HttpPost]
        public ActionResult EditContactInfo(SP_Contact model)
        {
            if (ModelState.IsValid)
            {
                SP_Contact contact = new SP_Contact();
                if (TryUpdateModel(contact))
                {
                    _db.Entry(contact).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
            }
            return Json(new { result = "FAIL" });
        }
        [HttpPost]
        public ActionResult DeleteContact(int contactId)
        {
            var Contact = _db.SP_Contact.AsNoTracking().SingleOrDefault(m => m.Id == contactId);
            SP_Contact contact = new SP_Contact();
            contact.Id = Contact.Id;
            contact.Contact_Name = Contact.Contact_Name;
            contact.Contact_Mobile = Contact.Contact_Mobile;
            contact.Contact_Address = Contact.Contact_Address;
            contact.Client_Id = Contact.Client_Id;
            contact.Contact_Type = -1;
            if (TryUpdateModel(contact))
            {
                _db.Entry(contact).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            return Json(new { result = "FALL" });

        }

        public ActionResult SalesList()
        {
            return View();
        }

        public ActionResult SalesListPartial(int clientId, int? page, string query)
        {
            int _page = page ?? 1;
            if (query != "")
            {
                var sales = (from m in _db.SP_SalesSystem
                               where m.Client_Id == clientId
                               select m);
                var SearchResult = (from m in sales
                                    where m.System_Name.Contains(query) || m.System_Phone.Contains(query)
                                    orderby m.Id descending
                                    select m).ToPagedList(_page, 15);
                return PartialView(SearchResult);
            }
            else
            {
                var SearchResult = (from m in _db.SP_SalesSystem
                                    where m.Client_Id == clientId
                                    orderby m.Id descending
                                    select m).ToPagedList(_page, 15);
                return PartialView(SearchResult);
            }
        }

        public ActionResult AddSalesPartial(int clientId)
        {
            var client = from m in _db.SP_Client
                         where m.Id == clientId
                         select m;
            ViewBag.ClientName = client;
            return PartialView();
        }
        [HttpPost]
        public ActionResult AddSalesPartial(SP_SalesSystem model, FormCollection form)
        {
            bool Sales = _db.SP_SalesSystem.Any(m => m.System_Name == model.System_Name && m.System_Phone == model.System_Phone);
            if (ModelState.IsValid)
            {
                if (Sales)
                {
                    return Content("FALL");
                }
                else
                {
                    var sales = new SP_SalesSystem();
                    sales.System_Name = model.System_Name;
                    sales.System_Phone = model.System_Phone;
                    sales.System_Address = model.System_Address;
                    sales.Client_Id = model.Client_Id;
                    sales.System_Type = 0;
                    _db.SP_SalesSystem.Add(sales);
                    _db.SaveChanges();
                    return Content("SUCCESS");
                }
            }
            else
            {
                return PartialView(model);
            }
            //return Content("ERROR1");
        }

        public ActionResult EditSalesInfo(int salesId)
        {
            var item = _db.SP_SalesSystem.SingleOrDefault(m => m.Id == salesId);
            var sales = from m in _db.SP_SalesSystem
                        where m.Id == salesId
                        select m;
            ViewBag.SalesName = sales;
            return PartialView(item);
        }

        [HttpPost]
        public ActionResult EditSalesInfo(SP_SalesSystem model)
        {
            bool Sales = _db.SP_SalesSystem.Any(m => m.System_Name == model.System_Name && m.System_Phone == model.System_Phone);
            if (ModelState.IsValid)
            {
                if (Sales)
                {
                    return Json(new { result = "FALL" });
                }
                else{
                    SP_SalesSystem sales = new SP_SalesSystem();
                    if (TryUpdateModel(sales))
                    {
                        _db.Entry(sales).State = System.Data.Entity.EntityState.Modified;
                        _db.SaveChanges();
                        return Json(new { result = "SUCCESS" });
                    }
                }
            }
            return Json(new { result = "FAIL" });
        }
        [HttpPost]
        public ActionResult DeleteSales(int salesId)
        {
            var Sales = _db.SP_SalesSystem.AsNoTracking().SingleOrDefault(m => m.Id == salesId);
            SP_SalesSystem sales = new SP_SalesSystem();
            sales.Id = Sales.Id;
            sales.Client_Id = Sales.Client_Id;
            sales.System_Name = Sales.System_Name;
            sales.System_Phone = Sales.System_Phone;
            sales.System_Address = Sales.System_Address;
            sales.System_Type = -1;
            if (TryUpdateModel(sales))
            {
                _db.Entry(sales).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            return Json(new { result = "FALL" });

        }

        //public ActionResult QuotedList(int clientId)
        //{
        //    var quoted = from m in _db.SP_Quoted
        //                 where m.Client_Id == clientId
        //                 select m;
        //    ViewBag.Quoted = quoted;
        //    return View();
        //}

        //public ActionResult QuotedListPartial(string query,int clientId)
        //{
        //    if (query != null)
        //    {
        //        if (query != "")
        //        {
        //            var product = (from m in _db.SP_Quoted
        //                           where m.Client_Id == clientId
        //                           select m);
        //            var SearchResult = from m in product
        //                               where m.SP_Product.Item_Name.Contains(query) || m.SP_Product.Item_Code.Contains(query) || m.SP_Product.System_Code.Contains(query)
        //                               orderby m.Id descending
        //                               select m;
        //            return PartialView(SearchResult);
        //        }
        //        else
        //        {
        //            var SearchResult = from m in _db.SP_Quoted
        //                               where m.Client_Id == clientId
        //                               orderby m.Id descending
        //                               select m;
        //            return PartialView(SearchResult);
        //        }

        //    }
        //    else
        //    {
        //        var productlist = from m in _db.SP_Quoted
        //                          where m.Client_Id == clientId
        //                          orderby m.Id descending
        //                          select m;
        //        return PartialView(productlist);
        //    }
        //}

        //public ActionResult AddQuotedPartial(int clientId)
        //{
        //    var quoted = _db.SP_Quoted.SingleOrDefault(m => m.SP_Client.Id == clientId);
        //    return PartialView(quoted);
        //}
    }
}