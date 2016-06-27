//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using Microsoft.AspNet.Identity.Owin;
//using Microsoft.AspNet.Identity;

//using PeriodAid.Models;
//namespace PeriodAid.Controllers
//{
//    public class OffManagerController : Controller
//    {
//        OfflineSales _offlineDB = new OfflineSales();
//        private ApplicationSignInManager _signInManager;
//        private ApplicationUserManager _userManager;
//        public OffManagerController()
//        {

//        }

//        public OffManagerController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
//        {
//            UserManager = userManager;
//            SignInManager = signInManager;
//        }

//        public ApplicationSignInManager SignInManager
//        {
//            get
//            {
//                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
//            }
//            private set
//            {
//                _signInManager = value;
//            }
//        }

//        public ApplicationUserManager UserManager
//        {
//            get
//            {
//                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
//            }
//            private set
//            {
//                _userManager = value;
//            }
//        }
//        // GET: OffManager
//        public ActionResult Index()
//        {
//            return View();
//        }

//        // Origin: Off_Manager_List(拆分Normal+Ajax),添加查询功能
//        // 0310 管理员列表
//        public ActionResult ManagerList()
//        {
//            return View();
//        }
//        public PartialViewResult ManagerListPartial(int? page, string query)
//        {
//            int _page = page ?? 1;
//            var user = UserManager.FindById(User.Identity.GetUserId());
//            if (query == null || query.Trim() == "")
//            {
//                var list = from m in _offlineDB.Off_StoreManager
//                           where m.Off_System_Id == user.DefaultSystemId
//                           select m;
//                return PartialView(list);
//            }
//            else
//            {
//                var list = from m in _offlineDB.Off_StoreManager
//                           where m.Off_System_Id == user.DefaultSystemId && m.NickName.Contains(query)
//                           select m;
//                return PartialView(list);
//            }
//        }


//        // Origin: Off_Manager_UpdateSenior
//        // 0310 升级为超级管理员
//        public ActionResult UpdateManagerToSenior(int id)
//        {
//            var item = _offlineDB.Off_StoreManager.SingleOrDefault(m => m.Id == id);
//            var user = UserManager.FindByName(item.UserName); 
//            UserManager.AddToRole(user.Id, "Senior");
//            item.Status = 2;
//            offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
//            offlineDB.SaveChanges();
//            return Content("SUCCESS");
//        }

//        public ActionResult Off_Manager_AddStore(int id)
//        {
//            //var item = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Id == id);
//            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.Id == id);
//            var user = UserManager.FindById(User.Identity.GetUserId());
//            if (manager != null)
//            {
//                ViewBag.StoreList = manager.Off_Store.OrderBy(m => m.StoreName);
//                var storesystem = from m in offlineDB.Off_Store
//                                  where m.Off_System_Id == user.DefaultSystemId
//                                  group m by m.StoreSystem into g
//                                  orderby g.Key
//                                  select new { Key = g.Key, Value = g.Key };
//                ViewBag.SystemList = new SelectList(storesystem, "Key", "Value", storesystem.FirstOrDefault().Value);
//                ViewBag.Name = manager.NickName;
//                ViewBag.ManagerId = manager.Id;
//                return View();
//            }
//            else
//            {
//                return View();
//            }
//        }
//        public JsonResult Off_GetStoreName(int storeid)
//        {
//            var item = offlineDB.Off_Store.SingleOrDefault(m => m.Id == storeid).StoreName;
//            return Json(new { id = storeid, name = item }, JsonRequestBehavior.AllowGet);
//        }
//        public ActionResult Off_Manager_AjaxAddStore(int managerId, string arr_list)
//        {
//            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.Id == managerId);
//            var currentlist = manager.Off_Store.Select(m => m.Id);
//            string[] arr_temp = arr_list.Split(',');
//            List<int> arr_int = new List<int>();
//            foreach (var s in arr_temp)
//            {
//                arr_int.Add(Convert.ToInt32(s));
//            }
//            var select_list = (from m in offlineDB.Off_Store
//                               where arr_int.Contains(m.Id)
//                               select m).Select(m => m.Id);
//            var storelist = (from m in manager.Off_Store
//                             select m.Id).ToList();
//            foreach (var item in storelist)
//            {
//                var temp = offlineDB.Off_Store.SingleOrDefault(m => m.Id == item);
//                manager.Off_Store.Remove(temp);
//            }

