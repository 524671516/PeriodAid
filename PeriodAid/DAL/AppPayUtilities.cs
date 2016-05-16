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
using System.Web.Mvc;

namespace PeriodAid.DAL
{
    public class AppPayUtilities
    {
        private AppPay PaymentDb;
        private WeChatUtilities WeUtil;
        private const string notify_url = "http://webapp.shouquanzhai.cn/Pay/wx_pay_nofity";
        public AppPayUtilities()
        {
            PaymentDb = new AppPay();
            WeUtil = new WeChatUtilities();
        }
        public string WxRedPackCreate(string openid, int amount)
        {
            Random random = new Random();
            string mch_billno = "WXREDPACK" + CommonUtilities.generateTimeStamp() + random.Next(1000, 9999);
            var result = WxRedPackCreate(openid, amount, mch_billno, "红包发放", "寿全斋", "红包发放", "恭喜获得红包");
            return result;
        }
        public string WxRedPackCreate(string openid, int amount, string mch_billno, string act_name, string send_name, string remark, string wishing)
        {
            //Wx_WebOauthAccessToken webToken = utilities.getWebOauthAccessToken(code);
            string nonce_str = CommonUtilities.generateNonce();
            //string mch_billno = "WXREDPACK" + CommonUtilities.generateTimeStamp() + random.Next(1000, 9999);
            string mch_id = WeUtil.getMchId();
            string wxappid = WeUtil.getAppId();
            string re_openid = openid;
            int total_amount = amount;
            int total_num = 1;
            string client_ip = WeChatUtilities.getConfigValue(WeChatUtilities.IP);
            List<QueryParameter> redpackParameter = new List<QueryParameter>();
            redpackParameter.Add(new QueryParameter("nonce_str", nonce_str));
            redpackParameter.Add(new QueryParameter("mch_billno", mch_billno));
            redpackParameter.Add(new QueryParameter("mch_id", mch_id));
            redpackParameter.Add(new QueryParameter("wxappid", wxappid));
            redpackParameter.Add(new QueryParameter("send_name", send_name));
            redpackParameter.Add(new QueryParameter("re_openid", re_openid));
            redpackParameter.Add(new QueryParameter("total_amount", total_amount.ToString()));
            redpackParameter.Add(new QueryParameter("total_num", total_num.ToString()));
            redpackParameter.Add(new QueryParameter("wishing", wishing));
            redpackParameter.Add(new QueryParameter("client_ip", client_ip));
            redpackParameter.Add(new QueryParameter("act_name", act_name));
            redpackParameter.Add(new QueryParameter("remark", remark));
            string sign = WeChatUtilities.createPaySign(redpackParameter);
            string content = parseXml(redpackParameter, sign);
            string post_url = "https://api.mch.weixin.qq.com/mmpaymkttransfers/sendredpack";
            var request = WebRequest.Create(post_url) as HttpWebRequest;
            string certpath = @"D:\apiclient_cert.p12";
            string password = "1224974002";
            X509Certificate2 cert = new X509Certificate2(certpath, password);
            request.ClientCertificates.Add(cert);
            try
            {
                request.Method = "post";
                StreamWriter streamWriter = new StreamWriter(request.GetRequestStream());
                streamWriter.Write(content);
                streamWriter.Flush();
                streamWriter.Close();
                var response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                //return reader.ReadToEnd();
                XmlSerializer xmldes = new XmlSerializer(typeof(WxHBSend_Result));
                WxHBSend_Result info = xmldes.Deserialize(reader) as WxHBSend_Result;
                // 是否成功
                string err_code_des = "";
                if (info.return_code == "SUCCESS")
                {
                    if (info.result_code == "SUCCESS")
                    {
                        WxRedPackOrder item = new WxRedPackOrder()
                        {
                            act_name = act_name,
                            mch_billno = mch_billno,
                            err_code_desc = err_code_des,
                            status = "SENDING",
                            send_time = DateTime.Now,
                            re_openid = openid,
                            send_name = send_name,
                            total_amount = amount,
                            total_num = 1,
                            remark = remark,
                            wishing = wishing
                        };
                        PaymentDb.WxRedPackOrder.Add(item);
                        PaymentDb.SaveChanges();
                        return "SUCCESS";
                    }
                    else
                        err_code_des = info.err_code_des;
                }
                else
                {
                    err_code_des = info.return_msg;
                }
                WxRedPackOrder redpack = new WxRedPackOrder()
                {
                    act_name = act_name,
                    mch_billno = mch_billno,
                    err_code_desc = err_code_des,
                    status = "FAILED",
                    re_openid = openid,
                    send_name = send_name,
                    total_amount = amount,
                    total_num = 1,
                    remark = remark,
                    wishing = wishing
                };
                PaymentDb.WxRedPackOrder.Add(redpack);
                PaymentDb.SaveChanges();
                return "FAIL";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public async Task<string> WxRedPackCreateAsync(string openid, int amount)
        {
            Random random = new Random();
            string mch_billno = "WXREDPACK" + CommonUtilities.generateTimeStamp() + random.Next(1000, 9999);
            var result = await WxRedPackCreateAsync(openid, amount, mch_billno, "红包发放", "寿全斋", "红包发放", "恭喜获得红包");
            return result;
        }
        public async Task<string> WxRedPackCreateAsync(string openid, int amount, string mch_billno, string act_name, string send_name, string remark, string wishing)
        {
            //Wx_WebOauthAccessToken webToken = utilities.getWebOauthAccessToken(code);
            string nonce_str = CommonUtilities.generateNonce();
            //string mch_billno = "WXREDPACK" + CommonUtilities.generateTimeStamp() + random.Next(1000, 9999);
            string mch_id = WeUtil.getMchId();
            string wxappid = WeUtil.getAppId();
            string re_openid = openid;
            int total_amount = amount;
            int total_num = 1;
            string client_ip = WeChatUtilities.getConfigValue(WeChatUtilities.IP);
            List<QueryParameter> redpackParameter = new List<QueryParameter>();
            redpackParameter.Add(new QueryParameter("nonce_str", nonce_str));
            redpackParameter.Add(new QueryParameter("mch_billno", mch_billno));
            redpackParameter.Add(new QueryParameter("mch_id", mch_id));
            redpackParameter.Add(new QueryParameter("wxappid", wxappid));
            redpackParameter.Add(new QueryParameter("send_name", send_name));
            redpackParameter.Add(new QueryParameter("re_openid", re_openid));
            redpackParameter.Add(new QueryParameter("total_amount", total_amount.ToString()));
            redpackParameter.Add(new QueryParameter("total_num", total_num.ToString()));
            redpackParameter.Add(new QueryParameter("wishing", wishing));
            redpackParameter.Add(new QueryParameter("client_ip", client_ip));
            redpackParameter.Add(new QueryParameter("act_name", act_name));
            redpackParameter.Add(new QueryParameter("remark", remark));
            string sign = WeChatUtilities.createPaySign(redpackParameter);
            string content = parseXml(redpackParameter, sign);
            string post_url = "https://api.mch.weixin.qq.com/mmpaymkttransfers/sendredpack";
            var request = WebRequest.Create(post_url) as HttpWebRequest;
            string certpath = @"D:\apiclient_cert.p12";
            string password = "1224974002";
            X509Certificate2 cert = new X509Certificate2(certpath, password);
            request.ClientCertificates.Add(cert);
            try
            {
                request.Method = "post";
                StreamWriter streamWriter = new StreamWriter(request.GetRequestStream());
                streamWriter.Write(content);
                streamWriter.Flush();
                streamWriter.Close();
                var response = await request.GetResponseAsync();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                //return reader.ReadToEnd();
                XmlSerializer xmldes = new XmlSerializer(typeof(WxHBSend_Result));
                WxHBSend_Result info = xmldes.Deserialize(reader) as WxHBSend_Result;
                // 是否成功
                string err_code_des = "";
                if (info.return_code == "SUCCESS")
                {
                    if (info.result_code == "SUCCESS")
                    {
                        WxRedPackOrder item = new WxRedPackOrder()
                        {
                            act_name = act_name,
                            mch_billno = mch_billno,
                            err_code_desc = err_code_des,
                            status = "SENDING",
                            send_time = DateTime.Now,
                            re_openid = openid,
                            send_name = send_name,
                            total_amount = amount,
                            total_num = 1,
                            remark = remark,
                            wishing = wishing
                        };
                        PaymentDb.WxRedPackOrder.Add(item);
                        await PaymentDb.SaveChangesAsync();
                        return "SUCCESS";
                    }
                    else
                        err_code_des = info.err_code_des;
                }
                else
                {
                    err_code_des = info.return_msg;
                }
                WxRedPackOrder redpack = new WxRedPackOrder()
                {
                    act_name = act_name,
                    mch_billno = mch_billno,
                    err_code_desc = err_code_des,
                    status = "FAILED",
                    re_openid = openid,
                    send_name = send_name,
                    total_amount = amount,
                    total_num = 1,
                    remark = remark,
                    wishing = wishing
                };
                PaymentDb.WxRedPackOrder.Add(redpack);
                await PaymentDb.SaveChangesAsync();
                return "FAIL";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // 更新红包状态
        public async Task<string> WxRedPackQuery(string orderId)
        {
            var order = PaymentDb.WxRedPackOrder.SingleOrDefault(m => m.mch_billno == orderId);
            if (order.status == "SENDING" || order.status == "SENT")
            {
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
                var response = await request.GetResponseAsync();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                XmlSerializer xmldes = new XmlSerializer(typeof(WxHBInfo_List));
                WxHBInfo_List info = xmldes.Deserialize(reader) as WxHBInfo_List;
                //判断是否读取成功
                if (info.return_code == "SUCCESS")
                {
                    if (info.result_code == "SUCCESS")
                    {
                        //return info.hblist.hbinfo.FirstOrDefault().rcv_time;
                        // 状态一致，返回当前状态
                        if (order.status == info.status)
                        {
                            return order.status;
                        }
                        else
                        {
                            order.status = info.status;
                            order.reason = info.reason;
                            order.hb_type = info.hb_type;
                            if (info.refund_time == null)
                                order.refund_time = null;
                            else
                                order.refund_time = Convert.ToDateTime(info.refund_time);
                            order.refund_amount = info.refund_amount;
                            order.send_type = info.send_type;
                            order.detail_id = info.detail_id;
                            if (info.send_time == null)
                                order.send_time = null;
                            else
                                order.send_time = Convert.ToDateTime(info.send_time);
                            order.total_amount = info.total_amount;
                            order.total_num = info.total_num;
                            if (info.hblist != null)
                            {
                                var hbinfo = info.hblist.hbinfo.FirstOrDefault();
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
            }
            return order.status;
        }

        public async Task<string> WxRedPackQueryAll()
        {
            var list = from m in PaymentDb.WxRedPackOrder
                       where m.status == "SENT"
                       select m;
            foreach (var item in list)
            {
                await WxRedPackQuery(item.mch_billno);
            }
            return "SUCCESS";
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
        public string hb_type { get; set; }
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
        [XmlElement(ElementName = "hblist")]
        public hblist hblist { get; set; }
    }
    public class hblist
    {
        [XmlElement(ElementName = "hbinfo")]
        public List<hbinfo> hbinfo { get; set; }
    }
    public class hbinfo
    {
        public string openid { get; set; }
        public string status { get; set; }
        public int amount { get; set; }
        public string rcv_time { get; set; }
    }
    [XmlRoot("xml")]
    public class WxHBSend_Result
    {
        public string return_code { get; set; }
        public string return_msg { get; set; }
        public string sign { get; set; }
        public string result_code { get; set; }
        public string err_code { get; set; }
        public string err_code_des { get; set; }
        public string mch_billno { get; set; }
        public string mch_id { get; set; }
        public string wxappid { get; set; }
        public string re_openid { get; set; }
        public int total_amount { get; set; }
        public string send_time { get; set; }
        public string send_listid { get; set; }

    }
    public enum Wx_RedPack_Status
    {
        SENDING = 1,
        SENT = 2,
        RECEIVED = 3,
        FAILED = 4,
        REFUND = 5
    }
}