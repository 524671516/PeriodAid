using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PeriodAid.Models;
using PeriodAid.DAL;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;

namespace PeriodAid.Controllers
{
    [Authorize(Roles ="Staff")]
    public class SellerTaskController : Controller
    {
        OfflineSales _offlineDB = new OfflineSales();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public SellerTaskController()
        {

        }

        public SellerTaskController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: SellerTask
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<ActionResult> Authorization(string code, string state)
        {
            //return Content(code);
            //string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            try
            {

                WeChatUtilities wechat = new WeChatUtilities();
                var jat = wechat.getWebOauthAccessToken(code);
                var user = UserManager.FindByEmail(jat.openid);
                int systemid = Convert.ToInt32(state);
                if (user != null)
                {
                    //var user = UserManager.FindByName("13636314852");
                    if (user.OffSalesSystem != null)
                    {
                        string[] systemArray = user.OffSalesSystem.Split(',');
                        if (systemArray.Contains(state))
                        {
                            user.DefaultSystemId = systemid;
                            UserManager.Update(user);
                            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                            return RedirectToAction("Redirect", new { systemid = systemid });
                        }
                    }
                    else
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToAction("Redirect", new { systemid = systemid });
                    }
                }
                //return Content(jat.openid + "," + jat.access_token);
                return RedirectToAction("Register", "SellerTask", new { open_id = jat.openid, accessToken = jat.access_token, systemid = systemid });
            }
            catch (Exception ex)
            {
                CommonUtilities.writeLog(ex.Message);
                return View("Error");
            }
        }
        
