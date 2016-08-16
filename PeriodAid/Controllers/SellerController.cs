using PeriodAid.DAL;
using PeriodAid.Filters;
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
        public ActionResult LoginManager(int? systemid)
        {
            int _systemid = systemid ?? 1;
            string user_Agent = HttpContext.Request.UserAgent;
            if (user_Agent.Contains("MicroMessenger"))
            {
                //return Content("微信");
                string redirectUri = Url.Encode("http://webapp.shouquanzhai.cn/Seller/Authorization");
                string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
                string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_base&state=" + _systemid + "#wechat_redirect";

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
                            return RedirectToAction("Wx_Seller_Redirect", new { systemid = systemid });
                        }
                    }
                    else
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToAction("Wx_Seller_Redirect", new { systemid = systemid });
                    }
                }
                //return Content(jat.openid + "," + jat.access_token);
                return RedirectToAction("Wx_Register", "Seller", new { open_id = jat.openid, accessToken = jat.access_token, systemid = systemid });
            }
            catch (Exception ex)
            {
                CommonUtilities.writeLog(ex.Message);
                return View("Error");
            }
        }
        [AllowAnonymous]
        public async Task<ActionResult> ForceAuthorization(string openid, string state)
        {
            try
            {
                string _state = "1";
                if (state == null)
                {
                    _state = state;
                }
                WeChatUtilities wechat = new WeChatUtilities();
                var user = UserManager.FindByEmail(openid);
                int systemid = Convert.ToInt32(_state);
                if (user != null)
                {
                    //var user = UserManager.FindByName("13636314852");
                    string[] systemArray = user.OffSalesSystem.Split(',');
                    if (systemArray.Contains(state))
                    {
                        user.DefaultSystemId = systemid;
                        UserManager.Update(user);
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToAction("Wx_Seller_Redirect");
                    }
                }
                //return Content(jat.openid + "," + jat.access_token);
                //return RedirectToAction("Wx_Register", "Seller", new { open_id = openid, accessToken = jat.access_token, systemid = systemid });
                return View("Error");
            }
            catch
            {
                return View("Error");
            }
        }
        [AllowAnonymous]
        public ActionResult Wx_Register(string open_id, string accessToken, int systemid)
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
        public async Task<ActionResult> Wx_Register(string open_id, Wx_OffRegisterViewModel model)
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
                            Off_Membership_Bind ofb = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.UserName == exist_user.UserName && m.Off_System_Id == model.SystemId);
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
                                    Type = 1
                                };
                                offlineDB.Off_Membership_Bind.Add(ofb);
                                await offlineDB.SaveChangesAsync();
                            }
                            await SignInManager.SignInAsync(exist_user, isPersistent: false, rememberBrowser: false);
                            return RedirectToAction("Wx_Seller_Home");
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
                            await UserManager.AddToRoleAsync(user.Id, "Seller");
                            Off_Membership_Bind ofb = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == model.SystemId);
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
                                    Type = 1
                                };
                                offlineDB.Off_Membership_Bind.Add(ofb);
                                await offlineDB.SaveChangesAsync();
                            }
                            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                            return RedirectToAction("Wx_Seller_Home");
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
        public ActionResult Wx_Seller_Redirect(int systemid, int? version)
        {
            int _version = version ?? 0;
            if (User.IsInRole("Manager"))
            {
                if (_version == 0)
                    return RedirectToAction("Wx_Manager_Home");
                else
                    return RedirectToAction("Manager_Home");
            }
            else if (User.IsInRole("Seller"))
            {
                return RedirectToAction("Wx_Seller_Home");
            }
            else
            {
                return RedirectToAction("Wx_Seller_Register", new { systemid = systemid });
            }
        }
        public ActionResult Wx_Seller_Register(int systemid)
        {
            Wx_SellerRegisterViewModel model = new Wx_SellerRegisterViewModel();
            model.Systemid = systemid;
            return View(model);
        }
        [ValidateAntiForgeryToken, HttpPost]
        public async Task<ActionResult> Wx_Seller_Register(FormCollection form, Wx_SellerRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.FindByName(User.Identity.Name);
                user.DefaultSystemId = model.Systemid;
                user.OffSalesSystem = model.Systemid.ToString();
                UserManager.Update(user);
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
                    UserName = user.UserName,
                    Off_System_Id = model.Systemid,
                    Type = 1
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
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var userbind = from m in offlineDB.Off_Membership_Bind
                           where m.UserName == User.Identity.Name
                           && m.Off_System_Id == user.DefaultSystemId && m.Type == 1
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
                //var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                ViewBag.NickName = user.NickName;
                ViewBag.Mobile = user.PhoneNumber;
                ViewBag.SellerId = userbind.FirstOrDefault().SellerId;
                WeChatUtilities utilities = new WeChatUtilities();
                string _url = ViewBag.Url = Request.Url.ToString();
                ViewBag.AppId = utilities.getAppId();
                string _nonce = CommonUtilities.generateNonce();
                ViewBag.Nonce = _nonce;
                string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
                ViewBag.TimeStamp = _timeStamp;
                ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url);
                return View(user);
            }
            else
            {
                return View("Error");
            }
        }

        public async Task<PartialViewResult> Wx_Seller_Panel(int? SellerId)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            int storeId = offlineDB.Off_Seller.SingleOrDefault(m => m.Id == SellerId && m.Off_System_Id == user.DefaultSystemId).StoreId;
            DateTime today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            int dow = (int)today.DayOfWeek;
            //int dow = (int)new DateTime(2016, 04, 03).DayOfWeek;
            var dowinfo = from m in offlineDB.Off_AVG_Info
                          where m.DayOfWeek == dow + 1 && m.StoreId == storeId
                          select new { AVG_Count = m.AVG_SalesData, AVG_Amount = m.AVG_AmountData };
            if (dowinfo.Count() == 0)
                ViewBag.AVG_Info = 0;
            else
            {
                var l = dowinfo.FirstOrDefault();
                ViewBag.AVG_Info = l.AVG_Count;
            }
            var item = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Subscribe == today && m.Off_Store_Id == storeId && m.Off_System_Id == user.DefaultSystemId);
            ViewBag.ReportCount = offlineDB.Off_Checkin.Where(m => m.Off_Seller_Id == SellerId && m.Status == 2).Count();
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
            if (Store == null)
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
                    ViewBag.StoreName = item.Off_Store.StoreName;
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
                            Status = 0,
                            Proxy = false
                        };
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
                    if (checkin.Status == 1)
                    {
                        checkin.CheckinTime = DateTime.Now;
                        checkin.Status = 1;
                        offlineDB.Entry(checkin).State = System.Data.Entity.EntityState.Modified;
                        //offlineDB.Off_Checkin.Add(checkin);
                        offlineDB.SaveChanges();
                        return RedirectToAction("Wx_Seller_Home");
                    }
                    else
                    {
                        checkin.CheckinTime = DateTime.Now;
                        checkin.Status = 1;
                        //offlineDB.Entry(checkin).State = System.Data.Entity.EntityState.Modified;
                        offlineDB.Off_Checkin.Add(checkin);
                        offlineDB.SaveChanges();
                        return RedirectToAction("Wx_Seller_Home");
                    }
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
                //Bitmap img = new Bitmap(res.GetResponseStream());//获取图片流
                //string folder = HttpContext.Server.MapPath("~/Content/checkin-img/");
                string filename = DateTime.Now.ToFileTime().ToString() + ".jpg";
                //img.Save(folder + filename);//随机名
                AliOSSUtilities util = new AliOSSUtilities();
                Stream inStream = res.GetResponseStream();
                MemoryStream ms = new MemoryStream();
                byte[] buffer = new byte[1024];
                while (true)
                {
                    int sz = inStream.Read(buffer, 0, 1024);
                    if (sz == 0)
                        break;
                    ms.Write(buffer, 0, sz);
                }
                ms.Position = 0;

                util.PutObject(ms.ToArray(), "checkin-img/" + filename);
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
            AliOSSUtilities util = new AliOSSUtilities();
            Bitmap originalImage = new Bitmap(util.GetObject("checkin-img/" + filename));
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
            AliOSSUtilities util = new AliOSSUtilities();
            Bitmap originalImage = new Bitmap(util.GetObject("checkin-img/" + filename));
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
                try
                {
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

        [HttpPost]
        public JsonResult Wx_Seller_CheckOut_Ajax(int CheckId, string lbs)
        {
            try
            {
                Off_Checkin checkin = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == CheckId);
                if (checkin != null)
                {
                    checkin.CheckoutLocation = lbs;
                    checkin.CheckoutTime = DateTime.Now;
                    checkin.Status = 2;
                    offlineDB.Entry(checkin).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                else
                {
                    return Json(new { result = "FAIL" });
                }
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
        }

        public ActionResult Wx_Seller_Report(int SellerId)
        {
            if (!Wx_Seller_ConfirmSellerId(SellerId))
                return View("Error");
            try
            {
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
                    // 获取模板产品列表
                    List<int> plist = new List<int>();
                    var Template = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == checkin.Off_Schedule_Id).Off_Sales_Template;
                    foreach (var i in Template.ProductList.Split(','))
                    {
                        plist.Add(Convert.ToInt32(i));
                    }
                    var productlist = from m in offlineDB.Off_Product
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
                        var checkinproductlist = offlineDB.Off_Checkin_Product.Where(m => m.CheckinId == checkin.Id);
                        var existdata = checkinproductlist.SingleOrDefault(m => m.ProductId == item.Id);
                        if (existdata != null)
                        {

                            if (sales == null && storage == null && amount == null)
                            {
                                // 无数据则删除
                                offlineDB.Off_Checkin_Product.Remove(existdata);
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
                                existdata = new Off_Checkin_Product()
                                {
                                    CheckinId = checkin.Id,
                                    ItemCode = item.ItemCode,
                                    ProductId = item.Id,
                                    SalesAmount = amount,
                                    SalesCount = sales,
                                    StorageCount = storage
                                };
                                offlineDB.Off_Checkin_Product.Add(existdata);
                                //offlineDB.SaveChanges();
                            }
                        }
                    }
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

        public PartialViewResult Wx_Seller_EditReport_Item(int CheckId)
        {
            var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == CheckId);
            string[] plist_tmp = item.Off_Checkin_Schedule.Off_Sales_Template.ProductList.Split(',');
            List<int> plist = new List<int>();
            foreach (var i in plist_tmp)
            {
                plist.Add(Convert.ToInt32(i));
            }
            var productlist = from m in offlineDB.Off_Product
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
            foreach (var i in item.Off_Checkin_Product)
            {
                var e = templatelist.SingleOrDefault(m => m.ProductId == i.ProductId);
                e.SalesCount = i.SalesCount;
                e.SalesAmount = i.SalesAmount;
                e.Storage = i.StorageCount;
            }
            Wx_ReportItemsViewModel model = new Wx_ReportItemsViewModel()
            {
                AmountRequried = item.Off_Checkin_Schedule.Off_Sales_Template.RequiredAmount,
                StorageRequired = item.Off_Checkin_Schedule.Off_Sales_Template.RequiredStorage,
                ProductList = templatelist
            };
            return PartialView(model);
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
                var user = UserManager.FindById(User.Identity.GetUserId());
                var banklistArray = offlineDB.Off_System_Setting.SingleOrDefault(m => m.Off_System_Id == user.DefaultSystemId && m.SettingName == "BankList");
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
                        seller.AccountSource = item.AccountSource;
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
                var user = UserManager.FindById(User.Identity.GetUserId());
                var banklistArray = offlineDB.Off_System_Setting.SingleOrDefault(m => m.Off_System_Id == user.DefaultSystemId && m.SettingName == "BankList");
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
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            var userbind = (from m in offlineDB.Off_Membership_Bind
                            where m.UserName == User.Identity.Name
                            && m.Off_System_Id == user.DefaultSystemId && m.Type == 1
                            select m.Off_Seller_Id);
            if (userbind.Contains(SellerId))
                return true;
            else
                return false;
        }

        [HttpPost]
        public JsonResult Wx_Seller_IsRecruit(int sellerid)
        {
            int confirmCount = offlineDB.Off_Checkin.Where(m => m.Off_Seller_Id == sellerid && m.Status > 3).Count();
            bool isRecruit = confirmCount > 4 ? false : true;
            return Json(new { result = "SUCCESS", recruit = isRecruit });
        }


        //管理员页面
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult> Wx_Manager_Home()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            ViewBag.NickName = user.NickName;
            ViewBag.Mobile = user.PhoneNumber;
            DateTime today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
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
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
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
            if (status == 1)
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
        [Authorize(Roles = "Manager")]
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
        [Authorize(Roles = "Manager")]
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
        [Authorize(Roles = "Manager")]
        public ActionResult Wx_Manager_StoreSellerList(int storeId)
        {
            ViewBag.StoreName = offlineDB.Off_Store.SingleOrDefault(m => m.Id == storeId).StoreName;
            var list = from m in offlineDB.Off_Seller
                       where m.StoreId == storeId
                       select m;
            return View(list);
        }

        [Authorize(Roles = "Manager")]
        public async Task<ActionResult> Wx_Manager_UnReportList()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Select(m => m.Id);
            var list = from m in offlineDB.Off_Checkin
                       where m.Status == 3 &&
                       storelist.Contains(m.Off_Checkin_Schedule.Off_Store_Id)
                       orderby m.Report_Time
                       select m;
            return View(list);
        }
        /* 代签到 */
        [Authorize(Roles = "Manager")]
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
        public ActionResult Wx_Manager_ProxyCheckIn(Off_Checkin model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin checkin = new Off_Checkin();
                if (TryUpdateModel(checkin))
                {
                    // 获取模板产品列表
                    List<int> plist = new List<int>();
                    var Template = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == checkin.Off_Schedule_Id).Off_Sales_Template;
                    foreach (var i in Template.ProductList.Split(','))
                    {
                        plist.Add(Convert.ToInt32(i));
                    }
                    var productlist = from m in offlineDB.Off_Product
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
                        var checkinproductlist = offlineDB.Off_Checkin_Product.Where(m => m.CheckinId == checkin.Id);
                        var existdata = checkinproductlist.SingleOrDefault(m => m.ProductId == item.Id);
                        if (existdata != null)
                        {

                            if (sales == null && storage == null && amount == null)
                            {
                                // 无数据则删除
                                offlineDB.Off_Checkin_Product.Remove(existdata);
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
                                existdata = new Off_Checkin_Product()
                                {
                                    CheckinId = checkin.Id,
                                    ItemCode = item.ItemCode,
                                    ProductId = item.Id,
                                    SalesAmount = amount,
                                    SalesCount = sales,
                                    StorageCount = storage
                                };
                                offlineDB.Off_Checkin_Product.Add(existdata);
                                //offlineDB.SaveChanges();
                            }
                        }
                    }
                    checkin.Report_Time = DateTime.Now;
                    checkin.CheckinLocation = checkin.CheckinLocation == null ? "N/A" : checkin.CheckinLocation;
                    checkin.CheckoutLocation = checkin.CheckoutLocation == null ? "N/A" : checkin.CheckoutLocation;
                    checkin.ConfirmTime = DateTime.Now;
                    checkin.ConfirmUser = User.Identity.Name;
                    checkin.Proxy = true;
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
        public ActionResult Wx_Manager_CreateCheckIn(Off_Checkin model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin checkin = new Off_Checkin();
                if (TryUpdateModel(checkin))
                {
                    // 获取模板产品列表

                    checkin.Report_Time = DateTime.Now;
                    checkin.CheckinLocation = "N/A";
                    checkin.CheckoutLocation = "N/A";
                    checkin.ConfirmTime = DateTime.Now;
                    checkin.ConfirmUser = User.Identity.Name;
                    checkin.Proxy = true;
                    checkin.Status = 3;
                    //offlineDB.Entry(checkin).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.Off_Checkin.Add(checkin);
                    offlineDB.SaveChanges();
                    List<int> plist = new List<int>();
                    var Template = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == checkin.Off_Schedule_Id).Off_Sales_Template;
                    foreach (var i in Template.ProductList.Split(','))
                    {
                        plist.Add(Convert.ToInt32(i));
                    }
                    var productlist = from m in offlineDB.Off_Product
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

                        if (sales == null && storage == null && amount == null)
                        { }
                        else
                        {
                            Off_Checkin_Product existdata = new Off_Checkin_Product()
                            {
                                Off_Checkin = checkin,
                                ItemCode = item.ItemCode,
                                ProductId = item.Id,
                                SalesAmount = amount,
                                SalesCount = sales,
                                StorageCount = storage
                            };
                            offlineDB.Off_Checkin_Product.Add(existdata);
                        }
                    }
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
        public PartialViewResult Wx_Manager_EditReport_Item(int ScheduleId)
        {
            var item = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == ScheduleId);
            string[] plist_tmp = item.Off_Sales_Template.ProductList.Split(',');
            List<int> plist = new List<int>();
            foreach (var i in plist_tmp)
            {
                plist.Add(Convert.ToInt32(i));
            }
            var productlist = from m in offlineDB.Off_Product
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
                AmountRequried = item.Off_Sales_Template.RequiredAmount,
                StorageRequired = item.Off_Sales_Template.RequiredStorage,
                ProductList = templatelist
            };
            return PartialView(model);
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
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            return View(manager.Off_Store);
        }
        [Authorize(Roles = "Manager")]
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
        [Authorize(Roles = "Manager")]
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
                Off_Checkin checkin = new Off_Checkin();
                if (TryUpdateModel(checkin))
                {
                    // 获取模板产品列表
                    List<int> plist = new List<int>();
                    var Template = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == checkin.Off_Schedule_Id).Off_Sales_Template;
                    foreach (var i in Template.ProductList.Split(','))
                    {
                        plist.Add(Convert.ToInt32(i));
                    }
                    var productlist = from m in offlineDB.Off_Product
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
                        var checkinproductlist = offlineDB.Off_Checkin_Product.Where(m => m.CheckinId == checkin.Id);
                        var existdata = checkinproductlist.SingleOrDefault(m => m.ProductId == item.Id);
                        if (existdata != null)
                        {

                            if (sales == null && storage == null && amount == null)
                            {
                                // 无数据则删除
                                offlineDB.Off_Checkin_Product.Remove(existdata);
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
                                existdata = new Off_Checkin_Product()
                                {
                                    CheckinId = checkin.Id,
                                    ItemCode = item.ItemCode,
                                    ProductId = item.Id,
                                    SalesAmount = amount,
                                    SalesCount = sales,
                                    StorageCount = storage
                                };
                                offlineDB.Off_Checkin_Product.Add(existdata);
                                //offlineDB.SaveChanges();
                            }
                        }
                    }
                    checkin.ConfirmTime = DateTime.Now;
                    checkin.ConfirmUser = User.Identity.Name;
                    checkin.Status = 4;
                    offlineDB.Entry(checkin).State = System.Data.Entity.EntityState.Modified;
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
        public async Task<ActionResult> Wx_Manager_ReportList()
        {
            ViewBag.today = DateTime.Now;
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = from m in manager.Off_Store
                            group m by m.StoreSystem into g
                            select new { Key = g.Key };
            ViewBag.StoreSystem = new SelectList(storelist, "Key", "Key", storelist.FirstOrDefault().Key);
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult> Wx_Manager_ReportList_Partial(string date, string storesystem)
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            DateTime today = Convert.ToDateTime(date);
            ViewBag.Today = today;
            int dow = (int)today.DayOfWeek;
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Where(m => m.StoreSystem == storesystem).Select(m => m.Id);
            var listview = from m in offlineDB.Off_Checkin
                           where storelist.Contains(m.Off_Checkin_Schedule.Off_Store_Id)
                           && m.Off_Checkin_Schedule.Subscribe == today
                           && m.Status >= 4
                           select new
                           {
                               Id = m.Id,
                               Status = m.Status,
                               StoreId = m.Off_Checkin_Schedule.Off_Store_Id,
                               SellerName = m.Off_Seller.Name,
                               StoreName = m.Off_Checkin_Schedule.Off_Store.StoreName,
                               Rep_Total = m.Off_Checkin_Product.Sum(g => g.SalesCount),
                               Bonus = m.Bonus
                           };
            //var storelist = list.Select(m => m.StoreId);
            var avglist = from m in offlineDB.Off_AVG_Info
                          where m.DayOfWeek == dow + 1 && m.Off_Store.Off_System_Id == user.DefaultSystemId &&
                          storelist.Contains(m.StoreId)
                          select new { StoreId = m.StoreId, AVG_Total = m.AVG_SalesData, AVG_Amount = m.AVG_AmountData };

            var finallist = from m1 in listview
                            join m2 in avglist on m1.StoreId equals m2.StoreId into lists
                            from m in lists.DefaultIfEmpty()
                            select new Wx_ManagerReportListViewModel
                            {
                                Id = m1.Id,
                                Status = m1.Status,
                                StoreId = m1.StoreId,
                                SellerName = m1.SellerName,
                                StoreName = m1.StoreName,
                                Rep_Total = m1.Rep_Total,
                                Bonus = m1.Bonus,
                                AVG_Total = m.AVG_Total
                            };
            return PartialView(finallist);
        }

        [Authorize(Roles = "Manager")]
        public ActionResult Wx_Manager_ConfirmRedPack(int checkid)
        {
            var record = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == checkid);
            Wx_ManagerReportListViewModel model = new Wx_ManagerReportListViewModel()
            {
                Id = record.Id,
                Status = record.Status,
                Rep_Total = record.Off_Checkin_Product.Sum(m => m.SalesCount),
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
                ApplicationUser manager = UserManager.FindById(User.Identity.GetUserId());
                if (TryUpdateModel(item))
                {
                    var record = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == item.Id);
                    record.Bonus_Remark = item.Bonus_Remark;
                    record.Bonus_User = User.Identity.Name;
                    record.Bonus = item.Bonus;
                    offlineDB.Entry(record).State = System.Data.Entity.EntityState.Modified;
                    var binduser = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Off_Seller_Id == record.Off_Seller_Id && m.Off_System_Id == manager.DefaultSystemId);

                    if (binduser == null)
                    {
                        offlineDB.SaveChanges();
                        return Content("FAIL");
                    }
                    var user = UserManager.FindByName(binduser.UserName);
                    Off_BonusRequest bonusrequest = offlineDB.Off_BonusRequest.SingleOrDefault(m => m.CheckinId == item.Id && m.Status >= 0);
                    if (bonusrequest != null)
                    {
                        if (bonusrequest.Status == 0)
                        {
                            bonusrequest.ReceiveAmount = Convert.ToInt32(item.Bonus * 100);
                            offlineDB.Entry(bonusrequest).State = System.Data.Entity.EntityState.Modified;
                        }
                        else
                        {
                            return Content("FAIL-2");
                        }
                    }
                    else
                    {
                        bonusrequest = new Off_BonusRequest()
                        {
                            CheckinId = item.Id,
                            ReceiveUserName = user.UserName,
                            ReceiveOpenId = user.OpenId,
                            ReceiveAmount = Convert.ToInt32(item.Bonus * 100),
                            RequestUserName = User.Identity.Name,
                            RequestTime = DateTime.Now,
                            Status = 0
                        };
                        offlineDB.Off_BonusRequest.Add(bonusrequest);
                    }
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

        [Authorize(Roles = "Manager")]
        public ActionResult Wx_Manager_EventList()
        {
            ViewBag.today = DateTime.Now;
            return View();
        }
        [Authorize(Roles = "Manager")]
        [HttpPost]
        public async Task<ActionResult> Wx_Manager_EventList_Partial(string date)
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            DateTime today = Convert.ToDateTime(date);
            ViewBag.Today = today;
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
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
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult> Wx_Manager_CreateEvent()
        {
            var schedule = new Wx_ManagerCreateScheduleViewModel();
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Select(m => new { Key = m.Id, Value = m.StoreName });
            ViewBag.StoreList = new SelectList(storelist, "Key", "Value");
            var today_org = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
            var templateList = offlineDB.Off_Sales_Template.Where(m => m.Off_System_Id == user.DefaultSystemId && m.Status >= 0).Select(m => new { Key = m.Id, Value = m.TemplateName });
            ViewBag.TemplateList = new SelectList(templateList, "Key", "Value");
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
                        var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                        Off_Checkin_Schedule item = new Off_Checkin_Schedule()
                        {
                            Off_Store_Id = model.Off_Store_Id,
                            Subscribe = model.Subscribe,
                            Standard_Salary = model.Standard_Salary,
                            Standard_CheckIn = Convert.ToDateTime(model.Subscribe.ToString("yyyy-MM-dd") + " " + model.Standard_CheckIn),
                            Standard_CheckOut = Convert.ToDateTime(model.Subscribe.ToString("yyyy-MM-dd") + " " + model.Standard_CheckOut),
                            Off_System_Id = user.DefaultSystemId,
                            TemplateId = model.Off_Template_Id
                        };
                        offlineDB.Off_Checkin_Schedule.Add(item);
                        offlineDB.SaveChanges();
                        return RedirectToAction("Wx_Manager_Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "店铺当天活动已存在，无法添加");
                        var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                        var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
                        var storelist = manager.Off_Store.Select(m => new { Key = m.Id, Value = m.StoreName });
                        ViewBag.StoreList = new SelectList(storelist, "Key", "Value");
                        var templateList = offlineDB.Off_Sales_Template.Where(m => m.Off_System_Id == user.DefaultSystemId && m.Status >= 0).Select(m => new { Key = m.Id, Value = m.TemplateName });
                        ViewBag.TemplateList = new SelectList(templateList, "Key", "Value");
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
                var templateList = offlineDB.Off_Sales_Template.Where(m => m.Off_System_Id == user.DefaultSystemId && m.Status >= 0).Select(m => new { Key = m.Id, Value = m.TemplateName });
                ViewBag.TemplateList = new SelectList(templateList, "Key", "Value");
                return View(model);
            }

        }

        [Authorize(Roles = "Manager")]
        public ActionResult Wx_Manager_Guide()
        {
            return View();
        }
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Wx_Manager_Task()
        {
            var today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            ViewBag.Weekly = today.AddDays(1 - (int)today.DayOfWeek).ToString("MM/dd") + " - " + today.AddDays(7 - (int)today.DayOfWeek).ToString("MM/dd");
            ViewBag.AnnounceCount = (from m in offlineDB.Off_Manager_Announcement
                                     where m.ManagerUserName.Contains(User.Identity.Name)
                                     && today >= m.StartTime && today < m.FinishTime
                                     select m).Count();
            var user = UserManager.FindById(User.Identity.GetUserId());
            var task = offlineDB.Off_Manager_Task.SingleOrDefault(m => m.TaskDate == today && m.Status >= 0 && m.UserName == User.Identity.Name && m.Off_System_Id == user.DefaultSystemId);
            if (task == null)
            {
                ViewBag.CheckInCount = 0;
            }
            else
                ViewBag.CheckInCount = task.Off_Manager_CheckIn.Count(m => m.Canceled == false);
            return View();
        }
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult> Wx_Manager_AddCheckIn()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == User.Identity.Name && m.Off_System_Id == user.DefaultSystemId);
            ViewBag.NickName = manager.NickName;
            var today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            var task = offlineDB.Off_Manager_Task.SingleOrDefault(m => m.TaskDate == today && m.Status >= 0 && m.UserName == manager.UserName && m.Off_System_Id == user.DefaultSystemId);
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
                    Status = 0,
                    UserName = User.Identity.Name,
                    NickName = manager.NickName,
                    Off_System_Id = user.DefaultSystemId
                };
                offlineDB.Off_Manager_Task.Add(item);
                await offlineDB.SaveChangesAsync();
                Off_Manager_CheckIn checkin = new Off_Manager_CheckIn();
                return View(checkin);
            }
        }
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Wx_Manager_AddCheckIn(Off_Manager_CheckIn model)
        {
            if (ModelState.IsValid)
            {
                Off_Manager_CheckIn item = new Off_Manager_CheckIn();
                var today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                var task = offlineDB.Off_Manager_Task.SingleOrDefault(m => m.TaskDate == today && m.Status >= 0 && m.UserName == User.Identity.Name && m.Off_System_Id == user.DefaultSystemId);
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
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == User.Identity.Name && m.Off_System_Id == user.DefaultSystemId);
                ViewBag.NickName = manager.NickName;
                var today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                var task = offlineDB.Off_Manager_Task.SingleOrDefault(m => m.TaskDate == today && m.UserName == User.Identity.Name && m.Status == 0 && m.Off_System_Id == user.DefaultSystemId);
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
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Wx_Manager_CheckInView()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in offlineDB.Off_Manager_Task
                       where m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId
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
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
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
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Wx_Manager_TaskReport(int? id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in offlineDB.Off_Manager_Task
                       where m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId
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
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Wx_Manager_TaskReport_Ajax(int id)
        {
            var item = offlineDB.Off_Manager_Task.SingleOrDefault(m => m.Id == id);

            return PartialView(item);
        }
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
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
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        [HttpPost]
        public JsonResult Wx_Manager_CancelCheckIn_Ajax(int id)
        {
            var item = offlineDB.Off_Manager_CheckIn.SingleOrDefault(m => m.Id == id);
            if (item.Off_Manager_Task.UserName == User.Identity.Name)
            {
                item.Canceled = true;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            return Json(new { result = "FAIL" });
        }
        [Authorize(Roles = "Manager")]
        public ActionResult Wx_Manager_Tools()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == User.Identity.Name && m.Off_System_Id == user.DefaultSystemId);
            ViewBag.NickName = manager.NickName;
            ViewBag.Mobile = manager.Mobile;
            return View();
        }
        // 0317 查看所有人签到列表
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Senior")]
        public ActionResult Wx_Senior_AllCheckInList()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in offlineDB.Off_Manager_Task
                       where m.Status == 0
                       && m.Off_System_Id == user.DefaultSystemId
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
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        public ActionResult Wx_Senior_AllCheckInList_Ajax(string date)
        {
            var _date = Convert.ToDateTime(date);
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in offlineDB.Off_Manager_Task
                       where m.TaskDate == _date && m.Status >= 0
                       && m.Off_System_Id == user.DefaultSystemId
                       select m;
            return PartialView(list);
        }


        // 0317 查看签到详情信息
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Senior")]
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

        // 0319 查询促销员
        [Authorize(Roles = "Manager")]
        public ActionResult Wx_Manager_QuerySeller()
        {
            return View();
        }

        [Authorize(Roles = "Manager")]
        [HttpPost]
        public JsonResult Wx_Manager_AjaxSellerName(string query)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in offlineDB.Off_Seller
                       where (m.Name.Contains(query) || m.Off_Store.StoreName.Contains(query))
                       && m.Off_System_Id == user.DefaultSystemId
                       select new { Id = m.Id, Name = m.Name, Store = m.Off_Store.StoreName };
            return Json(new { result = "SUCCESS", data = list.Take(5) });
        }

        [Authorize(Roles = "Manager")]
        [HttpPost]
        public JsonResult Wx_Manager_AjaxSellerDetails(int id)
        {
            var item = offlineDB.Off_Seller.SingleOrDefault(m => m.Id == id);
            return Json(new { result = "SUCCESS", data = new { Name = item.Name, Mobile = item.Mobile, CardNo = item.CardNo, CardName = item.CardName, StoreName = item.Off_Store.StoreName, AccountName = item.AccountName, IDNumber = item.IdNumber, BankName = item.AccountSource } });
        }

        // 0325 当日重点工作列表
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Wx_Manager_AnnouncementList()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            var list = from m in offlineDB.Off_Manager_Announcement
                       where m.Off_System_Id == user.DefaultSystemId && m.ManagerUserName.Contains(user.UserName)
                       && today >= m.StartTime && today < m.FinishTime
                       orderby m.Status, m.Priority descending, m.SubmitTime descending
                       select m;
            return View(list);
        }

        // 0325 督导需求提交
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult> Wx_Manager_Request_Create()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            Off_Manager_Request request = new Off_Manager_Request();
            request.ManagerUserName = user.UserName;
            request.Status = 0;
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Select(m => new { Key = m.Id, Value = m.StoreName });
            ViewBag.StoreList = new SelectList(storelist, "Key", "Value");
            List<Object> typelist = new List<object>();
            typelist.Add(new { Key = "店铺补货", Value = "店铺补货" });
            typelist.Add(new { Key = "赠品需求", Value = "赠品需求" });
            typelist.Add(new { Key = "问题调整", Value = "问题调整" });
            ViewBag.TypeList = new SelectList(typelist, "Key", "Value");
            WeChatUtilities utilities = new WeChatUtilities();
            string _url = ViewBag.Url = Request.Url.ToString();
            ViewBag.AppId = utilities.getAppId();
            string _nonce = CommonUtilities.generateNonce();
            ViewBag.Nonce = _nonce;
            string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
            ViewBag.TimeStamp = _timeStamp;
            ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url);
            return View(request);
        }
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Wx_Manager_Request_Create(Off_Manager_Request model)
        {
            if (ModelState.IsValid)
            {
                Off_Manager_Request item = new Off_Manager_Request();
                if (TryUpdateModel(item))
                {
                    item.ManagerUserName = User.Identity.Name;
                    item.RequestTime = DateTime.Now;
                    offlineDB.Off_Manager_Request.Add(item);
                    offlineDB.SaveChanges();
                    return RedirectToAction("Wx_Manager_Task");
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
                var storelist = manager.Off_Store.Select(m => new { Key = m.Id, Value = m.StoreName });
                ViewBag.StoreList = new SelectList(storelist, "Key", "Value");
                List<Object> typelist = new List<object>();
                typelist.Add(new { Key = "店铺补货", Value = "店铺补货" });
                typelist.Add(new { Key = "赠品需求", Value = "赠品需求" });
                typelist.Add(new { Key = "问题调整", Value = "问题调整" });
                ViewBag.TypeList = new SelectList(typelist, "Key", "Value");
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
        // 0328 督导需求变更
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult> Wx_Manager_Request_Edit(int id)
        {
            var item = offlineDB.Off_Manager_Request.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName);
                var storelist = manager.Off_Store.Select(m => new { Key = m.Id, Value = m.StoreName });
                ViewBag.StoreList = new SelectList(storelist, "Key", "Value", item.StoreId);
                List<Object> typelist = new List<object>();
                typelist.Add(new { Key = "店铺补货", Value = "店铺补货" });
                typelist.Add(new { Key = "赠品需求", Value = "赠品需求" });
                typelist.Add(new { Key = "问题调整", Value = "问题调整" });
                ViewBag.TypeList = new SelectList(typelist, "Key", "Value", item.RequestType);
                return View(item);
            }
            return View("Error");
        }
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Wx_Manager_Request_Edit(Off_Manager_Request model)
        {
            if (ModelState.IsValid)
            {
                Off_Manager_Request item = new Off_Manager_Request();
                if (TryUpdateModel(item))
                {
                    item.ManagerUserName = User.Identity.Name;
                    item.RequestTime = DateTime.Now;
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return RedirectToAction("Wx_Manager_Task");
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName);
                var storelist = manager.Off_Store.Select(m => new { Key = m.Id, Value = m.StoreName });
                ViewBag.StoreList = new SelectList(storelist, "Key", "Value");
                List<Object> typelist = new List<object>();
                typelist.Add(new { Key = "店铺补货", Value = "店铺补货" });
                typelist.Add(new { Key = "赠品需求", Value = "赠品需求" });
                typelist.Add(new { Key = "问题调整", Value = "问题调整" });
                ViewBag.TypeList = new SelectList(typelist, "Key", "Value");
                return View(model);
            }
        }
        // 0328 督导需求列表
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult> Wx_Manager_Request_List()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (User.IsInRole("Senior"))
            {
                var list = from m in offlineDB.Off_Manager_Request
                           where m.Status >= 0 && m.Off_Store.Off_System_Id == user.DefaultSystemId
                           orderby m.Status, m.Id descending
                           select m;
                return View(list);
            }
            else
            {
                var list = from m in offlineDB.Off_Manager_Request
                           where m.Status >= 0 && m.ManagerUserName == User.Identity.Name && m.Off_Store.Off_System_Id == user.DefaultSystemId
                           orderby m.Status, m.Id descending
                           select m;
                return View(list);
            }
        }

        // 0328 作废督导需求列表
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        [HttpPost]
        public ActionResult Wx_Manager_Request_Cancel_Ajax(int id)
        {
            var item = offlineDB.Off_Manager_Request.SingleOrDefault(m => m.Id == id);
            item.Status = -1;
            offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
            offlineDB.SaveChanges();
            return Json(new { result = "SUCCESS" });
        }

        // 0328 审核督导列表
        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        public ActionResult Wx_Manager_Request_Check(int id)
        {
            var item = offlineDB.Off_Manager_Request.SingleOrDefault(m => m.Id == id);
            WeChatUtilities utilities = new WeChatUtilities();
            string _url = ViewBag.Url = Request.Url.ToString();
            ViewBag.AppId = utilities.getAppId();
            string _nonce = CommonUtilities.generateNonce();
            ViewBag.Nonce = _nonce;
            string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
            ViewBag.TimeStamp = _timeStamp;
            ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url);
            return View(item);
        }
        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Wx_Manager_Request_Check(Off_Manager_Request model)
        {
            if (ModelState.IsValid)
            {
                Off_Manager_Request item = new Off_Manager_Request();
                if (TryUpdateModel(item))
                {
                    item.ReplyTime = DateTime.Now;
                    item.ReplyUser = User.Identity.Name;
                    item.Status = 1;
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return RedirectToAction("Wx_Manager_Request_List");
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "Error");
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
        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Wx_Manager_Request_Dismiss(Off_Manager_Request model)
        {
            if (ModelState.IsValid)
            {
                Off_Manager_Request item = new Off_Manager_Request();
                if (TryUpdateModel(item))
                {
                    item.ReplyTime = DateTime.Now;
                    item.ReplyUser = User.Identity.Name;
                    item.Status = 3;
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return RedirectToAction("Wx_Manager_Request_List");
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "Error");
                return View(model);
            }
        }
        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "BONUS")]
        public ActionResult Wx_Manager_BonusList()
        {
            return View();
        }
        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "BONUS")]
        public ActionResult Wx_Manager_BonusList_Ajax()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in offlineDB.Off_BonusRequest
                       where m.Status == 0
                       && m.Off_Checkin.Off_Checkin_Schedule.Off_System_Id == user.DefaultSystemId
                       orderby m.Off_Checkin.Off_Checkin_Schedule.Subscribe
                       select m;
            return PartialView(list);
        }

        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "BONUS")]
        [HttpPost]
        public ActionResult Wx_Manager_BonusList_Dismiss(string bonuslist)
        {
            try
            {
                string presql = "UPDATE Off_Checkin SET Bonus = NULL, Bonus_Remark = NULL where Id in (" +
                    "SELECT CheckinId FROM Off_BonusRequest WHERE Id IN (" + bonuslist + ")" +
                    ")";
                offlineDB.Database.ExecuteSqlCommand(presql);
                string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string sql = "UPDATE Off_BonusRequest SET Status = -1, CommitUserName = '" + User.Identity.Name + "', CommitTime = '" + time + "'  WHERE Id IN (" + bonuslist + ")";
                offlineDB.Database.ExecuteSqlCommand(sql);
                return Json(new { result = "SUCCESS" });
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
        }
        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "BONUS")]
        [HttpPost]
        public ActionResult Wx_Manager_BonusList_Confirm(string bonuslist)
        {
            string[] orderlist = bonuslist.Split(',');
            AppPayUtilities apppay = new AppPayUtilities();
            Random random = new Random();
            CommonUtilities.writeLog(DateTime.Now.ToShortTimeString() + "红包");
            foreach (var orderid in orderlist)
            {
                try
                {
                    int order = Convert.ToInt32(orderid);
                    var item = offlineDB.Off_BonusRequest.SingleOrDefault(m => m.Id == order);
                    if (item.Status == 0)
                    {
                        string mch_billno = "SELLERRP" + CommonUtilities.generateTimeStamp() + random.Next(1000, 9999);
                        string remark = item.Off_Checkin.Off_Checkin_Schedule.Subscribe.ToString("MM-dd") + " " + item.Off_Checkin.Off_Checkin_Schedule.Off_Store.StoreName + " " + "促销红包";
                        string result = apppay.WxRedPackCreate(item.ReceiveOpenId, item.ReceiveAmount, mch_billno, "促销员红包", "寿全斋", remark, remark);
                        if (result == "SUCCESS")
                        {
                            item.Mch_BillNo = mch_billno;
                            item.Status = 1;
                            item.CommitUserName = User.Identity.Name;
                            item.CommitTime = DateTime.Now;
                            offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                }
                catch
                {

                }
            }
            offlineDB.SaveChanges();
            return Json(new { result = "SUCCESS" });
        }
        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "BONUS")]
        public ActionResult Wx_Manager_BonusHistory_Ajax()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = (from m in offlineDB.Off_BonusRequest
                        where m.Status > 0
                        && m.Off_Checkin.Off_Checkin_Schedule.Off_System_Id == user.DefaultSystemId
                        orderby m.CommitTime descending
                        select m).Take(30);
            return PartialView(list);
        }
        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "BONUS")]
        [HttpPost]
        public async Task<ActionResult> Wx_Manager_BonusQuery_Ajax()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var query_list = from m in offlineDB.Off_BonusRequest
                             where m.Status == 1 && m.Off_Checkin.Off_Checkin_Schedule.Off_System_Id == user.DefaultSystemId
                             orderby m.CommitTime descending
                             select m;
            AppPayUtilities pay = new AppPayUtilities();
            foreach (var item in query_list)
            {
                try
                {
                    string result = await pay.WxRedPackQuery(item.Mch_BillNo);
                    switch (result)
                    {
                        case "SENT":
                            item.Status = 1;
                            break;
                        case "RECEIVED":
                            item.Status = 2;
                            break;
                        case "FAIL":
                            item.Status = 3;
                            break;
                        case "REFUND":
                            item.Status = 4;
                            break;
                        default:
                            item.Status = 1;
                            break;
                    }
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                }
                catch
                {

                }
            }
            offlineDB.SaveChanges();
            return Json(new { result = "SUCCESS" });
        }
        public ActionResult Wx_Manager_CreditInfo(int sellerid)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var Seller = offlineDB.Off_Seller.SingleOrDefault(m => m.Id == sellerid && m.Off_System_Id == user.DefaultSystemId);
            if (Seller != null)
            {
                var banklistArray = offlineDB.Off_System_Setting.SingleOrDefault(m => m.Off_System_Id == user.DefaultSystemId && m.SettingName == "BankList");
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
        public ActionResult Wx_Manager_CreditInfo(Wx_SellerCreditViewModel model)
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
                        seller.AccountSource = item.AccountSource;
                        offlineDB.Entry(seller).State = System.Data.Entity.EntityState.Modified;
                        offlineDB.SaveChanges();
                        return RedirectToAction("Wx_Manager_QuerySeller");
                    }
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "错误");
                var user = UserManager.FindById(User.Identity.GetUserId());
                var banklistArray = offlineDB.Off_System_Setting.SingleOrDefault(m => m.Off_System_Id == user.DefaultSystemId && m.SettingName == "BankList");
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


        /************ 新版本界面 ************/
        [AllowAnonymous]
        public ActionResult LoginManager_New(int? systemid)
        {
            int _systemid = systemid ?? 1;
            string user_Agent = HttpContext.Request.UserAgent;
            if (user_Agent.Contains("MicroMessenger"))
            {
                //return Content("微信");
                string redirectUri = Url.Encode("http://webapp.shouquanzhai.cn/Seller/Authorization_New");
                string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
                string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_base&state=" + _systemid + "#wechat_redirect";

                return Redirect(url);
            }
            else
            {
                return Content("其他");
            }
        }
        [AllowAnonymous]
        public async Task<ActionResult> Authorization_New(string code, string state)
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
                            return RedirectToAction("Wx_Seller_Redirect", new { systemid = systemid, version = 1 });
                        }
                    }
                    else
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToAction("Wx_Seller_Redirect", new { systemid = systemid, version = 1 });
                    }
                }
                //return Content(jat.openid + "," + jat.access_token);
                return RedirectToAction("Wx_Register", "Seller", new { open_id = jat.openid, accessToken = jat.access_token, systemid = systemid });
            }
            catch (Exception ex)
            {
                CommonUtilities.writeLog(ex.Message);
                return View("Error");
            }
        }

        [Authorize(Roles = "Manager")]
        public ActionResult Manager_Home()
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
        public PartialViewResult Manager_UserPanel()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == User.Identity.Name && m.Off_System_Id == user.DefaultSystemId);
            if (manager != null)
            {
                ViewBag.NickName = user.NickName;
                ViewBag.ImgUrl = user.ImgUrl;
                return PartialView(manager);
            }
            else
            {
                ViewBag.NickName = user.NickName;
                ViewBag.ImgUrl = user.ImgUrl;
                return PartialView();
            }

        }
        public ActionResult UpdateUserInfo()
        {
            string redirectUri = Url.Encode("http://webapp.shouquanzhai.cn/Seller/UserAuthorize");
            string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_userinfo&state=" + "1" + "#wechat_redirect";
            return Redirect(url);
        }
        
        public ActionResult UserAuthorize(string code, string state)
        {
            WeChatUtilities wechat = new WeChatUtilities();
            var jat = wechat.getWebOauthAccessToken(code);
            var user = UserManager.FindById(User.Identity.GetUserId());
            user.AccessToken = jat.access_token;
            UserManager.Update(user);
            //WeChatUtilities wechat = new WeChatUtilities();
            var userinfo = wechat.getWebOauthUserInfo(user.AccessToken, user.OpenId);
            user.NickName = userinfo.nickname;
            user.ImgUrl = userinfo.headimgurl;
            user.Sex = userinfo.sex == "1" ? true : false;
            user.Province = userinfo.province;
            user.City = userinfo.city;
            UserManager.Update(user);
            return RedirectToAction("Manager_Home");
        }
        
        
        public ActionResult Manager_Tempseller()
        {
            return View();
        }

        /************ 签到 ************/
        // 首页
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_Task()
        {
            var today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            ViewBag.Weekly = today.AddDays(1 - (int)today.DayOfWeek).ToString("MM/dd") + " - " + today.AddDays(7 - (int)today.DayOfWeek).ToString("MM/dd");
            ViewBag.AnnounceCount = (from m in offlineDB.Off_Manager_Announcement
                                     where m.ManagerUserName.Contains(User.Identity.Name)
                                     && today >= m.StartTime && today < m.FinishTime
                                     select m).Count();
            return View();
        }
        // 当前个人签到数量
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        [HttpPost]
        public JsonResult Manager_RefreshTaskCount()
        {
            var today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            var user = UserManager.FindById(User.Identity.GetUserId());
            var task = offlineDB.Off_Manager_Task.SingleOrDefault(m => m.TaskDate == today && m.Status >= 0 && m.UserName == User.Identity.Name && m.Off_System_Id == user.DefaultSystemId);
            if (task == null)
            {
                return Json(new { result = "SUCCESS", data = 0 });
            }
            else
            {
                int count = task.Off_Manager_CheckIn.Count(m => m.Canceled == false);
                return Json(new { result = "SUCCESS", data = count });
            }
        }

        // 主要工作列表
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_AnnouncementList()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            var list = from m in offlineDB.Off_Manager_Announcement
                       where m.Off_System_Id == user.DefaultSystemId && m.ManagerUserName.Contains(user.UserName)
                       && today >= m.StartTime && today < m.FinishTime
                       orderby m.Status, m.Priority descending, m.SubmitTime descending
                       select m;
            return PartialView(list);
        }

        // 添加督导签到
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult> Manager_AddCheckin()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == User.Identity.Name && m.Off_System_Id == user.DefaultSystemId);
            ViewBag.NickName = manager.NickName;
            var today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            var task = offlineDB.Off_Manager_Task.SingleOrDefault(m => m.TaskDate == today && m.Status >= 0 && m.UserName == manager.UserName && m.Off_System_Id == user.DefaultSystemId);
            if (task != null)
            {
                Off_Manager_CheckIn checkin = new Off_Manager_CheckIn();
                return PartialView(checkin);
            }
            else
            {
                Off_Manager_Task item = new Off_Manager_Task()
                {
                    TaskDate = today,
                    Status = 0,
                    UserName = User.Identity.Name,
                    NickName = manager.NickName,
                    Off_System_Id = user.DefaultSystemId
                };
                offlineDB.Off_Manager_Task.Add(item);
                await offlineDB.SaveChangesAsync();
                Off_Manager_CheckIn checkin = new Off_Manager_CheckIn();
                return PartialView(checkin);
            }
        }
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Manager_AddCheckIn(Off_Manager_CheckIn model)
        {
            if (ModelState.IsValid)
            {
                Off_Manager_CheckIn item = new Off_Manager_CheckIn();
                var today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                var task = offlineDB.Off_Manager_Task.SingleOrDefault(m => m.TaskDate == today && m.Status >= 0 && m.UserName == User.Identity.Name && m.Off_System_Id == user.DefaultSystemId);
                if (TryUpdateModel(item))
                {
                    item.Off_Manager_Task = task;
                    item.CheckIn_Time = DateTime.Now;
                    offlineDB.Off_Manager_CheckIn.Add(item);
                    await offlineDB.SaveChangesAsync();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                return Content("FAIL");
            }
        }

        // 督导日报
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_TaskReport(int? id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in offlineDB.Off_Manager_Task
                       where m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId
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
            return PartialView();
        }
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_TaskReportPartial(int id)
        {
            var item = offlineDB.Off_Manager_Task.SingleOrDefault(m => m.Id == id);
            return PartialView(item);
        }
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Manager_TaskReportPartial(Off_Manager_Task model)
        {
            if (ModelState.IsValid)
            {
                Off_Manager_Task item = new Off_Manager_Task();
                if (TryUpdateModel(item))
                {
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
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

        // 督导个人签到查询
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_CheckInView()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in offlineDB.Off_Manager_Task
                       where m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId
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
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_CheckInViewPartial(int id)
        {
            var list = from m in offlineDB.Off_Manager_CheckIn
                       where m.Manager_EventId == id
                       && m.Canceled == false
                       select m;
            ViewBag.TaskId = id;
            return PartialView(list);
        }

        // 作废签到位置
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        [HttpPost]
        public JsonResult Mananger_CancelManagerCheckin(int id)
        {
            var item = offlineDB.Off_Manager_CheckIn.SingleOrDefault(m => m.Id == id);
            if (item.Off_Manager_Task.UserName == User.Identity.Name)
            {
                item.Canceled = true;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            return Json(new { result = "FAIL" });
        }

        // 查看全部督导签到信息
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Senior")]
        public ActionResult Senior_AllCheckInList()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in offlineDB.Off_Manager_Task
                       where m.Status == 0
                       && m.Off_System_Id == user.DefaultSystemId
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
            return PartialView();
        }
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Senior")]
        public ActionResult Senior_AllCheckInListPartial(string date)
        {
            var _date = Convert.ToDateTime(date);
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in offlineDB.Off_Manager_Task
                       where m.TaskDate == _date && m.Status >= 0
                       && m.Off_System_Id == user.DefaultSystemId
                       select m;
            return PartialView(list);
        }
        // 督导签到详情
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Senior_CheckInDetails(int id)
        {
            var item = offlineDB.Off_Manager_Task.SingleOrDefault(m => m.Id == id);
            return PartialView(item);
        }

        // 添加需求
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_RequestCreate()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            Off_Manager_Request request = new Off_Manager_Request();
            request.ManagerUserName = user.UserName;
            request.Status = 0;
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Select(m => new { Key = m.Id, Value = m.StoreName });
            ViewBag.StoreList = new SelectList(storelist, "Key", "Value");
            List<Object> typelist = new List<object>();
            typelist.Add(new { Key = "店铺补货", Value = "店铺补货" });
            typelist.Add(new { Key = "赠品需求", Value = "赠品需求" });
            typelist.Add(new { Key = "问题调整", Value = "问题调整" });
            ViewBag.TypeList = new SelectList(typelist, "Key", "Value");
            return PartialView(request);
        }
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Mananger_RequestCreate(Off_Manager_Request model)
        {
            if (ModelState.IsValid)
            {
                Off_Manager_Request item = new Off_Manager_Request();
                if (TryUpdateModel(item))
                {
                    item.ManagerUserName = User.Identity.Name;
                    item.RequestTime = DateTime.Now;
                    offlineDB.Off_Manager_Request.Add(item);
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                var user = UserManager.FindById(User.Identity.GetUserId());
                var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
                var storelist = manager.Off_Store.Select(m => new { Key = m.Id, Value = m.StoreName });
                ViewBag.StoreList = new SelectList(storelist, "Key", "Value");
                List<Object> typelist = new List<object>();
                typelist.Add(new { Key = "店铺补货", Value = "店铺补货" });
                typelist.Add(new { Key = "赠品需求", Value = "赠品需求" });
                typelist.Add(new { Key = "问题调整", Value = "问题调整" });
                ViewBag.TypeList = new SelectList(typelist, "Key", "Value");
                return PartialView(model);
            }
        }

        // 修改需求
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_RequestEdit(int id)
        {
            var item = offlineDB.Off_Manager_Request.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
                var storelist = manager.Off_Store.Select(m => new { Key = m.Id, Value = m.StoreName });
                ViewBag.StoreList = new SelectList(storelist, "Key", "Value", item.StoreId);
                List<Object> typelist = new List<object>();
                typelist.Add(new { Key = "店铺补货", Value = "店铺补货" });
                typelist.Add(new { Key = "赠品需求", Value = "赠品需求" });
                typelist.Add(new { Key = "问题调整", Value = "问题调整" });
                ViewBag.TypeList = new SelectList(typelist, "Key", "Value", item.RequestType);
                return PartialView(item);
            }
            return PartialView("Error");
        }
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Manager_RequestEdit(Off_Manager_Request model)
        {
            if (ModelState.IsValid)
            {
                Off_Manager_Request item = new Off_Manager_Request();
                if (TryUpdateModel(item))
                {
                    item.ManagerUserName = User.Identity.Name;
                    item.RequestTime = DateTime.Now;
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                var user = UserManager.FindById(User.Identity.GetUserId());
                var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
                var storelist = manager.Off_Store.Select(m => new { Key = m.Id, Value = m.StoreName });
                ViewBag.StoreList = new SelectList(storelist, "Key", "Value");
                List<Object> typelist = new List<object>();
                typelist.Add(new { Key = "店铺补货", Value = "店铺补货" });
                typelist.Add(new { Key = "赠品需求", Value = "赠品需求" });
                typelist.Add(new { Key = "问题调整", Value = "问题调整" });
                ViewBag.TypeList = new SelectList(typelist, "Key", "Value");
                return View(model);
            }
        }

        // 需求列表
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_RequestList()
        {
            return PartialView();
        }
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_RequestListPartial()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (User.IsInRole("Senior"))
            {
                var list = from m in offlineDB.Off_Manager_Request
                           where m.Status >= 0 && m.Off_Store.Off_System_Id == user.DefaultSystemId
                           orderby m.Status, m.Id descending
                           select m;
                return PartialView(list);
            }
            else
            {
                var list = from m in offlineDB.Off_Manager_Request
                           where m.Status >= 0 && m.ManagerUserName == User.Identity.Name && m.Off_Store.Off_System_Id == user.DefaultSystemId
                           orderby m.Status, m.Id descending
                           select m;
                return PartialView(list);
            }
        }
        // 作废需求内容
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        [HttpPost]
        public JsonResult Manager_CancelRequestJson(int id)
        {
            var item = offlineDB.Off_Manager_Request.SingleOrDefault(m => m.Id == id);
            item.Status = -1;
            offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
            offlineDB.SaveChanges();
            return Json(new { result = "SUCCESS" });
        }

        // 需求查看
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_RequestView(int id)
        {
            var item = offlineDB.Off_Manager_Request.SingleOrDefault(m => m.Id == id);
            return PartialView(item);
        }

        /************ 巡店 ************/
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_Tools()
        {
            return View();
        }

        // 刷新店铺数量
        [Authorize(Roles = "Manager")]
        [HttpPost]
        public JsonResult Manager_RefreshAllCount()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            DateTime today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Select(m => m.Id);
            var today_schedule = from m in offlineDB.Off_Checkin_Schedule
                                 where storelist.Contains(m.Off_Store_Id)
                                 && m.Subscribe == today
                                 select m;
            int uncheckin = (from m in today_schedule
                             where m.Off_Checkin.Count(p => p.Status >= 0) == 0
                             select m).Count();
            int uncheckout = (from m in today_schedule
                              where m.Off_Checkin.Any(p => p.Status == 1)
                              select m).Count();
            int unreport = (from m in today_schedule
                            where m.Off_Checkin.Any(p => p.Status == 2)
                            select m).Count();
            int unconfirm = (from m in offlineDB.Off_Checkin_Schedule
                             where m.Off_Checkin.Any(p => p.Status == 3) &&
                             storelist.Contains(m.Off_Store_Id)
                             select m).Count();
            return Json(new { result = "SUCCESS", data = new { uncheckin = uncheckin, uncheckout = uncheckout, unreport = unreport, unconfirm = unconfirm } });
        }

        // 未签到列表
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_UnCheckInList()
        {
            return PartialView();
        }
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_UnCheckInListPartial(string date)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            DateTime _date = Convert.ToDateTime(date);
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Select(m => m.Id);
            var today_schedule = from m in offlineDB.Off_Checkin_Schedule
                                 where storelist.Contains(m.Off_Store_Id)
                                 && m.Subscribe == _date
                                 && m.Off_Checkin.Count(p => p.Status >= 0) == 0
                                 orderby m.Off_Store.StoreName
                                 select m;
            return PartialView(today_schedule);
        }

        // 门店促销员列表
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_ScheduleSeller(int id)
        {
            var schedule = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == id);
            ViewBag.StoreName = schedule.Off_Store.StoreName;
            var sellerlist = schedule.Off_Store.Off_Seller;
            return PartialView(sellerlist);
        }

        // 未签退列表
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_UnCheckOutList()
        {
            return PartialView();
        }
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_UnCheckOutListPartial(string date)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            DateTime _date = Convert.ToDateTime(date);
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Select(m => m.Id);
            var checkin = from m in offlineDB.Off_Checkin
                          where storelist.Contains(m.Off_Checkin_Schedule.Off_Store_Id)
                          && m.Off_Checkin_Schedule.Subscribe == _date
                          && m.Status == 1
                          orderby m.Off_Checkin_Schedule.Off_Store.StoreName
                          select m;
            return PartialView(checkin);
        }
        // 未提报销量列表
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_UnReportList()
        {
            return PartialView();
        }
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_UnReportListPartial(string date)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            DateTime _date = Convert.ToDateTime(date);
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Select(m => m.Id);
            var checkin = from m in offlineDB.Off_Checkin
                          where storelist.Contains(m.Off_Checkin_Schedule.Off_Store_Id)
                          && m.Off_Checkin_Schedule.Subscribe == _date
                          && m.Status == 2
                          orderby m.Off_Checkin_Schedule.Off_Store.StoreName
                          select m;
            return PartialView(checkin);
        }

        // 待确认销量列表
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_UnConfirmList()
        {
            return PartialView();
        }
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_UnConfirmListPartial()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Select(m => m.Id);
            var checkin = from m in offlineDB.Off_Checkin
                          where storelist.Contains(m.Off_Checkin_Schedule.Off_Store_Id)
                          && m.Status == 3
                          select m;
            var dategroup = from m in checkin
                                group m by m.Off_Checkin_Schedule.Subscribe into g
                                orderby g.Key descending
                                select new { g.Key };
            List<DateTime> p = new List<DateTime>();
            foreach(var item in dategroup)
            {
                p.Add(item.Key);
            }
            ViewBag.DateGroup = p;
            return PartialView(checkin);
        }

        // 作废签到信息
        [Authorize(Roles = "Manager")]
        [HttpPost]
        public ActionResult Manager_DeleteCheckIn(int id)
        {
            var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                item.Status = -1;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            return Json(new { result = "FAIL" });
        }

        // 代签到
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_CreateCheckIn(int id)
        {
            var schedule = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == id);
            ViewBag.StoreName = schedule.Off_Store.StoreName;
            ViewBag.Subscribe = schedule.Subscribe;
            Off_Checkin item = new Off_Checkin()
            {
                Off_Schedule_Id = id,
                Status = 0
            };
            var sellerlist = from m in offlineDB.Off_Seller
                             where m.StoreId == schedule.Off_Store_Id
                             select m;
            ViewBag.SellerDropDown = new SelectList(sellerlist, "Id", "Name");
            return PartialView(item);
        }
        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Manager_CreateCheckIn(Off_Checkin model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin checkin = new Off_Checkin();
                if (TryUpdateModel(checkin))
                {
                    // 获取模板产品列表
                    checkin.Report_Time = DateTime.Now;
                    checkin.CheckinLocation = "N/A";
                    checkin.CheckoutLocation = "N/A";
                    checkin.ConfirmTime = DateTime.Now;
                    checkin.ConfirmUser = User.Identity.Name;
                    checkin.Proxy = true;
                    checkin.Status = 3;
                    //offlineDB.Entry(checkin).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.Off_Checkin.Add(checkin);
                    offlineDB.SaveChanges();
                    List<int> plist = new List<int>();
                    var Template = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == checkin.Off_Schedule_Id).Off_Sales_Template;
                    foreach (var i in Template.ProductList.Split(','))
                    {
                        plist.Add(Convert.ToInt32(i));
                    }
                    var productlist = from m in offlineDB.Off_Product
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

                        if (sales == null && storage == null && amount == null)
                        { }
                        else
                        {
                            Off_Checkin_Product existdata = new Off_Checkin_Product()
                            {
                                Off_Checkin = checkin,
                                ItemCode = item.ItemCode,
                                ProductId = item.Id,
                                SalesAmount = amount,
                                SalesCount = sales,
                                StorageCount = storage
                            };
                            offlineDB.Off_Checkin_Product.Add(existdata);
                        }
                    }
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
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
        public PartialViewResult Manager_EditReport_Item(int id, int ScheduleId)
        {
            Off_Checkin item = null;
            string[] plist_tmp;
            if (id == 0)
            {
                var schedule = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == ScheduleId);
                plist_tmp = schedule.Off_Sales_Template.ProductList.Split(',');
            }
            else
            {
                item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == id);
                plist_tmp = item.Off_Checkin_Schedule.Off_Sales_Template.ProductList.Split(',');
            }
            List<int> plist = new List<int>();
            foreach (var i in plist_tmp)
            {
                plist.Add(Convert.ToInt32(i));
            }
            var productlist = from m in offlineDB.Off_Product
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
            if (item != null)
            {
                foreach (var i in item.Off_Checkin_Product)
                {
                    var e = templatelist.SingleOrDefault(m => m.ProductId == i.ProductId);
                    e.SalesCount = i.SalesCount;
                    e.SalesAmount = i.SalesAmount;
                    e.Storage = i.StorageCount;
                }

                Wx_ReportItemsViewModel model = new Wx_ReportItemsViewModel()
                {
                    AmountRequried = item.Off_Checkin_Schedule.Off_Sales_Template.RequiredAmount,
                    StorageRequired = item.Off_Checkin_Schedule.Off_Sales_Template.RequiredStorage,
                    ProductList = templatelist
                };
                return PartialView(model);
            }
            else
            {
                var schedule = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == ScheduleId);
                Wx_ReportItemsViewModel model = new Wx_ReportItemsViewModel()
                {
                    AmountRequried = schedule.Off_Sales_Template.RequiredAmount,
                    StorageRequired = schedule.Off_Sales_Template.RequiredStorage,
                    ProductList = templatelist
                };
                return PartialView(model);
            }
        }

        // 查看并修改签到信息
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_EditCheckin(int id)
        {
            var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == id);
            List<Object> status_list = new List<object>();
            status_list.Add(new { Key = 1, Value = "已签到" });
            status_list.Add(new { Key = 2, Value = "已签退" });
            status_list.Add(new { Key = 3, Value = "已提报销量" });
            ViewBag.StatusSelectList = new SelectList(status_list, "Key", "Value", item.Status);
            return PartialView(item);
        }
        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Manager_EditCheckin(Off_Checkin model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin checkin = new Off_Checkin();
                if (TryUpdateModel(checkin))
                {
                    // 获取模板产品列表
                    List<int> plist = new List<int>();
                    var Template = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == checkin.Off_Schedule_Id).Off_Sales_Template;
                    foreach (var i in Template.ProductList.Split(','))
                    {
                        plist.Add(Convert.ToInt32(i));
                    }
                    var productlist = from m in offlineDB.Off_Product
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
                        var checkinproductlist = offlineDB.Off_Checkin_Product.Where(m => m.CheckinId == checkin.Id);
                        var existdata = checkinproductlist.SingleOrDefault(m => m.ProductId == item.Id);
                        if (existdata != null)
                        {

                            if (sales == null && storage == null && amount == null)
                            {
                                // 无数据则删除
                                offlineDB.Off_Checkin_Product.Remove(existdata);
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
                                existdata = new Off_Checkin_Product()
                                {
                                    CheckinId = checkin.Id,
                                    ItemCode = item.ItemCode,
                                    ProductId = item.Id,
                                    SalesAmount = amount,
                                    SalesCount = sales,
                                    StorageCount = storage
                                };
                                offlineDB.Off_Checkin_Product.Add(existdata);
                                //offlineDB.SaveChanges();
                            }
                        }
                    }
                    checkin.Report_Time = DateTime.Now;
                    checkin.CheckinLocation = checkin.CheckinLocation == null ? "N/A" : checkin.CheckinLocation;
                    checkin.CheckoutLocation = checkin.CheckoutLocation == null ? "N/A" : checkin.CheckoutLocation;
                    checkin.ConfirmTime = DateTime.Now;
                    checkin.ConfirmUser = User.Identity.Name;
                    checkin.Proxy = true;
                    offlineDB.Entry(checkin).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "错误");
                return Content("FAIL");
            }
        }

        // 审核签到信息
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_CheckinConfirm(int id)
        {
            var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == id);
            return PartialView(item);
        }

        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Manager_CheckinConfirm(Off_Checkin model)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin checkin = new Off_Checkin();
                if (TryUpdateModel(checkin))
                {
                    checkin.ConfirmTime = DateTime.Now;
                    checkin.ConfirmUser = User.Identity.Name;
                    checkin.Status = 4;
                    offlineDB.Entry(checkin).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                return Content("FAIL");
            }
        }

        // 查看销量明细列表
        [Authorize(Roles = "Manager")]
        public PartialViewResult Manager_ViewReport_Item(int id)
        {

            var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == id);
            var plist_tmp = item.Off_Checkin_Schedule.Off_Sales_Template.ProductList.Split(',');
            List<int> plist = new List<int>();
            foreach (var i in plist_tmp)
            {
                plist.Add(Convert.ToInt32(i));
            }
            var productlist = from m in offlineDB.Off_Product
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
            foreach (var i in item.Off_Checkin_Product)
            {
                var e = templatelist.SingleOrDefault(m => m.ProductId == i.ProductId);
                e.SalesCount = i.SalesCount;
                e.SalesAmount = i.SalesAmount;
                e.Storage = i.StorageCount;
            }

            Wx_ReportItemsViewModel model = new Wx_ReportItemsViewModel()
            {
                AmountRequried = item.Off_Checkin_Schedule.Off_Sales_Template.RequiredAmount,
                StorageRequired = item.Off_Checkin_Schedule.Off_Sales_Template.RequiredStorage,
                ProductList = templatelist
            };
            return PartialView(model);
        }

        // 查看促销信息详细信息
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_ViewConfirm(int id)
        {
            var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == id);
            return PartialView(item);
        }

        /************ 工具 ************/

        // 销量排名
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_ReportList()
        {
            ViewBag.today = DateTime.Now;
            var user = UserManager.FindById(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = from m in manager.Off_Store
                            group m by m.StoreSystem into g
                            select new { Key = g.Key };
            ViewBag.StoreSystem = new SelectList(storelist, "Key", "Key", storelist.FirstOrDefault().Key);
            return PartialView();
        }

        [Authorize(Roles = "Manager")]
        public ActionResult Manager_ReportListPartial(string date, string storesystem)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            DateTime today = Convert.ToDateTime(date);
            ViewBag.Today = today;
            int dow = (int)today.DayOfWeek;
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Where(m => m.StoreSystem == storesystem).Select(m => m.Id);
            var listview = from m in offlineDB.Off_Checkin
                           where storelist.Contains(m.Off_Checkin_Schedule.Off_Store_Id)
                           && m.Off_Checkin_Schedule.Subscribe == today
                           && m.Status >= 4
                           select new
                           {
                               Id = m.Id,
                               Status = m.Status,
                               StoreId = m.Off_Checkin_Schedule.Off_Store_Id,
                               SellerName = m.Off_Seller.Name,
                               StoreName = m.Off_Checkin_Schedule.Off_Store.StoreName,
                               Rep_Total = m.Off_Checkin_Product.Sum(g => g.SalesCount),
                               Bonus = m.Bonus
                           };
            //var storelist = list.Select(m => m.StoreId);
            var avglist = from m in offlineDB.Off_AVG_Info
                          where m.DayOfWeek == dow + 1 && m.Off_Store.Off_System_Id == user.DefaultSystemId &&
                          storelist.Contains(m.StoreId)
                          select new { StoreId = m.StoreId, AVG_Total = m.AVG_SalesData, AVG_Amount = m.AVG_AmountData };

            var finallist = from m1 in listview
                            join m2 in avglist on m1.StoreId equals m2.StoreId into lists
                            from m in lists.DefaultIfEmpty()
                            select new Wx_ManagerReportListViewModel
                            {
                                Id = m1.Id,
                                Status = m1.Status,
                                StoreId = m1.StoreId,
                                SellerName = m1.SellerName,
                                StoreName = m1.StoreName,
                                Rep_Total = m1.Rep_Total,
                                Bonus = m1.Bonus,
                                AVG_Total = m.AVG_Total
                            };
            return PartialView(finallist);
        }

        // 活动门店列表
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_EventList()
        {
            return PartialView();
        }
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_EventListPartial(string date)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            DateTime today = Convert.ToDateTime(date);
            ViewBag.Today = today;
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Select(m => m.Id);
            var schedulelist = from m in offlineDB.Off_Checkin_Schedule
                               where m.Subscribe == today
                               && storelist.Contains(m.Off_Store_Id)
                               select m;
            return PartialView(schedulelist);
        }

        // 删除活动记录
        [Authorize(Roles = "Manager")]
        [HttpPost]
        public ActionResult Manager_DeleteEvent(int id)
        {
            var schedule = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == id);
            if (schedule!=null)
            {
                // 确认活动预约下是否有没有作废的签到
                var exist = schedule.Off_Checkin.Any(m => m.Status >= 0);
                if (!exist)
                {
                    offlineDB.Off_Checkin_Schedule.Remove(schedule);
                    offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                else
                {
                    return Json(new { result = "FAIL" });
                }
            }
            return Json(new { result = "FAIL" });
        }

        // 添加日程记录
        [Authorize(Roles ="Manager")]
        public ActionResult Manager_CreateEvent()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store;
            ViewBag.StoreList = storelist;
            var grouplist = from m in storelist
                            group m by m.StoreSystem into g
                            select g.Key;
            ViewBag.GroupList = grouplist;
            Off_Checkin_Schedule model = new Off_Checkin_Schedule();
            model.Off_System_Id = user.DefaultSystemId;
            return PartialView(model);
        }
        [Authorize(Roles ="Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Manager_CreateEvent(FormCollection form)
        {
            return Content("SUCCESS");
        }

        // 


        // 管辖门店列表
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_StoreList()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.OrderBy(m => m.StoreName);
            return PartialView(storelist);
        }

        // 管辖促销员列表
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_SellerList()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Select(m => m.Id);
            var sellerlist = from m in offlineDB.Off_Seller
                             where storelist.Contains(m.StoreId) && m.Off_System_Id == user.DefaultSystemId
                             orderby m.Name
                             select m;
            return PartialView(sellerlist);
        }
        // 促销红包填写
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_CheckinBonusRemark(int id)
        {
            var checkin = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == id);
            return PartialView(checkin);
        }

        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Manager_CheckinBonusRemark(Off_Checkin model)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin item = new Off_Checkin();
                if (TryUpdateModel(item))
                {
                    item.Bonus_User = User.Identity.Name;
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    var manager = UserManager.FindById(User.Identity.GetUserId());
                    var binduser = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Off_Seller_Id == item.Off_Seller_Id && m.Off_System_Id == manager.DefaultSystemId && m.Type==1);
                    if (binduser == null)
                    {
                        offlineDB.SaveChanges();
                        return Content("FAIL");
                    }
                    var user = UserManager.FindByName(binduser.UserName);
                    Off_BonusRequest bonusrequest = offlineDB.Off_BonusRequest.SingleOrDefault(m => m.CheckinId == item.Id && m.Status >= 0);
                    if (bonusrequest != null)
                    {
                        if (bonusrequest.Status == 0)
                        {
                            bonusrequest.ReceiveAmount = Convert.ToInt32(item.Bonus * 100);
                            offlineDB.Entry(bonusrequest).State = System.Data.Entity.EntityState.Modified;
                        }
                        else
                        {
                            return Content("FAIL");
                        }
                    }
                    else
                    {
                        bonusrequest = new Off_BonusRequest()
                        {
                            CheckinId = item.Id,
                            ReceiveUserName = user.UserName,
                            ReceiveOpenId = user.OpenId,
                            ReceiveAmount = Convert.ToInt32(item.Bonus * 100),
                            RequestUserName = User.Identity.Name,
                            RequestTime = DateTime.Now,
                            Status = 0
                        };
                        offlineDB.Off_BonusRequest.Add(bonusrequest);
                    }
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return View("Error");
            }
            else
            {
                return Content("FAIL");
            }
        }
        // 查看促销员详细信息
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_SellerDetails(int id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var seller = offlineDB.Off_Seller.SingleOrDefault(m => m.Id == id && m.Off_System_Id == user.DefaultSystemId);
            return PartialView(seller);
        }
        // 修改促销员信息
        [Authorize(Roles = "Manager")]
        public ActionResult Manager_EditSellerInfo(int id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var seller = offlineDB.Off_Seller.SingleOrDefault(m => m.Id == id && m.Off_System_Id == user.DefaultSystemId);
            var banklistArray = offlineDB.Off_System_Setting.SingleOrDefault(m => m.Off_System_Id == user.DefaultSystemId && m.SettingName == "BankList");
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
            return PartialView("Error");
        }
        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Manager_EditSellerInfo(Off_Seller model)
        {
            if (ModelState.IsValid)
            {
                Off_Seller seller = new Off_Seller();
                if (TryUpdateModel(seller))
                {
                    seller.UploadTime = DateTime.Now;
                    seller.UploadUser = User.Identity.Name;
                    offlineDB.Entry(seller).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                else
                {
                    return Content("FAIL");
                }
            }
            return Content("FAIL");
        }


        // 红包信息列表
        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "BONUS")]
        public ActionResult Manager_BonusList()
        {
            return PartialView();
        }
        // 未发红包列表
        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "BONUS")]
        public ActionResult Manager_BonusList_UnSendPartial()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in offlineDB.Off_BonusRequest
                       where m.Status == 0
                       && m.Off_Checkin.Off_Checkin_Schedule.Off_System_Id == user.DefaultSystemId
                       orderby m.Off_Checkin.Off_Checkin_Schedule.Subscribe
                       select m;
            return PartialView(list);
        }
        // 确认审核红包
        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "BONUS")]
        [HttpPost]
        public ActionResult Manager_BonusConfirm(int id)
        {
            AppPayUtilities apppay = new AppPayUtilities();
            Random random = new Random();
            CommonUtilities.writeLog(DateTime.Now.ToShortTimeString() + "红包");

            try
            {
                var item = offlineDB.Off_BonusRequest.SingleOrDefault(m => m.Id == id);
                if (item.Status == 0)
                {
                    string mch_billno = "SELLERRP" + CommonUtilities.generateTimeStamp() + random.Next(1000, 9999);
                    string remark = item.Off_Checkin.Off_Checkin_Schedule.Subscribe.ToString("MM-dd") + " " + item.Off_Checkin.Off_Checkin_Schedule.Off_Store.StoreName + " " + "促销红包";
                    string result = apppay.WxRedPackCreate(item.ReceiveOpenId, item.ReceiveAmount, mch_billno, "促销员红包", "寿全斋", remark, remark);
                    if (result == "SUCCESS")
                    {
                        item.Mch_BillNo = mch_billno;
                        item.Status = 1;
                        item.CommitUserName = User.Identity.Name;
                        item.CommitTime = DateTime.Now;
                        offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        offlineDB.SaveChanges();
                        return Json(new { result = "SUCCESS" });
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
            catch
            {
                return Json(new { result = "FAIL" });
            }
        }

        // 作废红包
        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "BONUS")]
        [HttpPost]
        public ActionResult Manager_BonusDismiss(int id)
        {
            try
            {
                var bonusrequest = offlineDB.Off_BonusRequest.SingleOrDefault(m => m.Id == id);
                var checkin = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == bonusrequest.CheckinId);
                bonusrequest.Status = -1;
                bonusrequest.CommitUserName = User.Identity.Name;
                bonusrequest.CommitTime = DateTime.Now;
                offlineDB.Entry(bonusrequest).State = System.Data.Entity.EntityState.Modified;
                checkin.Bonus = null;
                checkin.Bonus_Remark = null;
                offlineDB.Entry(checkin).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
        }

        // 历史红包信息
        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "BONUS")]
        public ActionResult Manager_BonusList_HistoryPartial()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = (from m in offlineDB.Off_BonusRequest
                        where m.Status > 0
                        && m.Off_Checkin.Off_Checkin_Schedule.Off_System_Id == user.DefaultSystemId
                        orderby m.CommitTime descending
                        select m).Take(30);
            return PartialView(list);
        }

        [Authorize(Roles = "Senior")]
        [SettingFilter(SettingName = "BONUS")]
        [HttpPost]
        public async Task<ActionResult> Manager_BonusList_HistoryRefresh()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var query_list = from m in offlineDB.Off_BonusRequest
                             where m.Status == 1 && m.Off_Checkin.Off_Checkin_Schedule.Off_System_Id == user.DefaultSystemId
                             orderby m.CommitTime descending
                             select m;
            AppPayUtilities pay = new AppPayUtilities();
            foreach (var item in query_list)
            {
                try
                {
                    string result = await pay.WxRedPackQuery(item.Mch_BillNo);
                    switch (result)
                    {
                        case "SENT":
                            item.Status = 1;
                            break;
                        case "RECEIVED":
                            item.Status = 2;
                            break;
                        case "FAIL":
                            item.Status = 3;
                            break;
                        case "REFUND":
                            item.Status = 4;
                            break;
                        default:
                            item.Status = 1;
                            break;
                    }
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                }
                catch
                {

                }
            }
            offlineDB.SaveChanges();
            return Json(new { result = "SUCCESS" });
        }

        /************ 暗促 ************/
        // 暗促首页
        [Authorize(Roles ="Manager")]
        public ActionResult Manager_SellerTaskHome()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = string.Join(",", manager.Off_Store.Select(m => m.Id));
            // 使用SQL查询
            string sql = "SELECT t.Id,t.ApplyDate, Min(t3.StorageCount) as MinStorage, T4.StoreName FROM[dbo].[Off_SellerTask] as t left join dbo.Off_SellerTaskProduct as t3 on t.Id= t3.SellerTaskId left join" +
                " dbo.Off_Store as T4 on t.StoreId = T4.Id where t.Id = (select top 1 t2.Id from [dbo].[Off_SellerTask] t2 where t2.StoreId in (" + storelist + ") and t2.StoreId = t.StoreId order by T2.ApplyDate desc) and t3.StorageCount>0" +
                " group by t.Id, T4.StoreName, t.ApplyDate having MIN(t3.StorageCount)<50";
            var tasklist = offlineDB.Database.SqlQuery<Wx_SellerTaskAlert>(sql);
            ViewBag.AlertCount = tasklist.Count();
            return PartialView();
        }

        // 暗促签到查看
        [Authorize(Roles = "Manager")]
        public ActionResult ManagerSellerTaskMonthStatistic()
        {
            var startDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-01"));
            ViewBag.CurrentMonth = startDate.ToString("yyyy-MM");
            List<object> l = new List<object>();
            for (int i = 0; i < 3; i++)
            {
                var t_date = startDate.AddMonths(0 - i);
                l.Add(new { Key = t_date.ToString("yyyy-MM"), Value = t_date.ToString("yyyy-MM") });
            }
            ViewBag.SelectMonth = new SelectList(l, "Key", "Value");
            return PartialView();
        }

        [Authorize(Roles = "Manager")]
        public ActionResult ManagerSellerTaskMonthStatisticPartial(string querydate)
        {
            // 获取督导的门店列表
            var user = UserManager.FindById(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Select(m => m.Id);
            // 查看参数，如无参数，默认为当月数据，也可查询上月数据
            DateTime startDate;
            if (querydate == "" || querydate == null)
            {
                startDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-01"));
            }
            else
            {
                startDate = Convert.ToDateTime(querydate + "-01");
            }
            var finishDate = startDate.AddMonths(1);
            // 获取督导对应门店的当月/或者指定月份的暗促信息

            var tasklist = from m in offlineDB.Off_SellerTask
                           where storelist.Contains(m.StoreId)
                           && m.ApplyDate >= startDate && m.ApplyDate < finishDate
                           group m by m.Off_Seller into g
                           select new Wx_SellerTaskMonthStatistic { Off_Seller = g.Key, AttendanceCount = g.Count() * 100 / 30 };
            //ViewBag.TaskList = tasklist;
            return PartialView(tasklist);
        }

        // 暗促促销员信息
        [Authorize(Roles = "Manager")]
        public ActionResult ManagerSellerTaskSeller(int id)
        {
            ViewBag.SellerId = id;
            return PartialView();
        }

        [Authorize(Roles = "Manager")]
        public ActionResult ManagerSellerTaskSellerPartial(int id, int? page)
        {
            // 第一页为1
            int _page = page ?? 1;
            _page--;
            var tasklist = (from m in offlineDB.Off_SellerTask
                            where m.SellerId == id
                            orderby m.ApplyDate descending
                            select m).Skip(_page * 20).Take(20);
            if (tasklist.Count() > 0)
            {
                return PartialView(tasklist);
            }
            else
                return Content("NONE");
        }

        // 暗促详情
        [Authorize(Roles = "Manager")]
        public ActionResult ManagerSellerTaskDetails(int id)
        {
            var item = offlineDB.Off_SellerTask.SingleOrDefault(m => m.Id == id);
            return PartialView(item);
        }

        // 库存预警
        [Authorize(Roles = "Manager")]
        public ActionResult ManangerSellerTaskStorageAlert()
        {
            // 最新的库存预紧
            // 获取督导的店铺列表
            var user = UserManager.FindById(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = string.Join(",", manager.Off_Store.Select(m => m.Id));
            // 使用SQL查询
            string sql = "SELECT t.Id,t.ApplyDate, Min(t3.StorageCount) as MinStorage, T4.StoreName FROM[dbo].[Off_SellerTask] as t left join dbo.Off_SellerTaskProduct as t3 on t.Id= t3.SellerTaskId left join" +
                " dbo.Off_Store as T4 on t.StoreId = T4.Id where t.Id = (select top 1 t2.Id from [dbo].[Off_SellerTask] t2 where t2.StoreId in (" + storelist + ") and t2.StoreId = t.StoreId order by T2.ApplyDate desc) and t3.StorageCount>0" +
                " group by t.Id, T4.StoreName, t.ApplyDate having MIN(t3.StorageCount)<50";
            var tasklist = offlineDB.Database.SqlQuery<Wx_SellerTaskAlert>(sql);
            return PartialView(tasklist);
        }

        // 暗促信息查询
        [Authorize(Roles = "Manager")]
        public ActionResult ManangerSellerTaskQuery()
        {
            // 获取督导的门店列表
            var user = UserManager.FindById(User.Identity.GetUserId());
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName && m.Off_System_Id == user.DefaultSystemId);
            var storelist = manager.Off_Store.Select(m => m.Id);
            var tasklist = from m in offlineDB.Off_SellerTask
                           where storelist.Contains(m.StoreId)
                           group m by m.Off_Seller into g
                           select new Wx_SellerTaskMonthStatistic { Off_Seller = g.Key, AttendanceCount = g.Count() };
            //ViewBag.TaskList = tasklist;
            return PartialView(tasklist);
        }
        public ActionResult ManagerAddSchedule()
        {
            return View();
        }
        public ActionResult Seller_Home()
        {
            return View();
        }
    }
}