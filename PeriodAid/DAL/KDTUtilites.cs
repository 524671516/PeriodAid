using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace PeriodAid.DAL
{
    
    public class KDTUtilites
    {
        private static string appid = "f9e38e927742dcc15a";
        private static string appsecret = "af47f8cbabb2295bb6e16d2c38d341ff";
        public string KDT_GetRegion(int level, int? parentid)
        {
            List<QueryParameter> parameter = new List<QueryParameter>();
            parameter.Add(new QueryParameter("method", "kdt.regions.get"));
            parameter.Add(new QueryParameter("timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            parameter.Add(new QueryParameter("v", "1.0"));
            parameter.Add(new QueryParameter("format", "json"));
            parameter.Add(new QueryParameter("app_id", appid));
            if(parentid!=null)
                parameter.Add(new QueryParameter("parent_id", parentid.ToString()));
            parameter.Add(new QueryParameter("level", level.ToString()));
            parameter.Add(new QueryParameter("sign_method", "md5"));
            string sign = SignMD5(QueryParameter.NormalizeKDTParameters(parameter));
            string url = "https://open.koudaitong.com/api/entry?sign=" + sign + "&" + QueryParameter.NormalizeRequestParameters(parameter);
            var request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            WebResponse response = request.GetResponse();
            string result = "";
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }
        public string KDT_GetTrade(DateTime st, DateTime et, string keyword)
        {
            List<QueryParameter> parameter = new List<QueryParameter>();
            // 系统级参数
            parameter.Add(new QueryParameter("method", "kdt.trades.sold.get"));
            parameter.Add(new QueryParameter("timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            parameter.Add(new QueryParameter("v", "1.0"));
            parameter.Add(new QueryParameter("format", "json"));
            parameter.Add(new QueryParameter("app_id", appid));
            parameter.Add(new QueryParameter("sign_method", "md5"));
            // 应用及参数
            //parameter.Add(new QueryParameter("keyword", keyword));
            parameter.Add(new QueryParameter("status", "TRADE_BUYER_SIGNED"));
            parameter.Add(new QueryParameter("fields", "status,receiver_mobile"));
            parameter.Add(new QueryParameter("start_created", st.ToString("yyyy-MM-dd")));
            parameter.Add(new QueryParameter("end_created", et.ToString("yyyy-MM-dd")));
            parameter.Add(new QueryParameter("page_size", "50000"));
            //parameter.Add(new QueryParameter("buyer_nick", ToUnicodeString("许亦安")));
            string sign = SignMD5(QueryParameter.NormalizeKDTParameters(parameter));
            string url = "https://open.koudaitong.com/api/entry?sign=" + sign + "&" + QueryParameter.NormalizeRequestParameters(parameter);
            var request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            WebResponse response = request.GetResponse();
            string result = "";
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }
        /*private string ToUnicodeString(this string str)
        {
            StringBuilder strResult = new StringBuilder();
            if (!string.IsNullOrEmpty(str))
            {
                for (int i = 0; i < str.Length; i++)
                {
                    strResult.Append("\\u");
                    strResult.Append(((int)str[i]).ToString("x"));
                }
            }
            return strResult.ToString();
        }*/
        private string SignMD5(string content)
        {
            StringBuilder enValue = new StringBuilder();
            //前后加上secret
            enValue.Append(appsecret);
            enValue.Append(content);
            enValue.Append(appsecret);
            //使用MD5加密(32位小写)
            return CommonUtilities.encrypt_MD5(enValue.ToString()).ToLower();
        }
    }
}