using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

using PagedList;
using PeriodAid.Models;
using PeriodAid.Filters;
namespace PeriodAid.Controllers
{
    [Authorize(Roles ="Admin")]
    public class OffExpensesController : Controller
    {
        OfflineSales _offlineDB = new OfflineSales();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public OffExpensesController()
        {

        }

        public OffExpensesController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: OffExpenses
        public ActionResult Index()
        {
            return View();
        }

        /* -----费用表----- */
        // Origin: Off_Expenses_List
        [SettingFilter(SettingName = "EXPENSES")]
        public ActionResult ExpensesIndex()
        {
            return View();
        }
        // Origin: Off_Expenses_AjaxList
        [SettingFilter(SettingName = "EXPENSES")]
        public ActionResult ExpensesListPartial(int? page, int? type)
        {
            int _type = type ?? 0;
            int _page = page ?? 1;
            var user = UserManager.FindById(User.Identity.GetUserId());
            var list = (from m in _offlineDB.Off_Expenses
                        where m.Status >= 0 && m.PaymentType == _type && m.Off_System_Id == user.DefaultSystemId
                        orderby m.Id descending
                        select m).ToPagedList(_page, 20);
            return PartialView(list);
        }

        // Origin: Off_Expenses_Add
        [SettingFilter(SettingName = "EXPENSES")]
        public ActionResult AddExpensesPartial()
        {
            List<Object> attendance = new List<Object>();
            attendance.Add(new { Key = 0, Value = "进场费" });
            attendance.Add(new { Key = 1, Value = "活动费" });
            ViewBag.PayType = new SelectList(attendance, "Key", "Value");
            return PartialView();
        }
        [SettingFilter(SettingName = "EXPENSES")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult AddExpensesPartial(FormCollection form)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var item = new Off_Expenses();
            if (TryUpdateModel(item))
            {
                item.Status = 0;
                item.UploadTime = DateTime.Now;
                item.UploadUser = User.Identity.Name;
                item.Off_System_Id = user.DefaultSystemId;
                _offlineDB.Off_Expenses.Add(item);
                _offlineDB.SaveChanges();
                return Content("SUCCESS");
            }
            return Content("FAIL");
        }

