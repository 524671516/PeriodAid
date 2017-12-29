using CsvHelper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using PagedList;
using PeriodAid.DAL;
using PeriodAid.Filters;
using PeriodAid.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        public ActionResult ProductDetail(int productId)
        {
            var product = _db.SP_Product.SingleOrDefault(m => m.Id == productId);
            ViewBag.Product = product;
            return PartialView(product);
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
        [Seller(OperationGroup = 101)]
        public ActionResult AddProductPartial(SP_Product model, FormCollection form)
        {
            bool Product = _db.SP_Product.Any(m => m.Item_Name == model.Item_Name);
            ModelState.Remove("Product_Status");
            ModelState.Remove("Item_Code");
            ModelState.Remove("System_Code");
            ModelState.Remove("Item_ShortName");
            ModelState.Remove("Bar_Code");
            ModelState.Remove("Product_Img");
            if (ModelState.IsValid)
            {
                if (Product)
                {
                    return Json(new { result = "UNAUTHORIZED" });
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
                    item.Product_Img = model.Product_Img;
                    _db.SP_Product.Add(item);
                    _db.Configuration.ValidateOnSaveEnabled = false;
                    _db.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
            }
            else
            {
                AddProductViewBag();
                return Json(new { result = "FAIL" });
            }
        }

        public ActionResult EditProductInfo(int productId)
        {
            var item = _db.SP_Product.SingleOrDefault(m => m.Id == productId);
            AddProductViewBag();
            return PartialView(item);
        }
        [HttpPost]
        [Seller(OperationGroup = 103)]
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
        [Seller(OperationGroup = 103)]
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
                                        where m.Client_Status != -1 && m.SP_Seller.Seller_Status != -1
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

        public ActionResult SalesList(int? clientId)
        {
            return View();
        }

        public ActionResult SalesListPartial(int? page, string query,int? clientId)
        {
            int _page = page ?? 1;
            var seller = getSeller(User.Identity.Name);
            if (clientId == null)
            {
                if (seller.Seller_Type == 0)
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
            else
            {
                if (seller.Seller_Type == 0)
                {
                    if (query != "")
                    {
                        var SearchResult = (from m in _db.SP_SalesSystem
                                            where m.System_Status != -1 && m.SP_Client.Seller_Id == seller.Id && m.Client_Id == clientId
                                            && m.System_Name.Contains(query) || m.System_Phone.Contains(query)
                                            orderby m.Id descending
                                            select m).ToPagedList(_page, 15);
                        return PartialView(SearchResult);
                    }
                    else
                    {
                        var SearchResult = (from m in _db.SP_SalesSystem
                                            where m.System_Status != -1 && m.SP_Client.Seller_Id == seller.Id && m.Client_Id == clientId
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
                                            where m.System_Status != -1 && m.SP_Client.SP_Seller.Seller_Type <= seller.Seller_Type && m.Client_Id == clientId
                                            && m.System_Name.Contains(query) || m.System_Phone.Contains(query)
                                            orderby m.Id descending
                                            select m).ToPagedList(_page, 15);
                        return PartialView(SearchResult);
                    }
                    else
                    {
                        var SearchResult = (from m in _db.SP_SalesSystem
                                            where m.System_Status != -1 && m.SP_Client.SP_Seller.Seller_Type <= seller.Seller_Type && m.Client_Id == clientId
                                            orderby m.Id descending
                                            select m).ToPagedList(_page, 15);
                        return PartialView(SearchResult);
                    }
                }
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
                            where m.SalesSystem_Id == SalesSystemId
                            select m;
                var SearchResult = from m in price
                                   where m.SP_Product.Item_Name.Contains(query) || m.SP_Product.System_Code.Contains(query) 
                                   || m.SP_Product.Item_Code.Contains(query)
                                   orderby m.Quoted_Status descending
                                   select m;
                return PartialView(SearchResult);
            }
            else
            {
                var SearchResult = from m in _db.SP_QuotePrice
                                   where m.SalesSystem_Id == SalesSystemId
                                   orderby m.Quoted_Status descending
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
            bool QuotePrice = _db.SP_QuotePrice.Any(m => m.Product_Id == model.Product_Id && m.Quoted_Date == model.Quoted_Date && m.SalesSystem_Id == model.SalesSystem_Id);
            var QuoteDate = _db.SP_QuotePrice.SingleOrDefault(m => m.Product_Id == model.Product_Id && m.Quoted_Status == 0 && m.SalesSystem_Id == model.SalesSystem_Id);
            ModelState.Remove("Quoted_Date");
            if (ModelState.IsValid)
            {
                if (QuotePrice)
                {
                    return Json(new { result = "UNAUTHORIZED" });
                }
                else
                {
                    if (QuoteDate != null)
                    {
                        QuoteDate.Quoted_Status = -1;
                        _db.Entry(QuoteDate).State = System.Data.Entity.EntityState.Modified;
                        _db.SaveChanges();
                    }
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
                AddProductViewBag();
                return Json(new { result = "FAIL" });
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
            ModelState.Remove("Quoted_Date");
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
            var seller = getSeller(User.Identity.Name);
            int _page = page ?? 1;
            if (query != "")
            {
                if (seller.Seller_Type != SellerType.ADMINISTARTOR)
                {
                    var sellerList = (from m in _db.SP_Seller
                                      where m.Seller_Status != -1 && m.Manager_Id == seller.Manager_Id && m.Seller_Name.Contains(query) || m.Seller_Mobile.Contains(query)
                                      orderby m.Id
                                      select m).ToPagedList(_page, 15);
                    return PartialView(sellerList);
                }
                else {

                    var sellerList = (from m in _db.SP_Seller
                                      where m.Seller_Status != -1 && m.Seller_Name.Contains(query) || m.Seller_Mobile.Contains(query)
                                      orderby m.Id
                                      select m).ToPagedList(_page, 15);
                    return PartialView(sellerList);
                }
            }
            else
            {
                if (seller.Seller_Type != SellerType.ADMINISTARTOR)
                {
                    var sellerList = (from m in _db.SP_Seller
                                      where m.Seller_Status != -1 && m.Manager_Id == seller.Manager_Id
                                      orderby m.Id
                                      select m).ToPagedList(_page, 15);
                    return PartialView(sellerList);
                }
                else
                {

                    var sellerList = (from m in _db.SP_Seller
                                      where m.Seller_Status != -1
                                      orderby m.Id
                                      select m).ToPagedList(_page, 15);
                    return PartialView(sellerList);
                }
            }
        }

        public void AddSellerViewBag()
        {
            var seller = getSeller(User.Identity.Name);
            ViewBag.Seller = seller;
            List<SelectListItem> sellerType = new List<SelectListItem>();
            sellerType.Add(new SelectListItem() { Text = "业务员", Value = SellerType.SELLER.ToString() });
            sellerType.Add(new SelectListItem() { Text = "产品部", Value = SellerType.PRODUCTDEPARTMENT.ToString() });
            sellerType.Add(new SelectListItem() { Text = "财务部", Value = SellerType.FINANCIALDEPARTMENT.ToString() });
            sellerType.Add(new SelectListItem() { Text = "业务主管", Value = SellerType.SELLERADMIN.ToString() });
            sellerType.Add(new SelectListItem() { Text = "管理员", Value = SellerType.ADMINISTARTOR.ToString() });
            ViewBag.SellerType = new SelectList(sellerType, "Value", "Text");
            List<SelectListItem> managerlist = new List<SelectListItem>();
            var managername = from m in _db.SP_Seller
                              where m.Seller_Type > SellerType.FINANCIALDEPARTMENT
                              select m;
            foreach (var name in managername)
            {
                managerlist.Add(new SelectListItem() { Text = name.Seller_Name, Value = name.Id.ToString() });
            }
            ViewBag.Manager = new SelectList(managerlist, "Value", "Text");
            List<SelectListItem> department = new List<SelectListItem>();
            var departmentname = from m in _db.SP_Department
                                 select m;
            foreach (var name in departmentname)
            {
                department.Add(new SelectListItem() { Text = name.Department_Name, Value = name.Id.ToString() });
            }
            ViewBag.Department = new SelectList(department, "Value", "Text");
        }

        public ActionResult AddSellerPartial()
        {
            AddSellerViewBag();
            return PartialView();
        }
        [HttpPost]
        [Seller(OperationGroup = 701)]
        public ActionResult AddSellerPartial(SP_Seller model, FormCollection form)
        {
            bool Seller = _db.SP_Seller.Any(m => m.Seller_Name == model.Seller_Name && m.Seller_Mobile == model.Seller_Mobile);
            if (ModelState.IsValid)
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
                    seller.Department_Id = model.Department_Id;
                    seller.Seller_Status = 0;
                    if (model.Seller_Type == SellerType.SELLER || model.Seller_Type == SellerType.FINANCIALDEPARTMENT || model.Seller_Type == SellerType.PRODUCTDEPARTMENT)
                    {
                        seller.Manager_Id = model.Manager_Id;
                        _db.SP_Seller.Add(seller);
                        _db.SaveChanges();
                        
                    }
                    else
                    {
                        seller.Manager_Id = null;
                        _db.SP_Seller.Add(seller);
                        _db.SaveChanges();
                        var newseller = _db.SP_Seller.SingleOrDefault(m => m.Id == seller.Id);
                        newseller.Manager_Id = newseller.Id.ToString();
                        _db.Entry(newseller).State = System.Data.Entity.EntityState.Modified;
                        _db.SaveChanges();
                    }
                    return Json(new { result = "SUCCESS" });


                }
            }
            else
            {
                ModelState.AddModelError("", "错误");
                AddSellerViewBag();
                return Json(new { result = "FAIL" });
            }
        }

        public ActionResult EditSellerInfo(int sellerId)
        {
            var Seller = _db.SP_Seller.SingleOrDefault(m => m.Id == sellerId);
            AddSellerViewBag();
            return PartialView(Seller);
        }
        [HttpPost]
        [Seller(OperationGroup = 703)]
        public ActionResult EditSellerInfo(SP_Seller model)
        {
            bool Seller = _db.SP_Seller.Any(m => m.Seller_Name == model.Seller_Name && m.Seller_Mobile == model.Seller_Mobile && m.Seller_Type == model.Seller_Type && m.Department_Id == model.Department_Id && m.Manager_Id == model.Manager_Id);
            ModelState.Remove("User_Name");
            ModelState.Remove("Seller_Mobile");
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

        public ActionResult OrderList(int? clientId)
        {
            return View();
        }

        public ActionResult OrderListPartial(int? page, string query,int? clientId)
        {
            var seller = getSeller(User.Identity.Name);
            int _page = page ?? 1;
            if (clientId == null)
            {
                if (seller.Seller_Type == 0)
                {
                    if (query != "")
                    {
                        var orderList = (from m in _db.SP_Order
                                         where m.SP_Contact.SP_Client.Seller_Id == seller.Id && m.Order_Number.Contains(query) 
                                         || m.SP_Contact.SP_Client.Client_Name.Contains(query) && m.Order_Status != -1
                                         orderby m.Id
                                         select m).ToPagedList(_page, 15);
                        return PartialView(orderList);
                    }
                    else
                    {
                        var SearchResult = (from m in _db.SP_Order
                                            where m.Order_Status != -1 && m.SP_Contact.SP_Client.Seller_Id == seller.Id
                                            orderby m.Id ascending
                                            select m).ToPagedList(_page, 15);
                        return PartialView(SearchResult);
                    }
                }
                else
                {
                    if (query != "")
                    {
                        var orderList = (from m in _db.SP_Order
                                         where m.Order_Number.Contains(query)|| m.SP_Contact.SP_Client.Client_Name.Contains(query) 
                                         && m.Order_Status != -1
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
            }
            else
            {
                if (seller.Seller_Type == 0)
                {
                    if (query != "")
                    {
                        var orderList = (from m in _db.SP_Order
                                         where m.SP_Contact.Client_Id == clientId && m.SP_Contact.SP_Client.Seller_Id == seller.Id 
                                         && m.Order_Number.Contains(query) || m.SP_Contact.SP_Client.Client_Name.Contains(query) && m.Order_Status != -1
                                         orderby m.Id
                                         select m).ToPagedList(_page, 15);
                        return PartialView(orderList);
                    }
                    else
                    {
                        var SearchResult = (from m in _db.SP_Order
                                            where m.Order_Status != -1 && m.SP_Contact.Client_Id == clientId 
                                            && m.SP_Contact.SP_Client.Seller_Id == seller.Id
                                            orderby m.Id ascending
                                            select m).ToPagedList(_page, 15);
                        return PartialView(SearchResult);
                    }
                }
                else
                {
                    if (query != "")
                    {
                        var orderList = (from m in _db.SP_Order
                                         where m.Order_Number.Contains(query)|| m.SP_Contact.SP_Client.Client_Name.Contains(query) 
                                         && m.Order_Status != -1 && m.SP_Contact.Client_Id == clientId
                                         orderby m.Id
                                         select m).ToPagedList(_page, 15);
                        return PartialView(orderList);
                    }
                    else
                    {
                        var SearchResult = (from m in _db.SP_Order
                                            where m.Order_Status != -1 && m.SP_Contact.Client_Id == clientId
                                            orderby m.Id ascending
                                            select m).ToPagedList(_page, 15);
                        return PartialView(SearchResult);
                    }
                }
            }
            
            
        }

        public ActionResult AddOrderPartial()
        {
            Random ran = new Random();
            int RandKey = ran.Next(01, 99);
            var ordernumber = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString();
            var OrderNum = int.Parse(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + RandKey);
            var strNum = OrderNum.ToString();
            if (strNum.Length == 1)
            {
                strNum = "00" + strNum;
            }
            else if (strNum.Length == 2)
            {
                strNum = "0" + strNum;
            }
            ordernumber += strNum;
            ViewBag.ordernumber = ordernumber;
            return PartialView();
        }
        [HttpPost]
        [Seller(OperationGroup = 601)]
        public ActionResult AddOrderPartial(SP_Order model, FormCollection form)
        {
            bool Order = _db.SP_Order.Any( m => m.Order_Number == model.Order_Number);
            ModelState.Remove("Order_Date");
            if (ModelState.IsValid)
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
                    order.Order_Date = model.Order_Date;
                    order.Contact_Id = model.Contact_Id;
                    _db.SP_Order.Add(order);
                    _db.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }

            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }

        public ActionResult EditOrderInfo(int orderId)
        {
            var Order = _db.SP_Order.SingleOrDefault(m => m.Id == orderId);
            var contact = (from m in _db.SP_Order
                          where m.Id == orderId
                          select m).FirstOrDefault();
            ViewBag.Contact = contact;
            return PartialView(Order);
        }
        [HttpPost]
        [Seller(OperationGroup = 603)]
        public ActionResult EditOrderInfo(SP_Order model)
        {
            bool Order = _db.SP_Order.Any(m => m.Order_Date == model.Order_Date && m.Order_Number == model.Order_Number && m.Contact_Id == model.Contact_Id);
            ModelState.Remove("Order_Date");
            ModelState.Remove("Order_Status");
            ModelState.Remove("Contact_Id");
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
        [Seller(OperationGroup = 603)]
        public ActionResult DeleteOrder(int orderId)
        {
            var Order = _db.SP_Order.AsNoTracking().SingleOrDefault(m => m.Id == orderId);
            Order.Order_Status = -1;
            _db.Entry(Order).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            return Json(new { result = "SUCCESS" });

        }
        
        public ActionResult OrderPriceList(int orderId)
        {
            var order = (from m in _db.SP_Order
                         where m.Id == orderId
                         select m).FirstOrDefault();
            ViewBag.Order = order;
            return View();
        }

        public ActionResult OrderPriceListPartial(int orderId)
        {
            var order = from m in _db.SP_OrderPrice
                        where m.Order_Id == orderId && m.OrderPrice_Status != -1
                        select m;
            ViewBag.Order = order;

            var order_num = (from m in _db.SP_Order
                             where m.Id == orderId && m.Order_Status != -1
                             select m).FirstOrDefault();
            ViewBag.OrderNum = order_num;

            var Price = from m in _db.SP_OrderPrice
                        where m.OrderPrice_Status != -1 && m.Order_Id == orderId
                        group m by m.Id into g
                        select new OrderPriceSum
                        {
                            SumCount = g.Sum(m => m.Order_Count),
                            SumPrice = g.Sum(m => m.Order_Price)
                        };
            int sumCount = 0;
            decimal sumPrice = 0;
            foreach (var price in Price)
            {
                sumCount += price.SumCount;
                var Sumprice = price.SumCount * price.SumPrice;
                sumPrice += Sumprice;
            }
            ViewBag.Count = sumCount;
            ViewBag.Price = sumPrice;
            return PartialView();
        }

        public ActionResult AddOrderPricePartial(int orderId)
        {
            var order = (from m in _db.SP_Order
                         where m.Id == orderId
                         select m).FirstOrDefault();
            ViewBag.Order = order;
            return PartialView();
        }
        [HttpPost]
        public ActionResult AddOrderPricePartial(SP_OrderPrice model, FormCollection form)
        {
            
            ModelState.Remove("Order_Count");
            ModelState.Remove("Order_Price");
            if (ModelState.IsValid)
            {

                var productlist = from m in _db.SP_QuotePrice
                                  where m.Quoted_Status != -1
                                  select m;
                foreach (var product in productlist)
                {
                    bool OrderPrice = _db.SP_OrderPrice.Any(m => m.Product_Id == product.Product_Id && m.Order_Id == model.Order_Id && m.OrderPrice_Status != -1);
                    if (OrderPrice)
                    {
                        return Json(new { result = "UNAUTHORIZED" });
                    }
                    else
                    {

                        int order = 0;
                        if (form["order_" + product.Id] != "")
                            order = Convert.ToInt32(form["order_" + product.Id]);
                        if (order == 0)
                        {
                        }
                        else
                        {
                            var orderprice = new SP_OrderPrice();
                            {
                                orderprice.Order_Count = order;
                                orderprice.Order_Price = product.Quote_Price;
                                orderprice.Product_Id = product.Product_Id;
                                orderprice.OrderPrice_Status = 0;
                                orderprice.Order_Id = model.Order_Id;
                            };
                            _db.SP_OrderPrice.Add(orderprice);
                        }
                    }

                }
                _db.SaveChangesAsync();
                return Json(new { result = "SUCCESS" });

            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }

        public JsonResult AllProductAjax(int clientId)
        {
            var product = from m in _db.SP_QuotePrice
                          where m.Quoted_Status != -1 && m.SP_SalesSystem.Client_Id == clientId
                          select new { Id = m.Id, ItemName =m.SP_SalesSystem.System_Name+"-"+ m.SP_Product.Item_Name };
            return Json(new { result = "SUCCESS", data = product }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditOrderPriceInfo(int orderPriceId)
        {
            var OrderPrice = _db.SP_OrderPrice.SingleOrDefault(m => m.Id == orderPriceId);
            var product = (from m in _db.SP_OrderPrice
                           where m.Id == orderPriceId
                           select m).FirstOrDefault();
            ViewBag.Product = product;
            return PartialView(OrderPrice);
        }
        [HttpPost]
        public ActionResult EditOrderPriceInfo(SP_OrderPrice model)
        {
            bool Order = _db.SP_OrderPrice.Any(m => m.Order_Count == model.Order_Count && m.Order_Id == model.Order_Id && m.Product_Id == model.Product_Id && m.OrderPrice_Status != -1);
            if (ModelState.IsValid)
            {
                if (Order)
                {
                    return Json(new { result = "UNAUTHORIZED" });
                }
                else
                {
                    SP_OrderPrice orderPrice = new SP_OrderPrice();
                    if (TryUpdateModel(orderPrice))
                    {
                        _db.Entry(orderPrice).State = System.Data.Entity.EntityState.Modified;
                        _db.SaveChanges();
                        return Json(new { result = "SUCCESS" });
                    }
                }
            }
            return Json(new { result = "FAIL" });
        }
        [HttpPost]
        public ActionResult DeleteOrderPrice(int orderPriceId)
        {
            var OrderPrice = _db.SP_OrderPrice.AsNoTracking().SingleOrDefault(m => m.Id == orderPriceId);
            OrderPrice.OrderPrice_Status = -1;
            _db.Entry(OrderPrice).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            return Json(new { result = "SUCCESS" });

        }
        // 搜索
        [HttpPost]
        public JsonResult QueryClient(string query)
        {
            var seller = getSeller(User.Identity.Name);
            if (seller.Seller_Type == 0)
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
        [HttpPost]
        public JsonResult QueryContact(string query)
        {
            var seller = getSeller(User.Identity.Name);
            if (seller.Seller_Type == 0)
            {
                var client = from m in _db.SP_Contact
                             where m.Contact_Status != -1 && m.SP_Client.Seller_Id == seller.Id
                             && m.SP_Client.Client_Name.Contains(query)
                             select new { Id = m.Id, Contact_Name = m.SP_Client.Client_Name+"-"+m.Contact_Name };
                return Json(client);
            }
            else
            {
                var client = from m in _db.SP_Contact
                             where m.Contact_Status != -1 && m.SP_Client.SP_Seller.Seller_Type <= seller.Seller_Type
                             && m.SP_Client.Client_Name.Contains(query)
                             select new { Id = m.Id, Contact_Name = m.SP_Client.Client_Name + "-" + m.Contact_Name };
                return Json(client);
            }
        }
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
        //生成PDF
        [HttpPost]
        public ActionResult OrderPdf(int orderId)
        {
            var product = from m in _db.SP_OrderPrice
                          where m.OrderPrice_Status != -1 && m.Order_Id == orderId
                          select m;
            var orderNum = _db.SP_Order.SingleOrDefault(m => m.Id == orderId);
            var count = product.Count();
            Document document = new Document(PageSize.A3);
            try
            {
                // 创建文档
                PdfWriter.GetInstance(document, new FileStream(@"C:\Users\Tork\Downloads\Create.pdf", FileMode.Append));
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
                cell = new PdfPCell(new Phrase(orderNum.Order_Number.ToString(),font1));
                cell.Colspan = 4;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("客户名称:", font1));
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(orderNum.SP_Contact.SP_Client.Client_Name.ToString(),font1));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 4;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("订单类型:", font1));
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(" "));
                cell.Colspan = 4;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("业务对接:", font1));
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(orderNum.SP_Contact.SP_Client.SP_Seller.Seller_Name.ToString(),font1));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 4;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(" "));
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_BOTTOM;
                cell.Border = Rectangle.NO_BORDER;
                cell.Colspan = 10;
                table.AddCell(cell);
                // 产品标题
                cell = new PdfPCell(new Paragraph("I.订单信息", font1));
                cell.Colspan = 10;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("订货情况", font1));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("订单内容", font1));
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
                cell = new PdfPCell(new Paragraph("订货品种", font1));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.Rowspan = count;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("订单内容", font1));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.Rowspan = count;
                table.AddCell(cell);
                foreach (var order in product)
                {
                    cell = new PdfPCell(new Paragraph(order.SP_Product.Item_Code, font2));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Paragraph(order.SP_Product.Item_Name.ToString(), font2));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.Colspan = 2;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Paragraph(order.Order_Count.ToString(), font2));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Paragraph(order.Order_Price.ToString(), font2));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Paragraph(Math.Round((order.Order_Price * order.Order_Count), 2).ToString(), font2));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Paragraph(" ", font2));
                    cell.Colspan = 2;
                    table.AddCell(cell);
                }
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
                var Price = from m in _db.SP_OrderPrice
                            where m.OrderPrice_Status != -1 && m.Order_Id == orderId
                            group m by m.Id into g
                            select new OrderPriceSum
                            {
                                SumCount = g.Sum(m=>m.Order_Count),
                                SumPrice = g.Sum(m=>m.Order_Price)
                            };
                decimal SumPrice = 0;
                int SumCount = 0;
                foreach (var price in Price)
                {
                    SumCount += price.SumCount;
                    var sumPrice = price.SumPrice * price.SumCount;
                    SumPrice += sumPrice;
                }
                cell = new PdfPCell(new Paragraph(SumCount.ToString(), font1));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("--"));
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(Math.Round(SumPrice,2).ToString(), font1));
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
                // 发货
                cell = new PdfPCell(new Paragraph("II.发货审核", font1));
                cell.Colspan = 10;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);

                cell = new PdfPCell(new Paragraph("业务对接人签署：", font1));
                cell.Colspan = 3;
                cell.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER|Rectangle.TOP_BORDER;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("负责人签署：", font1));
                cell.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER | Rectangle.TOP_BORDER;
                cell.Colspan = 3;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("ERP制单", font1));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 2;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("ERP审核", font1));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 2;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(""));
                cell.Border = Rectangle.RIGHT_BORDER | Rectangle.LEFT_BORDER;
                cell.Colspan = 3;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(""));
                cell.Border = Rectangle.RIGHT_BORDER | Rectangle.LEFT_BORDER;
                cell.Colspan = 3;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(" "));
                cell.Border = Rectangle.RIGHT_BORDER | Rectangle.LEFT_BORDER;
                cell.Colspan = 2;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(" "));
                cell.Border = Rectangle.RIGHT_BORDER | Rectangle.LEFT_BORDER;
                cell.Colspan = 2;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("[       ]年[    ]月[    ]日", font1));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Border = Rectangle.RIGHT_BORDER|Rectangle.LEFT_BORDER|Rectangle.BOTTOM_BORDER;
                cell.Colspan = 3;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("[       ]年[    ]月[    ]日", font1));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Border = Rectangle.RIGHT_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER;
                cell.Colspan = 3;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(""));
                cell.Border = Rectangle.RIGHT_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER;
                cell.Colspan = 2;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(""));
                cell.Border = Rectangle.RIGHT_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER;
                cell.Colspan = 2;
                table.AddCell(cell);
                // 回执
                cell = new PdfPCell(new Paragraph("III.发货回执", font1));
                cell.Colspan = 10;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);

                cell = new PdfPCell(new Paragraph("发货物流或快递名称", font2));
                cell.Colspan = 2;
                table.AddCell(cell);

                cell = new PdfPCell(new Paragraph(" "));
                cell.Colspan = 8;
                table.AddCell(cell);

                cell = new PdfPCell(new Paragraph("快递或物流单号", font2));
                cell.Colspan = 2;
                table.AddCell(cell);

                cell = new PdfPCell(new Paragraph(" "));
                cell.Colspan = 8;
                table.AddCell(cell);

                cell = new PdfPCell(new Paragraph("预计到货日期", font2));
                cell.Colspan = 2;
                table.AddCell(cell);

                cell = new PdfPCell(new Paragraph("[          ]年[      ]月[      ]日",font2));
                cell.Colspan = 8;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                // 
                cell = new PdfPCell(new Paragraph("产品部签署:", font2));
                cell.Border = Rectangle.LEFT_BORDER|Rectangle.RIGHT_BORDER;
                cell.Colspan = 10;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("[确认已经完成发货]", font2));
                cell.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;
                cell.Colspan = 10;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("[        ]年[    ]月[    ]日", font2));
                cell.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER|Rectangle.BOTTOM_BORDER;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 10;
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
            return Json(new { result= "SUCCESS"});
        }
        // 上传图片
        [HttpPost]
        public ActionResult UpLoadImg(FormCollection form)
        {
            var files = Request.Files;
            string msg = string.Empty;
            string error = string.Empty;
            string imgurl;
            if (files.Count > 0)
            {
                if (files[0].ContentLength > 0 && files[0].ContentType.Contains("image"))
                {
                    string filename = files[0].FileName; //改filename公式
                    string _filename = DateTime.Now.ToFileTime().ToString() + "sqzweb" + filename.ToString().Substring(filename.ToString().LastIndexOf("."));
                    //files[0].SaveAs(Server.MapPath("/Content/checkin-img/") + filename);
                    AliOSSUtilities util = new AliOSSUtilities();
                    util.PutWebObject(files[0].InputStream, "Content/" + _filename);
                    msg = "成功! 文件大小为:" + files[0].ContentLength;
                    imgurl = "http://cdn.shouquanzhai.cn/Content/" + _filename;
                    string res = "{ error:'" + error + "', msg:'" + msg + "',imgurl:'" + imgurl + "'}";
                    return Content(res);
                }
                else
                {
                    error = "文件错误";
                }
            }
            string err_res = "{ error:'" + error + "', msg:'" + msg + "',imgurl:''}";
            return Content(err_res);

        }
        // 报价单导出
        [HttpPost]
        public ActionResult getQuotePrice(FormCollection form,int SalesSystemId)
        {
           
            HSSFWorkbook book = new HSSFWorkbook();
            var price_list = _db.SP_QuotePrice.Where(m => m.SalesSystem_Id == SalesSystemId && m.Quoted_Status != -1);
            ISheet sheet = book.CreateSheet("报价单");
            // 写标题
            IRow row = sheet.CreateRow(0);
            int cell_pos = 0;
            row.CreateCell(cell_pos).SetCellValue("商品编码");
            row.CreateCell(++cell_pos).SetCellValue("产品名称");
            row.CreateCell(++cell_pos).SetCellValue("箱规");
            row.CreateCell(++cell_pos).SetCellValue("单价");
            int row_pos = 1;
            foreach (var price in price_list)
            {
                IRow single_row = sheet.CreateRow(row_pos);
                cell_pos = 0;
                single_row.CreateCell(cell_pos).SetCellValue(price.SP_Product.Item_Code);
                single_row.CreateCell(++cell_pos).SetCellValue(price.SP_Product.Item_Name);
                single_row.CreateCell(++cell_pos).SetCellValue(price.SP_Product.Carton_Spec);
                single_row.CreateCell(++cell_pos).SetCellValue((double)(price.Quote_Price));
                row_pos++;
            }
            
            MemoryStream _stream = new MemoryStream();
            book.Write(_stream);
            _stream.Flush();
            _stream.Seek(0, SeekOrigin.Begin);
            return File(_stream, "application/vnd.ms-excel", DateTime.Now.ToString("yyyyMMddHHmmss") + "报价单.xls");
        }
    }
}