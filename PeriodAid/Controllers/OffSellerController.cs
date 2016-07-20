using System;
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
using System.IO;
using CsvHelper;

namespace PeriodAid.Controllers
{
    public class OffSellerController : Controller
    {
        OfflineSales _offlineDB = new OfflineSales();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public OffSellerController()
        {

        }

        public OffSellerController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: OffSeller
        public ActionResult Index()
        {
            return View();
        }


        // Origin:Off_Seller_main
        public ActionResult SellerIndex()
        {
            return View();
        }

        // Origin: Off_Seller_ajaxlist
        public ActionResult SellerListPartial(int? page, string query)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            int _page = page ?? 1;
            if (query == null || query == "")
            {
                var list = (from m in _offlineDB.Off_Seller
                            where m.Off_System_Id == user.DefaultSystemId
                            orderby m.Id descending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
            else
            {
                var list = (from m in _offlineDB.Off_Seller
                            where (m.Name.Contains(query) || m.Off_Store.StoreName.Contains(query)) && m.Off_System_Id == user.DefaultSystemId
                            orderby m.Id descending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
        }
        // Origin: Ajax_AddSeller
        public PartialViewResult AddSellerPartial()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var storelist = _offlineDB.Off_Store.Where(m => m.Off_System_Id == user.DefaultSystemId).OrderBy(m => m.StoreName);
            ViewBag.Storelist = new SelectList(storelist, "Id", "StoreName");
            Off_Seller seller = new Off_Seller();
            seller.Off_System_Id = user.DefaultSystemId;
            var banklistArray = _offlineDB.Off_System_Setting.SingleOrDefault(m => m.Off_System_Id == user.DefaultSystemId && m.SettingName == "BankList");
            if (banklistArray != null)
            {
                string[] regionarray = banklistArray.SettingValue.Split(',');
                List<Object> banklist = new List<object>();
                foreach (var i in regionarray)
                {
                    banklist.Add(new { Key = i, Value = i });
                }
                ViewBag.BankList = new SelectList(banklist, "Key", "Value");
                return PartialView(seller);
            }
            else
                return PartialView("PartialError");
        }
        [HttpPost]
        public ActionResult AddSellerPartial(Off_Seller model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                var item = new Off_Seller();
                if (TryUpdateModel(item))
                {
                    var user = UserManager.FindById(User.Identity.GetUserId());
                    item.UploadUser = User.Identity.Name;
                    item.UploadTime = DateTime.Now;
                    item.Off_System_Id = user.DefaultSystemId;
                    _offlineDB.Off_Seller.Add(item);
                    _offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                else
                    return PartialView("PartialError");
            }
            else
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                ModelState.AddModelError("", "错误");
                var storelist = _offlineDB.Off_Store.Where(m => m.Off_System_Id == user.DefaultSystemId).OrderBy(m => m.StoreName);
                ViewBag.Storelist = new SelectList(storelist, "Id", "StoreName");
                var banklistArray = _offlineDB.Off_System_Setting.SingleOrDefault(m => m.Off_System_Id == user.DefaultSystemId && m.SettingName == "BankList");
                if (banklistArray != null)
                {
                    string[] regionarray = banklistArray.SettingValue.Split(',');
                    List<Object> banklist = new List<object>();
                    foreach (var i in regionarray)
                    {
                        banklist.Add(new { Key = i, Value = i });
                    }
                    ViewBag.BankList = new SelectList(banklist, "Key", "Value");
                    return PartialView(model);
                }
                else
                    return PartialView("PartialError");
            }
        }

        // Origin:Ajax_EditSeller
        public PartialViewResult EditSellerPartial(int id)
        {
            var item = _offlineDB.Off_Seller.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_System_Id == user.DefaultSystemId)
                {
                    var banklistArray = _offlineDB.Off_System_Setting.SingleOrDefault(m => m.Off_System_Id == user.DefaultSystemId && m.SettingName == "BankList");
                    if (banklistArray != null)
                    {
                        string[] regionarray = banklistArray.SettingValue.Split(',');
                        List<Object> banklist = new List<object>();
                        foreach (var i in regionarray)
                        {
                            banklist.Add(new { Key = i, Value = i });
                        }
                        ViewBag.BankList = new SelectList(banklist, "Key", "Value", item.CardName);
                        return PartialView(item);
                    }
                    else
                        return PartialView("PartialError");
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
        public ActionResult EditSellerPartial(int id, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                var item = new Off_Seller();
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

        // Origin: Off_DeleteSeller
        [HttpPost]
        public ActionResult DeleteSellerAjax(int id)
        {
            var item = _offlineDB.Off_Seller.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_System_Id == user.DefaultSystemId)
                {
                    try
                    {
                        _offlineDB.Off_Seller.Remove(item);
                        _offlineDB.SaveChanges();
                        return Json(new { result = "SUCCESS" });
                    }
                    catch
                    {
                        return Json(new { result = "UNAUTHORIZED" });
                    }
                }
                else
                    return Json(new { result = "FAIL" });
            }
            return Json(new { result = "FAIL" });
        }

        [SettingFilter(SettingName = "GENERAL")]
        public ActionResult UploadSeller()
        {
            return PartialView();
        }
        [HttpPost]
        [SettingFilter(SettingName = "GENERAL")]
        public async Task<ActionResult> UploadSeller(FormCollection form)
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
                    List<Excel_DataMessage> result = await UploadSellerByExcelAsync(filename, messageList);
                }
            }
            else
            {
                messageList.Add(new Excel_DataMessage(0, "文件上传错误", true));
            }
            return View("UploadResult", messageList);
        }

        // Origin:Ajax_downloadSalary
        public FileResult DownloadSalaryFile(DateTime start, DateTime end)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var sql = "Select T4.Name, T5.StoreName, T4.Mobile, T4.IdNumber, T4.AccountName, T4.AccountSource, T4.CardName, T4.CardNo, T3.Standard_Salary," +
                "T3.Salary, T3.Bonus, T3.Debit, T3.Att_All, T3.Att_Delay, T3.Att_Leave, T3.Att_Absence from (select T1.SellerId, SUM(T1.Salary) as Salary, SUM(T1.Bonus) as Bonus, SUM(T1.Debit) as Debit," +
                "cast(AVG(T2.Standard_Salary) as decimal(10, 2)) as Standard_Salary, " +
                "SUM(case when T1.Attendance = 0 then 1 else 0 end) as Att_All," +
                "SUM(case when T1.Attendance = 1 then 1 else 0 end) as Att_Delay," +
                "SUM(case when T1.Attendance = 2 then 1 else 0 end) as Att_Leave," +
                "SUM(case when T1.Attendance = 3 then 1 else 0 end) as Att_Absence" +
                " from Off_SalesInfo_Daily as T1 left join Off_Checkin_Schedule as T2 on" +
                " T1.Date = T2.Subscribe and T1.StoreId = T2.Off_Store_Id" +
                " where T1.Date >= '" + start.ToString("yyyy-MM-dd") + "' and T1.Date <= '" + end.ToString("yyyy-MM-dd") + "'" +
                " group by SellerId) as T3 left join Off_Seller as T4 on T3.SellerId = T4.Id" +
                " left join Off_Store as T5 on T4.StoreId = T5.Id" +
                " where T4.Off_System_Id = " + user.DefaultSystemId;
            var list = _offlineDB.Database.SqlQuery<SellerSalaryExcel>(sql);
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            CsvWriter csv = new CsvWriter(writer);
            //string[] columname = new string[] {"店铺名称", "经销商", "姓名", "电话号码", "身份证号码", "开户行", "银行卡号", "工资", "奖金", "全勤天数", "迟到天数" };
            csv.WriteField("店铺名称");
            csv.WriteField("姓名");
            csv.WriteField("电话号码");
            csv.WriteField("身份证号码");
            csv.WriteField("银行名称");
            csv.WriteField("开户行");
            csv.WriteField("开户人姓名");
            csv.WriteField("银行卡号");
            csv.WriteField("工资标准");
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
                csv.WriteField(item.Name);
                csv.WriteField("'" + item.Mobile);
                csv.WriteField("'" + item.IdNumber);
                csv.WriteField(item.CardName);
                csv.WriteField(item.AccountSource);
                csv.WriteField(item.AccountName);
                csv.WriteField("'" + item.CardNo);
                csv.WriteField(item.Standard_Salary ?? 0);
                csv.WriteField(item.Salary ?? 0);
                csv.WriteField(item.Bonus ?? 0);
                csv.WriteField(item.Debit ?? 0);
                csv.WriteField(item.Att_All ?? 0);
                csv.WriteField(item.Att_Delay ?? 0);
                csv.WriteField(item.Att_Leave ?? 0);
                csv.WriteField(item.Att_Absence ?? 0);
                csv.NextRecord();
            }
            //csv.WriteRecords(list);
            writer.Flush();
            writer.Close();
            return File(convertCSV(stream.ToArray()), "@text/csv", "工资信息" + start.ToShortDateString() + "-" + end.ToShortDateString() + ".csv");
        }
        #region 绑定促销员

        // Origin: Off_BindSeller_List
        public ActionResult BindSellerIndex(string query, int? page, bool? bind)
        {
            return View();
        }
        // Origin: Off_BindSeller_List_Ajax
        public PartialViewResult BindSellerListPartial(string query, int? page, bool? bind)
        {
            int _page = page ?? 1;
            bool _bind = bind ?? false;
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (query == null)
            {
                var list = (from m in _offlineDB.Off_Membership_Bind
                            where m.Off_System_Id == user.DefaultSystemId
                            && m.Bind == _bind && m.Type==1
                            orderby m.ApplicationDate descending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
            else
            {
                var list = (from m in _offlineDB.Off_Membership_Bind
                            where (m.NickName.Contains(query) || m.Off_Seller.Off_Store.StoreName.Contains(query))
                            && m.Off_System_Id == user.DefaultSystemId
                            && m.Bind == _bind && m.Type==1
                            orderby m.ApplicationDate descending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
        }

        // Origin: Off_BindSeller
        public ActionResult BindSellerPartial(int id)
        {
            var item = _offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Id == id && m.Type==1);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_System_Id == user.DefaultSystemId)
                {
                    return PartialView(item);
                }
                else
                    return PartialView("AuthorizeErrorPartial");
            }
            else
                return PartialView("ErrorPartial");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult BindSellerPartial(int id, FormCollection form)
        {
            Off_Membership_Bind item = new Off_Membership_Bind();
            if (TryUpdateModel(item))
            {
                item.Bind = true;

                _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                _offlineDB.SaveChanges();
                return Content("SUCCESS");
            }
            else
            {
                return View("Error");
            }
        }
        #endregion

        public ActionResult BindTempSellerIndex()
        {
            return View();
        }
        

        public PartialViewResult BindTempSellerListPartial(string query, int? page, bool? bind)
        {
            int _page = page ?? 1;
            bool _bind = bind ?? false;
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (query == null)
            {
                var list = (from m in _offlineDB.Off_Membership_Bind
                            where m.Off_System_Id == user.DefaultSystemId
                            && m.Bind == _bind && m.Type == 2
                            orderby m.ApplicationDate descending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
            else
            {
                var list = (from m in _offlineDB.Off_Membership_Bind
                            where (m.NickName.Contains(query) || m.Off_Seller.Off_Store.StoreName.Contains(query))
                            && m.Off_System_Id == user.DefaultSystemId
                            && m.Bind == _bind && m.Type == 2
                            orderby m.ApplicationDate descending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
        }

        // Origin: Off_BindSeller
        public ActionResult BindTempSellerPartial(int id)
        {
            var item = _offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Id == id && m.Type==2);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_System_Id == user.DefaultSystemId)
                {
                    return PartialView(item);
                }
                else
                    return PartialView("AuthorizeErrorPartial");
            }
            else
                return PartialView("ErrorPartial");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult BindTempSellerPartial(int id, FormCollection form)
        {
            Off_Membership_Bind item = new Off_Membership_Bind();
            if (TryUpdateModel(item))
            {
                item.Bind = true;
                _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                _offlineDB.SaveChanges();
                return Content("SUCCESS");
            }
            else
            {
                return View("Error");
            }
        }


        // Origin: Off_UpdateManager
        [HttpPost]
        public JsonResult UpdateManagerAjax(int id)
        {

            var item = _offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var currentuser = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_System_Id == currentuser.DefaultSystemId)
                {
                    try
                    {
                        var user = UserManager.FindByName(item.UserName);
                        UserManager.RemoveFromRole(user.Id, "Seller");
                        UserManager.AddToRole(user.Id, "Manager");
                        var manager = _offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
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
                            _offlineDB.Off_StoreManager.Add(manager);
                        }
                        item.Bind = false;
                        item.Off_Seller_Id = null;
                        _offlineDB.Off_Membership_Bind.Remove(item);
                        _offlineDB.SaveChanges();
                        return Json(new { result = "SUCCESS" });
                    }
                    catch
                    {
                        return Json(new { result = "FAIL" });
                    }
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
        [HttpPost]
        public JsonResult DeleteRegisterSellerAjax(int id)
        {
            var item = _offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var currentuser = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_System_Id == currentuser.DefaultSystemId)
                {
                    var user = UserManager.FindByName(item.UserName);
                    UserManager.RemoveFromRole(user.Id, "Seller");
                    UserManager.Delete(user);
                    _offlineDB.Off_Membership_Bind.Remove(item);
                    _offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                else
                    return Json(new { result = "UNAUTHORIZED" });
            }
            else
                return Json(new { result = "FAIL" });
        }


        // Origin: analyseExcel_SellerTable
        public async Task<List<Excel_DataMessage>> UploadSellerByExcelAsync(string filename, List<Excel_DataMessage> messageList)
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
                        string info_name = dr["姓名"].ToString();
                        //var exist_dailyinfo = offlineDB.Off_SalesInfo_Daily.SingleOrDefault(m => m.Date == info_date && m.StoreId == exist_store.Id);
                        var exist_seller = _offlineDB.Off_Seller.SingleOrDefault(m => m.Name == info_name);
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
                            _offlineDB.Off_Seller.Add(seller);
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