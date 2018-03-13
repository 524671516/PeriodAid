using System;
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
using System.Data.SqlClient;

namespace PeriodAid.DAL
{
    public class ERPOrderUtilities
    {
        private static string AppId = "130412";
        private static string AppSecret = "26d2e926f42a4f2181dd7d1b7f7d55c0";
        private static string SessionKey = "8a503b3d9d0d4119be2868cc69a8ef5a";
        private static string API_Url = "http://v2.api.guanyierp.com/rest/erp_open";
        private ERPOrderDataContext erpdb;
        public static int SEARCH_MOBILE = 1;
        public static int SEARCH_PLATFORMCODE = 2;
        public static int SEARCH_RECEIVERNAME = 3;

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
            if (totalcount > 50000)
            {
                currenttask.status = -1;
                currenttask.message = "超过50000条数据，获取失败";
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
        /// <summary>
        /// 查看近7天是否有订单，有的话返回单号，没有的话，返回NULL
        /// </summary>

        public orders existERPOrders(int searchType, string searchcontent, string shop_code)
        {
            StringBuilder json = new StringBuilder();
            json.Append("{");
            json.Append("\"appkey\":\"" + AppId + "\"," +
                   "\"method\":\"gy.erp.trade.get\"," +
                   "\"sessionkey\":\"" + SessionKey + "\"," +
                   "\"shop_code\":\"" + shop_code + "\",");
            switch (searchType)
            {
                case 1:
                    json.Append("\"receiver_mobile\":\"" + searchcontent + "\"");
                    break;
                case 2:
                    json.Append("\"platform_code\":\"" + searchcontent + "\"");
                    break;
                case 3:
                    json.Append("\"vip_name\":\"" + searchcontent + "\"");
                    break;
                default:
                    break;
            }
            json.Append("}");
            string signature = sign(json.ToString(), AppSecret);
            StringBuilder info = new StringBuilder();
            info.Append(json.ToString());
            info.Remove(info.Length - 1, 1);
            info.Append(",\"sign\":\"" + signature + "\"}");
            var request = WebRequest.Create(API_Url) as HttpWebRequest;
            request.ContentType = "text/json";
            request.Method = "post";
            string result = "";
            StreamWriter streamWriter = new StreamWriter(request.GetRequestStream());
            try
            {
                streamWriter.Write(info.ToString());
                streamWriter.Flush();
                streamWriter.Close();
                var response = request.GetResponse();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                    StringBuilder sb = new StringBuilder(result);
                    sb.Replace("\"refund\":\"NoRefund\"", "\"refund\":0");
                    sb.Replace("\"refund\":\"RefundSuccess\"", "\"refund\":1");
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    Orders_Result r = JsonConvert.DeserializeObject<Orders_Result>(sb.ToString());
                    if (r.total > 0)
                    {
                        return r.orders.LastOrDefault();
                    }
                    else
                        return null;
                }
            }
            catch (Exception)
            {
                streamWriter.Close();
                //CommonUtilities.writeLog("page: " + page + ", Exception: " + ex.Message);
                return null;
            }
        }
        public Orders_Result getERPORDERS(string platform_code, string shop_code)
        {
            string json = "{" +
                   "\"appkey\":\"" + AppId + "\"," +
                   "\"method\":\"gy.erp.trade.get\"," +
                   "\"sessionkey\":\"" + SessionKey + "\"," +
                   "\"shop_code\":\"" +shop_code+"\","+
                   "\"platform_code\":\"" + platform_code + "\"" +
                   "}";
            string signature = sign(json, AppSecret);
            string info = "{" +
                   "\"appkey\":\"" + AppId + "\"," +
                   "\"method\":\"gy.erp.trade.get\"," +
                   "\"sessionkey\":\"" + SessionKey + "\"," +
                   "\"shop_code\":\"" + shop_code + "\"," +
                   "\"platform_code\":\"" + platform_code + "\"," +
                    "\"sign\":\"" + signature + "\"" +
                    "}";
            var request = WebRequest.Create(API_Url) as HttpWebRequest;
            request.ContentType = "text/json";
            request.Method = "post";
            string result = "";
            StreamWriter streamWriter = new StreamWriter(request.GetRequestStream());
            try
            {
                streamWriter.Write(info);
                streamWriter.Flush();
                streamWriter.Close();
                var response = request.GetResponse();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                    StringBuilder sb = new StringBuilder(result);
                    sb.Replace("\"refund\":\"NoRefund\"", "\"refund\":0");
                    sb.Replace("\"refund\":\"RefundSuccess\"", "\"refund\":1");
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    Orders_Result r = JsonConvert.DeserializeObject<Orders_Result>(sb.ToString());
                    return r;
                }
            }
            catch (Exception)
            {
                streamWriter.Close();
                //CommonUtilities.writeLog("page: " + page + ", Exception: " + ex.Message);
                return null;
            }
            //return null;
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
            if (totalcount > 50000)
            {
                currenttask.status = -1;
                currenttask.message = "超过50000条数据，获取失败";
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

        public async Task<int> Download_ERPItems()
        {
            int totalcount = await getERPItems_Count();
            taskstatus currenttask = new taskstatus()
            {
                name = "Items-" + DateTime.Now.ToShortDateString(),
                totalcount = totalcount,
                currentcount = 0,
                status = 0,
                type = 2,
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
                string sql = "DELETE FROM items";
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
                        Task<string> newtask = getERPItems(i, currenttask.id);
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
        public async Task<List<shops>> getERPShops()
        {
            string json = "{" +
                    "\"appkey\":\"" + AppId + "\"," +
                    "\"method\":\"gy.erp.shop.get\"," +
                    "\"sessionkey\":\"" + SessionKey + "\"," +
                    "\"page_size\":100," +
                    "\"page_no\":" + 1 +
                    "}";
            string signature = sign(json, AppSecret);
            string post_url = "http://v2.api.guanyierp.com/rest/erp_open";
            var request = WebRequest.Create(post_url) as HttpWebRequest;
            string info = "{" +
                "\"appkey\":\"" + AppId + "\"," +
                    "\"method\":\"gy.erp.shop.get\"," +
                    "\"sessionkey\":\"" + SessionKey + "\"," +
                    "\"page_size\":100," +
                    "\"page_no\":" + 1 +"," +
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
                        Stores_Result r = JsonConvert.DeserializeObject<Stores_Result>(result);
                        if (r != null)
                        {
                            return r.shops;
                        }
                        return null;
                    }
                }

            }
            catch (UriFormatException)
            {
                return null;
                //return Content(uex.Message);// 出错处理
            }
            catch (WebException)
            {
                return null;//return Content(ex.Message);// 出错处理
            }
        }
        public async Task<int> getERPItems_Count()
        {
            string json = "{" +
                    "\"appkey\":\"" + AppId + "\"," +
                    "\"method\":\"gy.erp.items.get\"," +
                    "\"sessionkey\":\"" + SessionKey + "\"," +
                    "\"page_size\":1," +
                    "\"page_no\":" + 1 +
                    "}";
            string signature = sign(json, AppSecret);
            string post_url = "http://v2.api.guanyierp.com/rest/erp_open";
            var request = WebRequest.Create(post_url) as HttpWebRequest;
            string info = "{" +
                "\"appkey\":\"" + AppId + "\"," +
                    "\"method\":\"gy.erp.items.get\"," +
                    "\"sessionkey\":\"" + SessionKey + "\"," +
                    "\"page_size\":1," +
                    "\"page_no\":" + 1 + "," +
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
                        Items_Result r = JsonConvert.DeserializeObject<Items_Result>(result);
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

        public async Task<string> getERPItems(int page, int statusid)
        {
            var status = erpdb.taskstatus.SingleOrDefault(m => m.id == statusid);
            if (status != null)
            {
                string json = "{" +
                        "\"appkey\":\"" + AppId + "\"," +
                        "\"method\":\"gy.erp.items.get\"," +
                        "\"sessionkey\":\"" + SessionKey + "\"," +
                        "\"page_size\":100," +
                        "\"page_no\":" + page + 
                        "}";
                string signature = sign(json, AppSecret);
                string post_url = "http://v2.api.guanyierp.com/rest/erp_open";
                string info = "{" +
                    "\"appkey\":\"" + AppId + "\"," +
                    "\"method\":\"gy.erp.items.get\"," +
                    "\"sessionkey\":\"" + SessionKey + "\"," +
                    "\"page_size\":100," +
                    "\"page_no\":" + page + "," +
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
                            Items_Result r = JsonConvert.DeserializeObject<Items_Result>(result);
                            if (r != null)
                            {

                                if (r.items.Count > 0)
                                {
                                    
                                    erpdb.items.AddRange(r.items);
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


        public string getERPItems()
        {
            string json = "{" +
                    "\"appkey\":\"" + AppId + "\"," +
                    "\"method\":\"gy.erp.items.get\"," +
                    "\"sessionkey\":\"" + SessionKey + "\"," +
                    "\"code\":\"sqz003_2\"," +
                    "\"page_size\":100," +
                    "\"page_no\":" + 1 +
                    "}";
            string signature = sign(json, AppSecret);
            string post_url = "http://v2.api.guanyierp.com/rest/erp_open";
            var request = WebRequest.Create(post_url) as HttpWebRequest;
            string info = "{" +
                "\"appkey\":\"" + AppId + "\"," +
                    "\"method\":\"gy.erp.items.get\"," +
                    "\"sessionkey\":\"" + SessionKey + "\"," +
                    "\"code\":\"sqz003_2\"," +
                    "\"page_size\":100," +
                    "\"page_no\":" + 1 + "," +
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
                    var response = request.GetResponse();
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        result = reader.ReadToEnd();
                        return result;
                    }
                }

            }
            catch (UriFormatException)
            {
                return null;
                //return Content(uex.Message);// 出错处理
            }
            catch (WebException)
            {
                return null;//return Content(ex.Message);// 出错处理
            }
        }

        public async Task<int> setTags(IEnumerable<VipIds> vipids, int tagid)
        {
            int success = 0;
            var tag = erpdb.tags.SingleOrDefault(m => m.id == tagid);
            foreach (var item in vipids)
            {
                try
                {
                    var vip = erpdb.vips.SingleOrDefault(m => m.id == item.id);
                    if (vip.tags.Contains(tag))
                    {

                    }
                    else
                    {
                        vip.tags.Add(tag);
                        success++;
                    }
                }
                catch
                {
                    //return -1;
                }
            }
            await erpdb.SaveChangesAsync();
            tag.totalcount = tag.vips.Count;
            erpdb.Entry(tag).State = System.Data.Entity.EntityState.Modified;
            await erpdb.SaveChangesAsync();
            return success;
        }

        public IEnumerable<VipIds> getVipIdsByOrderId(string orderid)
        {
            string[] ids = orderid.Split('\n');
            // 加'号
            List<string> resultstring = new List<string>();
            for (int i = 0; i < ids.Length; i++)
            {
                if (ids[i].Trim().Length > 0)
                {
                    resultstring.Add("'" + ids[i].Trim() + "'");
                }
            }
            string formated_ids = string.Join(",", resultstring.ToArray());
            string sql = "SELECT T1.[id] FROM[ORDERERP].[dbo].[vips] as T1 left join[ORDERERP].[dbo].[orders] as T2 on " +
                "T1.name = T2.vip_name and T1.shop_name = T2.shop_name " +
                "where T2.platform_code in (" + formated_ids + ") group by T1.[id]";
            //SqlParameter[] parm = { new SqlParameter("content", formated_ids) };
            var vipids = erpdb.Database.SqlQuery<VipIds>(sql);
            return vipids;
        }
        public IEnumerable<VipIds> getVipIdsByVipName(string vipname)
        {
            string[] ids = vipname.Split('\n');
            // 加'号
            List<string> resultstring = new List<string>();
            for (int i = 0; i < ids.Length; i++)
            {
                if (ids[i].Trim().Length > 0)
                {
                    resultstring.Add("'" + ids[i].Trim() + "'");
                }
            }
            string formated_ids = string.Join(",", resultstring.ToArray());
            string sql = "SELECT T1.[id] FROM [ORDERERP].[dbo].[vips] as T1 "
                +"where T1.name in (" + formated_ids + ") group by T1.[id]";
            //SqlParameter[] parm = { new SqlParameter("content", formated_ids) };
            var vipids = erpdb.Database.SqlQuery<VipIds>(sql);
            return vipids;
        }

        public string createOrder(ERPCustomOrder order)
        {
            StringBuilder details = new StringBuilder();
            if (order.details != null)
            {
                foreach (var item in order.details)
                {
                    details.Append("{");
                    details.Append("\"qty\":" + item.qty + ",");
                    details.Append("\"price\":\"" + item.price + "\",");
                    details.Append("\"note\":null,");
                    details.Append("\"refund\":0,");
                    details.Append("\"oid\":0,");
                    details.Append("\"item_code\":\"" + item.item_code + "\",");
                    details.Append("\"sku_code\":null");
                    details.Append("},");
                }
                if(order.details.Count>=1)
                    details.Remove(details.Length - 1, 1);
            }
            StringBuilder json = new StringBuilder();
            json.Append("{" +
                    "\"appkey\":\"" + AppId + "\"," +
                    "\"method\":\"gy.erp.trade.add\"," +
                    "\"sessionkey\":\"" + SessionKey + "\"," +
                    "\"order_type_code\":\"销售订单\"," +
                    "\"platform_code\":\"" + order.platform_code + "\"," +
                    "\"shop_code\":\"" + order.shop_code + "\"," +
                    //"\"qty\": 1," +
                    "\"vip_code\":\"" + order.vip_code + "\"," +
                    //"\"vip_name\":\"" + order.event_name + "\"," +
                    "\"warehouse_code\":\"" + order.warehouse_code + "\"," +
                    "\"express_code\":\"" + order.express_code + "\"," +
                    "\"receiver_name\":\"" + replaceWord(order.receiver_name) + "\"," +
                    "\"receiver_province\":\"" + order.receiver_province + "\"," +
                    "\"receiver_city\":\"" + order.receiver_city + "\"," +
                    "\"receiver_district\":\"" + order.receiver_district + "\"," +
                    "\"receiver_mobile\":\"" + order.receiver_mobile + "\"," +
                    "\"receiver_zip\":\"" + order.receiver_zip + "\"," +
                    "\"receiver_address\":\"" + order.receiver_address + "\"," +
                    "\"deal_datetime\":\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\","
                    + "\"details\":[");
            json.Append(details);
            json.Append("]}");
            //json.Append("\"payments\":[{");
            //json.Append("\"payment\":0,\"paytime\":\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\",");
            //json.Append("\"account\":null,\"pay_type_code\":\"支付宝\",\"pay_code\":null");
            //json.Append("}],");
            //json.Append("\"invoices\":null}");
            //json.Append("\"invoices\":[{\"invoice_type\":1,\"invoice_title\":null,\"invoice_content\":null,\"invoice_amount\":null,\"bill_amount\":null}]}");
            //return json.ToString();
            string signature = sign(json.ToString(), AppSecret);
            string post_url = "http://v2.api.guanyierp.com/rest/erp_open";
            var request = WebRequest.Create(post_url) as HttpWebRequest;
            StringBuilder info = new StringBuilder();
            info.Append(json.ToString());
            info.Remove(info.Length - 1, 1);
            info.Append(", \"sign\":\"" + signature + "\"}");
            //return json.ToString() + "<br /><br />"+info.ToString();
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
                    var response = request.GetResponse();
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        result = reader.ReadToEnd();
                        return result;
                    }
                }

            }
            catch (UriFormatException)
            {
                return null;
                //return Content(uex.Message);// 出错处理
            }
            catch (WebException)
            {
                return null;//return Content(ex.Message);// 出错处理
            }
        }

