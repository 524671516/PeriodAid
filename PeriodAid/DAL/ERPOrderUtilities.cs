﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using PeriodAid.Models;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace PeriodAid.DAL
{
    public class ERPOrderUtilities
    {
        private static string AppId = "130412";
        private static string AppSecret = "26d2e926f42a4f2181dd7d1b7f7d55c0";
        private static string SessionKey = "8a503b3d9d0d4119be2868cc69a8ef5a";
        private static string API_Url = "http://v2.api.guanyierp.com/rest/erp_open";
        private ERPOrderDataContext erpdb;

        public ERPOrderUtilities()
        {
            erpdb = new ERPOrderDataContext();
        }

        #region 获取订单信息
        public async Task<int> Download_ErpOrders(string stime, string etime)
        {
            DateTime st = Convert.ToDateTime(stime);
            DateTime et = Convert.ToDateTime(etime);
            int totalcount = await getERPORDERS_Count(st, et);
            taskstatus currenttask = new taskstatus()
            {
                name = "Order-" + st.ToShortDateString() + "-" + et.ToShortDateString(),
                totalcount = totalcount,
                currentcount = 0,
                status = 0,
                type = 0,
                create_time = DateTime.Now
            };
            erpdb.taskstatus.Add(currenttask);
            string[] results;
            erpdb.SaveChanges();
            if (totalcount > 10000)
            {
                currenttask.status = -1;
                currenttask.message = "超过10000条数据，获取失败";
                erpdb.Entry(currenttask).State = System.Data.Entity.EntityState.Modified;
                erpdb.SaveChanges();
                return -1;
            }
            else
            {
                string sql = "DELETE FROM orders where createtime>='" + st.ToShortDateString() + "' AND createtime<='" + et.ToShortDateString() + "'";
                CommonUtilities.writeLog("sql" + sql);
                erpdb.Database.ExecuteSqlCommand(sql);
                CommonUtilities.writeLog("Remove Success");
                List<Task<string>> alltasks = new List<Task<string>>();
                int j = 0;
                try
                {
                    for (int i = 1; i <= (totalcount / 100) + 1; i++)
                    {
                        if (j == 20)
                        {
                            results = await Task.WhenAll<string>(alltasks);
                            if (results.Contains("FAIL"))
                            {
                                currenttask.status = -1;
                                erpdb.Entry(currenttask).State = System.Data.Entity.EntityState.Modified;
                                erpdb.SaveChanges();
                                return -1;
                            }
                            alltasks = new List<Task<string>>();
                            j = 0;
                        }
                        Task<string> newtask = getERPORDERS(i, st, et, currenttask.id);
                        alltasks.Add(newtask);
                        j++;
                    }
                }
                catch (Exception e)
                {
                    CommonUtilities.writeLog(e.Message);
                }
                results = await Task.WhenAll<string>(alltasks);
                if (results.Contains("FAIL"))
                {
                    currenttask.status = -1;
                    erpdb.Entry(currenttask).State = System.Data.Entity.EntityState.Modified;
                    erpdb.SaveChanges();
                    return -1;
                }
                else
                {
                    currenttask.currentcount = totalcount;
                    currenttask.finish_time = DateTime.Now;
                    currenttask.status = 1;
                    erpdb.Entry(currenttask).State = System.Data.Entity.EntityState.Modified;
                    erpdb.SaveChanges();
                    //return 1;
                    return 0;
                }
            }
        }

        private async Task<int> getERPORDERS_Count(DateTime st, DateTime et)
        {
            string json = "{" +
                    "\"appkey\":\"" + AppId + "\"," +
                    "\"method\":\"gy.erp.trade.history.get\"," +
                    "\"sessionkey\":\"" + SessionKey + "\"," +
                    "\"page_size\":1," +
                    "\"page_no\":" + 1 + "," +
                    "\"start_date\":\"" + st.ToString("yyyy-MM-dd HH:mm:ss") + "\"," +
                    "\"end_date\":\"" + et.ToString("yyyy-MM-dd HH:mm:ss") + "\"" +
                    "}";
            string signature = sign(json, AppSecret);
            //string post_url = "http://v2.api.guanyierp.com/rest/erp_open";
            var request = WebRequest.Create(API_Url) as HttpWebRequest;
            string info = "{" +
                "\"appkey\":\"" + AppId + "\"," +
                "\"method\":\"gy.erp.trade.history.get\"," +
                "\"sessionkey\":\"" + SessionKey + "\"," +
                "\"page_size\":1," +
                "\"page_no\":" + 1 + "," +
                "\"start_date\":\"" + st.ToString("yyyy-MM-dd HH:mm:ss") + "\"," +
                "\"end_date\":\"" + et.ToString("yyyy-MM-dd HH:mm:ss") + "\"," +
                "\"sign\":\"" + signature + "\"" +
                "}";
            //return Content(info);
            string result = "";
            try
            {
                request.ContentType = "text/json";
                request.Method = "post";
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(info);
                    streamWriter.Flush();
                    streamWriter.Close();
                    var response = await request.GetResponseAsync() as HttpWebResponse;
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        result = reader.ReadToEnd();
                        //修改数据合法性
                        StringBuilder sb = new StringBuilder(result);
                        sb.Replace("\"refund\":\"NoRefund\"", "\"refund\":0");
                        sb.Replace("\"refund\":\"RefundSuccess\"", "\"refund\":1");
                        JavaScriptSerializer serializer = new JavaScriptSerializer();
                        Orders_Result r = JsonConvert.DeserializeObject<Orders_Result>(sb.ToString());
                        if (r != null)
                        {
                            return r.total;
                        }
                        return -1;
                    }
                }
            }
            catch (UriFormatException)
            {
                return -1;
            }
            catch (WebException)
            {
                return -1;
            }
        }

        private async Task<string> getERPORDERS(int page, DateTime st, DateTime et, int id)
        {
            string json = "{" +
                    "\"appkey\":\"" + AppId + "\"," +
                    "\"method\":\"gy.erp.trade.history.get\"," +
                    "\"sessionkey\":\"" + SessionKey + "\"," +
                    "\"page_size\":100," +
                    "\"page_no\":" + page + "," +
                    "\"start_date\":\"" + st.ToString("yyyy-MM-dd HH:mm:ss") + "\"," +
                    "\"end_date\":\"" + et.ToString("yyyy-MM-dd HH:mm:ss") + "\"" +
                    "}";
            string signature = sign(json, AppSecret);
            string info = "{" +
                "\"appkey\":\"" + AppId + "\"," +
                "\"method\":\"gy.erp.trade.history.get\"," +
                "\"sessionkey\":\"" + SessionKey + "\"," +
                "\"page_size\":100," +
                "\"page_no\":" + page + "," +
                "\"start_date\":\"" + st.ToString("yyyy-MM-dd HH:mm:ss") + "\"," +
                "\"end_date\":\"" + et.ToString("yyyy-MM-dd HH:mm:ss") + "\"," +
                "\"sign\":\"" + signature + "\"" +
                "}";
            string result = "";
            int retry_count = 0;
            bool flag = false;
            var status = erpdb.taskstatus.SingleOrDefault(m => m.id == id);
            while (!flag)
            {
                if (retry_count > 10)
                {
                    CommonUtilities.writeLog("max retry detected, page: " + page);
                    return "FAIL";
                }
                var request = WebRequest.Create(API_Url) as HttpWebRequest;
                request.ContentType = "text/json";
                request.Method = "post";
                StreamWriter streamWriter = new StreamWriter(request.GetRequestStream());
                try
                {
                    streamWriter.Write(info);
                    streamWriter.Flush();
                    streamWriter.Close();
                    var response = await request.GetResponseAsync();
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        result = reader.ReadToEnd();
                        StringBuilder sb = new StringBuilder(result);
                        sb.Replace("\"refund\":\"NoRefund\"", "\"refund\":0");
                        sb.Replace("\"refund\":\"RefundSuccess\"", "\"refund\":1");
                        JavaScriptSerializer serializer = new JavaScriptSerializer();
                        Orders_Result r = JsonConvert.DeserializeObject<Orders_Result>(sb.ToString());
                        if (r != null)
                        {
                            if (r.orders.Count > 0)
                            {
                                // 验证数据合法性
                                foreach (var item in r.orders)
                                {
                                    if (item.paytime <= new DateTime(1970, 1, 1))
                                    {
                                        item.paytime = item.dealtime;
                                    }
                                    foreach (var payment_item in item.payments)
                                    {
                                        if (payment_item.paytime <= new DateTime(1970, 1, 1))
                                        {
                                            payment_item.paytime = item.dealtime;
                                        }
                                    }
                                }
                                erpdb.orders.AddRange(r.orders);
                                erpdb.SaveChanges();
                                flag = true;
                                
                                status.currentcount += 100;
                                erpdb.Entry(status).State = System.Data.Entity.EntityState.Modified;
                                erpdb.SaveChanges();
                                CommonUtilities.writeLog("Success: page: " + page + ", retry_count: " + retry_count);
                            }
                        }
                        else
                        {
                            //return Content("Fail");
                        }
                    }
                }
                catch (Exception ex)
                {
                    streamWriter.Close();
                    CommonUtilities.writeLog("page: " + page + ", Exception: " + ex.Message);
                }
                retry_count++;
            }
            return "Success";
        }
        #endregion


        #region 获取会员信息
        public async Task<int> Download_ERPVips(string stime, string etime)
        {

            DateTime st = Convert.ToDateTime(stime);
            DateTime et = Convert.ToDateTime(etime);
            int totalcount = await getERPVips_Count(st, et);
            taskstatus currenttask = new taskstatus()
            {
                name = "Vip-" + st.ToShortDateString() + "-" + et.ToShortDateString(),
                totalcount = totalcount,
                currentcount = 0,
                status = 0,
                type = 1,
                create_time = DateTime.Now
            };
            erpdb.taskstatus.Add(currenttask);
            erpdb.SaveChanges();
            string[] results;
            if (totalcount > 10000)
            {
                currenttask.status = -1;
                currenttask.message = "超过10000条数据，获取失败";
                erpdb.Entry(currenttask).State = System.Data.Entity.EntityState.Modified;
                erpdb.SaveChanges();
                return -1;
            }
            else
            {
                // 删除已有的数据
                string sql = "DELETE FROM vips where created>='" + st.ToShortDateString() + "' AND created<='" + et.ToShortDateString() + "'";
                CommonUtilities.writeLog("sql" + sql);
                erpdb.Database.ExecuteSqlCommand(sql);
                CommonUtilities.writeLog("Remove Success");
                List<Task<string>> alltasks = new List<Task<string>>();
                int j = 0;
                try
                {
                    for (int i = 1; i <= (totalcount / 100) + 1; i++)
                    {
                        if (j == 20)
                        {
                            results = await Task.WhenAll<string>(alltasks);
                            if (results.Contains("FAIL"))
                            {
                                currenttask.status = -1;
                                erpdb.Entry(currenttask).State = System.Data.Entity.EntityState.Modified;
                                erpdb.SaveChanges();
                                return -1;
                            }
                            alltasks = new List<Task<string>>();
                            j = 0;
                        }
                        Task<string> newtask = getERPVips(i, st, et, currenttask.id);
                        alltasks.Add(newtask);
                        j++;
                    }
                }
                catch (Exception e)
                {
                    CommonUtilities.writeLog(e.Message);
                }
                results = await Task.WhenAll<string>(alltasks);
                if (results.Contains("FAIL"))
                {
                    currenttask.status = -1;
                    erpdb.Entry(currenttask).State = System.Data.Entity.EntityState.Modified;
                    erpdb.SaveChanges();
                    return -1;
                }
                else
                {
                    currenttask.finish_time = DateTime.Now;
                    currenttask.status = 1;
                    currenttask.currentcount = totalcount;
                    erpdb.Entry(currenttask).State = System.Data.Entity.EntityState.Modified;
                    erpdb.SaveChanges();
                    return 0;
                }
            }
        }

        public async Task<int> getERPVips_Count(DateTime st, DateTime et)
        {
            string json = "{" +
                    "\"appkey\":\"" + AppId + "\"," +
                    "\"method\":\"gy.erp.vip.get\"," +
                    "\"sessionkey\":\"" + SessionKey + "\"," +
                    "\"page_size\":1," +
                    "\"page_no\":" + 1 + "," +
                    "\"start_created\":\"" + st.ToString("yyyy-MM-dd HH:mm:ss") + "\"," +
                    "\"end_created\":\"" + et.ToString("yyyy-MM-dd HH:mm:ss") + "\"" +
                    "}";
            string signature = sign(json, AppSecret);
            string post_url = "http://v2.api.guanyierp.com/rest/erp_open";
            var request = WebRequest.Create(post_url) as HttpWebRequest;
            string info = "{" +
                "\"appkey\":\"" + AppId + "\"," +
                    "\"method\":\"gy.erp.vip.get\"," +
                    "\"sessionkey\":\"" + SessionKey + "\"," +
                    "\"page_size\":1," +
                    "\"page_no\":" + 1 + "," +
                    "\"start_created\":\"" + st.ToString("yyyy-MM-dd HH:mm:ss") + "\"," +
                    "\"end_created\":\"" + et.ToString("yyyy-MM-dd HH:mm:ss") + "\"," +
                    "\"sign\":\"" + signature + "\"" +
                "}";
            //return Content(info);
            string result = "";
            try
            {
                request.ContentType = "text/json";
                request.Method = "post";
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(info);
                    streamWriter.Flush();
                    streamWriter.Close();
                    var response = await request.GetResponseAsync() as HttpWebResponse;
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        result = reader.ReadToEnd();
                        //return Content(result);
                        JavaScriptSerializer serializer = new JavaScriptSerializer();
                        Vips_Result r = JsonConvert.DeserializeObject<Vips_Result>(result);
                        if (r != null)
                        {
                            return r.total;
                        }
                        return -1;
                    }
                }

            }
            catch (UriFormatException)
            {
                return -1;
                //return Content(uex.Message);// 出错处理
            }
            catch (WebException)
            {
                return -1;//return Content(ex.Message);// 出错处理
            }
        }

        public async Task<string> getERPVips(int page, DateTime st, DateTime et, int statusid)
        {
            var status = erpdb.taskstatus.SingleOrDefault(m => m.id == statusid);
            if (status != null)
            {
                string json = "{" +
                        "\"appkey\":\"" + AppId + "\"," +
                        "\"method\":\"gy.erp.vip.get\"," +
                        "\"sessionkey\":\"" + SessionKey + "\"," +
                        "\"page_size\":100," +
                        "\"page_no\":" + page + "," +
                        "\"start_created\":\"" + st.ToString("yyyy-MM-dd HH:mm:ss") + "\"," +
                        "\"end_created\":\"" + et.ToString("yyyy-MM-dd HH:mm:ss") + "\"" +
                        "}";
                string signature = sign(json, AppSecret);
                string post_url = "http://v2.api.guanyierp.com/rest/erp_open";
                string info = "{" +
                    "\"appkey\":\"" + AppId + "\"," +
                        "\"method\":\"gy.erp.vip.get\"," +
                        "\"sessionkey\":\"" + SessionKey + "\"," +
                        "\"page_size\":100," +
                        "\"page_no\":" + page + "," +
                        "\"start_created\":\"" + st.ToString("yyyy-MM-dd HH:mm:ss") + "\"," +
                        "\"end_created\":\"" + et.ToString("yyyy-MM-dd HH:mm:ss") + "\"," +
                    "\"sign\":\"" + signature + "\"" +
                    "}";
                //return Content(info);
                string result = "";
                int retry_count = 0;
                bool flag = false;
                while (!flag)
                {
                    if (retry_count > 10)
                    {
                        return "FAIL";
                    }
                    var request = WebRequest.Create(post_url) as HttpWebRequest;
                    request.ContentType = "text/json";
                    request.Method = "post";
                    StreamWriter streamWriter = new StreamWriter(request.GetRequestStream());
                    try
                    {
                        streamWriter.Write(info);
                        streamWriter.Flush();
                        streamWriter.Close();
                        var response = await request.GetResponseAsync();
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            result = reader.ReadToEnd();
                            JavaScriptSerializer serializer = new JavaScriptSerializer();
                            Vips_Result r = JsonConvert.DeserializeObject<Vips_Result>(result);
                            if (r != null)
                            {
                                
                                if (r.vips.Count > 0)
                                {
                                    foreach (var item in r.vips)
                                    {
                                        if (item.birthday <= new DateTime(1970, 1, 1))
                                        {
                                            item.birthday = null;
                                        }
                                    }
                                    erpdb.vips.AddRange(r.vips);
                                    erpdb.SaveChanges();
                                    flag = true;
                                    status.currentcount += 100;
                                    erpdb.Entry(status).State = System.Data.Entity.EntityState.Modified;
                                    erpdb.SaveChanges();
                                    CommonUtilities.writeLog("Success: page: " + page + ", retry_count: " + retry_count);
                                }
                            }
                            else
                            {
                                //return Content("Fail");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        streamWriter.Close();
                        CommonUtilities.writeLog("page: " + page + ", Exception: " + ex.Message);
                    }
                    retry_count++;
                }
                return "Success";
            }
            return "FAIL";
        }
        #endregion


        private string sign(string json, string secret)
        {
            StringBuilder enValue = new StringBuilder();
            //前后加上secret
            enValue.Append(secret);
            enValue.Append(json);
            enValue.Append(secret);
            //使用MD5加密(32位大写)
            return CommonUtilities.encrypt_MD5(enValue.ToString()).ToUpper();
        }
    }

    public class Orders_Result
    {
        public bool success { get; set; }//响应成功/响应失败
        public string errorCode { get; set; }//错误代码
        public string subErrorCode { get; set; }//子错误代码
        public string errorDesc { get; set; }//错误描述
        public string subErrorDesc { get; set; }//子错误描述
        public string requestMethod { get; set; }//请求接口方法
        public List<orders> orders { get; set; }
        public int total;
    }

    public class Vips_Result
    {
        public bool success { get; set; }//响应成功/响应失败
        public string errorCode { get; set; }//错误代码
        public string subErrorCode { get; set; }//子错误代码
        public string errorDesc { get; set; }//错误描述
        public string subErrorDesc { get; set; }//子错误描述
        public string requestMethod { get; set; }//请求接口方法
        public List<vips> vips { get; set; }
        public int total;
    }
}