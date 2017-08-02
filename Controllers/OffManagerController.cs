﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

using PeriodAid.Models;
using PeriodAid.Filters;
using PagedList;
using System.IO;
using CsvHelper;

namespace PeriodAid.Controllers
{
    [Authorize(Roles ="Admin")]
    public class OffManagerController : Controller
    {
        OfflineSales _offlineDB = new OfflineSales();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public OffManagerController()
        {

        }

        public OffManagerController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: OffManager
        public ActionResult Index()
        {
            return View();
        }

        // Origin: Off_Manager_List(拆分Normal+Ajax),添加查询功能
        // 0310 管理员列表
        public ActionResult ManagerList()
        {
            return View();
        }
        public PartialViewResult ManagerListPartial(int? page, string query)
        {
            int _page = page ?? 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (query == null || query.Trim() == "")
            {
                var list = (from m in _offlineDB.Off_StoreManager
                           where m.Off_System_Id == user.DefaultSystemId && m.Status>=0
                           orderby m.Id descending
                           select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
            else
            {
                var list = (from m in _offlineDB.Off_StoreManager
                           where m.Off_System_Id == user.DefaultSystemId && m.NickName.Contains(query)
                           orderby m.Id descending
                           select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
        }


        // Origin: Off_Manager_UpdateSenior
        // 0310 升级为超级管理员
        [HttpPost]
        public JsonResult UpdateManagerToSeniorAjax(int id)
        {
            var manager = _offlineDB.Off_StoreManager.SingleOrDefault(m => m.Id == id);
            if (manager != null)
            {
                var currentuser = UserManager.FindById(User.Identity.GetUserId());
                if (manager.Off_System_Id == currentuser.DefaultSystemId)
                {
                    var user = UserManager.FindByName(manager.UserName);
                    UserManager.AddToRole(user.Id, "Senior");
                    manager.Status = 2;
                    _offlineDB.Entry(manager).State = System.Data.Entity.EntityState.Modified;
                    _offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                else
                {
                    return Json(new { result = "UNZUTHORIZED" });
                }
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }


        // Origin: Off_Manager_AddStore
        // 添加管理员管辖区域
        public ActionResult AddManagerArea(int id)
        {
            //var item = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Id == id);
            var manager = _offlineDB.Off_StoreManager.SingleOrDefault(m => m.Id == id);
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (manager != null)
            {
                if (manager.Off_System_Id == user.DefaultSystemId)
                {
                    ViewBag.StoreList = manager.Off_Store.OrderBy(m => m.StoreName);
                    var storesystem = from m in _offlineDB.Off_StoreSystem
                                      where m.Off_System_Id == user.DefaultSystemId
                                      select m;
                    ViewBag.SystemList = new SelectList(storesystem, "Id", "SystemName", storesystem.FirstOrDefault().Id);
                    //ViewBag.SystemId = manager.Off_System_Id;
                    return View(manager);
                }
                else
                {
                    return View("AuthorizeError");
                }
            }
            else
            {
                return View("Error");
            }
        }

        // Origin Off_GetStoreName
        [HttpPost]
        public JsonResult GetStoreNameAjax(int id)
        {
            var item = _offlineDB.Off_Store.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                
                return Json(new { result = "SUCCESS", data = new { id = item.Id, name = item.StoreName } });
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }

        //Origin: Off_Manager_AjaxAddStore
        // 添加门店
        [HttpPost]
        public ActionResult AddManagerAreaAjax(int id, string arr_list)
        {
            try
            {
                var manager = _offlineDB.Off_StoreManager.SingleOrDefault(m => m.Id == id);
                var user = UserManager.FindById(User.Identity.GetUserId());
                var currentlist = manager.Off_Store.Select(m => m.Id);
                string[] arr_temp = arr_list.Split(',');
                List<int> arr_int = new List<int>();
                foreach (var s in arr_temp)
                {
                    arr_int.Add(Convert.ToInt32(s));
                }
                var select_list = (from m in _offlineDB.Off_Store
                                   where arr_int.Contains(m.Id) && m.Off_StoreSystem.Off_System_Id == user.DefaultSystemId
                                   select m).Select(m => m.Id);
                var storelist = (from m in manager.Off_Store
                                 select m.Id).ToList();
                foreach (var item in storelist)
                {
                    var temp = _offlineDB.Off_Store.SingleOrDefault(m => m.Id == item);
                    manager.Off_Store.Remove(temp);
                }
                foreach (var item2 in select_list)
                {
                    manager.Off_Store.Add(_offlineDB.Off_Store.SingleOrDefault(m => m.Id == item2));
                }
                _offlineDB.Entry(manager).State = System.Data.Entity.EntityState.Modified;
                _offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
        }

        // 0314 降级为普通管理员
        // Origin: Off_Manager_ReduceManager
        [HttpPost]
        public JsonResult ReduceManagerAjax(int id)
        {
            var manager = _offlineDB.Off_StoreManager.SingleOrDefault(m => m.Id == id);
            if (manager != null)
            {
                var currentuser = UserManager.FindById(User.Identity.GetUserId());
                if (manager.Off_System_Id == currentuser.DefaultSystemId)
                {
                    var user = UserManager.FindByName(manager.UserName);
                    UserManager.RemoveFromRole(user.Id, "Senior");
                    manager.Status = 1;
                    _offlineDB.Entry(manager).State = System.Data.Entity.EntityState.Modified;
                    _offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                else
                    return Json(new { result = "UNAUTHORIZED" });
            }
            else
                return Json(new { result = "FAIL" });
        }
        public ActionResult EditManagerRoles(int id)
        {
            var manager = _offlineDB.Off_StoreManager.SingleOrDefault(m => m.Id == id);
            if (manager != null)
            {
                var currentuser = UserManager.FindById(User.Identity.GetUserId());
                if (manager.Off_System_Id == currentuser.DefaultSystemId)
                {
                    var user = UserManager.FindByName(manager.UserName);
                    var rolelist = new List<Object>();
                    rolelist.Add(new { Key = "Supervisor", Value = "督导" });
                    rolelist.Add(new { Key = "Manager", Value = "业务" });
                    rolelist.Add(new { Key = "Administrator", Value = "管理员" });
                    if (UserManager.IsInRole(user.Id, "Supervisor"))
                    {
                        ViewBag.RolesDropDownList = new SelectList(rolelist, "Key", "Value", "Supervisor");
                    }
                    else if (UserManager.IsInRole(user.Id, "Manager"))
                    {
                        ViewBag.RolesDropDownList = new SelectList(rolelist, "Key", "Value", "Manager");
                    }
                    else if (UserManager.IsInRole(user.Id, "Administrator"))
                    {
                        ViewBag.RolesDropDownList = new SelectList(rolelist, "Key", "Value", "Administrator");
                    }
                    else
                    {
                        ViewBag.RolesDropDownList = new SelectList(rolelist, "Key", "Value");
                    }
                    ViewBag.pid = id;
                    return PartialView();
                }
                return PartialView("Error");
            }
            return PartialView("Error");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EditManagerRoles(int id, FormCollection form)
        {
            var manager = _offlineDB.Off_StoreManager.SingleOrDefault(m => m.Id == id);
            if (manager != null)
            {
                var currentuser = UserManager.FindById(User.Identity.GetUserId());
                if (manager.Off_System_Id == currentuser.DefaultSystemId)
                {
                    var user = UserManager.FindByName(manager.UserName);
                    // 删除所有角色
                    //string[] roles = new string[] { "Supervisor", "Manager", "Administrator" };
                    UserManager.RemoveFromRole(user.Id,"Supervisor");
                    UserManager.RemoveFromRole(user.Id, "Manager");
                    UserManager.RemoveFromRole(user.Id, "Administrator");
                    if (form["role"] == "Supervisor")
                    {
                        UserManager.AddToRole(user.Id, "Supervisor");
                        manager.Status = 1;
                    }
                    else if (form["role"] == "Manager")
                    {
                        UserManager.AddToRole(user.Id, "Manager");
                        manager.Status = 2;
                    }
                    else if (form["role"] == "Administrator")
                    {
                        UserManager.AddToRole(user.Id, "Administrator");
                        manager.Status = 3;
                    }
                    _offlineDB.Entry(manager).State = System.Data.Entity.EntityState.Modified;
                    _offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                else
                {
                    return Content("FAIL");
                }
            }
            return Content("FAIL");
        }

        // 删除管理员
        [HttpPost]
        public JsonResult CancelManagerAjax(int id)
        {
            var manager = _offlineDB.Off_StoreManager.SingleOrDefault(m => m.Id == id);
            if (manager != null)
            {
                var currentuser = UserManager.FindById(User.Identity.GetUserId());
                if (manager.Off_System_Id == currentuser.DefaultSystemId)
                {
                    manager.Status = -1;
                    _offlineDB.Entry(manager).State = System.Data.Entity.EntityState.Modified;
                    _offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                else
                    return Json(new { result = "UNAUTHORIZED" });
            }
            else
                return Json(new { result = "FAIL" });
        }

        // Origin: Off_Manager_TaskList
        // 0314 督导工作汇报列表
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        public ActionResult TaskList()
        {
            return View();
        }
        // origin: Off_Manager_TaskList_Ajax
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        public PartialViewResult TaskListPartial(string query, int? status, int? page)
        {
            int _status = status ?? 0;
            int _page = page ?? 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (query == null)
            {
                var list = (from m in _offlineDB.Off_Manager_Task
                            where m.Status == _status && m.Off_System_Id == user.DefaultSystemId
                            orderby m.TaskDate descending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
            else
            {
                var list = (from m in _offlineDB.Off_Manager_Task
                            where m.Status == _status && m.Off_System_Id == user.DefaultSystemId
                            && m.NickName.Contains(query)
                            orderby m.TaskDate descending
                            select m).ToPagedList(_page, 20);
                return PartialView(list);
            }
        }

        // 0314 审核督导工作
        // Origin: Off_Manager_EditTask
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        public ActionResult EditTask(int id)
        {
            var item = _offlineDB.Off_Manager_Task.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_System_Id == user.DefaultSystemId)
                    return View(item);
                else
                    return View("AuthorizeError");
            }
            else
                return View("Error");
        }
        [HttpPost, ValidateAntiForgeryToken]
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        public ActionResult EditTask(Off_Manager_Task model)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                var item = new Off_Manager_Task();
                if (TryUpdateModel(item))
                {
                    item.Off_System_Id = user.DefaultSystemId;
                    item.Eval_User = User.Identity.Name;
                    item.Eval_Time = DateTime.Now;
                    item.Status = 1;
                    _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    _offlineDB.SaveChanges();
                    return RedirectToAction("TaskList");
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "错误");
                return View(model);
            }
        }

        // Origin: Off_Manager_CancelTask 
        // 0314 作废当日工作内容
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE"),HttpPost]
        public JsonResult CancelTaskAjax(int id)
        {
            var item = _offlineDB.Off_Manager_Task.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_System_Id == user.DefaultSystemId)
                {
                    item.Status = -1;
                    _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    _offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                else
                    return Json(new { result = "UNAUTHORIZED" });
            }
            return Json(new { result = "FAIL" });
        }

        // Origin: Off_Manager_Announcement_List
        // 0325 公告列表
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        public ActionResult AnnouncementList()
        {
            return View();
        }
        // Off_Manager_Announcement_List_Ajax
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        public ActionResult AnnouncementListPartial(int? page)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            int _page = page ?? 1;
            var list = (from m in _offlineDB.Off_Manager_Announcement
                        where m.Off_System_Id == user.DefaultSystemId
                        orderby m.Priority descending, m.FinishTime descending
                        select m).ToPagedList(_page, 20);
            ViewBag.SystemId = user.DefaultSystemId;
            return PartialView(list);
        }
        // 0325 添加公告
        // Origin: Off_Manager_Announcement_Create
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        public ActionResult CreateAnnouncement()
        {
            Off_Manager_Announcement model = new Off_Manager_Announcement();
            return View(model);
        }
        [HttpPost, ValidateAntiForgeryToken]
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        public ActionResult CreateAnnouncement(Off_Manager_Announcement model)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                Off_Manager_Announcement item = new Off_Manager_Announcement();
                if (TryUpdateModel(item))
                {
                    item.SubmitTime = DateTime.Now;
                    item.SubmitUser = User.Identity.Name;
                    item.Off_System_Id = user.DefaultSystemId;
                    _offlineDB.Off_Manager_Announcement.Add(item);
                    _offlineDB.SaveChanges();
                    return RedirectToAction("AnnouncementList");
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return View(model);
            }
        }

        // Origin: Off_Manager_Announcement_Edit
        // 0325 修改公告
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        public ActionResult EditAnnouncement(int id)
        {
            Off_Manager_Announcement model = _offlineDB.Off_Manager_Announcement.SingleOrDefault(m => m.Id == id);
            if (model != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if(model.Off_System_Id == user.DefaultSystemId)
                {
                    return View(model);
                }
                return View("AuthorizeError");
            }
            else
                return View("Error");
        }
        [HttpPost, ValidateAntiForgeryToken]
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        public ActionResult EditAnnouncement(Off_Manager_Announcement model)
        {
            if (ModelState.IsValid)
            {
                Off_Manager_Announcement item = new Off_Manager_Announcement();
                if (TryUpdateModel(item))
                {
                    item.SubmitTime = DateTime.Now;
                    item.SubmitUser = User.Identity.Name;
                    _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    _offlineDB.SaveChanges();
                    return RedirectToAction("AnnouncementList");
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return View(model);
            }
        }

        // 0325 删除公告
        // Origin:Off_Manager_Announcement_Delete_Ajax
        [HttpPost]
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        public ActionResult DeleteAnnouncementAjax(int id)
        {
            Off_Manager_Announcement model = _offlineDB.Off_Manager_Announcement.SingleOrDefault(m => m.Id == id);
            if (model != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (model.Off_System_Id == user.DefaultSystemId)
                {
                    _offlineDB.Off_Manager_Announcement.Remove(model);
                    _offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                else
                    return Json(new { result = "UNAUTHORIZED" });
            }
            return Json(new { result = "FAIL" });
        }

        // 0325 获取督导列表
        // Origin: Off_Manager_List_Ajax
        [HttpPost]
        public JsonResult ManagerListAjax()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in _offlineDB.Off_StoreManager
                       where m.Off_System_Id == user.DefaultSystemId
                       select new { UserName = m.UserName, NickName = m.NickName };
            return Json(new { result = "SUCCESS", managerlist = list });
        }

        // 0329 获取需求列表
        // Origin: Off_Manager_Request_List
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        public ActionResult RequestList()
        {
            return View();
        }
        // Origin Off_Manager_Request_List_Ajax
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        public ActionResult RequestListPartial(int? page)
        {
            var _page = page ?? 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = (from m in _offlineDB.Off_Manager_Request
                        where m.Status >= 0 && m.Off_Store.Off_StoreSystem.Off_System_Id == user.DefaultSystemId
                        orderby m.Status, m.Id descending
                        select m).ToPagedList(_page, 20);
            ViewBag.SystemId = user.DefaultSystemId;
            return PartialView(list);
        }

        // Origin: Off_Manager_Request_Edit
        // 0329 修改需求详情
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        public ActionResult EditRequest(int id)
        {
            Off_Manager_Request request = _offlineDB.Off_Manager_Request.SingleOrDefault(m => m.Id == id);
            if (request != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (request.Off_Store.Off_StoreSystem.Off_System_Id == user.DefaultSystemId)
                {
                    return PartialView(request);
                }
                else
                    return PartialView("AuthorizeErrorPartial");
            }
            else
                return PartialView("ErrorPartial");
        }
        [HttpPost, ValidateAntiForgeryToken]
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        public ActionResult EditRequest(Off_Manager_Request model)
        {
            if (ModelState.IsValid)
            {
                Off_Manager_Request item = new Off_Manager_Request();
                if (TryUpdateModel(item))
                {
                    item.Status = 2;
                    item.ReplyTime = DateTime.Now;
                    item.ReplyUser = User.Identity.Name;
                    _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    _offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return PartialView("Error");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return PartialView(model);
            }
        }

        // Off_Manager_Request_Dismiss_Ajax
        // 0329 驳回需求
        [HttpPost]
        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
        public ActionResult DismissRequestAjax(int id)
        {
            Off_Manager_Request request = _offlineDB.Off_Manager_Request.SingleOrDefault(m => m.Id == id);
            if (request != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (request.Off_Store.Off_StoreSystem.Off_System_Id == user.DefaultSystemId)
                {
                    request.Status = 3;
                    request.ReplyUser = User.Identity.Name;
                    request.ReplyTime = DateTime.Now;
                    _offlineDB.Entry(request).State = System.Data.Entity.EntityState.Modified;
                    _offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                else
                    return Json(new { result = "UNAUTHORIZED" });
            }
            return Json(new { result = "FAIL" });
        }
        //ManagerDarkList
        public ActionResult ManagerDarkList()
        {
            return View();
        }
        // 管理员签到路线
        public ActionResult ManagerRouter()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var managerlist = from m in _offlineDB.Off_StoreManager
                              where m.Off_System_Id == user.DefaultSystemId && m.Status>=0
                              select m;
            ViewBag.ManagerList = new SelectList(managerlist, "UserName", "NickName", managerlist.FirstOrDefault().UserName);
            return View();
        }
        [HttpPost]
        public JsonResult ManagerRouterDetails(string username, DateTime date)
        {
            var router = _offlineDB.Off_Manager_Task.SingleOrDefault(m => m.UserName == username && m.TaskDate == date && m.Status>=0);
            if (router != null)
            {
                var routerdetails = from m in router.Off_Manager_CheckIn
                                    where m.Canceled== false
                                    orderby m.CheckIn_Time
                                    select new { Id = m.Id, CheckIn_Time = m.CheckIn_Time.ToString("HH:mm:ss"), Location = m.Location, Remark = m.Remark, Photo = m.Photo };
                return Json(new { result = "SUCCESS", summary = new { Event_Complete = router.Event_Complete, Event_UnComplete = router.Event_UnComplete, Event_Assistance = router.Event_Assistance }, router = routerdetails });
            }
            else
            {
                return Json(new { result = "FAIL" });
            }

        }

        public FileResult DownloadManagerTaskFile(DateTime start, DateTime end)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = from m in _offlineDB.Off_Manager_Task
                       where m.TaskDate >= start && m.TaskDate <= end
                       select m;
            //var list = _offlineDB.Database.SqlQuery<SellerSalaryExcel>(sql);
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            CsvWriter csv = new CsvWriter(writer);
            //string[] columname = new string[] {"店铺名称", "经销商", "姓名", "电话号码", "身份证号码", "开户行", "银行卡号", "工资", "奖金", "全勤天数", "迟到天数" };
            csv.WriteField("日期");
            csv.WriteField("督导姓名");
            csv.WriteField("重点工作");
            csv.WriteField("主要工作");
            csv.WriteField("协调工作");
            csv.WriteField("巡店明细");
            csv.NextRecord();
            foreach (var item in list)
            {

                csv.WriteField(item.TaskDate.ToString("yyyy-MM-dd"));
                csv.WriteField(item.NickName);
                csv.WriteField(item.Event_Complete);
                csv.WriteField(item.Event_UnComplete);
                csv.WriteField(item.Event_Assistance);
                string content = "";
                foreach (var p_item in item.Off_Manager_CheckIn.Where(m => m.Canceled == false))
                {
                    content += p_item.CheckIn_Time.ToString("HH:mm:ss") + ": " + p_item.Remark + "\r\n";
                }
                csv.WriteField(content);
                csv.NextRecord();
            }
            //csv.WriteRecords(list);
            writer.Flush();
            writer.Close();
            return File(convertCSV(stream.ToArray()), "@text/csv", "督导巡店信息" + start.ToShortDateString() + "-" + end.ToShortDateString() + ".csv");
        }
        private byte[] convertCSV(byte[] array)
        {
            byte[] outBuffer = new byte[array.Length + 3];
            outBuffer[0] = (byte)0xEF;//有BOM,解决乱码
            outBuffer[1] = (byte)0xBB;
            outBuffer[2] = (byte)0xBF;
            Array.Copy(array, 0, outBuffer, 3, array.Length);
            return outBuffer;
        }
    }
    
}