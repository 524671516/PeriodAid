﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;

using PagedList;
using PeriodAid.Models;
using PeriodAid.Filters;
using PeriodAid.DAL;
using System.IO;
using CsvHelper;

namespace PeriodAid.Controllers
{
    [Authorize(Roles ="Admin")]
    public class OffSalesController : Controller
    {
        // GET: OffStore
        OfflineSales _offlineDB = new OfflineSales();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public OffSalesController()
        {

        }

        public OffSalesController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        }// GET: Sales
        public ActionResult Index()
        {
            return View();
        }

        // Origin:Off_Sales_main
        public ActionResult DailySalesIndex()
        {
            return View();
        }

        // Origin:Off_DailySalesInfo_ajaxlist
        public ActionResult DailySalesListPartial(int? page, string query)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            int _page = page ?? 1;
            if (query == null || query == "")
            {
                var list = (from m in _offlineDB.Off_SalesInfo_Daily
                            where m.Off_Store.Off_System_Id == user.DefaultSystemId
                            orderby m.Date descending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
            else
            {
                var list = (from m in _offlineDB.Off_SalesInfo_Daily
                            where m.Off_Store.StoreName.Contains(query) && m.Off_Store.Off_System_Id == user.DefaultSystemId
                            orderby m.Date descending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
        }
        // Origin:Ajax_EditDailyInfo
        public PartialViewResult EditDailySalesPartial(int id)
        {
            var item = _offlineDB.Off_SalesInfo_Daily.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_Store.Off_System_Id == user.DefaultSystemId)
                {
                    var sellerlist = from m in _offlineDB.Off_Seller
                                     where m.StoreId == item.StoreId
                                     select m;
                    ViewBag.Sellerlist = new SelectList(sellerlist, "Id", "Name");
                    List<Object> attendance = new List<Object>();
                    attendance.Add(new { Key = 0, Value = "全勤" });
                    attendance.Add(new { Key = 1, Value = "迟到" });
                    attendance.Add(new { Key = 2, Value = "早退" });
                    attendance.Add(new { Key = 3, Value = "旷工" });
                    ViewBag.Attendancelist = new SelectList(attendance, "Key", "Value", item.Attendance);
                    return PartialView(item);
                }
                else
                {
                    return PartialView("AuthorizeErrorPartial");
                }
            }
            else
            {
                return PartialView("ErrorPartial");
            }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> EditDailySalesPartial(int id, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                var item = new Off_SalesInfo_Daily();
                if (TryUpdateModel(item))
                {
                    List<int> plist = new List<int>();
                    var user = UserManager.FindById(User.Identity.GetUserId());
                    var productlist = from m in _offlineDB.Off_Product
                                      where m.Off_System_Id == user.DefaultSystemId
                                      && m.status >= 0
                                      select m;
                    // 添加或修改销售列表
                    foreach (var product in productlist)
                    {
                        // 获取单品数据
                        int? sales = null;
                        if (form["sales_" + product.Id] != "")
                            sales = Convert.ToInt32(form["sales_" + product.Id]);
                        int? storage = null;
                        if (form["storage_" + product.Id] != "")
                            storage = Convert.ToInt32(form["storage_" + product.Id]);
                        decimal? amount = null;
                        if (form["amount_" + product.Id] != "")
                            amount = Convert.ToDecimal(form["amount_" + product.Id]);
                        // 判断是否已有数据
                        var checkinproductlist = _offlineDB.Off_Daily_Product.Where(m => m.DailyId == id);
                        var existdata = checkinproductlist.SingleOrDefault(m => m.ProductId == product.Id);
                        if (existdata != null)
                        {
                            if (sales == null && storage == null && amount == null)
                            {
                                // 无数据则删除
                                _offlineDB.Off_Daily_Product.Remove(existdata);
                            }
                            else if (sales == 0 && storage == 0 && amount == 0)
                            {
                                _offlineDB.Off_Daily_Product.Remove(existdata);
                            }
                            else
                            {
                                // 修改数据
                                existdata.SalesAmount = amount;
                                existdata.SalesCount = sales;
                                existdata.StorageCount = storage;
                            }
                        }
                        else
                        {
                            // 添加数据
                            // 如果三项数据不为空，则添加
                            if (sales == null && storage == null && amount == null)
                            { }
                            else if (sales == 0 && storage == 0 && amount == 0)
                            {

                            }
                            else
                            {
                                existdata = new Off_Daily_Product()
                                {
                                    DailyId = id,
                                    ItemCode = product.ItemCode,
                                    ProductId = product.Id,
                                    SalesAmount = amount,
                                    SalesCount = sales,
                                    StorageCount = storage
                                };
                                _offlineDB.Off_Daily_Product.Add(existdata);
                                //offlineDB.SaveChanges();
                            }
                        }
                    }
                    item.UploadTime = DateTime.Now;
                    item.UploadUser = User.Identity.Name;
                    _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    var result = await _offlineDB.SaveChangesAsync();
                    // 该代码需要重写

                    OfflineSalesUtilities util = new OfflineSalesUtilities();
                    var result2 = await util.UpdateDailySalesAvg(item.StoreId, (int)item.Date.DayOfWeek + 1);
                    //update_Sales_AVGINfo(item.StoreId, (int)item.Date.DayOfWeek + 1);
                    return Content("SUCCESS");
                }
                else
                {
                    return PartialView("AuthorizeErrorPartial");
                }
            }
            else
            {
                return PartialView("ErrorPartial");
            }
        }
        // Origin: Off_CreateSalesDaily
        public ActionResult CreateSalesDailyPartial()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var item = new Off_SalesInfo_Daily();
            var storelist = from m in _offlineDB.Off_Store
                            where m.Off_System_Id == user.DefaultSystemId
                            orderby m.StoreName
                            select new { Key = m.Id, Value = m.StoreName };
            ViewBag.StoreDropDown = new SelectList(storelist, "Key", "Value");
            return PartialView(item);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateSalesDailyPartial(Off_SalesInfo_Daily model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                Off_SalesInfo_Daily item = new Off_SalesInfo_Daily();
                if (TryUpdateModel(item))
                {
                    List<int> plist = new List<int>();
                    var user = UserManager.FindById(User.Identity.GetUserId());
                    var productlist = from m in _offlineDB.Off_Product
                                      where m.Off_System_Id == user.DefaultSystemId
                                      && m.status >= 0
                                      select m;
                    // 添加或修改销售列表
                    foreach (var product in productlist)
                    {
                        // 获取单品数据
                        int? sales = null;
                        if (form["sales_" + product.Id] != "")
                            sales = Convert.ToInt32(form["sales_" + product.Id]);
                        int? storage = null;
                        if (form["storage_" + product.Id] != "")
                            storage = Convert.ToInt32(form["storage_" + product.Id]);
                        decimal? amount = null;
                        if (form["amount_" + product.Id] != "")
                            amount = Convert.ToDecimal(form["amount_" + product.Id]);
                        // 判断是否已有数据

                        // 添加数据
                        // 如果三项数据不为空，则添加
                        if ((sales == null|| sales==0) && storage == null && amount == null)
                        { }
                        else
                        {
                            Off_Daily_Product existdata = new Off_Daily_Product()
                            {
                                Off_SalesInfo_Daily = item,
                                ItemCode = product.ItemCode,
                                ProductId = product.Id,
                                SalesAmount = amount,
                                SalesCount = sales,
                                StorageCount = storage
                            };
                            _offlineDB.Off_Daily_Product.Add(existdata);
                            //offlineDB.SaveChanges();
                        }
                    }
                    item.UploadTime = DateTime.Now;
                    item.UploadUser = User.Identity.Name;
                    _offlineDB.Off_SalesInfo_Daily.Add(item);
                    var result = await _offlineDB.SaveChangesAsync();
                    OfflineSalesUtilities util = new OfflineSalesUtilities();
                    var result2 = await util.UpdateDailySalesAvg(item.StoreId, (int)item.Date.DayOfWeek + 1);
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                var storelist = from m in _offlineDB.Off_Store
                                orderby m.StoreName
                                select new { Key = m.Id, Value = m.StoreName };
                ViewBag.StoreDropDown = new SelectList(storelist, "Key", "Value");
                return PartialView(model);
            }
        }
        // Origin: Off_Ajax_GetStoreSeller
        [HttpPost]
        public ActionResult GetStoreSellerAjax(int id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in _offlineDB.Off_Seller
                       where m.StoreId == id && m.Off_System_Id == user.DefaultSystemId
                       select new { Id = m.Id, Name = m.Name };
            return Json(new { result = "SUCCESS", data = list });
        }

        // Origin: Off_DeleteSalesDaily
        [HttpPost]
        public async Task<JsonResult> DeleteSalesDailyAjax(int id)
        {
            var item = _offlineDB.Off_SalesInfo_Daily.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_Store.Off_System_Id == user.DefaultSystemId)
                {
                    try
                    {
                        _offlineDB.Off_SalesInfo_Daily.Remove(item);
                        var result = await _offlineDB.SaveChangesAsync();
                        OfflineSalesUtilities util = new OfflineSalesUtilities();
                        var result2 = await util.UpdateDailySalesAvg(item.StoreId, (int)(item.Date.DayOfWeek) + 1);
                        return Json(new { result = "SUCCESS" });
                    }
                    catch
                    {
                        return Json(new { result = "FAIL" });
                    }
                }
                else
                    return Json(new { result = "UNAUTHORIZED" });
            }
            else
                return Json(new { result = "FAIL" });
        }

        // Origin: Off_CreateSalesMonth
        [SettingFilter(SettingName = "GENERAL")]
        public ActionResult CreateSalesMonthPartial()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var item = new Off_SalesInfo_Month();
            var storelist = from m in _offlineDB.Off_Store
                            where m.Off_System_Id == user.DefaultSystemId
                            orderby m.StoreName
                            select new { Key = m.Id, Value = m.StoreName };
            ViewBag.StoreDropDown = new SelectList(storelist, "Key", "Value");
            return PartialView(item);
        }
        [SettingFilter(SettingName = "GENERAL")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CreateSalesMonthPartial(Off_SalesInfo_Month model)
        {
            if (ModelState.IsValid)
            {
                Off_SalesInfo_Month item = new Off_SalesInfo_Month();
                if (TryUpdateModel(item))
                {
                    item.UploadTime = DateTime.Now;
                    item.UploadUser = User.Identity.Name;
                    _offlineDB.Off_SalesInfo_Month.Add(item);
                    _offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                ModelState.AddModelError("", "发生错误");
                var storelist = from m in _offlineDB.Off_Store
                                orderby m.StoreName
                                select new { Key = m.Id, Value = m.StoreName };
                ViewBag.StoreDropDown = new SelectList(storelist, "Key", "Value");
                return PartialView(model);
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                var storelist = from m in _offlineDB.Off_Store
                                orderby m.StoreName
                                select new { Key = m.Id, Value = m.StoreName };
                ViewBag.StoreDropDown = new SelectList(storelist, "Key", "Value");
                return PartialView(model);
            }
        }

        // Origin: Off_DeleteSalesMonth
        [HttpPost]
        [SettingFilter(SettingName = "GENERAL")]
        public ActionResult DeleteSalesMonthAjax(int id)
        {
            var item = _offlineDB.Off_SalesInfo_Month.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_Store.Off_System_Id == user.DefaultSystemId)
                {
                    try
                    {
                        _offlineDB.Off_SalesInfo_Month.Remove(item);
                        _offlineDB.SaveChanges();
                        return Json(new { result = "SUCCESS" });
                    }
                    catch
                    {
                        return Json(new { result = "FAIL" });
                    }
                }
                else
                    return Json(new { result = "UNAUTHORIZED" });
            }
            else
                return Json(new { result = "FAIL" });
        }

        // Origin:Off_DailyProductList
        public PartialViewResult DailySalesProductListPartial(int DailyId)
        {
            var item = _offlineDB.Off_SalesInfo_Daily.SingleOrDefault(m => m.Id == DailyId);
            var model = item.Off_Daily_Product;
            return PartialView(model);
        }
        // Origin:Off_DailyInfo_Add_ProductList
        public JsonResult DailySalesAllProductAjax()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var product = from m in _offlineDB.Off_Product
                          where m.Off_System_Id == user.DefaultSystemId && m.status >= 0
                          select new { Id = m.Id, ItemCode = m.ItemCode, SimpleName = m.SimpleName };
            return Json(new { result = "SUCCESS", data = product }, JsonRequestBehavior.AllowGet);
        }

        // Origin:Off_Sales_Month
        [SettingFilter(SettingName = "GENERAL")]
        public ActionResult MonthSalesList()
        {
            return View();
        }

        // Origin: Off_MonthSalesInfo_ajaxlist
        [SettingFilter(SettingName = "GENERAL")]
        public ActionResult MonthSalesListPartial(int? page, string query)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            int _page = page ?? 1;
            if (query == null || query == "")
            {
                var list = (from m in _offlineDB.Off_SalesInfo_Month
                            where m.Off_Store.Off_System_Id == user.DefaultSystemId
                            orderby m.Date descending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
            else
            {
                var list = (from m in _offlineDB.Off_SalesInfo_Month
                            where m.Off_Store.StoreName.Contains(query) && m.Off_Store.Off_System_Id == user.DefaultSystemId
                            orderby m.Date descending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
        }
        // Orgin:Ajax_EditMonthInfo
        public PartialViewResult EditMonthSalesPartial(int id)
        {
            var item = _offlineDB.Off_SalesInfo_Month.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_Store.Off_System_Id == user.DefaultSystemId)
                {
                    return PartialView(item);
                }
                else
                {
                    return PartialView("AuthorizeErrorPartial");
                }
            }
            else
            {
                return PartialView("ErrorPartial");
            }
        }
        [HttpPost]
        public ActionResult EditMonthSalesPartial(int id, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                var item = new Off_SalesInfo_Month();
                if (TryUpdateModel(item))
                {
                    item.UploadTime = DateTime.Now;
                    item.UploadUser = User.Identity.Name;
                    _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    _offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                else
                {
                    ModelState.AddModelError("", "错误");
                    return PartialView(item);
                }

            }
            else
            {
                return PartialView("ErrorPartial");
            }
        }
        
       


        // Origin:UploadDailyInfo
        [SettingFilter(SettingName = "GENERAL")]
        public ActionResult UploadDailySales()
        {
            return PartialView();
        }
        [HttpPost]
        [SettingFilter(SettingName = "GENERAL")]
        public async Task<ActionResult> UploadDailySales(FormCollection form)
        {
            var file = Request.Files[0];
            List<Excel_DataMessage> messageList = new List<Excel_DataMessage>();
            string time_ticks = DateTime.Now.Ticks.ToString();
            if (file != null)
            {
                //文件不得大于500K
                if (file.ContentLength > 1024 * 500)
                {
                    messageList.Add(new Excel_DataMessage(0, "文件大于500K", true));
                }
                else if (file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    messageList.Add(new Excel_DataMessage(0, "文件类型错误", true));
                }
                else
                {
                    string folder = HttpContext.Server.MapPath("~/Content/xlsx/");
                    string filename = time_ticks + file.FileName.Substring(file.FileName.LastIndexOf('.'));
                    file.SaveAs(folder + filename);
                    List<Excel_DataMessage> result = await UploadDailySalesByExcelAsync(filename, messageList);
                }
            }
            else
            {
                messageList.Add(new Excel_DataMessage(0, "文件上传错误", true));
            }
            return View("UploadResult", messageList);
        }


        

        
        // Origin:UploadMonthInfo
        [SettingFilter(SettingName = "GENERAL")]
        public ActionResult UploadMonthSales()
        {
            return PartialView();
        }
        [HttpPost]
        [SettingFilter(SettingName = "GENERAL")]
        public async Task<ActionResult> UploadMonthSales(FormCollection form)
        {
            var file = Request.Files[0];
            List<Excel_DataMessage> messageList = new List<Excel_DataMessage>();
            string time_ticks = DateTime.Now.Ticks.ToString();
            if (file != null)
            {
                //文件不得大于500K
                if (file.ContentLength > 1024 * 500)
                {
                    messageList.Add(new Excel_DataMessage(0, "文件大于500K", true));
                }
                else if (file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    messageList.Add(new Excel_DataMessage(0, "文件类型错误", true));
                }
                else
                {
                    string folder = HttpContext.Server.MapPath("~/Content/xlsx/");
                    string filename = time_ticks + file.FileName.Substring(file.FileName.LastIndexOf('.'));
                    file.SaveAs(folder + filename);
                    List<Excel_DataMessage> result =await UploadMonthSalesByExcelAsync(filename, messageList);
                }
            }
            else
            {
                messageList.Add(new Excel_DataMessage(0, "文件上传错误", true));
            }
            return View("UploadResult", messageList);
        }
        
        // Origin:analyseExcel_MonthInfoTable
        public async Task<List<Excel_DataMessage>> UploadMonthSalesByExcelAsync(string filename, List<Excel_DataMessage> messageList)
        {
            try
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                string folder = HttpContext.Server.MapPath("~/Content/xlsx/");
                string strConn = "Provider=Microsoft.Ace.OleDb.12.0;" + "data source=" + folder + filename + ";Extended Properties='Excel 12.0; HDR=1; IMEX=1'"; //此连接可以操作.xls与.xlsx文件
                OleDbConnection conn = new OleDbConnection(strConn);
                conn.Open();
                DataSet ds = new DataSet();
                OleDbDataAdapter odda = new OleDbDataAdapter(string.Format("SELECT * FROM [{0}]", "Sheet1$"), conn);                    //("select * from [Sheet1$]", conn);
                odda.Fill(ds, "[Sheet1$]");
                conn.Close();
                DataTable dt = ds.Tables[0];
                int i = 0;
                bool result_flag = true;
                foreach (DataRow dr in dt.Rows)
                {
                    i++;
                    try
                    {
                        // 判断是否存在店铺
                        string storename = dr["店铺名称"].ToString();
                        var exist_store = _offlineDB.Off_Store.SingleOrDefault(m => m.StoreName == storename && m.Off_System_Id == user.DefaultSystemId);
                        if (exist_store == null)
                        {
                            messageList.Add(new Excel_DataMessage(i, "店铺不存在", true));
                            result_flag = false;
                            continue;

                        }
                        // 判断是否含已有数据
                        DateTime info_date = Convert.ToDateTime(dr["月份"]);
                        info_date = new DateTime(info_date.Year, info_date.Month, 1);
                        var exist_dailyinfo = _offlineDB.Off_SalesInfo_Daily.SingleOrDefault(m => m.Date.Year == info_date.Year && m.Date.Month == info_date.Month && m.StoreId == exist_store.Id);
                        if (exist_dailyinfo != null)
                        {
                            messageList.Add(new Excel_DataMessage(i, "当月数据已存在", true));
                            result_flag = false;
                            continue;
                        }
                        else
                        {
                            Off_SalesInfo_Month monthinfo = new Off_SalesInfo_Month()
                            {
                                StoreId = exist_store.Id,
                                Date = info_date,
                                Item_Brown = ExcelOperation.ConvertInt(dr, "红糖姜茶"),
                                Item_Black = ExcelOperation.ConvertInt(dr, "黑糖姜茶"),
                                Item_Lemon = ExcelOperation.ConvertInt(dr, "柠檬姜茶"),
                                Item_Honey = ExcelOperation.ConvertInt(dr, "蜂蜜姜茶"),
                                Item_Dates = ExcelOperation.ConvertInt(dr, "红枣姜茶"),
                                UploadTime = DateTime.Now,
                                UploadUser = User.Identity.Name
                            };
                            _offlineDB.Off_SalesInfo_Month.Add(monthinfo);
                            messageList.Add(new Excel_DataMessage(i, "数据类型验证成功", false));
                        }
                    }
                    catch (Exception e)
                    {
                        result_flag = false;
                        messageList.Add(new Excel_DataMessage(i, "表格格式错误," + e.ToString(), true));
                    }

                }
                if (result_flag)
                {
                    await _offlineDB.SaveChangesAsync();
                    messageList.Add(new Excel_DataMessage(0, "保存成功", false));
                }
                else
                {
                    messageList.Add(new Excel_DataMessage(0, "数据行发生错误，未保存", true));
                }
            }
            catch (Exception e)
            {
                messageList.Add(new Excel_DataMessage(-1, "表格格式错误" + e.ToString(), true));
            }
            return messageList;
        }

        
        

        // Origin: analyseExcel_DailyInfoTable
        public async Task<List<Excel_DataMessage>> UploadDailySalesByExcelAsync(string filename, List<Excel_DataMessage> messageList)
        {
            try
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                string folder = HttpContext.Server.MapPath("~/Content/xlsx/");
                string strConn = "Provider=Microsoft.Ace.OleDb.12.0;" + "data source=" + folder + filename + ";Extended Properties='Excel 12.0; HDR=1; IMEX=1'"; //此连接可以操作.xls与.xlsx文件
                OleDbConnection conn = new OleDbConnection(strConn);
                conn.Open();
                DataSet ds = new DataSet();
                OleDbDataAdapter odda = new OleDbDataAdapter(string.Format("SELECT * FROM [{0}]", "Sheet1$"), conn);                    //("select * from [Sheet1$]", conn);
                odda.Fill(ds, "[Sheet1$]");
                conn.Close();
                DataTable dt = ds.Tables[0];
                int i = 0;
                bool result_flag = true;
                foreach (DataRow dr in dt.Rows)
                {
                    i++;
                    try
                    {
                        // 判断是否存在店铺
                        string storename = dr["店铺名称"].ToString();
                        var exist_store = _offlineDB.Off_Store.SingleOrDefault(m => m.StoreName == storename && m.Off_System_Id == user.DefaultSystemId);
                        if (exist_store == null)
                        {
                            messageList.Add(new Excel_DataMessage(i, "店铺不存在", true));
                            result_flag = false;
                            continue;
                        }
                        // 判断是否有促销员，如有促销员，判断存在销售员
                        string sellername = dr["促销员"].ToString();
                        Off_Seller exist_seller = null;
                        if (sellername != "")
                        {
                            exist_seller = _offlineDB.Off_Seller.SingleOrDefault(m => m.Name == sellername);
                            if (exist_seller == null)
                            {
                                messageList.Add(new Excel_DataMessage(i, "销售员不存在", true));
                                result_flag = false;
                                continue;
                            }
                        }
                        // 判断是否含已有数据
                        DateTime info_date = Convert.ToDateTime(dr["日期"]);
                        var exist_dailyinfo = _offlineDB.Off_SalesInfo_Daily.SingleOrDefault(m => m.Date == info_date && m.StoreId == exist_store.Id && m.isMultiple == false);
                        if (exist_dailyinfo != null)
                        {
                            messageList.Add(new Excel_DataMessage(i, "当日数据已存在", true));
                            result_flag = false;
                            continue;
                        }
                        else
                        {
                            int? attendance = null;
                            string attendance_info = dr["考勤"].ToString();
                            switch (attendance_info)
                            {
                                case "全勤":
                                    attendance = 0;
                                    break;
                                case "迟到":
                                    attendance = 1;
                                    break;
                                case "早退":
                                    attendance = 2;
                                    break;
                                case "旷工":
                                    attendance = 3;
                                    break;
                                default:
                                    attendance = null;
                                    break;
                            }
                            Off_SalesInfo_Daily dailyinfo = new Off_SalesInfo_Daily()
                            {
                                StoreId = exist_store.Id,
                                Date = info_date,
                                Item_Brown = ExcelOperation.ConvertInt(dr, "红糖姜茶"),
                                Item_Black = ExcelOperation.ConvertInt(dr, "黑糖姜茶"),
                                Item_Lemon = ExcelOperation.ConvertInt(dr, "柠檬姜茶"),
                                Item_Honey = ExcelOperation.ConvertInt(dr, "蜂蜜姜茶"),
                                Item_Dates = ExcelOperation.ConvertInt(dr, "红枣姜茶"),
                                Off_Seller = exist_seller,
                                Attendance = attendance,
                                Salary = ExcelOperation.ConvertDecimal(dr, "工资"),
                                Bonus = ExcelOperation.ConvertDecimal(dr, "奖金"),
                                Debit = ExcelOperation.ConvertDecimal(dr, "扣款"),
                                isMultiple = ExcelOperation.ConvertBoolean(dr, "多人"),
                                remarks = dr["备注"].ToString(),
                                UploadTime = DateTime.Now,
                                UploadUser = User.Identity.Name
                            };
                            _offlineDB.Off_SalesInfo_Daily.Add(dailyinfo);
                            messageList.Add(new Excel_DataMessage(i, "数据类型验证成功", false));
                        }
                    }
                    catch (Exception e)
                    {
                        result_flag = false;
                        messageList.Add(new Excel_DataMessage(i, "表格格式错误," + e.ToString(), true));
                    }
                }
                if (result_flag)
                {
                    await _offlineDB.SaveChangesAsync();
                    messageList.Add(new Excel_DataMessage(0, "保存成功", false));
                }
                else
                {
                    messageList.Add(new Excel_DataMessage(0, "数据行发生错误，未保存", true));
                }
            }
            catch (Exception e)
            {
                messageList.Add(new Excel_DataMessage(-1, "表格格式错误" + e.ToString(), true));
            }
            return messageList;
        }
        [SettingFilter(SettingName = "GENERAL")]
        public ActionResult UploadResult()
        {
            return View();
        }

        
        // Origin: Off_Daily_Delete_batch
        [HttpPost]
        public JsonResult DeleteDailySalesBatchAjax(string ids)
        {
            try
            {
                string sql = "DELETE FROM Off_SalesInfo_Daily Where Id in (" + ids + ")";
                _offlineDB.Database.ExecuteSqlCommand(sql);
                _offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
        }

        // Origin:Ajax_downloadSalary
        // 下载门店销售统计
        public FileResult DownloadSalaryFile(DateTime start, DateTime end)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var sql = "select T3.StoreName, SUM(T4.SalesCount) as SalesCount, T3.StoreSystem, convert(char(10), T3.Date, 120) as SalesDate  from (SELECT T1.Id, convert(char(10), T1.Date, 120) as Date, T2.StoreName, T2.StoreSystem" +
                " FROM Off_SalesInfo_Daily as T1 left join Off_Store as T2 on T1.StoreId = T2.Id" +
                " where Date>= '" + start.ToString("yyyy-MM-dd") + "' and Date<= '" + end.ToString("yyyy-MM-dd") + "' and T2.Off_System_Id = " + user.DefaultSystemId + ") as T3 left join Off_Daily_Product as T4" +
                " on T3.Id = T4.DailyId group by T3.StoreName, T3.StoreSystem, convert(char(10), T3.Date, 120)";
            var list = _offlineDB.Database.SqlQuery<StoreStaticExcel>(sql);
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            CsvWriter csv = new CsvWriter(writer);
            //string[] columname = new string[] {"店铺名称", "经销商", "姓名", "电话号码", "身份证号码", "开户行", "银行卡号", "工资", "奖金", "全勤天数", "迟到天数" };
            csv.WriteField("日期");
            csv.WriteField("系统");
            csv.WriteField("店铺名称");
            csv.WriteField("促销数量");
            csv.NextRecord();
            foreach (var item in list)
            {
                csv.WriteField(item.SalesDate);
                csv.WriteField(item.StoreSystem);
                csv.WriteField(item.StoreName);
                csv.WriteField(item.SalesCount ?? 0);
                csv.NextRecord();
            }
            //csv.WriteRecords(list);
            writer.Flush();
            writer.Close();
            return File(convertCSV(stream.ToArray()), "@text/csv", "促销信息" + start.ToShortDateString() + "-" + end.ToShortDateString() + ".csv");
        }
        private byte[] convertCSV(byte[] array)
        {
            byte[] outBuffer = new byte[array.Length + 3];
            outBuffer[0] = (byte)0xEF;//有BOM,解决乱码
            outBuffer[1] = (byte)0xBB;
            outBuffer[2] = (byte)0xBF;
            Array.Copy(array, 0, outBuffer, 3, array.Length);
            return outBuffer;
        }
    }
}