        private string replaceWord(string name)
        {
            var srtName = "";
            if (name.Contains("+"))
            {
                srtName = name.Replace("+", "-");
            }
            return srtName;
        }

        private string addTags(string originaltags, string tagname)
        {
            if (originaltags.Contains(tagname))
            {
                return originaltags;
            }
            else if (originaltags.Trim() == "")
            {
                return tagname;
            }
            else
            {
                return originaltags + "," + tagname;
            }
        }
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
    public class ERPCustomOrder
    {
        public string platform_code { get; set; }
        public string event_name { get; set; }
        public string receiver_name { get; set; }
        public string receiver_province { get; set; }
        public string receiver_city { get; set; }
        public string receiver_district { get; set; }
        public string receiver_mobile { get; set; }
        public string receiver_zip { get; set; }
        public string receiver_address { get; set; }
        public string shop_code { get; set; }
        public string express_code { get; set; }
        public string warehouse_code { get; set; }
        public string vip_code { get; set; }
        public string deal_datetime { get; set; }
        public List<ERPCustomOrder_details> details { get; set; }
    }
    public class ERPCustomOrder_details
    {
        public int qty { get; set; }
        public decimal price { get; set; }
        public string item_code { get; set; }
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

    public class Stores_Result
    {
        public bool success { get; set; }
        public string errorCode { get; set; }//错误代码
        public string subErrorCode { get; set; }//子错误代码
        public string errorDesc { get; set; }//错误描述
        public string subErrorDesc { get; set; }//子错误描述
        public string requestMethod { get; set; }//
        public List<shops> shops { get; set; }
        public int total;
    }
    public class Items_Result
    {
        public bool success { get; set; }
        public string errorCode { get; set; }//错误代码
        public string subErrorCode { get; set; }//子错误代码
        public string errorDesc { get; set; }//错误描述
        public string subErrorDesc { get; set; }//子错误描述
        public string requestMethod { get; set; }//
        public List<items> items { get; set; }
        public int total;
    }

    public class shops
    {
        public string id { get; set; }
        public string nick { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string type_name { get; set; }
    }

    public class VipIds
    {
        public int id { get; set; }
    }
}