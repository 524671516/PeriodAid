﻿using PagedList;
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

        public ActionResult ClientList()
        {
            return View();
        }

        //public ActionResult ClientListPartial(int? page, string query, int plattformType)
        //{
        //    int _page = page ?? 1;
        //    if (plattformId == 0)
        //    {
        //        if (query != "")
        //        {
        //            var customer = (from m in _db.SP_Client
        //                            where m.SP_Plattform.Plattform_Type == plattformType
        //                            select m);
        //            var SearchResult = (from m in customer
        //                                where m.Client_Name.Contains(query) || m.SP_Seller.Seller_Name.Contains(query)
        //                                orderby m.Id descending
        //                                select m).ToPagedList(_page, 15);
        //            return PartialView(SearchResult);
        //        }
        //        else
        //        {
        //            var SearchResult = (from m in _db.SP_Client
        //                                where m.SP_Plattform.Plattform_Type == plattformType
        //                                orderby m.Id descending
        //                                select m).ToPagedList(_page, 15);
        //            return PartialView(SearchResult);
        //        }
        //    }
        //    else
        //    {
        //        if (query != "")
        //        {
        //            var customer = (from m in _db.SP_Client
        //                            where m.SP_Plattform.Plattform_Type == plattformType && m.Plattform_Id == plattformId
        //                            select m);
        //            var SearchResult = (from m in customer
        //                                where m.Client_Name.Contains(query) || m.SP_Seller.Seller_Name.Contains(query)
        //                                orderby m.Id descending
        //                                select m).ToPagedList(_page, 15);
        //            return PartialView(SearchResult);
        //        }
        //        else
        //        {
        //            var SearchResult = (from m in _db.SP_Client
        //                                where m.SP_Plattform.Plattform_Type == plattformType && m.Plattform_Id == plattformId
        //                                orderby m.Id descending
        //                                select m).ToPagedList(_page, 15);
        //            return PartialView(SearchResult);
        //        }
        //    }
        //}

        //public ActionResult AddClientPartial()
        //{
        //    List<SelectListItem> itemlist = new List<SelectListItem>();
        //    itemlist.Add(new SelectListItem() { Text = "活跃", Value = "1" });
        //    itemlist.Add(new SelectListItem() { Text = "待开发", Value = "0" });
        //    itemlist.Add(new SelectListItem() { Text = "解约", Value = "-1" });
        //    ViewBag.ClientType = new SelectList(itemlist, "Value", "Text");

        //    List<SelectListItem> plattformlist = new List<SelectListItem>();
        //    plattformlist.Add(new SelectListItem() { Text = "分销", Value = "1" });
        //    plattformlist.Add(new SelectListItem() { Text = "代销", Value = "2" });
        //    plattformlist.Add(new SelectListItem() { Text = "代发货", Value = "3" });
        //    ViewBag.PlattformList = new SelectList(plattformlist, "Value", "Text");

        //    List<SelectListItem> sellerlist = new List<SelectListItem>();
        //    sellerlist.Add(new SelectListItem() { Text = "孙楠楠", Value = "1" });
        //    sellerlist.Add(new SelectListItem() { Text = "杨丽萌", Value = "2" });
        //    ViewBag.SellerName = new SelectList(sellerlist, "Value", "Text");
        //    return PartialView();
        //}
        //[HttpPost]
        //public ActionResult AddClientPartial(SP_Client model, FormCollection form)
        //{
        //    bool Client = _db.SP_Client.Any(m => m.Client_Name == model.Client_Name);
        //    if (ModelState.IsValid)
        //    {
        //        if (Client)
        //        {
        //            return Content("FALL");
        //        }
        //        else
        //        {
        //            var client = new SP_Client();
        //            client.Client_Name = model.Client_Name;
        //            client.Client_Type = model.Client_Type;
        //            client.Plattform_Id = model.Plattform_Id;
        //            client.Seller_Id = model.Seller_Id;
        //            _db.SP_Client.Add(client);
        //            _db.SaveChanges();
        //            return Content("SUCCESS");
        //        };

        //    }
        //    else
        //    {
        //        return PartialView(model);
        //    }
        //    //return Content("ERROR1");
        //}

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

            List<SelectListItem> plattformlist = new List<SelectListItem>();
            plattformlist.Add(new SelectListItem() { Text = "分销", Value = "1" });
            plattformlist.Add(new SelectListItem() { Text = "代销", Value = "2" });
            plattformlist.Add(new SelectListItem() { Text = "代发货", Value = "3" });
            ViewBag.PlattformList = new SelectList(plattformlist, "Value", "Text");
            return PartialView(item);
        }
        [HttpPost]
        public ActionResult EditClientInfo(SP_Client model)
        {
            if (ModelState.IsValid)
            {
                SP_Client client = new SP_Client();
                if (TryUpdateModel(client))
                {
                    _db.Entry(client).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
            }
            return Json(new { result = "FAIL" });
        }
        //[HttpPost]
        //public ActionResult DeleteClient(int clientId) {
        //    var Client = _db.SP_Client.AsNoTracking().SingleOrDefault(m => m.Id == clientId);
        //    SP_Client client = new SP_Client();
        //    client.Id = Client.Id;
        //    client.Client_Name = Client.Client_Name;
        //    client.Plattform_Id = Client.Plattform_Id;
        //    client.Seller_Id = Client.Seller_Id;
        //    client.Client_Type = -1;
        //    if (TryUpdateModel(client))
        //    {
        //        _db.Entry(client).State = System.Data.Entity.EntityState.Modified;
        //        _db.SaveChanges();
        //        return Json(new { result = "SUCCESS" });
        //    }
        //    return Json(new { result = "FALL" });

        //}

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

        public ActionResult QuotedList(int clientId)
        {
            var quoted = from m in _db.SP_Quoted
                         where m.Client_Id == clientId
                         select m;
            ViewBag.Quoted = quoted;
            return View();
        }

        public ActionResult QuotedListPartial(string query,int clientId)
        {
            if (query != null)
            {
                if (query != "")
                {
                    var product = (from m in _db.SP_Quoted
                                   where m.Client_Id == clientId
                                   select m);
                    var SearchResult = from m in product
                                       where m.SP_Product.Item_Name.Contains(query) || m.SP_Product.Item_Code.Contains(query) || m.SP_Product.System_Code.Contains(query)
                                       orderby m.Id descending
                                       select m;
                    return PartialView(SearchResult);
                }
                else
                {
                    var SearchResult = from m in _db.SP_Quoted
                                       where m.Client_Id == clientId
                                       orderby m.Id descending
                                       select m;
                    return PartialView(SearchResult);
                }

            }
            else
            {
                var productlist = from m in _db.SP_Quoted
                                  where m.Client_Id == clientId
                                  orderby m.Id descending
                                  select m;
                return PartialView(productlist);
            }
        }

        public ActionResult AddQuotedPartial(int clientId)
        {
            var quoted = _db.SP_Quoted.SingleOrDefault(m => m.SP_Client.Id == clientId);
            return PartialView(quoted);
        }
    }
}