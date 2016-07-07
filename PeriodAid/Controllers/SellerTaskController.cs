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
    }
}