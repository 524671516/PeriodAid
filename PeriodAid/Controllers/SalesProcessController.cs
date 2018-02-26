using CsvHelper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.HSSF.UserModel;
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
    public class SalesProcessController : Controller
    {
        // GET: SalesProcess
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private SalesProcessModel _db;
        private IKCRMDATAModel crm_db;
        public SalesProcessController()
        {
            _db = new SalesProcessModel();
            crm_db = new IKCRMDATAModel();
        }
        public ActionResult Index()
        {
            return View();
        }

        private byte[] SetFileToByteArray(HttpPostedFileBase File)
        {
            Stream stream = File.InputStream;
            byte[] ArrayByte = new byte[File.ContentLength];
            stream.Read(ArrayByte, 0, File.ContentLength);
            stream.Close();
            return ArrayByte;
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
        [Seller(OperationGroup = 102)]
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
            ModelState.Remove("Brand_Name");
            ModelState.Remove("System_Code");
            ModelState.Remove("Item_ShortName");
            ModelState.Remove("Bar_Code");
            ModelState.Remove("Product_Img");
            ModelState.Remove("ProductType_Id");
            ModelState.Remove("Product_Status");
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
                    item.Product_Standard = model.Product_Standard;
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

        [Seller(OperationGroup = 202)]
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
                                        where m.Client_Name.Contains(query) || m.Client_Area.Contains(query) || m.SP_Seller.Seller_Name.Contains(query)
                                        orderby m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
                else
                {
                    var SearchResult = (from m in _db.SP_Client
                                        where m.Client_Status != -1 && m.Seller_Id == seller.Id
                                        orderby m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
            }
            else
            {
                if (query != "")
                {
                    var customer = (from m in _db.SP_Client
                                    where m.Client_Status != -1
                                    select m);
                    var SearchResult = (from m in customer
                                        where m.Client_Name.Contains(query) || m.Client_Area.Contains(query) || m.SP_Seller.Seller_Name.Contains(query)
                                        orderby m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
                else
                {
                    var SearchResult = (from m in _db.SP_Client
                                        where m.Client_Status != -1 && m.SP_Seller.Seller_Status != -1
                                        orderby m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
            }

        }

        public void AddClientViewBag()
        {
            var seller = getSeller(User.Identity.Name);
            ViewBag.Seller = seller;
            List<SelectListItem> itemlist = new List<SelectListItem>();
            itemlist.Add(new SelectListItem() { Text = "活跃", Value = "1" });
            itemlist.Add(new SelectListItem() { Text = "待开发", Value = "0" });
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
                    client.Client_Address = model.Client_Address;
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
            var client = (from m in _db.SP_Client
                          where m.Id == clientId && m.SP_Seller.Seller_Type <= seller.Seller_Type
                          select m).FirstOrDefault();
            ViewBag.Client = client;
            var item = _db.SP_Client.SingleOrDefault(m => m.Id == clientId);
            AddClientViewBag();
            return PartialView(item);
        }
        [HttpPost]
        [Seller(OperationGroup = 203)]
        public ActionResult EditClientInfo(SP_Client model)
        {
            bool Client = _db.SP_Client.Any(m => m.Client_Name == model.Client_Name && m.Client_Area == model.Client_Area && m.Client_Type == model.Client_Type && m.Client_Status == model.Client_Status && m.Seller_Id == model.Seller_Id && m.Client_Address == model.Client_Address);
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
                    contact.Contact_Job = model.Contact_Job;
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
            bool Contact = _db.SP_Contact.Any(m => m.Contact_Name == model.Contact_Name && m.Contact_Mobile == model.Contact_Mobile && m.Contact_Job == model.Contact_Job);
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
        [Seller(OperationGroup = 402)]
        public ActionResult SalesListPartial(int? page, string query, int? clientId)
        {
            int _page = page ?? 1;
            var seller = getSeller(User.Identity.Name);
            if (clientId == null)
            {
                if (seller.Seller_Type == 0)
                {
                    if (query != "")
                    {
                        var sales = from m in _db.SP_SalesSystem
                                    where m.System_Status != -1 && m.SP_Client.Seller_Id == seller.Id
                                    select m;
                        var SearchResult = (from m in sales
                                            where m.System_Name.Contains(query) || m.System_Phone.Contains(query)
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
                        var sales = from m in _db.SP_SalesSystem
                                    where m.System_Status != -1
                                    select m;
                        var SearchResult = (from m in sales
                                            where m.System_Name.Contains(query) || m.System_Phone.Contains(query)
                                            orderby m.Id descending
                                            select m).ToPagedList(_page, 15);
                        return PartialView(SearchResult);
                    }
                    else
                    {
                        var SearchResult = (from m in _db.SP_SalesSystem
                                            where m.System_Status != -1
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
                        var sales = from m in _db.SP_SalesSystem
                                    where m.System_Status != -1 && m.SP_Client.Seller_Id == seller.Id && m.Client_Id == clientId
                                    select m;
                        var SearchResult = (from m in sales
                                            where m.System_Name.Contains(query) || m.System_Phone.Contains(query)
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
                                            where m.System_Status != -1 && m.Client_Id == clientId
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

        public ActionResult AddSalesPartial(int? clientId)
        {
            var _client = clientId ?? 0;
            if (_client == 0)
            {
                ViewBag.Client = null;
            }
            else
            {
                var Client = (from m in _db.SP_Client
                              where m.Id == clientId
                              select m).FirstOrDefault();
                ViewBag.Client = Client;
            }

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

        public ActionResult QuotePricrListPartial(int? page, string query, int SalesSystemId)
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

        // 选择报价产品
        public ActionResult SelectQuotePrice(int SalesSystemId)
        {
            var QuotePrice = from m in _db.SP_QuotePrice
                             where m.SalesSystem_Id == SalesSystemId && m.Quoted_Status != -1
                             select m;
            ViewBag.QuotePrice = QuotePrice;
            return PartialView();
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
        [Seller(OperationGroup = 702)]
        public ActionResult SellerListPartial(int? page, string query)
        {
            var seller = getSeller(User.Identity.Name);
            int _page = page ?? 1;
            if (query != "")
            {
                if (seller.Seller_Type != SellerType.ADMINISTARTOR)
                {
                    var Seller = from m in _db.SP_Seller
                                 where m.Seller_Status != -1 && m.Manager_Id == seller.Manager_Id
                                 select m;
                    var sellerList = (from m in Seller
                                      where m.Seller_Name.Contains(query) || m.Seller_Mobile.Contains(query)
                                      orderby m.Id
                                      select m).ToPagedList(_page, 15);
                    return PartialView(sellerList);
                }
                else
                {
                    var Seller = from m in _db.SP_Seller
                                 where m.Seller_Status != -1
                                 select m;
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
            sellerType.Add(new SelectListItem() { Text = "普通", Value = SellerType.SELLER.ToString() });
            sellerType.Add(new SelectListItem() { Text = "产品操作", Value = SellerType.PRODUCTDEPARTMENT.ToString() });
            sellerType.Add(new SelectListItem() { Text = "财务审核", Value = SellerType.FINANCIALDEPARTMENT.ToString() });
            sellerType.Add(new SelectListItem() { Text = "部门主管", Value = SellerType.SELLERADMIN.ToString() });
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
            bool Seller = _db.SP_Seller.Any(m => m.Seller_Mobile == model.Seller_Mobile || m.User_Name == model.User_Name);
            var SellerName = _db.SP_Seller.SingleOrDefault(m => m.User_Name == model.User_Name);
            if (SellerName.Seller_Status == -1)
            {
                if (TryUpdateModel(SellerName))
                {
                    SellerName.Seller_Status = 0;
                    _db.Entry(SellerName).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                }
            }
            else
            {
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
            return Json(new { result = "SUCCESS" });

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
                        if (model.Seller_Type == SellerType.SELLER || model.Seller_Type == SellerType.FINANCIALDEPARTMENT || model.Seller_Type == SellerType.PRODUCTDEPARTMENT)
                        {
                            _db.Entry(seller).State = System.Data.Entity.EntityState.Modified;
                            _db.SaveChanges();
                        }
                        else
                        {
                            seller.Manager_Id = model.Id.ToString();
                            _db.Entry(seller).State = System.Data.Entity.EntityState.Modified;
                            _db.SaveChanges();
                        }
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

        [Seller(OperationGroup = 602)]
        public ActionResult OrderListPartial(int? page, string query, int? clientId, int orderType)
        {
            var seller = getSeller(User.Identity.Name);
            int _page = page ?? 1;
            if (seller.Seller_Type == 0)
            {
                if (clientId != null)
                {
                    if (query != "")
                    {
                        var order = from m in _db.SP_Order
                                    where m.Order_Type == orderType && m.Order_Status != -1
                                    && m.SP_Contact.Client_Id == clientId && m.SP_Contact.SP_Client.Seller_Id == seller.Id
                                    select m;
                        var orderList = (from m in order
                                         where m.Order_Number.Contains(query) || m.SP_Contact.SP_Client.Client_Name.Contains(query)
                                         orderby m.Order_Date descending
                                         select m).ToPagedList(_page, 15);
                        return PartialView(orderList);
                    }
                    else
                    {
                        var orderList = (from m in _db.SP_Order
                                         where m.Order_Type == orderType && m.SP_Contact.Client_Id == clientId
                                         && m.Order_Status != -1 && m.SP_Contact.SP_Client.Seller_Id == seller.Id
                                         orderby m.Order_Date descending
                                         select m).ToPagedList(_page, 15);
                        return PartialView(orderList);
                    }
                }
                else
                {
                    if (query != "")
                    {
                        var order = from m in _db.SP_Order
                                    where m.Order_Type == orderType && m.Order_Status != -1
                                    && m.SP_Contact.SP_Client.Seller_Id == seller.Id
                                    select m;
                        var orderList = (from m in order
                                         where m.Order_Number.Contains(query) || m.SP_Contact.SP_Client.Client_Name.Contains(query)
                                         orderby m.Order_Date descending
                                         select m).ToPagedList(_page, 15);
                        return PartialView(orderList);
                    }
                    else
                    {
                        var orderList = (from m in _db.SP_Order
                                         where m.Order_Type == orderType && m.SP_Contact.SP_Client.Seller_Id == seller.Id
                                         && m.Order_Status != -1
                                         orderby m.Order_Date descending
                                         select m).ToPagedList(_page, 15);
                        return PartialView(orderList);
                    }
                }
            }
            else
            {
                if (clientId != null)
                {
                    if (query != "")
                    {
                        var order = from m in _db.SP_Order
                                    where m.Order_Type == orderType && m.Order_Status != -1 && m.SP_Contact.Client_Id == clientId
                                    select m;
                        var orderList = (from m in order
                                         where m.Order_Number.Contains(query) || m.SP_Contact.SP_Client.Client_Name.Contains(query)
                                         orderby m.Order_Date descending
                                         select m).ToPagedList(_page, 15);
                        return PartialView(orderList);
                    }
                    else
                    {
                        var orderList = (from m in _db.SP_Order
                                         where m.Order_Type == orderType && m.SP_Contact.Client_Id == clientId
                                         && m.Order_Status != -1
                                         orderby m.Order_Date descending
                                         select m).ToPagedList(_page, 15);
                        return PartialView(orderList);
                    }
                }
                else
                {
                    if (query != "")
                    {
                        var order = from m in _db.SP_Order
                                    where m.Order_Type == orderType && m.Order_Status != -1
                                    select m;
                        var orderList = (from m in order
                                         where m.Order_Number.Contains(query) || m.SP_Contact.SP_Client.Client_Name.Contains(query)
                                         orderby m.Order_Date descending
                                         select m).ToPagedList(_page, 15);
                        return PartialView(orderList);
                    }
                    else
                    {
                        var orderList = (from m in _db.SP_Order
                                         where m.Order_Type == orderType && m.Order_Status != -1
                                         orderby m.Order_Date descending
                                         select m).ToPagedList(_page, 15);
                        return PartialView(orderList);
                    }
                }
            }



        }
        [Seller(OperationGroup = 601)]
        public ActionResult AddOrder()
        {
            Random ran = new Random();
            int RandKey = ran.Next(01, 99);
            var ordernumber = DateTime.Now.ToString("yyyyMMddHHmmss");
            ViewBag.ordernumber = ordernumber;
            return View();
        }
        //自动填充经销商地址
        [HttpPost]
        public JsonResult AutoFillAddress(int clientId)
        {
            var clientAddress = _db.SP_Client.SingleOrDefault(m => m.Id == clientId);
            return Json(clientAddress.Client_Address);
        }
        [HttpPost]
        [Seller(OperationGroup = 601)]
        public ActionResult AddOrderPartial(SP_Order model, FormCollection form)
        {
            bool Contact = _db.SP_Contact.Any(m => m.Id == model.Contact_Id);
            ModelState.Remove("Order_Date");
            ModelState.Remove("Order_Remark");
            ModelState.Remove("Signed_Number");
            ModelState.Remove("Cancellation_Fee");
            if (ModelState.IsValid)
            {
                if (Contact)
                {
                    var order = new SP_Order();
                    order.Order_Number = model.Order_Number;
                    order.Order_Date = model.Order_Date;
                    order.Order_Status = 0;
                    order.Contact_Id = model.Contact_Id;
                    order.Order_Address = model.Order_Address;
                    order.Order_Type = -1;
                    order.Order_Remark = model.Order_Remark;
                    order.Signed_Number = model.Signed_Number;
                    order.Cancellation_Fee = model.Cancellation_Fee;
                    _db.SP_Order.Add(order);
                    _db.SaveChanges();
                    var OrderId = _db.SP_Order.SingleOrDefault(m => m.Order_Number == model.Order_Number);
                    return Json(new { result = "SUCCESS", order = OrderId.Id });
                }
                else
                {
                    return Json(new { result = "UNAUTHORIZED" });
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
            ViewBag.Order = Order;
            var contact = _db.SP_Contact.SingleOrDefault(m => m.Id == Order.SP_Contact.Id);
            ViewBag.contact = contact;
            return View(Order);
        }
        [HttpPost]
        [Seller(OperationGroup = 603)]
        public ActionResult EditOrderInfo(SP_Order model)
        {
            bool Order = _db.SP_Order.Any(m => m.Order_Address == model.Order_Address && m.Order_Remark == model.Order_Remark && m.Signed_Number == model.Signed_Number && m.Cancellation_Fee == model.Cancellation_Fee);
            ModelState.Remove("Order_Date");
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
        // 审批
        [HttpPost]
        [Seller(OperationGroup = 604)]
        public ActionResult ConfirmOrder(int orderId)
        {
            var seller = getSeller(User.Identity.Name);
            var order = _db.SP_Order.SingleOrDefault(m => m.Id == orderId);
            if (order.Order_Type != -1)
            {
                return Json(new { result = "WARNING" });
            }
            else
            {
                if (seller.Seller_Type == 0)
                {
                    return Json(new { result = "FALL" });
                }
                else
                {
                    if (TryUpdateModel(order))
                    {
                        order.Order_Type = 0;
                        _db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                        _db.SaveChanges();
                    }
                    return Json(new { result = "SUCCESS" });
                }
            }
        }

        public ActionResult OrderPriceList()
        {
            return PartialView();
        }

        public ActionResult OrderPriceListPartial(int orderId)
        {
            var order = from m in _db.SP_OrderPrice
                        where m.Order_Id == orderId && m.OrderPrice_Status != -1
                        select m;
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
                            CartonCount = g.Sum(m => m.Order_Count / m.SP_Product.Carton_Spec),
                            SumPrice = g.Sum(m => m.Order_Price),
                            SumDiscount = g.Sum(m => m.OrderPrice_Discount),
                        };
            int cartonCount = 0;
            decimal sumPrice = 0;
            decimal sumDiscount = 0;
            foreach (var price in Price)
            {
                cartonCount += price.CartonCount;
                var Sumprice = price.SumCount * price.SumPrice;
                sumPrice += Sumprice;
                sumDiscount += price.SumDiscount;
            }
            ViewBag.Count = cartonCount;
            ViewBag.Price = sumPrice;
            ViewBag.Discount = sumDiscount;
            return PartialView(order);
        }

        public ActionResult AddOrderPricePartial(int orderId)
        {
            var order = _db.SP_Order.SingleOrDefault(m => m.Id == orderId);
            ViewBag.Order = order;
            return PartialView();
        }
        [HttpPost]
        public JsonResult CheckProductName()
        {
            var productlist = (from m in _db.SP_Product
                              where m.Product_Status != -1
                               select new { id = m.Id, name = m.Item_Code + "-" + m.Item_Name + "-" + m.Product_Standard }).ToList();
            return Json(new { data = productlist });
        }
        [HttpPost]
        public ActionResult AddOrderPricePartial(SP_OrderPrice model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                var productlist = from m in _db.SP_Product
                                  where m.Product_Status != -1
                                  select m;
                foreach (var product in productlist)
                {
                    bool OrderPrice = _db.SP_OrderPrice.Any(m => m.Product_Id == model.Product_Id && m.Order_Id == model.Order_Id && m.OrderPrice_Status != -1);
                    if (OrderPrice)
                    {
                        return Json(new { result = "UNAUTHORIZED" });
                    }
                    else
                    {
                        int order = 0;
                        decimal price = 0;
                        string remark = "";
                        decimal discount = 0;
                        if (form["order_" + product.Id] != "")
                        order = Convert.ToInt32(form["order_" + product.Id]);
                        price = Convert.ToDecimal(form["price_" + product.Id]);
                        remark = Convert.ToString(form["remark_" + product.Id]);
                        discount = Convert.ToDecimal(form["discount_" + product.Id]);
                        var orderType = _db.SP_Order.SingleOrDefault(m => m.Id == model.Order_Id);
                        if (orderType.Order_Type != 0)
                        {
                            if (order == 0)
                            {
                            }
                            else
                            {
                                var orderprice = new SP_OrderPrice();
                                {
                                    orderprice.Order_Count = order;
                                    orderprice.Order_Price = price;
                                    orderprice.OrderPrice_Discount = discount;
                                    orderprice.Product_Id = product.Id;
                                    orderprice.OrderPrice_Status = 0;
                                    orderprice.Order_Id = model.Order_Id;
                                    orderprice.OrderPrice_Remark = remark;
                                };
                                _db.SP_OrderPrice.Add(orderprice);
                            }
                        }
                        else
                        {
                            return Json(new { result = "WARNING" });
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

        public ActionResult EditOrderPriceInfo(int orderPriceId)
        {
            var OrderPrice = _db.SP_OrderPrice.SingleOrDefault(m => m.Id == orderPriceId);
            ViewBag.OrderPrice = OrderPrice;
            return PartialView(OrderPrice);
        }
        [HttpPost]
        [Seller(OperationGroup = 903)]
        public ActionResult EditOrderPriceInfo(SP_OrderPrice model)
        {
            bool Order = _db.SP_OrderPrice.Any(m =>m.Product_Id == model.Product_Id && m.Order_Count == model.Order_Count && m.OrderPrice_Remark == model.OrderPrice_Remark && m.Order_Price == model.Order_Price && m.OrderPrice_Discount == model.OrderPrice_Discount && m.OrderPrice_Status != -1);
            if (ModelState.IsValid)
            {
                if (Order)
                {
                    return Json(new { result = "UNAUTHORIZED" });
                }
                else
                {
                    var order = _db.SP_Order.SingleOrDefault(m => m.Id == model.Order_Id);
                    if (order.Order_Type != 0)
                    {
                        SP_OrderPrice orderPrice = new SP_OrderPrice();
                        if (TryUpdateModel(orderPrice))
                        {
                            _db.Entry(orderPrice).State = System.Data.Entity.EntityState.Modified;
                            _db.SaveChanges();
                            return Json(new { result = "SUCCESS" });
                        }
                    }
                    else
                    {
                        return Json(new { result = "WARNING" });
                    }
                }
            }
            return Json(new { result = "FAIL" });
        }
        [HttpPost]
        [Seller(OperationGroup = 903)]
        public ActionResult DeleteOrderPrice(int orderPriceId)
        {
            var OrderPrice = _db.SP_OrderPrice.AsNoTracking().SingleOrDefault(m => m.Id == orderPriceId);
            var order = _db.SP_Order.SingleOrDefault(m => m.Id == OrderPrice.Order_Id);
            if (order.Order_Type != 0)
            {
                OrderPrice.OrderPrice_Status = -1;
                _db.Entry(OrderPrice).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
            }
            else
            {
                return Json(new { result = "WARNING" });
            }
            return Json(new { result = "SUCCESS" });

        }

        // 搜索
        [HttpPost]
        public JsonResult QueryAddress(string query)
        {
            var seller = getSeller(User.Identity.Name);
            var salesAddress = from m in _db.SP_SalesSystem
                               where m.System_Address.Contains(query) || m.System_Name.Contains(query) && m.SP_Client.Seller_Id == seller.Id 
                               select new { System_Address = m.System_Address, Address_Show = m.System_Name + "-" + m.System_Address };
            return Json(salesAddress);
        }
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
                             where m.Client_Status != -1 && m.Client_Name.Contains(query)
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
                             && m.Contact_Name.Contains(query)
                             select new { Id = m.Id, Contact_Name = m.SP_Client.Client_Name + "-" + m.Contact_Name };
                return Json(client);
            }
            else
            {
                var client = from m in _db.SP_Contact
                             where m.Contact_Status != -1 && m.Contact_Name.Contains(query)
                             select new { Id = m.Id, Contact_Name = m.SP_Client.Client_Name + "-" + m.Contact_Name };
                return Json(client);
            }
        }
        [HttpPost]
        public JsonResult QueryProduct(string query)
        {
            var productlist = from m in _db.SP_Product
                              where m.Product_Status != -1
                              select m;
            var product = from m in productlist
                          where m.Item_Name.Contains(query) || m.Item_Code.Contains(query)
                          select new { Id = m.Id, ProductName = m.Item_Code + "-" +m.Item_Name + "-" + m.Product_Standard };
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
        [HttpPost]
        public JsonResult QueryContactPhone(string query, int clientId)
        {
            var seller = getSeller(User.Identity.Name);
            if (seller.Seller_Type == 0)
            {
                var client = from m in _db.SP_Contact
                             where m.Contact_Status != -1 && m.SP_Client.Seller_Id == seller.Id && m.Client_Id == clientId
                             && m.Contact_Name.Contains(query)
                             select new { Id = m.Id, Contact_Name = m.Contact_Name + "  " + m.Contact_Mobile };
                return Json(client);
            }
            else
            {
                var client = from m in _db.SP_Contact
                             where m.Contact_Status != -1 && m.SP_Client.SP_Seller.Seller_Type <= seller.Seller_Type && m.Client_Id == clientId
                             && m.Contact_Name.Contains(query)
                             select new { Id = m.Id, Contact_Name = m.Contact_Name + " " + m.Contact_Mobile };
                return Json(client);
            }
        }

        public JsonResult AllPriceAjax(int? productId, int clientId)
        {
            var _productId = productId ?? null;
            if (_productId == null)  {
                return Json(new { result = "FALL" });
            }else
            {
                bool Price = _db.SP_QuotePrice.Any(m => m.Product_Id == _productId && m.Quoted_Status != -1 && m.SP_SalesSystem.Client_Id == clientId);
                if (Price)
                {
                    var product = from m in _db.SP_QuotePrice
                                  where m.Quoted_Status != -1 && m.Product_Id == _productId && m.SP_SalesSystem.Client_Id == clientId
                                  select new { Id = m.Product_Id, Price = m.Quote_Price };
                    return Json(new { result = "SUCCESS", data = product }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var product = from m in _db.SP_Product
                                  where m.Product_Status != -1 && m.Id == _productId
                                  select new { Id = m.Id, Price = m.Purchase_Price };
                    return Json(new { result = "SUCCESS", data = product }, JsonRequestBehavior.AllowGet);
                }
            }
            
        }
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
                PdfWriter.GetInstance(document, new FileStream(@"D:\Create.pdf", FileMode.Append));
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
                cell = new PdfPCell(new Phrase(orderNum.Order_Number.ToString(), font1));
                cell.Colspan = 4;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("客户名称:", font1));
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(orderNum.SP_Contact.SP_Client.Client_Name.ToString(), font1));
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
                cell = new PdfPCell(new Phrase(orderNum.SP_Contact.SP_Client.SP_Seller.Seller_Name.ToString(), font1));
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
                                SumCount = g.Sum(m => m.Order_Count),
                                SumPrice = g.Sum(m => m.Order_Price)
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
                cell = new PdfPCell(new Paragraph(Math.Round(SumPrice, 2).ToString(), font1));
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
                cell.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER | Rectangle.TOP_BORDER;
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
                cell.Border = Rectangle.RIGHT_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER;
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

                cell = new PdfPCell(new Paragraph("[          ]年[      ]月[      ]日", font2));
                cell.Colspan = 8;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                // 
                cell = new PdfPCell(new Paragraph("产品部签署:", font2));
                cell.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;
                cell.Colspan = 10;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("[确认已经完成发货]", font2));
                cell.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;
                cell.Colspan = 10;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("[        ]年[    ]月[    ]日", font2));
                cell.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER | Rectangle.BOTTOM_BORDER;
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
            return Json(new { result = "SUCCESS" });
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
                    AliOSSUtilities util = new AliOSSUtilities();
                    util.PutWebObject(files[0].InputStream, "Content/" + _filename);
                    //msg = "成功! 文件大小为:" + files[0].ContentLength;
                    System.Drawing.Image image = System.Drawing.Image.FromStream(files[0].InputStream);
                    int iWidth = image.Width;
                    int iHeight = image.Height;
                    if (iWidth < 330 || iHeight < 330)
                    {
                        error = "图片过小,建议最小尺寸为330*330";
                    } else if (iWidth > 800 || iHeight > 800) 
                    {
                        error = "图片过大,建议最大尺寸为800*800";
                    }
                    else
                    {
                        imgurl = "http://cdn.shouquanzhai.cn/Content/" + _filename;
                        string res = "{ error:'" + error + "',imgurl:'" + imgurl + "'}";
                        return Content(res);
                    }
                    //string fileSize = GetFileSize(files[0].ContentLength);
                }
                else
                {
                    error = "文件错误";
                }
            }
            string err_res = "{ error:'" + error  + "',imgurl:''}";
            return Content(err_res);

        }
        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        //private string GetFileSize(long bytes)
        //{
        //    long kblength = 1024;
        //    long mbLength = 1024 * 1024;
        //    if (bytes < kblength)
        //        return bytes.ToString() + "B";
        //    if (bytes < mbLength)
        //        return decimal.Round(decimal.Divide(bytes, kblength), 2).ToString() + "KB";
        //    else
        //        return decimal.Round(decimal.Divide(bytes, mbLength), 2).ToString() + "MB";
        //}
        // 报价单导出
        [HttpPost]
        public ActionResult getQuotePrice(FormCollection form, int SalesSystemId, string productId)
        {
            HSSFWorkbook book = new HSSFWorkbook();
            ISheet sheet = book.CreateSheet("报价单");
            // 写标题
            IRow row = sheet.CreateRow(0);
            int cell_pos = 0;
            row.CreateCell(cell_pos).SetCellValue("商品编码");
            row.CreateCell(++cell_pos).SetCellValue("产品名称");
            row.CreateCell(++cell_pos).SetCellValue("箱规");
            row.CreateCell(++cell_pos).SetCellValue("单价");
            int row_pos = 1;
            string _productId = productId;
            string[] sArray = _productId.Split(',');
            string num = "";
            foreach (string i in sArray)
            {
                num = i;
                int _ProductId = Convert.ToInt32(num);
                var price_list = _db.SP_QuotePrice.SingleOrDefault(m => m.SalesSystem_Id == SalesSystemId && m.Quoted_Status != -1 && m.Product_Id == _ProductId);
                IRow single_row = sheet.CreateRow(row_pos);
                cell_pos = 0;
                single_row.CreateCell(cell_pos).SetCellValue(price_list.SP_Product.Item_Code);
                single_row.CreateCell(++cell_pos).SetCellValue(price_list.SP_Product.Item_Name);
                single_row.CreateCell(++cell_pos).SetCellValue(price_list.SP_Product.Carton_Spec);
                single_row.CreateCell(++cell_pos).SetCellValue((double)(price_list.Quote_Price));
                row_pos++;
            };
            MemoryStream _stream = new MemoryStream();
            book.Write(_stream);
            _stream.Flush();
            _stream.Seek(0, SeekOrigin.Begin);
            return File(_stream, "application/vnd.ms-excel", DateTime.Now.ToString("yyyyMMddHHmmss") + "报价单.xls");
        }

        public ICellStyle ExcelCellStyle(HSSFWorkbook book,string styleName)
        {
            if (styleName == "标题")
            {
                ICellStyle cellStyle = book.CreateCellStyle();//标题样式
                cellStyle.BorderLeft = BorderStyle.Thin;
                cellStyle.BorderBottom = BorderStyle.Thin;
                cellStyle.BorderRight = BorderStyle.Thin;
                cellStyle.BorderTop = BorderStyle.Thin;
                cellStyle.VerticalAlignment = VerticalAlignment.Center;//垂直对齐
                cellStyle.Alignment = HorizontalAlignment.Center;//水平对齐
                IFont titleFont = book.CreateFont(); //创建一个字体样式对象
                titleFont.FontName = "宋体"; //和excel里面的字体对应
                titleFont.FontHeightInPoints = 16;//字体大小
                titleFont.Boldweight = (short)FontBoldWeight.Bold;
                cellStyle.SetFont(titleFont);
                return cellStyle;
            }
            else if (styleName == "居中正文")
            {
                ICellStyle cellStyle = book.CreateCellStyle();//正文样式（居中）
                cellStyle.BorderBottom = BorderStyle.Thin;
                cellStyle.BorderLeft = BorderStyle.Thin;
                cellStyle.BorderRight = BorderStyle.Thin;
                cellStyle.BorderTop = BorderStyle.Thin;
                cellStyle.VerticalAlignment = VerticalAlignment.Center;//垂直对齐
                cellStyle.Alignment = HorizontalAlignment.Center;
                IFont textFont1 = book.CreateFont(); //创建一个字体样式对象
                textFont1.FontName = "宋体"; //和excel里面的字体对应
                textFont1.FontHeightInPoints = 12;//字体大小
                cellStyle.SetFont(textFont1);
                return cellStyle;
            }
            else
            {
                ICellStyle cellStyle = book.CreateCellStyle();//正文样式（居左）
                cellStyle.BorderBottom = BorderStyle.Thin;
                cellStyle.BorderLeft = BorderStyle.Thin;
                cellStyle.BorderRight = BorderStyle.Thin;
                cellStyle.BorderTop = BorderStyle.Thin;
                cellStyle.VerticalAlignment = VerticalAlignment.Center;//垂直对齐
                cellStyle.Alignment = HorizontalAlignment.Left;
                IFont textFont1 = book.CreateFont(); //创建一个字体样式对象
                textFont1.FontName = "宋体"; //和excel里面的字体对应
                textFont1.FontHeightInPoints = 12;//字体大小
                cellStyle.SetFont(textFont1);
                return cellStyle;
            }
        }
        //生成订货通知单
        [HttpPost]
        public ActionResult CreatOrderExcel(int orderId)
        {
            HSSFWorkbook book = new HSSFWorkbook();
            ISheet sheet = book.CreateSheet("报价单");
            var titleStyle = ExcelCellStyle(book, "标题");
            var textStyle1 = ExcelCellStyle(book, "居中正文");
            var textStyle2 = ExcelCellStyle(book, "居左正文");
            //合并单元格
            for (int i = 0; i < 21; i++)
            {
                int j = i + 1;
                int[] a = { 0, 0, 2, 1, 3, 1, 4, 1, 5, 1, 6, 1, 2, 4, 3, 4, 4, 4, 5, 4, 6, 4 };
                int[] b = { 1, 8, 2, 2, 3, 2, 4, 2, 5, 2, 6, 2, 2, 8, 3, 8, 4, 8, 5, 8, 6, 8 };
                if (i % 2 == 0)
                {
                    sheet.AddMergedRegion(new CellRangeAddress(a[i], b[i], a[j], b[j]));
                }
            }
            var orderInfo = _db.SP_Order.SingleOrDefault(m => m.Id == orderId);
            if (orderInfo.Order_Type != 0)
            {
                return Json(new { result = "FALL" });
            }
            // 写标题
            IRow row0 = sheet.CreateRow(0);
            row0.Height = 40 * 20;
            for (int i = 0; i < 7; i++)
            {
                sheet.SetColumnWidth(i, 20 * 256);
                //sheet.SetDefaultColumnStyle(i, borderstyle);
            }
            IRow row1 = sheet.CreateRow(1);
            row1.Height = 40 * 20;
            for (int i = 0; i < 9; i++)
            {
                var r0c = row0.CreateCell(i);
                r0c.CellStyle = textStyle1;
                var r1c = row1.CreateCell(i);
                r1c.CellStyle = textStyle1;
            }
            int cell_pos = 0;
            var eTitle = row0.CreateCell(cell_pos);
            eTitle.SetCellValue("寿全斋订货通知单");
            eTitle.CellStyle = titleStyle;
            var r0c6 = row0.CreateCell(6);
            var r1c6 = row1.CreateCell(6);
            r0c6.CellStyle = titleStyle;
            r1c6.CellStyle = titleStyle;
            IRow row2 = sheet.CreateRow(2);//第三行
            row2.Height = 35 * 20;
            var r2c0 = row2.CreateCell(cell_pos);
            r2c0.SetCellValue("购货单位：");
            r2c0.CellStyle = textStyle2;
            var r2c3 = row2.CreateCell(3);
            r2c3.SetCellValue("订单编号：");
            r2c3.CellStyle = textStyle2;
            IRow row3 = sheet.CreateRow(3);//第四行
            row3.Height = 35 * 20;
            var r3c0 = row3.CreateCell(cell_pos);
            r3c0.SetCellValue("联系人及电话：");
            r3c0.CellStyle = textStyle2;
            var r3c3 = row3.CreateCell(3);
            r3c3.SetCellValue("订货日期：");
            r3c3.CellStyle = textStyle2;
            var r3c6 = row3.CreateCell(6);
            r3c6.CellStyle = textStyle1;
            IRow row4 = sheet.CreateRow(4);//第五行
            row4.Height = 35 * 20;
            var r4c6 = row4.CreateCell(6);
            r4c6.CellStyle = textStyle1;
            var r4c0 = row4.CreateCell(cell_pos);
            r4c0.SetCellValue("签呈编号：");
            r4c0.CellStyle = textStyle2;
            var r4c3 = row4.CreateCell(3);
            r4c3.SetCellValue("收货地址：");
            r4c3.CellStyle = textStyle2;
            IRow row5 = sheet.CreateRow(5);//第六行
            row5.Height = 35 * 20;
            var r5c6 = row5.CreateCell(6);
            r5c6.CellStyle = textStyle1;
            var r5c0 = row5.CreateCell(cell_pos);
            r5c0.SetCellValue("核销费用：");
            r5c0.CellStyle = textStyle2;
            IRow row6 = sheet.CreateRow(6);//第七行
            row6.Height = 35 * 20;
            var r6c6 = row6.CreateCell(6);
            r6c6.CellStyle = textStyle1;
            var r6c0 = row6.CreateCell(cell_pos);
            r6c0.SetCellValue("备注：");
            r6c0.CellStyle = textStyle2;
            IRow row7 = sheet.CreateRow(7);//第八行（数据区）
            row7.Height = 30 * 20;
            var r7c0 = row7.CreateCell(cell_pos);
            r7c0.SetCellValue("序号");
            var r7c1 = row7.CreateCell(++cell_pos);
            r7c1.SetCellValue("产品代码");
            var r7c2 = row7.CreateCell(++cell_pos);
            r7c2.SetCellValue("品名");
            var r7c3 = row7.CreateCell(++cell_pos);
            r7c3.SetCellValue("规格");
            var r7c4 = row7.CreateCell(++cell_pos);
            r7c4.SetCellValue("订货数量");
            var r7c5 = row7.CreateCell(++cell_pos);
            r7c5.SetCellValue("箱数");
            var r7c6 = row7.CreateCell(++cell_pos);
            r7c6.SetCellValue("单价");
            var r7c7 = row7.CreateCell(++cell_pos);
            r7c7.SetCellValue("金额");
            var r7c8 = row7.CreateCell(++cell_pos);
            r7c8.SetCellValue("备注");
            r7c0.CellStyle = textStyle1;
            r7c1.CellStyle = textStyle1;
            r7c2.CellStyle = textStyle1;
            r7c3.CellStyle = textStyle1;
            r7c4.CellStyle = textStyle1;
            r7c5.CellStyle = textStyle1;
            r7c6.CellStyle = textStyle1;
            r7c7.CellStyle = textStyle1;
            r7c8.CellStyle = textStyle1;
            var priceData = from m in _db.SP_OrderPrice
                            where m.Order_Id == orderId && m.OrderPrice_Status != -1
                            select m;
            var cell_data = 7;
            var order_num = 0;
            foreach (var data in priceData)
            {
                IRow rowData = sheet.CreateRow(++cell_data);
                rowData.Height = 30 * 20;
                var rd0 = rowData.CreateCell(0);
                rd0.SetCellValue(++order_num);
                var rd1 = rowData.CreateCell(1);
                rd1.SetCellValue(data.SP_Product.Item_Code);
                var rd2 = rowData.CreateCell(2);
                rd2.SetCellValue(data.SP_Product.Item_Name);
                var rd3 = rowData.CreateCell(3);
                rd3.SetCellValue(data.SP_Product.Carton_Spec);
                var rd4 = rowData.CreateCell(4);
                rd4.SetCellValue(data.Order_Count);
                var rd5 = rowData.CreateCell(5);
                rd5.SetCellValue(data.Order_Count / data.SP_Product.Carton_Spec);
                var rd6 = rowData.CreateCell(6);
                rd6.SetCellValue(data.SP_Product.Purchase_Price.ToString());
                var rd7 = rowData.CreateCell(7);
                rd7.SetCellValue((data.Order_Count * data.SP_Product.Purchase_Price).ToString());
                var rd8 = rowData.CreateCell(8);
                rd8.SetCellValue(data.OrderPrice_Remark);
                rd0.CellStyle = textStyle1;
                rd1.CellStyle = textStyle1;
                rd2.CellStyle = textStyle1;
                rd3.CellStyle = textStyle1;
                rd4.CellStyle = textStyle1;
                rd5.CellStyle = textStyle1;
                rd6.CellStyle = textStyle1;
                rd7.CellStyle = textStyle1;
                rd8.CellStyle = textStyle1;
            }
            for (int i = 1; i < 9; i++)//3-7行样式
            {
                if (i != 3)
                {
                    var r2c = row2.CreateCell(i);
                    r2c.CellStyle = textStyle1;
                    var r3c = row3.CreateCell(i);
                    r3c.CellStyle = textStyle1;
                    var r4c = row4.CreateCell(i);
                    r4c.CellStyle = textStyle1;
                }
                var r5c = row5.CreateCell(i);
                r5c.CellStyle = textStyle1;
                var r6c = row6.CreateCell(i);
                r6c.CellStyle = textStyle1;
            }
            var rest = priceData.Count() + 8;
            var restEnd = priceData.Count() + 8;
            for (; rest - restEnd <= 2; rest++)
            {
                IRow rowRest = sheet.CreateRow(rest);
                rowRest.Height = 30 * 20;
                for (int i = 0; i < 9; i++)
                {
                    var rcRest = rowRest.CreateCell(i);//数据区后追三行
                    rcRest.CellStyle = textStyle1;
                }
            }
            IRow rowAdd = sheet.CreateRow(rest);//合计数据区
            rowAdd.Height = 30 * 20;
            var rcAdd = rowAdd.CreateCell(0);
            rcAdd.SetCellValue("合计");
            rcAdd.CellStyle = textStyle1;
            for (int i = 1; i < 9; i++)
            {
                var rcDataAdd = rowAdd.CreateCell(i);
                rcDataAdd.CellStyle = textStyle1;
            }
            var Price = from m in _db.SP_OrderPrice
                        where m.OrderPrice_Status != -1 && m.Order_Id == orderId
                        group m by m.Id into g
                        select new OrderPriceSum
                        {
                            SumCount = g.Sum(m => m.Order_Count),
                            CartonCount = g.Sum(m => m.Order_Count / m.SP_Product.Carton_Spec),
                            SumPrice = g.Sum(m => m.Order_Price)
                        };
            int cartonCount = 0;
            int orderCount = 0;
            decimal sumPrice = 0;
            foreach (var price in Price)
            {
                cartonCount += price.CartonCount;
                orderCount += price.SumCount;
                var Sumprice = price.SumCount * price.SumPrice;
                sumPrice += Sumprice;
            }
            var row_orderCount = rowAdd.CreateCell(4);
            row_orderCount.SetCellValue(orderCount);
            row_orderCount.CellStyle = textStyle1;
            var row_sumCount = rowAdd.CreateCell(5);
            row_sumCount.SetCellValue(cartonCount);
            row_sumCount.CellStyle = textStyle1;
            var row_sumPrice = rowAdd.CreateCell(7);
            row_sumPrice.SetCellValue(sumPrice.ToString());
            row_sumPrice.CellStyle = textStyle1;
            //未知区
            //填充订单数据
            var r2c1 = row2.CreateCell(1);
            r2c1.SetCellValue(orderInfo.SP_Contact.SP_Client.Client_Name);
            r2c1.CellStyle = textStyle1;
            var r2c4 = row2.CreateCell(4);
            r2c4.SetCellValue(orderInfo.Order_Number);
            r2c4.CellStyle = textStyle1;
            var r3c1 = row3.CreateCell(1);
            r3c1.SetCellValue(orderInfo.SP_Contact.Contact_Name + " " + orderInfo.SP_Contact.Contact_Mobile);
            r3c1.CellStyle = textStyle1;
            var r3c4 = row3.CreateCell(4);
            r3c4.SetCellValue(orderInfo.Order_Date.ToString("yyyy-MM-dd"));
            r3c4.CellStyle = textStyle1;
            var r4c4 = row4.CreateCell(4);
            r4c4.SetCellValue(orderInfo.Order_Address);
            r4c4.CellStyle = textStyle1;
            MemoryStream _stream = new MemoryStream();
            book.Write(_stream);
            _stream.Flush();
            _stream.Seek(0, SeekOrigin.Begin);
            return File(_stream, "application/vnd.ms-excel", DateTime.Now.ToString("yyyyMMddHHmmss") + "订货通知单.xls");
        }

        //CRM
        public static String buildQueryStr(Dictionary<String, String> dicList)
        {
            String postStr = "";

            foreach (var item in dicList)
            {
                postStr += item.Key + "=" + HttpUtility.UrlEncode(item.Value, Encoding.UTF8) + "&";
            }
            postStr = postStr.Substring(0, postStr.LastIndexOf('&'));
            return postStr;
        }
        
        public ActionResult GetUserToken()
        {
            var try_times = 0;
            var token_time = crm_db.CRM_User_Token.SingleOrDefault(m => m.Id == 1);
            TimeSpan ts = DateTime.Now - token_time.download_at;
            int days = ts.Days;
            if (days >= 1)
            {
                string url = "https://api.ikcrm.com/api/v2/auth/login";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "post";
                request.ContentType = "application/x-www-form-urlencoded";
                //request.ContentType = "application/json";

                ///添加参数  
                Dictionary<String, String> dicList = new Dictionary<String, String>();
                dicList.Add("login", UserInfo.login);
                dicList.Add("password", UserInfo.password);
                dicList.Add("device", UserInfo.device);
                String postStr = buildQueryStr(dicList);
                byte[] data = Encoding.UTF8.GetBytes(postStr);

                request.ContentLength = data.Length;

                Stream myRequestStream = request.GetRequestStream();
                myRequestStream.Write(data, 0, data.Length);
                myRequestStream.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader myStreamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                var retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                result_Data resultdata = JsonConvert.DeserializeObject<result_Data>(retString);
                if (resultdata.code != "0")
                {
                    token_time.user_token = resultdata.data.user_token;
                    token_time.download_at = DateTime.Now;
                    crm_db.Entry(token_time).State = System.Data.Entity.EntityState.Modified;
                    crm_db.SaveChanges();
                }
                else
                {
                    CRM_ExceptionLogs logs = new CRM_ExceptionLogs();
                    Thread.Sleep(1000);
                    try_times++;
                    if (try_times >= 10) {
                        logs.type = "user_token";
                        logs.exception = "获取失败";
                        logs.exception_at = DateTime.Now;
                        crm_db.CRM_ExceptionLogs.Add(logs);
                        crm_db.SaveChanges();
                        return Content("failed");
                    }
                    return GetUserToken();
                }

            }
            return Json(new { result = "SUCCESS" }, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult GetCrmInfo()
        {
            var user_token = crm_db.CRM_User_Token.SingleOrDefault(m => m.Id == 1);
            string url = "https://api.ikcrm.com/api/v2/contracts/?user_token=" + user_token.user_token + "&device=dingtalk&version_code=9.8.0";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "get";
            request.ContentType = "application/x-www-form-urlencoded";
            
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader myStreamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            var retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            CRM_Contract_ReturnData r = JsonConvert.DeserializeObject<CRM_Contract_ReturnData>(retString);
            
            for(int i = 0; i< r.data.contracts.Count();i++)
            {
                var contractId = r.data.contracts[i].id;
                var check_data = crm_db.CRM_Contract.SingleOrDefault(m => m.contract_id == contractId);
                if (check_data == null)
                {
                    check_data = new CRM_Contract();
                    check_data.contract_id = r.data.contracts[i].id;
                    check_data.user_id = r.data.contracts[i].user_id;
                    check_data.user_name = r.data.contracts[i].user_name;
                    check_data.customer_id = r.data.contracts[i].customer_id;
                    check_data.customer_name = r.data.contracts[i].customer_name;
                    check_data.title = r.data.contracts[i].title;
                    check_data.total_amount = r.data.contracts[i].total_amount;
                    check_data.status = r.data.contracts[i].status;
                    check_data.updated_at = r.data.contracts[i].updated_at;
                    crm_db.CRM_Contract.Add(check_data);
                }
                else
                {
                    // update
                    check_data.contract_id = r.data.contracts[i].id;
                    check_data.user_id = r.data.contracts[i].user_id;
                    check_data.user_name = r.data.contracts[i].user_name;
                    check_data.customer_id = r.data.contracts[i].customer_id;
                    check_data.customer_name = r.data.contracts[i].customer_name;
                    check_data.title = r.data.contracts[i].title;
                    check_data.total_amount = r.data.contracts[i].total_amount;
                    check_data.status = r.data.contracts[i].status;
                    check_data.updated_at = r.data.contracts[i].updated_at;
                    crm_db.Entry(check_data).State = System.Data.Entity.EntityState.Modified;
                }

            }
            crm_db.SaveChanges();
            return Json(new { result = "SUCCESS" , data = r.code }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCrmDetailInfo()
        {
            var user_token = crm_db.CRM_User_Token.SingleOrDefault(m => m.Id == 1);
            var contracts = from m in crm_db.CRM_Contract
                            select m;
            foreach (var C_id in contracts)
            {
                string url = "https://api.ikcrm.com/api/v2/contracts/" + C_id.contract_id + "?user_token=" + user_token.user_token + "&device=dingtalk&version_code=9.8.0";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "get";
                request.ContentType = "application/x-www-form-urlencoded";
                //request.ContentType = "application/json";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader myStreamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                var retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                CRM_ContractDetail_ReturnData r = JsonConvert.DeserializeObject<CRM_ContractDetail_ReturnData>(retString);
                var contact = crm_db.CRM_ContractDetail.SingleOrDefault(m => m.contract_id == C_id.contract_id);
                if (contact == null)
                {
                    contact = new CRM_ContractDetail();
                    contact.contract_id = C_id.id;
                    contact.customer_address = r.data.customer.address.full_address;
                    contact.customer_tel = r.data.customer.address.tel;
                    contact.contacts_address = r.data.customer.contacts[0].address.full_address;
                    contact.contacts_tel = r.data.customer.contacts[0].address.tel;
                    crm_db.CRM_ContractDetail.Add(contact);
                }
            }
            crm_db.SaveChanges();
            return Json(new { result = "SUCCESS" }, JsonRequestBehavior.AllowGet);
            //return Json(new { result = "SUCCESS", data = retString }, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public ActionResult SaveCRMInfo()
        //{
        //    var sr = new StreamReader(Request.InputStream);
        //    var stream = sr.ReadToEnd();
        //    JavaScriptSerializer js = new JavaScriptSerializer();
        //    var list = js.Deserialize<List<CRM_Contract>>(stream);
        //    if (list.Count() != 0)
        //    {
        //        foreach (var item in list)
        //        {
        //            var check_data = crm_db.CRM_Contract.AsNoTracking().SingleOrDefault(m => m.contract_id == item.contract_id);
        //            if (check_data == null)
        //            {
        //                // new
        //                check_data = new CRM_Contract();
        //                check_data.user_id = item.user_id;
        //                check_data.user_name = item.user_name;
        //                check_data.contract_id = item.contract_id;
        //                check_data.contract_price = item.contract_price;
        //                check_data.contract_title = item.contract_title;
        //                check_data.customer_id = item.customer_id;
        //                check_data.sign_date = item.updated_at;
        //                check_data.updated_at = item.updated_at;
        //                check_data.status = item.status;
        //                check_data.customer_name = item.customer_name;
        //                crm_db.CRM_Contract.Add(check_data);
        //            }
        //            else
        //            {
        //                // update
        //                check_data.user_id = item.user_id;
        //                check_data.user_name = item.user_name;
        //                check_data.contract_id = item.contract_id;
        //                check_data.contract_price = item.contract_price;
        //                check_data.contract_title = item.contract_title;
        //                check_data.customer_id = item.customer_id;
        //                check_data.sign_date = item.updated_at;
        //                check_data.updated_at = item.updated_at;
        //                check_data.status = item.status;
        //                check_data.customer_name = item.customer_name;
        //                crm_db.Entry(check_data).State = System.Data.Entity.EntityState.Modified;
        //            }
        //        }
        //    }
        //    crm_db.SaveChanges();
        //    return Json(new { result = "success" });
        //}

        public ActionResult UpdateCRM(int[] c_id)
        {
            var user_token = crm_db.CRM_User_Token.SingleOrDefault(m => m.Id == 1);
            var _Cid = c_id;
            foreach (var i in _Cid)
            {
                var check_data = crm_db.CRM_Contract.SingleOrDefault(m => m.id == i);
                string url = "https://api.ikcrm.com/api/v2/contracts/" + check_data.contract_id + "?user_token=" + user_token.user_token + "&device=dingtalk&version_code=9.8.0";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "PUT";
                request.ContentType = "application/x-www-form-urlencoded";
                //request.ContentType = "application/json";

                // 添加参数
                Dictionary<String, String> dicList = new Dictionary<String, String>();
                //只修改了订单状态和备注
                dicList.Add("contract[status]", UserInfo.status_undelivered.ToString());
                //dicList.Add("contract[special_terms]", "快递单号" + special_terms);
                String postStr = buildQueryStr(dicList);
                byte[] b_data = Encoding.UTF8.GetBytes(postStr);
                request.ContentLength = b_data.Length;

                Stream myRequestStream = request.GetRequestStream();
                myRequestStream.Write(b_data, 0, b_data.Length);
                myRequestStream.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader myStreamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                var retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                check_data.status = UserInfo.status_undelivered.ToString();
            }
            crm_db.SaveChanges();
            return Content("succ");

        }

        public ActionResult CRM_show()
        {
            return View();
        }

        public ActionResult CRM_undeliveredPartical(string status)
        {
            var undeliveredData = from m in crm_db.CRM_Contract
                                  where m.status == status
                                  select m;
            return PartialView(undeliveredData);
        }


        private static string AppId = "130412";
        private static string AppSecret = "26d2e926f42a4f2181dd7d1b7f7d55c0";
        private static string SessionKey = "8a503b3d9d0d4119be2868cc69a8ef5a";
        private static string API_Url = "http://v2.api.guanyierp.com/rest/erp_open";

        private string sign(string json, string secret)
        {
            StringBuilder enValue = new StringBuilder();
            //前后加上secret
            enValue.Append(secret);
            enValue.Append(json);
            enValue.Append(secret);
            //使用MD5加密(32位大写)
            return CommonUtilities.encrypt_MD5(enValue.ToString()).ToUpper();
        }

        public ActionResult getERPORDERS()
        {
            var platform_code = "126899286590086675";
            string json = "{" +
                   "\"appkey\":\"" + AppId + "\"," +
                    "\"method\":\"gy.erp.trade.get\"," +
                    "\"platform_code\":\"" + platform_code + "\"," +
                    "\"sessionkey\":\"" + SessionKey + "\"" +
                    "}";
            string signature = sign(json, AppSecret);
            string info = "{" +
                   "\"appkey\":\"" + AppId + "\"," +
                    "\"method\":\"gy.erp.trade.get\"," +
                    "\"platform_code\":\"" + platform_code + "\"," +
                    "\"sessionkey\":\"" + SessionKey + "\"," +
                    "\"sign\":\"" + signature + "\"" +
                "}";
            var request = WebRequest.Create(API_Url) as HttpWebRequest;
            request.ContentType = "text/json";
            request.Method = "post";
            string result = "";
            StreamWriter streamWriter = new StreamWriter(request.GetRequestStream());
            try
            {
                streamWriter.Write(info);
                streamWriter.Flush();
                streamWriter.Close();
                var response = request.GetResponse();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                    StringBuilder sb = new StringBuilder(result);
                    sb.Replace("\"refund\":\"NoRefund\"", "\"refund\":0");
                    sb.Replace("\"refund\":\"RefundSuccess\"", "\"refund\":1");
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    orders_Result r = JsonConvert.DeserializeObject<orders_Result>(sb.ToString());
                    return Json(new { result = "SUCCESS", data = r }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                streamWriter.Close();
                //CommonUtilities.writeLog("page: " + page + ", Exception: " + ex.Message);
                return null;
            }
            //return null;
        }

        //public async Task<int> getERPItems_Count()
        //{
        //    string json = "{" +
        //            "\"appkey\":\"" + AppId + "\"," +
        //            "\"method\":\"gy.erp.trade.get\"," +
        //            "\"sessionkey\":\"" + SessionKey + "\"" +
        //            "}";
        //    string signature = sign(json, AppSecret);
        //    var request = WebRequest.Create(API_Url) as HttpWebRequest;
        //    string info = "{" +
        //        "\"appkey\":\"" + AppId + "\"," +
        //            "\"method\":\"gy.erp.trade.get\"," +
        //            "\"sessionkey\":\"" + SessionKey + "\"," +
        //            "\"sign\":\"" + signature + "\"" +
        //        "}";
        //    //return Content(info);
        //    string result = "";
        //    try
        //    {
        //        request.ContentType = "text/json";
        //        request.Method = "post";
        //        using (var streamWriter = new StreamWriter(request.GetRequestStream()))
        //        {
        //            streamWriter.Write(info);
        //            streamWriter.Flush();
        //            streamWriter.Close();
        //            var response = await request.GetResponseAsync() as HttpWebResponse;
        //            using (var reader = new StreamReader(response.GetResponseStream()))
        //            {
        //                result = reader.ReadToEnd();
        //                //return Content(result);
        //                JavaScriptSerializer serializer = new JavaScriptSerializer();
        //                Items_Result r = JsonConvert.DeserializeObject<Items_Result>(result);
        //                if (r != null)
        //                {
        //                    return r;
        //                }
        //                return -1;
        //            }
        //        }

        //    }
        //    catch (UriFormatException)
        //    {
        //        return -1;
        //        //return Content(uex.Message);// 出错处理
        //    }
        //    catch (WebException)
        //    {
        //        return -1;//return Content(ex.Message);// 出错处理
        //    }
        //}
    }
}