//            foreach (var item2 in select_list)
//            {
//                manager.Off_Store.Add(offlineDB.Off_Store.SingleOrDefault(m => m.Id == item2));
//            }
//            offlineDB.Entry(manager).State = System.Data.Entity.EntityState.Modified;
//            offlineDB.SaveChanges();
//            return Content("SUCCESS");
//        }
//        // 0314 降级为普通管理员
//        public ActionResult Off_Manager_ReduceManager(int id)
//        {
//            var item = offlineDB.Off_StoreManager.SingleOrDefault(m => m.Id == id);
//            var user = UserManager.FindByName(item.UserName);
//            UserManager.RemoveFromRole(user.Id, "Senior");
//            item.Status = 1;
//            offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
//            offlineDB.SaveChanges();
//            return Content("SUCCESS");
//        }
//        // 0314 督导工作汇报列表
//        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
//        public ActionResult Off_Manager_TaskList()
//        {
//            return View();
//        }
//        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
//        public PartialViewResult Off_Manager_TaskList_Ajax(string query, int? status, int? page)
//        {
//            int _status = status ?? 0;
//            int _page = page ?? 1;
//            var user = UserManager.FindById(User.Identity.GetUserId());
//            if (query == null)
//            {
//                var list = (from m in offlineDB.Off_Manager_Task
//                            where m.Status == _status && m.Off_System_Id == user.DefaultSystemId
//                            orderby m.TaskDate descending
//                            select m).ToPagedList(_page, 30);
//                return PartialView(list);
//            }
//            else
//            {
//                var list = (from m in offlineDB.Off_Manager_Task
//                            where m.Status == _status && m.Off_System_Id == user.DefaultSystemId
//                            && m.NickName.Contains(query)
//                            orderby m.TaskDate descending
//                            select m).ToPagedList(_page, 30);
//                return PartialView(list);
//            }
//        }

//        // 0314 审核督导工作
//        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
//        public ActionResult Off_Manager_EditTask(int id)
//        {
//            var item = offlineDB.Off_Manager_Task.SingleOrDefault(m => m.Id == id);
//            if (item != null)
//            {
//                return View(item);
//            }
//            else
//            {
//                return View("Error");
//            }
//        }
//        [HttpPost, ValidateAntiForgeryToken]
//        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
//        public ActionResult Off_Manager_EditTask(Off_Manager_Task model)
//        {

//            if (ModelState.IsValid)
//            {
//                var user = UserManager.FindById(User.Identity.GetUserId());
//                var item = new Off_Manager_Task();
//                if (TryUpdateModel(item))
//                {
//                    item.Off_System_Id = user.DefaultSystemId;
//                    item.Eval_User = User.Identity.Name;
//                    item.Eval_Time = DateTime.Now;
//                    item.Status = (int)ManagerTaskStatus.Confirmed;
//                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
//                    offlineDB.SaveChanges();
//                    return RedirectToAction("Off_Manager_TaskList");
//                }
//                return View("Error");
//            }
//            else
//            {
//                ModelState.AddModelError("", "错误");
//                return View(model);
//            }
//        }

//        // 0314 作废当日工作内容
//        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
//        public ActionResult Off_Manager_CancelTask(int id)
//        {
//            var item = offlineDB.Off_Manager_Task.SingleOrDefault(m => m.Id == id);
//            if (item != null)
//            {
//                item.Status = (int)ManagerTaskStatus.Canceled;
//                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
//                offlineDB.SaveChanges();
//                return Content("SUCCESS");
//            }
//            return Content("FAIL");
//        }
//        // 0325 公告列表
//        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
//        public ActionResult Off_Manager_Announcement_List()
//        {
//            return View();
//        }
//        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
//        public ActionResult Off_Manager_Announcement_List_Ajax(int? page)
//        {
//            var user = UserManager.FindById(User.Identity.GetUserId());
//            int _page = page ?? 1;
//            var list = (from m in offlineDB.Off_Manager_Announcement
//                        where m.Off_System_Id == user.DefaultSystemId
//                        orderby m.Priority descending, m.FinishTime descending
//                        select m).ToPagedList(_page, 30);
//            ViewBag.SystemId = user.DefaultSystemId;
//            return PartialView(list);
//        }
//        // 0325 添加公告
//        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
//        public ActionResult Off_Manager_Announcement_Create()
//        {
//            Off_Manager_Announcement model = new Off_Manager_Announcement();
//            return View(model);
//        }
//        [HttpPost, ValidateAntiForgeryToken]
//        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
//        public ActionResult Off_Manager_Announcement_Create(Off_Manager_Announcement model)
//        {
//            if (ModelState.IsValid)
//            {
//                var user = UserManager.FindById(User.Identity.GetUserId());
//                Off_Manager_Announcement item = new Off_Manager_Announcement();
//                if (TryUpdateModel(item))
//                {
//                    item.SubmitTime = DateTime.Now;
//                    item.SubmitUser = User.Identity.Name;
//                    item.Off_System_Id = user.DefaultSystemId;
//                    offlineDB.Off_Manager_Announcement.Add(item);
//                    offlineDB.SaveChanges();
//                    return RedirectToAction("Off_Manager_Announcement_List");
//                }
//                return View("Error");
//            }
//            else
//            {
//                ModelState.AddModelError("", "发生错误");
//                return View(model);
//            }
//        }

