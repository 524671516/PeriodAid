using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PeriodAid.Models;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Threading.Tasks;

namespace PeriodAid.DAL
{
    public class AppPayUtilites
    {
        private AppPay PaymentDb;
        private WeChatUtilities WeUtil;
        private const string notify_url = "http://webapp.shouquanzhai.cn/Pay/wx_pay_nofity";
        public AppPayUtilites()
        {
            PaymentDb = new AppPay();
            WeUtil = new WeChatUtilities();
        }

        public string WxRedPackCreate(string openid)
        {
            return "";
        } 

        public async Task<string> WxRedPackQuery(string orderId)
        {
            var order = PaymentDb.WxRedPackOrder.SingleOrDefault(m => m.mch_billno == orderId);
            string query_url = "https://api.mch.weixin.qq.com/mmpaymkttransfers/gethbinfo";
            List<QueryParameter> parameters = new List<QueryParameter>();
            parameters.Add(new QueryParameter("mch_billno", order.mch_billno));
            parameters.Add(new QueryParameter("nonce_str", CommonUtilities.generateNonce()));
            parameters.Add(new QueryParameter("mch_id", WeUtil.getMchId()));
            parameters.Add(new QueryParameter("bill_type", "MCHT"));
            parameters.Add(new QueryParameter("appid", WeUtil.getAppId()));
            string sign = WeChatUtilities.createPaySign(parameters);
            string request_xml = parseXml(parameters, sign);
            var request = WebRequest.Create(query_url) as HttpWebRequest;
            string certpath = @"D:\apiclient_cert.p12";
            string password = "1224974002";
            X509Certificate2 cert = new X509Certificate2(certpath, password);
            request.ClientCertificates.Add(cert);
            request.Method = "post";

            StreamWriter streamWriter = new StreamWriter(request.GetRequestStream());
            streamWriter.Write(request_xml);
            streamWriter.Flush();
            streamWriter.Close();
            var response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            XmlSerializer xmldes = new XmlSerializer(typeof(WxHBInfo_List));
            WxHBInfo_List info = xmldes.Deserialize(reader) as WxHBInfo_List;
            // 判断是否读取成功
            if (info.return_code == "SUCCESS")
            {
                if (info.result_code == "SUCCESS")
                {
                    // 状态一致，返回当前状态
                    if (order.status == info.status)
                    {
                        return order.status;
                    }
                    else
                    {
                        order.status = info.status;
                        order.reason = info.reason;
                        if (info.refund_time == null)
                            order.refund_time = null;
                        else
                            order.refund_time = Convert.ToDateTime(info.refund_time);
                        order.refund_amount = info.refund_amount;
                        order.detail_id = info.detail_id;
                        if (info.send_time == null)
                            order.send_time = null;
                        else
                            order.send_time = Convert.ToDateTime(info.send_time);
                        order.total_amount = info.total_amount;
                        order.total_num = info.total_num;
                        var hbinfo = info.hblist.FirstOrDefault();
                        if (hbinfo != null)
                        {
                            if (hbinfo.rcv_time == null)
                                order.rcv_time = null;
                            else
                                order.rcv_time = Convert.ToDateTime(hbinfo.rcv_time);
                        }
                        PaymentDb.Entry(order).State = System.Data.Entity.EntityState.Modified;
                        await PaymentDb.SaveChangesAsync();
                        return order.status;
                    }
                }
                else
                {
                    return info.err_code_des;
                }
            }
            return info.return_msg;

        }
        private string parseXml(List<QueryParameter> parameters, string sign)
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
    }
    [XmlRoot("xml")]
    public class WxHBInfo_List
    {
        public string return_code { get; set; }
        public string return_msg { get; set; }
        public string sign { get; set; }
        public string result_code { get; set; }
        public string err_code { get; set; }
        public string err_code_des { get; set; }
        public string mch_billno { get; set; }
        public string mch_id { get; set; }
        public string detail_id { get; set; }
        public string status { get; set; }
        public string send_type { get; set; }
        public int total_num { get; set; }
        public int total_amount { get; set; }
        public string reason { get; set; }
        public string send_time { get; set; }
        public string refund_time { get; set; }
        public int refund_amount { get; set; }
        public string wishing { get; set; }
        public string remark { get; set; }
        public string act_name { get; set; }
        [XmlElement("hblist")]
        public List<WxHBInfo> hblist { get; set; }
    }
    [XmlRoot("hbinfo")]
    public class WxHBInfo
    {
        public string openid { get; set; }
        public string status { get; set; }
        public int amount { get; set; }
        public string rcv_time { get; set; }
    }
}