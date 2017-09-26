using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CsvHelper;
using System.Text;
using Microsoft.AspNet.Identity.Owin;
using PeriodAid.Models;

namespace PeriodAid.Controllers
{
    public class PlattformInventoryController : Controller
    {
        // GET: PlattformInventory
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ShopStorageModel _db;

        public PlattformInventoryController()
        {
            _db = new ShopStorageModel();
        }

        public PlattformInventoryController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Read_InsertFile()
        {
            string folder = HttpContext.Server.MapPath("~/Content/xlsx/");
            StreamReader reader = new StreamReader(folder + "201709221540.csv", System.Text.Encoding.GetEncoding("GB2312"), false);
            CsvReader csv_reader = new CsvReader(reader);
            StringBuilder sb = new StringBuilder();
            //string s = String.Join(",", csv_reader.FieldHeaders);
            int row_count = 0;
            List<string> headers = new List<string>();
            var storage_list = from m in _db.SS_Storage
                               where m.Plattform_Id == 1
                               select m;
            int sales_field = 0;
            int inventory_field = 0;
            while (csv_reader.Read())
            {
                try
                {
                    string system_code = csv_reader.GetField<string>("商品编号");
                    var product = _db.SS_Product.SingleOrDefault(m => m.System_Code == system_code);
                    if(product != null)
                    {
                        foreach(var inventory in storage_list)
                        {
                            SS_SalesRecord record = new SS_SalesRecord()
                            {
                                Product_Id = product.Id,
                                SalesRecord_Date = DateTime.Now.Date,
                                Storage_Id = inventory.Id,
                                Sales_Count = csv_reader.TryGetField<int>(inventory.Sales_Header, out sales_field) ? sales_field : 0,
                                Storage_Count = csv_reader.TryGetField<int>(inventory.Inventory_Header, out inventory_field) ? inventory_field : 0
                            };
                            _db.SS_SalesRecord.Add(record);
                        }
                    }
                    row_count++;
                }catch(Exception e)
                {
                    sb.Append(e.Message);
                    row_count++;
                }
                //try
                //{
                //    string system_code = csv_reader.GetField<string>("商品编号");
                //    string product_name = csv_reader.GetField<string>("商品名称");
                //    DateTime date = new DateTime(2017, 9, 12);
                //    SS_Product Product = new SS_Product()
                //    {
                //        System_Code = system_code,
                //        Item_Name = product_name.Substring(9, 15),
                //        Inventory_Date = date,
                //        Plattform_Id = 1
                //    };
                //    _db.SS_Product.Add(Product);
                //    row_count++;
                //}
                //catch (Exception e)
                //{
                //    sb.Append(e.Message);
                //    row_count++;
                //}
            }
            _db.SaveChanges();
            return Content(sb.ToString());
        }

        public ActionResult LeadingIn() {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult PlattformInventory_form() {
            var storage = from m in _db.SS_Storage                         
                          select m;
            ViewBag.storage = storage;
            var DataDate = (from m in _db.SS_Product
                            select m).Take(1);
            ViewBag.DataDate = DataDate;
            var SalesRecord = from m in _db.SS_SalesRecord
                              group m by m.SS_Product into g
                              select new Product_SummaryViewModel
                              {
                                  Product = g.Key,
                                  Sales_Sum = g.Sum(m => m.Sales_Count),
                                  Inventory_Sum = g.Sum(m => m.Storage_Count)
                              };
            return View(SalesRecord);
        }

        public ActionResult StorageShow(int Storage) {
            var storage = from m in _db.SS_Storage
                          select m;
            ViewBag.storage = storage;
            var SalesRecord = from m in _db.SS_SalesRecord
                          where m.Storage_Id == Storage
                          select m;
            ViewBag.SalesRecord = SalesRecord;
            var DataDate = (from m in _db.SS_Product
                            select m).Take(1);
            ViewBag.DataDate = DataDate;
            return PartialView();
        }

    }
    
}