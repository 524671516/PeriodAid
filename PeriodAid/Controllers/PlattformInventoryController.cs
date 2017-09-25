using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CsvHelper;
using System.IO;
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
            }
            _db.SaveChanges();
            return Content(sb.ToString());
        }
    }
    
}