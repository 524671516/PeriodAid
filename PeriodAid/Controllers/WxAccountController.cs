using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using PeriodAid.Models;
using System.IO;
using System.Text;
using System.Net;
using PeriodAid.DAL;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace PeriodAid.Controllers
{
    public class WxAccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public WxAccountController()
        {
        }

        public WxAccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        public ActionResult Wx_Login(string redirectUrl)
        {
            string user_Agent = HttpContext.Request.UserAgent;
            if (user_Agent.Contains("MicroMessenger"))
            {
                //return Content("微信");
                string redirectUri = Url.Encode("http://webapp.shouquanzhai.cn/WxAccount/Wx_Authorization");
                string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
                string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_base&state=" + "0" + "#wechat_redirect";

                return Redirect(url);
            }
            else
            {
                return Content("其他");
            }
        }

        public ActionResult LoginManager()
        {
            string user_Agent = HttpContext.Request.UserAgent;
            if(user_Agent.Contains("MicroMessenger"))
            {
                //return Content("微信");
                string redirectUri = Url.Encode("http://webapp.shouquanzhai.cn/WxAccount/Wx_Authorization");
                string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
                string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_base&state=" + "0" + "#wechat_redirect";

                return Redirect(url);
            }
            else
            { 
                return Content("其他");
            }
        }

        public async Task<ActionResult> Wx_Authorization(string code, string state)
        {
            //return Content(code);
            //string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            if(state == "0")
            {
                // 微信普通登陆
                WeChatUtilities wechat = new WeChatUtilities();
                var jat = wechat.getWebOauthAccessToken(code);
                var user = UserManager.FindByEmail(jat.openid);
                if (user != null)
                {
                    //var user = UserManager.FindByName("13636314852");
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    return RedirectToAction("Index", "PeriodAid");
                }
                //return Content(jat.openid + "," + jat.access_token);
                return RedirectToAction("Wx_Register", new { open_id = jat.openid, accessToken = jat.access_token });
            }
            else if(state == "1")
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
                return Content("1");
            }

        }

        public ActionResult Wx_Register(string open_id, string accessToken)
        {
            var model = new Wx_RegisterViewModel();
            model.Open_Id = open_id;
            model.AccessToken = accessToken;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Wx_Register(string open_id, Wx_RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 手机号校验
                var exist_user = UserManager.FindByName(model.Mobile);
                if(exist_user != null)
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
                else if(smsRecord.ValidateCode != model.CheckCode)
                {
                    ModelState.AddModelError("CheckCode", "手机验证码错误");
                    return View(model);
                }
                else if(smsRecord.SendDate.AddSeconds(1800) <= DateTime.Now)
                {
                    ModelState.AddModelError("CheckCode", "手机验证码超时");
                    return View(model);
                }
                else
                {
                    var user = new ApplicationUser { UserName = model.Mobile, Email = model.Open_Id, PhoneNumber = model.Mobile, AccessToken = model.AccessToken, OpenId = model.Open_Id };
                    var result = await UserManager.CreateAsync(user, open_id);
                    if (result.Succeeded)
                    {
                        smsRecord.Status = true;
                        smsDB.SaveChanges();
                        
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToAction("Wx_RedirectUpdateUser");
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Wx_SendSms(string mobile)
        {
            if (Regex.IsMatch(mobile, "1[3|5|7|8|][0-9]{9}"))
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

        public ActionResult Wx_RedirectUpdateUser()
        {
            string redirectUri = Url.Encode("http://webapp.shouquanzhai.cn/WxAccount/Wx_Authorization");
            string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_userinfo&state=" + "1" + "#wechat_redirect";
            return Redirect(url);
        }

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
        
        public async Task<ActionResult> Wx_UpdateUserInfo()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            //string url = "https://api.weixin.qq.com/sns/userinfo?access_token=" + user.AccessToken + "&openid=" + user.OpenId + "&lang=zh_CN";
            WeChatUtilities wechat = new WeChatUtilities();
            var userinfo = wechat.getWebOauthUserInfo(user.AccessToken, user.OpenId);
            user.NickName = userinfo.nickname;
            user.ImgUrl = userinfo.headimgurl;
            user.Sex = userinfo.sex == "1"? true :false;
            user.Province = userinfo.province;
            user.City = userinfo.city;
            UserManager.Update(user);
            return RedirectToAction("UserHome","PeriodAid");
        }

        public ActionResult CheckCode()
        {
            //生成验证码
            ValidateCode validateCode = new ValidateCode();
            string code = validateCode.CreateValidateCode(4);
            Session["ValidateCode"] = code;
            byte[] bytes = validateCode.CreateValidateGraphic(code);
            return File(bytes, @"image/jpeg");
        }

        public async Task<ActionResult> TestAddUser(string username)
        {
            var user = new ApplicationUser { UserName = "13636314852", Email = "13636314852@139.com", PhoneNumber = "13636314852" };
            var result = await UserManager.CreateAsync(user, "13636314852");
            if (result.Succeeded)
            {
                return Content("Success");
            }
            else
                return Content("Failure");
        }

        public async Task<ActionResult> TestLogin(string username)
        {
            var user = UserManager.FindByName("13817657312");
            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            return Content("Success");
        }

        /*public ActionResult TestImageUpload()
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

        public JsonResult SaveOrignalImage(string serverId)
        {
            try
            {
                WeChatUtilities utilities = new WeChatUtilities();
                string url = "http://file.api.weixin.qq.com/cgi-bin/media/get?access_token=" + utilities.getAccessToken() + "&media_id=" + serverId;
                System.Uri httpUrl = new System.Uri(url);
                HttpWebRequest req = (HttpWebRequest)(WebRequest.Create(httpUrl));
                //req.Timeout = 180000; //设置超时值10秒
                //req.UserAgent = "XXXXX";
                //req.Accept = "XXXXXX";
                req.Method = "GET";
                HttpWebResponse res = (HttpWebResponse)(req.GetResponse());
                Bitmap img = new Bitmap(res.GetResponseStream());//获取图片流
                string folder = HttpContext.Server.MapPath("~/Content/downloads/");
                string filename = DateTime.Now.ToFileTime().ToString() + ".jpg";
                img.Save(folder + filename);//随机名
                //ViewBag.filename = filename;
                return Json(new { result = "SUCCESS", filename = filename }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                string aa = ex.Message;
            }

            return Json(new { result = "FAIL" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CutImage(string url, int beginX, int beginY, int getX, int getY, string fileName)
        {
            
                string folder = HttpContext.Server.MapPath("~/Content/downloads/");
                Bitmap bitmap = new Bitmap(folder + url);//原图
                if (((beginX + getX) <= bitmap.Width) && ((beginY + getY) <= bitmap.Height))
                {
                    Bitmap destBitmap = new Bitmap(getX, getY);//目标图
                    Rectangle destRect = new Rectangle(0, 0, getX, getY);//矩形容器
                    Rectangle srcRect = new Rectangle(beginX, beginY, getX, getY);

                    Graphics g = Graphics.FromImage(destBitmap);
                    g.DrawImage(bitmap, destRect, srcRect, GraphicsUnit.Pixel);
                    ImageFormat format = ImageFormat.Jpeg;
                    
                    destBitmap.Save(folder + fileName, format);
                    return Json(new { result = "SUCCESS", filename = fileName }, JsonRequestBehavior.AllowGet);
                }
            return Json(new { result = "FAIL" }, JsonRequestBehavior.AllowGet);
        }*/
        public ActionResult TestAudio()
        {
            return View();
        }

        /*public ContentResult TestPrint()
        {
            
            //return Content("SUCCESS");
        }*/

        public ContentResult TestSendMessage(string mobile)
        {
            string url = "http://121.40.60.163:8081/message/balance?loginname=180&password=sqz180sqz";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            string postdata = "loginname=180&password=sqz180sqz";
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
            return Content(result);
        }
        
    }
}