using PeriodAid.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;


namespace PeriodAid.DAL
{
    public class WeChatUtilities
    {
        public Configuration configDb;
        public static string APP_ID = "appId";
        private static string APP_SECRET = "appSecret";
        public static string ACCESS_TOKEN = "accessToken";
        public static string ACCESS_TOKTEN_TIMESTAMP = "accessTokenTimestamp";
        public static string JSAPI_TICKET = "jsApiTicket";
        public static string JSAPI_TICKET_TIMESTAMP = "jsApiTicketTimestamp";
        public static string TOKEN = "token";
        public static string MCH_ID = "mchId";
        public static string PAYAPI_KEY = "payApiKey";
        public static string IP = "ip";
        public static string TRADE_TYPE_JSAPI = "JSAPI";
        public static string TRADE_TYPE_NATIVE = "NATIVE";
        public static string TRADE_YPPE_APP = "APP";
        public static int TRADE_STATUS_CREATE = 0;
        public static int TRADE_STATUS_SUCCESS = 1;
        public static int TRADE_STATUS_CLOSE = 2;

        public WeChatUtilities()
        {
            configDb = new Configuration();
        }


        #region 获取AppId
        /// <summary>
        /// 获取AppId的值
        /// </summary>
        /// <returns>AppId的值</returns>
        public string getAppId()
        {
            return getWeChatConfigValue(APP_ID);
        }
        #endregion


        #region 获取微信商户号
        /// <summary>
        /// 获取微信商户号
        /// </summary>
        /// <returns>微信商户号</returns>
        public string getMchId(){
            return getWeChatConfigValue(MCH_ID);
        }
        #endregion
        

        #region 获取放在服务器的TOKEN值
        /// <summary>
        /// 获取放在服务器的TOKEN值
        /// </summary>
        /// <returns>TOKEN值</returns>
        public string getToken()
        {
            return getWeChatConfigValue(TOKEN);
        }
        #endregion


        #region 获取AccessToken值并判断是否需要重新获取
        /// <summary>
        /// 获取AccessToken值并判断是否需要重新获取
        /// </summary>
        /// <returns>返回AccessToken的值</returns>
        public string getAccessToken()
        {
            string accessToken = getWeChatConfigValue(ACCESS_TOKEN);
            string accessTokenExpires = getWeChatConfigValue(ACCESS_TOKTEN_TIMESTAMP);
            if (accessToken == "" || accessTokenExpires == "")
            {
                generateAccessToken();
                return getWeChatConfigValue(ACCESS_TOKEN);
            }
            else if (CommonUtilities.generateTimeStamp() + 200 > Convert.ToInt64(accessTokenExpires))
            {
                generateAccessToken();
                return getWeChatConfigValue(ACCESS_TOKEN);
            }
            else
            {
                return accessToken;
            }
        }
        #endregion


        #region 获取JsApiTicket值并判断是否需要重新获取
        /// <summary>
        /// 获取JsApiTicket值并判断是否需要重新获取
        /// </summary>
        /// <returns>返回JsApiTicket的值</returns>
        public string getJsApiTicket()
        {
            string jsApiTicket = getWeChatConfigValue(JSAPI_TICKET);
            string jsApiTicketExpires = getWeChatConfigValue(JSAPI_TICKET_TIMESTAMP);
            // 判断是否为空值
            if (jsApiTicket == "" || jsApiTicketExpires == "")
            {
                generateJsApiTicket();
                return getWeChatConfigValue(JSAPI_TICKET);
            }
            else if (CommonUtilities.generateTimeStamp() + 200 > Convert.ToInt64(jsApiTicketExpires))
            {
                // 判断是否超时（预留200秒）
                generateJsApiTicket();
                return getWeChatConfigValue(JSAPI_TICKET);
            }
            else
            {
                return jsApiTicket;
            }
        }
        #endregion


        #region 重新调用微信AccessToken接口,并保存到数据库
        /// <summary>
        /// 调用微信AccessToken接口,并保存当前AccessToken到数据库
        /// </summary>
        private void generateAccessToken()
        {
            string appId = getWeChatConfigValue(APP_ID);
            string appSecret = getWeChatConfigValue(APP_SECRET);
            string urlstring = "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + appId + "&secret=" + appSecret;
            Uri url = new Uri(urlstring);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string jsonresult = new StreamReader(response.GetResponseStream()).ReadToEnd();
            Wx_AccessToken token = serializer.Deserialize<Wx_AccessToken>(jsonresult);
            setWeChatConfigValue(ACCESS_TOKEN, token.access_token);
            long accesstokenexpires = CommonUtilities.generateTimeStamp() + token.expires_in;
            setWeChatConfigValue(ACCESS_TOKTEN_TIMESTAMP, accesstokenexpires.ToString());
        }
        #endregion


