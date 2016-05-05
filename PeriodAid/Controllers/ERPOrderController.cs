using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PeriodAid.Models;
using PeriodAid.DAL;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using PagedList;
using System.Data.OleDb;
using System.Data;

namespace PeriodAid.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ERPOrderController : Controller
    {
        //OfflineSales offlineDB = new OfflineSales();
        ERPOrderDataContext erpdb = new ERPOrderDataContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public ERPOrderController()
        {

        }

        public ERPOrderController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: ERPOrder
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Download_Order_List()
        {
            return View();
        }
        public ActionResult Download_Order_List_Ajax()
        {
            var list = from m in erpdb.taskstatus
                       where m.type == 0
                       orderby m.id descending
                       select m;
            return PartialView(list);
        }

        public ActionResult Download_Member_List()
        {
            return View();
        }

        public ActionResult Download_Member_List_Ajax()
        {
            var list = from m in erpdb.taskstatus
                       where m.type == 1
                       orderby m.id descending
                       select m;
            return PartialView(list);
        }

        [HttpPost]
        public async Task<JsonResult> Download_Order_Start_Ajax(string st, string et)
        {
            ERPOrderUtilities util = new ERPOrderUtilities();
            await util.Download_ErpOrders(st, et);
            return Json(new { result = "SUCCESS" });
        }

        [HttpPost]
        public async Task<JsonResult> Download_Vips_Start_Ajax(string st, string et)
        {
            ERPOrderUtilities util = new ERPOrderUtilities();
            await util.Download_ERPVips(st, et);
            return Json(new { result = "SUCCESS" });
        }

        // 0330 会员标签设定
        public ActionResult Vip_Tag_Setup()
        {
            return View();
        }

        // 0330 会员标签创建
        public ActionResult Vip_Tag_Create()
        {
            var taglist = from m in erpdb.tags
                          select m;
            return View(taglist);
        }

        // 0330 会员标签上传
        public ActionResult Vip_Tag_Upload()
        {
            var taglist = from m in erpdb.tags
                          select new { Key = m.id, Value = m.name };
            ViewBag.SelectList = new SelectList(taglist, "Key", "Value");
            return View();
        }

        public async Task<ActionResult> teststores()
        {
            ERPOrderUtilities util = new ERPOrderUtilities();
            var stores = await util.getERPShops();
            var list = from m in stores
                       select new { Key = m.name, Value = m.name };
            ViewBag.SelectList = new SelectList(list, "Key", "Value");
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> setTagByOrderId(string orderids, int tagid)
        {
            ERPOrderUtilities util = new ERPOrderUtilities();
            var vipidlist = util.getVipIdsByOrderId(orderids);
            //return Content(vipidlist.Count() + "");
            int success = await util.setTags(vipidlist, tagid);
            return Content("成功：" + success);
        }

        [HttpPost]
        public async Task<ActionResult> setTagByVipName(string vipnames, int tagid)
        {
            ERPOrderUtilities util = new ERPOrderUtilities();
            var vipidlist = util.getVipIdsByVipName(vipnames);
            //return Content(vipidlist.Count() + "");
            int success = await util.setTags(vipidlist, tagid);
            return Content("成功：" + success);
        }

        public async Task<ActionResult> GetItems()
        {
            ERPOrderUtilities util = new ERPOrderUtilities();
            await util.Download_ERPItems();
            return Json(new { result = "SUCCESS" }, JsonRequestBehavior.AllowGet);
        }
        // 0422 店铺运营数据
        public ActionResult GenericData_List()
        {
            return View();
        }

        public PartialViewResult GenericData_List_Ajax(string storename, int? page)
        {
            int _page = page ?? 1;
            if (storename == null)
            {
                var list = (from m in erpdb.generic_data
                            orderby m.date descending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
            else
            {
                var list = (from m in erpdb.generic_data
                           where m.storename == storename
                           orderby m.date descending
                           select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
        }
        public ActionResult Create_GenericData()
        {
            generic_data data = new generic_data();
            return View(data);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create_GenericData(generic_data model)
        {
            if(ModelState.IsValid)
            {
                generic_data item = new generic_data();
                if (TryUpdateModel(item))
                {
                    erpdb.generic_data.Add(item);
                    erpdb.SaveChanges();
                    return RedirectToAction("GenericData_List");
                }
                else
                {
                    return View("Error");
                }
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return View(model);
            }
        }
        public ActionResult Edit_GenericData(int id)
        {
            try
            {
                var item = erpdb.generic_data.SingleOrDefault(m => m.id == id);
                if (item != null)
                {
                    return View(item);
                }
                else
                    return View("Error");
            }
            catch
            {
                return View("Error");
            }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit_GenericData(generic_data model)
        {
            if (ModelState.IsValid)
            {
                generic_data item = new generic_data();
                if (TryUpdateModel(item))
                {
                    erpdb.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    erpdb.SaveChanges();
                    return RedirectToAction("GenericData_List");
                }
                else
                {
                    return View("Error");
                }
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return View(model);
            }
        }
        [HttpPost]
        public ActionResult Delete_GenericData(int id)
        {
            try
            {
                var item = erpdb.generic_data.SingleOrDefault(m => m.id == id);
                if (item != null)
                {
                    erpdb.generic_data.Remove(item);
                    erpdb.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                else
                    return Json(new { result = "FAIL" });
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
        }

        public ActionResult GenericData_Statistic()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GenericData_Satistic_Ajax(string startdate, string enddate, string storename)
        {
            if (startdate == null || enddate == null)
            {
                var list = from m in erpdb.generic_data
                           where m.storename == storename
                           orderby m.date
                           select m;
                return Json(new { result = "SUCCESS", data = list });
            }
            else
            {
                try
                {
                    DateTime sd = Convert.ToDateTime(startdate);
                    DateTime ed = Convert.ToDateTime(enddate);
                    var list = from m in erpdb.generic_data
                               where m.storename == storename && m.date >= sd && m.date <= ed
                               orderby m.date
                               select m;
                    return Json(new { result = "SUCCESS", data = list });
                }
                catch
                {
                    return Json(new { result = "FAIL" });
                }

            }
        }
        // 0426 产品销售明细
        public ActionResult Product_Details_List()
        {
            return View();
        }
        public ActionResult Product_Details_List_Ajax(string storename, string date)
        {
            DateTime _date = Convert.ToDateTime(date);
            var list = from m in erpdb.product_details
                       where m.storename == storename && m.date == _date
                       select m;
            return PartialView(list);
        }
        public ActionResult Product_Details_Create()
        {
            product_details data = new product_details();
            return View(data);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Product_Details_Create(product_details model)
        {
            if (ModelState.IsValid)
            {
                product_details item = new product_details();
                if (TryUpdateModel(item))
                {
                    erpdb.product_details.Add(item);
                    erpdb.SaveChanges();
                    return RedirectToAction("Product_Details_List");
                }
                else
                {
                    return View("Error");
                }
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return View(model);
            }
        }
        public ActionResult Product_Details_Edit(int id)
        {
            try
            {
                var item = erpdb.product_details.SingleOrDefault(m => m.id == id);
                if (item != null)
                {
                    return View(item);
                }
                else
                    return View("Error");
            }
            catch
            {
                return View("Error");
            }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Product_Details_Edit(product_details model)
        {
            if (ModelState.IsValid)
            {
                product_details item = new product_details();
                if (TryUpdateModel(item))
                {
                    erpdb.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    erpdb.SaveChanges();
                    return RedirectToAction("Product_Details_List");
                }
                else
                {
                    return View("Error");
                }
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return View(model);
            }
        }
        [HttpPost]
        public ActionResult Product_Details_Delete(int id)
        {
            try
            {
                var item = erpdb.product_details.SingleOrDefault(m => m.id == id);
                if (item != null)
                {
                    erpdb.product_details.Remove(item);
                    erpdb.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                else
                    return Json(new { result = "FAIL" });
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
        }

        public ActionResult Product_Details_Statistic()
        {
            return View();
        }
        [HttpPost]
        public JsonResult Product_Details_Statistic_Ajax(string storename, string startdate, string enddate, string itemcode)
        {
            if (startdate == null || enddate == null)
            {
                var list = from m in erpdb.product_details
                           where m.storename == storename && m.item_code == itemcode
                           orderby m.date
                           select m;
                return Json(new { result = "SUCCESS", data = list });
            }
            else
            {
                try
                {
                    DateTime sd = Convert.ToDateTime(startdate);
                    DateTime ed = Convert.ToDateTime(enddate);
                    var list = from m in erpdb.product_details
                               where m.storename == storename && m.date >= sd && m.date <= ed && m.item_code== itemcode
                               orderby m.date
                               select m;
                    return Json(new { result = "SUCCESS", data = list });
                }
                catch
                {
                    return Json(new { result = "FAIL" });
                }

            }
        }
        // 0426 产品运营数据
        public ActionResult Product_Generic_Data_List()
        {
            return View();
        }
        public ActionResult Product_Generic_Data_List_Ajax(string storename, string date)
        {
            DateTime _date = Convert.ToDateTime(date);
            var list = from m in erpdb.product_generic_data
                       where m.storename == storename && m.date == _date
                       select m;
            return PartialView(list);
        }
        public ActionResult Product_Generic_Data_Create()
        {
            product_generic_data data = new product_generic_data();
            return View(data);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Product_Generic_Data_Create(product_details model)
        {
            if (ModelState.IsValid)
            {
                product_generic_data item = new product_generic_data();
                if (TryUpdateModel(item))
                {
                    erpdb.product_generic_data.Add(item);
                    erpdb.SaveChanges();
                    return RedirectToAction("Product_Generic_Data_List");
                }
                else
                {
                    return View("Error");
                }
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return View(model);
            }
        }
        public ActionResult Product_Generic_Data_Edit(int id)
        {
            try
            {
                var item = erpdb.product_generic_data.SingleOrDefault(m => m.id == id);
                if (item != null)
                {
                    return View(item);
                }
                else
                    return View("Error");
            }
            catch
            {
                return View("Error");
            }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Product_Generic_Data_Edit(product_generic_data model)
        {
            if (ModelState.IsValid)
            {
                product_generic_data item = new product_generic_data();
                if (TryUpdateModel(item))
                {
                    erpdb.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    erpdb.SaveChanges();
                    return RedirectToAction("Product_Generic_Data_List");
                }
                else
                {
                    return View("Error");
                }
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return View(model);
            }
        }
        [HttpPost]
        public ActionResult Product_Generic_Data_Delete(int id)
        {
            try
            {
                var item = erpdb.product_generic_data.SingleOrDefault(m => m.id == id);
                if (item != null)
                {
                    erpdb.product_generic_data.Remove(item);
                    erpdb.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                else
                    return Json(new { result = "FAIL" });
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
        }

        public ActionResult Product_Generic_Data_Statistic()
        {
            return View();
        }
        [HttpPost]
        public JsonResult Product_Generic_Data_Statistic_Ajax(string storename, string startdate, string enddate, string itemcode)
        {
            if (startdate == null || enddate == null)
            {
                var list = from m in erpdb.product_generic_data
                           where m.storename == storename && m.item_code == itemcode
                           orderby m.date
                           select m;
                return Json(new { result = "SUCCESS", data = list });
            }
            else
            {
                try
                {
                    DateTime sd = Convert.ToDateTime(startdate);
                    DateTime ed = Convert.ToDateTime(enddate);
                    var list = from m in erpdb.product_generic_data
                               where m.storename == storename && m.date >= sd && m.date <= ed && m.item_code == itemcode
                               orderby m.date
                               select m;
                    return Json(new { result = "SUCCESS", data = list });
                }
                catch
                {
                    return Json(new { result = "FAIL" });
                }

            }
        }
        #region 上传商品基本信息
        public ActionResult UploadProductDetails()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UploadStoreProductDetails(FormCollection form)
        {
            var file = Request.Files[0];
            List<Excel_DataMessage> messageList = new List<Excel_DataMessage>();
            string time_ticks = DateTime.Now.Ticks.ToString();
            if (file != null)
            {
                //文件不得大于500K
                if (file.ContentLength > 1024 * 500)
                {
                    messageList.Add(new Excel_DataMessage(0, "文件大于500K", true));
                }
                else if (file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    messageList.Add(new Excel_DataMessage(0, "文件类型错误", true));
                }
                else
                {
                    string folder = HttpContext.Server.MapPath("~/Content/xlsx/");
                    string filename = time_ticks + file.FileName.Substring(file.FileName.LastIndexOf('.'));
                    file.SaveAs(folder + filename);
                    List<Excel_DataMessage> result = analyseExcel_StoreTable(filename, messageList);
                }
            }
            else
            {
                messageList.Add(new Excel_DataMessage(0, "文件上传错误", true));
            }
            return View("UploadResult", messageList);
        }
        private List<Excel_DataMessage> analyseExcel_StoreTable(string filename, List<Excel_DataMessage> messageList)
        {
            try
            {
                string folder = HttpContext.Server.MapPath("~/Content/xlsx/");
                string strConn = "Provider=Microsoft.Ace.OleDb.12.0;" + "data source=" + folder + filename + ";Extended Properties='Excel 12.0; HDR=1; IMEX=1'"; //此连接可以操作.xls与.xlsx文件
                OleDbConnection conn = new OleDbConnection(strConn);
                conn.Open();
                DataSet ds = new DataSet();
                OleDbDataAdapter odda = new OleDbDataAdapter(string.Format("SELECT * FROM [{0}]", "Sheet1$"), conn);                    //("select * from [Sheet1$]", conn);
                odda.Fill(ds, "[Sheet1$]");
                conn.Close();
                DataTable dt = ds.Tables[0];
                int i = 0;
                bool result_flag = true;
                foreach (DataRow dr in dt.Rows)
                {
                    i++;
                    try
                    {
                        // 判断是否含有数据
                        string storename = dr["店铺名称"].ToString();
                        DateTime date = Convert.ToDateTime(ExcelOperation.ConvertDateTime(dr, "月份"));
                        string item_code = dr["商品编码"].ToString();
                        var exist_item = erpdb.product_details.SingleOrDefault(m => m.storename == storename && m.date == date && m.item_code == item_code);
                        if (exist_item != null)
                        {
                            // 更新数据
                            exist_item.storename = storename;
                            exist_item.date = date;
                            exist_item.item_code = item_code;
                            exist_item.simple_name = dr["商品简称"].ToString();
                            exist_item.sales_count = Convert.ToInt32(ExcelOperation.ConvertInt(dr, "销售数量"));
                            exist_item.sales_amount = Convert.ToDecimal(dr["销售金额"]);
                            messageList.Add(new Excel_DataMessage(i, "数据修改成功", false));
                        }
                        else
                        {
                            // 添加数据
                            product_details details = new product_details()
                            {
                                storename = storename,
                            date = date,
                            item_code = item_code,
                            simple_name = dr["商品简称"].ToString(),
                            sales_count = Convert.ToInt32(ExcelOperation.ConvertInt(dr, "销售数量")),
                            sales_amount = Convert.ToDecimal(dr["销售金额"])
                        };
                            erpdb.product_details.Add(details);
                            messageList.Add(new Excel_DataMessage(i, "数据添加成功", false));
                        }
                    }
                    catch (Exception e)
                    {
                        result_flag = false;
                        messageList.Add(new Excel_DataMessage(i, "格式错误或列名不存在," + e.InnerException, true));
                    }
                }
                if (result_flag)
                {
                    erpdb.SaveChanges();
                    messageList.Add(new Excel_DataMessage(0, "数据存储成功", false));
                }
                else
                {
                    messageList.Add(new Excel_DataMessage(0, "数据行发生错误，未保存", true));

                }
            }
            catch (Exception e)
            {
                messageList.Add(new Excel_DataMessage(-1, "表格格式错误" + e.ToString(), true));
            }
            return messageList;
        }
        #endregion
        #region 上传商品基本信息
        public ActionResult UploadProductGenericData()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UploadProductGenericData(FormCollection form)
        {
            var file = Request.Files[0];
            List<Excel_DataMessage> messageList = new List<Excel_DataMessage>();
            string time_ticks = DateTime.Now.Ticks.ToString();
            if (file != null)
            {
                //文件不得大于500K
                if (file.ContentLength > 1024 * 500)
                {
                    messageList.Add(new Excel_DataMessage(0, "文件大于500K", true));
                }
                else if (file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    messageList.Add(new Excel_DataMessage(0, "文件类型错误", true));
                }
                else
                {
                    string folder = HttpContext.Server.MapPath("~/Content/xlsx/");
                    string filename = time_ticks + file.FileName.Substring(file.FileName.LastIndexOf('.'));
                    file.SaveAs(folder + filename);
                    List<Excel_DataMessage> result = analyseExcel_GenericTable(filename, messageList);
                }
            }
            else
            {
                messageList.Add(new Excel_DataMessage(0, "文件上传错误", true));
            }
            return View("UploadResult", messageList);
        }
        private List<Excel_DataMessage> analyseExcel_GenericTable(string filename, List<Excel_DataMessage> messageList)
        {
            try
            {
                string folder = HttpContext.Server.MapPath("~/Content/xlsx/");
                string strConn = "Provider=Microsoft.Ace.OleDb.12.0;" + "data source=" + folder + filename + ";Extended Properties='Excel 12.0; HDR=1; IMEX=1'"; //此连接可以操作.xls与.xlsx文件
                OleDbConnection conn = new OleDbConnection(strConn);
                conn.Open();
                DataSet ds = new DataSet();
                OleDbDataAdapter odda = new OleDbDataAdapter(string.Format("SELECT * FROM [{0}]", "Sheet1$"), conn);                    //("select * from [Sheet1$]", conn);
                odda.Fill(ds, "[Sheet1$]");
                conn.Close();
                DataTable dt = ds.Tables[0];
                int i = 0;
                bool result_flag = true;
                foreach (DataRow dr in dt.Rows)
                {
                    i++;
                    try
                    {
                        // 判断是否含有数据
                        string storename = dr["店铺名称"].ToString();
                        DateTime date = Convert.ToDateTime(ExcelOperation.ConvertDateTime(dr, "月份"));
                        string item_code = dr["商品编码"].ToString();
                        var exist_item = erpdb.product_generic_data.SingleOrDefault(m => m.storename == storename && m.date == date && m.item_code == item_code);
                        if (exist_item != null)
                        {
                            // 更新数据
                            exist_item.storename = storename;
                            exist_item.date = date;
                            exist_item.item_code = item_code;
                            exist_item.simple_name = dr["商品简称"].ToString();
                            exist_item.uv = ExcelOperation.ConvertInt(dr, "访客数");
                            exist_item.pv = ExcelOperation.ConvertInt(dr, "浏览量");
                            exist_item.order_count = ExcelOperation.ConvertInt(dr, "订单数");
                            exist_item.order_amount = ExcelOperation.ConvertDecimal(dr, "销售金额");
                            exist_item.product_unit = ExcelOperation.ConvertInt(dr, "下单件数");
                            exist_item.convertion = ExcelOperation.ConvertInt(dr, "转化率");
                            messageList.Add(new Excel_DataMessage(i, "数据修改成功", false));
                        }
                        else
                        {
                            // 添加数据
                            product_generic_data details = new product_generic_data()
                            {
                                storename = storename,
                                date = date,
                                item_code = item_code,
                                simple_name = dr["商品简称"].ToString(),
                                uv = ExcelOperation.ConvertInt(dr, "访客数"),
                                pv = ExcelOperation.ConvertInt(dr, "浏览量"),
                                order_count = ExcelOperation.ConvertInt(dr, "订单数"),
                                order_amount = ExcelOperation.ConvertDecimal(dr, "销售金额"),
                                product_unit = ExcelOperation.ConvertInt(dr, "下单件数"),
                                convertion = ExcelOperation.ConvertInt(dr, "转化率")
                            };
                            erpdb.product_generic_data.Add(details);
                            messageList.Add(new Excel_DataMessage(i, "数据添加成功", false));
                        }
                    }
                    catch (Exception e)
                    {
                        result_flag = false;
                        messageList.Add(new Excel_DataMessage(i, "格式错误或列名不存在," + e.InnerException, true));
                    }
                }
                if (result_flag)
                {
                    erpdb.SaveChanges();
                    messageList.Add(new Excel_DataMessage(0, "数据存储成功", false));
                }
                else
                {
                    messageList.Add(new Excel_DataMessage(0, "数据行发生错误，未保存", true));

                }
            }
            catch (Exception e)
            {
                messageList.Add(new Excel_DataMessage(-1, "表格格式错误" + e.ToString(), true));
            }
            return messageList;
        }
    }
    #endregion
}
    

    
}