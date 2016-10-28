using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PeriodAid.DAL;
using System.Text;
using System.Net;
using System.Xml;
using System.IO;
using PeriodAid.Models;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

namespace PeriodAid.Controllers
{
    public class PayController : Controller
    {
        // GET: Pay
        OfflineSales offlineDB = new OfflineSales();
        PressConference pcdb = new PressConference();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public PayController()
        {

        }

        public PayController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        public async Task<ActionResult> QueryWxPack(string orderno)
        {
            AppPayUtilities appPay = new AppPayUtilities();
            var result = await appPay.WxRedPackQuery(orderno);
            return Content(result);
        }
        public async Task<ActionResult> CreateWxPack(string openid)
        {

            AppPayUtilities appPay = new AppPayUtilities();
            var result = await appPay.WxRedPackCreateAsync(openid, 100);
            return Content(result);
        }
        #region 微信支付通知接口
        /// <summary>
        /// 支付通知接口
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult wx_pay_nofity()
        {
            StreamReader reader = new StreamReader(Request.InputStream, Encoding.UTF8);
            XmlDocument doc = new XmlDocument();
            doc.Load(reader);
            //CommonUtilities.writeLog(doc.OuterXml);
            string return_code = doc.GetElementsByTagName("return_code")[0].InnerText;
            try
            {
                if (return_code == "SUCCESS")
                {
                    string appid = doc.GetElementsByTagName("appid")[0]== null ? "" : doc.GetElementsByTagName("appid")[0].InnerText;
                    string mch_id = doc.GetElementsByTagName("mch_id")[0] == null ? "" : doc.GetElementsByTagName("mch_id")[0].InnerText;
                    string is_subscribe = doc.GetElementsByTagName("is_subscribe")[0] == null ? "" : doc.GetElementsByTagName("is_subscribe")[0].InnerText;
                    string nonce_str = doc.GetElementsByTagName("nonce_str")[0] == null ? "" : doc.GetElementsByTagName("nonce_str")[0].InnerText;
                    string request_sign = doc.GetElementsByTagName("sign")[0] == null ? "" : doc.GetElementsByTagName("sign")[0].InnerText;
                    string result_code = doc.GetElementsByTagName("result_code")[0] == null ? "" : doc.GetElementsByTagName("result_code")[0].InnerText;
                    string openid = doc.GetElementsByTagName("openid")[0] == null ? "" : doc.GetElementsByTagName("openid")[0].InnerText;
                    string trade_type = doc.GetElementsByTagName("trade_type")[0] == null ? "" : doc.GetElementsByTagName("trade_type")[0].InnerText;
                    string bank_type = doc.GetElementsByTagName("bank_type")[0] == null ? "" : doc.GetElementsByTagName("bank_type")[0].InnerText;
                    string total_fee = doc.GetElementsByTagName("total_fee")[0] == null ? "" : doc.GetElementsByTagName("total_fee")[0].InnerText;
                    string fee_type = doc.GetElementsByTagName("fee_type")[0] == null ? "" : doc.GetElementsByTagName("fee_type")[0].InnerText;
                    string transaction_id = doc.GetElementsByTagName("transaction_id")[0] == null ? "" : doc.GetElementsByTagName("transaction_id")[0].InnerText;
                    string out_trade_no = doc.GetElementsByTagName("out_trade_no")[0] == null ? "" : doc.GetElementsByTagName("out_trade_no")[0].InnerText;
                    string attach = doc.GetElementsByTagName("attach")[0] == null ? "" : doc.GetElementsByTagName("attach")[0].InnerText;
                    string time_end = doc.GetElementsByTagName("time_end")[0] == null ? "" : doc.GetElementsByTagName("time_end")[0].InnerText;
                    string device_info = doc.GetElementsByTagName("device_info")[0] == null ? "" : doc.GetElementsByTagName("device_info")[0].InnerText;
                    var order = offlineDB.WxPayOrder.SingleOrDefault(m => m.Trade_No == out_trade_no);
                    if (order != null)
                    {
                        // 已成功订单
                        CommonUtilities.writeLog("有订单");
                        if (order.Trade_Status == WeChatUtilities.TRADE_STATUS_SUCCESS)
                        {
                            CommonUtilities.writeLog("已成功");
                            string xmldoc = "<xml><return_code><![CDATA[SUCCESS]]></return_code><return_msg><![CDATA[OK]]></return_msg></xml>";
                            return Content(xmldoc, "text/plain");
                        }
                        // 通知成功
                        if (result_code == "SUCCESS")
                        {
                            //order.Attach = attach;
                            order.Bank_Type = bank_type;
                            order.Fee_Type = fee_type;
                            order.Trade_Status = WeChatUtilities.TRADE_STATUS_SUCCESS;
                            order.Trade_Type = trade_type;
                            //CommonUtilities.writeLog("修改订单");
                            
                            order.Time_Expire = DateTime.ParseExact(time_end, "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture);
                            offlineDB.SaveChanges();
                            //判断是否需要打印小票
                            if (device_info != "")
                            {
                                string printContent = "上海寿全斋电子商务有限公司\r\n" +
                                    "订单编号:" + transaction_id + "\r\n" +
                                    "订单金额:" + (double)Convert.ToInt32(total_fee) / 100 + "元\r\n" +
                                    "付款时间:" + time_end + "\r\n" + "<q>http://www.shouquanzhai.cn/</q>";
                                GPRSPrint.SendGprsPrintContent(printContent);
                            }
                            string xmldoc = "<xml><return_code><![CDATA[SUCCESS]]></return_code><return_msg><![CDATA[OK]]></return_msg></xml>";
                            return Content(xmldoc, "text/plain");
                        }
                        else
                        {
                            string xmldoc = "<xml><return_code><![CDATA[FAIL]]></return_code><return_msg><![CDATA[未知错误]]></return_msg></xml>";
                            return Content(xmldoc, "text/plain");
                        }
                    }
                    CommonUtilities.writeLog("无订单");
                }
                else
                {
                    string err_code = doc.GetElementsByTagName("err_code")[0].InnerText;
                    string err_code_des = doc.GetElementsByTagName("err_code_des")[0].InnerText;
                    string out_trade_no = doc.GetElementsByTagName("out_trade_no")[0].InnerText;
                    var order = offlineDB.WxPayOrder.SingleOrDefault(m => m.Trade_No == out_trade_no);
                    if (order != null)
                    {
                        // 已成功订单
                        if (order.Trade_Status == WeChatUtilities.TRADE_STATUS_SUCCESS)
                        {
                            string xmldoc = "<xml><return_code><![CDATA[SUCCESS]]></return_code><return_msg><![CDATA[OK]]></return_msg></xml>";
                            return Content(xmldoc, "text/plain");
                        }
                        // 不成功订单
                        else
                        {
                            order.Error_Msg = err_code;
                            order.Error_Msg_Des = err_code_des;
                            order.Trade_Status = WeChatUtilities.TRADE_STATUS_CLOSE;
                            offlineDB.SaveChanges();
                            
                            string xmldoc = "<xml><return_code><![CDATA[SUCCESS]]></return_code><return_msg><![CDATA[OK]]></return_msg></xml>";
                            return Content(xmldoc, "text/plain");

                        }
                    }
                }
            }
            catch(Exception e)
            {
                CommonUtilities.writeLog(e.Message + "," + e.StackTrace + "," + e.Source + ","  + e.InnerException);
            }
            string xml = "<xml><return_code><![CDATA[FAIL]]></return_code><return_msg><![CDATA[未知错误]]></return_msg></xml>";
            return Content(xml, "text/plain");
        }
        #endregion

        #region 创建微信统一下单
        /// <summary>
        /// 创建微信统一下单
        /// </summary>
        /// <param name="openid"></param>
        /// <param name="body"></param>
        /// <param name="out_trade_no"></param>
        /// <param name="total_fee"></param>
        /// <param name="trade_type"></param>
        public Wx_OrderResult createUnifiedOrder(string openid, string body, string out_trade_no, int total_fee, string trade_type, string device_no)
        {
            string appid = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            string mch_id = WeChatUtilities.getConfigValue(WeChatUtilities.MCH_ID);
            //先确认，之后做随机数
            string nonce_str = CommonUtilities.generateNonce();
            //string out_trade_no = "WX" + CommonUtilities.generateTimeStamp();
            string spbill_create_ip = WeChatUtilities.getConfigValue(WeChatUtilities.IP);
            string notify_url = "http://webapp.shouquanzhai.cn/Pay/wx_pay_nofity";
            List<QueryParameter> parameters = new List<QueryParameter>();
            parameters.Add(new QueryParameter("appid", appid));
            parameters.Add(new QueryParameter("mch_id", mch_id));
            parameters.Add(new QueryParameter("nonce_str", nonce_str));
            parameters.Add(new QueryParameter("body", body));
            parameters.Add(new QueryParameter("out_trade_no", out_trade_no));
            parameters.Add(new QueryParameter("total_fee", total_fee.ToString()));
            parameters.Add(new QueryParameter("spbill_create_ip", spbill_create_ip));
            parameters.Add(new QueryParameter("notify_url", notify_url));
            parameters.Add(new QueryParameter("trade_type", trade_type));
            parameters.Add(new QueryParameter("openid", openid));
            if (device_no != "")
            {
                parameters.Add(new QueryParameter("device_info", device_no));
            }
            string sign = WeChatUtilities.createPaySign(parameters);
            string xml_content = parseXml(parameters, sign);
            string post_url = "https://api.mch.weixin.qq.com/pay/unifiedorder";
            var request = WebRequest.Create(post_url) as HttpWebRequest;
            try
            {
                
                request.Method = "post";
                StreamWriter streamWriter = new StreamWriter(request.GetRequestStream());
                streamWriter.Write(xml_content);
                streamWriter.Flush();
                streamWriter.Close();
                var response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string result = reader.ReadToEnd();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(result);
                string fromUser = doc.GetElementsByTagName("return_code")[0].InnerText;
                string returnText = doc.GetElementsByTagName("return_msg")[0].InnerText;
                string prepay_id = doc.GetElementsByTagName("prepay_id")[0].InnerText;
                return new Wx_OrderResult("SUCCESS", prepay_id, "OK");

            }
            catch(Exception e)
            {
                return new Wx_OrderResult("FAIL", "", e.ToString());
            }
        }
        #endregion

        #region 创建JSAPI支付
        /// <summary>
        /// 创建JSAPI支付
        /// </summary>
        /// <param name="prepay_id"></param>
        /// <returns></returns>
        /// { result="SUCCESS", appid="", timeStamp="", nonceStr="", package="", paySign="" }
        public JsonResult createPay(string prepay_id)
        {
            try
            {
                List<QueryParameter> JSPayParameters = new List<QueryParameter>();
                string _appid = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
                JSPayParameters.Add(new QueryParameter("appId", _appid));
                string _timeStamp = CommonUtilities.generateTimeStamp().ToString();
                JSPayParameters.Add(new QueryParameter("timeStamp", _timeStamp));
                string _jspayNonce = CommonUtilities.generateNonce();
                JSPayParameters.Add(new QueryParameter("nonceStr", CommonUtilities.generateNonce()));
                string _package = "prepay_id=" + prepay_id;
                JSPayParameters.Add(new QueryParameter("package", _package));
                JSPayParameters.Add(new QueryParameter("signType", "MD5"));
                string JSPayQuery = QueryParameter.NormalizeRequestParameters(JSPayParameters);
                StringBuilder jSenValue = new StringBuilder();
                jSenValue.Append(JSPayQuery);
                jSenValue.Append(string.Format("&key={0}", WeChatUtilities.getConfigValue(WeChatUtilities.PAYAPI_KEY)));
                string jssign = CommonUtilities.encrypt_MD5(jSenValue.ToString()).ToUpper();
                return Json(new { result = "SUCCESS", appid = _appid, timeStamp = _timeStamp, nonceStr = _jspayNonce, package = _package, paySign = jssign }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { result = "FAIL" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 提交刷卡支付
        /// <summary>
        /// 提交刷卡支付
        /// </summary>
        /// <param name="code">条码号</param>
        /// <param name="body">支付信息</param>
        /// <param name="amount">支付金额(分)</param>
        /// <returns></returns>
        public JsonResult createMicroPay(string code, string body, int amount)
        {
            //随机数字，并且生成Prepay
            string appid = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            string mch_id = WeChatUtilities.getConfigValue(WeChatUtilities.MCH_ID);
            //先确认，之后做随机数
            string nonce_str = CommonUtilities.generateNonce();
            string out_trade_no = "WX" + CommonUtilities.generateTimeStamp();
            Random r = new Random();
            int total_fee = amount;
            string spbill_create_ip = Request.ServerVariables.Get("Remote_Addr").ToString();
            //string notify_url = "https://api.mch.weixin.qq.com/pay/micropay";
            //string trade_type = "JSAPI";
            //string openid = _openId;
            List<QueryParameter> parameters = new List<QueryParameter>();
            parameters.Add(new QueryParameter("appid", appid));
            parameters.Add(new QueryParameter("mch_id", mch_id));
            parameters.Add(new QueryParameter("nonce_str", nonce_str));
            parameters.Add(new QueryParameter("body", body));
            parameters.Add(new QueryParameter("out_trade_no", out_trade_no));
            parameters.Add(new QueryParameter("total_fee", total_fee.ToString()));
            parameters.Add(new QueryParameter("spbill_create_ip", spbill_create_ip));
            parameters.Add(new QueryParameter("auth_code", code));
            string query = QueryParameter.NormalizeRequestParameters(parameters);
            StringBuilder enValue = new StringBuilder();
            enValue.Append(query);
            enValue.Append("&key=" + WeChatUtilities.getConfigValue(WeChatUtilities.PAYAPI_KEY));
            string sign = CommonUtilities.encrypt_MD5(enValue.ToString()).ToUpper();
            string content = parseXml(parameters, sign);
            string post_url = "https://api.mch.weixin.qq.com/pay/micropay";
            var request = WebRequest.Create(post_url) as HttpWebRequest;
            try
            {
                request.Method = "post";
                StreamWriter streamWriter = new StreamWriter(request.GetRequestStream());
                streamWriter.Write(content);
                streamWriter.Flush();
                streamWriter.Close();
                var response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string result = reader.ReadToEnd();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(result);
                string return_code = doc.GetElementsByTagName("return_code")[0].InnerText;
                string result_code = doc.GetElementsByTagName("result_code")[0].InnerText;
                string openid = doc.GetElementsByTagName("openid")[0].InnerText;
                //return Json(new { result = "SUCCESS", prepay_id = prepay_id, total_fee = total_fee }, JsonRequestBehavior.AllowGet);
                return Json(new { result = "SUCCESS" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { result = "FAIL" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 微信扫码回调通知页面，通过productID，调用微信统一下单API(createUnifiedOrder).数据库生成订单信息，同时返回给微信prepay_id
        /// <summary>
        /// 微信扫码回调通知页面，通过productID，调用微信统一下单API(createUnifiedOrder).数据库生成订单信息，同时返回给微信prepay_id
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ContentResult wx_native_notify()
        {
            StreamReader reader = new StreamReader(Request.InputStream, Encoding.UTF8);
            XmlDocument doc = new XmlDocument();
            doc.Load(reader);
            string appid = doc.GetElementsByTagName("appid")[0].InnerText;
            string openid = doc.GetElementsByTagName("openid")[0].InnerText;
            string mch_id = doc.GetElementsByTagName("mch_id")[0].InnerText;
            string is_subscribe = doc.GetElementsByTagName("is_subscribe")[0].InnerText;
            string nonce_str = doc.GetElementsByTagName("nonce_str")[0].InnerText;
            string product_id = doc.GetElementsByTagName("product_id")[0].InnerText;
            string request_sign = doc.GetElementsByTagName("sign")[0].InnerText;
            string trade_no = "WXNATIVE_" + DateTime.Now.Ticks;
            int total_fee = 10;
            string _body = "未知商品";
            string _detail = "未知商品";
            try
            {
                int _product_id = Convert.ToInt32(product_id);
                WxPayProduct product = offlineDB.WxPayProduct.SingleOrDefault(m => m.Id == _product_id);
                if (product != null)
                {
                    _body = product.ProductName;
                    _detail = product.ProductDetails;
                    total_fee = Convert.ToInt32(product.Total_Fee * 100);
                }
            }
            catch(Exception e)
            {
                CommonUtilities.writeLog(e.ToString());
                total_fee = 10;
                _body = "未知商品";
                _detail = "未知商品";
            }

            Wx_OrderResult result = createUnifiedOrder(openid, _body, trade_no, total_fee, WeChatUtilities.TRADE_TYPE_NATIVE, "");
            if(result.Result == "SUCCESS")
            {
                WxPayOrder order = new WxPayOrder()
                {
                    Body = _body,
                    Time_Start = DateTime.Now,
                    Mch_Id = mch_id,
                    Open_Id = openid,
                    Detail = _detail,
                    Trade_No = trade_no,
                    Product_Id = product_id,
                    Prepay_Id = result.PrepayId,
                    Total_Fee = total_fee,
                    Trade_Status = WeChatUtilities.TRADE_STATUS_CREATE,
                    Trade_Type = WeChatUtilities.TRADE_TYPE_NATIVE
                };
                offlineDB.WxPayOrder.Add(order);
                offlineDB.SaveChanges();
                List<QueryParameter> parameters = new List<QueryParameter>();
                parameters.Add(new QueryParameter("return_code", result.Result));
                parameters.Add(new QueryParameter("return_msg", result.Message));
                parameters.Add(new QueryParameter("appid", appid));
                parameters.Add(new QueryParameter("mch_id", mch_id));
                parameters.Add(new QueryParameter("nonce_str", CommonUtilities.generateNonce()));
                parameters.Add(new QueryParameter("prepay_id", result.PrepayId));
                parameters.Add(new QueryParameter("result_code", result.Result));
                parameters.Add(new QueryParameter("err_code_des", result.Message));
                string response_sign = WeChatUtilities.createPaySign(parameters);
                string xmlcontent = parseXml(parameters, response_sign);
                return Content(xmlcontent, "text/plain");
            }
            else
            {
                List<QueryParameter> parameters = new List<QueryParameter>();
                parameters.Add(new QueryParameter("return_code", result.Result));
                parameters.Add(new QueryParameter("return_msg", result.Message));
                string response_sign = WeChatUtilities.createPaySign(parameters);
                string xmlcontent = parseXml(parameters, response_sign);
                return Content(xmlcontent, "text/plain");
            }
            
        }
        #endregion

        #region 生成扫码链接，参数为ProductId
        /// <summary>
        /// 生成扫码链接 
        /// </summary>
        /// <param name="product_id"></param>
        /// <returns></returns>
        public ContentResult create_native_pay_url(string product_id)
        {
            string appid = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            string mch_id = WeChatUtilities.getConfigValue(WeChatUtilities.MCH_ID);
            string nonce = CommonUtilities.generateNonce();
            string time_stamp = Convert.ToString(CommonUtilities.generateTimeStamp());
            List<QueryParameter> parameters = new List<QueryParameter>();
            parameters.Add(new QueryParameter("appid", appid));
            parameters.Add(new QueryParameter("mch_id", mch_id));
            parameters.Add(new QueryParameter("time_stamp", time_stamp));
            parameters.Add(new QueryParameter("nonce_str", nonce));
            parameters.Add(new QueryParameter("product_id", product_id));
            string sign = WeChatUtilities.createPaySign(parameters);
            string url = "weixin://wxpay/bizpayurl?" + QueryParameter.NormalizeRequestParameters(parameters) + "&sign=" + sign;
            return Content(url);
        }
        #endregion

        public ActionResult Wechat_Redirect(string url, string state)
        {
            string redirectUri = Url.Encode(url);
            string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            string redirect_url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_base&state=" + state + "#wechat_redirect";

            return Redirect(redirect_url);
        }

        #region 扫码支付流程
        public ActionResult CustomMicroPay()
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
        #endregion


        #region 摇一摇随机金额
        public ActionResult Gambling_Pay(string code, string state)
        {
            WeChatUtilities utilites = new WeChatUtilities();
            Wx_WebOauthAccessToken webToken = utilites.getWebOauthAccessToken(code);
            ViewBag.openId = webToken.openid;
            WxPayStatistic s = new WxPayStatistic()
            {
                EventName = "大学城手机摇一摇",
                ApplicationDate = DateTime.Now,
                Details = state
            };
            offlineDB.WxPayStatistic.Add(s);
            offlineDB.SaveChanges();
            return View();
        }
        public ActionResult Gambling_Success(string prepay_id)
        {
            var order_item = offlineDB.WxPayOrder.SingleOrDefault(m => m.Prepay_Id == prepay_id);
            if (order_item != null)
            {
                return View(order_item);
            }
            return View("Gambling_Failed");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult setRandomMoney(string _openId, string body)
        {
            //随机数字，并且生成Prepay
            string appid = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            string mch_id = WeChatUtilities.getConfigValue(WeChatUtilities.MCH_ID);
            //先确认，之后做随机数
            string nonce_str = CommonUtilities.generateNonce();
            string out_trade_no = "WXJSAPI_" + DateTime.Now.Ticks;
            Random r = new Random();
            int total_fee = 1580;
            int baseRandom = r.Next(0, 99);
            if (baseRandom < 3)
            {
                total_fee = r.Next(40, 158)*10;
            }
            else
                total_fee = r.Next(108, 158) * 10;
            try
            {
                Wx_OrderResult result = createUnifiedOrder(_openId, body, out_trade_no, total_fee, WeChatUtilities.TRADE_TYPE_JSAPI,"");
                if(result.Result == "SUCCESS")
                {
                    WxPayOrder order = new WxPayOrder()
                    {
                        Body = body,
                        Time_Start = DateTime.Now,
                        Mch_Id = mch_id,
                        Open_Id = _openId,
                        Trade_No = out_trade_no,
                        Total_Fee = total_fee,
                        Prepay_Id = result.PrepayId,
                        Trade_Status = WeChatUtilities.TRADE_STATUS_CREATE,
                        Trade_Type = WeChatUtilities.TRADE_TYPE_JSAPI
                    };
                    offlineDB.WxPayOrder.Add(order);
                    offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS", prepay_id = result.PrepayId, total_fee }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { result = "FAIL", msg = result.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { result = "FAIL", msg = e.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        public ActionResult Error()
        {
            return View();
        }
        public ActionResult StartPayAmount(string code, string state)
        {
            try
            {
                WeChatUtilities utilites = new WeChatUtilities();
                Wx_WebOauthAccessToken webToken = utilites.getWebOauthAccessToken(code);
                int amount = 1980;
                ViewBag.openId = webToken.openid;
                if (state == "AC69IPVD")
                    amount = 100;
                else if (state == "BGAER9CB")
                    amount = 300;
                ViewBag.amount = amount;
                return View();
            }
            catch
            {
                return View("Error");
            }
        }
        [HttpPost,ValidateAntiForgeryToken]
        public JsonResult SetFixMoney(string _openId, string body, int amount)
        {
            //随机数字，并且生成Prepay
            string appid = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            string mch_id = WeChatUtilities.getConfigValue(WeChatUtilities.MCH_ID);
            //先确认，之后做随机数
            string nonce_str = CommonUtilities.generateNonce();
            string out_trade_no = "WXJSAPI_" + DateTime.Now.Ticks;
            try
            {
                Wx_OrderResult result = createUnifiedOrder(_openId, body, out_trade_no, amount, WeChatUtilities.TRADE_TYPE_JSAPI, "");
                if (result.Result == "SUCCESS")
                {
                    WxPayOrder order = new WxPayOrder()
                    {
                        Body = body,
                        Time_Start = DateTime.Now,
                        Mch_Id = mch_id,
                        Open_Id = _openId,
                        Trade_No = out_trade_no,
                        Total_Fee = amount,
                        Prepay_Id = result.PrepayId,
                        Trade_Status = WeChatUtilities.TRADE_STATUS_CREATE,
                        Trade_Type = WeChatUtilities.TRADE_TYPE_JSAPI
                    };
                    offlineDB.WxPayOrder.Add(order);
                    offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS", prepay_id = result.PrepayId, amount }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { result = "FAIL", msg = result.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { result = "FAIL", msg = e.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        public string parseXml(List<QueryParameter> parameters, string sign)
        {
            var list = parameters.OrderBy(m => m.Name).ToList();
            string xml = "<xml>";
            foreach (var item in list)
            {
                if (item.Value.GetType() == typeof(int))
                {
                    xml += "<" + item.Name + ">" + item.Value + "</" + item.Name + ">";
                }
                else if (item.Value.GetType() == typeof(string))
                {
                    xml += "<" + item.Name + ">" + "<![CDATA[" + item.Value + "]]></" + item.Name + ">";
                }
            }
            xml += "<sign>" + sign + "</sign>";
            xml += "</xml>";
            return xml;
        }

        public async Task<ActionResult> PressConferenceMain(string code, string state)
        {
            try
            {
                WeChatUtilities utilites = new WeChatUtilities();
                Wx_WebOauthAccessToken jat = utilites.getWebOauthAccessToken(code);
                var user = UserManager.FindByEmail(jat.openid);
                if (user != null)
                {
                    //var user = UserManager.FindByName("13636314852");
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    return RedirectToAction("PressConferenceHome");
                }
                //return Content(jat.openid + "," + jat.access_token);
                return RedirectToAction("PressConferenceRegister", new { open_id = jat.openid, accessToken = jat.access_token });
            }
            catch
            {
                return View("Error");
            }
        }
        public ActionResult PressConferenceRegister(string open_id, string accessToken)
        {
            var model = new Wx_OffRegisterViewModel();
            model.Open_Id = open_id;
            model.AccessToken = accessToken;

            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> PressConferenceRegister(Wx_OffRegisterViewModel model)
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
                    var result = await UserManager.CreateAsync(user, model.Open_Id);
                    if (result.Succeeded)
                    {
                        smsRecord.Status = true;
                        smsDB.SaveChanges();

                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToAction("PressConferenceHome");
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

        [Authorize]
        public ActionResult PressConferenceHome()
        {
            return View();
        }
        [Authorize, HttpPost, ValidateAntiForgeryToken]
        public JsonResult PayMoney()
        {
            //随机数字，并且生成Prepay
            string appid = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            string mch_id = WeChatUtilities.getConfigValue(WeChatUtilities.MCH_ID);
            //先确认，之后做随机数
            string nonce_str = CommonUtilities.generateNonce();
            string out_trade_no = "WXJSAPI_" + DateTime.Now.Ticks;
            var user = UserManager.FindById(User.Identity.GetUserId());
            try
            {
                Wx_OrderResult result = createUnifiedOrder(user.OpenId, "测试", out_trade_no, 10, WeChatUtilities.TRADE_TYPE_JSAPI, "");
                if (result.Result == "SUCCESS")
                {
                    PC_Order order = new PC_Order()
                    {
                        ApplicationTime = DateTime.Now,
                        OrderAmount = 100000,
                        PurchaseAmount = 10,
                        Status = 0,
                        UserName = user.UserName
                    };
                    pcdb.PC_Order.Add(order);
                    pcdb.SaveChanges();
                    return Json(new { result = "SUCCESS", prepay_id = result.PrepayId });
                }
                return Json(new { result = "FAIL", msg = result.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { result = "FAIL", msg = e.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}