        #region 调用微信JsApiTickets接口，并保存到数据库
        /// <summary>
        /// 调用微信JsApiTickets接口，并保存到数据库
        /// </summary>
        private void generateJsApiTicket()
        {
            string access_token = getAccessToken();
            string urlstring = "https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token=" + access_token + "&type=jsapi";
            Uri url = new Uri(urlstring);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string jsonresult = new StreamReader(response.GetResponseStream()).ReadToEnd();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Wx_JsApiTicket jat = serializer.Deserialize<Wx_JsApiTicket>(jsonresult);
            setWeChatConfigValue(JSAPI_TICKET, jat.ticket);
            long jsApiTicketexpires = CommonUtilities.generateTimeStamp() + jat.expires_in;
            setWeChatConfigValue(JSAPI_TICKET_TIMESTAMP, jsApiTicketexpires.ToString());
        }
        #endregion


        #region 调用微信网页授权access_token接口，并保存到数据库(未完成)
        /// <summary>
        /// 调用微信网页授权access_token接口，并保存到数据库
        /// </summary>
        public Wx_WebOauthAccessToken getWebOauthAccessToken(string code)
        {
            string appId = getWeChatConfigValue(APP_ID);
            string appSecret = getWeChatConfigValue(APP_SECRET);
            string urlstring = "https://api.weixin.qq.com/sns/oauth2/access_token?appid=" + appId + "&secret=" + appSecret + "&code=" + code + "&grant_type=authorization_code";
            Uri url = new Uri(urlstring);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string jsonresult = new StreamReader(response.GetResponseStream()).ReadToEnd();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Wx_WebOauthAccessToken jat = serializer.Deserialize<Wx_WebOauthAccessToken>(jsonresult);
            return jat;
        }
        #endregion

        #region 调用微信用户信息
        /// <summary>
        /// 调用微信用户信息
        /// </summary>
        public Wx_WebUserInfo getWebOauthUserInfo(string access_token, string openid)
        {
            string urlstring = "https://api.weixin.qq.com/sns/userinfo?access_token=" + access_token + "&openid=" + openid + "&lang=zh_CN";
            Uri url = new Uri(urlstring);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string jsonresult = new StreamReader(response.GetResponseStream()).ReadToEnd();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Wx_WebUserInfo user = serializer.Deserialize<Wx_WebUserInfo>(jsonresult);
            return user;
        }
        #endregion


        #region 生成针对微信JS接口的签名(SHA1)
        /// <summary>
        /// 生成针对微信JS接口的签名(SHA1)
        /// </summary>
        /// <param name="noncestr">随机字符串</param>
        /// <param name="jsapi_ticket">调用微信JS接口的临时票据</param>
        /// <param name="timestamp">请求的时间戳</param>
        /// <param name="url">当前网页URL</param>
        /// <returns>生成的SHA1签名</returns>
        public string generateWxJsApiSignature(string noncestr, string jsapi_ticket, string timestamp, string url)
        {
            List<QueryParameter> parameters = new List<QueryParameter>();
            parameters.Add(new QueryParameter("jsapi_ticket", jsapi_ticket));
            parameters.Add(new QueryParameter("noncestr", noncestr));
            parameters.Add(new QueryParameter("timestamp", timestamp));
            parameters.Add(new QueryParameter("url", url));
            string query = QueryParameter.NormalizeRequestParameters(parameters);
            return CommonUtilities.encrypt_SHA1(query);
        }
        #endregion



        #region WeChatConfig通用GET & SET
        /// <summary>
        /// 获取对应数据名称的值
        /// </summary>
        /// <param name="key">数据名称</param>
        /// <returns>对应数据名称的值</returns>
        private string getWeChatConfigValue(string key)
        {
            var item = configDb.WeChatConfigs.SingleOrDefault(m => m.Key == key);
            if (item != null)
                return item.Value;
            else
                return null;
        }


        /// <summary>
        /// 更新对应数据名称的值
        /// </summary>
        /// <param name="key">数据名称</param>
        /// <param name="value">对应数据名称的值</param>
        private void setWeChatConfigValue(string key, string value)
        {
            var item = configDb.WeChatConfigs.SingleOrDefault(m => m.Key == key);
            if (item != null)
            {
                item.Value = value;
                item.LastModify = DateTime.Now;
            }
            configDb.SaveChanges();
        }
        #endregion


        public static string getConfigValue(string key)
        {
            Configuration configContext = new Configuration();
            var item = configContext.WeChatConfigs.SingleOrDefault(m => m.Key == key);
            if (item != null)
                return item.Value;
            else
                return null;
        }

        public static string createPaySign(List<QueryParameter> parameters)
        {
            string query = QueryParameter.NormalizeRequestParameters(parameters);
            StringBuilder enValue = new StringBuilder();
            enValue.Append(query);
            enValue.Append("&key=" + WeChatUtilities.getConfigValue(WeChatUtilities.PAYAPI_KEY));
            string sign = CommonUtilities.encrypt_MD5(enValue.ToString()).ToUpper();
            return sign;
        }
    }
}