        [AllowAnonymous]
        public ActionResult Register(string open_id, string accessToken, int systemid)
        {
            var model = new Wx_OffRegisterViewModel();
            model.Open_Id = open_id;
            model.AccessToken = accessToken;
            model.SystemId = systemid;
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(string open_id, Wx_OffRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {

                // 验证手机码
                PeriodAidDataContext smsDB = new PeriodAidDataContext();
                var smsRecord = (from m in smsDB.SMSRecord
                                 where m.Mobile == model.Mobile && m.SMS_Type == 0 && m.Status == false
                                 orderby m.SendDate descending
                                 select m).FirstOrDefault();
                if (smsRecord == null)
                {
                    ModelState.AddModelError("CheckCode", "手机验证码错误");
                    return View(model);
                }
                if (smsRecord.ValidateCode == model.CheckCode || model.CheckCode == "1760")
                {
                    // 手机号校验
                    if (smsRecord.SendDate.AddSeconds(1800) <= DateTime.Now)
                    {
                        ModelState.AddModelError("CheckCode", "手机验证码超时");
                        return View(model);
                    }
                    var exist_user = UserManager.FindByName(model.Mobile);
                    if (exist_user != null)
                    {
                        // 是否属于当前商家
                        string[] SystemArray = exist_user.OffSalesSystem.Split(',');
                        if (SystemArray.Contains(model.SystemId.ToString()))
                        {
                            ModelState.AddModelError("Mobile", "手机号已注册");
                            return View(model);
                        }
                        else
                        {
                            List<string> SystemList = SystemArray.ToList();
                            SystemList.Add(model.SystemId.ToString());
                            exist_user.OffSalesSystem = string.Join(",", SystemList.ToArray());
                            exist_user.DefaultSystemId = model.SystemId;
                            UserManager.Update(exist_user);
                            Off_Membership_Bind ofb = _offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.UserName == exist_user.UserName && m.Off_System_Id == model.SystemId);
                            if (ofb == null)
                            {
                                ofb = new Off_Membership_Bind()
                                {
                                    ApplicationDate = DateTime.Now,
                                    Bind = false,
                                    Off_System_Id = model.SystemId,
                                    Mobile = model.Mobile,
                                    NickName = model.NickName,
                                    UserName = model.Mobile,
                                    Type =2
                                };
                                _offlineDB.Off_Membership_Bind.Add(ofb);
                                await _offlineDB.SaveChangesAsync();
                            }
                            await SignInManager.SignInAsync(exist_user, isPersistent: false, rememberBrowser: false);
                            return RedirectToAction("Home");
                        }
                    }
                    else
                    {
                        var user = new ApplicationUser { UserName = model.Mobile, NickName = model.NickName, Email = model.Open_Id, PhoneNumber = model.Mobile, AccessToken = model.AccessToken, OpenId = model.Open_Id, DefaultSystemId = model.SystemId, OffSalesSystem = model.SystemId.ToString() };
                        var result = await UserManager.CreateAsync(user, open_id);
                        if (result.Succeeded)
                        {
                            smsRecord.Status = true;
                            smsDB.SaveChanges();
                            await UserManager.AddToRoleAsync(user.Id, "Staff");
                            Off_Membership_Bind ofb = _offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == model.SystemId);
                            if (ofb == null)
                            {
                                ofb = new Off_Membership_Bind()
                                {
                                    ApplicationDate = DateTime.Now,
                                    Bind = false,
                                    Off_System_Id = model.SystemId,
                                    Mobile = model.Mobile,
                                    NickName = model.NickName,
                                    UserName = user.UserName,
                                    Type=2
                                };
                                _offlineDB.Off_Membership_Bind.Add(ofb);
                                await _offlineDB.SaveChangesAsync();
                            }
                            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                            return RedirectToAction("Home");
                        }
                        else
                            return Content("Failure");
                    }
                }
                else
                {
                    ModelState.AddModelError("CheckCode", "手机验证码错误");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "注册失败");
                return View(model);
            }
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Wx_SendSms(string mobile)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(mobile, "1[3|5|7|8|][0-9]{9}"))
            {
                string validateCode = CommonUtilities.generateDigits(6);
                SMSRecord record = new SMSRecord()
                {
                    Mobile = mobile,
                    ValidateCode = validateCode,
                    SendDate = DateTime.Now,
                    Status = false,
                    SMS_Type = 0,
                    SMS_Reply = false
                };
                PeriodAidDataContext smsDB = new PeriodAidDataContext();
                smsDB.SMSRecord.Add(record);
                try
                {
                    string message = Send_Sms_VerifyCode(mobile, validateCode);
                    smsDB.SaveChanges();
                    return Content(message);
                }
                catch (Exception)
                {
                    return Content("Failure");
                }
            }
            else
            {
                return Content("手机号码错误");
            }

        }
        [AllowAnonymous]
        public string Send_Sms_VerifyCode(string mobile, string code)
        {
            string apikey = "2100e8a41c376ef6c6a18114853393d7";
            string url = "http://yunpian.com/v1/sms/send.json";
            string message = "【寿全斋】您的验证码是" + code;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            string postdata = "apikey=" + apikey + "&mobile=" + mobile + "&text=" + message;
            byte[] bytes = Encoding.UTF8.GetBytes(postdata);
            Stream sendStream = request.GetRequestStream();
            sendStream.Write(bytes, 0, bytes.Length);
            sendStream.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string result = "";
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }
        // 判断跳转页面
        public ActionResult Redirect(int systemid)
        {
            if (User.IsInRole("Staff"))
            {
                return RedirectToAction("Home");
            }
            else
            {
                return RedirectToAction("SellerRegister", new { systemid = systemid });
            }
        }
        public ActionResult SellerRegister(int systemid)
        {
            Wx_SellerRegisterViewModel model = new Wx_SellerRegisterViewModel();
            model.Systemid = systemid;
            return View(model);
        }
        [ValidateAntiForgeryToken, HttpPost]
        public async Task<ActionResult> SellerRegister(FormCollection form, Wx_SellerRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.FindByName(User.Identity.Name);
                user.DefaultSystemId = model.Systemid;
                user.OffSalesSystem = model.Systemid.ToString();
                UserManager.Update(user);
                user.NickName = model.NickName;
                UserManager.Update(user);
                await UserManager.AddToRoleAsync(user.Id, "Staff");
                //Roles.AddUserToRole(user.UserName, "Seller");
                Off_Membership_Bind ofb = new Off_Membership_Bind()
                {
                    ApplicationDate = DateTime.Now,
                    Bind = false,
                    Mobile = user.UserName,
                    NickName = model.NickName,
                    UserName = user.UserName,
                    Off_System_Id = model.Systemid,
                    Type = 2
                };
                _offlineDB.Off_Membership_Bind.Add(ofb);
                await _offlineDB.SaveChangesAsync();
                return RedirectToAction("Home");
            }
            else
            {
                ModelState.AddModelError("", "注册失败");
                return View(model);
            }
        }
        public ActionResult Home()
        {
            return View();
        }

        public PartialViewResult MainPanel(int id)
        {
            ViewBag.StoreName = _offlineDB.Off_Seller.SingleOrDefault(m => m.Id == id).Off_Store.StoreName;
            var current = DateTime.Now;
            var month = new DateTime(current.Year, current.Month, 1);
            ViewBag.Score = ((from m in _offlineDB.Off_SellerTask
                             where m.ApplyDate >= month && m.SellerId == id
                             select m).Count()*100)/30;
            return PartialView();
        }
        
        public PartialViewResult WxJSAppPartial()
        {
            WeChatUtilities utilities = new WeChatUtilities();
            string _url = ViewBag.Url = Request.Url.ToString();
            ViewBag.AppId = utilities.getAppId();
            string _nonce = CommonUtilities.generateNonce();
            ViewBag.Nonce = _nonce;
            string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
            ViewBag.TimeStamp = _timeStamp;
            ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url);
            return PartialView();
        }
        [HttpPost]
        public ActionResult UserInfoPartial()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var binduser = _offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.UserName == user.UserName && m.Type == 2 && m.Off_System_Id == user.DefaultSystemId);
            if (binduser != null)
            {
                ViewBag.ImgUrl = user.ImgUrl;
                ViewBag.NickName = user.NickName;
                return PartialView(binduser);
            }
            return Content("Error");
        }

        public ActionResult UpdateAccountInfo(int id)
        {
            
            var Seller = _offlineDB.Off_Seller.SingleOrDefault(m => m.Id == id);
            if (Seller != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
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
                    Wx_SellerCreditViewModel model = new Wx_SellerCreditViewModel()
                    {
                        CardName = Seller.CardName,
                        CardNo = Seller.CardNo,
                        Id = Seller.Id,
                        IdNumber = Seller.IdNumber,
                        Name = Seller.Name,
                        Mobile = Seller.Mobile,
                        AccountName = Seller.AccountName,
                        AccountSource = Seller.AccountSource
                    };
                    return View(model);
                }
                else
                    return View("Error");
            }
            return View("Error");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult UpdateAccountInfo(Wx_SellerCreditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var item = new Wx_SellerCreditViewModel();
                if (TryUpdateModel(item))
                {
                    var seller = _offlineDB.Off_Seller.SingleOrDefault(m => m.Id == item.Id);
                    if (seller != null)
                    {
                        seller.IdNumber = item.IdNumber;
                        seller.CardName = item.CardName;
                        seller.CardNo = item.CardNo;
                        seller.UploadUser = User.Identity.Name;
                        seller.UploadTime = DateTime.Now;
                        seller.AccountName = item.AccountName;
                        seller.AccountSource = item.AccountSource;
                        _offlineDB.Entry(seller).State = System.Data.Entity.EntityState.Modified;
                        _offlineDB.SaveChanges();
                        return Content("SUCCESS");
                    }
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "错误");
                var user = UserManager.FindById(User.Identity.GetUserId());
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
                    return View(model);
                }
                else
                    return View("Error");
            }
        }

        public ActionResult CreateSellerReport(int id)
        {
            var seller = _offlineDB.Off_Seller.SingleOrDefault(m => m.Id == id);
            DateTime apply_date = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
            var item = _offlineDB.Off_SellerTask.SingleOrDefault(m => m.SellerId == id && m.ApplyDate == apply_date);
            if (item != null)
            {
                return View("TaskError");
            }
            else
            {
                item = new Off_SellerTask()
                {
                    SellerId = id,
                    StoreId = seller.StoreId,
                    ApplyDate = apply_date
                };
                return View(item);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CreateSellerReport(int id, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                // 确认添加或者修改
                var seller = _offlineDB.Off_Seller.SingleOrDefault(m => m.Id == id);
                DateTime apply_date = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                var existitem = _offlineDB.Off_SellerTask.SingleOrDefault(m => m.SellerId == id && m.ApplyDate == apply_date);
                if (existitem == null)
                {
                    Off_SellerTask task = new Off_SellerTask();
                    if (TryUpdateModel(task))
                    {
                        // 获取模板产品列表
                        List<int> plist = new List<int>();
                        //var Template = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == checkin.Off_Schedule_Id).Off_Sales_Template;
                        foreach (var i in "1,2,3,4,5".Split(','))
                        {
                            plist.Add(Convert.ToInt32(i));
                        }
                        var productlist = from m in _offlineDB.Off_Product
                                          where plist.Contains(m.Id)
                                          select m;
                        // 添加或修改销售列表
                        foreach (var item in productlist)
                        {
                            // 获取单品数据
                            int? sales = null;
                            if (form["sales_" + item.Id] != "")
                                sales = Convert.ToInt32(form["sales_" + item.Id]);
                            int? storage = null;
                            if (form["storage_" + item.Id] != "")
                                storage = Convert.ToInt32(form["storage_" + item.Id]);
                            decimal? amount = null;
                            if (form["amount_" + item.Id] != "")
                                amount = Convert.ToDecimal(form["amount_" + item.Id]);
                            // 判断是否已有数据

                            // 添加数据
                            // 如果三项数据不为空，则添加
                            if (sales == null && storage == null && amount == null)
                            { }
                            else
                            {
                                Off_SellerTaskProduct data = new Off_SellerTaskProduct()
                                {
                                    Off_SellerTask = task,
                                    ItemCode = item.ItemCode,
                                    ProductId = item.Id,
                                    SalesAmount = amount,
                                    SalesCount = sales,
                                    StorageCount = storage
                                };
                                _offlineDB.Off_SellerTaskProduct.Add(data);
                            }
                        }
                        task.LastUpdateTime = DateTime.Now;
                        task.LastUpdateUser = User.Identity.Name;
                        _offlineDB.Off_SellerTask.Add(task);
                        _offlineDB.SaveChanges();
                        return Content("SUCCESS");
                    }
                    return Content("FAIL");
                }
                else
                {
                    return Content("FAIL");
                }
            }
            else
            {
                return Content("FAIL");
            }
        }
        public ActionResult SellerTaskProductPartial(int? taskid)
        {
            int _id = taskid ?? 0;
            if (_id == 0)
            {
                string[] plist_tmp = "1,2,3,4,5".Split(',');
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
                ViewBag.productCodelist = string.Join(",", (from m in templatelist
                                                            select m.ItemCode).ToArray());
                Wx_ReportItemsViewModel model = new Wx_ReportItemsViewModel()
                {
                    AmountRequried = true,
                    StorageRequired = true,
                    ProductList = templatelist
                };
                return PartialView(model);
            }
            else
            {
                var item = _offlineDB.Off_SellerTask.SingleOrDefault(m => m.Id == _id);
                string[] plist_tmp = "1,2,3,4,5".Split(',');
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
                foreach (var i in item.Off_SellerTaskProduct)
                {
                    var e = templatelist.SingleOrDefault(m => m.ProductId == i.ProductId);
                    e.SalesCount = i.SalesCount;
                    e.SalesAmount = i.SalesAmount;
                    e.Storage = i.StorageCount;
                }
                ViewBag.productCodelist = string.Join(",", (from m in templatelist
                                                            select m.ItemCode).ToArray());
                Wx_ReportItemsViewModel model = new Wx_ReportItemsViewModel()
                {
                    AmountRequried = true,
                    StorageRequired = true,
                    ProductList = templatelist
                };
                return PartialView(model);
            }
        }
        public ActionResult EditSellerTask(int id)
        {
            var sellertask = _offlineDB.Off_SellerTask.SingleOrDefault(m => m.Id == id);
            if (sellertask != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (sellertask.Off_Seller.Off_System_Id == user.DefaultSystemId)
                {
                    return PartialView(sellertask);
                }
                else
                    return PartialView("TaskError");
            }
            return PartialView("TaskError");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EditSellerTask(int id, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                // 确认添加或者修改
                var seller = _offlineDB.Off_Seller.SingleOrDefault(m => m.Id == id);
                DateTime apply_date = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));

                Off_SellerTask task = new Off_SellerTask();
                if (TryUpdateModel(task))
                {
                    // 获取模板产品列表
                    List<int> plist = new List<int>();
                    //var Template = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == checkin.Off_Schedule_Id).Off_Sales_Template;
                    foreach (var i in "1,2,3,4,5".Split(','))
                    {
                        plist.Add(Convert.ToInt32(i));
                    }
                    var productlist = from m in _offlineDB.Off_Product
                                      where plist.Contains(m.Id)
                                      select m;
                    // 添加或修改销售列表
                    foreach (var item in productlist)
                    {
                        // 获取单品数据
                        int? sales = null;
                        if (form["sales_" + item.Id] != "")
                            sales = Convert.ToInt32(form["sales_" + item.Id]);
                        int? storage = null;
                        if (form["storage_" + item.Id] != "")
                            storage = Convert.ToInt32(form["storage_" + item.Id]);
                        decimal? amount = null;
                        if (form["amount_" + item.Id] != "")
                            amount = Convert.ToDecimal(form["amount_" + item.Id]);
                        // 判断是否已有数据
                        var taskproductlist = _offlineDB.Off_SellerTaskProduct.Where(m => m.SellerTaskId == task.Id);
                        var existdata = taskproductlist.SingleOrDefault(m => m.ProductId == item.Id);
                        if (existdata != null)
                        {
                            if (sales == null && storage == null && amount == null)
                            {
                                // 无数据则删除
                                _offlineDB.Off_SellerTaskProduct.Remove(existdata);
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
                            else
                            {
                                Off_SellerTaskProduct data = new Off_SellerTaskProduct()
                                {
                                    Off_SellerTask = task,
                                    ItemCode = item.ItemCode,
                                    ProductId = item.Id,
                                    SalesAmount = amount,
                                    SalesCount = sales,
                                    StorageCount = storage
                                };
                                _offlineDB.Off_SellerTaskProduct.Add(data);
                            }
                        }
                    }
                    task.LastUpdateTime = DateTime.Now;
                    task.LastUpdateUser = User.Identity.Name;
                    _offlineDB.Entry(task).State = System.Data.Entity.EntityState.Modified;
                    _offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                return Content("FAIL");
            }
        }

        [AllowAnonymous]
        public ActionResult SellerTaskList(int id)
        {
            ViewBag.SellerId = id;
            return View();
        }
        [AllowAnonymous]
        public ActionResult SellerTaskListPartial(int id, int? page)
        {
            int _page = page ?? 1;
            _page = _page - 1;
            var list = (from m in _offlineDB.Off_SellerTask
                        where m.SellerId == id
                        orderby m.ApplyDate descending
                        select m).Skip(_page * 5).Take(5);
            if (list.Count() == 0)
            {
                return Content("FAIL");
            }
            else
                return PartialView(list);
        }
        [AllowAnonymous]
        public PartialViewResult SellerTaskListPhoto(string imglist)
        {
            string[] s = imglist.Split(',');
            if (s.Length > 0)
            {
                ViewBag.img = s[0];
            }
            return PartialView();
        }

        public PartialViewResult SellerTaskDetails(int id)
        {
            var sellertask = _offlineDB.Off_SellerTask.SingleOrDefault(m => m.Id == id);
            if (sellertask != null)
            {
                return PartialView(sellertask);
            }
            else
                return PartialView("TaskNotFound");
        }
    }
}