        // Origin: Off_Expenses_Edit
        [SettingFilter(SettingName = "EXPENSES")]
        public ActionResult EditExpenses(int id)
        {
            var item = _offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_System_Id == user.DefaultSystemId)
                {
                    List<Object> attendance = new List<Object>();
                    attendance.Add(new { Key = 0, Value = "进场费" });
                    attendance.Add(new { Key = 1, Value = "活动费" });
                    ViewBag.PayType = new SelectList(attendance, "Key", "Value", item.Status);
                    return View(item);
                }
                else
                {
                    return View("AuthorizeError");
                }
            }
            else
                return View("Error");
        }
        [SettingFilter(SettingName = "EXPENSES")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EditExpenses(int id, FormCollection form)
        {
            var expenses = _offlineDB.Off_Expenses.AsNoTracking().SingleOrDefault(m => m.Id == id);
            if (expenses != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (expenses.Off_System_Id == user.DefaultSystemId)
                {
                    Off_Expenses item = new Off_Expenses();
                    if (TryUpdateModel(item))
                    {
                        var detailitemcnt = form.GetValues("detailid") == null ? 0 : form.GetValues("detailid").Length;
                        for (int i = 0; i < detailitemcnt; i++)
                        {
                            if (form.GetValues("detailid")[i] == "0")
                            {
                                Off_Expenses_Details detailtemp = new Off_Expenses_Details()
                                {
                                    Off_Expenses = item,
                                    DetailsFee = Convert.ToDecimal(form.GetValues("detailfee")[i].ToString()),
                                    DetailsName = form.GetValues("detaillist")[i].ToString(),
                                    Remarks = form.GetValues("detailremarks")[i].ToString(),
                                    UploadTime = DateTime.Now,
                                    UploadUser = User.Identity.Name,
                                    ExpensesType = 0
                                };
                                _offlineDB.Off_Expenses_Details.Add(detailtemp);
                            }
                            else
                            {
                                int d_id = Convert.ToInt32(form.GetValues("detailid")[i]);
                                Off_Expenses_Details detailstemp = _offlineDB.Off_Expenses_Details.SingleOrDefault(m => m.Id == d_id);
                                detailstemp.DetailsFee = Convert.ToDecimal(form.GetValues("detailfee")[i].ToString());
                                detailstemp.DetailsName = form.GetValues("detaillist")[i].ToString();
                                detailstemp.Remarks = form.GetValues("detailremarks")[i].ToString();
                                detailstemp.UploadTime = DateTime.Now;
                                detailstemp.UploadUser = User.Identity.Name;
                                _offlineDB.Entry(detailstemp).State = System.Data.Entity.EntityState.Modified;
                            }
                        }
                        item.UploadTime = DateTime.Now;
                        item.UploadUser = User.Identity.Name;
                        _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        _offlineDB.SaveChanges();
                    }
                    return RedirectToAction("ExpensesIndex");
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

        // Origin: Off_Expenses_Balance
        [SettingFilter(SettingName = "EXPENSES")]
        public ActionResult BalanceExpenses(int id)
        {
            var item = _offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_System_Id == user.DefaultSystemId)
                {
                    List<Object> attendance = new List<Object>();
                    attendance.Add(new { Key = 0, Value = "进场费" });
                    attendance.Add(new { Key = 1, Value = "活动费" });
                    ViewBag.PayType = new SelectList(attendance, "Key", "Value", item.PaymentType);
                    return View(item);
                }
                else
                {
                    return View("AuthorizeError");
                }
            }
            else
                return View("Error");
        }
        [SettingFilter(SettingName = "EXPENSES")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult BalanceExpenses(int id, FormCollection form)
        {
            var expenses = _offlineDB.Off_Expenses.AsNoTracking().SingleOrDefault(m => m.Id == id);
            if (expenses != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (expenses.Off_System_Id == user.DefaultSystemId)
                {
                    Off_Expenses item = new Off_Expenses();
                    if (TryUpdateModel(item))
                    {
                        var detailitemcnt = form.GetValues("detailid") == null ? 0 : form.GetValues("detailid").Length;
                        for (int i = 0; i < detailitemcnt; i++)
                        {
                            if (form.GetValues("detailid")[i] == "0")
                            {
                                Off_Expenses_Details detailtemp = new Off_Expenses_Details()
                                {
                                    Off_Expenses = item,
                                    DetailsFee = Convert.ToDecimal(form.GetValues("detailfee")[i].ToString()),
                                    DetailsName = form.GetValues("detaillist")[i].ToString(),
                                    Remarks = form.GetValues("detailremarks")[i].ToString(),
                                    UploadTime = DateTime.Now,
                                    UploadUser = User.Identity.Name,
                                    ExpensesType = 1
                                };
                                _offlineDB.Off_Expenses_Details.Add(detailtemp);
                            }
                            else
                            {
                                int d_id = Convert.ToInt32(form.GetValues("detailid")[i]);
                                Off_Expenses_Details detailstemp = _offlineDB.Off_Expenses_Details.SingleOrDefault(m => m.Id == d_id);
                                detailstemp.DetailsFee = Convert.ToDecimal(form.GetValues("detailfee")[i].ToString());
                                detailstemp.DetailsName = form.GetValues("detaillist")[i].ToString();
                                detailstemp.Remarks = form.GetValues("detailremarks")[i].ToString();
                                detailstemp.UploadTime = DateTime.Now;
                                detailstemp.UploadUser = User.Identity.Name;
                                _offlineDB.Entry(detailstemp).State = System.Data.Entity.EntityState.Modified;
                            }
                        }
                        item.UploadTime = DateTime.Now;
                        item.UploadUser = User.Identity.Name;
                        _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        _offlineDB.SaveChanges();
                        return RedirectToAction("ExpensesIndex");
                    }
                    return View("Error");
                }
                else
                    return View("AuthorizeError");
            }
            else
            {
                return View("Error");
            }
        }

        // Origin: Off_Expenses_VerifyCost
        [SettingFilter(SettingName = "EXPENSES")]
        public ActionResult VerifyExpenses(int id)
        {
            var item = _offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_System_Id == user.DefaultSystemId)
                {
                    List<Object> attendance = new List<Object>();
                    attendance.Add(new { Key = 0, Value = "进场费" });
                    attendance.Add(new { Key = 1, Value = "活动费" });
                    ViewBag.PayType = new SelectList(attendance, "Key", "Value", item.PaymentType);
                    return View(item);
                }
                else
                {
                    return View("AuthorizeError");
                }
            }
            else
                return View("Error");
        }
        [SettingFilter(SettingName = "EXPENSES")]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult VerifyExpenses(int id, FormCollection form)
        {
            var expenses = _offlineDB.Off_Expenses.AsNoTracking().SingleOrDefault(m => m.Id == id);
            if (expenses != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (expenses.Off_System_Id == user.DefaultSystemId)
                {
                    Off_Expenses item = new Off_Expenses();
                    if (TryUpdateModel(item))
                    {
                        var detailitemcnt = form.GetValues("detailid") == null ? 0 : form.GetValues("detailid").Length;
                        for (int i = 0; i < detailitemcnt; i++)
                        {
                            if (form.GetValues("detailid")[i] == "0")
                            {
                                Off_Expenses_Payment detailtemp = new Off_Expenses_Payment()
                                {
                                    Off_Expenses = item,
                                    VerifyCost = Convert.ToDecimal(form.GetValues("detailfee")[i].ToString()),
                                    VerifyType = Convert.ToInt32(form.GetValues("detaillist")[i].ToString()),
                                    ApplicationDate = Convert.ToDateTime(form.GetValues("apdate")[i]),
                                    Remarks = form.GetValues("detailremarks")[i].ToString(),
                                    UploadTime = DateTime.Now,
                                    UploadUser = User.Identity.Name
                                };
                                _offlineDB.Off_Expenses_Payment.Add(detailtemp);
                            }
                            else
                            {
                                int d_id = Convert.ToInt32(form.GetValues("detailid")[i]);
                                Off_Expenses_Payment detailstemp = _offlineDB.Off_Expenses_Payment.SingleOrDefault(m => m.Id == d_id);
                                detailstemp.VerifyCost = Convert.ToDecimal(form.GetValues("detailfee")[i].ToString());
                                detailstemp.VerifyType = Convert.ToInt32(form.GetValues("detaillist")[i].ToString());
                                detailstemp.ApplicationDate = Convert.ToDateTime(form.GetValues("apdate")[i]);
                                detailstemp.Remarks = form.GetValues("detailremarks")[i].ToString();
                                detailstemp.UploadTime = DateTime.Now;
                                detailstemp.UploadUser = User.Identity.Name;
                                _offlineDB.Entry(detailstemp).State = System.Data.Entity.EntityState.Modified;
                            }
                        }
                        item.UploadTime = DateTime.Now;
                        item.UploadUser = User.Identity.Name;
                        _offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        _offlineDB.SaveChanges();
                        return RedirectToAction("ExpensesIndex");
                    }
                    return View("Error");
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

        // Origin: Off_Expenses_Details_Del
        [SettingFilter(SettingName = "EXPENSES")]
        [HttpPost]
        public JsonResult DeleteExpensesDetailsAjax(int id)
        {
            if (id == 0)
            {
                return Json(new { result = "SUCCESS" });
            }
            var item = _offlineDB.Off_Expenses_Details.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                _offlineDB.Off_Expenses_Details.Remove(item);
                _offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }

        // Origin: Off_Expenses_Payment_Del
        [SettingFilter(SettingName = "EXPENSES")]
        [HttpPost]
        public JsonResult DeleteExpensesPaymentAjax(int id)
        {
            if (id == 0)
            {
                return Json(new { result = "SUCCESS" });
            }
            var item = _offlineDB.Off_Expenses_Payment.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                _offlineDB.Off_Expenses_Payment.Remove(item);
                _offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }

        // Origin: Off_Expenses_Check("ActionResult")
        [SettingFilter(SettingName = "EXPENSES")]
        [HttpPost]
        public JsonResult SubmitExpensesAjax(int id)
        {
            var item = _offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_System_Id == user.DefaultSystemId)
                {
                    item.CheckTime = DateTime.Now;
                    item.Status = 1;
                    _offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                else
                {
                    return Json(new { result = "UNAHTHORIZED" });
                }
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }
        // Origin:Off_Expenses_Balance_Submit(ActionResult)
        [SettingFilter(SettingName = "EXPENSES"), HttpPost]
        public JsonResult SubmitExpensesBalance(int id)
        {
            var item = _offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_System_Id == user.DefaultSystemId)
                {
                    item.BalanceTime = DateTime.Now;
                    item.Status = 2;
                    _offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                else
                    return Json(new { result = "UNAUTHORIZED" });
            }
            else
                return Json(new { result = "FAIL" });
        }
        // Origin: Off_Expenses_Verify_Submit("ActionResult")
        [SettingFilter(SettingName = "EXPENSES"), HttpPost]
        public JsonResult SubmitExpensesVerifyAjax(int id)
        {
            var item = _offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_System_Id == user.DefaultSystemId)
                {
                    item.CheckTime = DateTime.Now;
                    item.Status = 3;
                    _offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                else
                {
                    return Json(new { result = "UNAHTHORIZED" });
                }
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }
        // Origin: Off_Expenses_Cancel(ActionResult)
        [SettingFilter(SettingName = "EXPENSES"), HttpPost]
        public JsonResult CancelExpensesAjax(int id)
        {
            var item = _offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_System_Id == user.DefaultSystemId)
                {
                    item.UploadTime = DateTime.Now;
                    item.UploadUser = User.Identity.Name;
                    item.Status = -1;
                    _offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
                else
                    return Json(new { result = "UNAUTHORIZED" });
            }
            else
                return Json(new { result = "FAIL" });
        }

        // Origin: Off_Expenses_Details
        [SettingFilter(SettingName = "EXPENSES")]
        public ActionResult ViewExpenses(int id)
        {
            var item = _offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (item.Off_System_Id == user.DefaultSystemId)
                {
                    // 直接在内容里文字体现
                    //List<Object> attendance = new List<Object>();
                    //attendance.Add(new { Key = 0, Value = "进场费" });
                    //attendance.Add(new { Key = 1, Value = "活动费" });
                    //ViewBag.PayType = new SelectList(attendance, "Key", "Value", item.PaymentType);
                    //List<Object> status = new List<Object>();
                    //status.Add(new { Key = 0, Value = "未审核" });
                    //status.Add(new { Key = 1, Value = "已审核" });
                    //status.Add(new { Key = 2, Value = "已结算" });
                    //status.Add(new { Key = 3, Value = "已核销" });
                    //status.Add(new { Key = -1, Value = "作废" });
                    //ViewBag.ExpensesStatus = new SelectList(status, "Key", "Value", item.Status);
                    return View(item);
                }
                else
                {
                    return View("AuthorizeError");
                }
            }
            else
                return View("Error");
        }

        /* 使用位置未知
        [SettingFilter(SettingName = "EXPENSES")]
        public ActionResult Off_Expenses_Details_Edit(int id)
        {
            var item = offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
                return View(item);
            else
                return View("Error");
        }
        [SettingFilter(SettingName = "EXPENSES")]
        [HttpPost]
        public ActionResult Off_Expenses_Details_Edit(FormCollection form)
        {
            var listcontent = "";
            int i = 0;
            foreach (var item in form.GetValues("list"))
            {
                listcontent += item.ToString();
                i++;
            }
            return Content(form["ExpensesId"].ToString() + "," + i + "<BR />" + listcontent);
        }
        */
    }
}