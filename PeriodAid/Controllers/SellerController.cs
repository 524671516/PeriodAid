using PeriodAid.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PeriodAid.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace PeriodAid.Controllers
{
    [Authorize]
    public class SellerController : Controller
    {
        // GET: Seller
        OfflineSales offlineDB = new OfflineSales();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public SellerController()
        {

        }

        public SellerController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
            //return View();
            return Content(HttpContext.Request.Url.Host);
        }

        [AllowAnonymous]
        public ActionResult LoginManager()
        {

            string user_Agent = HttpContext.Request.UserAgent;
            if (user_Agent.Contains("MicroMessenger"))
            {
                //return Content("微信");
                string redirectUri = Url.Encode("http://webapp.shouquanzhai.cn/Seller/Authorization");
                string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
                string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_base&state=" + "0" + "#wechat_redirect";

                return Redirect(url);
            }
            else
            {
                return Content("其他");
            }
        }
        [AllowAnonymous]
        public async Task<ActionResult> Authorization(string code, string state)
        {
            //return Content(code);
            //string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            if (state == "0")
            {
                // 微信普通登陆
                WeChatUtilities wechat = new WeChatUtilities();
                var jat = wechat.getWebOauthAccessToken(code);
                var user = UserManager.FindByEmail(jat.openid);
                if (user != null)
                {
                    //var user = UserManager.FindByName("13636314852");
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    return RedirectToAction("Wx_Seller_Redirect");
                }
                //return Content(jat.openid + "," + jat.access_token);
                return RedirectToAction("Wx_Register", "Seller", new { open_id = jat.openid, accessToken = jat.access_token });
            }
            else if (state == "1")
            {
                // 微信信息更新
                WeChatUtilities wechat = new WeChatUtilities();
                var jat = wechat.getWebOauthAccessToken(code);
                var user = UserManager.FindById(User.Identity.GetUserId());
                user.AccessToken = jat.access_token;
                UserManager.Update(user);
                return RedirectToAction("Wx_UpdateUserInfo");
            }
            else
            {
                return View("Error");
            }
        }
        [AllowAnonymous]
        public ActionResult Wx_Register(string open_id, string accessToken)
        {
            var model = new Wx_RegisterViewModel();
            model.Open_Id = open_id;
            model.AccessToken = accessToken;
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Wx_Register(string open_id, Wx_OffRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 手机号校验
                var exist_user = UserManager.FindByName(model.Mobile);
                if (exist_user != null)
                {
                    ModelState.AddModelError("Mobile", "手机号已注册");
                    return View(model);
                }
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
                else if (smsRecord.ValidateCode != model.CheckCode)
                {
                    ModelState.AddModelError("CheckCode", "手机验证码错误");
                    return View(model);
                }
                else if (smsRecord.SendDate.AddSeconds(1800) <= DateTime.Now)
                {
                    ModelState.AddModelError("CheckCode", "手机验证码超时");
                    return View(model);
                }
                else
                {

                    var user = new ApplicationUser { UserName = model.Mobile, Email = model.Open_Id, PhoneNumber = model.Mobile, AccessToken = model.AccessToken, OpenId = model.Open_Id };
                    var result = await UserManager.CreateAsync(user, open_id);
                    //await UserManager.AddToRole(user, )
                    //user.Roles.Add
                    if (result.Succeeded)
                    {
                        smsRecord.Status = true;
                        smsDB.SaveChanges();
                        user.NickName = model.NickName;
                        UserManager.Update(user);
                        await UserManager.AddToRoleAsync(user.Id, "Seller");
                        //Roles.AddUserToRole(user.UserName, "Seller");
                        Off_Membership_Bind ofb = new Off_Membership_Bind()
                        {
                            ApplicationDate = DateTime.Now,
                            Bind = false,
                            Mobile = model.Mobile,
                            NickName = model.NickName,
                            UserName = user.UserName
                        };
                        offlineDB.Off_Membership_Bind.Add(ofb);
                        await offlineDB.SaveChangesAsync();
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToAction("Wx_Seller_Home");
                    }
                    else
                        return Content("Failure");
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
        public ActionResult Wx_Seller_Redirect()
        {
            if (User.IsInRole("Seller"))
            {
                return RedirectToAction("Wx_Seller_Home");
            }
            else if (User.IsInRole("Manager"))
            {
                return RedirectToAction("Wx_Manager_Home");
            }
            else
            {
                return RedirectToAction("Wx_Seller_Register");
            }
        }

        public ActionResult Wx_Seller_Register()
        {
            Wx_SellerRegisterViewModel model = new Wx_SellerRegisterViewModel();
            return View(model);
        }
        [ValidateAntiForgeryToken, HttpPost]
        public async Task<ActionResult> Wx_Seller_Register(FormCollection form, Wx_SellerRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.FindByName(User.Identity.Name);
                user.NickName = model.NickName;
                UserManager.Update(user);
                await UserManager.AddToRoleAsync(user.Id, "Seller");
                //Roles.AddUserToRole(user.UserName, "Seller");
                Off_Membership_Bind ofb = new Off_Membership_Bind()
                {
                    ApplicationDate = DateTime.Now,
                    Bind = false,
                    Mobile = user.UserName,
                    NickName = model.NickName,
                    UserName = user.UserName
                };
                offlineDB.Off_Membership_Bind.Add(ofb);
                await offlineDB.SaveChangesAsync();
                return RedirectToAction("Wx_Seller_Home");
            }
            else
            {
                ModelState.AddModelError("", "注册失败");
                return View(model);
            }
        }


        // 促销员首页
        public async Task<ActionResult> Wx_Seller_Home()
        {
            var userbind = from m in offlineDB.Off_Membership_Bind
                           where m.UserName == User.Identity.Name
                           select new { SellerId = m.Off_Seller_Id, StoreName = m.Off_Seller.Off_Store.StoreName };
            if (userbind != null)
            {
                var checked_item = userbind.FirstOrDefault(m => m.SellerId != null);
                if (checked_item != null)
                {
                    ViewBag.bindlist = new SelectList(userbind, "SellerId", "StoreName", checked_item.SellerId);
                }
                else
                {
                    ViewBag.bindlist = new SelectList(userbind, "SellerId", "StoreName");
                }
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                ViewBag.NickName = user.NickName;
                ViewBag.Mobile = user.PhoneNumber;
                ViewBag.SellerId = userbind.FirstOrDefault().SellerId;
                return View(user);
            }
            else
            {
                return View("Error");
            }
        }

        public PartialViewResult Wx_Seller_Panel(int? SellerId)
        {
            int storeId = offlineDB.Off_Seller.SingleOrDefault(m => m.Id == SellerId).StoreId;
            DateTime today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            var item = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Subscribe == today && m.Off_Store_Id == storeId);
            ViewBag.SellerId = SellerId;
            if (item != null)
            {
                ViewBag.CheckIn = true;
                ViewBag.ScheduleId = item.Id;
                var checkitem = offlineDB.Off_Checkin.SingleOrDefault(m => m.Off_Schedule_Id == item.Id && m.Off_Seller_Id == SellerId && m.Status != -1);
                if (checkitem != null)
                {
                    return PartialView(checkitem);
                }
                return PartialView(null);
            }
            else
            {
                ViewBag.CheckIn = false;
            }
            return PartialView(null);
        }
        public async Task<ActionResult> Wx_Seller_CheckIn(int ScheduleId, int SellerId)
        {
            if (!Wx_Seller_ConfirmSellerId(SellerId))
                return View("Error");
            WeChatUtilities utilities = new WeChatUtilities();
            string _url = ViewBag.Url = Request.Url.ToString();
            ViewBag.AppId = utilities.getAppId();
            string _nonce = CommonUtilities.generateNonce();
            ViewBag.Nonce = _nonce;
            string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
            ViewBag.TimeStamp = _timeStamp;
            ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            ViewBag.NickName = user.NickName;
            var Store = offlineDB.Off_Seller.SingleOrDefault(m => m.Id == SellerId);
            if(Store == null)
            {
                return View("Error");
            }
            var storeId = Store.StoreId;
            DateTime today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            var item = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == ScheduleId);
            if (item != null)
            {
                if (item.Subscribe == today && item.Off_Store_Id == storeId)
                {
                    var checkitem = offlineDB.Off_Checkin.SingleOrDefault(m => m.Off_Schedule_Id == item.Id && m.Off_Seller_Id == SellerId && m.Status != -1);
                    if (checkitem != null)
                    {
                        return View(checkitem);
                    }
                    else
                    {
                        checkitem = new Off_Checkin()
                        {
                            Off_Seller_Id = SellerId,
                            Off_Schedule_Id = ScheduleId,
                            Status = 0
                        };
                        offlineDB.Off_Checkin.Add(checkitem);
                        offlineDB.SaveChanges();
                        return View(checkitem);
                    }
                }
            }
            return View("Error");
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Wx_Seller_CheckIn(int ScheduleId, FormCollection form)
        {
            Off_Checkin checkin = new Off_Checkin();
            if (TryUpdateModel(checkin))
            {
                try
                {
                    checkin.CheckinTime = DateTime.Now;
                    checkin.Status = 1;
                    offlineDB.Entry(checkin).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return RedirectToAction("Wx_Seller_Home");
                }
                catch
                {
                    return View("Error");
                }
            }
            else
            {
                return View("Error");
            }
        }
        [HttpPost]
        public JsonResult SaveOrignalImage(string serverId)
        {
            try
            {
                WeChatUtilities utilities = new WeChatUtilities();
                string url = "http://file.api.weixin.qq.com/cgi-bin/media/get?access_token=" + utilities.getAccessToken() + "&media_id=" + serverId;
                System.Uri httpUrl = new System.Uri(url);
                HttpWebRequest req = (HttpWebRequest)(WebRequest.Create(httpUrl));
                req.Method = "GET";
                HttpWebResponse res = (HttpWebResponse)(req.GetResponse());
                Bitmap img = new Bitmap(res.GetResponseStream());//获取图片流
                string folder = HttpContext.Server.MapPath("~/Content/checkin-img/");
                string filename = DateTime.Now.ToFileTime().ToString() + ".jpg";
                img.Save(folder + filename);//随机名
                return Json(new { result = "SUCCESS", filename = filename });
            }
            catch (Exception ex)
            {
                string aa = ex.Message;
                CommonUtilities.writeLog(aa);
            }
            return Json(new { result = "FAIL" });
        }
        public FileResult ThumbnailImage(string filename)
        {
            string folder = HttpContext.Server.MapPath("~/Content/checkin-img/");
            Bitmap originalImage = new Bitmap(folder + filename);
            int towidth = 100;
            int toheight = 100;

            int x = 0;
            int y = 0;
            int ow = originalImage.Width;
            int oh = originalImage.Height;
            if (originalImage.Width >= originalImage.Height)
            {
                towidth = originalImage.Width * 100 / originalImage.Height;
            }
            else
            {
                toheight = originalImage.Height * 100 / originalImage.Width;
            }
            System.Drawing.Image bitmap = new System.Drawing.Bitmap(towidth, toheight);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);

            //设置高质量插值法
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

            //设置高质量,低速度呈现平滑程度
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充
            g.Clear(System.Drawing.Color.White);

            //在指定位置并且按指定大小绘制原图片的指定部分
            g.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, towidth, toheight),
                    new System.Drawing.Rectangle(x, y, ow, oh),
                    System.Drawing.GraphicsUnit.Pixel);
            try
            {
                //以jpg格式保存缩略图
                MemoryStream s = new MemoryStream();

                bitmap.Save(s, ImageFormat.Jpeg);
                byte[] imgdata = s.ToArray();
                //s.Read(imgdata, 0, imgdata.Length);
                //s.Seek(0, SeekOrigin.Begin);
                return File(imgdata, "image/jpeg");
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                g.Dispose();
            }
            //return File(null);
        }
        public FileResult ThumbnailImage_Box(string filename)
        {
            string folder = HttpContext.Server.MapPath("~/Content/checkin-img/");
            Bitmap originalImage = new Bitmap(folder + filename);
            int towidth = 100;
            int toheight = 100;

            int x = 0;
            int y = 0;
            int ow = originalImage.Width;
            int oh = originalImage.Height;
            if (originalImage.Width >= originalImage.Height)
            {
                x = (originalImage.Width - originalImage.Height) / 2;
                ow = oh;
            }
            else
            {
                y = (originalImage.Height - originalImage.Width) / 2;
                oh = ow;
            }
            System.Drawing.Image bitmap = new System.Drawing.Bitmap(towidth, toheight);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);

            //设置高质量插值法
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

            //设置高质量,低速度呈现平滑程度
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充
            g.Clear(System.Drawing.Color.White);

            //在指定位置并且按指定大小绘制原图片的指定部分
            g.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, towidth, toheight),
                    new System.Drawing.Rectangle(x, y, ow, oh),
                    System.Drawing.GraphicsUnit.Pixel);
            try
            {
                //以jpg格式保存缩略图
                MemoryStream s = new MemoryStream();

                bitmap.Save(s, ImageFormat.Jpeg);
                byte[] imgdata = s.ToArray();
                //s.Read(imgdata, 0, imgdata.Length);
                //s.Seek(0, SeekOrigin.Begin);
                return File(imgdata, "image/jpeg");
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                g.Dispose();
            }
            //return File(null);
        }
        public async Task<ActionResult> Wx_Seller_CheckOut(int CheckId)
        {
            var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == CheckId);
            if (item != null)
            {
                WeChatUtilities utilities = new WeChatUtilities();
                string _url = ViewBag.Url = Request.Url.ToString();
                ViewBag.AppId = utilities.getAppId();
                string _nonce = CommonUtilities.generateNonce();
                ViewBag.Nonce = _nonce;
                string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
                ViewBag.TimeStamp = _timeStamp;
                ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url);
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                ViewBag.NickName = user.NickName;
                return View(item);
            }
            return View("Error");
        }
        [ValidateAntiForgeryToken, HttpPost]
        public ActionResult Wx_Seller_CheckOut(int CheckId, FormCollection form)
        {
            Off_Checkin checkin = new Off_Checkin();
            if (TryUpdateModel(checkin))
            {
                try {
                    checkin.CheckoutTime = DateTime.Now;
                    checkin.Status = 2;
                    offlineDB.Entry(checkin).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return RedirectToAction("Wx_Seller_Home");
                }
                catch
                {
                    return View("Error");
                }
            }
            else
            {
                return View("Error");
            }
        }

        public ActionResult Wx_Seller_Report(int SellerId)
        {
            if (!Wx_Seller_ConfirmSellerId(SellerId))
                return View("Error");
            try {
                WeChatUtilities utilities = new WeChatUtilities();
                string _url = ViewBag.Url = Request.Url.ToString();
                ViewBag.AppId = utilities.getAppId();
                string _nonce = CommonUtilities.generateNonce();
                ViewBag.Nonce = _nonce;
                string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
                ViewBag.TimeStamp = _timeStamp;
                ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url);
                ViewBag.StoreName = offlineDB.Off_Seller.SingleOrDefault(m => m.Id == SellerId).Off_Store.StoreName;
                var reportlist = from m in offlineDB.Off_Checkin
                                 where m.Off_Seller_Id == SellerId
                                 && (m.Status == 2 || m.Status == 3)
                                 select new { Id = m.Id, ReportDate = m.Off_Checkin_Schedule.Subscribe };
                List<Object> attendance = new List<Object>();
                foreach (var i in reportlist)
                {
                    attendance.Add(new { Key = i.Id, Value = i.ReportDate.ToString("yyyy-MM-dd") });
                }
                if (attendance.Count > 0)
                    ViewBag.Report = new SelectList(attendance, "Key", "Value", reportlist.FirstOrDefault().Id);
                else
                {
                    ViewBag.Report = null;
                }
                return View();
            }
            catch
            {
                return View("Error");
            }
        }

        public ActionResult Wx_Seller_EditReport(int CheckId)
        {
            var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == CheckId);
            return PartialView(item);
        }
        [ValidateAntiForgeryToken, HttpPost]
        public ActionResult Wx_Seller_EditReport(Off_Checkin model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin checkin = new Off_Checkin();
                if (TryUpdateModel(checkin))
                {
                    checkin.Report_Time = DateTime.Now;
                    checkin.Status = 3;
                    offlineDB.Entry(checkin).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "错误");
                return PartialView(model);
            }
        }

        public ActionResult Wx_Seller_ScheduleList(int SellerId)
        {
            if (!Wx_Seller_ConfirmSellerId(SellerId))
                return View("Error");
            var Seller = offlineDB.Off_Seller.SingleOrDefault(m => m.Id == SellerId);
            if (Seller != null)
            {
                ViewBag.StoreName = Seller.Off_Store.StoreName;
                var currentTime = DateTime.Now;
                //今日以前4个
                var schedule_before = (from m in offlineDB.Off_Checkin_Schedule
                                       where m.Off_Store_Id == Seller.StoreId
                                       && m.Subscribe <= currentTime
                                       orderby m.Subscribe descending
                                       select m).Take(4);
                //今日以后6个
                var schedule_after = (from m in offlineDB.Off_Checkin_Schedule
                                      where m.Off_Store_Id == Seller.StoreId
                                      && m.Subscribe > currentTime
                                      select m).Take(10 - schedule_before.Count());
                var schedule = schedule_before.Concat(schedule_after);
                return View(schedule);
            }
            return View("Error");
        }

        public ActionResult Wx_Seller_ConfirmedData(int SellerId)
        {
            if (!Wx_Seller_ConfirmSellerId(SellerId))
                return View("Error");
            ViewBag.SellerId = SellerId;
            return View();
        }
        public ActionResult Wx_Seller_SalaryResult(int SellerId, bool current)
        {
            if (!Wx_Seller_ConfirmSellerId(SellerId))
                return View("Error");
            DateTime MonthCurrent = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); ;
            if (current)
            {
                var start = MonthCurrent;
                var end = MonthCurrent.AddMonths(1);
                ViewBag.Month = start.ToString("yyyy-MM");
                var SalaryList = from m in offlineDB.Off_SalesInfo_Daily
                                 where m.Date >= start && m.Date < end
                                 && m.SellerId == SellerId
                                 orderby m.Date
                                 select m;

                return PartialView(SalaryList);

            }
            else
            {
                var start = MonthCurrent.AddMonths(-1);
                var end = MonthCurrent;
                ViewBag.Month = start.ToString("yyyy-MM");
                var SalaryList = from m in offlineDB.Off_SalesInfo_Daily
                                 where m.Date >= start && m.Date < end
                                 && m.SellerId == SellerId
                                 orderby m.Date
                                 select m;
                return PartialView(SalaryList);
            }
        }
        public ActionResult Wx_Seller_CreditInfo(int SellerId)
        {
            if (!Wx_Seller_ConfirmSellerId(SellerId))
                return View("Error");
            var Seller = offlineDB.Off_Seller.SingleOrDefault(m => m.Id == SellerId);
            if (Seller != null)
            {
                List<Object> banklist = new List<object>();
                banklist.Add(new { Key = "中国银行", Value = "中国银行" });
                banklist.Add(new { Key = "中国工商银行", Value = "中国工商银行" });
                banklist.Add(new { Key = "中国农业银行", Value = "中国农业银行" });
                banklist.Add(new { Key = "中国建设银行", Value = "中国建设银行" });
                banklist.Add(new { Key = "交通银行", Value = "交通银行" });
                ViewBag.BankList = new SelectList(banklist, "Key", "Value");
                Wx_SellerCreditViewModel model = new Wx_SellerCreditViewModel()
                {
                    CardName = Seller.CardName,
                    CardNo = Seller.CardNo,
                    Id = Seller.Id,
                    IdNumber = Seller.IdNumber,
                    Name = Seller.Name,
                    Mobile = Seller.Mobile, 
                    AccountName = Seller.AccountName
                };
                return View(model);
            }
            return View("Error");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Wx_Seller_CreditInfo(Wx_SellerCreditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var item = new Wx_SellerCreditViewModel();
                if (TryUpdateModel(item))
                {
                    var seller = offlineDB.Off_Seller.SingleOrDefault(m => m.Id == item.Id);
                    if (seller != null)
                    {
                        seller.IdNumber = item.IdNumber;
                        seller.CardName = item.CardName;
                        seller.CardNo = item.CardNo;
                        seller.UploadUser = User.Identity.Name;
                        seller.UploadTime = DateTime.Now;
                        seller.AccountName = item.AccountName;
                        offlineDB.Entry(seller).State = System.Data.Entity.EntityState.Modified;
                        offlineDB.SaveChanges();
                        return RedirectToAction("Wx_Seller_Home");
                    }
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "错误");
                List<Object> banklist = new List<object>();
                banklist.Add(new { Key = "中国银行", Value = "中国银行" });
                banklist.Add(new { Key = "中国工商银行", Value = "中国工商银行" });
                banklist.Add(new { Key = "中国农业银行", Value = "中国农业银行" });
                banklist.Add(new { Key = "中国建设银行", Value = "中国建设银行" });
                banklist.Add(new { Key = "交通银行", Value = "交通银行" });
                ViewBag.BankList = new SelectList(banklist, "Key", "Value");
                return View(model);
            }
        }

        [AllowAnonymous]
        public ActionResult Wx_Seller_Guide()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult Wx_Seller_APITest()
        {
            WeChatUtilities utilities = new WeChatUtilities();
            string _url = ViewBag.Url = Request.Url.ToString();
            ViewBag.AppId = utilities.getAppId();
            string _nonce = CommonUtilities.generateNonce();
            ViewBag.Nonce = _nonce;
            string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
            ViewBag.TimeStamp = _timeStamp;
            ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url);
            return View();
        }
        public bool Wx_Seller_ConfirmSellerId(int SellerId)
        {
            var userbind = (from m in offlineDB.Off_Membership_Bind
                           where m.UserName == User.Identity.Name
                           select m.Off_Seller_Id);
            if (userbind.Contains(SellerId))
                return true;
            else
                return false;
        }


        //管理员页面
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult> Wx_Manager_Home()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            ViewBag.NickName = user.NickName;
            ViewBag.Mobile = user.PhoneNumber;
            DateTime today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName);
            var storelist = manager.Off_Store.Select(m => m.Id);
            var today_schedule = from m in offlineDB.Off_Checkin_Schedule
                                 where storelist.Contains(m.Off_Store_Id)
                                 && m.Subscribe == today
                                 select m;
            ViewBag.uncheckin_null = (from m in today_schedule
                                      where m.Off_Checkin.Count(p => p.Status >= 0) == 0
                                      select m).Count();
            ViewBag.uncheckin = (from m in today_schedule
                                 where m.Off_Checkin.Any(p => p.Status == 0)
                                 select m).Count();
            ViewBag.uncheckout = (from m in today_schedule
                                  where m.Off_Checkin.Any(p => p.Status == 1)
                                  select m).Count();
            ViewBag.unreport = (from m in offlineDB.Off_Checkin_Schedule
                                where m.Off_Checkin.Any(p => p.Status == 2) &&
                                storelist.Contains(m.Off_Store_Id)
                                select m).Count();
            ViewBag.uncheck = (from m in offlineDB.Off_Checkin_Schedule
                               where m.Off_Checkin.Any(p => p.Status == 3) &&
                               storelist.Contains(m.Off_Store_Id)
                               select m).Count();
            return View(manager);
        }

        [Authorize(Roles = "Manager")]
        public async Task<ActionResult> Wx_Manager_UnCheckInList(int status)
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            DateTime today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName);
            var storelist = manager.Off_Store.Select(m => m.Id);
            var today_schedule = from m in offlineDB.Off_Checkin_Schedule
                                 where storelist.Contains(m.Off_Store_Id)
                                 && m.Subscribe == today
                                 select m;
            /*ViewBag.uncheckin_null = from m in today_schedule
                                     where m.Off_Checkin.Count() == 0
                                     select m;*/
            ViewBag.Status = status;
            if (status == 0)
            {
                ViewBag.uncheckin_null = from m in today_schedule
                                         where m.Off_Checkin.Count(p => p.Status >= 0) == 0
                                         select m;
                var uncheckin = from m in today_schedule
                                where m.Off_Checkin.Any(p => p.Status == 0)
                                select m;
                ViewBag.Title = today.ToString("MM-dd") + ": 未签到门店";
                return View(uncheckin);
            }
            if(status == 1)
            {
                ViewBag.uncheckin_null = new List<Off_Checkin_Schedule>();
                var uncheckout = from m in today_schedule
                                     where m.Off_Checkin.Any(p => p.Status == 1)
                                     select m;
                ViewBag.Title = today.ToString("MM-dd") + ": 未签退门店";
                return View(uncheckout);
            }
            if (status == 2)
            {
                ViewBag.uncheckin_null = new List<Off_Checkin_Schedule>();
                var unreport = from m in offlineDB.Off_Checkin_Schedule
                               where m.Off_Checkin.Any(p => p.Status == 2) &&
                               storelist.Contains(m.Off_Store_Id)
                               select m;
                ViewBag.Title = today.ToString("MM-dd") + ": 未提报销量门店";
                return View(unreport);
            }
            else
                return View("Error");
            
        }
        [Authorize(Roles ="Manager")]
        [HttpPost]
        public JsonResult Wx_Manager_CheckInInfo(int checkid)
        {
            var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == checkid);
            if (item != null)
            {
                return Json(new { result = "SUCCESS", res = new { SellerName = item.Off_Seller.Name, StoreName = item.Off_Checkin_Schedule.Off_Store.StoreName, Subscribe = item.Off_Checkin_Schedule.Subscribe.ToString("MM-dd"), Status = item.Status } });
            }
            return Json(new { result = "FAIL" });
        }
        [Authorize(Roles ="Manager")]
        [HttpPost]
        public ActionResult Wx_Manager_DeleteCheckIn(int checkid)
        {
            var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == checkid);
            if (item != null)
            {
                item.Status = -1;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Content("SUCCESS");
            }
            return Content("FAIL");
        }
        [Authorize(Roles ="Manager")]
        public ActionResult Wx_Manager_StoreSellerList(int storeId)
        {
            ViewBag.StoreName = offlineDB.Off_Store.SingleOrDefault(m => m.Id == storeId).StoreName;
            var list = from m in offlineDB.Off_Seller
                       where m.StoreId == storeId
                       select m;
            return View(list);
        }

        [Authorize(Roles ="Manager")]
        public async Task<ActionResult> Wx_Manager_UnReportList()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName);
            var storelist = manager.Off_Store.Select(m => m.Id);
            var list = from m in offlineDB.Off_Checkin
                       where m.Status == 3 &&
                       storelist.Contains(m.Off_Checkin_Schedule.Off_Store_Id)
                       orderby m.Report_Time
                       select m;
            return View(list);
        }
        /* 代签到 */
        [Authorize(Roles ="Manager")]
        public ActionResult Wx_Manager_ProxyCheckIn(int checkid)
        {
            WeChatUtilities utilities = new WeChatUtilities();
            string _url = ViewBag.Url = Request.Url.ToString();
            ViewBag.AppId = utilities.getAppId();
            string _nonce = CommonUtilities.generateNonce();
            ViewBag.Nonce = _nonce;
            string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
            ViewBag.TimeStamp = _timeStamp;
            ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url);
            var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == checkid);
            ViewBag.StoreName = item.Off_Checkin_Schedule.Off_Store.StoreName;
            ViewBag.Subscribe = item.Off_Checkin_Schedule.Subscribe;
            ViewBag.SellerName = item.Off_Seller.Name;
            ViewBag.SellerMobile = item.Off_Seller.Mobile;
            return View(item);
        }
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken, HttpPost]
        public ActionResult Wx_Manager_ProxyCheckIn(Off_Checkin model)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin checkin = new Off_Checkin();
                if (TryUpdateModel(checkin))
                {
                    checkin.Report_Time = DateTime.Now;
                    checkin.CheckinLocation = checkin.CheckinLocation == null ? "N/A" : checkin.CheckinLocation;
                    checkin.CheckoutLocation = checkin.CheckoutLocation == null ? "N/A" : checkin.CheckoutLocation;
                    checkin.ConfirmTime = DateTime.Now;
                    checkin.ConfirmUser = User.Identity.Name;
                    checkin.Status = 3;
                    offlineDB.Entry(checkin).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return RedirectToAction("Wx_Manager_Home");
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "错误");
                var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == model.Id);
                ViewBag.StoreName = item.Off_Checkin_Schedule.Off_Store.StoreName;
                ViewBag.Subscribe = item.Off_Checkin_Schedule.Subscribe;
                ViewBag.SellerName = item.Off_Seller.Name;
                ViewBag.SellerMobile = item.Off_Seller.Mobile;
                return View(model);
            }
        }
        /* 新建签到 */
        [Authorize(Roles = "Manager")]
        public ActionResult Wx_Manager_CreateCheckIn(int sid)
        {
            WeChatUtilities utilities = new WeChatUtilities();
            string _url = ViewBag.Url = Request.Url.ToString();
            ViewBag.AppId = utilities.getAppId();
            string _nonce = CommonUtilities.generateNonce();
            ViewBag.Nonce = _nonce;
            string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
            ViewBag.TimeStamp = _timeStamp;
            ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url);
            var schedule = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == sid);
            ViewBag.StoreName = schedule.Off_Store.StoreName;
            ViewBag.Subscribe = schedule.Subscribe;
            Off_Checkin item = new Off_Checkin()
            {
                Off_Schedule_Id = sid,
                Status = 0
            };
            var sellerlist = from m in offlineDB.Off_Seller
                             where m.StoreId == schedule.Off_Store_Id
                             select m;
            ViewBag.SellerDropDown = new SelectList(sellerlist, "Id", "Name");
            return View(item);
        }
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken, HttpPost]
        public ActionResult Wx_Manager_CreateCheckIn(Off_Checkin model)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin checkin = new Off_Checkin();
                if (TryUpdateModel(checkin))
                {
                    checkin.Report_Time = DateTime.Now;
                    checkin.CheckinLocation = "N/A";
                    checkin.CheckoutLocation = "N/A";
                    checkin.ConfirmTime = DateTime.Now;
                    checkin.ConfirmUser = User.Identity.Name;
                    checkin.Status = 3;
                    //offlineDB.Entry(checkin).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.Off_Checkin.Add(checkin);
                    offlineDB.SaveChanges();
                    return RedirectToAction("Wx_Manager_Home");
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "错误");
                var schedule = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == model.Off_Schedule_Id);
                ViewBag.StoreName = schedule.Off_Store.StoreName;
                ViewBag.Subscribe = schedule.Subscribe;
                var sellerlist = from m in offlineDB.Off_Seller
                                 where m.StoreId == schedule.Off_Store_Id
                                 select m;
                ViewBag.SellerDropDown = new SelectList(sellerlist, "Id", "Name");
                return View(model);
            }
        }

        [Authorize(Roles = "Manager")]
        public async Task<ActionResult> Wx_Manager_StoreList()
        {
            WeChatUtilities utilities = new WeChatUtilities();
            string _url = ViewBag.Url = Request.Url.ToString();
            ViewBag.AppId = utilities.getAppId();
            string _nonce = CommonUtilities.generateNonce();
            ViewBag.Nonce = _nonce;
            string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
            ViewBag.TimeStamp = _timeStamp;
            ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            ViewBag.NickName = user.NickName;
            ViewBag.Mobile = user.PhoneNumber;
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName);
            return View(manager.Off_Store);
        }
        [Authorize(Roles ="Manager")]
        public ActionResult Wx_Manager_ViewCheckIn(int checkid)
        {
            WeChatUtilities utilities = new WeChatUtilities();
            string _url = ViewBag.Url = Request.Url.ToString();
            ViewBag.AppId = utilities.getAppId();
            string _nonce = CommonUtilities.generateNonce();
            ViewBag.Nonce = _nonce;
            string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
            ViewBag.TimeStamp = _timeStamp;
            ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url);
            var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == checkid);
            ViewBag.CheckIn = item;
            return View(item);
        }
        [Authorize(Roles ="Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Wx_Manager_ViewCheckIn(Off_Checkin model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin item = new Off_Checkin();
                if (TryUpdateModel(item))
                {
                    model.Confirm_Remark = item.Confirm_Remark;
                    offlineDB.Entry(model).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return RedirectToAction("Wx_Manager_Home");
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "请填写正确内容");
                WeChatUtilities utilities = new WeChatUtilities();
                string _url = ViewBag.Url = Request.Url.ToString();
                ViewBag.AppId = utilities.getAppId();
                string _nonce = CommonUtilities.generateNonce();
                ViewBag.Nonce = _nonce;
                string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
                ViewBag.TimeStamp = _timeStamp;
                ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url);
                var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == model.Id);
                ViewBag.CheckIn = item;
                return View(model);
            }
        }
        [Authorize(Roles = "Manager")]
        public ActionResult Wx_Manager_CheckReport(int CheckId)
        {
            WeChatUtilities utilities = new WeChatUtilities();
            string _url = ViewBag.Url = Request.Url.ToString();
            ViewBag.AppId = utilities.getAppId();
            string _nonce = CommonUtilities.generateNonce();
            ViewBag.Nonce = _nonce;
            string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
            ViewBag.TimeStamp = _timeStamp;
            ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url);
            var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == CheckId);
            ViewBag.CheckIn = item;
            return View(item);
        }
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken, HttpPost]
        public ActionResult Wx_Manager_CheckReport(Off_Checkin model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin item = new Off_Checkin();
                if (TryUpdateModel(item))
                {
                    item.ConfirmTime = DateTime.Now;
                    item.ConfirmUser = User.Identity.Name;
                    item.Status = 4;
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return RedirectToAction("Wx_Manager_Home");
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "请填写正确内容");
                WeChatUtilities utilities = new WeChatUtilities();
                string _url = ViewBag.Url = Request.Url.ToString();
                ViewBag.AppId = utilities.getAppId();
                string _nonce = CommonUtilities.generateNonce();
                ViewBag.Nonce = _nonce;
                string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
                ViewBag.TimeStamp = _timeStamp;
                ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url);
                var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == model.Id);
                ViewBag.CheckIn = item;
                return View(model);
            }
        }
        
        [Authorize(Roles = "Manager")]
        public ActionResult Wx_Manager_ReportList()
        {
            ViewBag.today = DateTime.Now;
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult> Wx_Manager_ReportList_Partial(string date)
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            DateTime today = Convert.ToDateTime(date);
            ViewBag.Today = today;
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName);
            var storelist = manager.Off_Store.Select(m => m.Id);
            var list = from m in offlineDB.Off_Checkin
                       where storelist.Contains(m.Off_Checkin_Schedule.Off_Store_Id)
                       && m.Off_Checkin_Schedule.Subscribe == today
                       && m.Status >= 4
                       select new Wx_ManagerReportListViewModel
                       {
                           Id = m.Id,
                           Status = m.Status,
                           Rep_Black = m.Rep_Black,
                           Rep_Brown = m.Rep_Brown,
                           Rep_Dates = m.Rep_Dates,
                           Rep_Honey = m.Rep_Honey,
                           Rep_Lemon = m.Rep_Lemon,
                           Rep_Other = m.Rep_Other,
                           SellerName = m.Off_Seller.Name,
                           StoreName = m.Off_Checkin_Schedule.Off_Store.StoreName,
                           Rep_Total = ((m.Rep_Brown ?? 0) + (m.Rep_Black ?? 0) + (m.Rep_Honey ?? 0) + (m.Rep_Lemon ?? 0) + (m.Rep_Dates ?? 0) + (m.Rep_Other ?? 0)),
                           Bonus = m.Bonus
                       };
            return PartialView(list);
        }

        [Authorize(Roles ="Manager")]
        public ActionResult Wx_Manager_ConfirmRedPack(int checkid)
        {
            var record = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == checkid);
            Wx_ManagerReportListViewModel model = new Wx_ManagerReportListViewModel()
            {
                Id = record.Id,
                Status = record.Status,
                Rep_Brown = record.Rep_Brown ?? 0,
                Rep_Black = record.Rep_Black ?? 0,
                Rep_Lemon = record.Rep_Lemon ?? 0,
                Rep_Honey = record.Rep_Honey ?? 0,
                Rep_Dates = record.Rep_Dates ?? 0,
                Rep_Other = record.Rep_Other ?? 0,
                Rep_Total = ((record.Rep_Brown ?? 0) + (record.Rep_Black ?? 0) + (record.Rep_Honey ?? 0) + (record.Rep_Lemon ?? 0) + (record.Rep_Dates ?? 0) + (record.Rep_Other ?? 0)),
                Bonus = record.Bonus,
                Bonus_Remark = record.Bonus_Remark,
                SellerName = record.Off_Seller.Name,
                StoreName = record.Off_Checkin_Schedule.Off_Store.StoreName
            };
            return PartialView(model);
        }

        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Wx_Manager_ConfirmRedPack(Wx_ManagerReportListViewModel model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                Wx_ManagerReportListViewModel item = new Wx_ManagerReportListViewModel();
                if (TryUpdateModel(item))
                {
                    var record = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == item.Id);
                    record.Bonus_Remark = item.Bonus_Remark;
                    record.Bonus_User = User.Identity.Name;
                    record.Bonus = item.Bonus;
                    offlineDB.Entry(record).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                else
                {
                    return View("Error");
                }
            }
            else
            {
                ModelState.AddModelError("", "请输入正确内容");
                return PartialView(model);
            }
            
        }

        [Authorize(Roles ="Manager")]
        public ActionResult Wx_Manager_EventList()
        {
            ViewBag.today = DateTime.Now;
            return View();
        }
        [Authorize(Roles ="Manager")]
        [HttpPost]
        public async Task<ActionResult> Wx_Manager_EventList_Partial(string date)
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            DateTime today = Convert.ToDateTime(date);
            ViewBag.Today = today;
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName);
            var storelist = manager.Off_Store.Select(m => m.Id);
            var schedulelist = from m in offlineDB.Off_Checkin_Schedule
                               where m.Subscribe == today
                               && storelist.Contains(m.Off_Store_Id)
                               select m;
            return PartialView(schedulelist);
        }
        [Authorize(Roles = "Manager")]
        [HttpPost]
        public ActionResult Wx_Manager_DeleteEvent(int scheduleid)
        {
            try
            {
                var item = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == scheduleid);
                if (item.Off_Checkin.Any(m => m.Status >= 5))
                {
                    return Content("FAIL");
                    
                }
                else
                {
                    offlineDB.Off_Checkin.RemoveRange(item.Off_Checkin);
                    offlineDB.Off_Checkin_Schedule.Remove(item);
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
            }
            catch
            {
                return Content("ERROR");
            }
        }
        [Authorize(Roles ="Manager")]
        public async Task<ActionResult> Wx_Manager_CreateEvent()
        {
            var schedule = new Wx_ManagerCreateScheduleViewModel();
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName);
            var storelist = manager.Off_Store.Select(m => new { Key = m.Id, Value = m.StoreName });
            ViewBag.StoreList = new SelectList(storelist, "Key", "Value");
            var today_org = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
            schedule.Subscribe = today_org;
            schedule.Standard_CheckIn = "10:00";
            schedule.Standard_CheckOut = "21:00";
            return View(schedule);
        }
        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Wx_Manager_CreateEvent(Wx_ManagerCreateScheduleViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (TryUpdateModel(model))
                {
                    var exist_list = from m in offlineDB.Off_Checkin_Schedule
                                     where m.Off_Store_Id == model.Off_Store_Id &&
                                     m.Subscribe == model.Subscribe
                                     select m;
                    if (exist_list.Count() == 0)
                    {
                        Off_Checkin_Schedule item = new Off_Checkin_Schedule()
                        {
                            Off_Store_Id = model.Off_Store_Id,
                            Subscribe = model.Subscribe,
                            Standard_Salary = model.Standard_Salary,
                            Standard_CheckIn = Convert.ToDateTime(model.Subscribe.ToString("yyyy-MM-dd") + " " + model.Standard_CheckIn),
                            Standard_CheckOut = Convert.ToDateTime(model.Subscribe.ToString("yyyy-MM-dd") + " " + model.Standard_CheckOut)
                        };
                        offlineDB.Off_Checkin_Schedule.Add(item);
                        offlineDB.SaveChanges();
                        return RedirectToAction("Wx_Manager_Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "店铺当天活动已存在，无法添加");
                        var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                        var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName);
                        var storelist = manager.Off_Store.Select(m => new { Key = m.Id, Value = m.StoreName });
                        ViewBag.StoreList = new SelectList(storelist, "Key", "Value");
                        return View(model);
                    }
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "提交错误，请根据提示修改");
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName);
                var storelist = manager.Off_Store.Select(m => new { Key = m.Id, Value = m.StoreName });
                ViewBag.StoreList = new SelectList(storelist, "Key", "Value");
                return View(model);
            }
            
        }

        [Authorize(Roles ="Manager")]
        public ActionResult Wx_Manager_Guide()
        {
            return View();
        }

        [Authorize(Roles ="Manager")]
        public ActionResult Wx_Manager_Task()
        {
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == User.Identity.Name);
            ViewBag.NickName = manager.NickName;
            ViewBag.Mobile = manager.Mobile;
            var today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            var task = offlineDB.Off_Manager_Task.SingleOrDefault(m => m.TaskDate == today && m.Status >= 0 && m.UserName == User.Identity.Name);
            if (task == null)
            {
                ViewBag.CheckInCount = 0;
            }
            else
                ViewBag.CheckInCount = task.Off_Manager_CheckIn.Count(m=>m.Canceled==false);
            return View();
        }

        [Authorize(Roles ="Manager")]
        public async Task<ActionResult> Wx_Manager_AddCheckIn()
        {
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == User.Identity.Name);
            ViewBag.NickName = manager.NickName;
            var today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            var task = offlineDB.Off_Manager_Task.SingleOrDefault(m => m.TaskDate == today && m.Status >= 0 && m.UserName==User.Identity.Name);
            WeChatUtilities utilities = new WeChatUtilities();
            string _url = ViewBag.Url = Request.Url.ToString();
            ViewBag.AppId = utilities.getAppId();
            string _nonce = CommonUtilities.generateNonce();
            ViewBag.Nonce = _nonce;
            string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
            ViewBag.TimeStamp = _timeStamp;
            ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url);
            if (task != null)
            {
                Off_Manager_CheckIn checkin = new Off_Manager_CheckIn();
                return View(checkin);
            }
            else
            {
                Off_Manager_Task item = new Off_Manager_Task()
                {
                    TaskDate = today,
                    Status = (int)ManagerTaskStatus.Reported,
                    UserName = User.Identity.Name,
                    NickName = manager.NickName
                };
                offlineDB.Off_Manager_Task.Add(item);
                await offlineDB.SaveChangesAsync();
                Off_Manager_CheckIn checkin = new Off_Manager_CheckIn();
                return View(checkin);
            }
        }

        [Authorize(Roles ="Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Wx_Manager_AddCheckIn(Off_Manager_CheckIn model)
        {
            if (ModelState.IsValid)
            {
                Off_Manager_CheckIn item = new Off_Manager_CheckIn();
                var today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                var task = offlineDB.Off_Manager_Task.SingleOrDefault(m => m.TaskDate == today && m.Status >= 0 && m.UserName == User.Identity.Name);
                if (TryUpdateModel(item))
                {
                    item.Off_Manager_Task = task;
                    item.CheckIn_Time = DateTime.Now;
                    offlineDB.Off_Manager_CheckIn.Add(item);
                    await offlineDB.SaveChangesAsync();
                    return RedirectToAction("Wx_Manager_Task");
                }
                return View("Manager_Error");
            }
            else
            {
                var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == User.Identity.Name);
                ViewBag.NickName = manager.NickName;
                var today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                var task = offlineDB.Off_Manager_Task.SingleOrDefault(m => m.TaskDate == today && m.UserName == User.Identity.Name && m.Status == 0);
                WeChatUtilities utilities = new WeChatUtilities();
                string _url = ViewBag.Url = Request.Url.ToString();
                ViewBag.AppId = utilities.getAppId();
                string _nonce = CommonUtilities.generateNonce();
                ViewBag.Nonce = _nonce;
                string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
                ViewBag.TimeStamp = _timeStamp;
                ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url);
                return View(model);
            }
        }

        [Authorize(Roles = "Manager")]
        public ActionResult Wx_Manager_CheckInView()
        {
            var list = from m in offlineDB.Off_Manager_Task
                       where m.UserName == User.Identity.Name
                       && m.Status == 0
                       orderby m.TaskDate descending
                       select m;
            List<Object> attendance = new List<Object>();
            foreach (var i in list)
            {
                attendance.Add(new { Key = i.Id, Value = i.TaskDate.ToString("yyyy-MM-dd") });
            }
            if (attendance.Count > 0)
                ViewBag.checkinlist = new SelectList(attendance, "Key", "Value", list.FirstOrDefault().Id);
            return View();
        }

        [Authorize(Roles = "Manager")]
        public ActionResult Wx_Manager_CheckInList_Ajax(int id)
        {
            var list = from m in offlineDB.Off_Manager_CheckIn
                       where m.Manager_EventId == id
                       && m.Canceled == false
                       select m;
            ViewBag.TaskId = id;
            return PartialView(list);
        }

        [Authorize(Roles ="Manager")]
        public ActionResult Wx_Manager_TaskReport(int? id)
        {

            var list = from m in offlineDB.Off_Manager_Task
                       where m.UserName == User.Identity.Name
                       && m.Status == 0
                       orderby m.TaskDate descending
                       select m;
            List<Object> attendance = new List<Object>();
            foreach (var i in list)
            {
                attendance.Add(new { Key = i.Id, Value = i.TaskDate.ToString("yyyy-MM-dd") });
            }
            int _id = id ?? list.FirstOrDefault().Id;
            if (attendance.Count > 0)
                ViewBag.checkinlist = new SelectList(attendance, "Key", "Value", _id);

            WeChatUtilities utilities = new WeChatUtilities();
            string _url = ViewBag.Url = Request.Url.ToString();
            ViewBag.AppId = utilities.getAppId();
            string _nonce = CommonUtilities.generateNonce();
            ViewBag.Nonce = _nonce;
            string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
            ViewBag.TimeStamp = _timeStamp;
            ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url);
            return View();
        }

        [Authorize(Roles ="Manager")]
        public ActionResult Wx_Manager_TaskReport_Ajax(int id)
        {
            var item = offlineDB.Off_Manager_Task.SingleOrDefault(m => m.Id == id);
            
            return PartialView(item);
        }

        [Authorize(Roles ="Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Wx_Manager_TaskReport_Ajax(Off_Manager_Task model)
        {
            if (ModelState.IsValid)
            {
                Off_Manager_Task item = new Off_Manager_Task();
                if (TryUpdateModel(item))
                {
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    await offlineDB.SaveChangesAsync();
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

        [Authorize(Roles = "Manager")]
        [HttpPost]
        public JsonResult Wx_Manager_CancelCheckIn_Ajax(int id)
        {
            var item = offlineDB.Off_Manager_CheckIn.SingleOrDefault(m => m.Id == id);
            if(item.Off_Manager_Task.UserName== User.Identity.Name)
            {
                item.Canceled = true;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            return Json(new { result = "FAIL" });
        }
        [Authorize(Roles ="Manager")]
        public ActionResult Wx_Manager_Tools()
        {
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == User.Identity.Name);
            ViewBag.NickName = manager.NickName;
            ViewBag.Mobile = manager.Mobile;
            return View();
        }

        // 0317 查看所有人签到列表
        [Authorize(Roles = "Senior")]
        public ActionResult Wx_Senior_AllCheckInList()
        {
            var list = from m in offlineDB.Off_Manager_Task
                       where m.Status == 0
                       group m by m.TaskDate into g
                       select new { g.Key };
            list = list.OrderByDescending(m => m.Key);
            List<Object> attendance = new List<Object>();
            foreach (var i in list)
            {
                attendance.Add(new { Key = i.Key, Value = i.Key.ToString("yyyy-MM-dd") });
            }
            if (attendance.Count > 0)
                ViewBag.checkinlist = new SelectList(attendance, "Key", "Value", list.FirstOrDefault().Key);
            return View();
        }

        // 0317 查看所有人签到列表-AJAX
        public ActionResult Wx_Senior_AllCheckInList_Ajax(string date)
        {
            var _date = Convert.ToDateTime(date);
            var list = from m in offlineDB.Off_Manager_Task
                       where m.TaskDate == _date && m.Status >= 0
                       select m;
            return PartialView(list);
        }


        // 0317 查看签到详情信息
        [Authorize(Roles ="Senior")]
        public ActionResult Wx_Senior_CheckInDetails(int id)
        {
            WeChatUtilities utilities = new WeChatUtilities();
            string _url = ViewBag.Url = Request.Url.ToString();
            ViewBag.AppId = utilities.getAppId();
            string _nonce = CommonUtilities.generateNonce();
            ViewBag.Nonce = _nonce;
            string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
            ViewBag.TimeStamp = _timeStamp;
            ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url);
            var item = offlineDB.Off_Manager_Task.SingleOrDefault(m => m.Id == id);
            return View(item);
        }

        [AllowAnonymous]
        public ActionResult Wx_Manager_QuerySeller()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public JsonResult Wx_Manager_AjaxSellerName(string query)
        {
            var list = from m in offlineDB.Off_Seller
                       where m.Name.Contains(query) || m.Off_Store.StoreName.Contains(query)
                       select new { Id = m.Id, Name = m.Name, Store = m.Off_Store.StoreName };
            return Json(new { result = "SUCCESS", data = list.Take(5) });
        }

        [AllowAnonymous]
        [HttpPost]
        public JsonResult Wx_Manager_AjaxSellerDetails(int id)
        {
            var item = offlineDB.Off_Seller.SingleOrDefault(m => m.Id == id);
            return Json(new { result = "SUCCESS", data = new { Name = item.Name, Mobile = item.Mobile, CardNo = item.CardNo, CardName = item.CardName, StoreName = item.Off_Store.StoreName, AccountName = item.AccountName, IDNumber = item.IdNumber } });
        }
    }
}