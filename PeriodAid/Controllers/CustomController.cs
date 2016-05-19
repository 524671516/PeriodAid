using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PeriodAid.Models;
using PeriodAid.DAL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System.Net;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace PeriodAid.Controllers
{
    public class CustomController : Controller
    {
        // GET: Custom
        OfflineSales offlineDB = new OfflineSales();
        Promotion promotionDB = new Promotion();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public CustomController()
        {

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
        public CustomController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ActionResult Index()


        {
            return View();
        }

        /********** 个性化图片上传 **********/
        #region 个性化图片上传
        // 微信登陆
        public ActionResult Wx_RedirectUploadImage()
        {
            string redirectUri = Url.Encode("http://webapp.shouquanzhai.cn/Custom/Wx_Authorization");
            string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_userinfo&state=" + "0" + "#wechat_redirect";
            return Redirect(url);
        }

        public ActionResult Wx_Authorization(string code, string state)
        {
            WeChatUtilities wechat = new WeChatUtilities();
            var jat = wechat.getWebOauthAccessToken(code);
            var userinfo = wechat.getWebOauthUserInfo(jat.access_token, jat.openid);
            return RedirectToAction("UploadImage", new { open_id = userinfo.openid, nickname = Url.Encode(userinfo.nickname) });
        }

        // 欢迎界面&上传图片
        public ActionResult UploadImage(string open_id, string nickname)
        {
            ViewBag.OpenId = open_id;
            ViewBag.Nickname = HttpUtility.UrlDecode(nickname);
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

        public JsonResult SaveOrignalImage(string serverId, string openId, string nickname)
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
                DateTime current = DateTime.Now;
                CustomOrder order = new CustomOrder()
                {
                    OpenId = openId,
                    NickName = nickname,
                    OrignalImage = filename,
                    OrderStatus = 0,
                    CardStatus = 0,
                    OrderNumber = "SQZ" + current.ToString("yyyyMMddHHmmss")
                };
                offlineDB.CustomOrder.Add(order);
                offlineDB.SaveChanges();
                //ViewBag.filename = filename;
                return Json(new { result = "SUCCESS", orderId = order.Id }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                string aa = ex.Message;
            }

            return Json(new { result = "FAIL" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CutImage(int orderId)
        {
            var order = offlineDB.CustomOrder.SingleOrDefault(m => m.Id == orderId);
            if (order != null)
            {
                CustomImage_ViewModel model = new CustomImage_ViewModel()
                {
                    crop_h = 50,
                    crop_w = 50,
                    crop_x = 400,
                    crop_y = 300,
                    filename = order.OrignalImage
                };
                return View(model);
            }
            else
            {
                return View("Error");
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CutImage(int orderId, CustomImage_ViewModel model)
        {
            if (ModelState.IsValid)
            {
                var order = offlineDB.CustomOrder.SingleOrDefault(m => m.Id == orderId);
                if (order != null)
                {
                    string folder = HttpContext.Server.MapPath("~/Content/downloads/");
                    Bitmap bitmap = new Bitmap(folder + model.filename);//原图
                    string cutfilename = model.filename.Substring(0, model.filename.LastIndexOf('.')) + "-cut.jpg";
                    if (((model.crop_x + model.crop_w) <= bitmap.Width) && ((model.crop_y + model.crop_h) <= bitmap.Height))
                    {
                        Bitmap destBitmap = new Bitmap(model.crop_w, model.crop_h);//目标图
                        Rectangle destRect = new Rectangle(0, 0, model.crop_w, model.crop_h);//矩形容器
                        Rectangle srcRect = new Rectangle(model.crop_x, model.crop_y, model.crop_w, model.crop_h);
                        Graphics g = Graphics.FromImage(destBitmap);
                        g.DrawImage(bitmap, destRect, srcRect, GraphicsUnit.Pixel);

                        destBitmap.Save(folder + cutfilename, ImageFormat.Jpeg);
                        order.OrderStatus = 1;
                        order.CropImage = cutfilename;
                        offlineDB.SaveChanges();
                    }
                    return RedirectToAction("CustomSuccess", new { orderId = orderId });
                }
                else
                {
                    return View("Error");
                }
            }
            else
            {
                ModelState.AddModelError("", "初始化信息错误");
                return View(model);
            }
        }

        public ActionResult CustomSuccess(int orderId)
        {
            var order = offlineDB.CustomOrder.SingleOrDefault(m => m.Id == orderId);
            if (order != null)
            {
                return View(order);
            }
            else
            {
                return View("Error");
            }
        }

        public ActionResult getPreviewImage(string filename, int beginX, int beginY, int getX, int getY)
        {
            string folder = HttpContext.Server.MapPath("~/Content/downloads/");
            Bitmap bitmap = new Bitmap(folder + filename);//原图
            WeChatUtilities utilities = new WeChatUtilities();
            string _url = ViewBag.Url = Request.Url.ToString();
            ViewBag.AppId = utilities.getAppId();
            string _nonce = CommonUtilities.generateNonce();
            ViewBag.Nonce = _nonce;
            string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
            ViewBag.TimeStamp = _timeStamp;
            ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url);
            if (((beginX + getX) <= bitmap.Width) && ((beginY + getY) <= bitmap.Height))
            {
                Bitmap destBitmap = new Bitmap(getX, getY);//目标图
                Rectangle destRect = new Rectangle(0, 0, getX, getY);//矩形容器
                Rectangle srcRect = new Rectangle(beginX, beginY, getX, getY);
                Graphics g = Graphics.FromImage(destBitmap);
                g.DrawImage(bitmap, destRect, srcRect, GraphicsUnit.Pixel);
                MemoryStream stream = new MemoryStream();
                destBitmap.Save(stream, ImageFormat.Jpeg);
                return File(stream.ToArray(), @"image/jpeg");
            }
            else
            {
                return File(folder + filename, @"image/jpeg");
            }
        }
        #endregion

        public ActionResult TestLocation()
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

        /********** 语音祝福 **********/
        #region 语音祝福
        public ActionResult CustomVoice(string code)
        {
            if (code == null)
            {
                Random r = new Random(Convert.ToInt32(CommonUtilities.generateTimeStamp()));
                int rand_digit = r.Next(0, 2);
                switch (rand_digit)
                {
                    case 0:
                        code = "A4DGSPG1";
                        break;
                    case 1:
                        code = "BOI98R2A";
                        break;
                    default:
                        break;
                }
            }
            ViewBag.Code = code;
            ViewBag.Background = "webapp_voice_background.jpg";
            switch (code)
            {
                case "A4DGSPG1":
                    ViewBag.voice = "audio_01.mp3";
                    ViewBag.voice_img = "audio_01_background.jpg";
                    break;
                case "BOI98R2A":
                    ViewBag.voice = "audio_02.mp3";
                    ViewBag.voice_img = "audio_02_background.jpg";
                    break;
                case "CB012ASP":
                    ViewBag.voice = "audio_03.mp3";
                    ViewBag.voice_img = "audio_03_background.jpg";
                    break;
                case "DC39K9BS":
                    ViewBag.voice = "audio_04.mp3";
                    ViewBag.voice_img = "audio_01_background.jpg";
                    break;
                case "EB2FSP4M":
                    ViewBag.voice = "audio_05.mp3";
                    ViewBag.voice_img = "audio_05_background.jpg";
                    ViewBag.Background = "webapp_voice_38_background.jpg";
                    break;
                case "FPO84BVA":
                    ViewBag.voice = "audio_05.mp3";
                    ViewBag.voice_img = "audio_06_background.jpg";
                    ViewBag.Background = "webapp_voice_38_background.jpg";
                    break;
                default:
                    break;
            }
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
        #endregion

        /************ 百家企业送福利 **************/
        #region 百家企业送福利
        public ActionResult Wx_Redirect_Benefits()
        {
            string redirectUri = Url.Encode("http://webapp.shouquanzhai.cn/Custom/Wx_Benefits_Authorization");
            string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_base&state=" + "0" + "#wechat_redirect";
            return Redirect(url);
        }
        public ActionResult Wx_Benefits_Authorization(string code, string state)
        {
            WeChatUtilities wechat = new WeChatUtilities();
            var jat = wechat.getWebOauthAccessToken(code);
            //var userinfo = wechat.getWebOauthUserInfo(jat.access_token, jat.openid);
            return RedirectToAction("Benefits_main", new { open_id = jat.openid });
        }
        public ActionResult Benefits_main(string open_id)
        {
            if (open_id == null)
            {
                return RedirectToAction("Benefits_error");
            }
            else
            {
                string openid = open_id ?? "";
                WeChatUtilities utilities = new WeChatUtilities();
                string _url = ViewBag.Url = Request.Url.ToString();
                ViewBag.AppId = utilities.getAppId();
                string _nonce = CommonUtilities.generateNonce();
                ViewBag.Nonce = _nonce;
                string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
                ViewBag.TimeStamp = _timeStamp;
                ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url);
                ViewBag.OpenId = open_id;
                var item = offlineDB.Benefits.SingleOrDefault(m => m.OpenId == open_id);
                if (item == null)
                {
                    Benefits benefits = new Benefits()
                    {
                        OpenId = open_id,
                        Status = 0,
                        Company = "1",
                        Name = "1",
                        Industry = "1",
                        Region = "1",
                        Reason = "1",
                        Staff = 1,
                        Mobile = "1",
                        JobTitle = "1",
                        Share = false
                    };
                    offlineDB.Benefits.Add(benefits);
                    offlineDB.SaveChanges();
                }
                else
                {
                    if (item.Status == 2)
                    {
                        return RedirectToAction("Benefits_done", new { open_id = open_id });
                    }
                }
                return View();
            }
        }
        [HttpPost]
        public JsonResult Benefits_confirm_share(string open_id)
        {
            var item = offlineDB.Benefits.SingleOrDefault(m => m.OpenId == open_id);
            if (item != null)
            {
                item.Status = 1;
                offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            else
            {
                return Json(new { result = "FAIL" });
            }

        }
        public ActionResult Benefits_info(string open_id)
        {
            List<SelectListItem> industryList = new List<SelectListItem>();
            industryList.Add(new SelectListItem() { Text = "机构组织", Value = "机构组织" });
            industryList.Add(new SelectListItem() { Text = "医药卫生", Value = "医药卫生" });
            industryList.Add(new SelectListItem() { Text = "建筑建材", Value = "建筑建材" });
            industryList.Add(new SelectListItem() { Text = "旅游休闲", Value = "旅游休闲" });
            industryList.Add(new SelectListItem() { Text = "家居用品", Value = "家居用品" });
            industryList.Add(new SelectListItem() { Text = "交通运输", Value = "交通运输" });
            industryList.Add(new SelectListItem() { Text = "信息产业", Value = "信息产业" });
            industryList.Add(new SelectListItem() { Text = "机械机电", Value = "机械机电" });
            industryList.Add(new SelectListItem() { Text = "轻工食品", Value = "轻工食品" });
            industryList.Add(new SelectListItem() { Text = "专业服务", Value = "专业服务" });
            industryList.Add(new SelectListItem() { Text = "其他", Value = "其他" });
            ViewBag.Industry = new SelectList(industryList, "Value", "Text", "-请选择行业-");

            List<SelectListItem> regionList = new List<SelectListItem>();
            regionList.Add(new SelectListItem() { Text = "北京", Value = "北京" });
            regionList.Add(new SelectListItem() { Text = "上海", Value = "上海" });
            regionList.Add(new SelectListItem() { Text = "广东", Value = "广东" });
            regionList.Add(new SelectListItem() { Text = "深圳", Value = "深圳" });
            regionList.Add(new SelectListItem() { Text = "其他", Value = "其他" });
            ViewBag.Region = new SelectList(regionList, "Value", "Text", "-请选择区域-");



            Benefits_ViewModel model = new Benefits_ViewModel()
            {
                OpenId = open_id,
                Status = 0
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Benefits_info(string open_id, Benefits_ViewModel model)
        {
            if (ModelState.IsValid)
            {
                var item = offlineDB.Benefits.SingleOrDefault(m => m.OpenId == open_id);
                if (item != null)
                {
                    // 已分享阶段
                    if (item.Status == 1)
                    {
                        item.Name = model.Name;
                        item.Mobile = model.Mobile;
                        item.JobTitle = model.JobTitle;
                        item.Reason = model.Reason;
                        item.Staff = model.Staff;
                        item.Industry = model.Industry;
                        item.Region = model.Region;
                        item.Company = model.Company;
                        item.Address = model.Address;
                        item.Status = 2;
                        offlineDB.SaveChanges();
                    }
                    // 已完成阶段
                    else if (item.Status == 2)
                    {

                    }
                    else
                    {
                        item.Status = 0;
                        offlineDB.SaveChanges();
                        return RedirectToAction("benefits_main", new { open_id = open_id });
                    }
                }
                else
                {
                    return RedirectToAction("benefits_main", new { open_id = open_id });
                }
                return RedirectToAction("Benefits_done", new { open_id = open_id });
            }
            else
            {
                List<SelectListItem> industryList = new List<SelectListItem>();
                industryList.Add(new SelectListItem() { Text = "机构组织", Value = "机构组织" });
                industryList.Add(new SelectListItem() { Text = "医药卫生", Value = "医药卫生" });
                industryList.Add(new SelectListItem() { Text = "建筑建材", Value = "建筑建材" });
                industryList.Add(new SelectListItem() { Text = "旅游休闲", Value = "旅游休闲" });
                industryList.Add(new SelectListItem() { Text = "家居用品", Value = "家居用品" });
                industryList.Add(new SelectListItem() { Text = "交通运输", Value = "交通运输" });
                industryList.Add(new SelectListItem() { Text = "信息产业", Value = "信息产业" });
                industryList.Add(new SelectListItem() { Text = "机械机电", Value = "机械机电" });
                industryList.Add(new SelectListItem() { Text = "轻工食品", Value = "轻工食品" });
                industryList.Add(new SelectListItem() { Text = "专业服务", Value = "专业服务" });
                industryList.Add(new SelectListItem() { Text = "其他", Value = "其他" });
                ViewBag.Industry = new SelectList(industryList, "Value", "Text", "-请选择行业-");

                List<SelectListItem> regionList = new List<SelectListItem>();
                regionList.Add(new SelectListItem() { Text = "北京", Value = "北京" });
                regionList.Add(new SelectListItem() { Text = "上海", Value = "上海" });
                regionList.Add(new SelectListItem() { Text = "广东", Value = "广东" });
                regionList.Add(new SelectListItem() { Text = "深圳", Value = "深圳" });
                regionList.Add(new SelectListItem() { Text = "其他", Value = "其他" });
                ViewBag.Region = new SelectList(regionList, "Value", "Text", "-请选择区域-");
                ModelState.AddModelError("", "表单错误");
                return View(model);
            }
        }
        public ActionResult Benefits_done(string open_id)
        {
            if (open_id == null)
            {
                return RedirectToAction("Benefits_error");
            }
            else
            {
                string openid = open_id ?? "";
                WeChatUtilities utilities = new WeChatUtilities();
                string _url = ViewBag.Url = Request.Url.ToString();
                ViewBag.AppId = utilities.getAppId();
                string _nonce = CommonUtilities.generateNonce();
                ViewBag.Nonce = _nonce;
                string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
                ViewBag.TimeStamp = _timeStamp;
                ViewBag.Signature = utilities.generateWxJsApiSignature(_nonce, utilities.getJsApiTicket(), _timeStamp, _url);
                ViewBag.OpenId = open_id;
                return View();
            }
        }
        [HttpPost]
        public ActionResult Benefits_submit_share(string open_id)
        {
            var item = offlineDB.Benefits.SingleOrDefault(m => m.OpenId == open_id);
            if (item != null)
            {
                item.Share = true;
                offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }
        public ActionResult Benefits_error()
        {
            return View();
        }
        #endregion

        public ActionResult LyyjPromotion()
        {
            return View();
        }

        // 糖酒会活动红包发送
        #region 糖酒会活动红包发送
        public ActionResult Wx_Beacon_Tjh()
        {
            return View();
        }


        public ActionResult Wx_Redirect_RedPack_Tjh()
        {
            string redirectUri = Url.Encode("http://webapp.shouquanzhai.cn/Custom/Wx_RedPack_Tjh_Authorization");
            string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_base&state=" + "0" + "#wechat_redirect";
            return Redirect(url);
        }
        public ActionResult Wx_RedPack_Tjh_Authorization(string code, string state)
        {
            WeChatUtilities wechat = new WeChatUtilities();
            var jat = wechat.getWebOauthAccessToken(code);
            //var userinfo = wechat.getWebOauthUserInfo(jat.access_token, jat.openid);
            return RedirectToAction("Wx_RedPack_Tjh_main", new { open_id = jat.openid });
        }
        public ActionResult Wx_RedPack_Tjh_main(string open_id)
        {
            bool exist = promotionDB.Promotion_TJH.Where(m => m.openId == open_id).Count() > 0;
            if (!exist)
            {
                var order = new Promotion_TJH_ViewModel()
                {
                    openId = open_id
                };
                List<SelectListItem> industryList = new List<SelectListItem>();
                industryList.Add(new SelectListItem() { Text = "媒体", Value = "媒体" });
                industryList.Add(new SelectListItem() { Text = "经销商", Value = "经销商" });
                industryList.Add(new SelectListItem() { Text = "采购", Value = "采购" });
                industryList.Add(new SelectListItem() { Text = "广告", Value = "广告" });
                industryList.Add(new SelectListItem() { Text = "其他", Value = "其他" });
                ViewBag.Industry = new SelectList(industryList, "Value", "Text", "-请选择行业-");
                return View(order);
            }
            else
                return RedirectToAction("Wx_RedPack_Tjh_Result", new { openId = open_id });
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Wx_RedPack_Tjh_main(Promotion_TJH_ViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 写入数据
                var existitem = promotionDB.Promotion_TJH.SingleOrDefault(m => m.openId == model.openId);
                if (existitem == null)
                {
                    Promotion_TJH_ViewModel item = new Promotion_TJH_ViewModel();
                    if (TryUpdateModel(item))
                    {
                        Promotion_TJH entry = new Promotion_TJH();
                        entry.name = item.name;
                        entry.openId = item.openId;
                        entry.mobile = item.mobile;
                        entry.branch = item.branch;
                        Random r = new Random(DateTime.Now.Millisecond);
                        string billno = "WxRedPackTjh_" + CommonUtilities.generateTimeStamp() + r.Next(1000, 9999);
                        entry.mch_billno = billno;
                        entry.status = 0;
                        entry.submit_time = DateTime.Now;
                        promotionDB.Promotion_TJH.Add(entry);
                        await promotionDB.SaveChangesAsync();
                        // 创建红包
                        int amount = r.Next(100, 500);
                        AppPayUtilities pay = new AppPayUtilities();
                        string result = await pay.WxRedPackCreateAsync(entry.openId, amount, entry.mch_billno, "糖酒会红包", "寿全斋", "糖酒会红包", "感谢您的关注");
                        //
                        entry.mch_result = result;
                        promotionDB.Entry(entry).State = System.Data.Entity.EntityState.Modified;
                        promotionDB.SaveChanges();
                        return RedirectToAction("Wx_RedPack_Tjh_Result", new { openid = model.openId });
                    }
                    return View("Error");
                }
                else
                {
                    return RedirectToAction("Wx_RedPack_Tjh_Result", new { openId = model.openId });
                }
            }
            else
            {
                ModelState.AddModelError("", "数据录入错误");
                List<SelectListItem> industryList = new List<SelectListItem>();
                industryList.Add(new SelectListItem() { Text = "媒体", Value = "媒体" });
                industryList.Add(new SelectListItem() { Text = "经销商", Value = "经销商" });
                industryList.Add(new SelectListItem() { Text = "采购", Value = "采购" });
                industryList.Add(new SelectListItem() { Text = "广告", Value = "广告" });
                industryList.Add(new SelectListItem() { Text = "其他", Value = "其他" });
                ViewBag.Industry = new SelectList(industryList, "Value", "Text", "-请选择行业-");
                return View(model);
            }
        }
        public async Task<ActionResult> Wx_RedPack_Tjh_Result(string openid)
        {
            try
            {
                var item = promotionDB.Promotion_TJH.SingleOrDefault(m => m.openId == openid);
                if (item != null)
                {
                    if (item.status == 0)
                    {
                        AppPayUtilities pay = new AppPayUtilities();
                        string result = await pay.WxRedPackQuery(item.mch_billno);
                        CommonUtilities.writeLog(result);
                        if (result == "RECEIVED")
                        {
                            item.status = 1;
                            item.mch_result = result;
                            promotionDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                            await promotionDB.SaveChangesAsync();
                            return View("Wx_RedPack_Tjh_Success");
                        }
                        else if (result == "FAILED")
                        {
                            item.status = 2;
                            item.mch_result = result;
                            promotionDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                            await promotionDB.SaveChangesAsync();
                            return View("Wx_RedPack_Tjh_Fail");
                        }
                        else if (result == "REFUND")
                        {
                            item.status = 3;
                            item.mch_result = result;
                            promotionDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                            await promotionDB.SaveChangesAsync();
                            return View("Wx_RedPack_Tjh_Fail");
                        }
                        else
                        {
                            item.status = 0;
                            item.mch_result = result;
                            promotionDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                            await promotionDB.SaveChangesAsync();
                            return View("Wx_RedPack_Tjh_Success");
                        }
                    }
                    else
                    {
                        if(item.status == 1)
                            return View("Wx_RedPack_Tjh_Success");
                        else
                            return View("Wx_RedPack_Tjh_Fail");
                    }
                }
                return View("Wx_RedPack_Tjh_Fail");
            }
            catch 
            {
                return View("Error");
            }
        }
        #endregion

        public async Task<ActionResult> QueryAllRedPack()
        {
            var list = from m in promotionDB.Promotion_TJH
                       where m.status == 0
                       select m;
            int count = 0;
            foreach(var item in list)
            {
                AppPayUtilities pay = new AppPayUtilities();
                string result = await pay.WxRedPackQuery(item.mch_billno);
                CommonUtilities.writeLog(result);
                if (result == "RECEIVED")
                {
                    item.status = 1;
                    item.mch_result = result;
                    promotionDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                }
                else if (result == "FAILED")
                {
                    item.status = 2;
                    item.mch_result = result;
                    promotionDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                }
                else if (result == "REFUND")
                {
                    item.status = 3;
                    item.mch_result = result;
                    promotionDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    item.status = 0;
                    item.mch_result = result;
                    promotionDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                }
                count++;
            }
            await promotionDB.SaveChangesAsync();
            return Content("SUCCESS:" + count);
        }

        public ActionResult TestVoice()
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

        public ActionResult ColdTutorial()
        {
            return View();
        }

        // 三级联动地址
        public ActionResult TestRegion(P_Presents model)
        {
            //P_Presents model = new P_Presents();
            return View(model);
        }
        [HttpPost]
        public ActionResult TestRegion(P_Presents model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                P_Presents present = new P_Presents();
                if(TryUpdateModel(present))
                {
                    ERPCustomOrder_details d1 = new ERPCustomOrder_details()
                    {
                        qty = 1,
                        price = 0,
                        item_code = "sqz122"
                    };
                    ERPCustomOrder order = new ERPCustomOrder()
                    {
                        platform_code = model.plattform_code,
                        event_name = "闺蜜礼",
                        receiver_name = model.target_ReceiverName,
                        receiver_address = model.target_ReceiverState + " " + model.target_ReceiverCity + " " + model.target_ReceiverDistrict + " " + model.target_ReceiverAddress,
                        receiver_mobile = model.target_ReceiverMobile,
                        receiver_province = model.target_ReceiverState,
                        receiver_city = model.target_ReceiverCity,
                        receiver_district = model.target_ReceiverDistrict,
                        receiver_zip = model.target_ReceiverZip,
                        details = new List<ERPCustomOrder_details>()
                    };
                    order.details.Add(d1);
                    ERPOrderUtilities util = new ERPOrderUtilities();
                    util.createOrder(order);
                    present.status = 1;
                    promotionDB.Entry(present).State = System.Data.Entity.EntityState.Modified;
                    promotionDB.SaveChanges();
                    return RedirectToAction("GirlFriend_Result", new { open_id = present.openId });
                }
                else
                {
                    return View("GirlFriend_None");
                }
            }
            else
            {
                ModelState.AddModelError("", "格式错误");
                return View(model);
            }
        }
        public ActionResult GetRegion(int level, int? parentid)
        {
            KDTUtilites util = new KDTUtilites();
            string result = util.KDT_GetRegion(level, parentid);
            return Content(result);
        }
        /*public ActionResult GetTrade(string keywords)
        {
            KDTUtilites util = new KDTUtilites();
            DateTime st = new DateTime(2016, 4, 1);
            DateTime et = new DateTime(2016, 5, 1);
            string result = util.KDT_GetTrade(st, et, keywords);
            return Content(result);
        }*/
        public ActionResult Wx_Redirect_Girlfriend()
        {
            string redirectUri = Url.Encode("http://webapp.shouquanzhai.cn/Custom/Wx_Girlfriend_Authorization");
            string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_base&state=" + "0" + "#wechat_redirect";
            return Redirect(url);
        }
        public ActionResult Wx_Girlfriend_Authorization(string code, string state)
        {
            WeChatUtilities wechat = new WeChatUtilities();
            var jat = wechat.getWebOauthAccessToken(code);
            //var userinfo = wechat.getWebOauthUserInfo(jat.access_token, jat.openid);
            return RedirectToAction("GirlFriend_Start", new { open_id = jat.openid });
        }
        public ActionResult GirlFriend_Start(string open_id)
        {
            ViewBag.OpenId = open_id;
            var item = promotionDB.P_Presents.SingleOrDefault(m => m.openId == open_id);
            if (item != null)
            {
                if (item.status == 0)
                {

                    return View("TestRegion", item);
                }
                else
                {
                    return RedirectToAction("GirlFriend_Result", new { open_id = item.openId });
                }
            }
            return View();
        }
        [HttpPost]
        public ActionResult GirlFriend_Start(FormCollection form)
        {

            string openid = form["openid"].ToString();
            var item = promotionDB.P_Presents.SingleOrDefault(m => m.openId == openid);
            if (item != null)
            {
                if (item.status == 0)
                {

                    return View("TestRegion", item);
                }
                else
                {
                    return RedirectToAction("GirlFriend_Result", new { open_id = item.openId });
                }
            }
            else
            {
                ERPOrderDataContext erpdb = new ERPOrderDataContext();
                DateTime st = new DateTime(2016, 4, 1);
                DateTime et = new DateTime(2016, 5, 2);
                string mobile = form["mobile"].ToString();
                var orderlist = from m in erpdb.orders
                                where m.receiver_mobile == mobile &&
                                m.createtime >= st && m.createtime <= et
                                && m.shop_code == "微信商城"
                                select m;
                if (orderlist.Count() > 0)
                {
                    var order = orderlist.FirstOrDefault();
                    P_Presents p = new P_Presents()
                    {
                        source_ReceiverName = order.receiver_name,
                        source_ReceiverMobile = order.receiver_mobile,
                        source_ReceiverAddress = order.receiver_address,
                        status = 0,
                        openId = form["openid"].ToString(),
                        create_time = DateTime.Now,
                        plattform_code = "P" + DateTime.Now.ToString("yyyyMMddHHmmss") + form["openid"].ToString()
                    };
                    promotionDB.P_Presents.Add(p);
                    promotionDB.SaveChanges();
                    return View("TestRegion", p);
                }
                return View("GirlFriend_None");
            }
        }
        // 订单结果
        public ActionResult GirlFriend_Result(string open_id)
        {
            // 获取订单
            var order = promotionDB.P_Presents.SingleOrDefault(m => m.openId == open_id);
            if (order != null && order.status == 1)
            {
                ERPOrderUtilities util = new ERPOrderUtilities();
                Orders_Result result = util.getERPORDERS(order.plattform_code, "线上其他渠道");
                if (result.orders != null)
                {
                    var o = result.orders.FirstOrDefault();
                    var delivery = o.deliverys.FirstOrDefault();
                    if (delivery != null)
                    {
                        if (delivery.mail_no != null && delivery.delivery == true)
                        {
                            order.mail_no = delivery.mail_no;
                            order.express_name = delivery.express_name;
                            order.status = 2;
                            promotionDB.Entry(order).State = System.Data.Entity.EntityState.Modified;
                            promotionDB.SaveChanges();
                        }
                    }
                }
                return View(order);
            }
            else
            {
                // 无订单,显示没有找到订单
                return View("GirlFriend_None");
            }
        }
    }
}