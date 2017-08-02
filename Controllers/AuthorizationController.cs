using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PeriodAid.DAL;
using System.Threading.Tasks;

namespace PeriodAid.Controllers
{
    public class AuthorizationController : Controller
    {
        // GET: Authorization
        public ActionResult Index()
        {
            return View();
        }
        // 普通认证
        [AllowAnonymous]
        public ActionResult LoginManager(string state)
        {
            string user_Agent = HttpContext.Request.UserAgent;
            if (state == "")
            {
                state = "undefined";
            }
            if (user_Agent.Contains("MicroMessenger"))
            {
                //return Content("微信");
                string redirectUri = Url.Encode("http://webapp.shouquanzhai.cn/Authorization/Authorization");
                string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
                string redirect_url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_base&state=" + state + "#wechat_redirect";
                return Redirect(redirect_url);
            }
            else
            {
                return Content("FAIL");
            }
        }

        public ActionResult UpdateUserInfo(string url, string state)
        {
            string user_Agent = HttpContext.Request.UserAgent;
            if (state == "")
            {
                state = "undefined";
            }
            if (user_Agent.Contains("MicroMessenger"))
            {
                string redirectUri = Url.Encode(url);
                string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
                string redirect_url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appId + "&redirect_uri=" + redirectUri + "&response_type=code&scope=snsapi_userinfo&state=" + state + "#wechat_redirect";
                return Redirect(redirect_url);
            }
            else
            {
                return Content("FAIL");
            }
        }

        [AllowAnonymous]
        public ActionResult Authorization(string code, string state)
        {
            //return Content(code);
            //string appId = WeChatUtilities.getConfigValue(WeChatUtilities.APP_ID);
            try
            {

                WeChatUtilities wechat = new WeChatUtilities();
                var jat = wechat.getWebOauthAccessToken(code);
                return Redirect("http://www.shouquanzhai.cn/Pay/Authorization?code=" + jat.openid + "&state=" + state);
            }
            catch (Exception ex)
            {
                CommonUtilities.writeLog(ex.Message);
                return View("Error");
            }
        }
    }
}