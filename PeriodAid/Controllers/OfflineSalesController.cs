﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PeriodAid.Models;
using PeriodAid.DAL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using PagedList;
using System.Data.OleDb;
using System.Data;
using System.Text;
using System.IO;
using CsvHelper;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.RegularExpressions;

namespace PeriodAid.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OfflineSalesController : Controller
    {
        // GET: OfflineSales
        OfflineSales offlineDB = new OfflineSales();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public OfflineSalesController()
        {

        }

        public OfflineSalesController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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


        public ActionResult Off_Store_main()
        {
            return View();
        }
        public PartialViewResult Off_Store_ajaxlist(int? page, string query)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            int _page = page ?? 1;
            if (query == null || query == "")
            {

                var list = (from m in offlineDB.Off_Store
                            where m.Off_System_Id == user.DefaultSystemId
                            orderby m.Id descending
                            select m).ToPagedList(_page, 50);
                return PartialView(list);
            }
            else
            {
                var list = (from m in offlineDB.Off_Store
                            where (m.StoreName.Contains(query) || m.Address.Contains(query))
                            && m.Off_System_Id == user.DefaultSystemId
                            orderby m.Id descending
                            select m).ToPagedList(_page, 100);
                return PartialView(list);
            }
        }

        public ActionResult Off_Sales_main()
        {
            return View();
        }
        public ActionResult Off_DailySalesInfo_ajaxlist(int? page, string query)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (query == null || query == "")
            {
                int _page = page ?? 1;
                var list = (from m in offlineDB.Off_SalesInfo_Daily
                            where m.Off_Store.Off_System_Id == user.DefaultSystemId
                            orderby m.Date descending
                            select m).ToPagedList(_page, 50);
                return PartialView(list);
            }
            else
            {
                int _page = page ?? 1;
                var list = (from m in offlineDB.Off_SalesInfo_Daily
                            where m.Off_Store.StoreName.Contains(query) && m.Off_Store.Off_System_Id == user.DefaultSystemId
                            orderby m.Date descending
                            select m).ToPagedList(_page, 50);
                return PartialView(list);
            }
        }
        public ActionResult Off_Sales_Month()
        {
            return View();
        }
        public ActionResult Off_MonthSalesInfo_ajaxlist(int? page, string query)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (query == null || query == "")
            {
                int _page = page ?? 1;
                var list = (from m in offlineDB.Off_SalesInfo_Month
                            where m.Off_Store.Off_System_Id == user.DefaultSystemId
                            orderby m.Date descending
                            select m).ToPagedList(_page, 50);
                return PartialView(list);
            }
            else
            {
                int _page = page ?? 1;
                var list = (from m in offlineDB.Off_SalesInfo_Month
                            where m.Off_Store.StoreName.Contains(query) && m.Off_Store.Off_System_Id== user.DefaultSystemId
                            orderby m.Date descending
                            select m).ToPagedList(_page, 50);
                return PartialView(list);
            }
        }

        
        public ActionResult Off_Seller_main()
        {
            return View();
        }
        public ActionResult Off_Seller_ajaxlist(int? page, string query)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (query == null || query == "")
            {
                int _page = page ?? 1;
                var list = (from m in offlineDB.Off_Seller
                            where m.Off_System_Id == user.DefaultSystemId
                            orderby m.Id descending
                            select m).ToPagedList(_page, 50);
                return PartialView(list);
            }
            else
            {
                int _page = page ?? 1;
                var list = (from m in offlineDB.Off_Seller
                            where (m.Name.Contains(query) || m.Off_Store.StoreName.Contains(query)) && m.Off_System_Id == user.DefaultSystemId
                            orderby m.Id descending
                            select m).ToPagedList(_page, 50);
                return PartialView(list);
            }
        }
        

        #region 上传店铺信息
        public ActionResult UploadStore()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UploadStore(FormCollection form)
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
                    List<Excel_DataMessage> result = analyseExcel_StoreTable(filename, messageList);
                }
            }
            else
            {
                messageList.Add(new Excel_DataMessage(0, "文件上传错误", true));
            }
            return View("UploadResult", messageList);
        }
        private List<Excel_DataMessage> analyseExcel_StoreTable(string filename, List<Excel_DataMessage> messageList)
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
                        // 判断是否含有数据
                        string storename = dr["店铺名称"].ToString();
                        var exist_item = offlineDB.Off_Store.SingleOrDefault(m => m.StoreName == storename && m.Off_System_Id == user.DefaultSystemId);
                        if (exist_item != null)
                        {
                            // 更新数据
                            exist_item.StoreSystem = dr["渠道"].ToString();
                            exist_item.StoreName = dr["店铺名称"].ToString();
                            exist_item.Distributor = dr["经销商"].ToString();
                            exist_item.Region = dr["区域"].ToString();
                            exist_item.Address = dr["详细地址"].ToString();
                            exist_item.Longitude = dr["经度"].ToString();
                            exist_item.Latitude = dr["纬度"].ToString();
                            exist_item.UploadTime = DateTime.Now;
                            exist_item.UploadUser = User.Identity.Name;
                            messageList.Add(new Excel_DataMessage(i, "数据修改成功", false));
                        }
                        else
                        {
                            // 添加数据
                            Off_Store store = new Off_Store()
                            {
                                StoreSystem = dr["渠道"].ToString(),
                                StoreName = dr["店铺名称"].ToString(),
                                Distributor = dr["经销商"].ToString(),
                                Region = dr["区域"].ToString(),
                                Address = dr["详细地址"].ToString(),
                                Longitude = dr["经度"].ToString(),
                                Latitude = dr["纬度"].ToString(),
                                UploadTime = DateTime.Now,
                                UploadUser = User.Identity.Name,
                                Off_System_Id = user.DefaultSystemId
                            };
                            offlineDB.Off_Store.Add(store);
                            messageList.Add(new Excel_DataMessage(i, "数据添加成功", false));
                        }
                    }
                    catch (Exception e)
                    {
                        result_flag = false;
                        messageList.Add(new Excel_DataMessage(i, "格式错误或列名不存在," + e.InnerException, true));
                    }
                }
                if (result_flag)
                {
                    offlineDB.SaveChanges();
                    messageList.Add(new Excel_DataMessage(0, "数据存储成功", false));
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
        public PartialViewResult Ajax_EditStore(int id)
        {
            var item = offlineDB.Off_Store.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                List<Object> attendance = new List<Object>();
                attendance.Add(new { Key = "华东", Value = "华东" });
                attendance.Add(new { Key = "外区", Value = "外区" });
                ViewBag.Regionlist = new SelectList(attendance, "Key", "Value", item.Region);
                return PartialView(item);
            }
            else
            {
                return PartialView("Error");
            }
        }
        [HttpPost]
        public ActionResult Ajax_EditStore(int id, FormCollection form)
        {
            var item = new Off_Store();
            if (TryUpdateModel(item))
            {
                item.UploadTime = DateTime.Now;
                item.UploadUser = User.Identity.Name;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Content("SUCCESS");
            }
            else
            {
                ModelState.AddModelError("", "错误");
                List<Object> attendance = new List<Object>();
                attendance.Add(new { Key = "华东", Value = "华东" });
                attendance.Add(new { Key = "外区", Value = "外区" });
                ViewBag.Regionlist = new SelectList(attendance, "Key", "Value", item.Region);
                return PartialView(item);
            }
        }
        #endregion

        #region 上传店铺日报信息
        public ActionResult UploadDailyInfo()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UploadDailyInfo(FormCollection form)
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
                    List<Excel_DataMessage> result = analyseExcel_DailyInfoTable(filename, messageList);
                }
            }
            else
            {
                messageList.Add(new Excel_DataMessage(0, "文件上传错误", true));
            }
            return View("UploadResult", messageList);
        }
        public List<Excel_DataMessage> analyseExcel_DailyInfoTable(string filename, List<Excel_DataMessage> messageList)
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
                        var exist_store = offlineDB.Off_Store.SingleOrDefault(m => m.StoreName == storename && m.Off_System_Id == user.DefaultSystemId);
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
                            exist_seller = offlineDB.Off_Seller.SingleOrDefault(m => m.Name == sellername);
                            if (exist_seller == null)
                            {
                                messageList.Add(new Excel_DataMessage(i, "销售员不存在", true));
                                result_flag = false;
                                continue;
                            }
                        }

                        // 判断是否含已有数据
                        DateTime info_date = Convert.ToDateTime(dr["日期"]);
                        var exist_dailyinfo = offlineDB.Off_SalesInfo_Daily.SingleOrDefault(m => m.Date == info_date && m.StoreId == exist_store.Id && m.isMultiple == false);
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
                            offlineDB.Off_SalesInfo_Daily.Add(dailyinfo);
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
                    offlineDB.SaveChanges();
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

        public PartialViewResult Ajax_EditDailyInfo(int id)
        {
            var item = offlineDB.Off_SalesInfo_Daily.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var sellerlist = from m in offlineDB.Off_Seller
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
                return PartialView("Error");
            }
        }
        [HttpPost]
        public ActionResult Ajax_EditDailyInfo(int id, FormCollection form)
        {
            var item = new Off_SalesInfo_Daily();
            if (TryUpdateModel(item))
            {
                item.UploadTime = DateTime.Now;
                item.UploadUser = User.Identity.Name;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Content("SUCCESS");
            }
            else
            {
                ModelState.AddModelError("", "错误");
                var sellerlist = from m in offlineDB.Off_Seller
                                 where m.StoreId == item.StoreId
                                 select m;
                ViewBag.Sellerlist = new SelectList(sellerlist, "Id", "Name");
                List<Object> attendance = new List<Object>();
                attendance.Add(new { Key = 0, Value = "全勤" });
                attendance.Add(new { Key = 1, Value = "迟到" });
                attendance.Add(new { Key = 2, Value = "早退" });
                attendance.Add(new { Key = 3, Value = "旷工" });
                ViewBag.Attendance = new SelectList(attendance, "Key", "Value", item.Attendance);
                return PartialView(item);
            }
        }
        #endregion

        #region 上传月度门店销售表
        public ActionResult UploadMonthInfo()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UploadMonthInfo(FormCollection form)
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
                    List<Excel_DataMessage> result = analyseExcel_MonthInfoTable(filename, messageList);
                }
            }
            else
            {
                messageList.Add(new Excel_DataMessage(0, "文件上传错误", true));
            }
            return View("UploadResult", messageList);
        }
        public List<Excel_DataMessage> analyseExcel_MonthInfoTable(string filename, List<Excel_DataMessage> messageList)
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
                        var exist_store = offlineDB.Off_Store.SingleOrDefault(m => m.StoreName == storename && m.Off_System_Id == user.DefaultSystemId);
                        if (exist_store == null)
                        {
                            messageList.Add(new Excel_DataMessage(i, "店铺不存在", true));
                            result_flag = false;
                            continue;

                        }
                        // 判断是否含已有数据
                        DateTime info_date = Convert.ToDateTime(dr["月份"]);
                        info_date = new DateTime(info_date.Year, info_date.Month, 1);
                        var exist_dailyinfo = offlineDB.Off_SalesInfo_Daily.SingleOrDefault(m => m.Date.Year == info_date.Year && m.Date.Month == info_date.Month && m.StoreId == exist_store.Id);
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
                            offlineDB.Off_SalesInfo_Month.Add(monthinfo);
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
                    offlineDB.SaveChanges();
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
        public PartialViewResult Ajax_EditMonthInfo(int id)
        {
            var item = offlineDB.Off_SalesInfo_Month.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                return PartialView(item);
            }
            else
            {
                return PartialView("Error");
            }
        }
        [HttpPost]
        public ActionResult Ajax_EditMonthInfo(int id, FormCollection form)
        {
            var item = new Off_SalesInfo_Month();
            if (TryUpdateModel(item))
            {
                item.UploadTime = DateTime.Now;
                item.UploadUser = User.Identity.Name;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Content("SUCCESS");
            }
            else
            {
                ModelState.AddModelError("", "错误");

                return PartialView(item);
            }
        }
        #endregion

        #region 上传促销人员信息
        /*---------- 上传促销人员信息 ----------*/
        public ActionResult UploadSeller()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UploadSeller(FormCollection form)
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
                    List<Excel_DataMessage> result = analyseExcel_SellerTable(filename, messageList);
                }
            }
            else
            {
                messageList.Add(new Excel_DataMessage(0, "文件上传错误", true));
            }
            return View("UploadResult", messageList);
        }
        public List<Excel_DataMessage> analyseExcel_SellerTable(string filename, List<Excel_DataMessage> messageList)
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
                        var exist_store = offlineDB.Off_Store.SingleOrDefault(m => m.StoreName == storename && m.Off_System_Id == user.DefaultSystemId);
                        if (exist_store == null)
                        {
                            messageList.Add(new Excel_DataMessage(i, "店铺不存在", true));
                            result_flag = false;
                            continue;
                        }
                        // 判断是否含已有数据
                        string info_name = dr["姓名"].ToString();
                        //var exist_dailyinfo = offlineDB.Off_SalesInfo_Daily.SingleOrDefault(m => m.Date == info_date && m.StoreId == exist_store.Id);
                        var exist_seller = offlineDB.Off_Seller.SingleOrDefault(m => m.Name == info_name);
                        if (exist_seller != null)
                        {
                            messageList.Add(new Excel_DataMessage(i, "销售员信息已存在", true));
                            result_flag = false;
                            continue;
                        }
                        else
                        {
                            Off_Seller seller = new Off_Seller()
                            {
                                StoreId = exist_store.Id,
                                Name = info_name,
                                Mobile = dr["联系方式"].ToString(),
                                IdNumber = dr["身份证"].ToString(),
                                CardName = dr["开户行"].ToString(),
                                CardNo = dr["银行卡号"].ToString(),
                                UploadTime = DateTime.Now,
                                UploadUser = user.UserName,
                                Off_System_Id = user.DefaultSystemId
                            };
                            offlineDB.Off_Seller.Add(seller);
                        }
                        messageList.Add(new Excel_DataMessage(i, "数据类型验证成功", false));
                    }
                    catch (Exception e)
                    {
                        messageList.Add(new Excel_DataMessage(i, "表格格式错误," + e.ToString(), true));
                        result_flag = false;
                    }
                }
                if (result_flag)
                {
                    offlineDB.SaveChanges();
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
        public PartialViewResult Ajax_EditSeller(int id)
        {
            var item = offlineDB.Off_Seller.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                return PartialView(item);
            }
            else
            {
                return PartialView("Error");
            }
        }
        [HttpPost]
        public ActionResult Ajax_EditSeller(int id, FormCollection form)
        {
            var item = new Off_Seller();
            if (TryUpdateModel(item))
            {
                item.UploadTime = DateTime.Now;
                item.UploadUser = User.Identity.Name;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Content("SUCCESS");
            }
            else
            {
                ModelState.AddModelError("", "错误");
                return PartialView(item);
            }
        }
        public PartialViewResult Ajax_AddSeller()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var storelist = offlineDB.Off_Store.OrderBy(m => m.StoreName);
            ViewBag.Storelist = new SelectList(storelist, "Id", "StoreName");
            Off_Seller seller = new Off_Seller();
            seller.Off_System_Id = user.DefaultSystemId;
            return PartialView(seller);
        }
        [HttpPost]
        public ActionResult Ajax_AddSeller(FormCollection form)
        {
            var item = new Off_Seller();
            if (TryUpdateModel(item))
            {
                item.UploadUser = User.Identity.Name;
                item.UploadTime = DateTime.Now;
                offlineDB.Off_Seller.Add(item);
                offlineDB.SaveChanges();
                return Content("SUCCESS");
            }
            else
            {
                ModelState.AddModelError("", "错误");
                var storelist = offlineDB.Off_Store.OrderBy(m => m.StoreName);
                ViewBag.Storelist = new SelectList(storelist, "Id", "StoreName");
                return PartialView(item);
            }
        }
        #endregion
        
        /* -----费用表----- */
        public ActionResult Off_Expenses_List()
        {
            return View();
        }
        public ActionResult Off_Expenses_Add()
        {
            List<Object> attendance = new List<Object>();
            attendance.Add(new { Key = 0, Value = "进场费" });
            attendance.Add(new { Key = 1, Value = "活动费" });
            ViewBag.PayType = new SelectList(attendance, "Key", "Value");
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Off_Expenses_Add(FormCollection form)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var item = new Off_Expenses();
            if (TryUpdateModel(item))
            {
                item.Status = 0;
                item.UploadTime = DateTime.Now;
                item.UploadUser = User.Identity.Name;
                item.Off_System_Id = user.DefaultSystemId;
                offlineDB.Off_Expenses.Add(item);
                offlineDB.SaveChanges();
            }
            return RedirectToAction("Off_Expenses_List");
        }
        public ActionResult Off_Expenses_Edit(int id)
        {
            var item = offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                List<Object> attendance = new List<Object>();
                attendance.Add(new { Key = 0, Value = "进场费" });
                attendance.Add(new { Key = 1, Value = "活动费" });
                ViewBag.PayType = new SelectList(attendance, "Key", "Value", item.Status);
                return View(item);
            }
            else
                return View("Error");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Off_Expenses_Edit(int id, FormCollection form)
        {
            Off_Expenses item = new Off_Expenses();
            if (TryUpdateModel(item))
            {
                var detailitemcnt = form.GetValues("detailid") == null ? 0 : form.GetValues("detailid").Length;
                for (int i = 0; i < detailitemcnt; i++)
                {
                    if (form.GetValues("detailid")[i] == "0")
                    {
                        Off_Expenses_Details detailtemp = new Off_Expenses_Details()
                        {
                            Off_Expenses = item,
                            DetailsFee = Convert.ToDecimal(form.GetValues("detailfee")[i].ToString()),
                            DetailsName = form.GetValues("detaillist")[i].ToString(),
                            Remarks = form.GetValues("detailremarks")[i].ToString(),
                            UploadTime = DateTime.Now,
                            UploadUser = User.Identity.Name,
                            ExpensesType = 0
                        };
                        offlineDB.Off_Expenses_Details.Add(detailtemp);
                    }
                    else
                    {
                        int d_id = Convert.ToInt32(form.GetValues("detailid")[i]);
                        Off_Expenses_Details detailstemp = offlineDB.Off_Expenses_Details.SingleOrDefault(m => m.Id == d_id);
                        detailstemp.DetailsFee = Convert.ToDecimal(form.GetValues("detailfee")[i].ToString());
                        detailstemp.DetailsName = form.GetValues("detaillist")[i].ToString();
                        detailstemp.Remarks = form.GetValues("detailremarks")[i].ToString();
                        detailstemp.UploadTime = DateTime.Now;
                        detailstemp.UploadUser = User.Identity.Name;
                        offlineDB.Entry(detailstemp).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                item.UploadTime = DateTime.Now;
                item.UploadUser = User.Identity.Name;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
            }
            return RedirectToAction("Off_Expenses_List");
        }
        public ActionResult Off_Expenses_Balance(int id)
        {
            var item = offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                List<Object> attendance = new List<Object>();
                attendance.Add(new { Key = 0, Value = "进场费" });
                attendance.Add(new { Key = 1, Value = "活动费" });
                ViewBag.PayType = new SelectList(attendance, "Key", "Value", item.PaymentType);
                return View(item);
            }
            else
                return View("Error");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Off_Expenses_Balance(int id, FormCollection form)
        {
            Off_Expenses item = new Off_Expenses();
            if (TryUpdateModel(item))
            {
                var detailitemcnt = form.GetValues("detailid") == null ? 0 : form.GetValues("detailid").Length;
                for (int i = 0; i < detailitemcnt; i++)
                {
                    if (form.GetValues("detailid")[i] == "0")
                    {
                        Off_Expenses_Details detailtemp = new Off_Expenses_Details()
                        {
                            Off_Expenses = item,
                            DetailsFee = Convert.ToDecimal(form.GetValues("detailfee")[i].ToString()),
                            DetailsName = form.GetValues("detaillist")[i].ToString(),
                            Remarks = form.GetValues("detailremarks")[i].ToString(),
                            UploadTime = DateTime.Now,
                            UploadUser = User.Identity.Name,
                            ExpensesType = 1
                        };
                        offlineDB.Off_Expenses_Details.Add(detailtemp);
                    }
                    else
                    {
                        int d_id = Convert.ToInt32(form.GetValues("detailid")[i]);
                        Off_Expenses_Details detailstemp = offlineDB.Off_Expenses_Details.SingleOrDefault(m => m.Id == d_id);
                        detailstemp.DetailsFee = Convert.ToDecimal(form.GetValues("detailfee")[i].ToString());
                        detailstemp.DetailsName = form.GetValues("detaillist")[i].ToString();
                        detailstemp.Remarks = form.GetValues("detailremarks")[i].ToString();
                        detailstemp.UploadTime = DateTime.Now;
                        detailstemp.UploadUser = User.Identity.Name;
                        offlineDB.Entry(detailstemp).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                item.UploadTime = DateTime.Now;
                item.UploadUser = User.Identity.Name;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return RedirectToAction("Off_Expenses_List");
            }
            return View("Error");
        }
        public ActionResult Off_Expenses_VerifyCost(int id)
        {
            var item = offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                List<Object> attendance = new List<Object>();
                attendance.Add(new { Key = 0, Value = "进场费" });
                attendance.Add(new { Key = 1, Value = "活动费" });
                ViewBag.PayType = new SelectList(attendance, "Key", "Value", item.PaymentType);
                return View(item);
            }
            else
                return View("Error");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Off_Expenses_VerifyCost(FormCollection form)
        {
            Off_Expenses item = new Off_Expenses();
            if (TryUpdateModel(item))
            {
                var detailitemcnt = form.GetValues("detailid") == null ? 0 : form.GetValues("detailid").Length;
                for (int i = 0; i < detailitemcnt; i++)
                {
                    if (form.GetValues("detailid")[i] == "0")
                    {
                        Off_Expenses_Payment detailtemp = new Off_Expenses_Payment()
                        {
                            Off_Expenses = item,
                            VerifyCost = Convert.ToDecimal(form.GetValues("detailfee")[i].ToString()),
                            VerifyType = Convert.ToInt32(form.GetValues("detaillist")[i].ToString()),
                            ApplicationDate = Convert.ToDateTime(form.GetValues("apdate")[i]),
                            Remarks = form.GetValues("detailremarks")[i].ToString(),
                            UploadTime = DateTime.Now,
                            UploadUser = User.Identity.Name
                        };
                        offlineDB.Off_Expenses_Payment.Add(detailtemp);
                    }
                    else
                    {
                        int d_id = Convert.ToInt32(form.GetValues("detailid")[i]);
                        Off_Expenses_Payment detailstemp = offlineDB.Off_Expenses_Payment.SingleOrDefault(m => m.Id == d_id);
                        detailstemp.VerifyCost = Convert.ToDecimal(form.GetValues("detailfee")[i].ToString());
                        detailstemp.VerifyType = Convert.ToInt32(form.GetValues("detaillist")[i].ToString());
                        detailstemp.ApplicationDate = Convert.ToDateTime(form.GetValues("apdate")[i]);
                        detailstemp.Remarks = form.GetValues("detailremarks")[i].ToString();
                        detailstemp.UploadTime = DateTime.Now;
                        detailstemp.UploadUser = User.Identity.Name;
                        offlineDB.Entry(detailstemp).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                item.UploadTime = DateTime.Now;
                item.UploadUser = User.Identity.Name;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return RedirectToAction("Off_Expenses_List");
            }
            return View("Error");
        }
        [HttpPost]
        public JsonResult Off_Expenses_Details_Del(int id)
        {
            if (id == 0)
            {
                return Json(new { result = "SUCCESS" });
            }
            var item = offlineDB.Off_Expenses_Details.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                offlineDB.Off_Expenses_Details.Remove(item);
                offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }
        [HttpPost]
        public JsonResult Off_Expenses_Payment_Del(int id)
        {
            if (id == 0)
            {
                return Json(new { result = "SUCCESS" });
            }
            var item = offlineDB.Off_Expenses_Payment.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                offlineDB.Off_Expenses_Payment.Remove(item);
                offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }
        public ActionResult Off_Expenses_Check(int id)
        {
            var item = offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                item.CheckTime = DateTime.Now;
                item.Status = 1;
                offlineDB.SaveChanges();
            }
            return RedirectToAction("Off_Expenses_List");
        }
        public ActionResult Off_Expenses_Verify_Submit(int id)
        {
            var item = offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                item.CheckTime = DateTime.Now;
                item.Status = 3;
                offlineDB.SaveChanges();
            }
            return RedirectToAction("Off_Expenses_List");
        }
        public ActionResult Off_Expenses_Details(int id)
        {
            var item = offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                List<Object> attendance = new List<Object>();
                attendance.Add(new { Key = 0, Value = "进场费" });
                attendance.Add(new { Key = 1, Value = "活动费" });
                ViewBag.PayType = new SelectList(attendance, "Key", "Value", item.PaymentType);
                List<Object> status = new List<Object>();
                status.Add(new { Key = 0, Value = "未审核" });
                status.Add(new { Key = 1, Value = "已审核" });
                status.Add(new { Key = 2, Value = "已结算" });
                status.Add(new { Key = 3, Value = "已核销" });
                status.Add(new { Key = -1, Value = "作废" });
                ViewBag.ExpensesStatus = new SelectList(status, "Key", "Value", item.Status);
                return View(item);
            }
            else
                return View("Error");

        }
        public ActionResult Off_Expenses_AjaxList(int? page, int? type)
        {
            int _type = type ?? 0;
            int _page = page ?? 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = (from m in offlineDB.Off_Expenses
                        where m.Status >= 0 && m.PaymentType==_type && m.Off_System_Id == user.DefaultSystemId
                        orderby m.Id descending
                        select m).ToPagedList(_page, 50);
            return PartialView(list);
        }
        public ActionResult Off_Expenses_Balance_Submit(int id)
        {
            var item = offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                item.BalanceTime = DateTime.Now;
                item.Status = 2;
                offlineDB.SaveChanges();
            }
            return RedirectToAction("Off_Expenses_List");
        }
        public ActionResult Off_Expenses_Cancel(int id)
        {
            var item = offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                item.UploadTime = DateTime.Now;
                item.UploadUser = User.Identity.Name;
                item.Status = -1;
                offlineDB.SaveChanges();
            }
            return RedirectToAction("Off_Expenses_List");
        }
        public ActionResult Off_Expenses_Details_Edit(int id)
        {
            var item = offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
                return View(item);
            else
                return View("Error");
        }
        [HttpPost]
        public ActionResult Off_Expenses_Details_Edit(FormCollection form)
        {
            var listcontent = "";
            int i = 0;
            foreach (var item in form.GetValues("list"))
            {
                listcontent += item.ToString();
                i++;
            }
            return Content(form["ExpensesId"].ToString() + "," + i + "<BR />" + listcontent);
        }


        public ActionResult UploadResult()
        {
            return View();
        }


        public ActionResult StoreMap()
        {
            return View();
        }

        [HttpPost]
        public JsonResult ajax_StoreSystem()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var storesystem = from m in offlineDB.Off_Store
                              where m.Off_System_Id == user.DefaultSystemId
                              group m by m.StoreSystem into g
                              select g.Key;
            return Json(new { storesystem = storesystem });
        }
        public JsonResult JsonStoreList(string storesystem)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (storesystem == null || storesystem == "")
            {
                var list = from m in offlineDB.Off_Store
                           where m.Latitude != "" && m.Longitude != "" && m.Off_System_Id == user.DefaultSystemId
                           orderby m.Id descending
                           select new { StoreName = m.StoreName, StoreSystem = m.StoreSystem, Address = m.Address, Longitude = m.Longitude, Latitude = m.Latitude };
                return Json(new { result = "SUCCESS", list = list }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                string[] systems = storesystem.Split(',');
                var list = from m in offlineDB.Off_Store
                           where systems.Contains(m.StoreSystem) && m.Off_System_Id == user.DefaultSystemId
                           orderby m.Id descending
                           select new { StoreName = m.StoreName, StoreSystem = m.StoreSystem, Address = m.Address, Longitude = m.Longitude, Latitude = m.Latitude };
                return Json(new { result = "SUCCESS", list = list }, JsonRequestBehavior.AllowGet);
            }
        }
        public FileResult Ajax_downloadSalary(DateTime start, DateTime end)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in offlineDB.Off_SalesInfo_Daily
                       where m.Date >= start && m.Date <= end && m.Off_Seller.Off_System_Id == user.DefaultSystemId
                       group m by m.Off_Seller into g
                       select new
                       {
                           Name = g.Key.Name,
                           StoreName = g.Key.Off_Store.StoreName,
                           Distributor = g.Key.Off_Store.Distributor,
                           Mobile = g.Key.Mobile,
                           IdNumber = g.Key.IdNumber,
                           CardName = g.Key.CardName,
                           CardNo = g.Key.CardNo,
                           Salary = g.Sum(m => m.Salary) == null ? 0 : g.Sum(m => m.Salary),
                           Bonus = g.Sum(m => m.Bonus) == null ? 0 : g.Sum(m => m.Bonus),
                           Debit = g.Sum(m => m.Debit) == null ? 0 : g.Sum(m => m.Debit),
                           AccountName = g.Key.AccountName,
                           All = g.Count(m => m.Attendance == 0),
                           Delay = g.Count(m => m.Attendance == 1),
                           Leave = g.Count(m => m.Attendance == 2),
                           Absence = g.Count(m => m.Attendance == 3)
                       };
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            CsvWriter csv = new CsvWriter(writer);
            //string[] columname = new string[] {"店铺名称", "经销商", "姓名", "电话号码", "身份证号码", "开户行", "银行卡号", "工资", "奖金", "全勤天数", "迟到天数" };
            csv.WriteField("店铺名称");
            csv.WriteField("经销商");
            csv.WriteField("姓名");
            csv.WriteField("电话号码");
            csv.WriteField("身份证号码");
            csv.WriteField("开户行");
            csv.WriteField("开户人姓名");
            csv.WriteField("银行卡号");
            csv.WriteField("工资");
            csv.WriteField("奖金");
            csv.WriteField("扣款");
            csv.WriteField("全勤天数");
            csv.WriteField("迟到天数");
            csv.WriteField("早退天数");
            csv.WriteField("旷工天数");
            csv.NextRecord();
            foreach (var item in list)
            {
                csv.WriteField(item.StoreName);
                csv.WriteField(item.Distributor);
                csv.WriteField(item.Name);
                csv.WriteField("'" + item.Mobile);
                csv.WriteField("'" + item.IdNumber);
                csv.WriteField(item.CardName);
                csv.WriteField(item.AccountName);
                csv.WriteField("'" + item.CardNo);
                csv.WriteField(item.Salary);
                csv.WriteField(item.Bonus);
                csv.WriteField(item.Debit);
                csv.WriteField(item.All);
                csv.WriteField(item.Delay);
                csv.WriteField(item.Leave);
                csv.WriteField(item.Absence);
                csv.NextRecord();
            }
            //csv.WriteRecords(list);
            writer.Flush();
            writer.Close();
            return File(convertCSV(stream.ToArray()), "@text/csv", "工资信息" + start.ToShortDateString() + "-" + end.ToShortDateString() + ".csv");
        }
        #region 绑定促销员
        public ActionResult Off_BindSeller_List(string query)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (query == null)
            {
                var list = offlineDB.Off_Membership_Bind.Where(m => m.Off_System_Id == user.DefaultSystemId).OrderByDescending(m => m.ApplicationDate);
                return View(list);
            }
            else
            {
                var list = from m in offlineDB.Off_Membership_Bind
                           where (m.NickName.Contains(query) || m.Off_Seller.Off_Store.StoreName.Contains(query)) && m.Off_System_Id == user.DefaultSystemId
                           select m;
                return View(list);
            }
        }
        public ActionResult Off_BindSeller(int id)
        {
            var item = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Id == id);
            return PartialView(item);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Off_BindSeller(int id, FormCollection form)
        {
            Off_Membership_Bind item = new Off_Membership_Bind();
            if (TryUpdateModel(item))
            {
                item.Bind = true;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Content("SUCCESS");
            }
            else
            {
                return View("Error");
            }
        }
        #endregion

        #region 绑定促销门店
        public ActionResult Off_ScheduleList(bool? history)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            bool _history = history ?? true;
            var currentTime = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            if (_history)
            {
                var list = from m in offlineDB.Off_Checkin_Schedule
                           where m.Subscribe <= currentTime && m.Off_System_Id == user.DefaultSystemId
                           group m by m.Subscribe into g
                           orderby g.Key descending
                           select new ScheduleList { Subscribe = g.Key, Count = g.Count(), Unfinished = g.Count(m => m.Off_Checkin.Any(p => p.Status >= 3)) };
                return View(list);
            }
            else
            {
                var list = from m in offlineDB.Off_Checkin_Schedule
                           where m.Subscribe > currentTime && m.Off_System_Id == user.DefaultSystemId
                           group m by m.Subscribe into g
                           orderby g.Key
                           select new ScheduleList { Subscribe = g.Key, Count = g.Count(), Unfinished = g.Count(m => m.Off_Checkin.Any(p => p.Status >= 3)) };
                return View(list);
            }

        }
        public ActionResult Off_ScheduleDetails(string date, string query)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            DateTime day = DateTime.Parse(date);
            ViewBag.CurrentDate = date;
            if (query!=null)
            {
                var list = from m in offlineDB.Off_Checkin_Schedule
                           where m.Subscribe == day && m.Off_Store.StoreName.Contains(query) && m.Off_System_Id == user.DefaultSystemId
                           orderby m.Off_Store.StoreName
                           select m;
                return View(list);
            }
            else
            {
                var list = from m in offlineDB.Off_Checkin_Schedule
                           where m.Subscribe == day && m.Off_System_Id == user.DefaultSystemId
                           orderby m.Off_Store.StoreName
                           select m;
                return View(list);
            }
        }
        public ActionResult Ajax_EditSchedule(int id)
        {
            var item = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == id);
            ViewBag.StoreName = item.Off_Store.StoreName;
            return PartialView(item);
        }
        [ValidateAntiForgeryToken, HttpPost]
        public ActionResult Ajax_EditSchedule(Off_Checkin_Schedule model)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin_Schedule item = new Off_Checkin_Schedule();
                if (TryUpdateModel(item))
                {
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "请重试");
                ViewBag.StoreName = offlineDB.Off_Store.SingleOrDefault(m => m.Id == model.Off_Store_Id).StoreName;
                return PartialView(model);
            }
        }
        [HttpPost]
        public ActionResult Ajax_DeleteSchedule(int id)
        {
            var item = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == id);
            offlineDB.Off_Checkin_Schedule.Remove(item);
            offlineDB.SaveChanges();
            return Content("SUCCESS");
        }
        public ActionResult Off_Add_Schedule()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var storesystem = from m in offlineDB.Off_Store
                              where m.Off_System_Id == user.DefaultSystemId
                              group m by m.StoreSystem into g
                              orderby g.Key
                              select new { Key = g.Key, Value = g.Key };
            ViewBag.SystemList = new SelectList(storesystem, "Key", "Value", storesystem.FirstOrDefault().Value);
            return View();
        }
        [HttpPost]
        public ActionResult Off_Add_Schedule(FormCollection form)
        {
            StoreSchedule_ViewModel model = new StoreSchedule_ViewModel();
            if (TryUpdateModel(model))
            {
                if (ModelState.IsValid)
                {
                    if (model.StartDate > model.EndDate)
                    {
                        var storesystem = from m in offlineDB.Off_Store
                                          group m by m.StoreSystem into g
                                          orderby g.Key
                                          select new { Key = g.Key, Value = g.Key };
                        ViewBag.SystemList = new SelectList(storesystem, "Key", "Value", storesystem.FirstOrDefault().Value);
                        ModelState.AddModelError("", "开始日期不得大于结束日期");
                        return View(model);
                    }
                    if (form["StoreList"].ToString().Trim() == "")
                    {
                        var storesystem = from m in offlineDB.Off_Store
                                          group m by m.StoreSystem into g
                                          orderby g.Key
                                          select new { Key = g.Key, Value = g.Key };
                        ViewBag.SystemList = new SelectList(storesystem, "Key", "Value", storesystem.FirstOrDefault().Value);
                        ModelState.AddModelError("", "请至少选择一个店铺");
                        return View(model);
                    }
                    var datelength = Convert.ToInt32(model.EndDate.Subtract(model.StartDate).TotalDays);
                    // 每天循环
                    for (int i = 0; i <= datelength; i++)
                    {
                        var user = UserManager.FindById(User.Identity.GetUserId());
                        string[] storelist = form["StoreList"].ToString().Split(',');
                        for (int j = 0; j < storelist.Length; j++)
                        {
                            string[] begintime = model.BeginTime.Split(':');
                            string[] finishtime = model.FinishTime.Split(':');
                            int year = model.StartDate.AddDays(i).Year;
                            int month = model.StartDate.AddDays(i).Month;
                            int day = model.StartDate.AddDays(i).Day;
                            int storeid = Convert.ToInt32(storelist[j]);
                            DateTime subscribe = model.StartDate.AddDays(i);
                            var schedule = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Off_Store_Id == storeid && m.Subscribe == subscribe);
                            if (schedule == null)
                            {
                                schedule = new Off_Checkin_Schedule()
                                {
                                    Off_Store_Id =storeid,
                                    Subscribe = subscribe,
                                    Standard_CheckIn = new DateTime(year, month, day, Convert.ToInt32(begintime[0]), Convert.ToInt32(begintime[1]), 0),
                                    Standard_CheckOut = new DateTime(year, month, day, Convert.ToInt32(finishtime[0]), Convert.ToInt32(finishtime[1]), 0),
                                    Standard_Salary = model.Salary,
                                    TemplateId =1,
                                    Off_System_Id = user.DefaultSystemId
                                };
                                offlineDB.Off_Checkin_Schedule.Add(schedule);
                            }
                            else
                            {
                                schedule.Standard_CheckIn = new DateTime(year, month, day, Convert.ToInt32(begintime[0]), Convert.ToInt32(begintime[1]), 0);
                                schedule.Standard_CheckOut = new DateTime(year, month, day, Convert.ToInt32(finishtime[0]), Convert.ToInt32(finishtime[1]), 0);
                                schedule.Standard_Salary = model.Salary;
                                schedule.TemplateId = 1;
                                offlineDB.Entry(schedule).State = System.Data.Entity.EntityState.Modified;
                            }
                        }
                    }
                    offlineDB.SaveChanges();
                    return RedirectToAction("Off_ScheduleList");
                }
                else
                {
                    var storesystem = from m in offlineDB.Off_Store
                                      group m by m.StoreSystem into g
                                      orderby g.Key
                                      select new { Key = g.Key, Value = g.Key };
                    ViewBag.SystemList = new SelectList(storesystem, "Key", "Value", storesystem.FirstOrDefault().Value);
                    return View(model);
                }
            }
            return RedirectToAction("Off_ScheduleList");
        }
        [HttpPost]
        public JsonResult Off_Add_Schedule_StoreList(string storesystem)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in offlineDB.Off_Store
                       where m.StoreSystem == storesystem && m.Off_System_Id == user.DefaultSystemId
                       select new { ID = m.Id, StoreName = m.StoreName };
            return Json(new { StoreList = list });
        }
        #endregion

        #region 确认销售金额
        public ActionResult Off_ConfirmCheckIn(int CheckinId)
        {
            var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == CheckinId);
            bool attendance_late = false;
            bool attendance_early = false;
            if (item.CheckinTime >= item.Off_Checkin_Schedule.Standard_CheckIn)
                attendance_late = true;
            if (item.CheckoutTime <= item.Off_Checkin_Schedule.Standard_CheckOut)
                attendance_early = true;
            int attendance = 0;
            int debits = 0;
            if (attendance_early && attendance_late)
            {
                attendance = 3;
                debits = -40;
            }
            else if (attendance_early)
            {
                attendance = 2;
                debits = -20;
            }
            else if (attendance_late)
            {
                attendance = 1;
                debits = -20;
            }
            List<Object> attendance_list = new List<Object>();
            attendance_list.Add(new { Key = 0, Value = "全勤" });
            attendance_list.Add(new { Key = 1, Value = "迟到" });
            attendance_list.Add(new { Key = 2, Value = "早退" });
            attendance_list.Add(new { Key = 3, Value = "旷工" });
            ViewBag.Attendancelist = new SelectList(attendance_list, "Key", "Value", attendance);
            var user = UserManager.FindById(User.Identity.GetUserId());
            ViewBag.SystemId = user.DefaultSystemId;
            ConfirmCheckIn_ViewModel model = new ConfirmCheckIn_ViewModel()
            {
                CheckIn_Id = CheckinId,
                AttendanceStatus = attendance,
                Salary = item.Off_Checkin_Schedule.Standard_Salary,
                Bonus = item.Bonus,
                Debits = debits,
                Rep_Brown = item.Rep_Brown,
                Rep_Black = item.Rep_Black,
                Rep_Dates = item.Rep_Dates,
                Rep_Honey = item.Rep_Honey,
                Rep_Lemon = item.Rep_Lemon,
                Rep_Other = item.Rep_Other,
                Remark = item.Remark,
                Proxy = item.Proxy,
                Bonus_Remark = item.Bonus_Remark,
                Confirm_Remark = item.Confirm_Remark
            };
            ViewBag.CheckItem = item;
            return View(model);
        }
        [ValidateAntiForgeryToken, HttpPost]
        public ActionResult Off_ConfirmCheckIn(FormCollection form)
        {
            ConfirmCheckIn_ViewModel model = new ConfirmCheckIn_ViewModel();
            if (TryUpdateModel(model))
            {
                var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == model.CheckIn_Id);
                if (item.Status == 4)
                {
                    Off_SalesInfo_Daily info = new Off_SalesInfo_Daily()
                    {
                        Attendance = model.AttendanceStatus,
                        Date = item.Off_Checkin_Schedule.Subscribe,
                        Bonus = model.Bonus,
                        Debit = model.Debits,
                        isMultiple = false,
                        Item_Black = item.Rep_Black,
                        Item_Brown = item.Rep_Brown,
                        Item_Dates = item.Rep_Dates,
                        Item_Honey = item.Rep_Honey,
                        Item_Lemon = item.Rep_Lemon,
                        remarks = model.Remark,
                        SellerId = item.Off_Seller_Id,
                        Salary = model.Salary,
                        StoreId = item.Off_Checkin_Schedule.Off_Store_Id,
                        UploadTime = DateTime.Now,
                        UploadUser = User.Identity.Name
                    };
                    offlineDB.Off_SalesInfo_Daily.Add(info);
                    item.Status = 5;
                    item.Bonus = model.Bonus;
                    item.Bonus_Remark = model.Bonus_Remark;
                    item.SubmitTime = DateTime.Now;
                    item.SubmitUser = User.Identity.Name;
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return RedirectToAction("Off_CheckIn_List");
                }
                else if (item.Status >= 0 && item.Status <= 3)
                {
                    item.Status = 4;
                    item.Rep_Lemon = model.Rep_Lemon;
                    item.Rep_Brown = model.Rep_Brown;
                    item.Rep_Black = model.Rep_Black;
                    item.Rep_Honey = model.Rep_Honey;
                    item.Rep_Dates = model.Rep_Dates;
                    item.Rep_Other = model.Rep_Other;
                    item.Proxy = model.Proxy;
                    item.CheckinLocation = item.CheckinLocation ?? "N/A";
                    item.CheckoutLocation = item.CheckoutLocation ?? "N/A";
                    item.Confirm_Remark = model.Confirm_Remark;
                    item.ConfirmUser = User.Identity.Name;
                    item.ConfirmTime = DateTime.Now;
                    item.Proxy = true;
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return RedirectToAction("Off_CheckIn_List");
                }
                return View("Error");
            }
            else
            {
                return View("Error");
            }
        }
        public ActionResult Off_CreateCheckIn(int scheduleId)
        {

            Off_Checkin item = new Off_Checkin();
            item.Status = 3;
            item.Proxy = true;
            item.Off_Schedule_Id = scheduleId;
            var schedule = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == scheduleId);
            ViewBag.StoreName = schedule.Off_Store.StoreName;
            ViewBag.Subscribe = schedule.Subscribe.ToString("yyyy-MM-dd");

            var sellerlist = from m in offlineDB.Off_Seller
                             where m.StoreId == schedule.Off_Store_Id
                             select m;
            ViewBag.Sellerlist = new SelectList(sellerlist, "Id", "Name");
            return View(item);
        }
        [ValidateAntiForgeryToken, HttpPost]
        public ActionResult Off_CreateCheckIn(Off_Checkin model)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin item = new Off_Checkin();
                if (TryUpdateModel(item))
                {
                    var schedule = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == item.Off_Schedule_Id);
                    item.CheckinLocation = "N/A";
                    item.CheckinTime = null;
                    item.CheckoutTime = null;
                    item.CheckoutLocation = "N/A";
                    item.ConfirmTime = DateTime.Now;
                    item.ConfirmUser = User.Identity.Name;
                    item.Proxy = true;
                    item.Status = 4;
                    offlineDB.Off_Checkin.Add(item);
                    //offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return RedirectToAction("Off_CheckIn_List");
                }
            }
            return View(model);
        }
        public ActionResult Off_ProxyCheckIn(int checkid)
        {
            Off_Checkin item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == checkid);
            return View(item);
        }
        [ValidateAntiForgeryToken, HttpPost]
        public ActionResult Off_ProxyCheckIn(Off_Checkin model)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin item = new Off_Checkin();
                if (TryUpdateModel(item))
                {
                    //var schedule = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == item.Off_Schedule_Id);
                    item.CheckinLocation = "N/A";
                    item.CheckoutLocation = "N/A";
                    item.CheckinTime = null;
                    item.CheckoutTime = null;
                    item.ConfirmTime = DateTime.Now;
                    item.ConfirmUser = User.Identity.Name;
                    item.Proxy = true;
                    item.Status = 4;
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return RedirectToAction("Off_CheckIn_List");
                }
            }
            return View(model);
        }
        [HttpPost]
        public ActionResult Off_CreateCheckIn_FileUpload(FormCollection form)
        {
            var files = Request.Files;
            string msg = string.Empty;
            string error = string.Empty;
            string imgurl;
            if (files.Count > 0)
            {
                if (files[0].ContentLength > 0 && files[0].ContentType.Contains("image"))
                {
                    string filename = DateTime.Now.ToFileTime().ToString() + ".jpg";
                    //files[0].SaveAs(Server.MapPath("/Content/checkin-img/") + filename);
                    AliOSSUtilities util = new AliOSSUtilities();
                    util.PutObject(files[0].InputStream, "checkin-img/" + filename);
                    msg = "成功! 文件大小为:" + files[0].ContentLength;
                    imgurl = filename;
                    string res = "{ error:'" + error + "', msg:'" + msg + "',imgurl:'" + imgurl + "'}";
                    return Content(res);
                }
                else {
                    error = "文件错误"; 
                }
            }
            string err_res = "{ error:'" + error + "', msg:'" + msg + "',imgurl:''}";
            return Content(err_res);
        }

        public ActionResult Off_ConfirmCheckIn_Map(bool trans, string lbs)
        {
            ViewBag.LBS = lbs;
            ViewBag.Trans = trans;
            return PartialView();
        }
        [HttpPost]
        public ActionResult Off_CancelCheckIn(int id)
        {
            var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == id);
            if (item.Status >= 0 && item.Status < 5)
            {
                item.Status = -1;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Content("SUCCESS");
            }
            else
            {
                return Content("FAIL");
            }
        }
        public ActionResult Off_CheckIn_List()
        {
            return View();
        }
        public PartialViewResult Off_CheckIn_List_ajax(int? status, int? page, string query)
        {
            int _page = page ?? 1;
            int _status = status ?? 4;
            var user = UserManager.FindById(User.Identity.GetUserId());
            // 按照活动日期排序
            if (query == null)
            {
                var list = (from m in offlineDB.Off_Checkin
                            where m.Status == _status && m.Off_Checkin_Schedule.Off_System_Id == user.DefaultSystemId
                            orderby m.Off_Checkin_Schedule.Subscribe descending
                            select m).ToPagedList(_page, 50);
                return PartialView(list);
            }
            else
            {
                var list = (from m in offlineDB.Off_Checkin
                            where m.Status == _status && m.Off_Checkin_Schedule.Off_System_Id == user.DefaultSystemId
                            && (m.Off_Checkin_Schedule.Off_Store.StoreName.Contains(query) || m.Off_Seller.Name.Contains(query))
                            orderby m.Off_Checkin_Schedule.Subscribe descending
                            select m).ToPagedList(_page, 50);
                return PartialView(list);
            }
        }
        #endregion
        public ActionResult Off_UpdateManager(int id)
        {
            var item = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Id == id);
            var user = UserManager.FindByName(item.UserName);
            UserManager.RemoveFromRole(user.Id, "Seller");
            UserManager.AddToRole(user.Id, "Manager");
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            if (manager == null)
            {
                manager = new Off_StoreManager()
                {
                    UserName = user.UserName,
                    NickName = user.NickName,
                    Mobile = user.UserName,
                    Status = 1,
                    Off_System_Id = user.DefaultSystemId
                };
                offlineDB.Off_StoreManager.Add(manager);
            }
            item.Bind = false;
            item.Off_Seller_Id = null;
            //offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
            offlineDB.Off_Membership_Bind.Remove(item);
            offlineDB.SaveChanges();
            return Content("SUCCESS");
        }



        public ActionResult Off_CreateStore()
        {
            var store = new Off_Store();
            return PartialView(store);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Off_CreateStore(Off_Store model)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (ModelState.IsValid)
            {
                Off_Store item = new Off_Store();
                if (TryUpdateModel(item))
                {
                    item.UploadTime = DateTime.Now;
                    item.UploadUser = User.Identity.Name;
                    item.Off_System_Id = user.DefaultSystemId;
                    offlineDB.Off_Store.Add(item);
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return PartialView(model);
            }
        }
        //0308
        [HttpPost]
        public ActionResult Off_DeleteStore(int id)
        {
            var item = offlineDB.Off_Store.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                try
                {
                    offlineDB.Off_Store.Remove(item);
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                catch
                {
                    return Content("FAIL");
                }
            }
            return Content("FAIL");
        }
        public ActionResult Off_CreateSalesDaily()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var item = new Off_SalesInfo_Daily();
            var storelist = from m in offlineDB.Off_Store
                            where m.Off_System_Id == user.DefaultSystemId
                            orderby m.StoreName 
                            select new { Key = m.Id, Value = m.StoreName };
            ViewBag.StoreDropDown = new SelectList(storelist, "Key", "Value");
            return PartialView(item);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Off_CreateSalesDaily(Off_SalesInfo_Daily model)
        {
            if (ModelState.IsValid)
            {
                Off_SalesInfo_Daily item = new Off_SalesInfo_Daily();
                if (TryUpdateModel(item))
                {
                    item.UploadTime = DateTime.Now;
                    item.UploadUser = User.Identity.Name;
                    offlineDB.Off_SalesInfo_Daily.Add(item);
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                var storelist = from m in offlineDB.Off_Store
                                orderby m.StoreName
                                select new { Key = m.Id, Value = m.StoreName };
                ViewBag.StoreDropDown = new SelectList(storelist, "Key", "Value");
                return PartialView(model);
            }
        }
        [HttpPost]
        public ActionResult Off_Ajax_GetStoreSeller(int id)
        {
            var list = from m in offlineDB.Off_Seller
                       where m.StoreId == id
                       select new { Id = m.Id, Name = m.Name };
            return Json(new { result = "SUCCESS", data = list });
        }

        // 0308
        [HttpPost]
        public ActionResult Off_DeleteSalesDaily(int id)
        {
            var item = offlineDB.Off_SalesInfo_Daily.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                try
                {
                    offlineDB.Off_SalesInfo_Daily.Remove(item);
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                catch
                {
                    return Content("FAIL");
                }
            }
            return Content("FAIL");
        }

        public ActionResult Off_CreateSalesMonth()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var item = new Off_SalesInfo_Month();
            var storelist = from m in offlineDB.Off_Store
                            where m.Off_System_Id == user.DefaultSystemId
                            orderby m.StoreName
                            select new { Key = m.Id, Value = m.StoreName };
            ViewBag.StoreDropDown = new SelectList(storelist, "Key", "Value");
            return PartialView(item);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Off_CreateSalesMonth(Off_SalesInfo_Month model)
        {
            if (ModelState.IsValid)
            {
                Off_SalesInfo_Month item = new Off_SalesInfo_Month();
                if (TryUpdateModel(item))
                {
                    item.UploadTime = DateTime.Now;
                    item.UploadUser = User.Identity.Name;
                    offlineDB.Off_SalesInfo_Month.Add(item);
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                var storelist = from m in offlineDB.Off_Store
                                orderby m.StoreName
                                select new { Key = m.Id, Value = m.StoreName };
                ViewBag.StoreDropDown = new SelectList(storelist, "Key", "Value");
                return PartialView(model);
            }
        }
        // 0308
        [HttpPost]
        public ActionResult Off_DeleteSalesMonth(int id)
        {
            var item = offlineDB.Off_SalesInfo_Month.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                try
                {
                    offlineDB.Off_SalesInfo_Month.Remove(item);
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                catch
                {
                    return Content("FAIL");
                }
            }
            return Content("FAIL");
        }

        // 0309 删除促销员信息 /OfflineSales/Off_DeleteSeller
        [HttpPost]
        public ActionResult Off_DeleteSeller(int id)
        {
            var item = offlineDB.Off_Seller.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                try
                {
                    offlineDB.Off_Seller.Remove(item);
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                catch
                {
                    return Content("FAIL");
                }
            }
            return Content("FAIL");
        }
        // 0309 查询促销签到记录 /OfflineSales/Off_QueryCheckIn
        public ActionResult Off_QueryCheckIn(DateTime? start, DateTime? end, string query)
        {
            DateTime _end = end ?? Convert.ToDateTime(DateTime.Now.ToShortDateString());
            DateTime _start = start ?? _end.AddDays(-1);
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in offlineDB.Off_Checkin
                       where m.Off_Checkin_Schedule.Off_System_Id == user.DefaultSystemId
                       && m.Off_Checkin_Schedule.Subscribe >= _start &&
                       m.Off_Checkin_Schedule.Subscribe <= _end &&
                       (m.Off_Checkin_Schedule.Off_Store.StoreName.Contains(query) || m.Off_Seller.Name.Contains(query))
                       select m;
            return View(list);
        }

        // 0310 管理员列表
        public ActionResult Off_Manager_List()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in offlineDB.Off_StoreManager
                       where m.Off_System_Id == user.DefaultSystemId
                       select m;
            return View(list);
        }

        // 0310 升级为超级管理员
        public ActionResult Off_Manager_UpdateSenior(int id)
        {
            var item = offlineDB.Off_StoreManager.SingleOrDefault(m => m.Id == id);
            var user = UserManager.FindByName(item.UserName);
            UserManager.AddToRole(user.Id, "Senior");
            item.Status = 2;
            offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
            offlineDB.SaveChanges();
            return Content("SUCCESS");
        }

        public ActionResult Off_Manager_AddStore(int id)
        {
            //var item = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Id == id);
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.Id == id);
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (manager != null)
            {
                ViewBag.StoreList = manager.Off_Store.OrderBy(m => m.StoreName);
                var storesystem = from m in offlineDB.Off_Store
                                  where m.Off_System_Id == user.DefaultSystemId
                                  group m by m.StoreSystem into g
                                  orderby g.Key
                                  select new { Key = g.Key, Value = g.Key };
                ViewBag.SystemList = new SelectList(storesystem, "Key", "Value", storesystem.FirstOrDefault().Value);
                ViewBag.Name = manager.NickName;
                ViewBag.ManagerId = manager.Id;
                return View();
            }
            else
            {
                return View();
            }
        }
        public JsonResult Off_GetStoreName(int storeid)
        {
            var item = offlineDB.Off_Store.SingleOrDefault(m => m.Id == storeid).StoreName;
            return Json(new { id = storeid, name = item }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Off_Manager_AjaxAddStore(int managerId, string arr_list)
        {
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.Id == managerId);
            var currentlist = manager.Off_Store.Select(m => m.Id);
            string[] arr_temp = arr_list.Split(',');
            List<int> arr_int = new List<int>();
            foreach (var s in arr_temp)
            {
                arr_int.Add(Convert.ToInt32(s));
            }
            var select_list = (from m in offlineDB.Off_Store
                               where arr_int.Contains(m.Id)
                               select m).Select(m => m.Id);
            var storelist = (from m in manager.Off_Store
                             select m.Id).ToList();
            foreach(var item in storelist)
            {
                var temp = offlineDB.Off_Store.SingleOrDefault(m => m.Id == item);
                manager.Off_Store.Remove(temp);
            }
            
            foreach (var item2 in select_list)
            {
                manager.Off_Store.Add(offlineDB.Off_Store.SingleOrDefault(m => m.Id == item2));
            }
            offlineDB.Entry(manager).State = System.Data.Entity.EntityState.Modified;
            offlineDB.SaveChanges();
            return Content("SUCCESS");
        }
        // 0314 降级为普通管理员
        public ActionResult Off_Manager_ReduceManager(int id)
        {
            var item = offlineDB.Off_StoreManager.SingleOrDefault(m => m.Id == id);
            var user = UserManager.FindByName(item.UserName);
            UserManager.RemoveFromRole(user.Id, "Senior");
            item.Status = 1;
            offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
            offlineDB.SaveChanges();
            return Content("SUCCESS");
        }
        // 0314 督导工作汇报列表
        public ActionResult Off_Manager_TaskList()
        {
            return View();
        }
        public PartialViewResult Off_Manager_TaskList_Ajax(string query, int? status, int? page)
        {
            int _status = status ?? 0;
            int _page = page ?? 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (query == null)
            {
                var list = (from m in offlineDB.Off_Manager_Task
                            where m.Status == _status && m.Off_System_Id == user.DefaultSystemId
                            orderby m.TaskDate descending
                            select m).ToPagedList(_page, 30);
                return PartialView(list);
            }
            else
            {
                var list = (from m in offlineDB.Off_Manager_Task
                            where m.Status == _status && m.Off_System_Id == user.DefaultSystemId
                            && m.NickName.Contains(query)
                            orderby m.TaskDate descending
                            select m).ToPagedList(_page, 30);
                return PartialView(list);
            }
        }

        // 0314 审核督导工作
        public ActionResult Off_Manager_EditTask(int id)
        {
            var item = offlineDB.Off_Manager_Task.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                return View(item);
            }
            else
            {
                return View("Error");
            }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Off_Manager_EditTask(Off_Manager_Task model)
        {

            if (ModelState.IsValid)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                var item = new Off_Manager_Task();
                if (TryUpdateModel(item))
                {
                    item.Off_System_Id = user.DefaultSystemId;
                    item.Eval_User = User.Identity.Name;
                    item.Eval_Time = DateTime.Now;
                    item.Status = (int)ManagerTaskStatus.Confirmed;
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return RedirectToAction("Off_Manager_TaskList");
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "错误");
                return View(model);
            }
        }

        // 0314 作废当日工作内容
        public ActionResult Off_Manager_CancelTask(int id)
        {
            var item = offlineDB.Off_Manager_Task.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                item.Status = (int)ManagerTaskStatus.Canceled;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Content("SUCCESS");
            }
            return Content("FAIL");
        }
        // 0325 公告列表
        public ActionResult Off_Manager_Announcement_List()
        {
            return View();
        }

        public ActionResult Off_Manager_Announcement_List_Ajax(int? page)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            int _page = page ?? 1;
            var list = (from m in offlineDB.Off_Manager_Announcement
                        where m.Off_System_Id == user.DefaultSystemId
                        orderby m.Priority descending, m.FinishTime descending
                        select m).ToPagedList(_page, 30);
            ViewBag.SystemId = user.DefaultSystemId;
            return PartialView(list);
        }
        // 0325 添加公告
        public ActionResult Off_Manager_Announcement_Create()
        {
            Off_Manager_Announcement model = new Off_Manager_Announcement();
            return View(model);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Off_Manager_Announcement_Create(Off_Manager_Announcement model)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                Off_Manager_Announcement item = new Off_Manager_Announcement();
                if (TryUpdateModel(item))
                {
                    item.SubmitTime = DateTime.Now;
                    item.SubmitUser = User.Identity.Name;
                    item.Off_System_Id = user.DefaultSystemId;
                    offlineDB.Off_Manager_Announcement.Add(item);
                    offlineDB.SaveChanges();
                    return RedirectToAction("Off_Manager_Announcement_List");
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return View(model);
            }
        }

        // 0325 修改公告
        public ActionResult Off_Manager_Announcement_Edit(int id)
        {
            Off_Manager_Announcement model = offlineDB.Off_Manager_Announcement.SingleOrDefault(m => m.Id == id);
            if (model != null)
                return View(model);
            else
                return View("Error");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Off_Manager_Announcement_Edit(Off_Manager_Announcement model)
        {
            if (ModelState.IsValid)
            {
                Off_Manager_Announcement item = new Off_Manager_Announcement();
                if (TryUpdateModel(item))
                {
                    item.SubmitTime = DateTime.Now;
                    item.SubmitUser = User.Identity.Name;
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return RedirectToAction("Off_Manager_Announcement_List");
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return View(model);
            }
        }

        // 0325 删除公告
        [HttpPost]
        public ActionResult Off_Manager_Announcement_Delete_Ajax(int id)
        {
            Off_Manager_Announcement model = offlineDB.Off_Manager_Announcement.SingleOrDefault(m => m.Id == id);
            if (model != null)
            {
                offlineDB.Off_Manager_Announcement.Remove(model);
                offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            return Json(new { result = "FAIL" });
        }

        // 0325 获取督导列表
        [HttpPost]
        public ActionResult Off_Manager_List_Ajax()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in offlineDB.Off_StoreManager
                       where m.Off_System_Id == user.DefaultSystemId
                       select new { UserName = m.UserName, NickName = m.NickName };
            return Json(new { result = "SUCCESS", managerlist = list });
        }

        // 0329 获取需求列表
        public ActionResult Off_Manager_Request_List()
        {
            
            return View();
        }

        public ActionResult Off_Manager_Request_List_Ajax(int? page)
        {
            var _page = page ?? 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = (from m in offlineDB.Off_Manager_Request
                        where m.Status >= 0 && m.Off_Store.Off_System_Id== user.DefaultSystemId
                        orderby m.Status, m.Id descending
                        select m).ToPagedList(_page, 20);
            ViewBag.SystemId = user.DefaultSystemId;
            return PartialView(list);
        }

        // 0329 修改需求详情
        public ActionResult Off_Manager_Request_Edit(int id)
        {
            Off_Manager_Request model = offlineDB.Off_Manager_Request.SingleOrDefault(m => m.Id == id);
            if (model != null)
                return View(model);
            else
                return View("Error");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Off_Manager_Request_Edit(Off_Manager_Request model)
        {
            if (ModelState.IsValid)
            {
                Off_Manager_Request item = new Off_Manager_Request();
                if (TryUpdateModel(item))
                {
                    item.Status = 2;
                    item.ReplyTime = DateTime.Now;
                    item.ReplyUser = User.Identity.Name;
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return RedirectToAction("Off_Manager_Request_List");
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return View(model);
            }
        }

        // 0329 驳回需求
        [HttpPost]
        public ActionResult Off_Manager_Request_Dismiss_Ajax(int id)
        {
            Off_Manager_Request model = offlineDB.Off_Manager_Request.SingleOrDefault(m => m.Id == id);
            if (model != null)
            {
                model.Status = 3;
                model.ReplyUser = User.Identity.Name;
                model.ReplyTime = DateTime.Now;
                offlineDB.Entry(model).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            return Json(new { result = "FAIL" });
        }
        // 0407 活动统计页面
        public ActionResult Off_Schedule_Statistic(string datetime)
        {
            ViewBag.CurrentDate = datetime;
            return View();
        }

        // 0407 活动数据AJAX
        [HttpPost]
        public JsonResult Off_Schedule_Statistic_Ajax(string datetime)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            DateTime targetDate = Convert.ToDateTime(datetime);
            var schedulelist = from m in offlineDB.Off_Checkin_Schedule
                               where m.Subscribe == targetDate && m.Off_System_Id == user.DefaultSystemId
                               select m;
            int self = schedulelist.Count(g => g.Off_Checkin.Any(m => m.Status >= 3 && !m.Proxy));
            int proxy = schedulelist.Count(g => g.Off_Checkin.Any(m => m.Status >= 3 && m.Proxy));
            int rest = schedulelist.Count() - self - proxy;
            return Json(new { result = "SUCCESS", totalcount = schedulelist.Count(), selfcount = self, proxycount = proxy, restcount = rest });
        }
        // 0413 统计数据-渠道数据
        public ActionResult Off_Statistic_StoreSystem()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var storesystem = from m in offlineDB.Off_Store
                              where m.Off_System_Id == user.DefaultSystemId
                              group m by m.StoreSystem into g
                              orderby g.Key
                              select new { Key = g.Key, Value = g.Key };
            ViewBag.SystemList = new SelectList(storesystem, "Key", "Value", storesystem.FirstOrDefault().Value);
            return View();
        }
        public JsonResult Off_Statistic_StoreSystem_Ajax(string startdate, string enddate, string storesystem)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            DateTime st = Convert.ToDateTime(startdate);
            DateTime et = Convert.ToDateTime(enddate);
            var data = from m in offlineDB.Off_SalesInfo_Daily
                       where m.Date >= st && m.Date <= et && m.Off_Store.StoreSystem.Contains(storesystem) && m.Off_Store.Off_System_Id == user.DefaultSystemId
                       group m by m.Date into g
                       select new { date = g.Key, count = g.Count(), brown = g.Sum(m => m.Item_Brown), black = g.Sum(m => m.Item_Black), lemon = g.Sum(m => m.Item_Lemon), honey = g.Sum(m => m.Item_Honey), dates = g.Sum(m => m.Item_Dates) };
            return Json(new { result = "SUCCESS", data = data }, JsonRequestBehavior.AllowGet);

        }
        // 0413 统计数据-门店数据
        public ActionResult Off_Statistic_Store()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var storesystem = from m in offlineDB.Off_Store
                              where m.Off_System_Id == user.DefaultSystemId
                              group m by m.StoreSystem into g
                              orderby g.Key
                              select new { Key = g.Key, Value = g.Key };
            ViewBag.SystemList = new SelectList(storesystem, "Key", "Value", storesystem.FirstOrDefault().Value);
            return View();
        }
        public JsonResult Off_Statistic_Store_Ajax(string startdate, string enddate, int storeid)
        {
            DateTime st = Convert.ToDateTime(startdate);
            DateTime et = Convert.ToDateTime(enddate);
            var user = UserManager.FindById(User.Identity.GetUserId());
            var data = from m in offlineDB.Off_SalesInfo_Daily
                       where m.Date >= st && m.Date <= et && m.StoreId == storeid && m.Off_Store.Off_System_Id == user.DefaultSystemId
                       group m by m.Date into g
                       select new { date = g.Key, count = g.Count(), brown = g.Sum(m => m.Item_Brown), black = g.Sum(m => m.Item_Black), lemon = g.Sum(m => m.Item_Lemon), honey = g.Sum(m => m.Item_Honey), dates = g.Sum(m => m.Item_Dates) };
            return Json(new { result = "SUCCESS", data = data }, JsonRequestBehavior.AllowGet);
        }
        // 0413 统计数据-促销员数据
        public ActionResult Off_Statistic_Seller()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var storesystem = from m in offlineDB.Off_Store
                              where m.Off_System_Id == user.DefaultSystemId
                              group m by m.StoreSystem into g
                              orderby g.Key
                              select new { Key = g.Key, Value = g.Key };
            ViewBag.SystemList = new SelectList(storesystem, "Key", "Value", storesystem.FirstOrDefault().Value);
            return View();
        }
        public JsonResult Off_Statistic_Seller_Ajax(string startdate, string enddate, int sellerid)
        {
            string st = Convert.ToDateTime(startdate).ToString("yyyy-MM-dd");
            string et = Convert.ToDateTime(enddate).ToString("yyyy-MM-dd");
            string sql = "SET DATEFIRST 1;" + "Select Date, isnull(Item_Brown, 0) as Item_Brown, isnull(Item_Black, 0) as Item_Black, isnull(Item_Lemon,0) as Item_Lemon, isnull(Item_Honey,0) as Item_Honey, isnull(Item_Dates,0) as Item_Dates," +
                " AVG_BROWN, AVG_BLACK, AVG_LEMON, AVG_HONEY, AVG_DATES" +
                " from (SELECT Date, DATEPART(DW, T1.Date) as DW, T1.StoreId, Item_Brown, Item_Black, Item_Lemon, Item_Honey, Item_Dates" +
                " FROM Off_SalesInfo_Daily as T1" +
                " where Date >= '" + st + "' and Date <= '" + et + "' and SellerId = " + sellerid + ") as T3 left join Off_AVG_SalesData as T4" +
                " on T3.DW = T4.DayOfWeek and T4.StoreId = T3.StoreId" +
                " order by T3.Date";
            var data = offlineDB.Database.SqlQuery<Seller_Statistic>(sql);
            return Json(new { result = "SUCCESS", data = data }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Off_Statistic_QuerySeller_Ajax(string query)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = (from m in offlineDB.Off_Seller
                        where (m.Name.Contains(query) || m.Off_Store.StoreName.Contains(query)) && m.Off_System_Id == user.DefaultSystemId
                        select new { value = m.Id, label = m.Name, desc = m.Off_Store.StoreName}).Take(5);
            return Json(new { result = "SUCCESS", data = list }, JsonRequestBehavior.AllowGet);
        }


        // 数据库更新辅助程序 更新完成后删除
        #region v1.3 辅助程序
        // 01-添加商品
        public ContentResult AddProduct()
        {
            var product_brown = offlineDB.Off_Product.SingleOrDefault(m => m.ItemCode == "sqz122");
            if (product_brown == null)
            {
                product_brown = new Off_Product()
                {
                    ItemCode = "sqz122",
                    ItemName = "寿全斋红糖姜茶120g纸盒装",
                    Off_System_Id = 1,
                    SalesPrice = Convert.ToDecimal(19.8),
                    status = 0
                };
                offlineDB.Off_Product.Add(product_brown);
            }
            var product_black = offlineDB.Off_Product.SingleOrDefault(m => m.ItemCode == "sqz123");
            if (product_black == null)
            {
                product_black = new Off_Product()
                {
                    ItemCode = "sqz123",
                    ItemName = "寿全斋黑糖姜茶120g纸盒装",
                    Off_System_Id = 1,
                    SalesPrice = Convert.ToDecimal(19.8),
                    status = 0
                };
                offlineDB.Off_Product.Add(product_black);
            }
            var product_lemon = offlineDB.Off_Product.SingleOrDefault(m => m.ItemCode == "sqz125");
            if (product_lemon == null)
            {
                product_lemon = new Off_Product()
                {
                    ItemCode = "sqz125",
                    ItemName = "寿全斋柠檬姜茶120g纸盒装",
                    Off_System_Id = 1,
                    SalesPrice = Convert.ToDecimal(19.8),
                    status = 0
                };
                offlineDB.Off_Product.Add(product_lemon);
            }
            var product_honey = offlineDB.Off_Product.SingleOrDefault(m => m.ItemCode == "sqz124");
            if (product_honey == null)
            {
                product_honey = new Off_Product()
                {
                    ItemCode = "sqz124",
                    ItemName = "寿全斋蜂蜜姜茶120g纸盒装",
                    Off_System_Id = 1,
                    SalesPrice = Convert.ToDecimal(19.8),
                    status = 0
                };
                offlineDB.Off_Product.Add(product_honey);
            }
            var product_dates = offlineDB.Off_Product.SingleOrDefault(m => m.ItemCode == "sqz126");
            if (product_dates == null)
            {
                product_dates = new Off_Product()
                {
                    ItemCode = "sqz126",
                    ItemName = "寿全斋红枣姜茶120g纸盒装",
                    Off_System_Id = 1,
                    SalesPrice = Convert.ToDecimal(19.8),
                    status = 0
                };
                offlineDB.Off_Product.Add(product_dates);
            }
            offlineDB.SaveChanges();
            return Content("SUCCESS");
        }
        // 02-签到数据
        public ContentResult migrate_checkin()
        {
            var list = from m in offlineDB.Off_Checkin
                       where m.Off_Checkin_Schedule.Off_System_Id == 1
                       select m;
            foreach(var item in list)
            {
                if (item.Rep_Brown != null&& item.Rep_Brown>0)
                {
                    var product = offlineDB.Off_Product.SingleOrDefault(m => m.ItemCode == "sqz122");
                    var itemBrown = new Off_Checkin_Product()
                    {
                        Off_Checkin = item,
                        ItemCode = "sqz122",
                        Off_Product = product,
                        SalesCount = item.Rep_Brown
                    };
                    offlineDB.Off_Checkin_Product.Add(itemBrown);
                }
                if (item.Rep_Black != null && item.Rep_Black>0)
                {
                    var product = offlineDB.Off_Product.SingleOrDefault(m => m.ItemCode == "sqz123");
                    var itemBlack = new Off_Checkin_Product()
                    {
                        Off_Checkin = item,
                        ItemCode = "sqz123",
                        Off_Product = product,
                        SalesCount = item.Rep_Black
                    };
                    offlineDB.Off_Checkin_Product.Add(itemBlack);
                }
                if (item.Rep_Lemon != null && item.Rep_Lemon>0)
                {
                    var product = offlineDB.Off_Product.SingleOrDefault(m => m.ItemCode == "sqz125");
                    var itemLemon = new Off_Checkin_Product()
                    {
                        Off_Checkin = item,
                        ItemCode = "sqz125",
                        Off_Product = product,
                        SalesCount = item.Rep_Lemon
                    };
                    offlineDB.Off_Checkin_Product.Add(itemLemon);
                }
                if (item.Rep_Honey != null && item.Rep_Honey >0)
                {
                    var product = offlineDB.Off_Product.SingleOrDefault(m => m.ItemCode == "sqz124");
                    var itemHoney = new Off_Checkin_Product()
                    {
                        Off_Checkin = item,
                        ItemCode = "sqz124",
                        Off_Product = product,
                        SalesCount = item.Rep_Honey
                    };
                    offlineDB.Off_Checkin_Product.Add(itemHoney);
                }
                if (item.Rep_Dates != null && item.Rep_Dates>0)
                {
                    var product = offlineDB.Off_Product.SingleOrDefault(m => m.ItemCode == "sqz126");
                    var itemDates = new Off_Checkin_Product()
                    {
                        Off_Checkin = item,
                        ItemCode = "sqz126",
                        Off_Product = product,
                        SalesCount = item.Rep_Dates
                    };
                    offlineDB.Off_Checkin_Product.Add(itemDates);
                }
                //offlineDB.SaveChanges();
            }
            offlineDB.SaveChanges();
            return Content("SUCCESS");
        }
        // 03-每日数据
        public ContentResult migrate_daily()
        {
            var list = from m in offlineDB.Off_SalesInfo_Daily
                       where m.Off_Store.Off_System_Id == 1
                       select m;
            foreach (var item in list)
            {
                if (item.Item_Brown != null && item.Item_Brown > 0)
                {
                    var product = offlineDB.Off_Product.SingleOrDefault(m => m.ItemCode == "sqz122");
                    var itemBrown = new Off_Daily_Product()
                    {
                        Off_SalesInfo_Daily = item,
                        ItemCode = "sqz122",
                        Off_Product = product,
                        SalesCount = item.Item_Brown
                    };
                    offlineDB.Off_Daily_Product.Add(itemBrown);
                }
                if (item.Item_Black != null && item.Item_Black > 0)
                {
                    var product = offlineDB.Off_Product.SingleOrDefault(m => m.ItemCode == "sqz123");
                    var itemBlack = new Off_Daily_Product()
                    {
                        Off_SalesInfo_Daily = item,
                        ItemCode = "sqz123",
                        Off_Product = product,
                        SalesCount = item.Item_Black
                    };
                    offlineDB.Off_Daily_Product.Add(itemBlack);
                }
                if (item.Item_Lemon != null && item.Item_Lemon > 0)
                {
                    var product = offlineDB.Off_Product.SingleOrDefault(m => m.ItemCode == "sqz125");
                    var itemLemon = new Off_Daily_Product()
                    {
                        Off_SalesInfo_Daily = item,
                        ItemCode = "sqz125",
                        Off_Product = product,
                        SalesCount = item.Item_Lemon
                    };
                    offlineDB.Off_Daily_Product.Add(itemLemon);
                }
                if (item.Item_Honey != null && item.Item_Honey > 0)
                {
                    var product = offlineDB.Off_Product.SingleOrDefault(m => m.ItemCode == "sqz124");
                    var itemHoney = new Off_Daily_Product()
                    {
                        Off_SalesInfo_Daily = item,
                        ItemCode = "sqz124",
                        Off_Product = product,
                        SalesCount = item.Item_Honey
                    };
                    offlineDB.Off_Daily_Product.Add(itemHoney);
                }
                if (item.Item_Dates != null && item.Item_Dates > 0)
                {
                    var product = offlineDB.Off_Product.SingleOrDefault(m => m.ItemCode == "sqz126");
                    var itemDates = new Off_Daily_Product()
                    {
                        Off_SalesInfo_Daily = item,
                        ItemCode = "sqz126",
                        Off_Product = product,
                        SalesCount = item.Item_Dates
                    };
                    offlineDB.Off_Daily_Product.Add(itemDates);
                }
                //offlineDB.SaveChanges();
            }
            offlineDB.SaveChanges();
            return Content("SUCCESS");
        }
        #endregion

        // 产品 查增改删
        // 0509 产品列表
        public ActionResult Off_Product_List()
        {
            return View();
        }

        public ActionResult Off_Product_List_Ajax(int? page)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            int _page = page ?? 1;
            var list = (from m in offlineDB.Off_Product
                        where m.Off_System_Id == user.DefaultSystemId
                        orderby m.ItemCode
                        select m).ToPagedList(_page, 30);
            return PartialView(list);
        }
        // 0509 添加产品
        public ActionResult Off_Product_Create()
        {
            Off_Product model = new Off_Product();
            return PartialView(model);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Off_Product_Create(Off_Product model)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                Off_Product item = new Off_Product();
                if (TryUpdateModel(item))
                {
                    item.Off_System_Id = user.DefaultSystemId;
                    offlineDB.Off_Product.Add(item);
                    offlineDB.SaveChanges();
                    return RedirectToAction("Off_Product_List");
                }
                return PartialView("Error");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return PartialView(model);
            }
        }

        // 0509 修改产品
        public ActionResult Off_Product_Edit(int id)
        {
            Off_Product model = offlineDB.Off_Product.SingleOrDefault(m => m.Id == id);
            if (model != null)
                return PartialView(model);
            else
                return PartialView("Error");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Off_Product_Edit(Off_Product model)
        {
            if (ModelState.IsValid)
            {
                Off_Product item = new Off_Product();
                if (TryUpdateModel(item))
                {
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return RedirectToAction("Off_Product_List");
                }
                return PartialView("Error");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return PartialView(model);
            }
        }

        // 0509 删除产品
        [HttpPost]
        public ActionResult Off_Product_Delete_Ajax(int id)
        {
            Off_Product model = offlineDB.Off_Product.SingleOrDefault(m => m.Id == id);
            if (model != null)
            {
                offlineDB.Off_Product.Remove(model);
                offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            return Json(new { result = "FAIL" });
        }

        public static string getManagerNickName(string username, int systemId)
        {
            //
            if (username == null)
            {
                return "";
            }
            OfflineSales offlineDB = new OfflineSales();
            var item = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == username && m.Off_System_Id == systemId);
            if (item !=null)
            return item.NickName;
            else
            {
                return username;
            }
        }
        public static string getManagerNickNameCollection(string usernames, int systemId)
        {
            string[] names = usernames.Split(',');
            string[] nicknames = new string[names.Length];
            for(int i = 0; i < names.Length; i++)
            {
                nicknames[i] = getManagerNickName(names[i], systemId);
            }
            return string.Join(",", nicknames);
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

    public class Form_Product_Details
    {
        public string Product_Code { get; set; }
        public int? ReportNum { get; set; }
        public int? CheckNum { get; set; }
        public string Product_Name { get; set; }
        public Form_Product_Details()
        {

        }
    }
    public class Form_Product_Sales_Month
    {
        public string Product_Code { get; set; }
        public int? Sales_Count { get; set; }
        public decimal? Sales_Amount { get; set; }
        public string Product_Name { get; set; }
        public Form_Product_Sales_Month()
        {

        }
    }
    public class StoreView
    {
        public string StoreName { get; set; }
        public string StoreSystem
        {
            get; set;
        }
        public string Address
        {
            get; set;
        }
        public string Longitude
        {
            get; set;
        }
        public string Latitude
        {
            get; set;
        }
    }
    public class ScheduleList
    {
        public DateTime Subscribe { get; set; }
        public int Count { get; set; }
        public int Unfinished { get; set; }
    }
    public class Excel_DataMessage
    {
        public int line;
        public string message;
        public bool error;
        public Excel_DataMessage(int line, string message, bool error)
        {
            this.line = line;
            this.message = message;
            this.error = error;
        }
    }
    public class Seller_Statistic
    {
        public DateTime Date { get; set; }
        public int? Item_Brown { get; set; }
        public int? Item_Black { get; set; }
        public int? Item_Lemon { get; set; }
        public int? Item_Honey { get; set; }
        public int? Item_Dates { get; set; }
        public decimal? AVG_BROWN { get; set; }
        public decimal? AVG_BLACK { get; set; }
        public decimal? AVG_LEMON { get; set; }
        public decimal? AVG_HONEY { get; set; }
        public decimal? AVG_DATES { get; set; }
    }
    public enum Expenses_Name
    {
        [Display(Name = "进场费")]
        进场费,
        [Display(Name = "促销员工资")]
        促销员工资,
        [Display(Name = "促销员奖金")]
        促销员奖金,
        [Display(Name = "海报费")]
        海报费,
        [Display(Name = "端架费用")]
        端架费用,
        [Display(Name = "TG费用")]
        TG费用,
        [Display(Name = "地堆费用")]
        地堆费用,
        [Display(Name = "运输费")]
        运输费,
        [Display(Name = "试吃物料")]
        试吃物料,
        [Display(Name = "其他赠品")]
        其他赠品,
        [Display(Name = "公司赠品")]
        公司赠品,
        [Display(Name = "POSM")]
        POSM,
        [Display(Name = "其他费用")]
        其他费用
    }
}
