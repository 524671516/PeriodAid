using iTextSharp.text;
using iTextSharp.text.pdf;
using PagedList;
using PeriodAid.Filters;
using PeriodAid.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
        public SP_Seller getSeller(string username)
        {
            var seller = _db.SP_Seller.SingleOrDefault(m => m.User_Name == username);
            return seller;
        }

        public ActionResult ProductList()
        {
            var P_Type = from m in _db.SP_ProductType
                         select m;
            ViewBag.ProductType = P_Type;
            return View();
        }

        public ActionResult ProductListPartial(int? page, string query, int productType)
        {
            int _page = page ?? 1;
            if (productType == 0)
            {
                if (query != "")
                {
                    var product = (from m in _db.SP_Product
                                   where m.Product_Status != -1
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
                                        where m.Product_Status != -1
                                        orderby m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
            }
            else
            {
                if (query != "")
                {
                    var product = (from m in _db.SP_Product
                                   where m.ProductType_Id == productType
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
                                        where m.ProductType_Id == productType
                                        orderby m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
            }
        }
        public void AddProductViewBag()
        {
            List<SelectListItem> productType = new List<SelectListItem>();
            productType.Add(new SelectListItem() { Text = "姜茶", Value = "1" });
            productType.Add(new SelectListItem() { Text = "花草茶", Value = "2" });
            productType.Add(new SelectListItem() { Text = "糕点", Value = "3" });
            productType.Add(new SelectListItem() { Text = "其它", Value = "4" });
            ViewBag.productType = new SelectList(productType, "Value", "Text");

            List<SelectListItem> productStatus = new List<SelectListItem>();
            productStatus.Add(new SelectListItem() { Text = "爆款", Value = "1" });
            productStatus.Add(new SelectListItem() { Text = "在售", Value = "0" });
            productStatus.Add(new SelectListItem() { Text = "下架", Value = "-1" });
            ViewBag.productStatus = new SelectList(productStatus, "Value", "Text");
        }
        public ActionResult AddProductPartial()
        {
            AddProductViewBag();
            return PartialView();
        }
        [HttpPost]
        public ActionResult AddProductPartial(SP_Product model, FormCollection form)
        {
            bool Product = _db.SP_Product.Any(m => m.System_Code == model.System_Code);
            ModelState.Remove("Product_Status");
            if (ModelState.IsValid)
            {
                if (Product)
                {
                    return Json(new { result = "FALL" });
                }
                else
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
                    item.ProductType_Id = model.ProductType_Id;
                    item.Product_Status = model.Product_Status;
                    _db.SP_Product.Add(item);
                    _db.SaveChanges();
                    return Json(new { result = "SUCCESS" });

                }
            }
            else
            {
                AddProductViewBag();
                return Json(new { result = "ERROR" });
            }
            //return Content("ERROR1");
        }

        public ActionResult EditProductInfo(int productId)
        {
            var item = _db.SP_Product.SingleOrDefault(m => m.Id == productId);
            var productType = (from m in _db.SP_Product
                               where m.Id == productId
                               select m).FirstOrDefault();
            ViewBag.productType = productType;
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
            var Product = _db.SP_Product.AsNoTracking().SingleOrDefault(m => m.Id == productId);
            Product.Product_Status = -1;
            _db.Entry(Product).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            return Json(new { result = "SUCCESS" });
        }

        public ActionResult ClientList()
        {
            return View();
        }

        public ActionResult ClientListPartial(int? page, string query)
        {
            int _page = page ?? 1;
            var seller = getSeller(User.Identity.Name);
            if (seller.Seller_Type == 0)
            {
                if (query != "")
                {
                    var customer = (from m in _db.SP_Client
                                    where m.Client_Status != -1 && m.Seller_Id == seller.Id
                                    select m);
                    var SearchResult = (from m in customer
                                        where m.Client_Name.Contains(query) || m.Client_Area.Contains(query)
                                        orderby m.Client_Name descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
                else
                {
                    var SearchResult = (from m in _db.SP_Client
                                        where m.Client_Status != -1 && m.Seller_Id == seller.Id
                                        orderby m.Client_Name descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
            } else
            {
                if (query != "")
                {
                    var customer = (from m in _db.SP_Client
                                    where m.Client_Status != -1 && m.SP_Seller.Seller_Type <= seller.Seller_Type
                                    select m);
                    var SearchResult = (from m in customer
                                        where m.Client_Name.Contains(query) || m.Client_Area.Contains(query) || m.SP_Seller.Seller_Name.Contains(query)
                                        orderby m.Client_Name descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
                else
                {
                    var SearchResult = (from m in _db.SP_Client
                                        where m.Client_Status != -1 && m.SP_Seller.Seller_Type <= seller.Seller_Type
                                        orderby m.Client_Name descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
            }

        }
        public void AddClientViewBag(){
            var seller = getSeller(User.Identity.Name);
            ViewBag.Seller = seller;
            List<SelectListItem> itemlist = new List<SelectListItem>();
            itemlist.Add(new SelectListItem() { Text = "活跃", Value = "1" });
            itemlist.Add(new SelectListItem() { Text = "待开发", Value = "0" });
            itemlist.Add(new SelectListItem() { Text = "解约", Value = "-1" });
            ViewBag.ClientStatus = new SelectList(itemlist, "Value", "Text");

            List<SelectListItem> typelist = new List<SelectListItem>();
            typelist.Add(new SelectListItem() { Text = "未知", Value = "0" });
            typelist.Add(new SelectListItem() { Text = "大客户", Value = "1" });
            typelist.Add(new SelectListItem() { Text = "经销商", Value = "2" });
            ViewBag.ClientType = new SelectList(typelist, "Value", "Text");

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
        }
        public ActionResult AddClientPartial()
        {
            AddClientViewBag();
            return PartialView();
        }
        [HttpPost]
        [Seller(OperationGroup = 201)]
        public ActionResult AddClientPartial(SP_Client model, FormCollection form)
        {
            bool Client = _db.SP_Client.Any(m => m.Client_Name == model.Client_Name && m.Client_Area == model.Client_Area);
            if (ModelState.IsValid)
            {
                if (Client)
                {
                    return Json(new { result = "UNAUTHORIZED" });
                }
                else
                {
                    var client = new SP_Client();
                    client.Client_Name = model.Client_Name;
                    client.Client_Type = model.Client_Type;
                    client.Client_Area = model.Client_Area;
                    client.Client_Status = model.Client_Status;
                    client.Seller_Id = model.Seller_Id;
                    _db.SP_Client.Add(client);
                    _db.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                };
            }
            else
            {
                AddClientViewBag();
                return Json(new { result = "ERROR" });
            }
        }

        public ActionResult EditClientInfo(int clientId)
        {
            var seller = getSeller(User.Identity.Name);
            ViewBag.Seller = seller;
            var item = _db.SP_Client.SingleOrDefault(m => m.Id == clientId);
            List<SelectListItem> itemlist = new List<SelectListItem>();
            itemlist.Add(new SelectListItem() { Text = "活跃", Value = "1" });
            itemlist.Add(new SelectListItem() { Text = "待开发", Value = "0" });
            itemlist.Add(new SelectListItem() { Text = "解约", Value = "-1" });
            ViewBag.ClientStatus = new SelectList(itemlist, "Value", "Text");

            List<SelectListItem> typelist = new List<SelectListItem>();
            typelist.Add(new SelectListItem() { Text = "未知", Value = "0" });
            typelist.Add(new SelectListItem() { Text = "大客户", Value = "1" });
            typelist.Add(new SelectListItem() { Text = "经销商", Value = "2" });
            ViewBag.ClientType = new SelectList(typelist, "Value", "Text");

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
        [Seller(OperationGroup = 203)]
        public ActionResult EditClientInfo(SP_Client model)
        {
            bool Client = _db.SP_Client.Any(m => m.Client_Name == model.Client_Name && m.Client_Area == model.Client_Area && m.Client_Type == model.Client_Type && m.Client_Status == model.Client_Status && m.Seller_Id == model.Seller_Id);
            if (ModelState.IsValid)
            {
                if (Client)
                {
                    return Json(new { result = "UNAUTHORIZED" });
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
        [Seller(OperationGroup = 203)]
        public ActionResult DeleteClient(int clientId)
        {
            var Client = _db.SP_Client.AsNoTracking().SingleOrDefault(m => m.Id == clientId);
            Client.Client_Status = -1;
            _db.Entry(Client).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            return Json(new { result = "SUCCESS" });
        }

        public ActionResult ContactList(int clientId)
        {
            var client = (from m in _db.SP_Client
                          where m.Id == clientId
                          select m).FirstOrDefault();
            ViewBag.ClientName = client;
            return View();
        }

        public ActionResult ContactListPartial(int clientId, int? page, string query)
        {
            int _page = page ?? 1;
            if (query != "")
            {
                var contact = (from m in _db.SP_Contact
                               where m.Client_Id == clientId && m.Contact_Status != -1
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
                                    where m.Client_Id == clientId && m.Contact_Status != -1
                                    orderby m.Id descending
                                    select m).ToPagedList(_page, 15);
                return PartialView(SearchResult);
            }

        }

        public ActionResult AddContactPartial(int clientId)
        {
            var client = (from m in _db.SP_Client
                          where m.Id == clientId
                          select m).FirstOrDefault();
            ViewBag.ClientName = client;
            return PartialView();
        }
        [HttpPost]
        [Seller(OperationGroup = 301)]
        public ActionResult AddContactPartial(SP_Contact model, FormCollection form)
        {
            bool Contact = _db.SP_Contact.Any(m => m.Contact_Name == model.Contact_Name && m.Contact_Mobile == model.Contact_Mobile && m.Contact_Status == model.Contact_Status);
            if (ModelState.IsValid)
            {
                if (model.Contact_Name == null || model.Contact_Mobile == null || model.Contact_Address == null)
                {
                    return Json(new { result = "FAIL" });
                }
                else
                {
                    if (Contact)
                    {
                        return Json(new { result = "UNAUTHORIZED" });
                    }
                    else
                    {
                        var contact = new SP_Contact();
                        contact.Contact_Name = model.Contact_Name;
                        contact.Contact_Mobile = model.Contact_Mobile;
                        contact.Contact_Address = model.Contact_Address;
                        contact.Contact_Status = 0;
                        contact.Client_Id = model.Client_Id;
                        _db.SP_Contact.Add(contact);
                        _db.Configuration.ValidateOnSaveEnabled = false;
                        _db.SaveChanges();
                        return Json(new { result = "SUCCESS" });
                    }
                }
                
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
            var contact = (from m in _db.SP_Contact
                           where m.Id == contactId
                           select m).FirstOrDefault();
            ViewBag.ClientName = contact;
            return PartialView(item);
        }
        [HttpPost]
        [Seller(OperationGroup = 303)]
        public ActionResult EditContactInfo(SP_Contact model)
        {
            bool Contact = _db.SP_Contact.Any(m => m.Contact_Name == model.Contact_Name && m.Contact_Mobile == model.Contact_Mobile && m.Contact_Address == model.Contact_Address);
            if (ModelState.IsValid)
            {
                if (Contact)
                {
                    return Json(new { result = "UNAUTHORIZED" });
                }
                else
                {
                    SP_Contact contact = new SP_Contact();
                    if (TryUpdateModel(contact))
                    {
                        _db.Entry(contact).State = System.Data.Entity.EntityState.Modified;
                        _db.SaveChanges();
                        return Json(new { result = "SUCCESS" });
                    }
                }
            }
            return Json(new { result = "FAIL" });
        }
        [HttpPost]
        [Seller(OperationGroup = 303)]
        public ActionResult DeleteContact(int contactId)
        {
            var Contact = _db.SP_Contact.AsNoTracking().SingleOrDefault(m => m.Id == contactId);
            Contact.Contact_Status = -1;
            _db.Entry(Contact).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            return Json(new { result = "SUCCESS" });
        }

        public ActionResult SalesList()
        {
            return View();
        }

        public ActionResult SalesListPartial(int? page, string query)
        {
            int _page = page ?? 1;
            var seller = getSeller(User.Identity.Name);
            if(seller.Seller_Type == 0)
            {
                if (query != "")
                {
                    var SearchResult = (from m in _db.SP_SalesSystem
                                        where m.System_Status != -1 && m.SP_Client.Seller_Id == seller.Id 
                                        && m.System_Name.Contains(query) || m.System_Phone.Contains(query)
                                        orderby m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
                else
                {
                    var SearchResult = (from m in _db.SP_SalesSystem
                                        where m.System_Status != -1 && m.SP_Client.Seller_Id == seller.Id
                                        orderby m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
            }
            else
            {
                if (query != "")
                {
                    var SearchResult = (from m in _db.SP_SalesSystem
                                        where m.System_Status != -1 && m.SP_Client.SP_Seller.Seller_Type <= seller.Seller_Type
                                        && m.System_Name.Contains(query) || m.System_Phone.Contains(query)
                                        orderby m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
                else
                {
                    var SearchResult = (from m in _db.SP_SalesSystem
                                        where m.System_Status != -1 && m.SP_Client.SP_Seller.Seller_Type <= seller.Seller_Type
                                        orderby m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
            }
        }
        [HttpPost]
        public JsonResult QueryClient(string query)
        {
            var seller = getSeller(User.Identity.Name);
            if(seller.Seller_Type == 0)
            {
                var client = from m in _db.SP_Client
                             where m.Client_Status != -1 && m.Seller_Id == seller.Id
                             && m.Client_Name.Contains(query)
                             select new { Id = m.Id, Client_Name = m.Client_Name };
                return Json(client);
            }
            else
            {
                var client = from m in _db.SP_Client
                             where m.Client_Status != -1 && m.SP_Seller.Seller_Type <= seller.Seller_Type
                             && m.Client_Name.Contains(query)
                             select new { Id = m.Id, Client_Name = m.Client_Name };
                return Json(client);
            }
        }

        public ActionResult AddSalesPartial()
        {
            List<SelectListItem> saleslist = new List<SelectListItem>();
            var salesname = from m in _db.SP_Client
                            select m;
            foreach(var name in salesname)
            {
                saleslist.Add(new SelectListItem() { Text = name.Client_Name, Value = name.Id.ToString() });
            }
            ViewBag.Sales = new SelectList(saleslist, "Value", "Text");
            return PartialView();
        }
        [HttpPost]
        [Seller(OperationGroup = 401)]
        public ActionResult AddSalesPartial(SP_SalesSystem model, FormCollection form)
        {
            bool Sales = _db.SP_SalesSystem.Any(m => m.System_Name == model.System_Name && m.System_Phone == model.System_Phone);
            if (ModelState.IsValid)
            {
                if (Sales)
                {
                    return Json(new { result = "UNAUTHORIZED" });
                }
                else
                {
                    var sales = new SP_SalesSystem();
                    sales.System_Name = model.System_Name;
                    sales.System_Phone = model.System_Phone;
                    sales.System_Address = model.System_Address;
                    sales.Client_Id = model.Client_Id;
                    sales.System_Status = 0;
                    _db.SP_SalesSystem.Add(sales);
                    _db.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }

        public ActionResult EditSalesInfo(int salesId)
        {
            var item = _db.SP_SalesSystem.SingleOrDefault(m => m.Id == salesId);
            var sales = (from m in _db.SP_SalesSystem
                         where m.Id == salesId
                         select m).FirstOrDefault();
            ViewBag.SalesName = sales;
            return PartialView(item);
        }
        [HttpPost]
        [Seller(OperationGroup = 403)]
        public ActionResult EditSalesInfo(SP_SalesSystem model)
        {
            bool Sales = _db.SP_SalesSystem.Any(m => m.System_Name == model.System_Name && m.System_Phone == model.System_Phone && m.System_Address == model.System_Address && m.System_Status == model.System_Status);
            if (ModelState.IsValid)
            {
                if (Sales)
                {
                    return Json(new { result = "UNAUTHORIZED" });
                }
                else
                {
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
        [Seller(OperationGroup = 403)]
        public ActionResult DeleteSales(int Id)
        {
            var Sales = _db.SP_SalesSystem.AsNoTracking().SingleOrDefault(m => m.Id == Id);
            Sales.System_Status = -1;
            _db.Entry(Sales).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            return Json(new { result = "SUCCESS" });
        }
        
        //public ActionResult QuotedList(int SalesSystemId)
        //{
        //    var sales = (from m in _db.SP_SalesSystem
        //                  where m.Id == SalesSystemId
        //                  select m).FirstOrDefault();
        //    ViewBag.Sales = sales;
        //    return View();
        //}

        //public ActionResult QuotedListPartial(int? page, string query, int SalesSystemId)
        //{
        //    int _page = page ?? 1;
        //    if (query != null)
        //    {
        //        if (query != "")
        //        {
        //            var product = from m in _db.SP_Quoted
        //                          where m.SalesSystem_Id == SalesSystemId && m.Quoted_Status != -1
        //                          select m;
        //            var SearchResult = (from m in product
        //                                where m.Quotation_Num.Contains(query)
        //                                orderby m.Quoted_Date descending
        //                                select m).ToPagedList(_page, 15);
        //            return PartialView(SearchResult);
        //        }
        //        else
        //        {
        //            var SearchResult = (from m in _db.SP_Quoted
        //                                where m.SalesSystem_Id == SalesSystemId && m.Quoted_Status != -1
        //                                orderby m.Quoted_Date descending
        //                                select m).ToPagedList(_page, 15);
        //            return PartialView(SearchResult);
        //        }

        //    }
        //    else
        //    {
        //        var productlist = (from m in _db.SP_Quoted
        //                           where m.SalesSystem_Id == SalesSystemId
        //                           orderby m.Quoted_Date descending
        //                           select m).ToPagedList(_page, 15);
        //        return PartialView(productlist);
        //    }
        //}

        //public ActionResult AddQuotedPartial(int SalesSystemId)
        //{
        //    var salessystem = (from m in _db.SP_SalesSystem
        //                       where m.Id == SalesSystemId
        //                       select m).FirstOrDefault();
        //    ViewBag.Sales = salessystem;
        //    return PartialView();
        //}
        //[HttpPost]
        //[Seller(OperationGroup = 501)]
        //public ActionResult AddQuotedPartial(SP_Quoted model, FormCollection form)
        //{
        //    bool Quoted = _db.SP_Quoted.Any(m =>m.Quotation_Num == model.Quotation_Num);
        //    ModelState.Remove("Quoted_Date");
        //    if (ModelState.IsValid)
        //    {
        //        if(model.Quotation_Num == null || model.Remark == null || model.Quoted_Date.DayOfYear == 1)
        //        {
        //            return Json(new { result = "FAIL" });
        //        }
        //        else
        //        {
        //            if (Quoted)
        //            {
        //                return Json(new { result = "UNAUTHORIZED" });
        //            }
        //            else
        //            {
        //                var quoted = new SP_Quoted();
        //                quoted.Quoted_Date = model.Quoted_Date;
        //                quoted.Quotation_Num = model.Quotation_Num;
        //                quoted.Remark = model.Remark;
        //                quoted.SalesSystem_Id = model.SalesSystem_Id;
        //                _db.SP_Quoted.Add(quoted);
        //                _db.SaveChanges();
        //                return Json(new { result = "SUCCESS" });
        //            }
        //        }
        //    }
        //    else
        //    {
        //        return PartialView(model);
        //    }
        //}
        
        //public ActionResult EditQuotedInfo(int quotedId)
        //{
        //    var Quoted = _db.SP_Quoted.SingleOrDefault(m => m.Id == quotedId);
        //    var quoted = (from m in _db.SP_Quoted
        //                  where m.Id == quotedId
        //                  select m).FirstOrDefault();
        //    ViewBag.Quoted = quoted;
        //    return PartialView(Quoted);
        //}
        //[HttpPost]
        //[Seller(OperationGroup = 503)]
        //public ActionResult EditQuotedInfo(SP_Quoted model)
        //{
        //    bool Quoted = _db.SP_Quoted.Any(m => m.Quotation_Num == model.Quotation_Num && m.Quoted_Date == model.Quoted_Date && m.Remark == model.Remark);
        //    if (ModelState.IsValid)
        //    {
        //        if (Quoted)
        //        {
        //            return Json(new { result = "UNAUTHORIZED" });
        //        }
        //        else
        //        {
        //            SP_Quoted quoted = new SP_Quoted();
        //            if (TryUpdateModel(quoted))
        //            {
        //                _db.Entry(quoted).State = System.Data.Entity.EntityState.Modified;
        //                _db.SaveChanges();
        //                return Json(new { result = "SUCCESS" });
        //            }
        //        }
        //    }
        //    return Json(new { result = "FAIL" });
        //}
        //[HttpPost]
        //[Seller(OperationGroup = 503)]
        //public ActionResult DeleteQuoted(int quotedId)
        //{
        //    var Quoted = _db.SP_Quoted.AsNoTracking().SingleOrDefault(m => m.Id == quotedId);
        //    Quoted.Quoted_Status = -1;
        //    _db.Entry(Quoted).State = System.Data.Entity.EntityState.Modified;
        //    _db.SaveChanges();
        //    return Json(new { result = "SUCCESS" });
        //}
        [HttpPost]
        public JsonResult QueryProduct(string query)
        {
            var product = from m in _db.SP_Product
                          where m.Product_Status != -1
                          && m.Item_Name.Contains(query)
                          select new { Id = m.Id, ProductName = m.Item_Name };
            return Json(product);
        }
        [HttpPost]
        public JsonResult QuerySeller(string query)
        {
            var seller = from m in _db.SP_Seller
                          where m.Seller_Status != -1
                          && m.Seller_Name.Contains(query)
                          select new { Id = m.Id, SellerName = m.Seller_Name };
            return Json(seller);
        }

        public ActionResult QuotePricrList()
        {
            return View();
        }

        public ActionResult QuotePricrListPartial(int? page, string query,int SalesSystemId)
        {
            int _page = page ?? 1;
            if (query != "")
            {
                var price = from m in _db.SP_QuotePrice
                            where m.SalesSystem_Id == SalesSystemId && m.Quoted_Status != -1
                            select m;
                var SearchResult = from m in price
                                   where m.SP_Product.Item_Name.Contains(query) || m.SP_Product.System_Code.Contains(query) 
                                   || m.SP_Product.Item_Code.Contains(query) || m.SP_Product.Brand_Name.Contains(query)
                                   orderby m.Product_Id descending
                                   select m;
                return PartialView(SearchResult);
            }
            else
            {
                var SearchResult = from m in _db.SP_QuotePrice
                                   where m.SalesSystem_Id == SalesSystemId && m.Quoted_Status != -1
                                   orderby m.Product_Id descending
                                   select m;
                return PartialView(SearchResult);
            }
        }

        public ActionResult AddQuotePricrPartial(int SalesSystemId)
        {
            var sales = (from m in _db.SP_SalesSystem
                         where m.Id == SalesSystemId
                         select m).FirstOrDefault();
            ViewBag.Sales = sales;
            return PartialView();
        }
        [HttpPost]
        [Seller(OperationGroup = 801)]
        public ActionResult AddQuotePricrPartial(SP_QuotePrice model, FormCollection form)
        {
            bool QuotePrice = _db.SP_QuotePrice.Any(m => m.Product_Id == model.Product_Id);
            ModelState.Remove("Quoted_Date");
            if (ModelState.IsValid)
            {
                if (QuotePrice)
                {
                    return Json(new { result = "UNAUTHORIZED" });
                }
                else
                {
                    var quotePrice = new SP_QuotePrice();
                    quotePrice.Product_Id = model.Product_Id;
                    quotePrice.Quote_Price = model.Quote_Price;
                    quotePrice.Quoted_Status = 0;
                    quotePrice.Quoted_Date = model.Quoted_Date;
                    quotePrice.SalesSystem_Id = model.SalesSystem_Id;
                    _db.SP_QuotePrice.Add(quotePrice);
                    _db.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
            }
            else
            {
                return PartialView(model);
            }
        }
        
        public ActionResult EditQuotePriceInfo(int quotepriceId)
        {
            var item = _db.SP_QuotePrice.SingleOrDefault(m => m.Id == quotepriceId);
            var quotePrice = (from m in _db.SP_QuotePrice
                              where m.Id == quotepriceId
                              select m).FirstOrDefault();
            ViewBag.QuotePrice = quotePrice;
            return PartialView(item);
        }
        [HttpPost]
        [Seller(OperationGroup = 803)]
        public ActionResult EditQuotePriceInfo(SP_QuotePrice model)
        {
            bool QuotePrice = _db.SP_QuotePrice.Any(m => m.Quote_Price == model.Quote_Price);
            if (ModelState.IsValid)
            {
                if (QuotePrice)
                {
                    return Json(new { result = "UNAUTHORIZED" });
                }
                else
                {
                    SP_QuotePrice quoteprice = new SP_QuotePrice();
                    if (TryUpdateModel(quoteprice))
                    {
                        _db.Entry(quoteprice).State = System.Data.Entity.EntityState.Modified;
                        _db.SaveChanges();
                        return Json(new { result = "SUCCESS" });
                    }
                }
            }
            return Json(new { result = "FAIL" });
        }
        [HttpPost]
        [Seller(OperationGroup = 803)]
        public ActionResult DeleteQuotePrice(int quotePriceId)
        {
            var item = _db.SP_QuotePrice.SingleOrDefault(m => m.Id == quotePriceId);
            item.Quoted_Status = -1;
            _db.Entry(item).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            return Json(new { result = "SUCCESS" });

        }

        public ActionResult SellerList()
        {
            return View();
        }

        public ActionResult SellerListPartial(int? page,string query)
        {
            int _page = page ?? 1;
            if (query != "")
            {
                var sellerList = (from m in _db.SP_Seller
                                  where m.Seller_Status != -1 && m.Seller_Name.Contains(query) || m.Seller_Mobile.Contains(query)
                                  orderby m.Id
                                  select m).ToPagedList(_page, 15);
                return PartialView(sellerList);
            }
            else
            {
                var SearchResult = (from m in _db.SP_Seller
                                    where m.Seller_Status != -1
                                    orderby m.Id ascending
                                    select m).ToPagedList(_page, 15);
                return PartialView(SearchResult);
            }
        }
        
        public ActionResult AddSellerPartial()
        {
            List<SelectListItem> sellerType = new List<SelectListItem>();
            sellerType.Add(new SelectListItem() { Text = "业务员", Value = SellerType.SELLER.ToString() });
            sellerType.Add(new SelectListItem() { Text = "产品部", Value = SellerType.PRODUCTDEPARTMENT.ToString() });
            sellerType.Add(new SelectListItem() { Text = "财务部", Value = SellerType.FINANCIALDEPARTMENT.ToString() });
            sellerType.Add(new SelectListItem() { Text = "业务主管", Value = SellerType.SELLERADMIN.ToString() });
            sellerType.Add(new SelectListItem() { Text = "管理员", Value = SellerType.ADMINISTARTOR.ToString() });
            ViewBag.SellerType = new SelectList(sellerType, "Value", "Text");
            return PartialView();
        }
        [HttpPost]
        [Seller(OperationGroup = 701)]
        public ActionResult AddSellerPartial(SP_Seller model, FormCollection form)
        {
            bool Seller = _db.SP_Seller.Any(m => m.Seller_Name == model.Seller_Name && m.Seller_Mobile == model.Seller_Mobile);
            if (ModelState.IsValid)
            {
                if(model.Seller_Name == null || model.Seller_Mobile == null || model.User_Name == null)
                {
                    return Json(new { result = "FAIL" });
                }
                else
                {
                    if (Seller)
                    {
                        return Json(new { result = "UNAUTHORIZED" });
                    }
                    else
                    {
                        var seller = new SP_Seller();
                        seller.Seller_Name = model.Seller_Name;
                        seller.Seller_Mobile = model.Seller_Mobile;
                        seller.Seller_Type = model.Seller_Type;
                        seller.User_Name = model.User_Name;
                        seller.Seller_Status = 0;
                        _db.SP_Seller.Add(seller);
                        //_db.Configuration.ValidateOnSaveEnabled = false;
                        _db.SaveChanges();
                        return Json(new { result = "SUCCESS" });
                    }
                }
                
            }
            else
            {
                ModelState.AddModelError("", "错误");
                List<SelectListItem> sellerType = new List<SelectListItem>();
                sellerType.Add(new SelectListItem() { Text = "业务员", Value = SellerType.SELLER.ToString() });
                sellerType.Add(new SelectListItem() { Text = "产品部", Value = SellerType.PRODUCTDEPARTMENT.ToString() });
                sellerType.Add(new SelectListItem() { Text = "财务部", Value = SellerType.FINANCIALDEPARTMENT.ToString() });
                sellerType.Add(new SelectListItem() { Text = "业务主管", Value = SellerType.SELLERADMIN.ToString() });
                sellerType.Add(new SelectListItem() { Text = "管理员", Value = SellerType.ADMINISTARTOR.ToString() });
                ViewBag.SellerType = new SelectList(sellerType, "Value", "Text");
                return PartialView(model);
            }
        }

        public ActionResult EditSellerInfo(int sellerId)
        {
            var Seller = _db.SP_Seller.SingleOrDefault(m => m.Id == sellerId);
            List<SelectListItem> sellerlist = new List<SelectListItem>();
            sellerlist.Add(new SelectListItem() { Text = "业务员", Value = SellerType.SELLER.ToString() });
            sellerlist.Add(new SelectListItem() { Text = "产品部", Value = SellerType.PRODUCTDEPARTMENT.ToString() });
            sellerlist.Add(new SelectListItem() { Text = "财务部", Value = SellerType.FINANCIALDEPARTMENT.ToString() });
            sellerlist.Add(new SelectListItem() { Text = "业务主管", Value = SellerType.SELLERADMIN.ToString() });
            sellerlist.Add(new SelectListItem() { Text = "管理员", Value = SellerType.ADMINISTARTOR.ToString() });
            ViewBag.Seller = new SelectList(sellerlist, "Value", "Text");
            return PartialView(Seller);
        }
        [HttpPost]
        [Seller(OperationGroup = 703)]
        public ActionResult EditSellerInfo(SP_Seller model)
        {
            bool Seller = _db.SP_Seller.Any(m => m.Seller_Name == model.Seller_Name && m.Seller_Mobile == model.Seller_Mobile && m.Seller_Type == model.Seller_Type);
            ModelState.Remove("User_Name");
            if (ModelState.IsValid)
            {
                if (Seller)
                {
                    return Json(new { result = "UNAUTHORIZED" });
                }
                else
                {
                    SP_Seller seller = new SP_Seller();
                    if (TryUpdateModel(seller))
                    {
                        _db.Entry(seller).State = System.Data.Entity.EntityState.Modified;
                        _db.SaveChanges();
                        return Json(new { result = "SUCCESS" });
                    }
                }
            }
            return Json(new { result = "FAIL" });
        }
        [HttpPost]
        [Seller(OperationGroup = 703)]
        public ActionResult DeleteSeller(int sellerId)
        {
            var Seller = _db.SP_Seller.AsNoTracking().SingleOrDefault(m => m.Id == sellerId);
            Seller.Seller_Status = -1;
            _db.Entry(Seller).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            return Json(new { result = "SUCCESS" });

        }

        public ActionResult OrderList()
        {
            return View();
        }

        public ActionResult OrderListPartial(int? page, string query)
        {
            int _page = page ?? 1;
            if (query != "")
            {
                var orderList =  (from m in _db.SP_Order
                                  where m.Order_Number.Contains(query) || m.Quotation_Num.Contains(query) && m.Order_Status != -1
                                  orderby m.Id
                                  select m).ToPagedList(_page, 15);
                return PartialView(orderList);
            }
            else
            {
                var SearchResult = (from m in _db.SP_Order
                                    where m.Order_Status != -1
                                    orderby m.Id ascending
                                    select m).ToPagedList(_page, 15);
                return PartialView(SearchResult);
            }
        }
        //[HttpPost]
        //public JsonResult QueryQuoted(string query)
        //{
        //    var quoted = from m in _db.SP_Quoted
        //                 where m.Quoted_Status != -1
        //                 && m.Quotation_Num.Contains(query)
        //                 select new { Id = m.Quotation_Num, Quotation_Num = m.Quotation_Num };
        //    return Json(quoted);
        //}

        public ActionResult AddOrderPricrPartial()
        {
            return PartialView();
        }
        [HttpPost]
        public ActionResult AddOrderPricrPartial(SP_Order model, FormCollection form)
        {
            bool Order = _db.SP_Order.Any( m => m.Order_Number == model.Order_Number && m.Quotation_Num == model.Quotation_Num);
            ModelState.Remove("Order_Date");
            if (ModelState.IsValid)
            {
                if (model.Order_Number == null || model.Quotation_Num == null)
                {
                    return Json(new { result = "FAIL" });
                }
                else
                {
                    if (Order)
                    {
                        return Json(new { result = "UNAUTHORIZED" });
                    }
                    else
                    {
                        var order = new SP_Order();
                        order.Order_Number = model.Order_Number;
                        order.Order_Status = 0;
                        order.Quotation_Num = model.Quotation_Num;
                        order.Order_Date = model.Order_Date;
                        _db.SP_Order.Add(order);
                        _db.Configuration.ValidateOnSaveEnabled = false;
                        _db.SaveChanges();
                        return Json(new { result = "SUCCESS" });
                    }
                }
                
            }
            else
            {
                return PartialView(model);
            }
        }

        public ActionResult EditOrderInfo(int orderId)
        {
            var Order = _db.SP_Order.SingleOrDefault(m => m.Id == orderId);
            var quotation_num = (from m in _db.SP_Order
                                 where m.Id == orderId
                                 select m).FirstOrDefault();
            ViewBag.Num = quotation_num;
            return PartialView(Order);
        }
        [HttpPost]
        public ActionResult EditOrderInfo(SP_Order model)
        {
            bool Order = _db.SP_Order.Any(m => m.Order_Date == model.Order_Date && m.Order_Number == model.Order_Number && m.Quotation_Num == model.Quotation_Num);
            ModelState.Remove("Order_Date");
            ModelState.Remove("Order_Status");
            if (ModelState.IsValid)
            {
                if (Order)
                {
                    return Json(new { result = "UNAUTHORIZED" });
                }
                else
                {
                    SP_Order order = new SP_Order();
                    if (TryUpdateModel(order))
                    {
                        _db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                        _db.SaveChanges();
                        return Json(new { result = "SUCCESS" });
                    }
                }
            }
            return Json(new { result = "FAIL" });
        }
        [HttpPost]
        public ActionResult DeleteOrder(int orderId)
        {
            var Order = _db.SP_Order.AsNoTracking().SingleOrDefault(m => m.Id == orderId);
            Order.Order_Status = -1;
            _db.Entry(Order).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            return Json(new { result = "SUCCESS" });

        }

        //生成PDF
        public ActionResult creatPdf()
        {
            var product = from m in _db.SP_Product
                                 where m.Product_Status != 1
                                 select m;
            var count = product.Count() + 1;
            Document document = new Document(PageSize.A3);
            try
            {
                // 创建文档
                PdfWriter.GetInstance(document, new FileStream(@"d:\Create.pdf", FileMode.Create));
                BaseFont setFont = BaseFont.CreateFont(@"C:\Windows\Fonts\simfang.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                Font font = new Font(setFont, 16);
                Font font1 = new Font(setFont, 12);
                Font font2 = new Font(setFont, 10);
                Paragraph title = new Paragraph("寿全斋[-订单发货-]审批单", font);
                title.Alignment = 1;
                // 打开文档
                document.Open();
                document.Add(title);
                PdfPTable table = new PdfPTable(10);
                PdfPCell cell;
                cell = new PdfPCell(new Paragraph(" "));
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_BOTTOM;
                cell.Border = Rectangle.NO_BORDER;
                cell.Colspan = 10;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(" "));
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_BOTTOM;
                cell.Border = Rectangle.NO_BORDER;
                cell.Colspan = 5;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("□有货就发", font1));
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_BOTTOM;
                cell.Border = Rectangle.NO_BORDER;
                cell.Colspan = 2;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("□发货时间:", font1));
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_BOTTOM;
                cell.Border = Rectangle.NO_BORDER;
                cell.Colspan = 3;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("订单编号:", font1));
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(" "));
                cell.Colspan = 4;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("客户名称:", font1));
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(" "));
                cell.Colspan = 4;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("订单类型:", font1));
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(" "));
                cell.Colspan = 4;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("业务对接:", font1));
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(" "));
                cell.Colspan = 4;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(" "));
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_BOTTOM;
                cell.Border = Rectangle.NO_BORDER;
                cell.Colspan = 10;
                table.AddCell(cell);
                // 产品标题
                cell = new PdfPCell(new Paragraph("订单信息", font1));
                cell.Colspan = 10;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("订货情况", font1));
                cell.Rowspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("订单内容", font1));
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.Rowspan = 1;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("产品代码", font1));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("订货品种", font1));
                cell.Colspan = 2;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("订货数量", font1));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("订货单价", font1));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("货款金额", font1));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("备注", font1));
                cell.Colspan = 2;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                //foreach(var name in plattform_name)
                //{
                //    cell = new PdfPCell(new Paragraph(name.System_Code.ToString(), font2));
                //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                //    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                //    table.AddCell(cell);
                //    cell = new PdfPCell(new Paragraph(name.Item_Name.ToString(), font2));
                //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                //    cell.Colspan = 2;
                //    table.AddCell(cell);
                //    cell = new PdfPCell(new Paragraph(" "));
                //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                //    table.AddCell(cell);
                //    cell = new PdfPCell(new Paragraph(name.Purchase_Price.ToString(), font2));
                //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                //    table.AddCell(cell);
                //    cell = new PdfPCell(new Paragraph(" "));
                //    table.AddCell(cell);
                //    cell = new PdfPCell(new Paragraph(" "));
                //    cell.Colspan = 2;
                //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                //    table.AddCell(cell);
                //}
                cell = new PdfPCell(new Paragraph(" "));
                cell.Colspan = 2;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("合计", font1));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("--"));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.Colspan = 2;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("数量", font1));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("--"));
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("价格", font1));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(" "));
                cell.Colspan = 2;
                table.AddCell(cell);
                // 付款
                cell = new PdfPCell(new Paragraph("付款内容", font1));
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Rowspan = 3;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("1"));
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("付款方式", font1));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(" "));
                cell.Colspan = 2;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("开票内容", font1));
                cell.Rowspan = 3;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("1"));
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("开票类型"));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(" "));
                cell.Colspan = 2;
                table.AddCell(cell);
                // 2
                cell = new PdfPCell(new Paragraph("2"));
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("付款时间", font1));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(" "));
                cell.Colspan = 2;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("2"));
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("开票内容"));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(" "));
                cell.Colspan = 2;
                table.AddCell(cell);
                //3 
                cell = new PdfPCell(new Paragraph("3"));
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("付款金额", font1));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(" "));
                cell.Colspan = 2;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("3"));
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("开票时间"));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(" "));
                cell.Colspan = 2;
                table.AddCell(cell);
                // 收货
                cell = new PdfPCell(new Paragraph("收货人信息", font1));
                cell.Colspan = 2;
                cell.Rowspan = 5;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("收货人", font1));
                cell.Colspan = 3;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(" "));
                cell.Colspan = 5;
                table.AddCell(cell);

                cell = new PdfPCell(new Paragraph("收货人联系电话", font1));
                cell.Colspan = 3;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(" "));
                cell.Colspan = 5;
                table.AddCell(cell);

                cell = new PdfPCell(new Paragraph("收货地址", font1));
                cell.Colspan = 3;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(" "));
                cell.Colspan = 5;
                table.AddCell(cell);


                cell = new PdfPCell(new Paragraph("运输方式（特批加急）", font1));
                cell.Colspan = 3;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("□快递-顺丰  □物流-德邦  □包车", font1));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 5;
                table.AddCell(cell);

                cell = new PdfPCell(new Paragraph("最后收货期限", font1));
                cell.Colspan = 3;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("     年     月     日", font1));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 5;
                table.AddCell(cell);

                document.Add(table);
            }
            catch (DocumentException de)
            {
                Console.Error.WriteLine(de.Message);
            }
            catch (IOException ioe)
            {
                Console.Error.WriteLine(ioe.Message);
            }
            // 关闭文档
            document.Close();
            return Content("success");
        }
    }
}