using CsvHelper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using PagedList;
using PeriodAid.DAL;
using PeriodAid.Filters;
using PeriodAid.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml;

namespace PeriodAid.Controllers
{
    [Authorize(Roles = "Staff")]
    public class MonthlyDeliveryController : Controller
    {
        // GET: MonthlyDelivery
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private MonthlyDeliveryModel md_db;
        int try_times = 0;
        public MonthlyDeliveryController()
        {
            md_db = new MonthlyDeliveryModel();
        }
        public MonthlyDeliveryController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        private byte[] SetFileToByteArray(HttpPostedFileBase File)
        {
            Stream stream = File.InputStream;
            byte[] ArrayByte = new byte[File.ContentLength];
            stream.Read(ArrayByte, 0, File.ContentLength);
            stream.Close();
            return ArrayByte;
        }
        
        private static string AppId = "130412";
        private static string AppSecret = "26d2e926f42a4f2181dd7d1b7f7d55c0";
        private static string SessionKey = "8a503b3d9d0d4119be2868cc69a8ef5a";
        private static string API_Url = "http://v2.api.guanyierp.com/rest/erp_open";
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
        
        // MD
        [Authorize(Roles = "MD")]
        public ActionResult MD_OrderView()
        {
            return View();
        }
        
        public ActionResult MD_OrderPartialView(int? page, string query, int create_status)
        {
            int _page = page ?? 1;
            if (create_status == -1)
            {
                if (query != "")
                {
                    var order = from m in md_db.MD_Order
                                where m.receiver_times == 1
                                select m;
                    var SearchResult = (from m in order
                                        where m.order_code.Contains(query) || m.MD_Product.product_code.Contains(query)
                                        orderby m.receiver_date descending
                                        select m).ToPagedList(_page, 20);
                    return PartialView(SearchResult);
                }
                else
                {
                    var SearchResult = (from m in md_db.MD_Order
                                        where m.receiver_times == 1
                                        orderby m.receiver_date descending
                                        select m).ToPagedList(_page, 20);
                    return PartialView(SearchResult);
                }
            }
            else
            {
                if (query != "")
                {
                    var order = from m in md_db.MD_Order
                                where m.receiver_times == 1 && m.createSub_status == create_status
                                select m;
                    var SearchResult = (from m in order
                                        where m.order_code.Contains(query) || m.MD_Product.product_code.Contains(query)
                                        orderby m.receiver_date descending
                                        select m).ToPagedList(_page, 20);
                    return PartialView(SearchResult);
                }
                else
                {
                    var SearchResult = (from m in md_db.MD_Order
                                        where m.receiver_times == 1 && m.createSub_status == create_status
                                        orderby m.receiver_date descending
                                        select m).ToPagedList(_page, 20);
                    return PartialView(SearchResult);
                }
            }

        }

        public int ReceiverTimes(int order_id)
        {
            var count = from m in md_db.MD_Order
                        where m.upload_status == 0 && m.parentOrder_id == order_id
                        select m;
            return count.Count();
        }
        [Authorize(Roles = "MD")]
        public ActionResult MD_OrderDetailView(int order_id)
        {
            var orderdetail = md_db.MD_Order.SingleOrDefault(m => m.Id == order_id && m.receiver_times == 1);
            ViewBag.orderDetail = orderdetail.parentOrder_id;
            return View();
        }
        
