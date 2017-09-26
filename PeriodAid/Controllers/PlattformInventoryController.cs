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
using System.Web.Script.Serialization;

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
        private bool Read_InsertFile(string filename, DateTime date)
        {
            string folder = HttpContext.Server.MapPath("~/Content/xlsx/");
            StreamReader reader = new StreamReader(folder + filename, System.Text.Encoding.GetEncoding("GB2312"), false);
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
                        }
                    }
                    row_count++;
                }catch(Exception)
                {
                    return false;
                }
            }
            _db.SaveChanges();
            return true;
        }
        // 获取仓库表格
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
            foreach(var product in product_list)
            {
                NPOI.SS.UserModel.IRow single_row = sheet.CreateRow(row_pos);
                cell_pos = 0;
                single_row.CreateCell(cell_pos).SetCellValue(product.System_Code);
                cell_pos++;
                single_row.CreateCell(cell_pos).SetCellValue(product.Item_Name);
                cell_pos++;
                DateTime current_date = new DateTime(2017,9,25);
                DateTime first_date = product.Inventory_Date.AddDays(-30);
                foreach (var inventory in inventory_list.OrderBy(m=>m.Id))
                {
                    // 最新库存
                    var last_inventory = _db.SS_SalesRecord.SingleOrDefault(m => m.Product_Id == product.Id && m.Storage_Id == inventory.Id && m.SalesRecord_Date == current_date);
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
                                              select m).Sum(m=>m.Sales_Count);
                    int recommand_storage = (period_sales_count - storage_count) > 0 ? (period_sales_count - storage_count) : 0;
                    single_row.CreateCell(cell_pos).SetCellValue(recommand_storage);
                    cell_pos++;
                }
                row_pos++;
            }

            MemoryStream _stream = new MemoryStream();
            book.Write(_stream);
            _stream.Flush();
            _stream.Seek(0, SeekOrigin.Begin);
            return File(_stream, "application/ms-excel", DateTime.Now.ToString("yyyyMMddHHmmss")+"库存表.xls");


        }

        /*public ActionResult LeadingIn() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HandleError(View = "~/Views/Shared/Error.cshtml")]
        public ActionResult Browse(HttpPostedFileBase file)
        {

            if (string.Empty.Equals(file.FileName) || ".xlsx" != Path.GetExtension(file.FileName))
            {
                throw new ArgumentException("当前文件格式不正确,请确保正确的Excel文件格式!");
            }

            var severPath = this.Server.MapPath("/files/"); //获取当前虚拟文件路径

            var savePath = Path.Combine(severPath, file.FileName); //拼接保存文件路径

            try
            {
                file.SaveAs(savePath);
                stus = ExcelHelper.ReadExcelToEntityList<Student>(savePath);
                ViewBag.Data = stus;
                return View("Index");
            }
            finally
            {
                System.IO.File.Delete(savePath);//每次上传完毕删除文件
            }

        }

        [HandleError(View = "~/Views/Shared/Error.cshtml")]
        public ActionResult Upload() {
            return View("UploadSuccess");
        }*/


    }
    
}