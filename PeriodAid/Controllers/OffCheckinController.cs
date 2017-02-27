using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

using PagedList;
using PeriodAid.Models;
using PeriodAid.Filters;
using System.Threading.Tasks;
using PeriodAid.DAL;

namespace PeriodAid.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OffCheckinController : Controller
    {
        OfflineSales _offlineDB = new OfflineSales();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public OffCheckinController()
        {

        }

        public OffCheckinController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        // GET: OffCheckin
        public ActionResult Index()
        {
            return View();
        }


        // Origin: Off_CheckIn_List
        public ActionResult CheckinList()
        {
            return View();
        }

        // Origin: Off_CheckIn_List_ajax
        public PartialViewResult CheckinListPartial(int? status, int? page, string query)
        {
            int _page = page ?? 1;
            int _status = status ?? 4;
            var user = UserManager.FindById(User.Identity.GetUserId());
            // 按照活动日期排序
            if (query == null)
            {
                var list = (from m in _offlineDB.Off_Checkin
                            where m.Status == _status && m.Off_Checkin_Schedule.Off_System_Id == user.DefaultSystemId
                            orderby m.Off_Checkin_Schedule.Subscribe descending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
            else
            {
                var list = (from m in _offlineDB.Off_Checkin
                            where m.Status == _status && m.Off_Checkin_Schedule.Off_System_Id == user.DefaultSystemId
                            && (m.Off_Checkin_Schedule.Off_Store.StoreName.Contains(query) || m.Off_Seller.Name.Contains(query) || m.Off_Checkin_Schedule.Off_Store.Off_StoreSystem.SystemName.Contains(query))
                            orderby m.Off_Checkin_Schedule.Subscribe descending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
        }

        // Origin: Off_ConfirmCheckIn
        public ActionResult ConfirmCheckin(int id)
        {
            var item = _offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_Checkin_Schedule.Off_System_Id == user.DefaultSystemId)
                {
                    bool attendance_late = false;
                    bool attendance_early = false;
                    int dt = 0;
                    var difftime = _offlineDB.Off_System_Setting.SingleOrDefault(m => m.Off_System_Id == user.DefaultSystemId && m.SettingName == "AttendanceAllow");
                    if (difftime != null)
                    {
                        try
                        {
                            dt = Convert.ToInt32(difftime.SettingValue);
                        }
                        catch
                        {
                            dt = 0;
                        }
                    }
                    else
                        dt = 0;
                    // 此处时间后期可以进行调整
                    if (item.CheckinTime >= item.Off_Checkin_Schedule.Standard_CheckIn.AddMinutes(dt))
                        attendance_late = true;
                    if (item.CheckoutTime <= item.Off_Checkin_Schedule.Standard_CheckOut.AddMinutes(0-dt))
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
                    //var user = UserManager.FindById(User.Identity.GetUserId());
                    ViewBag.SystemId = user.DefaultSystemId;
                    ConfirmCheckIn_ViewModel model = new ConfirmCheckIn_ViewModel()
                    {
                        CheckIn_Id = id,
                        AttendanceStatus = attendance,
                        Salary = item.Off_Checkin_Schedule.Standard_Salary,
                        Bonus = item.Bonus,
                        Debits = debits,
                        Remark = item.Remark,
                        Proxy = item.Proxy,
                        Bonus_Remark = item.Bonus_Remark,
                        Confirm_Remark = item.Confirm_Remark
                    };
                    ViewBag.CheckItem = item;
                    return View(model);
                }
                else
                    return View("AuthorizeError");
            }
            else
                return View("Error");
        }
        [ValidateAntiForgeryToken, HttpPost]
        public async Task<ActionResult> ConfirmCheckin(FormCollection form)
        {
            if (ModelState.IsValid)
            {
                ConfirmCheckIn_ViewModel model = new ConfirmCheckIn_ViewModel();
                if (TryUpdateModel(model))
                {
                    var item = _offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == model.CheckIn_Id);
                    if (item.Status == 4)
                    {
                        Off_SalesInfo_Daily info = new Off_SalesInfo_Daily()
                        {
                            Attendance = model.AttendanceStatus,
                            Date = item.Off_Checkin_Schedule.Subscribe,
                            Bonus = model.Bonus,
                            Debit = model.Debits,
                            isMultiple = false,
                            remarks = model.Remark,
                            SellerId = item.Off_Seller_Id,
                            Salary = model.Salary,
                            StoreId = item.Off_Checkin_Schedule.Off_Store_Id,
                            UploadTime = DateTime.Now,
                            UploadUser = User.Identity.Name
                        };
                        _offlineDB.Off_SalesInfo_Daily.Add(info);
                        // 获取模板产品列表
                        List<int> plist = new List<int>();
                        var Template = _offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == item.Off_Schedule_Id).Off_Store.Off_StoreSystem;
                        foreach (var i in Template.ProductList.Split(','))
                        {
                            plist.Add(Convert.ToInt32(i));
                        }
                        var productlist = from m in _offlineDB.Off_Product
                                          where plist.Contains(m.Id)
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
                            var checkinproductlist = _offlineDB.Off_Checkin_Product.Where(m => m.CheckinId == item.Id);
                            var existdata = checkinproductlist.SingleOrDefault(m => m.ProductId == product.Id);
                            if (existdata != null)
                            {
                                if ((sales == null||sales==0) && storage == null && amount == null)
                                {
                                    // 无数据则删除
                                    _offlineDB.Off_Checkin_Product.Remove(existdata);
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
                                if ((sales == null||sales==0) && storage == null && amount == null)
                                { }
                                else
                                {
                                    existdata = new Off_Checkin_Product()
                                    {
                                        CheckinId = item.Id,
                                        ItemCode = product.ItemCode,
                                        ProductId = product.Id,
                                        SalesAmount = amount,
                                        SalesCount = sales,
                                        StorageCount = storage
                                    };
                                    _offlineDB.Off_Checkin_Product.Add(existdata);
                                    //offlineDB.SaveChanges();
                                }
                            }
                        }
                        item.Status = 5;
                        item.Bonus = model.Bonus;
                        item.Bonus_Remark = model.Bonus_Remark;
                        item.SubmitTime = DateTime.Now;
                        item.SubmitUser = User.Identity.Name;
                        _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        foreach (var i in item.Off_Checkin_Product)
                        {
                            Off_Daily_Product product = new Off_Daily_Product()
                            {
                                Off_SalesInfo_Daily = info,
                                ItemCode = i.ItemCode,
                                ProductId = i.ProductId,
                                SalesAmount = i.SalesAmount,
                                SalesCount = i.SalesCount,
                                StorageCount = i.StorageCount
                            };
                            _offlineDB.Off_Daily_Product.Add(product);
                        }
                        var result = await _offlineDB.SaveChangesAsync();
                        // 计算平均值
                        OfflineSalesUtilities util = new OfflineSalesUtilities();
                        var result2= await util.UpdateDailySalesAvg(item.Off_Checkin_Schedule.Off_Store_Id, (int)item.Off_Checkin_Schedule.Subscribe.DayOfWeek + 1);
                        return RedirectToAction("CheckInList");
                    }
                    else if (item.Status >= 0 && item.Status <= 3)
                    {
                        List<int> plist = new List<int>();
                        var Template = _offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == item.Off_Schedule_Id).Off_Store.Off_StoreSystem;
                        foreach (var i in Template.ProductList.Split(','))
                        {
                            plist.Add(Convert.ToInt32(i));
                        }
                        var productlist = from m in _offlineDB.Off_Product
                                          where plist.Contains(m.Id)
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
                            var checkinproductlist = _offlineDB.Off_Checkin_Product.Where(m => m.CheckinId == item.Id);
                            var existdata = checkinproductlist.SingleOrDefault(m => m.ProductId == product.Id);
                            if (existdata != null)
                            {
                                if ((sales == null|| sales==0) && storage == null && amount == null)
                                {
                                    // 无数据则删除
                                    _offlineDB.Off_Checkin_Product.Remove(existdata);
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
                                if ((sales == null|| sales==0) && storage == null && amount == null)
                                { }
                                else
                                {
                                    existdata = new Off_Checkin_Product()
                                    {
                                        CheckinId = item.Id,
                                        ItemCode = product.ItemCode,
                                        ProductId = product.Id,
                                        SalesAmount = amount,
                                        SalesCount = sales,
                                        StorageCount = storage
                                    };
                                    _offlineDB.Off_Checkin_Product.Add(existdata);
                                    //offlineDB.SaveChanges();
                                }
                            }
                        }
                        item.Status = 4;
                        item.Proxy = model.Proxy;
                        item.CheckinLocation = item.CheckinLocation ?? "N/A";
                        item.CheckoutLocation = item.CheckoutLocation ?? "N/A";
                        item.Confirm_Remark = model.Confirm_Remark;
                        item.ConfirmUser = User.Identity.Name;
                        item.ConfirmTime = DateTime.Now;
                        item.Proxy = true;
                        _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        await _offlineDB.SaveChangesAsync();
                        return RedirectToAction("CheckInList");
                    }
                    return View("Error");
                }
                else
                {
                    return View("Error");
                }
            }
            else
                return View("Error");
        }

        // Origin: Off_ConfirmProductList(int CheckId)
        // 页面内产品数据列表
        public PartialViewResult CheckinProductListPartial(int id)
        {
            var item = _offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == id);

            string[] plist_tmp = item.Off_Checkin_Schedule.Off_Store.Off_StoreSystem.ProductList.Split(',');
            List<int> plist_int = new List<int>();
            foreach (var i in plist_tmp)
            {
                plist_int.Add(Convert.ToInt32(i));
            }
            var productlist = from m in _offlineDB.Off_Product
                              where plist_int.Contains(m.Id)
                              select m;
            List<Wx_TemplateProduct> templatelist = new List<Wx_TemplateProduct>();
            foreach (var i in productlist)
            {
                Wx_TemplateProduct p = new Wx_TemplateProduct()
                {
                    ProductId = i.Id,
                    ItemCode = i.ItemCode,
                    SimpleName = i.SimpleName
                };
                templatelist.Add(p);
            }
            foreach (var i in item.Off_Checkin_Product)
            {
                var e = templatelist.SingleOrDefault(m => m.ProductId == i.ProductId);
                if (e != null)
                {
                    e.SalesCount = i.SalesCount;
                    e.SalesAmount = i.SalesAmount;
                    e.Storage = i.StorageCount;
                }
            }
            Wx_ReportItemsViewModel model = new Wx_ReportItemsViewModel()
            {
                AmountRequried = item.Off_Checkin_Schedule.Off_Store.Off_StoreSystem.RequiredAmount,
                StorageRequired = item.Off_Checkin_Schedule.Off_Store.Off_StoreSystem.RequiredStorage,
                ProductList = templatelist
            };
            return PartialView(model);
        }

        // Origin: Off_ConfirmPrductListBySchedule
        public PartialViewResult AddCheckinProductListPartial(int id)
        {
            var item = _offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == id);
            string[] plist_tmp = item.Off_Store.Off_StoreSystem.ProductList.Split(',');
            List<int> plist = new List<int>();
            foreach (var i in plist_tmp)
            {
                plist.Add(Convert.ToInt32(i));
            }
            var productlist = from m in _offlineDB.Off_Product
                              where plist.Contains(m.Id)
                              select m;
            List<Wx_TemplateProduct> templatelist = new List<Wx_TemplateProduct>();
            foreach (var i in productlist)
            {
                Wx_TemplateProduct p = new Wx_TemplateProduct()
                {
                    ProductId = i.Id,
                    ItemCode = i.ItemCode,
                    SimpleName = i.SimpleName
                };
                templatelist.Add(p);
            }
            Wx_ReportItemsViewModel model = new Wx_ReportItemsViewModel()
            {
                AmountRequried = item.Off_Store.Off_StoreSystem.RequiredAmount,
                StorageRequired = item.Off_Store.Off_StoreSystem.RequiredStorage,
                ProductList = templatelist
            };
            return PartialView(model);
        }

        // Origin: Off_CreateCheckIn
        public ActionResult CreateCheckin(int id)
        {

            Off_Checkin item = new Off_Checkin();
            item.Status = 3;
            item.Proxy = true;
            item.Off_Schedule_Id = id;
            var schedule = _offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == id);
            ViewBag.StoreName = schedule.Off_Store.StoreName;
            ViewBag.Subscribe = schedule.Subscribe.ToString("yyyy-MM-dd");
            var sellerlist = from m in _offlineDB.Off_Seller
                             where m.StoreId == schedule.Off_Store_Id
                             select m;
            ViewBag.Sellerlist = new SelectList(sellerlist, "Id", "Name");
            return View(item);
        }
        [ValidateAntiForgeryToken, HttpPost]
        public ActionResult CreateCheckin(Off_Checkin model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin item = new Off_Checkin();
                if (TryUpdateModel(item))
                {
                    var schedule = _offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == item.Off_Schedule_Id);
                    item.CheckinLocation = "N/A";
                    item.CheckinTime = null;
                    item.CheckoutTime = null;
                    item.CheckoutLocation = "N/A";
                    item.ConfirmTime = DateTime.Now;
                    item.ConfirmUser = User.Identity.Name;
                    item.Proxy = true;
                    item.Status = 4;
                    _offlineDB.Off_Checkin.Add(item);
                    //offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    _offlineDB.SaveChanges();
                    List<int> plist = new List<int>();
                    var Template = _offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == item.Off_Schedule_Id).Off_Store.Off_StoreSystem;
                    foreach (var i in Template.ProductList.Split(','))
                    {
                        plist.Add(Convert.ToInt32(i));
                    }
                    var productlist = from m in _offlineDB.Off_Product
                                      where plist.Contains(m.Id)
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

                        if (sales == null && storage == null && amount == null)
                        { }
                        else
                        {
                            Off_Checkin_Product existdata = new Off_Checkin_Product()
                            {
                                Off_Checkin = item,
                                ItemCode = product.ItemCode,
                                ProductId = product.Id,
                                SalesAmount = amount,
                                SalesCount = sales,
                                StorageCount = storage
                            };
                            _offlineDB.Off_Checkin_Product.Add(existdata);
                        }
                    }
                    _offlineDB.SaveChanges();
                    return RedirectToAction("CheckInList");
                }
            }
            return View(model);
        }

        // Origin: Off_ProxyCheckIn
        public ActionResult EditCheckin(int id)
        {
            Off_Checkin item = _offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_Checkin_Schedule.Off_System_Id == user.DefaultSystemId)
                {
                    return View(item);
                }
                else
                    return View("AuthorizeError");
            }
            else
                return View("Error");
        }
        [ValidateAntiForgeryToken, HttpPost]
        public ActionResult EditCheckin(Off_Checkin model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin item = new Off_Checkin();
                if (TryUpdateModel(item))
                {
                    List<int> plist = new List<int>();
                    var Template = _offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == item.Off_Schedule_Id).Off_Store.Off_StoreSystem;
                    foreach (var i in Template.ProductList.Split(','))
                    {
                        plist.Add(Convert.ToInt32(i));
                    }
                    var productlist = from m in _offlineDB.Off_Product
                                      where plist.Contains(m.Id)
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
                        var checkinproductlist = _offlineDB.Off_Checkin_Product.Where(m => m.CheckinId == item.Id);
                        var existdata = checkinproductlist.SingleOrDefault(m => m.ProductId == product.Id);
                        if (existdata != null)
                        {
                            if ((sales == null || sales == 0) && storage == null && amount == null)
                            {
                                // 无数据则删除
                                _offlineDB.Off_Checkin_Product.Remove(existdata);
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
                            if ((sales == null || sales == 0) && storage == null && amount == null)
                            { }
                            else
                            {
                                existdata = new Off_Checkin_Product()
                                {
                                    CheckinId = item.Id,
                                    ItemCode = product.ItemCode,
                                    ProductId = product.Id,
                                    SalesAmount = amount,
                                    SalesCount = sales,
                                    StorageCount = storage
                                };
                                _offlineDB.Off_Checkin_Product.Add(existdata);
                            }
                        }
                    }
                    item.CheckinLocation = "N/A";
                    item.CheckoutLocation = "N/A";
                    item.CheckinTime = null;
                    item.CheckoutTime = null;
                    item.ConfirmTime = DateTime.Now;
                    item.ConfirmUser = User.Identity.Name;
                    item.Proxy = true;
                    item.Status = 4;
                    _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    _offlineDB.SaveChanges();
                    return RedirectToAction("CheckInList");
                }
                else
                {
                    ModelState.AddModelError("", "发生错误");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return View(model);
            }
        }

        // Origin: Off_CreateCheckIn_FileUpload
        [HttpPost]
        public ActionResult UploadCheckinFileAjax(FormCollection form)
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
                else
                {
                    error = "文件错误";
                }
            }
            string err_res = "{ error:'" + error + "', msg:'" + msg + "',imgurl:''}";
            return Content(err_res);
        }
        [HttpPost]
        public JsonResult UpdateCheckinFileAjax(int id, string filearray)
        {
            var checkin = _offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == id);
            if(checkin!=null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if(checkin.Off_Checkin_Schedule.Off_System_Id == user.DefaultSystemId)
                {
                    checkin.Rep_Image = filearray;
                    _offlineDB.Entry(checkin).State = System.Data.Entity.EntityState.Modified;
                    _offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                else
                    return Json(new { result = "UNAUTHORIZED" });
            }
            else
                return Json(new { result = "FAIL" });
        }

        // Origin: Off_ConfirmCheckIn_Map
        public ActionResult ViewCheckinLBS(bool trans, string lbs)
        {
            ViewBag.LBS = lbs;
            ViewBag.Trans = trans;
            return PartialView();
        }

        // Origin: Off_CancelCheckIn("Content")
        [HttpPost]
        public JsonResult CancelCheckinAjax(int id)
        {
            var item = _offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_Checkin_Schedule.Off_System_Id == user.DefaultSystemId)
                {
                    if (item.Status >= 0 && item.Status < 5)
                    {
                        item.Status = -1;
                        _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        _offlineDB.SaveChanges();
                        return Json(new { result = "SUCCESS" });
                    }
                    else
                    {
                        return Json(new { result = "FAIL" });
                    }
                }
                return Json(new { result = "UNAUTHORIZED" });
            }
            else
                return Json(new { result = "FAIL" });
        }

        // Origin: Off_QueryCheckIn
        public ActionResult QueryCheckin()
        {
            return View();
        }

        public ActionResult QueryCheckinPartial(DateTime? start, DateTime? end, string query, int? page, bool canceled)
        {
            DateTime _end = end ?? Convert.ToDateTime(DateTime.Now.ToShortDateString());
            DateTime _start = start ?? _end.AddDays(-7);
            var user = UserManager.FindById(User.Identity.GetUserId());
            int _page = page ?? 1;
            int status = canceled ? -1 : 0;
            if (query == null || query == "")
            {
                var list = (from m in _offlineDB.Off_Checkin
                            where m.Off_Checkin_Schedule.Off_System_Id == user.DefaultSystemId
                            && m.Off_Checkin_Schedule.Subscribe >= _start &&
                            m.Off_Checkin_Schedule.Subscribe <= _end &&
                            m.Status>=status
                            orderby m.Off_Checkin_Schedule.Subscribe descending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
            else
            {
                var list = (from m in _offlineDB.Off_Checkin
                            where m.Off_Checkin_Schedule.Off_System_Id == user.DefaultSystemId
                            && m.Off_Checkin_Schedule.Subscribe >= _start &&
                            m.Off_Checkin_Schedule.Subscribe <= _end &&
                            (m.Off_Checkin_Schedule.Off_Store.StoreName.Contains(query) || m.Off_Seller.Name.Contains(query)) &&
                            m.Status >= status
                            orderby m.Off_Checkin_Schedule.Subscribe descending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
        }

        // Origin: Off_ScheduleList(原本是单页面，现在做成ajax+partial的形式)
        public ActionResult ScheduleList()
        {
            return View();
        }
        public ActionResult ScheduleListPartial(bool? history, int? page)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            bool _history = history ?? true;
            int _page = page ?? 1;
            var currentTime = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            if (_history)
            {
                var list = (from m in _offlineDB.Off_Checkin_Schedule
                            where m.Subscribe <= currentTime && m.Off_System_Id == user.DefaultSystemId
                            group m by m.Subscribe into g
                            orderby g.Key descending
                            select new ScheduleList { Subscribe = g.Key, Count = g.Count(), Unfinished = g.Count(m => m.Off_Checkin.Any(p => p.Status >= 3)) }).ToPagedList(_page, 20);
                return PartialView(list);
            }
            else
            {
                var list = (from m in _offlineDB.Off_Checkin_Schedule
                            where m.Subscribe > currentTime && m.Off_System_Id == user.DefaultSystemId
                            group m by m.Subscribe into g
                            orderby g.Key
                            select new ScheduleList { Subscribe = g.Key, Count = g.Count(), Unfinished = g.Count(m => m.Off_Checkin.Any(p => p.Status >= 3)) }).ToPagedList(_page, 20);
                return PartialView(list);
            }
        }
        // Origin: Off_ScheduleDetails(原本是单页面，现在做成ajax+partial的形式)
        public ActionResult ViewScheduleDetails(string date)
        {
            ViewBag.Date = date;
            return View();
        }
        public PartialViewResult ViewScheduleDetailsPartial(string date, string query, int? page, bool? nonedata)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            DateTime day = DateTime.Parse(date);
            int _page = page ?? 1;
            bool _nonedata = nonedata ?? false;
            if (!_nonedata)
            {
                if (query != null)
                {
                    var list = (from m in _offlineDB.Off_Checkin_Schedule
                                where m.Subscribe == day && m.Off_Store.Off_StoreSystem.SystemName.Contains(query) && m.Off_System_Id == user.DefaultSystemId
                                orderby m.Off_Store.StoreName
                                select m).ToPagedList(_page, 20);
                    return PartialView(list);
                }
                else
                {
                    var list = (from m in _offlineDB.Off_Checkin_Schedule
                                where m.Subscribe == day && m.Off_System_Id == user.DefaultSystemId
                                orderby m.Off_Store.StoreName
                                select m).ToPagedList(_page, 20);
                    return PartialView(list);
                }
            }
            else
            {
                if (query != null)
                {
                    var list = (from m in _offlineDB.Off_Checkin_Schedule
                                where m.Subscribe == day && m.Off_Store.StoreName.Contains(query) && m.Off_System_Id == user.DefaultSystemId
                                && m.Off_Checkin.Count==0
                                orderby m.Off_Store.StoreName
                                select m).ToPagedList(_page, 20);
                    return PartialView(list);
                }
                else
                {
                    var list = (from m in _offlineDB.Off_Checkin_Schedule
                                where m.Subscribe == day && m.Off_System_Id == user.DefaultSystemId
                                && m.Off_Checkin.Count ==0
                                orderby m.Off_Store.StoreName
                                select m).ToPagedList(_page, 20);
                    return PartialView(list);
                }
            }
        }

        // Origin: Ajax_EditSchedule
        public ActionResult EditSchedulePartial(int id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var item = _offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                if (item.Off_System_Id == user.DefaultSystemId)
                {
                    ViewBag.StoreName = item.Off_Store.StoreName;
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
        [ValidateAntiForgeryToken, HttpPost]
        public ActionResult EditSchedulePartial(Off_Checkin_Schedule model)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin_Schedule item = new Off_Checkin_Schedule();
                if (TryUpdateModel(item))
                {
                    _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    _offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "请重试");
                var user = UserManager.FindById(User.Identity.GetUserId());
                ViewBag.StoreName = _offlineDB.Off_Store.SingleOrDefault(m => m.Id == model.Off_Store_Id).StoreName;
                return PartialView(model);
            }
        }

        // Origin: Ajax_DeleteSchedule(原来直接返回success,现在返回json)
        [HttpPost]
        public ActionResult Ajax_DeleteSchedule(int id)
        {
            var item = _offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_System_Id == user.DefaultSystemId)
                {
                    _offlineDB.Off_Checkin_Schedule.Remove(item);
                    _offlineDB.SaveChanges();
                    return Json(new { data = "SUCCESS" });
                }
                else
                    return Json(new { data = "UNAUTHORIZED" });
            }
            else
                return Json(new { data = "FAIL" });
        }

        // Origin: Off_Add_Schedule
        public ActionResult AddSchedule()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var storesystem = from m in _offlineDB.Off_StoreSystem
                              where m.Off_System_Id == user.DefaultSystemId
                              select m;
            ViewBag.SystemList = new SelectList(storesystem, "Id", "SystemName", storesystem.FirstOrDefault().Id);
            return View();
        }
        [HttpPost]
        public ActionResult AddSchedule(FormCollection form)
        {
            StoreSchedule_ViewModel model = new StoreSchedule_ViewModel();
            if (TryUpdateModel(model))
            {
                if (ModelState.IsValid)
                {
                    var user = UserManager.FindById(User.Identity.GetUserId());
                    if (model.StartDate > model.EndDate)
                    {
                        var storesystem = from m in _offlineDB.Off_StoreSystem
                                          where m.Off_System_Id == user.DefaultSystemId
                                          select m;
                        ViewBag.SystemList = new SelectList(storesystem, "Id", "SystemName", storesystem.FirstOrDefault().Id);
                        ModelState.AddModelError("", "开始日期不得大于结束日期");
                        return View(model);
                    }
                    if (form["StoreList"].ToString().Trim() == "")
                    {
                        var storesystem = from m in _offlineDB.Off_StoreSystem
                                          where m.Off_System_Id == user.DefaultSystemId
                                          select m;
                        ViewBag.SystemList = new SelectList(storesystem, "Id", "SystemName", storesystem.FirstOrDefault().Id);
                        ModelState.AddModelError("", "请至少选择一个店铺");
                        return View(model);
                    }
                    var datelength = Convert.ToInt32(model.EndDate.Subtract(model.StartDate).TotalDays);
                    // 每天循环
                    for (int i = 0; i <= datelength; i++)
                    {
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
                            var schedule = _offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Off_Store_Id == storeid && m.Subscribe == subscribe);
                            if (schedule == null)
                            {
                                schedule = new Off_Checkin_Schedule()
                                {
                                    Off_Store_Id = storeid,
                                    Subscribe = subscribe,
                                    Standard_CheckIn = new DateTime(year, month, day, Convert.ToInt32(begintime[0]), Convert.ToInt32(begintime[1]), 0),
                                    Standard_CheckOut = new DateTime(year, month, day, Convert.ToInt32(finishtime[0]), Convert.ToInt32(finishtime[1]), 0),
                                    Standard_Salary = model.Salary,
                                    Off_System_Id = user.DefaultSystemId
                                };
                                _offlineDB.Off_Checkin_Schedule.Add(schedule);
                            }
                            else
                            {
                                schedule.Standard_CheckIn = new DateTime(year, month, day, Convert.ToInt32(begintime[0]), Convert.ToInt32(begintime[1]), 0);
                                schedule.Standard_CheckOut = new DateTime(year, month, day, Convert.ToInt32(finishtime[0]), Convert.ToInt32(finishtime[1]), 0);
                                schedule.Standard_Salary = model.Salary;
                                _offlineDB.Entry(schedule).State = System.Data.Entity.EntityState.Modified;
                            }
                        }
                    }
                    _offlineDB.SaveChanges();
                    return RedirectToAction("ScheduleList");
                }
                else
                {
                    var user = UserManager.FindById(User.Identity.GetUserId());
                    var storesystem = from m in _offlineDB.Off_StoreSystem
                                      where m.Off_System_Id == user.DefaultSystemId
                                      select m;
                    ViewBag.SystemList = new SelectList(storesystem, "Id", "SystemName", storesystem.FirstOrDefault().Id);
                    return View(model);
                }
            }
            return RedirectToAction("ScheduleList");
        }

        // Origin: Off_Schedule_Statistic_Ajax
        // 0407 活动数据AJAX
        [HttpPost]
        public JsonResult ScheduleStatisticAjax(string datetime)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            DateTime targetDate = Convert.ToDateTime(datetime);
            var schedulelist = from m in _offlineDB.Off_Checkin_Schedule
                               where m.Subscribe == targetDate && m.Off_System_Id == user.DefaultSystemId
                               select m;
            int self = schedulelist.Count(g => g.Off_Checkin.Any(m => m.Status >= 3 && !m.Proxy));
            int proxy = schedulelist.Count(g => g.Off_Checkin.Any(m => m.Status >= 3 && m.Proxy));
            int rest = schedulelist.Count() - self - proxy;
            return Json(new { result = "SUCCESS", totalcount = schedulelist.Count(), selfcount = self, proxycount = proxy, restcount = rest });
        }

        // Origin: Off_EventDetails_Delete_batch
        [HttpPost]
        public JsonResult DeleteScheduleBatchAjax(string ids)
        {
            try
            {
                string sql = "DELETE FROM Off_Checkin_Schedule Where Id in (" + ids + ")";
                //string sql = "UPDATE Off_Store SET (Region = '" + modify_area + "') where Id in (" + ids + ")";
                _offlineDB.Database.ExecuteSqlCommand(sql);
                _offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
        }

        // Origin: Off_EventDetails_ModifyInfo_batch
        [HttpPost]
        public JsonResult EditScheduleBatchAjax(string ids, string starttime, string finishtime, decimal salary, string date)
        {
            try
            {
                string sql = "UPDATE Off_Checkin_Schedule SET Standard_CheckIn = '" + date + " " + starttime + "', Standard_CheckOut='" + date + " " + finishtime + "', Standard_Salary=" + salary + " where Id in (" + ids + ")";
                _offlineDB.Database.ExecuteSqlCommand(sql);
                _offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
        }

        // 0407 活动统计页面
        // Origin: Off_Schedule_Statisic
        public ActionResult ScheduleStatistic(string datetime)
        {
            ViewBag.CurrentDate = datetime;
            return View();
        }

        // 签呈列表
        public ActionResult SalesEventList()
        {
            return View();
        }
        public ActionResult SalesEventListPartial(int? page, string query)
        {
            
            int _page = page ?? 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (query == null)
            {
                var list = (from m in _offlineDB.Off_SalesEvent
                           where m.Off_StoreSystem.Off_System_Id== user.DefaultSystemId
                           && m.Status >= 0
                           orderby m.EndDate descending
                           select m).ToPagedList(_page,20);
                return PartialView(list);
            }
            else
            {
                var list = (from m in _offlineDB.Off_SalesEvent
                           where m.Off_StoreSystem.Off_System_Id == user.DefaultSystemId
                           && m.Status >= 0 && (m.EventTitle.Contains(query) || m.SerialNo.Contains(query))
                           orderby m.EndDate descending
                           select m).ToPagedList(_page,20);
                return PartialView(list);
            }
        }
        // 门店列表
        [HttpPost]
        public ActionResult GetStoreListByStoreSystem(int id)
        {
            var list = from m in _offlineDB.Off_Store
                       where m.Off_StoreSystemId == id
                       orderby m.StoreName
                       select new { Id = m.Id, StoreName = m.StoreName };
            return Json(new { result = "SUCCESS", data = list });
        }
        // 修改签呈
        public ActionResult EditSalesEvent(int id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var QD = from m in _offlineDB.Off_StoreSystem
                     where m.Off_System_Id == user.DefaultSystemId
                     select m;
            ViewBag.QD = new SelectList(QD, "Id", "SystemName");
            var model = _offlineDB.Off_SalesEvent.SingleOrDefault(m => m.Id == id);
            return PartialView(model);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EditSalesEvent(Off_SalesEvent model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                Off_SalesEvent item = _offlineDB.Off_SalesEvent.SingleOrDefault(m => m.Id == model.Id);
                if (TryUpdateModel(item))
                {
                    try
                    {
                        string[] storelist = form["StoreList"].Split(',');
                        //foreach(var existstore in item.st)
                        item.Off_Store.Clear();
                        List<int> storelistIds = new List<int>();
                        foreach (string v in storelist)
                        {
                            storelistIds.Add(Convert.ToInt32(v));
                        }
                        var stores = _offlineDB.Off_Store.Where(m => storelistIds.Contains(m.Id));
                        foreach (var store in stores)
                        {
                            item.Off_Store.Add(store);
                            //store.Off_SalesEvent.Add(item);
                        }
                        _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        _offlineDB.SaveChanges();
                        return Content("SUCCESS");
                    }
                    catch (Exception e)
                    {
                        return Content(e.Message);
                    }
                }
                return Content("FAIL");
            }
            else
            {
                ModelState.AddModelError("", "错误");
                var user = UserManager.FindById(User.Identity.GetUserId());
                var QD = from m in _offlineDB.Off_StoreSystem
                         where m.Off_System_Id == user.DefaultSystemId
                         select m;
                ViewBag.QD = new SelectList(QD, "Id", "SystemName");
                return PartialView(model);
            }
        }
        // 下架签呈
        [HttpPost]
        public async Task<ContentResult> DeleteSalesEvent(int id)
        {
            var item = _offlineDB.Off_SalesEvent.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                item.Status = -1;
                _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                await _offlineDB.SaveChangesAsync();
                return Content("SUCCESS");
            }
            else
            {
                return Content("FAIL");
            }
        }
        // 添加签呈
        public ActionResult CreateSalesEvent()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            Off_SalesEvent model = new Off_SalesEvent();
            var storesystem = from m in _offlineDB.Off_StoreSystem
                              where m.Off_System_Id == user.DefaultSystemId
                              select m;
            ViewBag.StoreSystemList = new SelectList(storesystem, "Id", "SystemName");
            return PartialView(model);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateSalesEvent(Off_SalesEvent model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                Off_SalesEvent item = new Off_SalesEvent();
                if (TryUpdateModel(item))
                {
                    try
                    {
                        string[] storelist = form["StoreList"].Split(',');
                        item.Status = 0;
                        item.CreateUserName = User.Identity.Name;
                        item.CreateDateTime = DateTime.Now;
                        List<int> storelistIds = new List<int>();
                        foreach (string v in storelist)
                        {
                            storelistIds.Add(Convert.ToInt32(v));
                        }
                        var stores = _offlineDB.Off_Store.Where(m => storelistIds.Contains(m.Id));
                        List<Off_Store> tempstorelist = new List<Off_Store>();
                        item.Off_Store = tempstorelist;
                        foreach (var store in stores)
                        {
                            item.Off_Store.Add(store);
                        }
                        _offlineDB.Off_SalesEvent.Add(item);
                        await _offlineDB.SaveChangesAsync();
                        return Content("SUCCESS");
                    }
                    catch (Exception e)
                    {
                        return Content(e.Message);
                    }
                }
                return Content("FAIL");
            }
            else
            {
                //return Content("FAIL");
                ModelState.AddModelError("", "错误");
                var user = UserManager.FindById(User.Identity.GetUserId());
                var storesystem = from m in _offlineDB.Off_StoreSystem
                                  where m.Off_System_Id == user.DefaultSystemId
                                  select m;
                ViewBag.StoreSystemList = new SelectList(storesystem, "Id", "SystemName");
                return PartialView(model);
            }
        }
        // 审核签呈
        public ActionResult CommitSalesEvent(int id)
        {
            var model = _offlineDB.Off_SalesEvent.SingleOrDefault(m => m.Id == id);
            return PartialView(model);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ConmmitSalesEvent(Off_SalesEvent model)
        {
            if (ModelState.IsValid)
            {
                Off_SalesEvent item = _offlineDB.Off_SalesEvent.SingleOrDefault(m => m.Id == model.Id);
                if (TryUpdateModel(item))
                {
                    item.CommitUserName = User.Identity.Name;
                    item.Status = 1;
                    item.CommitDateTime = DateTime.Now;
                    _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    _offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                ModelState.AddModelError("", "错误");
                return PartialView(model);
            }
        }
    }

}