        public ActionResult MD_OrderDetailPartialView(int parentOrder_id)
        {
            var orderDetail = from m in md_db.MD_Order
                              where m.parentOrder_id == parentOrder_id
                              orderby m.receiver_date ascending
                              select m;
            return PartialView(orderDetail);
        }
        // 合并
        [HttpPost]
        public JsonResult Amalgamate_Order(int[] order_id)
        {
            var FirstOid = order_id[0];
            var LastOid = order_id[order_id.Count() - 1];
            DateTime? receiverDate = null;
            var Order = md_db.MD_Order.SingleOrDefault(m => m.Id == FirstOid && m.upload_status == 0);
            var Orders = md_db.MD_Order.SingleOrDefault(m => m.Id == LastOid && m.upload_status == 0);
            var t1 = DateTime.Parse(Order.receiver_date.Value.ToString("yyyy-MM-dd"));
            var t2 = DateTime.Parse(Orders.receiver_date.Value.ToString("yyyy-MM-dd"));
            int total_quantity = 0;
            if (t1 > t2)
            {
                receiverDate = t2;
            }
            else
            {
                receiverDate = t1;
            }
            foreach (var oId in order_id)
            {
                var order = md_db.MD_Order.SingleOrDefault(m => m.Id == oId && m.delivery_state == 0 && m.receiver_times != 1);
                total_quantity += order.qty;
                md_db.MD_Order.Remove(order);
            }
            var count = order_id.Count();
            if (count != 0)
            {
                var OrderDetail = new MD_Order();
                if (Order.order_code.Contains("MD"))
                {
                    OrderDetail.order_code = Order.order_code;
                }
                else
                {
                    OrderDetail.order_code = "MD" + Order.order_code;
                }
                OrderDetail.qty = total_quantity;
                OrderDetail.receiver_date = receiverDate;
                OrderDetail.delivery_state = 0;
                OrderDetail.receiver_area = Order.receiver_area;
                OrderDetail.receiver_address = Order.receiver_address;
                OrderDetail.parentOrder_id = Order.parentOrder_id;
                OrderDetail.receiver_tel = Order.receiver_tel;
                OrderDetail.product_id = Order.product_id;
                OrderDetail.vip_code = Order.vip_code;
                OrderDetail.receiver_times = Order.receiver_times;
                OrderDetail.express_information = " ";
                OrderDetail.receiver_name = Order.receiver_name;
                OrderDetail.order_status = 1;
                md_db.MD_Order.Add(OrderDetail);
                md_db.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }
        // 取消发送
        public JsonResult Cancel_Order(int order_id)
        {
            var order = from m in md_db.MD_Order
                        where m.parentOrder_id == order_id && m.delivery_state == 0 && m.upload_status != 1 && m.receiver_times != 1
                        select m;
            var Order = md_db.MD_Order.SingleOrDefault(m => m.Id == order_id);
            if (order.Count() != 0)
            {
                md_db.MD_Order.RemoveRange(order);
                MD_Record logs = new MD_Record();
                logs.record_date = DateTime.Now;
                logs.record_type = "Cancel";
                logs.record_detail = Order.order_code + " 取消发货";
                logs.record_amount = 1;
                md_db.MD_Record.Add(logs);
                Order.order_status = -1;
                md_db.Entry(Order).State = System.Data.Entity.EntityState.Modified;
                md_db.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }
        // 更改
        public ActionResult MD_EditOrderInfo(int order_id)
        {
            var order = md_db.MD_Order.SingleOrDefault(m => m.Id == order_id);
            ViewBag.Order = order;
            return PartialView(order);
        }
        [HttpPost]
        public ActionResult MD_EditOrderInfo(MD_Order model)
        {
            var order = md_db.MD_Order.AsNoTracking().SingleOrDefault(m => m.Id == model.Id);
            if (ModelState.IsValid)
            {
                MD_Order Orders = new MD_Order();
                if (TryUpdateModel(Orders))
                {
                    MD_Record logs = new MD_Record();
                    logs.record_date = DateTime.Now;
                    logs.record_type = "Edit";
                    if (order.receiver_name != model.receiver_name)
                        logs.record_detail = order.order_code + " 收件人由: " + " " + order.receiver_name + " 修改为 :" + " " + model.receiver_name;
                    if (order.receiver_tel != model.receiver_tel)
                        logs.record_detail = order.order_code + " 联系电话由: " + " " + order.receiver_tel + " 修改为 :" + " " + model.receiver_tel;
                    if (order.receiver_date != model.receiver_date)
                        logs.record_detail = order.order_code + " 发货日期由: " +" "+ order .receiver_date+ " 修改至 :" +" " + model.receiver_date;
                    if(order.receiver_address != model.receiver_address)
                        logs.record_detail = order.order_code + " 发货地址由: " + " " + order.receiver_area + " " + order.receiver_address + " 修改至 :" + " " + model.receiver_area + " " + model.receiver_address;
                    if (order.receiver_date != model.receiver_date && order.receiver_address != model.receiver_address)
                        logs.record_detail = order.order_code + " 新增修改 :"+" " + model.receiver_date + " " + model.receiver_area + " " + model.receiver_address;
                    if(order.receiver_date == model.receiver_date && order.receiver_address == model.receiver_address && order.receiver_name == model.receiver_name && order.receiver_tel == model.receiver_tel)
                        return Json(new { result = "ERROR" });
                    logs.record_amount = 1;
                    md_db.MD_Record.Add(logs);
                    md_db.Entry(Orders).State = System.Data.Entity.EntityState.Modified;
                    md_db.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
            }
            return Json(new { result = "FAIL" });
        }
        // 手工获取ERP订单信息
        [HttpPost]
        public JsonResult getSingleErpOrder(string platform_code)
        {
            if (platform_code != "")
            {
                var md_order = md_db.MD_Order.SingleOrDefault(m => m.order_code == platform_code);
                string json = "{" +
                       "\"appkey\":\"" + AppId + "\"," +
                        "\"method\":\"gy.erp.trade.get\"," +
                        //"\"receiver_mobile\":\"" + platform_code + "\"," +
                        "\"platform_code\":\"" + platform_code + "\"," +
                        //"\"platform_code\":\"" + platform_code + "\"," +
                        "\"sessionkey\":\"" + SessionKey + "\"" +
                        "}";
                string signature = sign(json, AppSecret);
                string info = "{" +
                       "\"appkey\":\"" + AppId + "\"," +
                        "\"method\":\"gy.erp.trade.get\"," +
                        //"\"receiver_mobile\":\"" + platform_code + "\"," +
                        "\"platform_code\":\"" + platform_code + "\"," +
                        //"\"platform_code\":\"" + platform_code + "\"," +
                        "\"sessionkey\":\"" + SessionKey + "\"," +
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
                        orders_Result r = JsonConvert.DeserializeObject<orders_Result>(sb.ToString());
                        if (r.success)
                        {
                            if (r.orders.Count() != 0)
                            {
                                if (md_order == null)
                                {
                                    md_order = new MD_Order();
                                    for(int x = 0; x < r.orders[0].details.Count();x++)
                                    {
                                        if (r.orders[0].details[x].note != null)
                                        {
                                            if (r.orders[0].details[x].note.Length != 0)
                                            {
                                                if (r.orders[0].details[x].note.Contains("sqz333"))
                                                {
                                                    md_order.product_id = 1;
                                                    break;
                                                }
                                                else if (r.orders[0].details[x].note.Contains("sqz444"))
                                                {
                                                    md_order.product_id = 3;
                                                    break;
                                                }
                                            }
                                            if (r.orders[0].details[x].item_code.Contains("sqz187"))
                                            {
                                                md_order.product_id = 1;
                                                break;
                                            }
                                        }
                                        if (r.orders[0].details[x].item_code.Contains("sqz187"))
                                        {
                                            md_order.product_id = 1;
                                            break;
                                        }
                                    }
                                    if(md_order.product_id == 0)
                                    {
                                        return Json(new { result = "FAIL" });
                                    }
                                    var strAddre = r.orders[0].receiver_address;
                                    var indexAddre1 = strAddre.IndexOf(" ");
                                    var indexAddre2 = strAddre.IndexOf(" ", indexAddre1 + 1);
                                    var indexAddre3 = strAddre.IndexOf(" ", indexAddre2 + 1) + 1;
                                    var strReceiver_address = strAddre.Substring(indexAddre3, strAddre.Length - indexAddre3);
                                    var strArea = r.orders[0].receiver_area;
                                    var strReceiver_area = strArea;
                                    if (strArea == null)
                                    {
                                        strReceiver_area = strAddre.Substring(0, indexAddre3 - 1);
                                        strReceiver_area = strReceiver_area.Replace(" ", "-");
                                    }
                                    string[] t = strReceiver_area.Split('-');
                                    var findT = (t.Length - 1);
                                    if (findT < 2)
                                    {
                                        strReceiver_area = strReceiver_area + "-";
                                    }
                                    md_order.order_code = r.orders[0].platform_code;
                                    md_order.receiver_date = r.orders[0].createtime.Date;
                                    md_order.order_status = 0;
                                    if (r.orders[0].deliverys.Count != 0)
                                    {
                                        md_order.express_information = r.orders[0].deliverys[0].express_name + r.orders[0].deliverys[0].mail_no;
                                    }
                                    else
                                    {
                                        md_order.express_information = "";
                                    }
                                    md_order.remark = r.orders[0].buyer_memo;
                                    md_order.receiver_address = strReceiver_address;
                                    md_order.receiver_area = strReceiver_area;
                                    md_order.upload_status = 1;
                                    md_order.receiver_tel = r.orders[0].receiver_mobile;
                                    md_order.vip_code = r.orders[0].vip_code;
                                    md_order.receiver_times = 1;
                                    md_order.qty = (int)r.orders[0].qty / 3;
                                    md_order.amount = r.orders[0].amount;
                                    md_order.discount_fee = r.orders[0].discount_fee;
                                    md_order.payment_amount = r.orders[0].payment_amount;
                                    md_order.delivery_state = r.orders[0].delivery_state;
                                    md_order.payment = r.orders[0].payment;
                                    md_order.receiver_name = r.orders[0].receiver_name;
                                    md_db.MD_Order.Add(md_order);
                                    md_db.SaveChanges();
                                    md_order.parentOrder_id = md_order.Id;
                                    md_db.Entry(md_order).State = System.Data.Entity.EntityState.Modified;
                                    MD_Record logs = new MD_Record();
                                    logs.record_date = DateTime.Now;
                                    logs.record_type = "Create";
                                    logs.record_detail ="导入订单 :" + " " + md_order.order_code;
                                    logs.record_amount = 1;
                                    md_db.MD_Record.Add(logs);
                                }
                                else
                                {
                                    return Json(new { result = "ERROR" });
                                }
                            }
                            else
                            {
                                return Json(new { result = "NOTFOUND" });
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    streamWriter.Close();
                    MD_Record logs = new MD_Record();
                    try_times++;
                    if (try_times >= 5)
                    {
                        logs.record_date = DateTime.Now;
                        logs.record_type = "Fail";
                        logs.record_detail = "ErpOrder获取失败";
                        md_db.MD_Record.Add(logs);
                        md_db.SaveChanges();
                        try_times = 0;
                        return Json(new { result = "SYSTEMERROR" });
                    }
                    return getSingleErpOrder(platform_code);
                }
                md_db.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            return Json(new { result = "NOTFOUND" });
        }
        [HttpPost]
        public JsonResult getSingleHistoryErpOrder(string platform_code)
        {
            if (platform_code != "")
            {
                var md_order = md_db.MD_Order.SingleOrDefault(m => m.order_code == platform_code);
                string json = "{" +
                       "\"appkey\":\"" + AppId + "\"," +
                        "\"method\":\"gy.erp.trade.history.get\"," +
                        //"\"receiver_mobile\":\"" + platform_code + "\"," +
                        "\"platform_code\":\"" + platform_code + "\"," +
                        //"\"platform_code\":\"" + platform_code + "\"," +
                        "\"sessionkey\":\"" + SessionKey + "\"" +
                        "}";
                string signature = sign(json, AppSecret);
                string info = "{" +
                       "\"appkey\":\"" + AppId + "\"," +
                        "\"method\":\"gy.erp.trade.history.get\"," +
                        //"\"receiver_mobile\":\"" + platform_code + "\"," +
                        "\"platform_code\":\"" + platform_code + "\"," +
                        //"\"platform_code\":\"" + platform_code + "\"," +
                        "\"sessionkey\":\"" + SessionKey + "\"," +
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
                        orders_Result r = JsonConvert.DeserializeObject<orders_Result>(sb.ToString());
                        if (r.success)
                        {
                            if (r.orders.Count() != 0)
                            {
                                if (md_order == null)
                                {
                                    md_order = new MD_Order();
                                    for (int x = 0; x < r.orders[0].details.Count(); x++)
                                    {
                                        if (r.orders[0].details[x].note != null)
                                        {
                                            if (r.orders[0].details[x].note.Length != 0)
                                            {
                                                if (r.orders[0].details[x].note.Contains("sqz333"))
                                                {
                                                    md_order.product_id = 1;
                                                    break;
                                                }
                                                else if (r.orders[0].details[x].note.Contains("sqz444"))
                                                {
                                                    md_order.product_id = 3;
                                                    break;
                                                }
                                            }
                                            if (r.orders[0].details[x].item_code.Contains("sqz187"))
                                            {
                                                md_order.product_id = 1;
                                                break;
                                            }
                                        }
                                        if (r.orders[0].details[x].item_code.Contains("sqz187"))
                                        {
                                            md_order.product_id = 1;
                                            break;
                                        }
                                    }
                                    if (md_order.product_id == 0)
                                    {
                                        return Json(new { result = "FAIL" });
                                    }
                                    var strAddre = r.orders[0].receiver_address;
                                    var indexAddre1 = strAddre.IndexOf(" ");
                                    var indexAddre2 = strAddre.IndexOf(" ", indexAddre1 + 1);
                                    var indexAddre3 = strAddre.IndexOf(" ", indexAddre2 + 1) + 1;
                                    var strReceiver_address = strAddre.Substring(indexAddre3, strAddre.Length - indexAddre3);
                                    var strArea = r.orders[0].receiver_area;
                                    var strReceiver_area = strArea;
                                    if (strArea == null)
                                    {
                                        strReceiver_area = strAddre.Substring(0, indexAddre3 - 1);
                                        strReceiver_area = strReceiver_area.Replace(" ", "-");
                                    }
                                    string[] t = strReceiver_area.Split('-');
                                    var findT = (t.Length - 1);
                                    if (findT < 2)
                                    {
                                        strReceiver_area = strReceiver_area + "-";
                                    }
                                    md_order.order_code = r.orders[0].platform_code;
                                    md_order.receiver_date = r.orders[0].createtime.Date;
                                    md_order.order_status = 0;
                                    if (r.orders[0].deliverys.Count != 0)
                                    {
                                        md_order.express_information = r.orders[0].deliverys[0].express_name + r.orders[0].deliverys[0].mail_no;
                                    }
                                    else
                                    {
                                        md_order.express_information = "";
                                    }
                                    md_order.remark = r.orders[0].buyer_memo;
                                    md_order.receiver_address = strReceiver_address;
                                    md_order.receiver_area = strReceiver_area;
                                    md_order.upload_status = 1;
                                    md_order.receiver_tel = r.orders[0].receiver_mobile;
                                    md_order.vip_code = r.orders[0].vip_code;
                                    md_order.receiver_times = 1;
                                    md_order.qty = (int)r.orders[0].qty / 3;
                                    md_order.amount = r.orders[0].amount;
                                    md_order.discount_fee = r.orders[0].discount_fee;
                                    md_order.payment_amount = r.orders[0].payment_amount;
                                    md_order.delivery_state = r.orders[0].delivery_state;
                                    md_order.payment = r.orders[0].payment;
                                    md_order.receiver_name = r.orders[0].receiver_name;
                                    md_db.MD_Order.Add(md_order);
                                    md_db.SaveChanges();
                                    md_order.parentOrder_id = md_order.Id;
                                    md_db.Entry(md_order).State = System.Data.Entity.EntityState.Modified;
                                    MD_Record logs = new MD_Record();
                                    logs.record_date = DateTime.Now;
                                    logs.record_type = "Create";
                                    logs.record_detail = "导入订单 :" + " " + md_order.order_code;
                                    logs.record_amount = 1;
                                    md_db.MD_Record.Add(logs);
                                }
                                else
                                {
                                    return Json(new { result = "ERROR" });
                                }
                            }
                            else
                            {
                                return Json(new { result = "NOTFOUND" });
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    streamWriter.Close();
                    MD_Record logs = new MD_Record();
                    try_times++;
                    if (try_times >= 5)
                    {
                        logs.record_date = DateTime.Now;
                        logs.record_type = "Fail";
                        logs.record_detail = "ErpOrder获取失败";
                        md_db.MD_Record.Add(logs);
                        md_db.SaveChanges();
                        try_times = 0;
                        return Json(new { result = "SYSTEMERROR" });
                    }
                    return getSingleErpOrder(platform_code);
                }
                md_db.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            return Json(new { result = "NOTFOUND" });
        }
        // 生成子订单
        public ActionResult CreateSubOrders(int order_id)
        {
            var order = md_db.MD_Order.SingleOrDefault(m => m.Id == order_id);
            return PartialView(order);
        }
        [HttpPost]
        public ActionResult CreateSubOrders(MD_Order model, int order_qty, int times, int product_id)
        {
            var order = md_db.MD_Order.SingleOrDefault(m => m.Id == model.Id);
            for (int i = 1; i < times + 1; i++)
            {
                var subOrder = new MD_Order();
                subOrder.order_code = "MD" + order.order_code + "-" + i;
                subOrder.receiver_date = order.receiver_date.Value.AddDays(+i * 30);
                subOrder.order_status = 0;
                subOrder.remark = order.remark;
                subOrder.receiver_address = order.receiver_address;
                subOrder.receiver_tel = order.receiver_tel;
                subOrder.vip_code = order.vip_code;
                subOrder.receiver_area = order.receiver_area;
                subOrder.receiver_times = i + 1;
                subOrder.qty = order_qty;
                subOrder.product_id = product_id;
                subOrder.parentOrder_id = order.Id;
                subOrder.receiver_name = order.receiver_name;
                subOrder.express_information = "";
                md_db.MD_Order.Add(subOrder);
            }
            order.createSub_status = 1;
            md_db.Entry(order).State = System.Data.Entity.EntityState.Modified;
            MD_Record logs = new MD_Record();
            logs.record_date = DateTime.Now;
            logs.record_type = "CreateSubOrders";
            logs.record_detail = order.order_code + " 新增复购订单";
            logs.record_amount = times;
            md_db.MD_Record.Add(logs);
            md_db.SaveChanges();
            return Json(new { result = "SUCCESS" });
        }
        

    }
}