using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CsvHelper;
using Microsoft.AspNet.Identity.Owin;
using PeriodAid.Models;
using PeriodAid.DAL;

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

        public ActionResult CustomUploadFile(int plattformId)
        {
            ViewBag.PlattformId = plattformId;
            return View();
        }
        [HttpPost]
        public ActionResult CustomUploadFilePartial(int plattformId, string month)
        {
            DateTime start = Convert.ToDateTime(month);
            DateTime end = start.AddMonths(1);
            var list = from m in _db.SS_UploadRecord
                       where m.SalesRecord_Date >= start && m.SalesRecord_Date < end
                       select new { record_date = m.SalesRecord_Date};
            return Json(list);
        }

        [HttpPost]
        public ActionResult UploadFile(FormCollection form)
        {
            var file = Request.Files[0];
            if (file != null)
            {
                var fileName = DateTime.Now.Ticks + ".csv";
                AliOSSUtilities util = new AliOSSUtilities();
                util.PutObject(file.InputStream, "ExcelUpload/" + fileName);
                var date_time = form["file-date"].ToString();
                Read_InsertFile(fileName, Convert.ToDateTime(date_time));
                return Json(new { result = "SUCCESS" });
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }
        public ActionResult Calc_Storage(int plattformId)
        {
            var upload_record = _db.SS_UploadRecord.OrderByDescending(m => m.SalesRecord_Date).FirstOrDefault();
            if (upload_record != null)
            {
                DateTime end = upload_record.SalesRecord_Date;
                DateTime start = end.AddDays(-30);
                var content = from m in _db.SS_SalesRecord
                              where m.SalesRecord_Date >= start && m.SalesRecord_Date <= end
                              && m.SS_Product.Plattform_Id == 1
                              group m by m.SS_Product into g
                              select new CalcStorageViewModel { Product = g.Key, Sales_Count = g.Sum(m => m.Sales_Count), Storage_Count = g.Where(m => m.SalesRecord_Date == end).Sum(m => m.Storage_Count) };
                return View(content);
            }
            return View();
        }
        private bool Read_InsertFile(string filename, DateTime date)
        {
            AliOSSUtilities util = new AliOSSUtilities();
            StreamReader reader = new StreamReader(util.GetObject("ExcelUpload/" + filename), System.Text.Encoding.GetEncoding("GB2312"), false);
            CsvReader csv_reader = new CsvReader(reader);
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
                    if(system_code == "合计")
                    {
                        break;
                    }
                    var product = _db.SS_Product.SingleOrDefault(m => m.System_Code == system_code);
                    if(product != null)
                    {
                        foreach(var inventory in storage_list)
                        {
                            var exist_item = _db.SS_SalesRecord.SingleOrDefault(m => m.SalesRecord_Date == date && m.Product_Id == product.Id && m.Storage_Id == inventory.Id);
                            if (exist_item != null)
                            {
                                exist_item.Sales_Count = csv_reader.TryGetField<int>(inventory.Sales_Header, out sales_field) ? sales_field : 0;
                                exist_item.Storage_Count = csv_reader.TryGetField<int>(inventory.Inventory_Header, out inventory_field) ? inventory_field : 0;
                                _db.Entry(exist_item).State = System.Data.Entity.EntityState.Modified;
                            }
                            else
                            {
                                SS_SalesRecord record = new SS_SalesRecord()
                                {
                                    Product_Id = product.Id,
                                    SalesRecord_Date = date,
                                    Storage_Id = inventory.Id,
                                    Sales_Count = csv_reader.TryGetField<int>(inventory.Sales_Header, out sales_field) ? sales_field : 0,
                                    Storage_Count = csv_reader.TryGetField<int>(inventory.Inventory_Header, out inventory_field) ? inventory_field : 0
                                };
                                _db.SS_SalesRecord.Add(record);
                            }
                            if (date > product.Inventory_Date)
                            {
                                product.Inventory_Date = date;
                                _db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                            }
                        }
                    }
                    row_count++;
                }catch(Exception)
                {
                    return false;
                }
            }
            var upload_record = _db.SS_UploadRecord.SingleOrDefault(m => m.Plattform_Id == 1 && m.SalesRecord_Date == date);
            if (upload_record != null)
            {
                upload_record.Upload_Date = DateTime.Now;
                _db.Entry(upload_record).State = System.Data.Entity.EntityState.Modified;
            }
            else
            {
                upload_record = new SS_UploadRecord()
                {
                    Plattform_Id = 1,
                    Upload_Date = DateTime.Now,
                    SalesRecord_Date = date
                };
                _db.SS_UploadRecord.Add(upload_record);
            }
            _db.SaveChanges();
            return true;
        }
        // 获取仓库表格
        [HttpPost]
        public ActionResult getInventoryExcel(FormCollection form)
        {
            /*string pstr = System.Web.HttpContext.Current.Request.Form["Param"];
            JavaScriptSerializer s = new JavaScriptSerializer(); //继承自 System.Web.Script.Serialization;
            List<CalcStorageParmsViewModel> jr = s.Deserialize<List<CalcStorageParmsViewModel>>(pstr); //只要你的JSON串没问题就可以转*/
            HSSFWorkbook book = new HSSFWorkbook();
            ISheet sheet = book.CreateSheet("Total");
            var inventory_list = _db.SS_Storage.Where(m => m.Plattform_Id == 1);
            var product_list = _db.SS_Product.Where(m => m.Plattform_Id == 1);
            // 写标题
            IRow row = sheet.CreateRow(0);
            int cell_pos = 0;
            row.CreateCell(cell_pos).SetCellValue("产品编号");
            cell_pos++;
            row.CreateCell(cell_pos).SetCellValue("产品名称");
            cell_pos++;
            foreach(var inventory in inventory_list.OrderBy(m=>m.Id)) {
                row.CreateCell(cell_pos).SetCellValue(inventory.Storage_Name + "补货数量");
                cell_pos++;
            }
            // 写产品列
            int row_pos = 1;
            foreach(var product in product_list.Where(m=>m.Id==317))
            {
                NPOI.SS.UserModel.IRow single_row = sheet.CreateRow(row_pos);
                cell_pos = 0;
                single_row.CreateCell(cell_pos).SetCellValue(product.System_Code);
                cell_pos++;
                single_row.CreateCell(cell_pos).SetCellValue(product.Item_Name);
                cell_pos++;
                DateTime current_date = DateTime.Now.Date;
                DateTime first_date = product.Inventory_Date.AddDays(-30);
                // 最近库存
                var upload_record = _db.SS_UploadRecord.Where(m => m.Plattform_Id == 1).OrderByDescending(m => m.SalesRecord_Date).FirstOrDefault();
                if (form["p_rate_" + product.Id] != null)
                {
                    var _rate = Convert.ToDecimal(form["p_rate_" + product.Id].ToString());
                    foreach (var inventory in inventory_list.OrderBy(m => m.Id))
                    {
                        // 最新库存
                        var last_inventory = _db.SS_SalesRecord.SingleOrDefault(m => m.Product_Id == product.Id && m.Storage_Id == inventory.Id && m.SalesRecord_Date == upload_record.SalesRecord_Date);
                        int storage_count;
                        if (last_inventory != null)
                        {
                            storage_count = last_inventory.Storage_Count;
                        }
                        else
                        {
                            storage_count = 0;
                        }
                        var period_sales_count = (from m in _db.SS_SalesRecord
                                                  where m.Product_Id == product.Id && m.Storage_Id == inventory.Id
                                                  && m.SalesRecord_Date >= first_date && m.SalesRecord_Date <= current_date
                                                  select m).Sum(m => m.Sales_Count);
                        int recommand_storage = ((int)(period_sales_count * _rate) - storage_count) > 0 ? ((int)(period_sales_count * _rate) - storage_count) : 0;
                        single_row.CreateCell(cell_pos).SetCellValue(recommand_storage);
                        cell_pos++;
                    }
                    row_pos++;
                }
            }

            MemoryStream _stream = new MemoryStream();
            book.Write(_stream);
            _stream.Flush();
            _stream.Seek(0, SeekOrigin.Begin);
            return File(_stream, "application/vnd.ms-excel", DateTime.Now.ToString("yyyyMMddHHmmss")+"库存表.xls");


        }

        /*public ActionResult LeadingIn() {
            return View();
        }*/

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult PlattformInventory_form(DateTime? select_date) {
            var storage = from m in _db.SS_Storage                         
                          select m;
            ViewBag.storage = storage;
            var DataDate = (from m in _db.SS_SalesRecord
                            select m.SalesRecord_Date).Distinct();
            ViewBag.DataDate = DataDate;
            if (select_date != null)
            {
                var SalesRecord = from m in _db.SS_SalesRecord
                                  where m.SalesRecord_Date == select_date
                                  group m by m.SS_Product into g
                                  select new Product_SummaryViewModel
                                  {
                                      Product = g.Key,
                                      Sales_Sum = g.Sum(m => m.Sales_Count),
                                      Inventory_Sum = g.Sum(m => m.Storage_Count)
                                  };

                return View(SalesRecord);
            }
            else {
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
        }



        public ActionResult StorageShow(int Storage,DateTime? select_date) {
            var storage = from m in _db.SS_Storage
                          select m;
            ViewBag.storage = storage;
            var DataDate = (from m in _db.SS_SalesRecord

                            select m.SalesRecord_Date).Distinct();
            ViewBag.DataDate = DataDate;
            if (select_date == null)
            {
                var SalesRecord = from m in _db.SS_SalesRecord
                                  where m.Storage_Id == Storage
                                  select m;
                ViewBag.SalesRecord = SalesRecord;
            }
            else {
                var SalesRecord = from m in _db.SS_SalesRecord
                                  where m.SalesRecord_Date==select_date
                                  select m;
                ViewBag.SalesRecord = SalesRecord;

            }
            return PartialView();
        }

    }
    
}