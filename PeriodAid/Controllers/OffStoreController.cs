using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.Data.OleDb;
using System.Data;

using PagedList;
using PeriodAid.Models;
using PeriodAid.Filters;
using System.Threading.Tasks;

namespace PeriodAid.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OffStoreController : Controller
    {
        // GET: OffStore
        OfflineSales _offlineDB = new OfflineSales();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public OffStoreController()
        {

        }

        public OffStoreController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        // Origin:Off_Store_main
        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public ActionResult StoreIndex()
        {
            return View();
        }
        // Origin: Off_Store_ajaxlist
        /// <summary>
        /// 首页列表局部页
        /// </summary>
        /// <param name="page"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public PartialViewResult StoreListPartial(int? page, string query)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            int _page = page ?? 1;
            if (query == null || query == "")
            {
                var list = (from m in _offlineDB.Off_Store
                            where m.Off_StoreSystem.Off_System_Id == user.DefaultSystemId
                            orderby m.Id descending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
            else
            {
                var list = (from m in _offlineDB.Off_Store
                            where (m.StoreName.Contains(query) || m.Address.Contains(query))
                            && m.Off_StoreSystem.Off_System_Id == user.DefaultSystemId
                            orderby m.Id descending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
        }
        // Origin Off_CreateStore
        public ActionResult CreateStorePartial()
        {
            //var user = UserManager.
            var store = new Off_Store();
            var user = UserManager.FindById(User.Identity.GetUserId());
            var QD = from m in _offlineDB.Off_StoreSystem
                     where m.Off_System_Id == user.DefaultSystemId
                     select m;
            ViewBag.QD = new SelectList(QD, "Id", "SystemName");
            return PartialView(store);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CreateStorePartial(Off_Store model)
        {
            if (ModelState.IsValid)
            {
                Off_Store item = new Off_Store();
                if (TryUpdateModel(item))
                {
                    //var user = UserManager.FindById(User.Identity.GetUserId());
                    item.UploadTime = DateTime.Now;
                    item.UploadUser = User.Identity.Name;
                    _offlineDB.Off_Store.Add(item);
                    _offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return PartialView(model);
            }
        }

        // Origin: Off_DeleteStore
        [HttpPost]
        public ActionResult DeleteStoreAjax(int id)
        {
            var item = _offlineDB.Off_Store.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_StoreSystem.Off_System_Id == user.DefaultSystemId)
                {
                    try
                    {
                        _offlineDB.Off_Store.Remove(item);
                        _offlineDB.SaveChanges();
                        return Json(new { result = "SUCCESS" });
                    }
                    catch
                    {
                        return Json(new { result = "FAIL" });
                    }
                }
                else
                {
                    return Json(new { result = "UNAUTHRIZED" });
                }
            }
            return Json(new { result = "FAIL" });
        }

        // Origin: Off_Store_AreaChange_batch
        [HttpPost]
        public JsonResult ChangeStoreAreaBatchAjax(string ids, string modify_area)
        {
            try
            {
                string sql = "UPDATE Off_Store SET Region = '" + modify_area + "' where Id in (" + ids + ")";
                _offlineDB.Database.ExecuteSqlCommand(sql);
                _offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
        }
        
        [SettingFilter(SettingName = "GENERAL")]
        //上传店铺信息["删除"]
        /*public ActionResult UploadStore()
        {
            return PartialView();
        }
        [HttpPost]
        [SettingFilter(SettingName = "GENERAL")]
        public async Task<ActionResult> UploadStore(FormCollection form)
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
                    List<Excel_DataMessage> result = await UploadStoreByExcelAsync(filename, messageList);
                }
            }
            else
            {
                messageList.Add(new Excel_DataMessage(0, "文件上传错误", true));
            }
            return View("UploadResult", messageList);
        }*/

        // Origin: Ajax_EditStore
        public PartialViewResult EditStorePartial(int id)
        {
            var item = _offlineDB.Off_Store.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_StoreSystem.Off_System_Id == user.DefaultSystemId)
                {
                    var QD = from m in _offlineDB.Off_StoreSystem
                             where m.Off_System_Id == user.DefaultSystemId
                             select m;
                    ViewBag.QD = new SelectList(QD, "Id", "SystemName");
                    return PartialView(item);
                }
            }
            return PartialView("ErrorPartial");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditStorePartial(int id, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                var item = new Off_Store();
                if (TryUpdateModel(item))
                {
                    item.UploadTime = DateTime.Now;
                    item.UploadUser = User.Identity.Name;
                    _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    _offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                else
                {
                    ModelState.AddModelError("", "错误");
                    return PartialView(item);
                }
            }
            else
            {
                return PartialView("ErrorPartial");
            }
        }

        public ActionResult StoreMap()
        {
            return View();
        }
        // 地图里的店铺列表
        // Origin: JsonStoreList
        [HttpPost]
        public JsonResult StoreMapDetailsAjax(int storesystemId)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in _offlineDB.Off_Store
                       where m.Latitude != "" && m.Longitude != "" && m.Off_StoreSystemId == storesystemId
                       orderby m.Id descending
                       select new { StoreName = m.StoreName, StoreSystem = m.Off_StoreSystemId, Address = m.Address, Longitude = m.Longitude, Latitude = m.Latitude };
            return Json(new { result = "SUCCESS", list = list }, JsonRequestBehavior.AllowGet);
        }

        // Origin: analyseExcel_StoreTable["删除"]
        /*private async Task<List<Excel_DataMessage>> UploadStoreByExcelAsync(string filename, List<Excel_DataMessage> messageList)
        {
            try
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
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
                        var exist_item = _offlineDB.Off_Store.SingleOrDefault(m => m.StoreName == storename && m.Off_System_Id == user.DefaultSystemId);
                        if (exist_item != null)
                        {
                            // 更新数据
                            exist_item.StoreSystem = dr["渠道"].ToString();
                            exist_item.StoreName = dr["店铺名称"].ToString();
                            exist_item.Distributor = dr["经销商"].ToString();
                            exist_item.Region = dr["区域"].ToString();
                            exist_item.Address = dr["详细地址"].ToString();
                            exist_item.Longitude = dr["经度"].ToString();
                            exist_item.Latitude = dr["纬度"].ToString();
                            exist_item.UploadTime = DateTime.Now;
                            exist_item.UploadUser = User.Identity.Name;
                            messageList.Add(new Excel_DataMessage(i, "数据修改成功", false));
                        }
                        else
                        {
                            // 添加数据
                            Off_Store store = new Off_Store()
                            {
                                StoreSystem = dr["渠道"].ToString(),
                                StoreName = dr["店铺名称"].ToString(),
                                Distributor = dr["经销商"].ToString(),
                                Region = dr["区域"].ToString(),
                                Address = dr["详细地址"].ToString(),
                                Longitude = dr["经度"].ToString(),
                                Latitude = dr["纬度"].ToString(),
                                UploadTime = DateTime.Now,
                                UploadUser = User.Identity.Name,
                                Off_System_Id = user.DefaultSystemId
                            };
                            _offlineDB.Off_Store.Add(store);
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
                    await _offlineDB.SaveChangesAsync();
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
        [SettingFilter(SettingName = "GENERAL")]
        public ActionResult UploadResult()
        {
            return View();
        }*/

        public ActionResult StoreSystemList()
        {
            return View();
        }
        // Origin: Off_Store_ajaxlist
        /// <summary>
        /// 首页列表局部页
        /// </summary>
        /// <param name="page"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public PartialViewResult StoreSystemListPartial(int? page, string query)
        {
           var user = UserManager.FindById(User.Identity.GetUserId());
            int _page = page ?? 1;
            if (query == null || query == "")
            {
                var list = (from m in _offlineDB.Off_StoreSystem
                            where m.Off_System_Id == user.DefaultSystemId
                            orderby m.Id descending
                            select m).ToPagedList(_page, 2);
                return PartialView(list);
            }
            else
            {
                var list = (from m in _offlineDB.Off_StoreSystem
                            where (m.SystemName.Contains(query))
                            && m.Off_System_Id == user.DefaultSystemId
                            orderby m.Id descending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
        }
        // Origin Off_CreateStore
        public ActionResult CreateStoreSystemPartial()
        {
            var model = new Off_StoreSystem();
            var user = UserManager.FindById(User.Identity.GetUserId());
            var regionlist = _offlineDB.Off_System_Setting.SingleOrDefault(m => m.Off_System_Id == user.DefaultSystemId && m.SettingName == "AreaList");
            if (regionlist != null)
            {
                string[] regionarray = regionlist.SettingValue.Split(',');
                List<Object> attendance = new List<Object>();
                foreach (var i in regionarray)
                {
                    attendance.Add(new { Key = i, Value = i });
                }
                ViewBag.Regionlist = new SelectList(attendance, "Key", "Value");
                return PartialView(model);
            }
            else
                return PartialView("ErrorPartial");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CreateStoreSystemPartial(Off_StoreSystem model)
        {
            if (ModelState.IsValid)
            {
                Off_StoreSystem item = new Off_StoreSystem();
                if (TryUpdateModel(item))
                {
                    var user = UserManager.FindById(User.Identity.GetUserId());
                    //var user = UserManager.FindById(User.Identity.GetUserId());
                    item.Off_System_Id = user.DefaultSystemId;
                    _offlineDB.Off_StoreSystem.Add(item);
                    _offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                var user = UserManager.FindById(User.Identity.GetUserId());
                var regionlist = _offlineDB.Off_System_Setting.SingleOrDefault(m => m.Off_System_Id == user.DefaultSystemId && m.SettingName == "AreaList");
                if (regionlist != null)
                {
                    string[] regionarray = regionlist.SettingValue.Split(',');
                    List<Object> attendance = new List<Object>();
                    foreach (var i in regionarray)
                    {
                        attendance.Add(new { Key = i, Value = i });
                    }
                    ViewBag.Regionlist = new SelectList(attendance, "Key", "Value");
                    return PartialView(model);
                }
                else
                    return PartialView("ErrorPartial");
            }
        }
        // Origin: Ajax_EditStore
        public PartialViewResult EditStoreSystemPartial(int id)
        {
            var item = _offlineDB.Off_StoreSystem.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_System_Id == user.DefaultSystemId)
                {
                    var regionlist = _offlineDB.Off_System_Setting.SingleOrDefault(m => m.Off_System_Id == user.DefaultSystemId && m.SettingName == "AreaList");
                    if (regionlist != null)
                    {
                        string[] regionarray = regionlist.SettingValue.Split(',');
                        List<Object> attendance = new List<Object>();
                        foreach (var i in regionarray)
                        {
                            attendance.Add(new { Key = i, Value = i });
                        }
                        ViewBag.Regionlist = new SelectList(attendance, "Key", "Value");
                        return PartialView(item);
                    }
                    else
                        return PartialView("ErrorPartial");
                }
            }
            return PartialView("ErrorPartial");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditStoreSystemPartial(Off_StoreSystem model)
        {
            if (ModelState.IsValid)
            {
                var item = new Off_StoreSystem();
                if (TryUpdateModel(item))
                {
                    

                    _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    _offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                else
                {
                    ModelState.AddModelError("", "错误");
                    var user = UserManager.FindById(User.Identity.GetUserId());
                    var regionlist = _offlineDB.Off_System_Setting.SingleOrDefault(m => m.Off_System_Id == user.DefaultSystemId && m.SettingName == "AreaList");
                    if (regionlist != null)
                    {
                        string[] regionarray = regionlist.SettingValue.Split(',');
                        List<Object> attendance = new List<Object>();
                        foreach (var i in regionarray)
                        {
                            attendance.Add(new { Key = i, Value = i });
                        }
                        ViewBag.Regionlist = new SelectList(attendance, "Key", "Value");
                        return PartialView(model);
                    }
                    else
                        return PartialView("ErrorPartial");
                }
            }
            else
            {
                return PartialView("ErrorPartial");
            }
        }
        // 编辑模板
        public PartialViewResult EditProductListPartial(int id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var model = _offlineDB.Off_StoreSystem.SingleOrDefault(m => m.Id == id);
            var productlist = from m in _offlineDB.Off_Product
                              where m.Off_System_Id == user.DefaultSystemId
                              &&
                              m.status>=0
                              select m;
            ViewBag.ProductList = productlist;
            return PartialView(model);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EditProductListPartial(Off_StoreSystem model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                Off_StoreSystem item = new Off_StoreSystem();
                if (TryUpdateModel(item))
                {
                    item.ProductList = form["ProductList"].ToString();
                    _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    _offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return PartialView("ErrorPartial");

            }
            else
            {
                ModelState.AddModelError("", "错误");
                var user = UserManager.FindById(User.Identity.GetUserId());
                var productlist = from m in _offlineDB.Off_Product
                                  where m.Off_System_Id == user.DefaultSystemId
                                  select m;
                ViewBag.ProductList = productlist;
                return PartialView(model);
            }
        }
    }
}