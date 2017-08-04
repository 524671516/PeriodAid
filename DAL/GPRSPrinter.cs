using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;

namespace PeriodAid.DAL
{
    /// <summary>
    /// 易联云无线打印机接口类
    /// </summary>
    public class GPRSPrint
    {
        
              
        //private static int LineWordCount = 16; //小票每行最多打印汉字数
        //private static Dictionary<string, string> parameters = new Dictionary<string, string>();//加密参数列表

        /// <summary>
        /// 发送打印内容
        /// </summary>       
        /// <param name="Content"></param>
        /// <returns></returns>
        public static string SendGprsPrintContent(string Content)
        {
            string partner = "2713";                                          //用户id
            string machine_code = "4004503380";                             //终端号
            string mkey = "ywqfp4w6bp6t";                                    //终端密钥
            string apikey = "c3219c32691ff4bba24894efe8cc1eca45ae95a4";       //API 密钥
            List<QueryParameter> parameters = new List<QueryParameter>();
            parameters.Add(new QueryParameter("partner", partner));
            parameters.Add(new QueryParameter("machine_code", machine_code));
            string timestamp = CommonUtilities.generateTimeStamp().ToString();
            parameters.Add(new QueryParameter("time", timestamp));
            parameters.Add(new QueryParameter("content", Content));
            string source = apikey + "machine_code" + machine_code + "partner" + partner + "time" + timestamp + mkey;
            string sign = CommonUtilities.encrypt_MD5(source).ToUpper();
            parameters.Add(new QueryParameter("sign", sign));

            string post_url = "http://open.10ss.net:8888";
            var request = WebRequest.Create(post_url) as HttpWebRequest;
            request.Method = "POST";
            string postdata = QueryParameter.NormalizeRequestParameters(parameters);
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
        public static string TemplateCode(string code)
        {
            string printContent = "上海寿全斋电子商务有限公司\r\n\r\n" +
                                "验证码:" + code + "\r\n\r\n" +
                                "打印时间:" + DateTime.Now + "\r\n";
            return printContent;
        }
    }
}