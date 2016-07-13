using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

using PeriodAid.Models;
using PeriodAid.Filters;
using PagedList;

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
                           where m.Off_System_Id == user.DefaultSystemId
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
                    var storesystem = from m in _offlineDB.Off_Store
                                      where m.Off_System_Id == user.DefaultSystemId
                                      group m by m.StoreSystem into g
                                      orderby g.Key
                                      select new { Key = g.Key, Value = g.Key };
                    ViewBag.SystemList = new SelectList(storesystem, "Key", "Value", storesystem.FirstOrDefault().Value);
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
                                   where arr_int.Contains(m.Id) && m.Off_System_Id == user.DefaultSystemId
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
                    var user = UserManager.FindByName(manager.UserName);
                    UserManager.RemoveFromRole(user.Id, "Senior");
                    UserManager.RemoveFromRole(user.Id, "Manager");
                    UserManager.AddToRole(user.Id, "Seller");
                    Off_Membership_Bind omb = new Off_Membership_Bind()
                    {
                        NickName = manager.NickName,
                        Mobile = manager.Mobile,
                        UserName = manager.UserName,
                        Off_System_Id = currentuser.DefaultSystemId,
                        Bind = false,
                        ApplicationDate = DateTime.Now,
                        Recruit = true
                    };
                    _offlineDB.Off_StoreManager.Remove(manager);
                    _offlineDB.Off_Membership_Bind.Add(omb);
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
                        where m.Status >= 0 && m.Off_Store.Off_System_Id == user.DefaultSystemId
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
                if (request.Off_Store.Off_System_Id == user.DefaultSystemId)
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
                    return RedirectToAction("RequestList");
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                return View(model);
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
                if (request.Off_Store.Off_System_Id == user.DefaultSystemId)
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
    }
}