//        // 0325 修改公告
//        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
//        public ActionResult Off_Manager_Announcement_Edit(int id)
//        {
//            Off_Manager_Announcement model = offlineDB.Off_Manager_Announcement.SingleOrDefault(m => m.Id == id);
//            if (model != null)
//                return View(model);
//            else
//                return View("Error");
//        }
//        [HttpPost, ValidateAntiForgeryToken]
//        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
//        public ActionResult Off_Manager_Announcement_Edit(Off_Manager_Announcement model)
//        {
//            if (ModelState.IsValid)
//            {
//                Off_Manager_Announcement item = new Off_Manager_Announcement();
//                if (TryUpdateModel(item))
//                {
//                    item.SubmitTime = DateTime.Now;
//                    item.SubmitUser = User.Identity.Name;
//                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
//                    offlineDB.SaveChanges();
//                    return RedirectToAction("Off_Manager_Announcement_List");
//                }
//                return View("Error");
//            }
//            else
//            {
//                ModelState.AddModelError("", "发生错误");
//                return View(model);
//            }
//        }

//        // 0325 删除公告
//        [HttpPost]
//        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
//        public ActionResult Off_Manager_Announcement_Delete_Ajax(int id)
//        {
//            Off_Manager_Announcement model = offlineDB.Off_Manager_Announcement.SingleOrDefault(m => m.Id == id);
//            if (model != null)
//            {
//                offlineDB.Off_Manager_Announcement.Remove(model);
//                offlineDB.SaveChanges();
//                return Json(new { result = "SUCCESS" });
//            }
//            return Json(new { result = "FAIL" });
//        }

//        // 0325 获取督导列表
//        [HttpPost]
//        public ActionResult Off_Manager_List_Ajax()
//        {
//            var user = UserManager.FindById(User.Identity.GetUserId());
//            var list = from m in offlineDB.Off_StoreManager
//                       where m.Off_System_Id == user.DefaultSystemId
//                       select new { UserName = m.UserName, NickName = m.NickName };
//            return Json(new { result = "SUCCESS", managerlist = list });
//        }

//        // 0329 获取需求列表
//        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
//        public ActionResult Off_Manager_Request_List()
//        {

//            return View();
//        }
//        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
//        public ActionResult Off_Manager_Request_List_Ajax(int? page)
//        {
//            var _page = page ?? 1;
//            var user = UserManager.FindById(User.Identity.GetUserId());
//            var list = (from m in offlineDB.Off_Manager_Request
//                        where m.Status >= 0 && m.Off_Store.Off_System_Id == user.DefaultSystemId
//                        orderby m.Status, m.Id descending
//                        select m).ToPagedList(_page, 20);
//            ViewBag.SystemId = user.DefaultSystemId;
//            return PartialView(list);
//        }

//        // 0329 修改需求详情
//        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
//        public ActionResult Off_Manager_Request_Edit(int id)
//        {
//            Off_Manager_Request model = offlineDB.Off_Manager_Request.SingleOrDefault(m => m.Id == id);
//            if (model != null)
//                return View(model);
//            else
//                return View("Error");
//        }
//        [HttpPost, ValidateAntiForgeryToken]
//        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
//        public ActionResult Off_Manager_Request_Edit(Off_Manager_Request model)
//        {
//            if (ModelState.IsValid)
//            {
//                Off_Manager_Request item = new Off_Manager_Request();
//                if (TryUpdateModel(item))
//                {
//                    item.Status = 2;
//                    item.ReplyTime = DateTime.Now;
//                    item.ReplyUser = User.Identity.Name;
//                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
//                    offlineDB.SaveChanges();
//                    return RedirectToAction("Off_Manager_Request_List");
//                }
//                return View("Error");
//            }
//            else
//            {
//                ModelState.AddModelError("", "发生错误");
//                return View(model);
//            }
//        }

//        // 0329 驳回需求
//        [HttpPost]
//        [SettingFilter(SettingName = "MANAGER_ATTENDANCE")]
//        public ActionResult Off_Manager_Request_Dismiss_Ajax(int id)
//        {
//            Off_Manager_Request model = offlineDB.Off_Manager_Request.SingleOrDefault(m => m.Id == id);
//            if (model != null)
//            {
//                model.Status = 3;
//                model.ReplyUser = User.Identity.Name;
//                model.ReplyTime = DateTime.Now;
//                offlineDB.Entry(model).State = System.Data.Entity.EntityState.Modified;
//                offlineDB.SaveChanges();
//                return Json(new { result = "SUCCESS" });
//            }
//            return Json(new { result = "FAIL" });
//        }
//    }
//}