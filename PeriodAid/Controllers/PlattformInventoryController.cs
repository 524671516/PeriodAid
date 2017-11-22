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
using NPOI.SS.Util;
using ICSharpCode.SharpZipLib.Zip;

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
                if (plattformId == 1)
                {
                    var result = Read_JdFile(plattformId, fileName, Convert.ToDateTime(date_time));
                    if (result)
                        return Json(new { result = "SUCCESS" });
                    else
                        return Json(new { result = "FAIL" });
                }
                else if (plattformId == 2)
                {
                    var result = Read_TmFile(plattformId, fileName, Convert.ToDateTime(date_time));
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
            else
            {
                return Json(new { result = "FAIL" });
            }
        }

        // 京东送货单导入
        public ActionResult StorageOrder(FormCollection form)
        {
            var file = Request.Files[0];
            var filename = DateTime.Now.Ticks + ".csv";
            AliOSSUtilities util = new AliOSSUtilities();
            util.PutObject(file.InputStream, "ExcelUpload/" + filename);
            StreamReader reader = new StreamReader(util.GetObject("ExcelUpload/" + filename), System.Text.Encoding.GetEncoding("GB2312"), false);
            CsvReader csv_reader = new CsvReader(reader);
            List<string> headers = new List<string>();
            List<StorageOrder> storageOrder = new List<StorageOrder>();
            while (csv_reader.Read())
            {
                try
                {
                    string OrderId = csv_reader.GetField<string>("订单号");
                    if (OrderId.ToString() == "" || OrderId.ToString() == null)
                    {
                        break;
                    }
                    string ProductId = csv_reader.GetField<string>("商品编码");
                    var product = _db.SS_Product.SingleOrDefault(m => m.System_Code == ProductId);
                    if (product != null)
                    {
                        int o_count;
                        string s_name, o_code, su_name;
                        int order_count = csv_reader.TryGetField<int>("采购数量", out o_count) ? o_count : 0;
                        int _carton_count = order_count / (product.Carton_Spec == 0 ? 1 : product.Carton_Spec);
                        StorageOrder storageorder = new StorageOrder()
                        {
                            OrderId = csv_reader.TryGetField<string>("订单号", out o_code) ? o_code : "NaN",
                            SystemCode = product.System_Code,
                            ProductName = product.Item_Name,
                            StorageName = csv_reader.TryGetField<string>("分配机构", out s_name) ? s_name : "NaN",
                            SubStoName = csv_reader.TryGetField<string>("仓库", out su_name) ? su_name : "NaN",
                            CartonSpec = product.Carton_Spec,
                            OrderCount = order_count,
                            CartonCount = _carton_count
                        };
                        storageOrder.Add(storageorder);
                    }
                }
                catch (Exception)
                {
                    return View("error");
                }
            }
            var _stream = OutExcel(storageOrder);
            return File(_stream, "application/zip", DateTime.Now.ToString("yyyyMMddHHmmss") + "分仓表.zip");

        }
        // 分仓单
        private HSSFWorkbook SetStorageList(string orderId, string storageName, string subStoName, List<StorageOrder> storageOrder)
        {
            HSSFWorkbook book = new HSSFWorkbook();
            ISheet sheet = book.CreateSheet(storageName + orderId);

            // 基本信息
            int cell_pos = 0;
            int row_pos = 0;
            ICellStyle borderstyle = book.CreateCellStyle();//设置边框
            borderstyle.BorderBottom = BorderStyle.Thin;
            borderstyle.BorderLeft = BorderStyle.Thin;
            borderstyle.BorderRight = BorderStyle.Thin;
            borderstyle.BorderTop = BorderStyle.Thin;
            borderstyle.Alignment = HorizontalAlignment.Center;
            borderstyle.VerticalAlignment = VerticalAlignment.Center;
            IFont border_font = book.CreateFont(); //创建一个字体样式对象
            border_font.FontName = "宋体"; //和excel里面的字体对应
            border_font.FontHeightInPoints = 10;//字体大小
            borderstyle.SetFont(border_font);
            for (int i = 0; i < 6; i++)
            {
                int[] a = { 18, 27, 31, 18, 18, 14 };
                sheet.SetColumnWidth(i, a[i] * 256);
                //sheet.SetDefaultColumnStyle(i, borderstyle);
            }
            IRow row = sheet.CreateRow(row_pos);
            IRow row1 = sheet.CreateRow(0);
            row1.Height = 27 * 20;
            ICell cell1 = row1.CreateCell(0);
            for (int i=1;i<=5;i++) {
                row1.CreateCell(i);
            }
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 5));
            for (int i = 1; i < 5; i++)
            {
                sheet.AddMergedRegion(new CellRangeAddress(i, i, 2, 3));
                sheet.AddMergedRegion(new CellRangeAddress(i, i, 4, 5));
            }
            ICellStyle Title_style = book.CreateCellStyle();//大标题
            ICellStyle Left_style = book.CreateCellStyle();//左标题
            Left_style.BorderBottom = BorderStyle.Thin;
            Left_style.BorderLeft = BorderStyle.Thin;
            Left_style.BorderRight = BorderStyle.Thin;
            Left_style.BorderTop = BorderStyle.Thin;
            ICellStyle Center_style = book.CreateCellStyle();//居中标题
            Center_style.BorderBottom = BorderStyle.Thin;
            Center_style.BorderLeft = BorderStyle.Thin;
            Center_style.BorderRight = BorderStyle.Thin;
            Center_style.BorderTop = BorderStyle.Thin;
            Title_style.VerticalAlignment = VerticalAlignment.Center;//垂直对齐
            Title_style.Alignment = HorizontalAlignment.Center;//水平对齐
            Left_style.VerticalAlignment = VerticalAlignment.Center;
            Center_style.VerticalAlignment = VerticalAlignment.Center;
            Center_style.Alignment = HorizontalAlignment.Center;
            IFont Tfont = book.CreateFont(); //创建一个字体样式对象
            IFont Left_font = book.CreateFont(); //创建一个字体样式对象
            IFont Center_font = book.CreateFont(); //创建一个字体样式对象
            Tfont.FontName = "宋体"; //和excel里面的字体对应
            Tfont.FontHeightInPoints = 18;//字体大小
            Tfont.Boldweight = short.MaxValue;
            Left_font.FontName = "宋体"; //和excel里面的字体对应
            Left_font.FontHeightInPoints = 11;//字体大小
            Left_font.Boldweight = short.MaxValue;
            Center_font.FontName = "宋体"; //和excel里面的字体对应
            Center_font.FontHeightInPoints = 11;//字体大小
            Center_font.Boldweight = short.MaxValue;
            Title_style.SetFont(Tfont);
            Left_style.SetFont(Left_font);
            Center_style.SetFont(Center_font);
            cell1.SetCellValue("送货单");
            
            var single_sendorder = from m in storageOrder
                                   where m.OrderId == orderId
                                   select m;
            IRow single_row = sheet.CreateRow(++row_pos);
            cell_pos = 0;
            var s0 = single_row.CreateCell(cell_pos);
            s0.SetCellValue("送货单号");
            single_row.CreateCell(++cell_pos).SetCellValue(storageName + DateTime.Now.Month + DateTime.Now.Day);
            var c0 = single_row.CreateCell(++cell_pos);
            c0.SetCellValue("供应商名称");
            single_row.CreateCell(++cell_pos);
            single_row.CreateCell(++cell_pos).SetCellValue("上海寿全斋电子商务有限公司");
            single_row.CreateCell(++cell_pos);
            cell_pos = 0;
            IRow single_row1 = sheet.CreateRow(++row_pos);
            var s1 = single_row1.CreateCell(cell_pos);
            s1.SetCellValue("采购单号");
            single_row1.CreateCell(++cell_pos).SetCellValue(orderId);
            var c1 = single_row1.CreateCell(++cell_pos);
            c1.SetCellValue("总箱数");
            single_row1.CreateCell(++cell_pos);
            single_row1.CreateCell(++cell_pos).SetCellValue(single_sendorder.Sum(m => m.CartonCount));
            single_row1.CreateCell(++cell_pos);
            IRow single_row2 = sheet.CreateRow(++row_pos);
            cell_pos = 0;
            var s2 = single_row2.CreateCell(cell_pos);
            s2.SetCellValue("目的城市");
            single_row2.CreateCell(++cell_pos).SetCellValue(storageName);
            var c2 = single_row2.CreateCell(++cell_pos);
            c2.SetCellValue("目的仓库");
            single_row2.CreateCell(++cell_pos);
            single_row2.CreateCell(++cell_pos).SetCellValue(subStoName);
            single_row2.CreateCell(++cell_pos);
            IRow single_row3 = sheet.CreateRow(++row_pos);
            cell_pos = 0;
            var s3 = single_row3.CreateCell(cell_pos++);
            s3.SetCellValue("TC预约号");
            var c3 = single_row3.CreateCell(++cell_pos);
            c3.SetCellValue("TC预约送货日期");
            single_row3.CreateCell(++cell_pos);
            single_row3.CreateCell(++cell_pos);
            single_row3.CreateCell(++cell_pos);
            IRow single_row4 = sheet.CreateRow(++row_pos);
            cell_pos = 0;
            var s4 = single_row4.CreateCell(cell_pos);
            s4.SetCellValue("箱号/箱号段");
            var c4 = single_row4.CreateCell(++cell_pos);
            c4.SetCellValue("sku");
            var c5 = single_row4.CreateCell(++cell_pos);
            c5.SetCellValue("商品名称");
            var c6 = single_row4.CreateCell(++cell_pos);
            c6.SetCellValue("箱规（个/箱）");
            var c7 = single_row4.CreateCell(++cell_pos);
            c7.SetCellValue("商品总数量/个");
            var c8 = single_row4.CreateCell(++cell_pos);
            c8.SetCellValue("备注");
            var count_num = 1;
            foreach (var item in single_sendorder)
            {
                for (var i = 1; i <= item.CartonCount; i++)
                {
                    IRow single_row5 = sheet.CreateRow(++row_pos);
                    cell_pos = 0;
                    single_row5.CreateCell(cell_pos).SetCellValue("第" + count_num++ + "箱，共" + single_sendorder.Sum(m => m.CartonCount) + "箱");
                    single_row5.CreateCell(++cell_pos).SetCellValue(item.SystemCode);
                    single_row5.CreateCell(++cell_pos).SetCellValue(item.ProductName);
                    single_row5.CreateCell(++cell_pos).SetCellValue(item.CartonSpec);
                    single_row5.CreateCell(++cell_pos).SetCellValue(item.CartonSpec);
                    single_row5.CreateCell(++cell_pos);
                }
            }
            IRow single_row6 = sheet.CreateRow(++row_pos);
            cell_pos = 0;
            var s6 = single_row6.CreateCell(cell_pos);
            s6.SetCellValue("合计");
            single_row6.CreateCell(++cell_pos);
            single_row6.CreateCell(++cell_pos);
            var c9 = single_row6.CreateCell(++cell_pos);
            c9.SetCellValue(single_sendorder.Sum(m => m.CartonCount) + "箱");
            var c10 = single_row6.CreateCell(++cell_pos);
            c10.SetCellValue(single_sendorder.Sum(m => m.OrderCount));
            single_row6.CreateCell(++cell_pos);
            cell_pos = 0;
            IRow single_row7 = sheet.CreateRow(++row_pos);
            var s7 = single_row7.CreateCell(cell_pos);
            s7.SetCellValue("供应商发货盖章:");
            single_row7.CreateCell(++cell_pos);
            single_row7.CreateCell(++cell_pos);
            single_row7.CreateCell(++cell_pos);
            single_row7.CreateCell(++cell_pos);
            single_row7.CreateCell(++cell_pos);
            cell_pos = 0;
            IRow single_row8 = sheet.CreateRow(++row_pos);
            var s8 = single_row8.CreateCell(cell_pos);
            single_row8.CreateCell(++cell_pos);
            single_row8.CreateCell(++cell_pos);
            single_row8.CreateCell(++cell_pos);
            single_row8.CreateCell(++cell_pos);
            single_row8.CreateCell(++cell_pos);
            s8.SetCellValue("TC收货签章:");
            cell_pos = 0;
            IRow single_row9 = sheet.CreateRow(++row_pos);
            var s9 = single_row9.CreateCell(cell_pos);
            s9.SetCellValue("备注:");
            single_row9.CreateCell(++cell_pos);
            single_row9.CreateCell(++cell_pos);
            single_row9.CreateCell(++cell_pos);
            single_row9.CreateCell(++cell_pos);
            single_row9.CreateCell(++cell_pos);
            for (int i = 0; i <= sheet.LastRowNum; i++)
            {
                var default_row = sheet.GetRow(i);
                foreach(var column in default_row)
                {
                    column.CellStyle = borderstyle;
                    column.Row.Height = 19 * 20;
                }
            }
            for (int i = sheet.LastRowNum - 2; i <= sheet.LastRowNum; i++)
            {
                IRow irow = sheet.GetRow(i);
                sheet.AddMergedRegion(new CellRangeAddress(i, i, 0, 5));
                foreach (var column in irow)
                {
                    column.CellStyle = Left_style;
                }
            }
            cell1.CellStyle = Title_style;
            s0.CellStyle = Left_style;
            c0.CellStyle = Center_style;
            s1.CellStyle = Left_style;
            c1.CellStyle = Center_style;
            s2.CellStyle = Left_style;
            c2.CellStyle = Center_style;
            s3.CellStyle = Left_style;
            c4.CellStyle = Center_style;
            c3.CellStyle = Center_style;
            s4.CellStyle = Left_style;
            c5.CellStyle = Center_style;
            c6.CellStyle = Center_style;
            s6.CellStyle = Left_style;
            c7.CellStyle = Center_style;
            c8.CellStyle = Center_style;
            c9.CellStyle = Center_style;
            c10.CellStyle = Center_style;
            return book;
        }
        // 不干胶
        private HSSFWorkbook SetLabelList(List<StorageOrder> storageOrder)
        {
            HSSFWorkbook book = new HSSFWorkbook();
            // 打印不干胶
            ISheet _sheet = book.CreateSheet("不干胶");
            // 写标题
            IRow _row = _sheet.CreateRow(0);
            int _cell_pos = 0;
            _row.CreateCell(_cell_pos).SetCellValue("采购单号");
            _row.CreateCell(++_cell_pos).SetCellValue("目的城市");
            _row.CreateCell(++_cell_pos).SetCellValue("目的DC");
            _row.CreateCell(++_cell_pos).SetCellValue("SKU");
            _row.CreateCell(++_cell_pos).SetCellValue("商品名称");
            _row.CreateCell(++_cell_pos).SetCellValue("实装数量");
            _row.CreateCell(++_cell_pos).SetCellValue("箱数序号");
            _row.CreateCell(++_cell_pos).SetCellValue("总箱数");
            // 写产品列
            int _row_pos = 1;
            var order_list = from m in storageOrder
                             group m by m.OrderId into g
                             select g;
            foreach(var order in order_list)
            {
                int count = 1;
                foreach (var item in storageOrder.Where(m=>m.OrderId == order.Key))
                {
                    int cell_pos;
                    for (var i = 1; i <= item.CartonCount; i++)
                    {
                        IRow single_row = _sheet.CreateRow(_row_pos++);
                        cell_pos = 0;
                        single_row.CreateCell(cell_pos).SetCellValue(item.OrderId);
                        single_row.CreateCell(++cell_pos).SetCellValue(item.StorageName);
                        single_row.CreateCell(++cell_pos).SetCellValue(item.SubStoName);
                        single_row.CreateCell(++cell_pos).SetCellValue(item.SystemCode);
                        single_row.CreateCell(++cell_pos).SetCellValue(item.ProductName);
                        single_row.CreateCell(++cell_pos).SetCellValue(item.CartonSpec);
                        single_row.CreateCell(++cell_pos).SetCellValue(count);
                        single_row.CreateCell(++cell_pos).SetCellValue(storageOrder.Where(m => m.OrderId == item.OrderId).Sum(m => m.CartonCount));
                        count++;
                    }
                }
            }
            
            return book;
        }
        private MemoryStream OutExcel(List<StorageOrder> storageOrder)
        {
            MemoryStream _stream = new MemoryStream();
            ZipFile zip = ZipFile.Create(_stream);
            var sendorderlist = from m in storageOrder
                                group m by new { OrderId = m.OrderId, StorageName = m.StorageName, SubStoName = m.SubStoName } into g
                                select g;
            zip.BeginUpdate();
            foreach (var sendorder in sendorderlist)
            {
                var book = SetStorageList(sendorder.Key.OrderId, sendorder.Key.StorageName, sendorder.Key.SubStoName, storageOrder);
                MemoryStream file = new MemoryStream();
                book.Write(file);
                StreamDataSource sds = new StreamDataSource(file);
                zip.Add(sds, sendorder.Key.StorageName + sendorder.Key.OrderId + ".xls");
            }
            var book2 = SetLabelList(storageOrder);
            MemoryStream file2 = new MemoryStream();
            book2.Write(file2);
            StreamDataSource sds2 = new StreamDataSource(file2);
            zip.Add(sds2, "分仓单.xls");
            zip.CommitUpdate();
            _stream.Flush();
            _stream.Seek(0, SeekOrigin.Begin);
            return _stream;
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

        // 京东
        private bool Read_JdFile(int plattformId, string filename, DateTime date)
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
                    if (system_code == "合计")
                    {
                        break;
                    }
                    var product = _db.SS_Product.SingleOrDefault(m => m.System_Code == system_code && m.Plattform_Id == 1);
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
                            Plattform_Id = 1,
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
        // 天猫
        private bool Read_TmFile(int plattformId, string filename, DateTime date)
        {
            AliOSSUtilities util = new AliOSSUtilities();
            StreamReader reader = new StreamReader(util.GetObject("ExcelUpload/" + filename), System.Text.Encoding.GetEncoding("GB2312"), false);
            CsvReader csv_reader = new CsvReader(reader);
            int row_count = 0;
            List<string> headers = new List<string>();
            var storage_list = from m in _db.SS_Storage
                               where m.Plattform_Id == 2
                               select m;
            int sales_field = 0;
            decimal pay_money = 0;
            decimal subAccount = 0;
            while (csv_reader.Read())
            {
                try
                {
                    string system_code = csv_reader.GetField<string>("商品id");
                    if (system_code == "" || system_code == null)
                    {
                        break;
                    }
                    var product = _db.SS_Product.SingleOrDefault(m => m.System_Code == system_code && m.Plattform_Id == 2);
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
                            Plattform_Id = 2,
                            Purchase_Price = 0
                        };
                        _db.SS_Product.Add(product);
                    }
                    string inventory_name = csv_reader.GetField<string>("仓库编码");
                    var inventory = _db.SS_Storage.SingleOrDefault(m => m.Inventory_Header == inventory_name && m.Plattform_Id == 2);
                    if (inventory == null)
                    {
                        inventory = new SS_Storage()
                        {
                            Inventory_Header = inventory_name,
                            Storage_Name = inventory_name,
                            Plattform_Id = 2,
                            Index = 0,
                            Sales_Header = null,
                            Storage_Type = 0
                        };
                        _db.SS_Storage.Add(inventory);
                    }
                    var exist_item = _db.SS_SalesRecord.SingleOrDefault(m => m.SalesRecord_Date == date && m.Product_Id == product.Id && m.Storage_Id == inventory.Id);
                    if (exist_item != null)
                    {
                        exist_item.Sales_Count = csv_reader.TryGetField<int>("销量", out sales_field) ? sales_field : 0;
                        exist_item.Pay_Money = csv_reader.TryGetField<decimal>("支付金额", out pay_money) ? pay_money : 0;
                        exist_item.SubAccount_Price = csv_reader.TryGetField<decimal>("支付订单分账金额", out subAccount) ? subAccount : 0;
                        _db.Entry(exist_item).State = System.Data.Entity.EntityState.Modified;
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
            var upload_record = _db.SS_UploadRecord.SingleOrDefault(m => m.Plattform_Id == 2 && m.SalesRecord_Date == date);
            if (upload_record != null)
            {
                upload_record.Upload_Date = DateTime.Now;
                _db.Entry(upload_record).State = System.Data.Entity.EntityState.Modified;
            }
            else
            {
                upload_record = new SS_UploadRecord()
                {
                    Plattform_Id = 2,
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
            var product_list = _db.SS_Product.Where(m => m.Plattform_Id == 1 && m.Product_Type >= 0);
            // 写标题
            IRow row = sheet.CreateRow(0);
            int cell_pos = 0;
            row.CreateCell(cell_pos).SetCellValue("产品编号");
            row.CreateCell(++cell_pos).SetCellValue("产品名称");
            row.CreateCell(++cell_pos).SetCellValue("商品编码");
            int days = Convert.ToInt32(form["calc_days"].ToString());
            foreach (var inventory in inventory_list)
            {
                row.CreateCell(++cell_pos).SetCellValue(inventory.Storage_Name + "" + days + "天销量");
                row.CreateCell(++cell_pos).SetCellValue(inventory.Storage_Name + "库存");
                row.CreateCell(++cell_pos).SetCellValue(inventory.Storage_Name + "周转数");
                row.CreateCell(++cell_pos).SetCellValue(inventory.Storage_Name + "补货");
                row.CreateCell(++cell_pos).SetCellValue(inventory.Storage_Name + "箱数");
            }
            row.CreateCell(++cell_pos).SetCellValue("合计补货");
            row.CreateCell(++cell_pos).SetCellValue("合计金额");
            // 写产品列
            int row_pos = 1;
            foreach (var product in product_list)
            {
                IRow single_row = sheet.CreateRow(row_pos);
                cell_pos = 0;
                single_row.CreateCell(cell_pos).SetCellValue(product.System_Code);
                single_row.CreateCell(++cell_pos).SetCellValue(product.Item_Name);
                single_row.CreateCell(++cell_pos).SetCellValue(product.Item_Code);
                DateTime current_date = product.Inventory_Date;
                DateTime first_date = product.Inventory_Date.AddDays(0 - days);
                int total_count = 0;
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
                        int cartonspec = product.Carton_Spec == 0 ? 1 : product.Carton_Spec;
                        int carton_count = (recommand_storage % cartonspec) == 0 ? recommand_storage / cartonspec : (recommand_storage / cartonspec) + 1;
                        int final_storage = carton_count * cartonspec;
                        single_row.CreateCell(++cell_pos).SetCellValue(period_sales_count.Sum(m => m.Sales_Count));
                        single_row.CreateCell(++cell_pos).SetCellValue(storage_count);
                        single_row.CreateCell(++cell_pos).SetCellValue(_rate);
                        single_row.CreateCell(++cell_pos).SetCellValue(final_storage);
                        single_row.CreateCell(++cell_pos).SetCellValue(carton_count);
                        total_count += final_storage;
                    }
                    single_row.CreateCell(++cell_pos).SetCellValue(total_count);
                    single_row.CreateCell(++cell_pos).SetCellValue((double)(total_count * product.Purchase_Price));
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
        public ActionResult PlattformInventory_form(int? plattformId)
        {
            var DataDate = (from m in _db.SS_SalesRecord
                            where m.SS_Storage.SS_Plattform.Id == plattformId && m.SS_Product.Plattform_Id == plattformId
                            orderby m.SalesRecord_Date descending
                            select m.SalesRecord_Date).FirstOrDefault();
            ViewBag.DataDate = DataDate;
            var _plattformId = plattformId ?? 1;
            var storage = from m in _db.SS_Storage
                          where m.Plattform_Id == plattformId
                          select m;
            ViewBag.storage = storage;
            return View();
        }

        public ActionResult StorageShow(int plattformId, int? Storage, DateTime start, DateTime end)
        {
            int _storage = Storage ?? 0;
            if (_storage == 0)
            {
                var SalesRecord = from m in _db.SS_SalesRecord
                                  where m.SalesRecord_Date >= start && m.SalesRecord_Date <= end && m.SS_Storage.SS_Plattform.Id == plattformId
                                  && m.SS_Product.Product_Type>=0
                                  group m by m.SS_Product into g
                                  select new Product_SummaryViewModel
                                  {
                                      Product = g.Key,
                                      Sales_Sum = g.Sum(m => m.Sales_Count),
                                      Inventory_Sum = g.Where(m => m.SalesRecord_Date == end).Sum(m => m.Storage_Count),
                                      Pay_Sum = g.Sum(m => m.Pay_Money),
                                      SubAccount_Sum = g.Sum(m => m.SubAccount_Price),
                                      Settlement = g.Key.Purchase_Price
                                      // 问题出在哪里？
                                      // 理解意义
                                      // 效率提升
                                      // 去除无用代码
                                  };
                return PartialView(SalesRecord);
            }
            else
            {
                var SalesRecord = from m in _db.SS_SalesRecord
                                  where m.SalesRecord_Date >= start && m.SalesRecord_Date <= end && m.Storage_Id == Storage
                                  && m.SS_Product.Product_Type >= 0
                                  group m by m.SS_Product into g
                                  select new Product_SummaryViewModel
                                  {
                                      Product = g.Key,
                                      Sales_Sum = g.Sum(m => m.Sales_Count),
                                      Inventory_Sum = g.Where(m => m.SalesRecord_Date == end).Sum(m => m.Storage_Count),
                                      Pay_Sum = g.Sum(m => m.Pay_Money),
                                      SubAccount_Sum = g.Sum(m => m.SubAccount_Price),
                                      Settlement = g.Key.Purchase_Price
                                  };
                return PartialView(SalesRecord);
            }
        }

        public ActionResult TMS_StorageShow(int plattformId, int? Storage, DateTime start, DateTime end)
        {
            int _storage = Storage ?? 0;
            if(_storage == 0)
            {
                var SalesRecord = from m in _db.SS_SalesRecord
                                  where m.SalesRecord_Date >= start && m.SalesRecord_Date <= end && m.SS_Storage.SS_Plattform.Id == plattformId
                                  group m by m.SS_Product into g
                                  select new Product_SummaryViewModel
                                  {
                                      Product = g.Key,
                                      Sales_Sum = g.Sum(m => m.Sales_Count),
                                      Pay_Sum = g.Sum(m => m.Pay_Money),
                                      SubAccount_Sum = g.Sum(m => m.SubAccount_Price)
                                  };
                return PartialView(SalesRecord);
            }else
            {
                var SalesRecord = from m in _db.SS_SalesRecord
                                  where m.SalesRecord_Date >= start && m.SalesRecord_Date <= end && m.Storage_Id == Storage
                                  group m by m.SS_Product into g
                                  select new Product_SummaryViewModel
                                  {
                                      Product = g.Key,
                                      Sales_Sum = g.Sum(m => m.Sales_Count),
                                      Pay_Sum = g.Sum(m => m.Pay_Money),
                                      SubAccount_Sum = g.Sum(m => m.SubAccount_Price)
                                  };
                return PartialView(SalesRecord);
            }
        }

        public ActionResult TMShow_StorageShow(int plattformId,int Storage, DateTime start, DateTime end)
        {
            if(Storage!= 0)
            {
                var SalesRecord = from m in _db.SS_SalesRecord
                                  where m.SalesRecord_Date >= start && m.SalesRecord_Date <= end && m.SS_Storage.SS_Plattform.Id == plattformId && m.Storage_Id == Storage
                                  group m by m.SS_Storage into g
                                  select new Product_SummaryViewModel
                                  {
                                      Sales_Sum = g.Sum(m => m.Sales_Count),
                                      Pay_Sum = g.Sum(m => m.Pay_Money),
                                      SubAccount_Sum = g.Sum(m => m.SubAccount_Price)
                                  };
                return PartialView(SalesRecord);
            }
            else
            {
                var SalesRecord = from m in _db.SS_SalesRecord
                                  where m.SalesRecord_Date >= start && m.SalesRecord_Date <= end && m.SS_Storage.SS_Plattform.Id == plattformId
                                  group m by m.SS_Storage.SS_Plattform into g
                                  select new Product_SummaryViewModel
                                  {
                                      Sales_Sum = g.Sum(m => m.Sales_Count),
                                      Pay_Sum = g.Sum(m => m.Pay_Money),
                                      SubAccount_Sum = g.Sum(m => m.SubAccount_Price)
                                  };
                return PartialView(SalesRecord);
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
                if (query != "")
                {
                    var product = (from m in _db.SS_Product
                                   where m.Plattform_Id == plattformId
                                   select m);
                    var SearchResult = (from m in product
                                        where m.Item_Name.Contains(query) || m.Item_Code.Contains(query) || m.System_Code.Contains(query)
                                        orderby m.Product_Type descending, m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
                else
                {
                    var SearchResult = (from m in _db.SS_Product
                                        where m.Plattform_Id == plattformId
                                        orderby m.Product_Type descending, m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }

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
            var eventList = from m in _db.SS_Event
                            where m.Product_Id == productId
                            select m;
            ViewBag.eventList = eventList;
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

        // 活动打标
        public ActionResult EventList(int plattformId)
        {
            var plattFormId = from m in _db.SS_Event
                              where m.SS_Product.Plattform_Id == plattformId
                              select m;
            return View();
        }

        public ActionResult EventListPartial(int? page,string query,int plattformId)
        {
            int _page = page ?? 1;
            
            if (query != null)
            {
                if (query != "")
                {
                    var productlist = (from m in _db.SS_Event
                                       where m.EventName.Contains(query) || m.SS_Product.Item_Name.Contains(query) && m.SS_Product.Plattform_Id == plattformId
                                       orderby m.EventDate descending, m.Id descending
                                       select m).ToPagedList(_page, 15);
                    return PartialView(productlist);
                }
                else
                {
                    var productlist = (from m in _db.SS_Event
                                       where m.SS_Product.Plattform_Id == plattformId
                                       orderby m.EventDate descending, m.Id descending
                                       select m).ToPagedList(_page, 15);
                    return PartialView(productlist);
                }

            }
            else
            {
                var productlist = (from m in _db.SS_Event
                                   where m.SS_Product.Plattform_Id == plattformId
                                   orderby m.EventDate descending, m.Id descending
                                   select m).ToPagedList(_page, 15);
                return PartialView(productlist);
            }
        }

        public ActionResult EditEventInfo(int eventId)
        {
            var item =  _db.SS_Event.SingleOrDefault(m => m.Id == eventId);
            return PartialView(item);
        }

        [HttpPost]
        public ActionResult EditEventInfo(SS_Event model)
        {
            if (ModelState.IsValid)
            {
                SS_Event item = new SS_Event();
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
        public ActionResult DeleteEvent(int id)
        {
            var item = _db.SS_Event.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                try
                {
                    _db.SS_Event.Remove(item);
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

        public ActionResult AddEventPartial(int plattformId)
        {
            var plattFormId = from m in _db.SS_Event
                              where m.SS_Product.Plattform_Id == plattformId
                              select m;
            return PartialView();
        }
        [HttpPost]
        public ActionResult AddEventPartial(SS_Event model, FormCollection form)
        {
            ModelState.Remove("EventDate");
            if (ModelState.IsValid)
            {
                string[] timelist = form["eventtime"].ToString().Split(',');
                // 每天循环
                for (int i = 0; i < timelist.Length; i++)
                {
                    var item = new SS_Event();
                    item.EventName = model.EventName;
                    item.Product_Id = model.Product_Id;
                    item.EventDate = Convert.ToDateTime(timelist[i]);
                    _db.SS_Event.Add(item);
                }
                _db.SaveChanges();
                return Content("SUCCESS");
            }
            else
            {
                return PartialView(model);
            }
            //return Content("ERROR1");
        }
        
        public ActionResult ViewEventList(int productId)
        {
            var eventList = from m in _db.SS_Event
                            select m;
            ViewBag.eventList = eventList;

            return PartialView();

        }

        public ActionResult ViewEventListPartial(int productId,DateTime select_date, int plattformId)
        {
            var item = from m in _db.SS_Event
                       where m.Product_Id == productId && m.EventDate == select_date
                       select m;
            var count = from m in _db.SS_SalesRecord
                        where m.Product_Id == productId && m.SalesRecord_Date == select_date && m.SS_Product.Plattform_Id == plattformId
                        group m by m.SalesRecord_Date into g
                        orderby g.Key
                        select new EventStatisticViewModel {
                            salesdate = g.Key,
                            salescount = g.Sum(m => m.Sales_Count),
                            Pay_Sum = g.Sum(m => m.Pay_Money),
                            SubAccount_Sum = g.Sum(m => m.SubAccount_Price),
                            Sales_Price = g.Sum(m => m.SS_Product.Purchase_Price)/g.Count()
                        };
            ViewBag.eventList = item;
            ViewBag.salesCount = count;
            return PartialView(item);
        }
        // 产品数据图表
        public ActionResult ViewEventStatistic(int productId)
        {
            var eventList = from m in _db.SS_Event
                            where m.Product_Id == productId
                            select m;
            ViewBag.eventList = eventList;
            return View();
        }
        [HttpPost]
        public JsonResult ViewEventStatisticPartial(int productId, string start, string end)
        {
            DateTime _start = Convert.ToDateTime(start);
            DateTime _end = Convert.ToDateTime(end);
            var info_data = from m in _db.SS_SalesRecord
                            where m.SalesRecord_Date >= _start && m.SalesRecord_Date <= _end
                            && m.Product_Id == productId 
                            group m by m.SalesRecord_Date into g
                            orderby g.Key
                            select new EventStatisticViewModel { salesdate = g.Key, salescount = g.Sum(m => m.Sales_Count) };
            DateTime current_date = _start;
            var data = new List<EventStatisticViewModel>();
            while (current_date <= _end)
            {
                int _salescount = 0;
                var item = info_data.SingleOrDefault(m => m.salesdate == current_date);
                if (item != null)
                {
                    _salescount = item.salescount;
                }
                data.Add(new EventStatisticViewModel()
                {
                    salescount = _salescount,
                    salesdate = current_date
                });
                current_date = current_date.AddDays(1);
            }
            return Json(new { result = "SUCCESS", data = data });
        }

        [HttpPost]
        public JsonResult QueryProduct(string query, int plattformId)
        {
            var product = from m in _db.SS_Product
                        where m.Plattform_Id == plattformId
                        && m.Item_Name.Contains(query)
                        select new { Id = m.Id, ProductName = m.SS_Plattform.Plattform_Name + "- " + m.Item_Name  };
            return Json(product);
        }

        // 京东流量统计
        //public ActionResult JD_Traffic(int plattformId)
        //{
        //    return View();
        //}

        //private bool Read_TrafficFile(int plattformId, string filename, DateTime date, string plattformName)
        //{
        //    AliOSSUtilities util = new AliOSSUtilities();
        //    StreamReader reader = new StreamReader(util.GetObject("ExcelUpload/" + filename), System.Text.Encoding.UTF8, false);
        //    CsvReader csv_reader = new CsvReader(reader);
        //    List<string> headers = new List<string>();
        //    var traffic_plattform = _db.SS_TrafficPlattform.SingleOrDefault(m => m.TrafficPlattform_Name == plattformName);
        //    while (csv_reader.Read())
        //    {
        //        try
        //        {
        //            string date_flow = csv_reader.GetField<string>("日期");
        //            if (date_flow == "")
        //            {
        //                break;
        //            }
        //            if (traffic_plattform == null)
        //            {
        //                traffic_plattform = new SS_TrafficPlattform()
        //                {
        //                    TrafficPlattform_Name = plattformName,
        //                    Plattform_Id = 1,
        //                    AttendTrafficSource = new List<SS_TrafficSource>()
        //                };
        //                _db.SS_TrafficPlattform.Add(traffic_plattform);
        //                _db.SaveChanges();
        //            }
        //            //string s_name;
        //            string source_name = csv_reader.GetField<string>("流量渠道");
        //            var traffic_source = _db.SS_TrafficSource.SingleOrDefault(m => m.TrafficSource_Name == source_name);
        //            if (traffic_source == null)
        //            {
        //                traffic_source = new SS_TrafficSource()
        //                {
        //                    TrafficSource_Name = source_name,
        //                    TrafficPlattform_Id = traffic_plattform.Id,
        //                    Source_Type = 0
        //                };
        //                _db.SS_TrafficSource.Add(traffic_source);
        //                //traffic_plattform.AttendTrafficSource.Add(traffic_source);
        //            }
        //            _db.SaveChanges();
        //            string s_code;
        //            string system_code = csv_reader.TryGetField<string>("商品编码", out s_code) ? s_code : "NaN";
        //            var product = _db.SS_Product.SingleOrDefault(m => m.System_Code == system_code);
        //            if (product != null)
        //            {
        //                var traffic_data = _db.SS_TrafficData.SingleOrDefault(m => m.Update == date && m.TrafficPlattform_Id == traffic_plattform.Id && m.TrafficSource_Id == traffic_source.Id && m.Product_Id == product.Id);
        //                int p_flow, p_visitor, p_customer, o_count;
        //                if (traffic_data == null)
        //                {
        //                    traffic_data = new SS_TrafficData()
        //                    {
        //                        Update = date,
        //                        Product_Flow = csv_reader.TryGetField<int>("商品流量", out p_flow) ? p_flow : 0,
        //                        Product_Visitor = csv_reader.TryGetField<int>("商品访客", out p_visitor) ? p_visitor : 0,
        //                        Product_Customer = csv_reader.TryGetField<int>("商品消费者", out p_customer) ? p_customer : 0,
        //                        Order_Count = csv_reader.TryGetField<int>("商品订单行", out o_count) ? o_count : 0,
        //                        Product_Id = product.Id,
        //                        TrafficSource_Id = traffic_source.Id,
        //                        SS_TrafficPlattform = traffic_plattform
        //                    };
        //                    _db.SS_TrafficData.Add(traffic_data);
        //                }
        //                //else
        //                //{
        //                //    traffic_data.Update = date;
        //                //    traffic_data.Product_Flow = csv_reader.TryGetField<int>("商品流量", out p_flow) ? p_flow : 0;
        //                //    traffic_data.Product_Visitor = csv_reader.TryGetField<int>("商品访客", out p_visitor) ? p_visitor : 0;
        //                //    traffic_data.Product_Customer = csv_reader.TryGetField<int>("商品消费者", out p_customer) ? p_customer : 0;
        //                //    traffic_data.Order_Count = csv_reader.TryGetField<int>("商品订单行", out o_count) ? o_count : 0;
        //                //    traffic_data.Product_Id = product.Id;
        //                //    traffic_data.TrafficSource_Id = traffic_source.Id;
        //                //    traffic_data.SS_TrafficPlattform = traffic_plattform;
        //                //    _db.Entry(traffic_data).State = System.Data.Entity.EntityState.Modified;
        //                //}
        //            }
        //        }
        //        catch (Exception)
        //        {
        //            return false;
        //        }
        //    }
        //    var upload_traffic = _db.SS_UploadTraffic.SingleOrDefault(m => m.TrafficPlattform_Id == traffic_plattform.Id && m.Traffic_Date == date);
        //    if (upload_traffic != null)
        //    {
        //        upload_traffic.Upload_Date = DateTime.Now;
        //        _db.Entry(upload_traffic).State = System.Data.Entity.EntityState.Modified;
        //    }
        //    else
        //    {
        //        upload_traffic = new SS_UploadTraffic()
        //        {
        //            TrafficPlattform_Id = traffic_plattform.Id,
        //            Upload_Date = DateTime.Now,
        //            Traffic_Date = date
        //        };
        //        _db.SS_UploadTraffic.Add(upload_traffic);
        //    }
        //    _db.SaveChanges();
        //    return true;
        //}
        //[HttpPost]
        //public ActionResult UploadTrafficFile(FormCollection form, int plattformId, string plattformName)
        //{
        //    var file = Request.Files[0];
        //    if (file != null)
        //    {
        //        var fileName = DateTime.Now.Ticks + ".csv";
        //        AliOSSUtilities util = new AliOSSUtilities();
        //        util.PutObject(file.InputStream, "ExcelUpload/" + fileName);
        //        var date_time = form["file-date"].ToString();
        //        if (plattformId == 1)
        //        {
        //            var result = Read_TrafficFile(plattformId, fileName, Convert.ToDateTime(date_time), plattformName);
        //            if (result)
        //                return Json(new { result = "SUCCESS" });
        //            else
        //                return Json(new { result = "FAIL" });
        //        }
        //        else
        //        {
        //            return Json(new { result = "FAIL" });
        //        }
        //    }
        //    else
        //    {
        //        return Json(new { result = "FAIL" });
        //    }
        //}
        //[HttpPost]
        //public ActionResult getTrafficExcel(FormCollection form, DateTime date)
        //{
        //    HSSFWorkbook book = getTrafficPlattform(form, date);
        //    ISheet sheet = book.CreateSheet(date.Month + "." + date.Day + "汇总");
        //    // 写标题
        //    IRow row = sheet.CreateRow(0);
        //    int cell_pos = 0;
        //    row.CreateCell(cell_pos).SetCellValue("日期");
        //    row.CreateCell(++cell_pos).SetCellValue("商品编码");
        //    row.CreateCell(++cell_pos).SetCellValue("商品名称");
        //    row.CreateCell(++cell_pos).SetCellValue("流量渠道");
        //    row.CreateCell(++cell_pos).SetCellValue("商品流量");
        //    row.CreateCell(++cell_pos).SetCellValue("商品访客");
        //    row.CreateCell(++cell_pos).SetCellValue("商品消费者");
        //    row.CreateCell(++cell_pos).SetCellValue("商品订单行");
        //    row.CreateCell(++cell_pos).SetCellValue("商品转化率");
        //    // 写产品列
        //    int row_pos = 1;
        //    var productlist = from m in _db.SS_TrafficData
        //                      group m by m.SS_Product into g
        //                      select g;
        //    ICellStyle Center_style = book.CreateCellStyle();//居中标题
        //    Center_style.VerticalAlignment = VerticalAlignment.Center;//垂直对齐
        //    foreach (var product in productlist)
        //    {
        //        var productOrder = from m in product
        //                           where m.Update == date
        //                           group m by m.SS_TrafficPlattform into g
        //                           select g;
        //        bool firstCount = true;
        //        foreach (var productorder in productOrder)
        //        {
        //            IRow single_row = sheet.CreateRow(row_pos);
        //            cell_pos = 0;
        //            var c0 = single_row.CreateCell(cell_pos);
        //            c0.SetCellValue(date.ToString("d"));
        //            var c1 = single_row.CreateCell(++cell_pos);
        //            c1.SetCellValue(product.Key.System_Code);
        //            var c2 = single_row.CreateCell(++cell_pos);
        //            c2.SetCellValue(product.Key.Item_Name);
        //            var rowCount = productOrder.Count();
        //            if (rowCount > 1 && firstCount == true)
        //            {
        //                var row0 = row_pos;
        //                var row1 = row_pos + rowCount - 1;
        //                sheet.AddMergedRegion(new CellRangeAddress(row0, row1, 0, 0));
        //                sheet.AddMergedRegion(new CellRangeAddress(row0, row1, 1, 1));
        //                sheet.AddMergedRegion(new CellRangeAddress(row0, row1, 2, 2));
        //                c0.CellStyle = Center_style;
        //                c1.CellStyle = Center_style;
        //                c2.CellStyle = Center_style;
        //                firstCount = false;
        //            }
        //            single_row.CreateCell(++cell_pos).SetCellValue(productorder.Key.TrafficPlattform_Name);
        //            var Ratio = (decimal)productorder.Sum(m => m.Product_Customer) / (productorder.Sum(m => m.Product_Visitor) == 0 ? 1 : productorder.Sum(m => m.Product_Visitor));
        //            single_row.CreateCell(++cell_pos).SetCellValue(productorder.Sum(m => m.Product_Flow));
        //            single_row.CreateCell(++cell_pos).SetCellValue(productorder.Sum(m => m.Product_Visitor));
        //            single_row.CreateCell(++cell_pos).SetCellValue(productorder.Sum(m => m.Product_Customer));
        //            single_row.CreateCell(++cell_pos).SetCellValue(productorder.Sum(m => m.Order_Count));
        //            single_row.CreateCell(++cell_pos).SetCellValue(Ratio.ToString("p2"));
        //            row_pos++;
        //        }
        //        var product_count = from m in product
        //                            where m.Update == date
        //                            group m by m.Update into g
        //                            select g;
        //        foreach (var productCount in product_count)
        //        {
        //            IRow single_row1 = sheet.CreateRow(row_pos);
        //            cell_pos = 4;
        //            var Ratio = (decimal)productCount.Sum(m => m.Product_Customer) / (productCount.Sum(m => m.Product_Visitor) == 0 ? 1 : productCount.Sum(m => m.Product_Visitor));
        //            single_row1.CreateCell(cell_pos).SetCellValue(productCount.Sum(m => m.Product_Flow));
        //            single_row1.CreateCell(++cell_pos).SetCellValue(productCount.Sum(m => m.Product_Visitor));
        //            single_row1.CreateCell(++cell_pos).SetCellValue(productCount.Sum(m => m.Product_Customer));
        //            single_row1.CreateCell(++cell_pos).SetCellValue(productCount.Sum(m => m.Order_Count));
        //            single_row1.CreateCell(++cell_pos).SetCellValue(Ratio.ToString("p2"));
        //        }
        //        row_pos++;
        //    }
        //    row_pos++;
        //    // 各平台总和
        //    var data_each = from m in _db.SS_TrafficData
        //                    where m.Update == date
        //                    group m by m.SS_TrafficPlattform into g
        //                    select g;
        //    foreach (var eachData in data_each)
        //    {
        //        IRow single_row = sheet.CreateRow(row_pos);
        //        cell_pos = 3;
        //        var Ratio = (decimal)eachData.Sum(m => m.Product_Customer) / (eachData.Sum(m => m.Product_Visitor) == 0 ? 1 : eachData.Sum(m => m.Product_Visitor));
        //        single_row.CreateCell(cell_pos).SetCellValue(eachData.Key.TrafficPlattform_Name);
        //        single_row.CreateCell(++cell_pos).SetCellValue(eachData.Sum(m => m.Product_Flow));
        //        single_row.CreateCell(++cell_pos).SetCellValue(eachData.Sum(m => m.Product_Visitor));
        //        single_row.CreateCell(++cell_pos).SetCellValue(eachData.Sum(m => m.Product_Customer));
        //        single_row.CreateCell(++cell_pos).SetCellValue(eachData.Sum(m => m.Order_Count));
        //        single_row.CreateCell(++cell_pos).SetCellValue(Ratio.ToString("p2"));
        //        row_pos++;
        //    }
        //    // 平台总和
        //    var data_all = from m in _db.SS_TrafficData
        //                   where m.Update == date
        //                   group m by m.SS_TrafficPlattform.Plattform_Id into g
        //                   select g;
        //    foreach (var allData in data_all)
        //    {
        //        IRow single_row = sheet.CreateRow(++row_pos);
        //        cell_pos = 3;
        //        var Ratio = (decimal)allData.Sum(m => m.Product_Customer) / (allData.Sum(m => m.Product_Visitor) == 0 ? 1 : allData.Sum(m => m.Product_Visitor));
        //        single_row.CreateCell(cell_pos).SetCellValue("共计：");
        //        single_row.CreateCell(++cell_pos).SetCellValue(allData.Sum(m => m.Product_Flow));
        //        single_row.CreateCell(++cell_pos).SetCellValue(allData.Sum(m => m.Product_Visitor));
        //        single_row.CreateCell(++cell_pos).SetCellValue(allData.Sum(m => m.Product_Customer));
        //        single_row.CreateCell(++cell_pos).SetCellValue(allData.Sum(m => m.Order_Count));
        //        single_row.CreateCell(++cell_pos).SetCellValue(Ratio.ToString("p2"));
        //    }
        //    MemoryStream _stream = new MemoryStream();
        //    book.Write(_stream);
        //    _stream.Flush();
        //    _stream.Seek(0, SeekOrigin.Begin);
        //    return File(_stream, "application/vnd.ms-excel", DateTime.Now.ToString("yyyyMMddHHmmss") + "统计表.xls");
        //}
        //[HttpPost]
        //public HSSFWorkbook getTrafficPlattform(FormCollection form, DateTime date)
        //{
        //    var TrafficPlattform = from m in _db.SS_TrafficPlattform
        //                           select m;
        //    HSSFWorkbook book = new HSSFWorkbook();
        //    foreach (var plattform in TrafficPlattform)
        //    {
        //        var sheetName = plattform.TrafficPlattform_Name;
        //        ISheet sheet = book.CreateSheet(sheetName);
        //        // 写标题
        //        IRow row = sheet.CreateRow(0);
        //        int cell_pos = 0;
        //        row.CreateCell(cell_pos).SetCellValue("日期");
        //        row.CreateCell(++cell_pos).SetCellValue("商品编码");
        //        row.CreateCell(++cell_pos).SetCellValue("商品名称");
        //        row.CreateCell(++cell_pos).SetCellValue("流量渠道");
        //        row.CreateCell(++cell_pos).SetCellValue("商品流量");
        //        row.CreateCell(++cell_pos).SetCellValue("商品访客");
        //        row.CreateCell(++cell_pos).SetCellValue("商品消费者");
        //        row.CreateCell(++cell_pos).SetCellValue("商品订单行");
        //        row.CreateCell(++cell_pos).SetCellValue("商品转化率");
        //        // 写产品列
        //        int row_pos = 1;
        //        var TrafficData = from m in _db.SS_TrafficData
        //                          where m.SS_TrafficPlattform.Id == plattform.Id && m.Update == date
        //                          orderby m.Product_Id
        //                          select m;
        //        foreach (var trafficdata in TrafficData)
        //        {
        //            IRow single_row = sheet.CreateRow(row_pos);
        //            cell_pos = 0;
        //            single_row.CreateCell(cell_pos).SetCellValue(date.ToString("d"));
        //            single_row.CreateCell(++cell_pos).SetCellValue(trafficdata.SS_Product.System_Code);
        //            single_row.CreateCell(++cell_pos).SetCellValue(trafficdata.SS_Product.Item_Name);
        //            single_row.CreateCell(++cell_pos).SetCellValue(trafficdata.SS_TrafficSource.TrafficSource_Name);
        //            single_row.CreateCell(++cell_pos).SetCellValue(trafficdata.Product_Flow);
        //            single_row.CreateCell(++cell_pos).SetCellValue(trafficdata.Product_Visitor);
        //            single_row.CreateCell(++cell_pos).SetCellValue(trafficdata.Product_Customer);
        //            single_row.CreateCell(++cell_pos).SetCellValue(trafficdata.Order_Count);
        //            if (trafficdata.Product_Visitor == 0)
        //            {
        //                var ConvertRatio = (decimal)0;
        //                single_row.CreateCell(++cell_pos).SetCellValue(ConvertRatio.ToString("p2"));
        //            }
        //            else
        //            {
        //                var ConvertRatio = (decimal)trafficdata.Product_Customer / trafficdata.Product_Visitor;
        //                single_row.CreateCell(++cell_pos).SetCellValue(ConvertRatio.ToString("p2"));
        //            }
        //            row_pos++;
        //        }
        //    }
        //    MemoryStream _stream = new MemoryStream();
        //    book.Write(_stream);
        //    _stream.Flush();
        //    _stream.Seek(0, SeekOrigin.Begin);
        //    return book;
        //}
        //// 爆款统计
        //[HttpPost]
        //public ActionResult getHotExcel(FormCollection form)
        //{
        //    var product_list = from m in _db.SS_Product
        //                       where m.Product_Type == 1 && m.Plattform_Id == 1
        //                       select m;
        //    HSSFWorkbook book = new HSSFWorkbook();
        //    foreach (var product in product_list)
        //    {
        //        var sheetName = product.Item_Name;
        //        ISheet sheet = book.CreateSheet(sheetName);
        //        IRow row0 = sheet.CreateRow(0);
        //        int cell_pos = 0;
        //        row0.Height = 70 * 20;
        //        row0.CreateCell(cell_pos).SetCellValue("活动备注");
        //        IRow row = sheet.CreateRow(1);
        //        row.CreateCell(++cell_pos).SetCellValue(product.Item_Name + product.System_Code);
        //        row.CreateCell(++cell_pos).SetCellValue("访客");
        //        row.CreateCell(++cell_pos).SetCellValue("订单");
        //        row.CreateCell(++cell_pos).SetCellValue("商品数量");
        //        row.CreateCell(++cell_pos).SetCellValue("客件数");
        //        row.CreateCell(++cell_pos).SetCellValue("转化率");
        //        row.CreateCell(++cell_pos).SetCellValue("销售成本");
        //        row.CreateCell(++cell_pos).SetCellValue("uv价值");
        //        var data_date = from m in _db.SS_TrafficData
        //                        where m.Product_Id == product.Id
        //                        group m by m.Update into g
        //                        select g;
        //        var row_pos = 2;
        //        foreach (var dataDate in data_date)
        //        {
        //            var dateTime = Convert.ToDateTime(dataDate.Key.Year + "/" + dataDate.Key.Month + "/" + dataDate.Key.Day + " 0:00:00");
        //            var traffic_data = from m in dataDate
        //                               group m by m.SS_TrafficPlattform.Plattform_Id into g
        //                               select g;
        //            foreach (var trafficData in traffic_data)
        //            {
        //                var sales_date = from m in _db.SS_SalesRecord
        //                                 where m.Product_Id == product.Id && m.SalesRecord_Date == dateTime && m.SS_Storage.Storage_Type == 1
        //                                 group m by m.Product_Id into g
        //                                 select g;
        //                foreach (var salesDate in sales_date)
        //                {
        //                    IRow single_row = sheet.CreateRow(row_pos);
        //                    cell_pos = 1;
        //                    var Count = (double)salesDate.Sum(m => m.Sales_Count) / trafficData.Sum(m => m.Order_Count);
        //                    var Ratio = (double)trafficData.Sum(m => m.Order_Count) / trafficData.Sum(m => m.Product_Flow);
        //                    var Cost = salesDate.Sum(m => m.Sales_Count) * product.Purchase_Price;
        //                    var uvValue = salesDate.Sum(m => m.Sales_Count) * product.Purchase_Price / trafficData.Sum(m => m.Product_Visitor);
        //                    single_row.CreateCell(cell_pos).SetCellValue(dataDate.Key.Month + "." + dataDate.Key.Day);
        //                    single_row.CreateCell(++cell_pos).SetCellValue(trafficData.Sum(m => m.Product_Visitor));
        //                    single_row.CreateCell(++cell_pos).SetCellValue(trafficData.Sum(m => m.Order_Count));
        //                    single_row.CreateCell(++cell_pos).SetCellValue(salesDate.Sum(m => m.Sales_Count));
        //                    single_row.CreateCell(++cell_pos).SetCellValue(Math.Round(Count, 2));
        //                    single_row.CreateCell(++cell_pos).SetCellValue(Ratio.ToString("p2"));
        //                    single_row.CreateCell(++cell_pos).SetCellValue(Cost.ToString("0.##"));
        //                    single_row.CreateCell(++cell_pos).SetCellValue(uvValue.ToString("0.00"));
        //                    row_pos++;
        //                }
        //            }

        //        }
        //    }
        //    MemoryStream _stream = new MemoryStream();
        //    book.Write(_stream);
        //    _stream.Flush();
        //    _stream.Seek(0, SeekOrigin.Begin);
        //    return File(_stream, "application/vnd.ms-excel", DateTime.Now.ToString("yyyyMMddHHmmss") + "爆款统计表.xls");
        //}
        //// 渠道统计
        //[HttpPost]
        //public ActionResult getHotSourceExcel(FormCollection form)
        //{
        //    var product_list = from m in _db.SS_Product
        //                       where m.Product_Type == 1 && m.Plattform_Id == 1
        //                       select m;
        //    HSSFWorkbook book = new HSSFWorkbook();
        //    ICellStyle Center_style = book.CreateCellStyle();//居中标题
        //    Center_style.VerticalAlignment = VerticalAlignment.Center;//垂直对齐
        //    foreach (var product in product_list)
        //    {

        //        bool firstProduct = true;
        //        var sheetName = product.Item_Name;
        //        ISheet sheet = book.CreateSheet(sheetName);
        //        IRow row = sheet.CreateRow(0);
        //        int cell_pos = 0;
        //        var row_pos = 1;
        //        row.CreateCell(cell_pos).SetCellValue("日期");
        //        row.CreateCell(++cell_pos).SetCellValue("渠道名称");
        //        row.CreateCell(++cell_pos).SetCellValue("商品访客");
        //        row.CreateCell(++cell_pos).SetCellValue("商品转化率");
        //        var source_data = from m in _db.SS_TrafficData
        //                          where m.TrafficPlattform_Id == 417 && m.Product_Id == product.Id
        //                          select m;
        //        var ProductRow = from m in source_data
        //                         group m by m.Update into g
        //                         select g;
        //        int[] EndCount = new int[ProductRow.Count()];
        //        int i = 0;
        //        foreach (var sourceData in source_data)
        //        {
        //            IRow single_row = sheet.CreateRow(row_pos);
        //            cell_pos = 0;
        //            var c0 = single_row.CreateCell(cell_pos);
        //            c0.SetCellValue(sourceData.Update.ToString("d"));
        //            c0.CellStyle = Center_style;
        //            single_row.CreateCell(++cell_pos).SetCellValue(sourceData.SS_TrafficSource.TrafficSource_Name);
        //            single_row.CreateCell(++cell_pos).SetCellValue(sourceData.Product_Customer);
        //            if (sourceData.Product_Visitor == 0)
        //            {
        //                var ConvertRatio = (decimal)0;
        //                single_row.CreateCell(++cell_pos).SetCellValue(ConvertRatio.ToString("p2"));
        //            }
        //            else
        //            {
        //                var ConvertRatio = (decimal)sourceData.Product_Customer / sourceData.Product_Visitor;
        //                single_row.CreateCell(++cell_pos).SetCellValue(ConvertRatio.ToString("p2"));
        //            }
        //            row_pos++;

        //        }
        //        foreach (var productrow in ProductRow)
        //        {
        //            var DataCount = from m in productrow
        //                            select m;
        //            var datacount = DataCount.Count();
        //            if (datacount > 1 && firstProduct == true)
        //            {
        //                var row0 = 1;
        //                var row1 = datacount;
        //                EndCount[i] = row1;
        //                sheet.AddMergedRegion(new CellRangeAddress(row0, row1, 0, 0));
        //                firstProduct = false;
        //            }
        //            else
        //            {
        //                var row2 = EndCount[i - 1] + 1;
        //                var row3 = datacount + row2 - 1;
        //                EndCount[i] = row3;
        //                sheet.AddMergedRegion(new CellRangeAddress(row2, row3, 0, 0));
        //            }
        //            i++;
        //        }
        //    }
        //    MemoryStream _stream = new MemoryStream();
        //    book.Write(_stream);
        //    _stream.Flush();
        //    _stream.Seek(0, SeekOrigin.Begin);
        //    return File(_stream, "application/vnd.ms-excel", DateTime.Now.ToString("yyyyMMddHHmmss") + "渠道爆款统计表.xls");
        //}


         //11
        public ActionResult JD_Traffic(int plattformId)
        {
            return View();
        }

        private bool Read_TrafficFile(int plattformId, string filename, DateTime date, string plattformName)
        {
            AliOSSUtilities util = new AliOSSUtilities();
            StreamReader reader = new StreamReader(util.GetObject("ExcelUpload/" + filename), System.Text.Encoding.GetEncoding("GB2312"), false);
            CsvReader csv_reader = new CsvReader(reader);
            List<string> headers = new List<string>();
            var traffic_plattform = _db.SS_TrafficPlattform.SingleOrDefault(m => m.TrafficPlattform_Name == plattformName);
            var other_source = _db.SS_TrafficSource.SingleOrDefault(m => m.Source_Type == 0);
            while (csv_reader.Read())
            {
                try
                {
                    string date_flow = csv_reader.GetField<string>("日期");
                    if (date_flow == "")
                    {
                        break;
                    }
                    if (traffic_plattform == null)
                    {
                        traffic_plattform = new SS_TrafficPlattform()
                        {
                            TrafficPlattform_Name = plattformName,
                            Plattform_Id = 1,
                            AttendTrafficSource = new List<SS_TrafficSource>()
                        };
                        _db.SS_TrafficPlattform.Add(traffic_plattform);
                        _db.SaveChanges();
                    }
                    
                    string source_name = csv_reader.GetField<string>("流量渠道");
                    var traffic_source = _db.SS_TrafficSource.SingleOrDefault(m => m.TrafficSource_Name == source_name && m.Source_Type == 1 );
                    if (traffic_source == null)
                    {
                        traffic_source = other_source;
                    }
                    string s_code;
                    string system_code = csv_reader.TryGetField<string>("商品编码", out s_code) ? s_code : "NaN";
                    var product = _db.SS_Product.SingleOrDefault(m => m.System_Code == system_code);
                    if (product != null)
                    {
                        var traffic_data = _db.SS_TrafficData.SingleOrDefault(m => m.Update == date && m.TrafficPlattform_Id == traffic_plattform.Id && m.TrafficSource_Id == traffic_source.Id && m.Product_Id == product.Id);
                        int p_flow, p_visitor, p_customer, o_count;
                        double p_times;
                        if (traffic_data == null)
                        {
                            traffic_data = new SS_TrafficData()
                            {
                                Update = date,
                                Product_Flow = csv_reader.TryGetField<int>("商品流量", out p_flow) ? p_flow : 0,
                                Product_Visitor = csv_reader.TryGetField<int>("商品访客", out p_visitor) ? p_visitor : 0,
                                Product_VisitTimes = csv_reader.TryGetField<double>("商品访次", out p_times) ? p_times : 0,
                                Product_Customer = csv_reader.TryGetField<int>("商品消费者", out p_customer) ? p_customer : 0,
                                Order_Count = csv_reader.TryGetField<int>("商品订单行", out o_count) ? o_count : 0,
                                Product_Id = product.Id,
                                TrafficSource_Id = traffic_source.Id,
                                SS_TrafficPlattform = traffic_plattform
                            };
                            _db.SS_TrafficData.Add(traffic_data);
                        }
                        else
                        {
                            traffic_data.Update = date;
                            traffic_data.Product_Flow = csv_reader.TryGetField<int>("商品流量", out p_flow) ? p_flow : 0;
                            traffic_data.Product_Visitor = csv_reader.TryGetField<int>("商品访客", out p_visitor) ? p_visitor : 0;
                            traffic_data.Product_VisitTimes = csv_reader.TryGetField<double>("商品访次", out p_times) ? p_times : 0;
                            traffic_data.Product_Customer = csv_reader.TryGetField<int>("商品消费者", out p_customer) ? p_customer : 0;
                            traffic_data.Order_Count = csv_reader.TryGetField<int>("商品订单行", out o_count) ? o_count : 0;
                            traffic_data.Product_Id = product.Id;
                            traffic_data.TrafficSource_Id = traffic_source.Id;
                            traffic_data.SS_TrafficPlattform = traffic_plattform;
                            _db.Entry(traffic_data).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            var upload_traffic = _db.SS_UploadTraffic.SingleOrDefault(m => m.TrafficPlattform_Id == traffic_plattform.Id && m.Traffic_Date == date);
            if (upload_traffic != null)
            {
                upload_traffic.Upload_Date = DateTime.Now;
                _db.Entry(upload_traffic).State = System.Data.Entity.EntityState.Modified;
            }
            else
            {
                upload_traffic = new SS_UploadTraffic()
                {
                    TrafficPlattform_Id = traffic_plattform.Id,
                    Upload_Date = DateTime.Now,
                    Traffic_Date = date
                };
                _db.SS_UploadTraffic.Add(upload_traffic);
            }
            _db.SaveChanges();
            return true;
        }
        [HttpPost]
        public ActionResult UploadTrafficFile(FormCollection form, int plattformId, string plattformName)
        {
            var file = Request.Files[0];
            if (file != null)
            {
                var fileName = DateTime.Now.Ticks + ".csv";
                AliOSSUtilities util = new AliOSSUtilities();
                util.PutObject(file.InputStream, "ExcelUpload/" + fileName);
                var date_time = form["file-date"].ToString();
                if (plattformId == 1)
                {
                    var result = Read_TrafficFile(plattformId, fileName, Convert.ToDateTime(date_time), plattformName);
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
            else
            {
                return Json(new { result = "FAIL" });
            }
        }
        [HttpPost]
        public ActionResult getTrafficExcel(FormCollection form, DateTime date)
        {
            HSSFWorkbook book = getTrafficPlattform(form, date);
            ISheet sheet = book.CreateSheet(date.Month + "." + date.Day + "汇总");
            // 写标题
            IRow row = sheet.CreateRow(0);
            int cell_pos = 0;
            row.CreateCell(cell_pos).SetCellValue("日期");
            row.CreateCell(++cell_pos).SetCellValue("商品编码");
            row.CreateCell(++cell_pos).SetCellValue("商品名称");
            row.CreateCell(++cell_pos).SetCellValue("流量渠道");
            row.CreateCell(++cell_pos).SetCellValue("商品流量");
            row.CreateCell(++cell_pos).SetCellValue("商品访客");
            row.CreateCell(++cell_pos).SetCellValue("商品消费者");
            row.CreateCell(++cell_pos).SetCellValue("商品订单行");
            row.CreateCell(++cell_pos).SetCellValue("商品转化率");
            // 写产品列
            int row_pos = 1;
            var productlist = from m in _db.SS_TrafficData
                              group m by m.SS_Product into g
                              select g;
            ICellStyle Center_style = book.CreateCellStyle();//居中标题
            Center_style.VerticalAlignment = VerticalAlignment.Center;//垂直对齐
            foreach (var product in productlist)
            {
                var productOrder = from m in product
                                   where m.Update == date
                                   group m by m.SS_TrafficPlattform into g
                                   select g;
                bool firstCount = true;
                foreach (var productorder in productOrder)
                {
                    IRow single_row = sheet.CreateRow(row_pos);
                    cell_pos = 0;
                    var c0 = single_row.CreateCell(cell_pos);
                    c0.SetCellValue(date.ToString("d"));
                    var c1 = single_row.CreateCell(++cell_pos);
                    c1.SetCellValue(product.Key.System_Code);
                    var c2 = single_row.CreateCell(++cell_pos);
                    c2.SetCellValue(product.Key.Item_Name);
                    var rowCount = productOrder.Count();
                    if (rowCount > 1 && firstCount == true)
                    {
                        var row0 = row_pos;
                        var row1 = row_pos + rowCount - 1;
                        sheet.AddMergedRegion(new CellRangeAddress(row0, row1, 0, 0));
                        sheet.AddMergedRegion(new CellRangeAddress(row0, row1, 1, 1));
                        sheet.AddMergedRegion(new CellRangeAddress(row0, row1, 2, 2));
                        c0.CellStyle = Center_style;
                        c1.CellStyle = Center_style;
                        c2.CellStyle = Center_style;
                        firstCount = false;
                    }
                    single_row.CreateCell(++cell_pos).SetCellValue(productorder.Key.TrafficPlattform_Name);
                    var Ratio = (decimal)productorder.Sum(m => m.Product_Customer) / (productorder.Sum(m => m.Product_Visitor) == 0 ? 1 : productorder.Sum(m => m.Product_Visitor));
                    single_row.CreateCell(++cell_pos).SetCellValue(productorder.Sum(m => m.Product_Flow));
                    single_row.CreateCell(++cell_pos).SetCellValue(productorder.Sum(m => m.Product_Visitor));
                    single_row.CreateCell(++cell_pos).SetCellValue(productorder.Sum(m => m.Product_Customer));
                    single_row.CreateCell(++cell_pos).SetCellValue(productorder.Sum(m => m.Order_Count));
                    single_row.CreateCell(++cell_pos).SetCellValue(Ratio.ToString("p2"));
                    row_pos++;
                }
                var product_count = from m in product
                                    where m.Update == date
                                    group m by m.Update into g
                                    select g;
                foreach (var productCount in product_count)
                {
                    IRow single_row1 = sheet.CreateRow(row_pos);
                    cell_pos = 4;
                    var Ratio = (decimal)productCount.Sum(m => m.Product_Customer) / (productCount.Sum(m => m.Product_Visitor) == 0 ? 1 : productCount.Sum(m => m.Product_Visitor));
                    single_row1.CreateCell(cell_pos).SetCellValue(productCount.Sum(m => m.Product_Flow));
                    single_row1.CreateCell(++cell_pos).SetCellValue(productCount.Sum(m => m.Product_Visitor));
                    single_row1.CreateCell(++cell_pos).SetCellValue(productCount.Sum(m => m.Product_Customer));
                    single_row1.CreateCell(++cell_pos).SetCellValue(productCount.Sum(m => m.Order_Count));
                    single_row1.CreateCell(++cell_pos).SetCellValue(Ratio.ToString("p2"));
                }
                row_pos++;
            }
            row_pos++;
            // 各平台总和
            var data_each = from m in _db.SS_TrafficData
                            where m.Update == date
                            group m by m.SS_TrafficPlattform into g
                            select g;
            foreach (var eachData in data_each)
            {
                IRow single_row = sheet.CreateRow(row_pos);
                cell_pos = 3;
                var Ratio = (decimal)eachData.Sum(m => m.Product_Customer) / (eachData.Sum(m => m.Product_Visitor) == 0 ? 1 : eachData.Sum(m => m.Product_Visitor));
                single_row.CreateCell(cell_pos).SetCellValue(eachData.Key.TrafficPlattform_Name);
                single_row.CreateCell(++cell_pos).SetCellValue(eachData.Sum(m => m.Product_Flow));
                single_row.CreateCell(++cell_pos).SetCellValue(eachData.Sum(m => m.Product_Visitor));
                single_row.CreateCell(++cell_pos).SetCellValue(eachData.Sum(m => m.Product_Customer));
                single_row.CreateCell(++cell_pos).SetCellValue(eachData.Sum(m => m.Order_Count));
                single_row.CreateCell(++cell_pos).SetCellValue(Ratio.ToString("p2"));
                row_pos++;
            }
            // 平台总和
            var data_all = from m in _db.SS_TrafficData
                           where m.Update == date
                           group m by m.SS_TrafficPlattform.Plattform_Id into g
                           select g;
            foreach (var allData in data_all)
            {
                IRow single_row = sheet.CreateRow(++row_pos);
                cell_pos = 3;
                var Ratio = (decimal)allData.Sum(m => m.Product_Customer) / (allData.Sum(m => m.Product_Visitor) == 0 ? 1 : allData.Sum(m => m.Product_Visitor));
                single_row.CreateCell(cell_pos).SetCellValue("共计：");
                single_row.CreateCell(++cell_pos).SetCellValue(allData.Sum(m => m.Product_Flow));
                single_row.CreateCell(++cell_pos).SetCellValue(allData.Sum(m => m.Product_Visitor));
                single_row.CreateCell(++cell_pos).SetCellValue(allData.Sum(m => m.Product_Customer));
                single_row.CreateCell(++cell_pos).SetCellValue(allData.Sum(m => m.Order_Count));
                single_row.CreateCell(++cell_pos).SetCellValue(Ratio.ToString("p2"));
            }
            MemoryStream _stream = new MemoryStream();
            book.Write(_stream);
            _stream.Flush();
            _stream.Seek(0, SeekOrigin.Begin);
            return File(_stream, "application/vnd.ms-excel", DateTime.Now.ToString("yyyyMMddHHmmss") + "统计表.xls");
        }
        [HttpPost]
        public HSSFWorkbook getTrafficPlattform(FormCollection form, DateTime date)
        {
            var TrafficPlattform = from m in _db.SS_TrafficPlattform
                                   select m;
            HSSFWorkbook book = new HSSFWorkbook();
            foreach (var plattform in TrafficPlattform)
            {
                var sheetName = plattform.TrafficPlattform_Name;
                ISheet sheet = book.CreateSheet(sheetName);
                // 写标题
                IRow row = sheet.CreateRow(0);
                int cell_pos = 0;
                row.CreateCell(cell_pos).SetCellValue("日期");
                row.CreateCell(++cell_pos).SetCellValue("商品编码");
                row.CreateCell(++cell_pos).SetCellValue("商品名称");
                row.CreateCell(++cell_pos).SetCellValue("流量渠道");
                row.CreateCell(++cell_pos).SetCellValue("商品流量");
                row.CreateCell(++cell_pos).SetCellValue("商品访客");
                row.CreateCell(++cell_pos).SetCellValue("商品访次");
                row.CreateCell(++cell_pos).SetCellValue("商品消费者");
                row.CreateCell(++cell_pos).SetCellValue("商品订单行");
                row.CreateCell(++cell_pos).SetCellValue("商品转化率");
                // 写产品列
                int row_pos = 1;
                var TrafficData = from m in _db.SS_TrafficData
                                  where m.SS_TrafficPlattform.Id == plattform.Id && m.Update == date
                                  orderby m.Product_Id
                                  select m;
                foreach (var trafficdata in TrafficData)
                {
                    IRow single_row = sheet.CreateRow(row_pos);
                    cell_pos = 0;
                    single_row.CreateCell(cell_pos).SetCellValue(date.ToString("d"));
                    single_row.CreateCell(++cell_pos).SetCellValue(trafficdata.SS_Product.System_Code);
                    single_row.CreateCell(++cell_pos).SetCellValue(trafficdata.SS_Product.Item_Name);
                    single_row.CreateCell(++cell_pos).SetCellValue(trafficdata.SS_TrafficSource.TrafficSource_Name);
                    single_row.CreateCell(++cell_pos).SetCellValue(trafficdata.Product_Flow);
                    single_row.CreateCell(++cell_pos).SetCellValue(trafficdata.Product_Visitor);
                    single_row.CreateCell(++cell_pos).SetCellValue(trafficdata.Product_VisitTimes);
                    single_row.CreateCell(++cell_pos).SetCellValue(trafficdata.Product_Customer);
                    single_row.CreateCell(++cell_pos).SetCellValue(trafficdata.Order_Count);
                    if (trafficdata.Product_Visitor == 0)
                    {
                        var ConvertRatio = (decimal)0;
                        single_row.CreateCell(++cell_pos).SetCellValue(ConvertRatio.ToString("p2"));
                    }
                    else
                    {
                        var ConvertRatio = (decimal)trafficdata.Product_Customer / trafficdata.Product_Visitor;
                        single_row.CreateCell(++cell_pos).SetCellValue(ConvertRatio.ToString("p2"));
                    }
                    row_pos++;
                }
            }
            MemoryStream _stream = new MemoryStream();
            book.Write(_stream);
            _stream.Flush();
            _stream.Seek(0, SeekOrigin.Begin);
            return book;
        }
        // 爆款统计
        [HttpPost]
        public ActionResult getHotExcel(FormCollection form)
        {
            var product_list = from m in _db.SS_Product
                               where m.Product_Type == 1 && m.Plattform_Id == 1
                               select m;
            HSSFWorkbook book = new HSSFWorkbook();
            foreach (var product in product_list)
            {
                var sheetName = product.Item_Name;
                ISheet sheet = book.CreateSheet(sheetName);
                IRow row0 = sheet.CreateRow(0);
                int cell_pos = 0;
                row0.Height = 70 * 20;
                row0.CreateCell(cell_pos).SetCellValue("活动备注");
                IRow row = sheet.CreateRow(1);
                row.CreateCell(++cell_pos).SetCellValue(product.Item_Name + product.System_Code);
                row.CreateCell(++cell_pos).SetCellValue("访客");
                row.CreateCell(++cell_pos).SetCellValue("订单");
                row.CreateCell(++cell_pos).SetCellValue("商品数量");
                row.CreateCell(++cell_pos).SetCellValue("客件数");
                row.CreateCell(++cell_pos).SetCellValue("转化率");
                row.CreateCell(++cell_pos).SetCellValue("销售成本");
                row.CreateCell(++cell_pos).SetCellValue("uv价值");
                var data_date = from m in _db.SS_TrafficData
                                where m.Product_Id == product.Id
                                group m by m.Update into g
                                select g;
                var row_pos = 2;
                foreach (var dataDate in data_date)
                {
                    var dateTime = Convert.ToDateTime(dataDate.Key.Year + "/" + dataDate.Key.Month + "/" + dataDate.Key.Day + " 0:00:00");
                    var traffic_data = from m in dataDate
                                       group m by m.SS_TrafficPlattform.Plattform_Id into g
                                       select g;
                    foreach (var trafficData in traffic_data)
                    {
                        var sales_date = from m in _db.SS_SalesRecord
                                         where m.Product_Id == product.Id && m.SalesRecord_Date == dateTime && m.SS_Storage.Storage_Type == 1
                                         group m by m.Product_Id into g
                                         select g;
                        foreach (var salesDate in sales_date)
                        {
                            IRow single_row = sheet.CreateRow(row_pos);
                            cell_pos = 1;
                            var Count = (double)salesDate.Sum(m => m.Sales_Count) / trafficData.Sum(m => m.Order_Count);
                            var Ratio = (double)trafficData.Sum(m => m.Order_Count) / trafficData.Sum(m => m.Product_Flow);
                            var Cost = salesDate.Sum(m => m.Sales_Count) * product.Purchase_Price;
                            var uvValue = salesDate.Sum(m => m.Sales_Count) * product.Purchase_Price / trafficData.Sum(m => m.Product_Visitor);
                            single_row.CreateCell(cell_pos).SetCellValue(dataDate.Key.Month + "." + dataDate.Key.Day);
                            single_row.CreateCell(++cell_pos).SetCellValue(trafficData.Sum(m => m.Product_Visitor));
                            single_row.CreateCell(++cell_pos).SetCellValue(trafficData.Sum(m => m.Order_Count));
                            single_row.CreateCell(++cell_pos).SetCellValue(salesDate.Sum(m => m.Sales_Count));
                            single_row.CreateCell(++cell_pos).SetCellValue(Math.Round(Count, 2));
                            single_row.CreateCell(++cell_pos).SetCellValue(Ratio.ToString("p2"));
                            single_row.CreateCell(++cell_pos).SetCellValue(Cost.ToString("0.##"));
                            single_row.CreateCell(++cell_pos).SetCellValue(uvValue.ToString("0.00"));
                            row_pos++;
                        }
                    }

                }
            }
            MemoryStream _stream = new MemoryStream();
            book.Write(_stream);
            _stream.Flush();
            _stream.Seek(0, SeekOrigin.Begin);
            return File(_stream, "application/vnd.ms-excel", DateTime.Now.ToString("yyyyMMddHHmmss") + "爆款统计表.xls");
        }

        public ActionResult Temp_Action()
        {
            var trafficsourcelist = _db.SS_TrafficSource;
            var plattformlist = _db.SS_TrafficPlattform;
            foreach (var item in trafficsourcelist)
            {
                foreach (var plattform in plattformlist)
                {
                    item.AttendTrafficPlattform.Add(plattform);
                }
            }
            _db.SaveChanges();
            return Content("SUCCESS");
        }

        // TrafficList
        public ActionResult TrafficList(int plattformId,int productId)
        {
            var productList = from m in _db.SS_Product
                              where m.Plattform_Id == plattformId && m.Id == productId
                              select m;
            ViewBag.productList = productList;
            var trafficDate = from m in _db.SS_TrafficData
                              where m.SS_TrafficPlattform.Plattform_Id == plattformId && m.Product_Id == productId
                              orderby m.Update descending
                              group m by m.Update into g
                              select g.Key;
            ViewBag.trafficDate = trafficDate;
            return PartialView();
        }
        
        public ActionResult TrafficListPartial(string query, int plattformId, int productId, int trafficPlattformId, DateTime? single)
        {
            if (trafficPlattformId == 1)
            {
                if (query != "")
                {
                    var productlist = (from m in _db.SS_TrafficData
                                       where m.SS_TrafficSource.TrafficSource_Name.Contains(query) && m.Product_Id == productId && m.Update == single && m.SS_Product.Plattform_Id == plattformId
                                       group m by m.SS_TrafficSource into g
                                       orderby g.Sum(m => m.Product_Visitor) descending
                                       select new TrafficData
                                       {
                                           Date_Source = g.Key.TrafficSource_Name,
                                           Date_Source_Id = g.Key.Id,
                                           Product_Flow = g.Sum(m => m.Product_Flow),
                                           Product_Visitor = g.Sum(m => m.Product_Visitor),
                                           Product_Customer = g.Sum(m => m.Product_Customer),
                                           Order_Count = g.Sum(m => m.Order_Count)
                                       });
                    return PartialView(productlist);
                }
                else
                {
                    var productlist = (from m in _db.SS_TrafficData
                                       where m.SS_Product.Plattform_Id == plattformId && m.Product_Id == productId && m.Update == single
                                       group m by m.SS_TrafficSource into g
                                       orderby g.Sum(m => m.Product_Visitor) descending
                                       select new TrafficData
                                       {
                                           Date_Source = g.Key.TrafficSource_Name,
                                           Date_Source_Id = g.Key.Id,
                                           Product_Flow = g.Sum(m => m.Product_Flow),
                                           Product_Visitor = g.Sum(m => m.Product_Visitor),
                                           Product_Customer = g.Sum(m => m.Product_Customer),
                                           Order_Count = g.Sum(m => m.Order_Count)
                                       });
                    return PartialView(productlist);
                }
            }
            else
            {
                if (query != "")
                {
                    var productlist = (from m in _db.SS_TrafficData
                                       where m.SS_TrafficSource.TrafficSource_Name.Contains(query) && m.Product_Id == productId && m.Update == single && m.SS_Product.Plattform_Id == plattformId && m.TrafficPlattform_Id == trafficPlattformId
                                       group m by m.SS_TrafficSource into g
                                       orderby g.Sum(m => m.Product_Visitor) descending
                                       select new TrafficData
                                       {
                                           Date_Source = g.Key.TrafficSource_Name,
                                           Date_Source_Id = g.Key.Id,
                                           Product_Flow = g.Sum(m => m.Product_Flow),
                                           Product_Visitor = g.Sum(m => m.Product_Visitor),
                                           Product_Customer = g.Sum(m => m.Product_Customer),
                                           Order_Count = g.Sum(m => m.Order_Count)
                                       });
                    return PartialView(productlist);
                }
                else
                {
                    var productlist = (from m in _db.SS_TrafficData
                                       where m.Product_Id == productId && m.Update == single && m.SS_Product.Plattform_Id == plattformId && m.TrafficPlattform_Id == trafficPlattformId
                                       group m by m.SS_TrafficSource into g
                                       orderby g.Sum(m => m.Product_Visitor) descending
                                       select new TrafficData
                                       {
                                           Date_Source = g.Key.TrafficSource_Name,
                                           Date_Source_Id = g.Key.Id,
                                           Product_Flow = g.Sum(m => m.Product_Flow),
                                           Product_Visitor = g.Sum(m => m.Product_Visitor),
                                           Product_Customer = g.Sum(m => m.Product_Customer),
                                           Order_Count = g.Sum(m => m.Order_Count)
                                       });
                    return PartialView(productlist);
                }
            }

        }

        public ActionResult SumTrafficListPartial(int? page, string query, int plattformId, int productId, int trafficPlattformId, DateTime start,DateTime end)
        {
            int _page = page ?? 1;
            if (trafficPlattformId == 1)
            {
                if (query != "")
                {
                    var productlist = (from m in _db.SS_TrafficData
                                       where m.SS_TrafficSource.TrafficSource_Name.Contains(query) && m.Product_Id == productId && m.Update >= start && m.Update <= end && m.SS_Product.Plattform_Id == plattformId
                                       group m by m.SS_TrafficSource.TrafficSource_Name into g
                                       orderby g.Sum(m=>m.Product_Visitor) descending
                                       select new TrafficData {
                                           Date_Source=g.Key,
                                           Product_Flow = g.Sum(m => m.Product_Flow),
                                           Product_Visitor = g.Sum(m => m.Product_Visitor),
                                           Product_Customer = g.Sum(m => m.Product_Customer),
                                           Order_Count = g.Sum(m => m.Order_Count)
                                       }).ToPagedList(_page, 15);
                    return PartialView(productlist);
                }
                else
                {
                    var productlist = (from m in _db.SS_TrafficData
                                       where m.SS_Product.Plattform_Id == plattformId && m.Product_Id == productId && m.Update >= start && m.Update <= end
                                       group m by m.SS_TrafficSource.TrafficSource_Name into g
                                       orderby g.Sum(m => m.Product_Visitor) descending
                                       select new TrafficData
                                       {
                                           Date_Source = g.Key,
                                           Product_Flow = g.Sum(m => m.Product_Flow),
                                           Product_Visitor = g.Sum(m => m.Product_Visitor),
                                           Product_Customer = g.Sum(m => m.Product_Customer),
                                           Order_Count = g.Sum(m => m.Order_Count)
                                       }).ToPagedList(_page, 15);
                    return PartialView(productlist);
                }
            }
            else
            {
                if (query != "")
                {
                    var productlist = (from m in _db.SS_TrafficData
                                       where m.SS_TrafficSource.TrafficSource_Name.Contains(query) && m.Product_Id == productId && m.Update >= start && m.Update <= end && m.SS_Product.Plattform_Id == plattformId && m.TrafficPlattform_Id == trafficPlattformId
                                       group m by m.SS_TrafficSource.TrafficSource_Name into g
                                       orderby g.Sum(m => m.Product_Visitor) descending
                                       select new TrafficData
                                       {
                                           Date_Source = g.Key,
                                           Product_Flow = g.Sum(m => m.Product_Flow),
                                           Product_Visitor = g.Sum(m => m.Product_Visitor),
                                           Product_Customer = g.Sum(m => m.Product_Customer),
                                           Order_Count = g.Sum(m => m.Order_Count)
                                       }).ToPagedList(_page, 15);
                    return PartialView(productlist);
                }
                else
                {
                    var productlist = (from m in _db.SS_TrafficData
                                       where m.Product_Id == productId && m.Update >= start && m.Update <= end && m.SS_Product.Plattform_Id == plattformId && m.TrafficPlattform_Id == trafficPlattformId
                                       group m by m.SS_TrafficSource.TrafficSource_Name into g
                                       orderby g.Sum(m => m.Product_Visitor) descending
                                       select new TrafficData
                                       {
                                           Date_Source = g.Key,
                                           Product_Flow = g.Sum(m => m.Product_Flow),
                                           Product_Visitor = g.Sum(m => m.Product_Visitor),
                                           Product_Customer = g.Sum(m => m.Product_Customer),
                                           Order_Count = g.Sum(m => m.Order_Count)
                                       }).ToPagedList(_page, 15);
                    return PartialView(productlist);
                }
            }
        }

        //新增渠道
        public ActionResult AddTrafficSource()
        {
            var TrafficPlattform = from m in _db.SS_TrafficPlattform
                                   select m;
            ViewBag.TrafficPlattform = new SelectList(TrafficPlattform ,"TrafficPlattform_Name");
            var SourceType = (from m in _db.SS_TrafficSource
                              orderby m.Id descending
                             select m.Source_Type).Distinct();
            ViewBag.SourceType = new SelectList(SourceType, "Source_Type", "233");
            return PartialView();
        }
        [HttpPost]
        public ActionResult AddTrafficSource(SS_TrafficSource model,FormCollection form)
        {
            if (ModelState.IsValid)
            {
                var item = new SS_TrafficSource();
                item.TrafficSource_Name = model.TrafficSource_Name;
                item.TrafficPlattform_Id = model.TrafficPlattform_Id;
                item.Source_Type = model.Source_Type;
                _db.SS_TrafficSource.Add(item);
                _db.SaveChanges();
                return Content("SUCCESS");
            }
            else
            {
                return PartialView(model);
            }
        }

        //产品数据图表
        public ActionResult ViewTrafficStatistic(int productId,int sourceId,int trafficPlattformId)
        {
            if(trafficPlattformId == 1)
            {
                var TrafficList = from m in _db.SS_TrafficData
                                  where m.Product_Id == productId && m.TrafficSource_Id == sourceId
                                  select m;
                ViewBag.TrafficList = TrafficList;
            }
            else
            {
                var TrafficList = from m in _db.SS_TrafficData
                                  where m.Product_Id == productId && m.TrafficSource_Id == sourceId && m.TrafficPlattform_Id == trafficPlattformId
                                  select m;
                ViewBag.TrafficList = TrafficList;
            }
            
            return View();
        }

        public JsonResult ViewTrafficStatisticPartial(int productId, string start, string end, int sourceId,int trafficPlattformId)
        {
            DateTime _start = Convert.ToDateTime(start);
            DateTime _end = Convert.ToDateTime(end);
            if(trafficPlattformId == 1)
            {
                var info_data = from m in _db.SS_TrafficData
                                where m.Update >= _start && m.Update <= _end
                                && m.Product_Id == productId && m.TrafficSource_Id == sourceId
                                group m by m.Update into g
                                orderby g.Key
                                select new TrafficStatisticViewModel { salesdate = g.Key, productvisitor = g.Sum(m => m.Product_Visitor), productcustomer = g.Sum(m => m.Product_Customer) };
                DateTime current_date = _start;
                var data = new List<TrafficStatisticViewModel>();
                while (current_date <= _end)
                {
                    int _productvisitor = 0;
                    int _productcustomer = 0;
                    var item = info_data.SingleOrDefault(m => m.salesdate == current_date);
                    if (item != null)
                    {
                        _productvisitor = item.productvisitor;
                        _productcustomer = item.productcustomer;
                    }
                    data.Add(new TrafficStatisticViewModel()
                    {
                        productvisitor = _productvisitor,
                        salesdate = current_date,
                        productcustomer = _productcustomer
                    });
                    current_date = current_date.AddDays(1);
                }
                return Json(new { result = "SUCCESS", data = data });
            }
            else
            {
                var info_data = from m in _db.SS_TrafficData
                                where m.Update >= _start && m.Update <= _end
                                && m.Product_Id == productId && m.TrafficSource_Id == sourceId && m.TrafficPlattform_Id == trafficPlattformId
                                group m by m.Update into g
                                orderby g.Key
                                select new TrafficStatisticViewModel { salesdate = g.Key, productvisitor = g.Sum(m => m.Product_Visitor), productcustomer = g.Sum(m => m.Product_Customer) };
                DateTime current_date = _start;
                var data = new List<TrafficStatisticViewModel>();
                while (current_date <= _end)
                {
                    int _productvisitor = 0;
                    int _productcustomer = 0;
                    var item = info_data.SingleOrDefault(m => m.salesdate == current_date);
                    if (item != null)
                    {
                        _productvisitor = item.productvisitor;
                        _productcustomer = item.productcustomer;
                    }
                    data.Add(new TrafficStatisticViewModel()
                    {
                        productvisitor = _productvisitor,
                        salesdate = current_date,
                        productcustomer = _productcustomer
                    });
                    current_date = current_date.AddDays(1);
                }
                return Json(new { result = "SUCCESS", data = data });
            }
        }
        // 统计上传日期
        [HttpPost]
        public ActionResult TrafficUploadFilePartial(int plattformId, string month)
        {
            DateTime start = Convert.ToDateTime(month);
            DateTime end = start.AddMonths(1);
            var list = from m in _db.SS_UploadTraffic
                       where m.Traffic_Date >= start && m.Traffic_Date < end
                       && m.SS_TrafficPlattform.Plattform_Id == plattformId
                       select new { record_date = m.Traffic_Date };
            return Json(list);
        }
    }
}

