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
using PagedList;

namespace PeriodAid.Controllers
{
    [Authorize]
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
        // 上传销售记录页面
        public ActionResult CustomUploadFile(int plattformId)
        {
            ViewBag.PlattformId = plattformId;
            return View();
        }
        // 获取已上传列表
        [HttpPost]
        public ActionResult CustomUploadFilePartial(int plattformId, string month)
        {
            DateTime start = Convert.ToDateTime(month);
            DateTime end = start.AddMonths(1);
            var list = from m in _db.SS_UploadRecord
                       where m.SalesRecord_Date >= start && m.SalesRecord_Date < end
                       && m.Plattform_Id == plattformId
                       select new { record_date = m.SalesRecord_Date };
            return Json(list);
        }

        // 上传销售记录
        [HttpPost]
        public ActionResult UploadFile(FormCollection form, int plattformId)
        {
            var file = Request.Files[0];
            if (file != null)
            {
                var fileName = DateTime.Now.Ticks + ".csv";
                AliOSSUtilities util = new AliOSSUtilities();
                util.PutObject(file.InputStream, "ExcelUpload/" + fileName);
                var date_time = form["file-date"].ToString();
                var result = Read_InsertFile(plattformId, fileName, Convert.ToDateTime(date_time));
                if (result)
                    return Json(new { result = "SUCCESS" });
                else
                    return Json(new { result = "FAIL" });
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }
        // 库存预估
        public ActionResult Calc_Storage(int plattformId)
        {
            ViewBag.PlattformId = plattformId;
            return View();
        }
        public ActionResult Calc_StoragePartial(int plattformId, int? days)
        {
            int _days = days ?? 15;//默认30天
            var upload_record = _db.SS_UploadRecord.Where(m => m.Plattform_Id == plattformId).OrderByDescending(m => m.SalesRecord_Date).FirstOrDefault();
            if (upload_record != null)
            {
                ViewBag.Calc_Days = _days;
                DateTime end = upload_record.SalesRecord_Date;
                DateTime start = end.AddDays(0 - _days);
                var content = from m in _db.SS_SalesRecord
                              where m.SalesRecord_Date > start && m.SalesRecord_Date <= end
                              && m.SS_Product.Plattform_Id == plattformId && m.SS_Product.Product_Type >= 0
                              group m by m.SS_Product into g
                              select new CalcStorageViewModel { Product = g.Key, Sales_Count = g.Sum(m => m.Sales_Count), Storage_Count = g.Where(m => m.SalesRecord_Date == end).Sum(m => m.Storage_Count), Sales_Avg = g.Average(m => m.Sales_Count) };
                return PartialView(content);
            }
            return PartialView();
        }

        // 分析EXCEL文件
        private bool Read_InsertFile(int plattformId, string filename, DateTime date)
        {
            AliOSSUtilities util = new AliOSSUtilities();
            StreamReader reader = new StreamReader(util.GetObject("ExcelUpload/" + filename), System.Text.Encoding.GetEncoding("GB2312"), false);
            CsvReader csv_reader = new CsvReader(reader);
            int row_count = 0;
            List<string> headers = new List<string>();
            var storage_list = from m in _db.SS_Storage
                               where m.Plattform_Id == plattformId
                               select m;
            int sales_field = 0;
            int inventory_field = 0;
            decimal pay_money = 0;
            decimal subAccount = 0;
            if (plattformId == 1)
            {
                while (csv_reader.Read())
                {
                    try
                    {
                        string system_code = csv_reader.GetField<string>("商品编号");
                        if (system_code == "合计")
                        {
                            break;
                        }
                        var product = _db.SS_Product.SingleOrDefault(m => m.System_Code == system_code && m.Plattform_Id == plattformId);
                        if (product == null)
                        {
                            string p_code;
                            string p_name;
                            product = new SS_Product()
                            {
                                System_Code = csv_reader.TryGetField<string>("商品编号", out p_code) ? p_code : "NaN",
                                Item_Name = csv_reader.TryGetField<string>("商品名称", out p_name) ? p_name.Substring(0, p_name.Length > 15 ? 15 : p_name.Length) : "NaN",
                                Carton_Spec = 0,
                                Inventory_Date = date,
                                Item_Code = "",
                                Plattform_Id = plattformId,
                                Purchase_Price = 0
                            };
                            _db.SS_Product.Add(product);
                        }
                        foreach (var inventory in storage_list)
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
                                    SS_Product = product,
                                    SalesRecord_Date = date,
                                    Storage_Id = inventory.Id,
                                    Sales_Count = csv_reader.TryGetField<int>(inventory.Sales_Header, out sales_field) ? sales_field : 0,
                                    Storage_Count = csv_reader.TryGetField<int>(inventory.Inventory_Header, out inventory_field) ? inventory_field : 0,
                                };
                                _db.SS_SalesRecord.Add(record);
                            }
                            if (date > product.Inventory_Date)
                            {
                                product.Inventory_Date = date;
                                _db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                            }
                        }
                        row_count++;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            if (plattformId == 2)
            {
                while (csv_reader.Read())
                {
                    try
                    {
                        string system_code = csv_reader.GetField<string>("商品id");
                        if (system_code == "" || system_code == null)
                        {
                            break;
                        }
                        var product = _db.SS_Product.SingleOrDefault(m => m.System_Code == system_code && m.Plattform_Id == plattformId);
                        if (product == null)
                        {
                            string p_code;
                            string p_name;
                            product = new SS_Product()
                            {
                                System_Code = csv_reader.TryGetField<string>("商品id", out p_code) ? p_code : "NaN",
                                Item_Name = csv_reader.TryGetField<string>("商品标题", out p_name) ? p_name.Substring(10, p_name.Length > 15 ? 15 : p_name.Length) : "NaN",
                                Carton_Spec = 0,
                                Inventory_Date = date,
                                Item_Code = "",
                                Plattform_Id = plattformId,
                                Purchase_Price = 0
                            };
                            _db.SS_Product.Add(product);
                        }
                        string inventory_name = csv_reader.GetField<string>("仓库编码");
                        var inventory = _db.SS_Storage.SingleOrDefault(m => m.Inventory_Header == inventory_name && m.Plattform_Id == plattformId);
                        if (inventory == null)
                        {
                            inventory = new SS_Storage()
                            {
                                Inventory_Header = inventory_name,
                                Storage_Name = inventory_name,
                                Plattform_Id = plattformId,
                                Index = 0,
                                Sales_Header = null
                            };
                            _db.SS_Storage.Add(inventory);
                        }
                        var exist_item = _db.SS_SalesRecord.SingleOrDefault(m => m.SalesRecord_Date == date && m.Product_Id == product.Id && m.Storage_Id == inventory.Id);
                        if (exist_item != null)
                        {
                            exist_item.Sales_Count = csv_reader.TryGetField<int>("销量", out sales_field) ? sales_field : 0;
                            exist_item.Pay_Money = csv_reader.TryGetField<decimal>("支付金额", out pay_money) ? pay_money : 0;
                            exist_item.SubAccount_Price = csv_reader.TryGetField<decimal>("支付订单分账金额", out subAccount) ? subAccount : 0;
                        }
                        else
                        {
                            SS_SalesRecord record = new SS_SalesRecord()
                            {
                                SS_Product = product,
                                SalesRecord_Date = date,
                                Storage_Id = inventory.Id,
                                Sales_Count = csv_reader.TryGetField<int>("销量", out sales_field) ? sales_field : 0,
                                Storage_Count = 0,
                                Pay_Money = csv_reader.TryGetField<decimal>("支付金额", out pay_money) ? pay_money : 0,
                                SubAccount_Price = csv_reader.TryGetField<decimal>("支付订单分账金额", out subAccount) ? subAccount : 0
                            };
                            _db.SS_SalesRecord.Add(record);
                        }
                        if (date > product.Inventory_Date)
                        {
                            product.Inventory_Date = date;
                            _db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                        }
                        row_count++;

                    }
                    catch (Exception)
                    {
                        return false;
                    }
                    _db.SaveChanges();
                }
            }
            var upload_record = _db.SS_UploadRecord.SingleOrDefault(m => m.Plattform_Id == plattformId && m.SalesRecord_Date == date);
            if (upload_record != null)
            {
                upload_record.Upload_Date = DateTime.Now;
                _db.Entry(upload_record).State = System.Data.Entity.EntityState.Modified;
            }
            else
            {
                upload_record = new SS_UploadRecord()
                {
                    Plattform_Id = plattformId,
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
            var inventory_list = _db.SS_Storage.Where(m => m.Plattform_Id == 1).OrderBy(m => m.Id);
            var product_list = _db.SS_Product.Where(m => m.Plattform_Id == 1&&m.Product_Type>=0);
            // 写标题
            IRow row = sheet.CreateRow(0);
            int cell_pos = 0;
            row.CreateCell(++cell_pos).SetCellValue("产品编号");
            //cell_pos++;
            row.CreateCell(++cell_pos).SetCellValue("产品名称");
            //cell_pos++;
            int days = Convert.ToInt32(form["calc_days"].ToString());
            foreach (var inventory in inventory_list)
            {
                row.CreateCell(++cell_pos).SetCellValue(days + "天销量");
                row.CreateCell(++cell_pos).SetCellValue(inventory.Storage_Name+ "库存");
                row.CreateCell(++cell_pos).SetCellValue("周转数");
                row.CreateCell(++cell_pos).SetCellValue(inventory.Storage_Name + "补货");
            }
            // 写产品列
            int row_pos = 1;
            foreach (var product in product_list)
            {
                NPOI.SS.UserModel.IRow single_row = sheet.CreateRow(row_pos);
                cell_pos = 0;
                single_row.CreateCell(cell_pos).SetCellValue(product.System_Code);
                cell_pos++;
                single_row.CreateCell(cell_pos).SetCellValue(product.Item_Name);
                cell_pos++;
                DateTime current_date = product.Inventory_Date;
                DateTime first_date = product.Inventory_Date.AddDays(0 - days);
                // 最近库存
                var upload_record = _db.SS_UploadRecord.Where(m => m.Plattform_Id == 1).OrderByDescending(m => m.SalesRecord_Date).FirstOrDefault();
                if (form["p_rate_" + product.Id] != null)
                {
                    var _rate = Convert.ToInt32(form["p_rate_" + product.Id].ToString());
                    foreach (var inventory in inventory_list)
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
                                                  && m.SalesRecord_Date > first_date && m.SalesRecord_Date <= current_date
                                                  select m);
                        int recommand_storage = ((int)(period_sales_count.Average(m => m.Sales_Count) * _rate) - storage_count) > 0 ? ((int)(period_sales_count.Average(m => m.Sales_Count) * _rate) - storage_count) : 0;
                        single_row.CreateCell(++cell_pos).SetCellValue(period_sales_count.Sum(m => m.Sales_Count));
                        single_row.CreateCell(++cell_pos).SetCellValue(storage_count);
                        single_row.CreateCell(++cell_pos).SetCellValue(_rate);
                        single_row.CreateCell(++cell_pos).SetCellValue(recommand_storage);
                    }
                    row_pos++;
                }
            }
            MemoryStream _stream = new MemoryStream();
            book.Write(_stream);
            _stream.Flush();
            _stream.Seek(0, SeekOrigin.Begin);
            return File(_stream, "application/vnd.ms-excel", DateTime.Now.ToString("yyyyMMddHHmmss") + "库存表.xls");
        }

        /*public ActionResult LeadingIn() {
            return View();
        }*/

        /// <summary>
        /// 当前库存页面
        /// </summary>
        /// <returns></returns>
        public ActionResult PlattformInventory_form(int? plattformId, DateTime? select_date)
        {
            var DataDate = (from m in _db.SS_SalesRecord
                            select m.SalesRecord_Date).Distinct();
            ViewBag.DataDate = DataDate;
            if (plattformId != null)
            {
                var storage = from m in _db.SS_Storage
                              where m.Plattform_Id == plattformId
                              select m;
                ViewBag.storage = storage;
                var SalesRecord = from m in _db.SS_SalesRecord
                                  where m.SS_Storage.SS_Plattform.Id == plattformId && m.SS_Product.Plattform_Id == plattformId
                                  group m by m.SS_Product into g
                                  select new Product_SummaryViewModel
                                  {
                                      Product = g.Key,
                                      Sales_Sum = g.Sum(m => m.Sales_Count),
                                      Inventory_Sum = g.Sum(m => m.Storage_Count)
                                  };

                return View(SalesRecord);
            }
            else
            {
                var storage = from m in _db.SS_Storage
                              where m.Plattform_Id == plattformId
                              select m;
                ViewBag.storage = storage;
                var SalesRecord = from m in _db.SS_SalesRecord
                                  where m.SS_Product.Plattform_Id == plattformId
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



        public ActionResult StorageShow(int plattformId, int? Storage, DateTime? select_date)
        {
            var storage = from m in _db.SS_Storage
                          select m;
            ViewBag.storage = storage;
            var DataDate = (from m in _db.SS_SalesRecord
                            select m.SalesRecord_Date).Distinct();
            ViewBag.DataDate = DataDate;
            if (select_date != null)
            {
                if (Storage.ToString() != "")
                {
                    if (Storage.ToString() == "0")
                    {
                        var SalesRecord = from m in _db.SS_SalesRecord
                                          where m.SalesRecord_Date == select_date && m.SS_Storage.SS_Plattform.Id == plattformId
                                          group m by m.SS_Product into g
                                          select new Product_SummaryViewModel
                                          {
                                              Product = g.Key,
                                              Sales_Sum = g.Sum(m => m.Sales_Count),
                                              Inventory_Sum = g.Sum(m => m.Storage_Count)
                                          };
                        return PartialView(SalesRecord);
                    }
                    else
                    {
                        var SalesRecord = from m in _db.SS_SalesRecord
                                          where m.SalesRecord_Date == select_date && m.Storage_Id == Storage
                                          group m by m.SS_Product into g
                                          select new Product_SummaryViewModel
                                          {
                                              Product = g.Key,
                                              Sales_Sum = g.Sum(m => m.Sales_Count),
                                              Inventory_Sum = g.Sum(m => m.Storage_Count)
                                          };
                        return PartialView(SalesRecord);
                    }
                }
                else
                {
                    var SalesRecord = from m in _db.SS_SalesRecord
                                      where m.SalesRecord_Date == select_date && m.SS_Storage.SS_Plattform.Id == plattformId
                                      group m by m.SS_Product into g
                                      select new Product_SummaryViewModel
                                      {
                                          Product = g.Key,
                                          Sales_Sum = g.Sum(m => m.Sales_Count),
                                          Inventory_Sum = g.Sum(m => m.Storage_Count)
                                      };
                    return PartialView(SalesRecord);

                }
            }
            else
            {
                if (Storage.ToString() != "")
                {
                    if (Storage.ToString() == "0")
                    {
                        var SalesRecord = from m in _db.SS_SalesRecord
                                          group m by m.SS_Product into g
                                          select new Product_SummaryViewModel
                                          {
                                              Product = g.Key,
                                              Sales_Sum = g.Sum(m => m.Sales_Count),
                                              Inventory_Sum = g.Sum(m => m.Storage_Count)
                                          };
                        return PartialView(SalesRecord);
                    }
                    else
                    {
                        var SalesRecord = from m in _db.SS_SalesRecord
                                          where m.Storage_Id == Storage
                                          group m by m.SS_Product into g
                                          select new Product_SummaryViewModel
                                          {
                                              Product = g.Key,
                                              Sales_Sum = g.Sum(m => m.Sales_Count),
                                              Inventory_Sum = g.Sum(m => m.Storage_Count)
                                          };
                        return PartialView(SalesRecord);
                    }
                }
                else
                {
                    var SalesRecord = from m in _db.SS_SalesRecord
                                      where m.SS_Storage.Plattform_Id == plattformId
                                      group m by m.SS_Product into g
                                      select new Product_SummaryViewModel
                                      {
                                          Product = g.Key,
                                          Sales_Sum = g.Sum(m => m.Sales_Count),
                                          Inventory_Sum = g.Sum(m => m.Storage_Count)
                                      };
                    return PartialView(SalesRecord);

                }
            }
        }

        public ActionResult ProductList(int plattformId)
        {
            ViewBag.PlattformId = plattformId;
            return View();
        }

        public ActionResult ProductListPartial(int plattformId, int? page, string query)
        {
            int _page = page ?? 1;
            if (query != null)
            {
                var SearchResult = (from m in _db.SS_Product
                                    where m.Item_Name.Contains(query) || m.Item_Code.Contains(query) || m.System_Code.Contains(query)
                                    orderby m.Product_Type descending, m.Id descending
                                    select m).ToPagedList(_page, 15);
                return PartialView(SearchResult);
            }
            else
            {
                var productlist = (from m in _db.SS_Product
                                   where m.Plattform_Id == plattformId
                                   orderby m.Product_Type descending, m.Id descending
                                   select m).ToPagedList(_page, 15);
                return PartialView(productlist);
            }
        }

        public ActionResult EditProductInfo(int productId)
        {
            var item = _db.SS_Product.SingleOrDefault(m => m.Id == productId);
            List<Object> selectvalue = new List<Object>();
            selectvalue.Add(new { Text = "下架", Value = -1 });
            selectvalue.Add(new { Text = "正常", Value = 0 });
            selectvalue.Add(new { Text = "爆款", Value = 1 });
            ViewBag.SelectList = new SelectList(selectvalue, "Value", "Text", item.Product_Type);
            return PartialView(item);
        }

        [HttpPost]
        public ActionResult EditProductInfo(SS_Product model)
        {
            if (ModelState.IsValid)
            {
                SS_Product item = new SS_Product();
                if (TryUpdateModel(item))
                {
                    _db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
            }
            return Json(new { result = "FAIL" });
        }
        // 产品数据图表
        public ActionResult ViewProductStatistic(int productId)
        {
            ViewBag.ProductId = productId;
            return View();
        }
        [HttpPost]
        public JsonResult ViewProductStatisticPartial(int productId, string start, string end)
        {
            DateTime _start = Convert.ToDateTime(start);
            DateTime _end = Convert.ToDateTime(end);
            var info_data = from m in _db.SS_SalesRecord
                            where m.SalesRecord_Date >= _start && m.SalesRecord_Date <= _end
                            && m.Product_Id == productId
                            group m by m.SalesRecord_Date into g
                            orderby g.Key
                            select new ProductStatisticViewModel { salesdate = g.Key, salescount = g.Sum(m => m.Sales_Count) };
            DateTime current_date = _start;
            var data = new List<ProductStatisticViewModel>();
            while (current_date <= _end)
            {
                int _salescount = 0;
                var item = info_data.SingleOrDefault(m => m.salesdate == current_date);
                if (item != null)
                {
                    _salescount = item.salescount;
                }
                data.Add(new ProductStatisticViewModel()
                {
                    salescount = _salescount,
                    salesdate = current_date
                });
                current_date = current_date.AddDays(1);
            }
            return Json(new { result = "SUCCESS", data = data });
        }
    }

}