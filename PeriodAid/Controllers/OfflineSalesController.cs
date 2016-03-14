using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PeriodAid.Models;
using PeriodAid.DAL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using PagedList;
using System.Data.OleDb;
using System.Data;
using System.Text;
using System.IO;
using CsvHelper;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.RegularExpressions;

namespace PeriodAid.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OfflineSalesController : Controller
    {
        // GET: OfflineSales
        OfflineSales offlineDB = new OfflineSales();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public OfflineSalesController()
        {

        }

        public OfflineSalesController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        

        #region 销售数据表(旧版)
        public ActionResult Sales_Home()
        {
            return View();
        }

        public ActionResult Store_Home()
        {
            return View();
        }

        public ActionResult Seller_Home()
        {
            return View();
        }

        public ActionResult Event_Home()
        {
            return View();
        }


        /*------------ 门店系统 -----------*/

        public ActionResult Create_Store_System()
        {
            //Store_System s_system = new Store_System();
            Store_System_ViewModel model = new Store_System_ViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create_Store_System(Store_System_ViewModel model)
        {
            if (ModelState.IsValid)
            {
                Store_System s_system = new Store_System()
                {
                    System_Name = model.System_Name
                };
                offlineDB.Store_System.Add(s_system);
                offlineDB.SaveChanges();
                return RedirectToAction("List_Store_System");
            }
            else
            {
                ModelState.AddModelError("", "信息错误");
                return View(model);
            }
        }

        public ActionResult Edit_Store_System(int id)
        {
            var store_system = offlineDB.Store_System.SingleOrDefault(m => m.Id == id);
            if (store_system != null)
            {
                Store_System_ViewModel model = new Store_System_ViewModel()
                {
                    System_Name = store_system.System_Name
                };
                return View(model);
            }
            else
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult Edit_Store_System(int id, Store_System_ViewModel model)
        {
            if (ModelState.IsValid)
            {
                var s_system = offlineDB.Store_System.SingleOrDefault(m => m.Id == id);
                if (s_system != null)
                {
                    s_system.System_Name = model.System_Name;
                    offlineDB.SaveChanges();
                    return RedirectToAction("List_Store_System");
                }
                else
                {
                    //ModelState.AddModelError("", "信息错误");
                    return View("Error");
                }
            }
            else
            {
                ModelState.AddModelError("", "信息错误");
                return View(model);
            }
        }

        public ActionResult List_Store_System()
        {
            var list = from m in offlineDB.Store_System
                       select m;
            return View(list);
        }

        public ActionResult Delete_Store_System(int id)
        {
            var item = offlineDB.Store_System.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                Store_System_ViewModel model = new Store_System_ViewModel()
                {
                    System_Name = item.System_Name
                };
                return View(model);
            }
            else
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult Delete_Store_System(int id, Store_System_ViewModel model)
        {
            var item = offlineDB.Store_System.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                offlineDB.Store_System.Remove(item);
                offlineDB.SaveChanges();
                return RedirectToAction("List_Store_System");
            }
            else
            {
                return View("Error");
            }
        }


        /************** 门店 **************/
        public ActionResult List_Store()
        {
            var list = from m in offlineDB.Store
                       select m;

            return View(list);
        }

        public ActionResult Create_Store()
        {
            Store_ViewModel model = new Store_ViewModel();
            ViewBag.Store_System = new SelectList(offlineDB.Store_System, "Id", "System_Name");
            return View(model);
        }

        [HttpPost]
        public ActionResult Create_Store(Store_ViewModel model)
        {
            if (ModelState.IsValid)
            {
                Store s = new Store()
                {
                    Store_System_Id = model.Store_System_Id,
                    Address = model.Address,
                    Contact = model.Contact,
                    Store_Name = model.Store_Name
                };
                offlineDB.Store.Add(s);
                offlineDB.SaveChanges();
                return RedirectToAction("List_Store");
            }
            else
            {
                ViewBag.Store_System = new SelectList(offlineDB.Store_System, "Id", "System_Name");
                ModelState.AddModelError("", "信息错误");
                return View(model);
            }
        }

        public ActionResult Edit_Store(int id)
        {
            var item = offlineDB.Store.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                ViewBag.Store_System = new SelectList(offlineDB.Store_System, "Id", "System_Name");
                Store_ViewModel model = new Store_ViewModel()
                {
                    Store_System_Id = item.Store_System_Id,
                    Address = item.Address,
                    Contact = item.Contact,
                    Store_Name = item.Store_Name
                };
                return View(model);
            }
            else
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult Edit_Store(int id, Store_ViewModel model)
        {
            if (ModelState.IsValid)
            {
                var item = offlineDB.Store.SingleOrDefault(m => m.Id == id);
                if (item != null)
                {
                    item.Store_System_Id = model.Store_System_Id;
                    item.Store_Name = model.Store_Name;
                    item.Address = model.Address;
                    item.Contact = model.Contact;
                    offlineDB.SaveChanges();
                    return RedirectToAction("List_Store");
                }
                else
                {
                    return View("Error");
                }
            }
            else
            {
                ViewBag.Store_System = new SelectList(offlineDB.Store_System, "Id", "System_Name");
                ModelState.AddModelError("", "信息错误");
                return View(model);
            }
        }

        public ActionResult Delete_Store(int id)
        {
            var item = offlineDB.Store.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {

                return View(item);
            }
            else
            {
                return View("Error");
            }
        }
        [HttpPost]
        public ActionResult Delete_Store(int id, Store model)
        {
            var item = offlineDB.Store.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                offlineDB.Store.Remove(item);
                offlineDB.SaveChanges();
                return RedirectToAction("List_Store");
            }
            else
            {
                return View("Error");
            }
        }

        /*****************销售员************************/
        public ActionResult List_Seller()
        {
            var list = from m in offlineDB.Seller
                       select m;
            return View(list);
        }

        public ActionResult Create_Seller()
        {
            Seller_ViewModel model = new Seller_ViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create_Seller(Seller_ViewModel model)
        {
            if (ModelState.IsValid)
            {
                Seller s = new Seller()
                {
                    Sex = model.Sex,
                    Name = model.Name,
                    Contact = model.Contact
                };
                offlineDB.Seller.Add(s);
                offlineDB.SaveChanges();
                return RedirectToAction("List_Seller");
            }
            else
            {
                ModelState.AddModelError("", "信息错误");
                return View(model);
            }
        }

        public ActionResult Edit_Seller(int id)
        {
            var item = offlineDB.Seller.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                Seller_ViewModel model = new Seller_ViewModel()
                {
                    Name = item.Name,
                    Contact = item.Contact,
                    Sex = item.Sex
                };
                return View(model);
            }
            else
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult Edit_Seller(int id, Seller_ViewModel model)
        {
            if (ModelState.IsValid)
            {
                var item = offlineDB.Seller.SingleOrDefault(m => m.Id == id);
                if (item != null)
                {
                    item.Name = model.Name;
                    item.Contact = model.Contact;
                    item.Sex = model.Sex;
                    offlineDB.SaveChanges();
                    return RedirectToAction("List_Seller");
                }
                else
                {
                    return View("Error");
                }
            }
            else
            {
                ModelState.AddModelError("", "信息错误");
                return View(model);
            }
        }

        public ActionResult Delete_Seller(int id)
        {
            var item = offlineDB.Seller.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                return View(item);
            }
            else
            {
                return View("Error");
            }
        }
        [HttpPost]
        public ActionResult Delete_Seller(int id, Seller model)
        {
            var item = offlineDB.Seller.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                offlineDB.Seller.Remove(item);
                offlineDB.SaveChanges();
                return RedirectToAction("List_Seller");
            }
            else
            {
                return View("Error");
            }
        }

        /**********销售信息***********/
        public ActionResult List_Sales_Data(int page = 1)
        {
            //int final_page = page == null ? 1 : page;
            var list = (from m in offlineDB.Sales_Data
                        orderby m.Sales_Date descending
                        select m).ToPagedList(page, 10);
            ViewBag.Store_System = new SelectList(offlineDB.Store_System, "Id", "System_Name");
            var first_Store_System = offlineDB.Store_System.FirstOrDefault();
            ViewBag.Store = new SelectList(offlineDB.Store.Where(m => m.Store_System_Id == first_Store_System.Id), "Id", "Store_Name");
            return View(list);
        }

        [HttpPost]
        public PartialViewResult List_Sales_Data_Date(DateTime? date, int? page)
        {
            if (date != null)
            {
                var list = from m in offlineDB.Sales_Data
                           where m.Sales_Date == date
                           orderby m.Sales_Date descending
                           select m;
                return PartialView(list);
            }
            else
            {
                var list = (from m in offlineDB.Sales_Data
                            orderby m.Sales_Date descending
                            select m).Take(40);
                return PartialView(list);
            }
        }

        [HttpPost]
        public PartialViewResult List_sales_Data_Store(int? storesystem, int? store)
        {
            if (store != null)
            {
                var list = from m in offlineDB.Sales_Data
                           where m.Store_Id == store
                           orderby m.Sales_Date descending
                           select m;
                return PartialView(list);
            }
            else
            {
                if (storesystem != null)
                {
                    var list = from m in offlineDB.Sales_Data
                               where m.Store.Store_System_Id == storesystem
                               orderby m.Sales_Date descending
                               select m;
                    return PartialView(list);
                }
                else
                {
                    var list = (from m in offlineDB.Sales_Data
                                orderby m.Sales_Date descending
                                select m).Take(40);
                    return PartialView(list);
                }
            }
        }

        public ActionResult Create_Sales_Data()
        {
            Sales_Data_ViewModel model = new Sales_Data_ViewModel();
            ViewBag.Store_System = new SelectList(offlineDB.Store_System, "Id", "System_Name");
            var first_Store_System = offlineDB.Store_System.FirstOrDefault();
            ViewBag.Store = new SelectList(offlineDB.Store.Where(m => m.Store_System_Id == first_Store_System.Id), "Id", "Store_Name");
            ViewBag.Seller = new SelectList(offlineDB.Seller, "Id", "Name");
            ViewBag.Product = new SelectList(offlineDB.Product, "Product_Code", "Product_Name");
            ViewBag.ProductDetails = new List<Form_Product_Details>();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create_Sales_Data(Sales_Data_ViewModel model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                var exist_item = from m in offlineDB.Sales_Data
                                 where m.Store_Id == model.Store_Id
                                 && m.Sales_Date == model.Sales_Date
                                 select m;
                if (exist_item.Count() == 0)
                {
                    Sales_Data sales = new Sales_Data()
                    {
                        Store_Id = model.Store_Id,
                        Sales_Date = model.Sales_Date,
                        Trial_Count = model.Trial_Count,
                        Seller_Id = model.Seller_Id,
                        Max_Sale = model.Max_Sale,
                        Feedback = model.Feedback,
                        Event = model.Event,
                        Summary = model.Summary,
                        Comsumption_Age = model.Comsumption_Age,
                        Event_Type = model.EventType
                    };
                    offlineDB.Sales_Data.Add(sales);
                    offlineDB.SaveChanges();
                    Progress_Sales_Details(sales, form);
                    return RedirectToAction("List_Sales_Data");
                }
                else
                {
                    var store = offlineDB.Store.SingleOrDefault(m => m.Id == model.Store_Id);
                    ViewBag.Store_System = new SelectList(offlineDB.Store_System, "Id", "System_Name");
                    ViewBag.Store = new SelectList(offlineDB.Store.Where(m => m.Store_System_Id == store.Store_System_Id), "Id", "Store_Name", model.Store_Id);
                    ViewBag.Seller = new SelectList(offlineDB.Seller, "Id", "Name");
                    ViewBag.ProductDetails = GetForm_Sales_Details(form);
                    ViewBag.Product = new SelectList(offlineDB.Product, "Product_Code", "Product_Name");
                    ModelState.AddModelError("", "同一日期、同一门店记录不能重复");
                    return View(model);
                }
            }
            else
            {
                var store = offlineDB.Store.SingleOrDefault(m => m.Id == model.Store_Id);
                ViewBag.Store_System = new SelectList(offlineDB.Store_System, "Id", "System_Name");
                ViewBag.Store = new SelectList(offlineDB.Store.Where(m => m.Store_System_Id == store.Store_System_Id), "Id", "Store_Name", model.Store_Id);
                ViewBag.Seller = new SelectList(offlineDB.Seller, "Id", "Name");
                ViewBag.ProductDetails = GetForm_Sales_Details(form);
                ViewBag.Product = new SelectList(offlineDB.Product, "Product_Code", "Product_Name");
                ModelState.AddModelError("", "信息错误");
                return View(model);
            }
        }

        public ActionResult Edit_Sales_Data(int id)
        {
            var item = offlineDB.Sales_Data.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var store = offlineDB.Store.SingleOrDefault(m => m.Id == item.Store_Id);
                ViewBag.Store_System = new SelectList(offlineDB.Store_System, "Id", "System_Name");
                ViewBag.Store = new SelectList(offlineDB.Store.Where(m => m.Store_System_Id == store.Store_System_Id), "Id", "Store_Name", item.Store_Id);
                ViewBag.Seller = new SelectList(offlineDB.Seller, "Id", "Name");
                ViewBag.Product = new SelectList(offlineDB.Product, "Product_Code", "Product_Name");
                Sales_Data_ViewModel model = new Sales_Data_ViewModel()
                {
                    Store_System_Id = store.Store_System_Id,
                    Store_Id = item.Store_Id,
                    Sales_Date = item.Sales_Date,
                    Seller_Id = item.Seller_Id,
                    Summary = item.Summary,
                    Max_Sale = item.Max_Sale,
                    Comsumption_Age = item.Comsumption_Age,
                    Event = item.Event,
                    Trial_Count = item.Trial_Count,
                    Feedback = item.Feedback,
                    EventType = item.Event_Type
                };

                var getlist = from m in offlineDB.Sales_Details
                              where m.Sales_Data_Id == item.Id
                              orderby m.Product_Id
                              select m;
                List<Form_Product_Details> details = new List<Form_Product_Details>();
                foreach (var i in getlist)
                {
                    Form_Product_Details detail = new Form_Product_Details()
                    {
                        CheckNum = i.Checkout_Num,
                        Product_Name = i.Product.Product_Name,
                        Product_Code = i.Product.Product_Code,
                        ReportNum = i.Report_Num
                    };
                    details.Add(detail);
                }
                ViewBag.ProductDetails = details;
                return View(model);
            }
            else
            {
                return View("Error");
            }

        }
        [HttpPost]
        public ActionResult Edit_Sales_Data(int id, Sales_Data_ViewModel model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                // 同一个门店，同一个日期不能重复(除当前ID以外)
                var exist_item = from m in offlineDB.Sales_Data
                                 where m.Store_Id == model.Store_Id
                                 && m.Sales_Date == model.Sales_Date
                                 && m.Id != id
                                 select m;
                if (exist_item.Count() == 0)
                {
                    var item = offlineDB.Sales_Data.SingleOrDefault(m => m.Id == id);
                    if (item != null)
                    {
                        item.Store_Id = model.Store_Id;
                        item.Sales_Date = model.Sales_Date;
                        item.Trial_Count = model.Trial_Count;
                        item.Seller_Id = model.Seller_Id;
                        item.Max_Sale = model.Max_Sale;
                        item.Feedback = model.Feedback;
                        item.Event = model.Event;
                        item.Summary = model.Summary;
                        item.Comsumption_Age = model.Comsumption_Age;
                        item.Event_Type = model.EventType;

                        offlineDB.SaveChanges();
                        Progress_Sales_Details(item, form);
                        return RedirectToAction("List_Sales_Data");
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                else
                {
                    var store = offlineDB.Store.SingleOrDefault(m => m.Id == model.Store_Id);
                    ViewBag.Store_System = new SelectList(offlineDB.Store_System, "Id", "System_Name");
                    ViewBag.Store = new SelectList(offlineDB.Store.Where(m => m.Store_System_Id == store.Store_System_Id), "Id", "Store_Name", model.Store_Id);
                    ViewBag.Seller = new SelectList(offlineDB.Seller, "Id", "Name");
                    ViewBag.ProductDetails = GetForm_Sales_Details(form);
                    ViewBag.Product = new SelectList(offlineDB.Product, "Product_Code", "Product_Name");
                    ModelState.AddModelError("", "同一日期、同一门店记录不能重复");
                    return View(model);
                }
            }
            else
            {
                var store = offlineDB.Store.SingleOrDefault(m => m.Id == model.Store_Id);
                ViewBag.Store_System = new SelectList(offlineDB.Store_System, "Id", "System_Name");
                ViewBag.Store = new SelectList(offlineDB.Store.Where(m => m.Store_System_Id == store.Store_System_Id), "Id", "Store_Name", model.Store_Id);
                ViewBag.Seller = new SelectList(offlineDB.Seller, "Id", "Name");
                ViewBag.ProductDetails = GetForm_Sales_Details(form);
                ViewBag.Product = new SelectList(offlineDB.Product, "Product_Code", "Product_Name");
                ModelState.AddModelError("", "信息错误");
                return View(model);
            }
        }

        public ActionResult Delete_Sales_Data(int id)
        {
            var item = offlineDB.Sales_Data.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                return View(item);
            }
            else
            {
                return View("Error");
            }
        }
        [HttpPost]
        public ActionResult Delete_Sales_Data(int id, Sales_Data model)
        {
            var item = offlineDB.Sales_Data.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var list = from m in offlineDB.Sales_Details
                           where m.Sales_Data_Id == item.Id
                           select m;
                offlineDB.Sales_Details.RemoveRange(list);
                offlineDB.Sales_Data.Remove(item);
                offlineDB.SaveChanges();
                return RedirectToAction("List_Sales_Data");
            }
            else
            {
                return View("Error");
            }
        }

        public PartialViewResult DropDownList_Store(int store_system_id)
        {
            ViewBag.Store = new SelectList(offlineDB.Store.Where(m => m.Store_System_Id == store_system_id), "Id", "Store_Name");
            return PartialView();
        }

        private List<Form_Product_Details> GetForm_Sales_Details(FormCollection form)
        {
            List<Form_Product_Details> list = new List<Form_Product_Details>();
            // 检查红糖姜茶
            if (form["sqz122_report"] != null && form["sqz122_check"] != null)
            {
                // 表单内存在产品
                int reportNum = Convert.ToInt32(form["sqz122_report"]);
                int checkNum = Convert.ToInt32(form["sqz122_check"]);
                Form_Product_Details item = new Form_Product_Details()
                {
                    Product_Code = "sqz122",
                    Product_Name = "红糖姜茶",
                    CheckNum = checkNum,
                    ReportNum = reportNum
                };
                list.Add(item);
            }
            // 检查黑糖姜茶
            if (form["sqz123_report"] != null && form["sqz123_check"] != null)
            {
                // 表单内存在产品
                int reportNum = Convert.ToInt32(form["sqz123_report"]);
                int checkNum = Convert.ToInt32(form["sqz123_check"]);
                Form_Product_Details item = new Form_Product_Details()
                {
                    Product_Code = "sqz123",
                    Product_Name = "黑糖姜茶",
                    CheckNum = checkNum,
                    ReportNum = reportNum
                };
                list.Add(item);
            }
            // 检查蜂蜜姜茶
            if (form["sqz124_report"] != null && form["sqz124_check"] != null)
            {
                // 表单内存在产品
                int reportNum = Convert.ToInt32(form["sqz124_report"]);
                int checkNum = Convert.ToInt32(form["sqz124_check"]);
                Form_Product_Details item = new Form_Product_Details()
                {
                    Product_Code = "sqz124",
                    Product_Name = "蜂蜜姜茶",
                    CheckNum = checkNum,
                    ReportNum = reportNum
                };
                list.Add(item);
            }
            // 检查柠檬姜茶
            if (form["sqz125_report"] != null && form["sqz125_check"] != null)
            {
                // 表单内存在产品
                int reportNum = Convert.ToInt32(form["sqz125_report"]);
                int checkNum = Convert.ToInt32(form["sqz125_check"]);
                Form_Product_Details item = new Form_Product_Details()
                {
                    Product_Code = "sqz125",
                    Product_Name = "柠檬姜茶",
                    CheckNum = checkNum,
                    ReportNum = reportNum
                };
                list.Add(item);
            }
            // 检查红枣姜茶
            if (form["sqz126_report"] != null && form["sqz126_check"] != null)
            {
                // 表单内存在产品
                int reportNum = Convert.ToInt32(form["sqz126_report"]);
                int checkNum = Convert.ToInt32(form["sqz126_check"]);
                Form_Product_Details item = new Form_Product_Details()
                {
                    Product_Code = "sqz126",
                    Product_Name = "红枣姜茶",
                    CheckNum = checkNum,
                    ReportNum = reportNum
                };
                list.Add(item);
            }
            // 检查薄荷姜茶
            if (form["sqz127_report"] != null && form["sqz127_check"] != null)
            {
                // 表单内存在产品
                int reportNum = Convert.ToInt32(form["sqz127_report"]);
                int checkNum = Convert.ToInt32(form["sqz127_check"]);
                Form_Product_Details item = new Form_Product_Details()
                {
                    Product_Code = "sqz127",
                    Product_Name = "薄荷姜茶",
                    CheckNum = checkNum,
                    ReportNum = reportNum
                };
                list.Add(item);
            }
            // 检查蜂蜜菊花茶
            if (form["sqz128_report"] != null && form["sqz128_check"] != null)
            {
                // 表单内存在产品
                int reportNum = Convert.ToInt32(form["sqz128_report"]);
                int checkNum = Convert.ToInt32(form["sqz128_check"]);
                Form_Product_Details item = new Form_Product_Details()
                {
                    Product_Code = "sqz128",
                    Product_Name = "蜂蜜菊花茶",
                    CheckNum = checkNum,
                    ReportNum = reportNum
                };
                list.Add(item);
            }
            // 检查陈皮酸梅汤
            if (form["sqz129_report"] != null && form["sqz129_check"] != null)
            {
                // 表单内存在产品
                int reportNum = Convert.ToInt32(form["sqz129_report"]);
                int checkNum = Convert.ToInt32(form["sqz129_check"]);
                Form_Product_Details item = new Form_Product_Details()
                {
                    Product_Code = "sqz129",
                    Product_Name = "陈皮酸梅汤",
                    CheckNum = checkNum,
                    ReportNum = reportNum
                };
                list.Add(item);
            }
            // 检查生姜红茶
            if (form["sqz130_report"] != null && form["sqz130_check"] != null)
            {
                // 表单内存在产品
                int reportNum = Convert.ToInt32(form["sqz130_report"]);
                int checkNum = Convert.ToInt32(form["sqz130_check"]);
                Form_Product_Details item = new Form_Product_Details()
                {
                    Product_Code = "sqz130",
                    Product_Name = "生姜红茶",
                    CheckNum = checkNum,
                    ReportNum = reportNum
                };
                list.Add(item);
            }
            return list;
        }
        private void Progress_Sales_Details(Sales_Data sales, FormCollection form)
        {

            // 检查红糖姜茶
            if (form["sqz122_report"] != null && form["sqz122_check"] != null)
            {
                // 表单内存在产品
                int reportNum = Convert.ToInt32(form["sqz122_report"]);
                int checkNum = Convert.ToInt32(form["sqz122_check"]);
                var item = offlineDB.Sales_Details.SingleOrDefault(m => m.Sales_Data_Id == sales.Id && m.Product_Id == 1);
                if (item != null)
                {
                    // 商品已存在
                    item.Report_Num = reportNum;
                    item.Checkout_Num = checkNum;
                }
                else
                {
                    // 商品不存在，需要添加
                    Sales_Details detail = new Sales_Details()
                    {
                        Product_Id = 1,
                        Sales_Data = sales,
                        Checkout_Num = checkNum,
                        Report_Num = reportNum
                    };
                    offlineDB.Sales_Details.Add(detail);
                }
            }
            else if (form["sqz122_report"] == null || form["sqz122_check"] == null)
            {
                // 表单内不存在产品
                var item = offlineDB.Sales_Details.SingleOrDefault(m => m.Sales_Data_Id == sales.Id && m.Product_Id == 1);
                if (item != null)
                {
                    // 商品已存在，需要删除
                    offlineDB.Sales_Details.Remove(item);
                }
            }
            // 检查黑糖姜茶
            if (form["sqz123_report"] != null && form["sqz123_check"] != null)
            {
                // 表单内存在产品
                int reportNum = Convert.ToInt32(form["sqz123_report"]);
                int checkNum = Convert.ToInt32(form["sqz123_check"]);
                var item = offlineDB.Sales_Details.SingleOrDefault(m => m.Sales_Data_Id == sales.Id && m.Product_Id == 2);
                if (item != null)
                {
                    // 商品已存在
                    item.Report_Num = reportNum;
                    item.Checkout_Num = checkNum;
                }
                else
                {
                    // 商品不存在，需要添加
                    Sales_Details detail = new Sales_Details()
                    {
                        Product_Id = 2,
                        Sales_Data = sales,
                        Checkout_Num = checkNum,
                        Report_Num = reportNum
                    };
                    offlineDB.Sales_Details.Add(detail);
                }
            }
            else if (form["sqz123_report"] == null || form["sqz123_check"] == null)
            {
                // 表单内不存在产品
                var item = offlineDB.Sales_Details.SingleOrDefault(m => m.Sales_Data_Id == sales.Id && m.Product_Id == 2);
                if (item != null)
                {
                    // 商品已存在，需要删除
                    offlineDB.Sales_Details.Remove(item);
                }
            }
            // 检查蜂蜜姜茶
            if (form["sqz124_report"] != null && form["sqz124_check"] != null)
            {
                // 表单内存在产品
                int reportNum = Convert.ToInt32(form["sqz124_report"]);
                int checkNum = Convert.ToInt32(form["sqz124_check"]);
                var item = offlineDB.Sales_Details.SingleOrDefault(m => m.Sales_Data_Id == sales.Id && m.Product_Id == 3);
                if (item != null)
                {
                    // 商品已存在
                    item.Report_Num = reportNum;
                    item.Checkout_Num = checkNum;
                }
                else
                {
                    // 商品不存在，需要添加
                    Sales_Details detail = new Sales_Details()
                    {
                        Product_Id = 3,
                        Sales_Data = sales,
                        Checkout_Num = checkNum,
                        Report_Num = reportNum
                    };
                    offlineDB.Sales_Details.Add(detail);
                }
            }
            else if (form["sqz124_report"] == null || form["sqz124_check"] == null)
            {
                // 表单内不存在产品
                var item = offlineDB.Sales_Details.SingleOrDefault(m => m.Sales_Data_Id == sales.Id && m.Product_Id == 3);
                if (item != null)
                {
                    // 商品已存在，需要删除
                    offlineDB.Sales_Details.Remove(item);
                }
            }
            // 检查柠檬姜茶
            if (form["sqz125_report"] != null && form["sqz125_check"] != null)
            {
                // 表单内存在产品
                int reportNum = Convert.ToInt32(form["sqz125_report"]);
                int checkNum = Convert.ToInt32(form["sqz125_check"]);
                var item = offlineDB.Sales_Details.SingleOrDefault(m => m.Sales_Data_Id == sales.Id && m.Product_Id == 4);
                if (item != null)
                {
                    // 商品已存在
                    item.Report_Num = reportNum;
                    item.Checkout_Num = checkNum;
                }
                else
                {
                    // 商品不存在，需要添加
                    Sales_Details detail = new Sales_Details()
                    {
                        Product_Id = 4,
                        Sales_Data = sales,
                        Checkout_Num = checkNum,
                        Report_Num = reportNum
                    };
                    offlineDB.Sales_Details.Add(detail);
                }
            }
            else if (form["sqz125_report"] == null || form["sqz125_check"] == null)
            {
                // 表单内不存在产品
                var item = offlineDB.Sales_Details.SingleOrDefault(m => m.Sales_Data_Id == sales.Id && m.Product_Id == 4);
                if (item != null)
                {
                    // 商品已存在，需要删除
                    offlineDB.Sales_Details.Remove(item);
                }
            }
            // 检查红枣姜茶
            if (form["sqz126_report"] != null && form["sqz126_check"] != null)
            {
                // 表单内存在产品
                int reportNum = Convert.ToInt32(form["sqz126_report"]);
                int checkNum = Convert.ToInt32(form["sqz126_check"]);
                var item = offlineDB.Sales_Details.SingleOrDefault(m => m.Sales_Data_Id == sales.Id && m.Product_Id == 5);
                if (item != null)
                {
                    // 商品已存在
                    item.Report_Num = reportNum;
                    item.Checkout_Num = checkNum;
                }
                else
                {
                    // 商品不存在，需要添加
                    Sales_Details detail = new Sales_Details()
                    {
                        Product_Id = 5,
                        Sales_Data = sales,
                        Checkout_Num = checkNum,
                        Report_Num = reportNum
                    };
                    offlineDB.Sales_Details.Add(detail);
                }
            }
            else if (form["sqz126_report"] == null || form["sqz126_check"] == null)
            {
                // 表单内不存在产品
                var item = offlineDB.Sales_Details.SingleOrDefault(m => m.Sales_Data_Id == sales.Id && m.Product_Id == 5);
                if (item != null)
                {
                    // 商品已存在，需要删除
                    offlineDB.Sales_Details.Remove(item);
                }
            }
            // 检查薄荷姜茶
            if (form["sqz127_report"] != null && form["sqz127_check"] != null)
            {
                // 表单内存在产品
                int reportNum = Convert.ToInt32(form["sqz127_report"]);
                int checkNum = Convert.ToInt32(form["sqz127_check"]);
                var item = offlineDB.Sales_Details.SingleOrDefault(m => m.Sales_Data_Id == sales.Id && m.Product_Id == 6);
                if (item != null)
                {
                    // 商品已存在
                    item.Report_Num = reportNum;
                    item.Checkout_Num = checkNum;
                }
                else
                {
                    // 商品不存在，需要添加
                    Sales_Details detail = new Sales_Details()
                    {
                        Product_Id = 6,
                        Sales_Data = sales,
                        Checkout_Num = checkNum,
                        Report_Num = reportNum
                    };
                    offlineDB.Sales_Details.Add(detail);
                }
            }
            else if (form["sqz127_report"] == null || form["sqz127_check"] == null)
            {
                // 表单内不存在产品
                var item = offlineDB.Sales_Details.SingleOrDefault(m => m.Sales_Data_Id == sales.Id && m.Product_Id == 6);
                if (item != null)
                {
                    // 商品已存在，需要删除
                    offlineDB.Sales_Details.Remove(item);
                }
            }
            // 检查蜂蜜菊花茶
            if (form["sqz128_report"] != null && form["sqz128_check"] != null)
            {
                // 表单内存在产品
                int reportNum = Convert.ToInt32(form["sqz128_report"]);
                int checkNum = Convert.ToInt32(form["sqz128_check"]);
                var item = offlineDB.Sales_Details.SingleOrDefault(m => m.Sales_Data_Id == sales.Id && m.Product_Id == 7);
                if (item != null)
                {
                    // 商品已存在
                    item.Report_Num = reportNum;
                    item.Checkout_Num = checkNum;
                }
                else
                {
                    // 商品不存在，需要添加
                    Sales_Details detail = new Sales_Details()
                    {
                        Product_Id = 7,
                        Sales_Data = sales,
                        Checkout_Num = checkNum,
                        Report_Num = reportNum
                    };
                    offlineDB.Sales_Details.Add(detail);
                }
            }
            else if (form["sqz128_report"] == null || form["sqz128_check"] == null)
            {
                // 表单内不存在产品
                var item = offlineDB.Sales_Details.SingleOrDefault(m => m.Sales_Data_Id == sales.Id && m.Product_Id == 7);
                if (item != null)
                {
                    // 商品已存在，需要删除
                    offlineDB.Sales_Details.Remove(item);
                }
            }
            // 检查陈皮酸梅
            if (form["sqz129_report"] != null && form["sqz129_check"] != null)
            {
                // 表单内存在产品
                int reportNum = Convert.ToInt32(form["sqz129_report"]);
                int checkNum = Convert.ToInt32(form["sqz129_check"]);
                var item = offlineDB.Sales_Details.SingleOrDefault(m => m.Sales_Data_Id == sales.Id && m.Product_Id == 8);
                if (item != null)
                {
                    // 商品已存在
                    item.Report_Num = reportNum;
                    item.Checkout_Num = checkNum;
                }
                else
                {
                    // 商品不存在，需要添加
                    Sales_Details detail = new Sales_Details()
                    {
                        Product_Id = 8,
                        Sales_Data = sales,
                        Checkout_Num = checkNum,
                        Report_Num = reportNum
                    };
                    offlineDB.Sales_Details.Add(detail);
                }
            }
            else if (form["sqz129_report"] == null || form["sqz129_check"] == null)
            {
                // 表单内不存在产品
                var item = offlineDB.Sales_Details.SingleOrDefault(m => m.Sales_Data_Id == sales.Id && m.Product_Id == 8);
                if (item != null)
                {
                    // 商品已存在，需要删除
                    offlineDB.Sales_Details.Remove(item);
                }
            }
            // 检查生姜红茶
            if (form["sqz130_report"] != null && form["sqz130_check"] != null)
            {
                // 表单内存在产品
                int reportNum = Convert.ToInt32(form["sqz130_report"]);
                int checkNum = Convert.ToInt32(form["sqz130_check"]);
                var item = offlineDB.Sales_Details.SingleOrDefault(m => m.Sales_Data_Id == sales.Id && m.Product_Id == 9);
                if (item != null)
                {
                    // 商品已存在
                    item.Report_Num = reportNum;
                    item.Checkout_Num = checkNum;
                }
                else
                {
                    // 商品不存在，需要添加
                    Sales_Details detail = new Sales_Details()
                    {
                        Product_Id = 9,
                        Sales_Data = sales,
                        Checkout_Num = checkNum,
                        Report_Num = reportNum
                    };
                    offlineDB.Sales_Details.Add(detail);
                }
            }
            else if (form["sqz130_report"] == null || form["sqz130_check"] == null)
            {
                // 表单内不存在产品
                var item = offlineDB.Sales_Details.SingleOrDefault(m => m.Sales_Data_Id == sales.Id && m.Product_Id == 1);
                if (item != null)
                {
                    // 商品已存在，需要删除
                    offlineDB.Sales_Details.Remove(item);
                }
            }
            offlineDB.SaveChanges();
        }
        /************* 月度门店销售表 **************/
        public ActionResult List_Store_Sales_Month()
        {
            var item = from m in offlineDB.Store_Sales_Month
                       select m;
            return View(item);
        }

        public ActionResult Create_Store_Sales_Month()
        {
            ViewBag.Store_System = new SelectList(offlineDB.Store_System, "Id", "System_Name");
            var first_Store_System = offlineDB.Store_System.FirstOrDefault();
            ViewBag.Store = new SelectList(offlineDB.Store.Where(m => m.Store_System_Id == first_Store_System.Id), "Id", "Store_Name");
            ViewBag.ProductDetails = new List<Form_Product_Sales_Month>();
            ViewBag.Product = new SelectList(offlineDB.Product, "Product_Code", "Product_Name");
            Store_Sales_Month_ViewModel model = new Store_Sales_Month_ViewModel();
            return View(model);
        }
        [HttpPost]
        public ActionResult Create_Store_Sales_Month(Store_Sales_Month_ViewModel model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                // 同一日期、同一店铺已存在数据
                var exist = offlineDB.Store_Sales_Month.SingleOrDefault(m => m.Sales_Year == model.Sales_Year && m.Sales_Month == model.Sales_Month && m.Store_Id == model.Store_Id);
                // 没有数据，可以添加
                if (exist == null)
                {
                    //var store = offlineDB.Store.SingleOrDefault(m => m.Id == model.Store_Id);
                    Store_Sales_Month item = new Store_Sales_Month()
                    {
                        Sales_Month = model.Sales_Month,
                        Sales_Year = model.Sales_Year,
                        Store_Id = model.Store_Id
                    };
                    offlineDB.Store_Sales_Month.Add(item);
                    offlineDB.SaveChanges();
                    Progress_Store_Sales_Month_Details(item, form);
                    return RedirectToAction("List_Store_Sales_Month");
                }
                else
                {
                    var store = offlineDB.Store.SingleOrDefault(m => m.Id == model.Store_Id);
                    ViewBag.Store_System = new SelectList(offlineDB.Store_System, "Id", "System_Name");
                    ViewBag.Store = new SelectList(offlineDB.Store.Where(m => m.Store_System_Id == store.Store_System_Id), "Id", "Store_Name", model.Store_Id);
                    ViewBag.ProductDetails = GetForm_Sales_Month(form);
                    ViewBag.Product = new SelectList(offlineDB.Product, "Product_Code", "Product_Name");
                    ModelState.AddModelError("", "同一门店，同一店铺数据不能重复");
                    return View(model);
                }
            }
            else
            {
                var store = offlineDB.Store.SingleOrDefault(m => m.Id == model.Store_Id);
                ViewBag.Store_System = new SelectList(offlineDB.Store_System, "Id", "System_Name");
                ViewBag.Store = new SelectList(offlineDB.Store.Where(m => m.Store_System_Id == store.Store_System_Id), "Id", "Store_Name", model.Store_Id);
                ViewBag.ProductDetails = GetForm_Sales_Month(form);
                ViewBag.Product = new SelectList(offlineDB.Product, "Product_Code", "Product_Name");
                ModelState.AddModelError("", "信息错误");
                return View(model);
            }
        }

        public ActionResult Edit_Store_Sales_Month(int id)
        {
            var item = offlineDB.Store_Sales_Month.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var store = offlineDB.Store.SingleOrDefault(m => m.Id == item.Store_Id);
                ViewBag.Store_System = new SelectList(offlineDB.Store_System, "Id", "System_Name");
                ViewBag.Store = new SelectList(offlineDB.Store.Where(m => m.Store_System_Id == store.Store_System_Id), "Id", "Store_Name", item.Store_Id);
                List<Form_Product_Sales_Month> productlist = new List<Form_Product_Sales_Month>();
                var getlist = from m in offlineDB.Store_Sales_Month_Details
                              where m.Store_Sales_Month_Id == item.Id
                              select m;
                foreach (var i in getlist)
                {
                    Form_Product_Sales_Month j = new Form_Product_Sales_Month()
                    {
                        Product_Code = i.Product.Product_Code,
                        Product_Name = i.Product.Product_Name,
                        Sales_Count = i.Sales_Num,
                        Sales_Amount = i.Sales_Amount
                    };
                    productlist.Add(j);
                }
                ViewBag.ProductDetails = productlist;
                ViewBag.Product = new SelectList(offlineDB.Product, "Product_Code", "Product_Name");
                Store_Sales_Month_ViewModel model = new Store_Sales_Month_ViewModel()
                {
                    Store_Id = item.Store_Id,
                    Sales_Month = item.Sales_Month,
                    Sales_Year = item.Sales_Year
                };
                return View(model);
            }
            else
            {
                return View("Error");
            }
        }
        [HttpPost]
        public ActionResult Edit_Store_Sales_Month(int id, Store_Sales_Month_ViewModel model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                // 同一门店，同一日期不能重复
                int exist_count = (from m in offlineDB.Store_Sales_Month
                                   where m.Sales_Year == model.Sales_Year
                                   && m.Sales_Month == model.Sales_Month
                                   && m.Store_Id == model.Store_Id
                                   && m.Id != id
                                   select m).Count();
                if (exist_count == 0)
                {
                    var item = offlineDB.Store_Sales_Month.SingleOrDefault(m => m.Id == id);
                    if (offlineDB != null)
                    {
                        item.Sales_Month = model.Sales_Month;
                        item.Sales_Year = model.Sales_Year;
                        item.Store_Id = model.Store_Id;
                        offlineDB.SaveChanges();
                        Progress_Store_Sales_Month_Details(item, form);
                        return RedirectToAction("List_Store_Sales_Month");
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                else
                {
                    var store = offlineDB.Store.SingleOrDefault(m => m.Id == model.Store_Id);
                    ViewBag.Store_System = new SelectList(offlineDB.Store_System, "Id", "System_Name");
                    ViewBag.Store = new SelectList(offlineDB.Store.Where(m => m.Store_System_Id == store.Store_System_Id), "Id", "Store_Name", model.Store_Id);
                    ViewBag.ProductDetails = GetForm_Sales_Month(form);
                    ViewBag.Product = new SelectList(offlineDB.Product, "Product_Code", "Product_Name");
                    ModelState.AddModelError("", "同一时间、同一门店信息不能重复");
                    return View(model);
                }

            }
            else
            {
                var store = offlineDB.Store.SingleOrDefault(m => m.Id == model.Store_Id);
                ViewBag.Store_System = new SelectList(offlineDB.Store_System, "Id", "System_Name");
                ViewBag.Store = new SelectList(offlineDB.Store.Where(m => m.Store_System_Id == store.Store_System_Id), "Id", "Store_Name", model.Store_Id);
                ViewBag.ProductDetails = GetForm_Sales_Month(form);
                ViewBag.Product = new SelectList(offlineDB.Product, "Product_Code", "Product_Name");
                ModelState.AddModelError("", "信息错误");
                return View(model);
            }
        }

        public ActionResult Delete_Store_Sales_Month(int id)
        {
            var item = offlineDB.Store_Sales_Month.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                return View(item);
            }
            else
            {
                return View("Error");
            }
        }
        [HttpPost]
        public ActionResult Delete_Store_Sales_Month(int id, Store_Sales_Month model)
        {
            var item = offlineDB.Store_Sales_Month.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var list = from m in offlineDB.Store_Sales_Month_Details
                           where m.Store_Sales_Month.Id == model.Id
                           select m;
                offlineDB.Store_Sales_Month_Details.RemoveRange(list);
                offlineDB.Store_Sales_Month.Remove(item);
                offlineDB.SaveChanges();
                return RedirectToAction("List_Store_Sales_Month");
            }
            else
            {
                return View("Error");
            }
        }

        private List<Form_Product_Sales_Month> GetForm_Sales_Month(FormCollection form)
        {
            List<Form_Product_Sales_Month> list = new List<Form_Product_Sales_Month>();
            // 检查红糖姜茶
            if (form["sqz122_count"] != null && form["sqz122_amount"] != null)
            {
                // 表单内存在产品
                int count = Convert.ToInt32(form["sqz122_count"]);
                decimal amount = Convert.ToDecimal(form["sqz122_amount"]);
                Form_Product_Sales_Month item = new Form_Product_Sales_Month()
                {
                    Product_Code = "sqz122",
                    Product_Name = "红糖姜茶",
                    Sales_Amount = amount,
                    Sales_Count = count
                };
                list.Add(item);
            }
            // 检查黑糖姜茶
            if (form["sqz123_count"] != null && form["sqz123_amount"] != null)
            {
                // 表单内存在产品
                int count = Convert.ToInt32(form["sqz123_count"]);
                decimal amount = Convert.ToDecimal(form["sqz123_amount"]);
                Form_Product_Sales_Month item = new Form_Product_Sales_Month()
                {
                    Product_Code = "sqz123",
                    Product_Name = "黑糖姜茶",
                    Sales_Amount = amount,
                    Sales_Count = count
                };
                list.Add(item);
            }
            // 检查蜂蜜姜茶
            if (form["sqz124_count"] != null && form["sqz124_amount"] != null)
            {
                // 表单内存在产品
                int count = Convert.ToInt32(form["sqz124_count"]);
                decimal amount = Convert.ToDecimal(form["sqz124_amount"]);
                Form_Product_Sales_Month item = new Form_Product_Sales_Month()
                {
                    Product_Code = "sqz124",
                    Product_Name = "蜂蜜姜茶",
                    Sales_Amount = amount,
                    Sales_Count = count
                };
                list.Add(item);
            }
            // 检查柠檬姜茶
            if (form["sqz125_count"] != null && form["sqz125_amount"] != null)
            {
                // 表单内存在产品
                int count = Convert.ToInt32(form["sqz125_count"]);
                decimal amount = Convert.ToDecimal(form["sqz125_amount"]);
                Form_Product_Sales_Month item = new Form_Product_Sales_Month()
                {
                    Product_Code = "sqz125",
                    Product_Name = "柠檬姜茶",
                    Sales_Amount = amount,
                    Sales_Count = count
                };
                list.Add(item);
            }
            // 检查红枣姜茶
            if (form["sqz126_count"] != null && form["sqz126_amount"] != null)
            {
                // 表单内存在产品
                int count = Convert.ToInt32(form["sqz126_count"]);
                decimal amount = Convert.ToDecimal(form["sqz126_amount"]);
                Form_Product_Sales_Month item = new Form_Product_Sales_Month()
                {
                    Product_Code = "sqz126",
                    Product_Name = "红枣姜茶",
                    Sales_Amount = amount,
                    Sales_Count = count
                };
                list.Add(item);
            }
            // 检查薄荷姜茶
            if (form["sqz127_count"] != null && form["sqz127_amount"] != null)
            {
                // 表单内存在产品
                int count = Convert.ToInt32(form["sqz127_count"]);
                decimal amount = Convert.ToDecimal(form["sqz127_amount"]);
                Form_Product_Sales_Month item = new Form_Product_Sales_Month()
                {
                    Product_Code = "sqz127",
                    Product_Name = "薄荷姜茶",
                    Sales_Amount = amount,
                    Sales_Count = count
                };
                list.Add(item);
            }
            // 检查蜂蜜菊花茶
            if (form["sqz128_count"] != null && form["sqz128_amount"] != null)
            {
                // 表单内存在产品
                int count = Convert.ToInt32(form["sqz128_count"]);
                decimal amount = Convert.ToDecimal(form["sqz128_amount"]);
                Form_Product_Sales_Month item = new Form_Product_Sales_Month()
                {
                    Product_Code = "sqz128",
                    Product_Name = "蜂蜜菊花茶",
                    Sales_Amount = amount,
                    Sales_Count = count
                };
                list.Add(item);
            }
            // 检查陈皮酸梅汤
            if (form["sqz129_count"] != null && form["sqz129_amount"] != null)
            {
                // 表单内存在产品
                int count = Convert.ToInt32(form["sqz129_count"]);
                decimal amount = Convert.ToDecimal(form["sqz129_amount"]);
                Form_Product_Sales_Month item = new Form_Product_Sales_Month()
                {
                    Product_Code = "sqz129",
                    Product_Name = "陈皮酸梅汤",
                    Sales_Amount = amount,
                    Sales_Count = count
                };
                list.Add(item);
            }
            // 检查生姜红茶
            if (form["sqz130_count"] != null && form["sqz130_amount"] != null)
            {
                // 表单内存在产品
                int count = Convert.ToInt32(form["sqz130_count"]);
                decimal amount = Convert.ToDecimal(form["sqz130_amount"]);
                Form_Product_Sales_Month item = new Form_Product_Sales_Month()
                {
                    Product_Code = "sqz130",
                    Product_Name = "生姜红茶",
                    Sales_Amount = amount,
                    Sales_Count = count
                };
                list.Add(item);
            }
            return list;
        }

        private void Progress_Store_Sales_Month_Details(Store_Sales_Month data, FormCollection form)
        {

            // 检查红糖姜茶
            if (form["sqz122_count"] != null && form["sqz122_amount"] != null)
            {
                // 表单内存在产品
                int count = Convert.ToInt32(form["sqz122_count"]);
                decimal amount = Convert.ToDecimal(form["sqz122_amount"]);
                var item = offlineDB.Store_Sales_Month_Details.SingleOrDefault(m => m.Store_Sales_Month_Id == data.Id && m.Product_Id == 1);
                if (item != null)
                {
                    // 商品已存在
                    item.Sales_Num = count;
                    item.Sales_Amount = amount;
                }
                else
                {
                    // 商品不存在，需要添加
                    Store_Sales_Month_Details detail = new Store_Sales_Month_Details()
                    {
                        Product_Id = 1,
                        Store_Sales_Month_Id = data.Id,
                        Sales_Num = count,
                        Sales_Amount = amount
                    };
                    offlineDB.Store_Sales_Month_Details.Add(detail);
                }
            }
            else if (form["sqz122_count"] == null || form["sqz122_amount"] == null)
            {
                // 表单内不存在产品
                var item = offlineDB.Store_Sales_Month_Details.SingleOrDefault(m => m.Store_Sales_Month_Id == data.Id && m.Product_Id == 1);
                if (item != null)
                {
                    // 商品已存在，需要删除
                    offlineDB.Store_Sales_Month_Details.Remove(item);
                }
            }
            // 检查黑糖姜茶
            if (form["sqz123_count"] != null && form["sqz123_amount"] != null)
            {
                // 表单内存在产品
                int count = Convert.ToInt32(form["sqz123_count"]);
                decimal amount = Convert.ToDecimal(form["sqz123_amount"]);
                var item = offlineDB.Store_Sales_Month_Details.SingleOrDefault(m => m.Store_Sales_Month_Id == data.Id && m.Product_Id == 2);
                if (item != null)
                {
                    // 商品已存在
                    item.Sales_Num = count;
                    item.Sales_Amount = amount;
                }
                else
                {
                    // 商品不存在，需要添加
                    Store_Sales_Month_Details detail = new Store_Sales_Month_Details()
                    {
                        Product_Id = 2,
                        Store_Sales_Month_Id = data.Id,
                        Sales_Num = count,
                        Sales_Amount = amount
                    };
                    offlineDB.Store_Sales_Month_Details.Add(detail);
                }
            }
            else if (form["sqz123_count"] == null || form["sqz123_amount"] == null)
            {
                // 表单内不存在产品
                var item = offlineDB.Store_Sales_Month_Details.SingleOrDefault(m => m.Store_Sales_Month_Id == data.Id && m.Product_Id == 2);
                if (item != null)
                {
                    // 商品已存在，需要删除
                    offlineDB.Store_Sales_Month_Details.Remove(item);
                }
            }

            // 检查蜂蜜姜茶
            if (form["sqz124_count"] != null && form["sqz124_amount"] != null)
            {
                // 表单内存在产品
                int count = Convert.ToInt32(form["sqz124_count"]);
                decimal amount = Convert.ToDecimal(form["sqz124_amount"]);
                var item = offlineDB.Store_Sales_Month_Details.SingleOrDefault(m => m.Store_Sales_Month_Id == data.Id && m.Product_Id == 3);
                if (item != null)
                {
                    // 商品已存在
                    item.Sales_Num = count;
                    item.Sales_Amount = amount;
                }
                else
                {
                    // 商品不存在，需要添加
                    Store_Sales_Month_Details detail = new Store_Sales_Month_Details()
                    {
                        Product_Id = 3,
                        Store_Sales_Month_Id = data.Id,
                        Sales_Num = count,
                        Sales_Amount = amount
                    };
                    offlineDB.Store_Sales_Month_Details.Add(detail);
                }
            }
            else if (form["sqz124_count"] == null || form["sqz124_amount"] == null)
            {
                // 表单内不存在产品
                var item = offlineDB.Store_Sales_Month_Details.SingleOrDefault(m => m.Store_Sales_Month_Id == data.Id && m.Product_Id == 3);
                if (item != null)
                {
                    // 商品已存在，需要删除
                    offlineDB.Store_Sales_Month_Details.Remove(item);
                }
            }
            // 检查柠檬姜茶
            if (form["sqz125_count"] != null && form["sqz125_amount"] != null)
            {
                // 表单内存在产品
                int count = Convert.ToInt32(form["sqz125_count"]);
                decimal amount = Convert.ToDecimal(form["sqz125_amount"]);
                var item = offlineDB.Store_Sales_Month_Details.SingleOrDefault(m => m.Store_Sales_Month_Id == data.Id && m.Product_Id == 4);
                if (item != null)
                {
                    // 商品已存在
                    item.Sales_Num = count;
                    item.Sales_Amount = amount;
                }
                else
                {
                    // 商品不存在，需要添加
                    Store_Sales_Month_Details detail = new Store_Sales_Month_Details()
                    {
                        Product_Id = 4,
                        Store_Sales_Month_Id = data.Id,
                        Sales_Num = count,
                        Sales_Amount = amount
                    };
                    offlineDB.Store_Sales_Month_Details.Add(detail);
                }
            }
            else if (form["sqz125_count"] == null || form["sqz125_amount"] == null)
            {
                // 表单内不存在产品
                var item = offlineDB.Store_Sales_Month_Details.SingleOrDefault(m => m.Store_Sales_Month_Id == data.Id && m.Product_Id == 4);
                if (item != null)
                {
                    // 商品已存在，需要删除
                    offlineDB.Store_Sales_Month_Details.Remove(item);
                }
            }
            // 检查红枣姜茶
            if (form["sqz126_count"] != null && form["sqz126_amount"] != null)
            {
                // 表单内存在产品
                int count = Convert.ToInt32(form["sqz126_count"]);
                decimal amount = Convert.ToDecimal(form["sqz126_amount"]);
                var item = offlineDB.Store_Sales_Month_Details.SingleOrDefault(m => m.Store_Sales_Month_Id == data.Id && m.Product_Id == 5);
                if (item != null)
                {
                    // 商品已存在
                    item.Sales_Num = count;
                    item.Sales_Amount = amount;
                }
                else
                {
                    // 商品不存在，需要添加
                    Store_Sales_Month_Details detail = new Store_Sales_Month_Details()
                    {
                        Product_Id = 5,
                        Store_Sales_Month_Id = data.Id,
                        Sales_Num = count,
                        Sales_Amount = amount
                    };
                    offlineDB.Store_Sales_Month_Details.Add(detail);
                }
            }
            else if (form["sqz126_count"] == null || form["sqz126_amount"] == null)
            {
                // 表单内不存在产品
                var item = offlineDB.Store_Sales_Month_Details.SingleOrDefault(m => m.Store_Sales_Month_Id == data.Id && m.Product_Id == 5);
                if (item != null)
                {
                    // 商品已存在，需要删除
                    offlineDB.Store_Sales_Month_Details.Remove(item);
                }
            }
            // 检查薄荷姜茶
            if (form["sqz127_count"] != null && form["sqz127_amount"] != null)
            {
                // 表单内存在产品
                int count = Convert.ToInt32(form["sqz127_count"]);
                decimal amount = Convert.ToDecimal(form["sqz127_amount"]);
                var item = offlineDB.Store_Sales_Month_Details.SingleOrDefault(m => m.Store_Sales_Month_Id == data.Id && m.Product_Id == 6);
                if (item != null)
                {
                    // 商品已存在
                    item.Sales_Num = count;
                    item.Sales_Amount = amount;
                }
                else
                {
                    // 商品不存在，需要添加
                    Store_Sales_Month_Details detail = new Store_Sales_Month_Details()
                    {
                        Product_Id = 6,
                        Store_Sales_Month_Id = data.Id,
                        Sales_Num = count,
                        Sales_Amount = amount
                    };
                    offlineDB.Store_Sales_Month_Details.Add(detail);
                }
            }
            else if (form["sqz127_count"] == null || form["sqz127_amount"] == null)
            {
                // 表单内不存在产品
                var item = offlineDB.Store_Sales_Month_Details.SingleOrDefault(m => m.Store_Sales_Month_Id == data.Id && m.Product_Id == 6);
                if (item != null)
                {
                    // 商品已存在，需要删除
                    offlineDB.Store_Sales_Month_Details.Remove(item);
                }
            }
            // 检查蜂蜜菊花茶
            if (form["sqz128_count"] != null && form["sqz128_amount"] != null)
            {
                // 表单内存在产品
                int count = Convert.ToInt32(form["sqz128_count"]);
                decimal amount = Convert.ToDecimal(form["sqz128_amount"]);
                var item = offlineDB.Store_Sales_Month_Details.SingleOrDefault(m => m.Store_Sales_Month_Id == data.Id && m.Product_Id == 7);
                if (item != null)
                {
                    // 商品已存在
                    item.Sales_Num = count;
                    item.Sales_Amount = amount;
                }
                else
                {
                    // 商品不存在，需要添加
                    Store_Sales_Month_Details detail = new Store_Sales_Month_Details()
                    {
                        Product_Id = 7,
                        Store_Sales_Month_Id = data.Id,
                        Sales_Num = count,
                        Sales_Amount = amount
                    };
                    offlineDB.Store_Sales_Month_Details.Add(detail);
                }
            }
            else if (form["sqz128_count"] == null || form["sqz128_amount"] == null)
            {
                // 表单内不存在产品
                var item = offlineDB.Store_Sales_Month_Details.SingleOrDefault(m => m.Store_Sales_Month_Id == data.Id && m.Product_Id == 7);
                if (item != null)
                {
                    // 商品已存在，需要删除
                    offlineDB.Store_Sales_Month_Details.Remove(item);
                }
            }
            // 检查陈皮酸梅
            if (form["sqz129_count"] != null && form["sqz129_amount"] != null)
            {
                // 表单内存在产品
                int count = Convert.ToInt32(form["sqz129_count"]);
                decimal amount = Convert.ToDecimal(form["sqz129_amount"]);
                var item = offlineDB.Store_Sales_Month_Details.SingleOrDefault(m => m.Store_Sales_Month_Id == data.Id && m.Product_Id == 8);
                if (item != null)
                {
                    // 商品已存在
                    item.Sales_Num = count;
                    item.Sales_Amount = amount;
                }
                else
                {
                    // 商品不存在，需要添加
                    Store_Sales_Month_Details detail = new Store_Sales_Month_Details()
                    {
                        Product_Id = 8,
                        Store_Sales_Month_Id = data.Id,
                        Sales_Num = count,
                        Sales_Amount = amount
                    };
                    offlineDB.Store_Sales_Month_Details.Add(detail);
                }
            }
            else if (form["sqz129_count"] == null || form["sqz129_amount"] == null)
            {
                // 表单内不存在产品
                var item = offlineDB.Store_Sales_Month_Details.SingleOrDefault(m => m.Store_Sales_Month_Id == data.Id && m.Product_Id == 8);
                if (item != null)
                {
                    // 商品已存在，需要删除
                    offlineDB.Store_Sales_Month_Details.Remove(item);
                }
            }
            // 检查生姜红茶
            if (form["sqz130_count"] != null && form["sqz130_amount"] != null)
            {
                // 表单内存在产品
                int count = Convert.ToInt32(form["sqz130_count"]);
                decimal amount = Convert.ToDecimal(form["sqz130_amount"]);
                var item = offlineDB.Store_Sales_Month_Details.SingleOrDefault(m => m.Store_Sales_Month_Id == data.Id && m.Product_Id == 9);
                if (item != null)
                {
                    // 商品已存在
                    item.Sales_Num = count;
                    item.Sales_Amount = amount;
                }
                else
                {
                    // 商品不存在，需要添加
                    Store_Sales_Month_Details detail = new Store_Sales_Month_Details()
                    {
                        Product_Id = 9,
                        Store_Sales_Month_Id = data.Id,
                        Sales_Num = count,
                        Sales_Amount = amount
                    };
                    offlineDB.Store_Sales_Month_Details.Add(detail);
                }
            }
            else if (form["sqz130_count"] == null || form["sqz130_amount"] == null)
            {
                // 表单内不存在产品
                var item = offlineDB.Store_Sales_Month_Details.SingleOrDefault(m => m.Store_Sales_Month_Id == data.Id && m.Product_Id == 9);
                if (item != null)
                {
                    // 商品已存在，需要删除
                    offlineDB.Store_Sales_Month_Details.Remove(item);
                }
            }
            offlineDB.SaveChanges();
        }
        #endregion 

        public ActionResult Off_Store_main()
        {
            return View();
        }
        public PartialViewResult Off_Store_ajaxlist(int? page, string query)
        {
            int _page = page ?? 1;
            if (query == null || query == "")
            {

                var list = (from m in offlineDB.Off_Store
                            orderby m.Id descending
                            select m).ToPagedList(_page, 50);
                return PartialView(list);
            }
            else
            {
                var list = (from m in offlineDB.Off_Store
                            where m.StoreName.Contains(query) || m.Address.Contains(query)
                            orderby m.Id descending
                            select m).ToPagedList(_page, 100);
                return PartialView(list);
            }
        }

        public ActionResult Off_Sales_main()
        {
            return View();
        }
        public ActionResult Off_DailySalesInfo_ajaxlist(int? page, string query)
        {
            if (query == null || query == "")
            {
                int _page = page ?? 1;
                var list = (from m in offlineDB.Off_SalesInfo_Daily
                            orderby m.Date descending
                            select m).ToPagedList(_page, 50);
                return PartialView(list);
            }
            else
            {
                int _page = page ?? 1;
                var list = (from m in offlineDB.Off_SalesInfo_Daily
                            where m.Off_Store.StoreName.Contains(query)
                            orderby m.Date descending
                            select m).ToPagedList(_page, 50);
                return PartialView(list);
            }
        }
        public ActionResult Off_Sales_Month()
        {
            return View();
        }
        public ActionResult Off_MonthSalesInfo_ajaxlist(int? page, string query)
        {
            if (query == null || query == "")
            {
                int _page = page ?? 1;
                var list = (from m in offlineDB.Off_SalesInfo_Month
                            orderby m.Date descending
                            select m).ToPagedList(_page, 50);
                return PartialView(list);
            }
            else
            {
                int _page = page ?? 1;
                var list = (from m in offlineDB.Off_SalesInfo_Month
                            where m.Off_Store.StoreName.Contains(query)
                            orderby m.Date descending
                            select m).ToPagedList(_page, 50);
                return PartialView(list);
            }
        }

        public ActionResult Off_Costs_main()
        {
            return View();
        }
        public ActionResult Off_Costs_ajaxlist(int? page, string query)
        {
            if (query == null || query == "")
            {
                int _page = page ?? 1;
                var list = (from m in offlineDB.Off_Costs
                            orderby m.ApplicationDate descending
                            select m).ToPagedList(_page, 50);
                return PartialView(list);
            }
            else
            {
                int _page = page ?? 1;
                var list = (from m in offlineDB.Off_Costs
                            where m.Off_Store.StoreName.Contains(query) || m.Off_Store.Distributor.Contains(query)
                            orderby m.ApplicationDate descending
                            select m).ToPagedList(_page, 50);
                return PartialView(list);
            }
        }
        public ActionResult Off_Seller_main()
        {
            return View();
        }
        public ActionResult Off_Seller_ajaxlist(int? page, string query)
        {
            if (query == null || query == "")
            {
                int _page = page ?? 1;
                var list = (from m in offlineDB.Off_Seller
                            orderby m.Id descending
                            select m).ToPagedList(_page, 50);
                return PartialView(list);
            }
            else
            {
                int _page = page ?? 1;
                var list = (from m in offlineDB.Off_Seller
                            where m.Name.Contains(query) || m.Off_Store.StoreName.Contains(query)
                            orderby m.Id descending
                            select m).ToPagedList(_page, 50);
                return PartialView(list);
            }
        }
        public ActionResult Off_Costs_StoreSystem()
        {
            return View();
        }
        public ActionResult Off_StoreSystemCosts_ajaxlist(int? page, string query)
        {
            if (query == null || query == "")
            {
                int _page = page ?? 1;
                var list = (from m in offlineDB.Off_StoreSystem_Costs
                            orderby m.ApplicationDate descending
                            select m).ToPagedList(_page, 50);
                return PartialView(list);
            }
            else
            {
                int _page = page ?? 1;
                var list = (from m in offlineDB.Off_StoreSystem_Costs
                            where m.Distributor.Contains(query) || m.StoreSystem.Contains(query)
                            orderby m.ApplicationDate descending
                            select m).ToPagedList(_page, 50);
                return PartialView(list);
            }
        }

        #region 上传店铺信息
        public ActionResult UploadStore()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UploadStore(FormCollection form)
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
                        var exist_item = offlineDB.Off_Store.SingleOrDefault(m => m.StoreName == storename);
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
                                UploadUser = User.Identity.Name
                            };
                            offlineDB.Off_Store.Add(store);
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
                    offlineDB.SaveChanges();
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
        public PartialViewResult Ajax_EditStore(int id)
        {
            var item = offlineDB.Off_Store.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                List<Object> attendance = new List<Object>();
                attendance.Add(new { Key = "华东", Value = "华东" });
                attendance.Add(new { Key = "外区", Value = "外区" });
                ViewBag.Regionlist = new SelectList(attendance, "Key", "Value", item.Region);
                return PartialView(item);
            }
            else
            {
                return PartialView("Error");
            }
        }
        [HttpPost]
        public ActionResult Ajax_EditStore(int id, FormCollection form)
        {
            var item = new Off_Store();
            if (TryUpdateModel(item))
            {
                item.UploadTime = DateTime.Now;
                item.UploadUser = User.Identity.Name;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Content("SUCCESS");
            }
            else
            {
                ModelState.AddModelError("", "错误");
                List<Object> attendance = new List<Object>();
                attendance.Add(new { Key = "华东", Value = "华东" });
                attendance.Add(new { Key = "外区", Value = "外区" });
                ViewBag.Regionlist = new SelectList(attendance, "Key", "Value", item.Region);
                return PartialView(item);
            }
        }
        #endregion

        #region 上传店铺日报信息
        public ActionResult UploadDailyInfo()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UploadDailyInfo(FormCollection form)
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
                    List<Excel_DataMessage> result = analyseExcel_DailyInfoTable(filename, messageList);
                }
            }
            else
            {
                messageList.Add(new Excel_DataMessage(0, "文件上传错误", true));
            }
            return View("UploadResult", messageList);
        }
        public List<Excel_DataMessage> analyseExcel_DailyInfoTable(string filename, List<Excel_DataMessage> messageList)
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
                        // 判断是否存在店铺
                        string storename = dr["店铺名称"].ToString();
                        var exist_store = offlineDB.Off_Store.SingleOrDefault(m => m.StoreName == storename);
                        if (exist_store == null)
                        {
                            messageList.Add(new Excel_DataMessage(i, "店铺不存在", true));
                            result_flag = false;
                            continue;
                        }
                        // 判断是否有促销员，如有促销员，判断存在销售员
                        string sellername = dr["促销员"].ToString();
                        Off_Seller exist_seller = null;
                        if (sellername != "")
                        {
                            exist_seller = offlineDB.Off_Seller.SingleOrDefault(m => m.Name == sellername);
                            if (exist_seller == null)
                            {
                                messageList.Add(new Excel_DataMessage(i, "销售员不存在", true));
                                result_flag = false;
                                continue;
                            }
                        }

                        // 判断是否含已有数据
                        DateTime info_date = Convert.ToDateTime(dr["日期"]);
                        var exist_dailyinfo = offlineDB.Off_SalesInfo_Daily.SingleOrDefault(m => m.Date == info_date && m.StoreId == exist_store.Id && m.isMultiple == false);
                        if (exist_dailyinfo != null)
                        {
                            messageList.Add(new Excel_DataMessage(i, "当日数据已存在", true));
                            result_flag = false;
                            continue;
                        }
                        else
                        {
                            int? attendance = null;
                            string attendance_info = dr["考勤"].ToString();
                            switch (attendance_info)
                            {
                                case "全勤":
                                    attendance = 0;
                                    break;
                                case "迟到":
                                    attendance = 1;
                                    break;
                                case "早退":
                                    attendance = 2;
                                    break;
                                case "旷工":
                                    attendance = 3;
                                    break;
                                default:
                                    attendance = null;
                                    break;
                            }
                            Off_SalesInfo_Daily dailyinfo = new Off_SalesInfo_Daily()
                            {
                                StoreId = exist_store.Id,
                                Date = info_date,
                                Item_Brown = ExcelOperation.ConvertInt(dr, "红糖姜茶"),
                                Item_Black = ExcelOperation.ConvertInt(dr, "黑糖姜茶"),
                                Item_Lemon = ExcelOperation.ConvertInt(dr, "柠檬姜茶"),
                                Item_Honey = ExcelOperation.ConvertInt(dr, "蜂蜜姜茶"),
                                Item_Dates = ExcelOperation.ConvertInt(dr, "红枣姜茶"),
                                Off_Seller = exist_seller,
                                Attendance = attendance,
                                Salary = ExcelOperation.ConvertDecimal(dr, "工资"),
                                Bonus = ExcelOperation.ConvertDecimal(dr, "奖金"),
                                Debit = ExcelOperation.ConvertDecimal(dr, "扣款"),
                                isMultiple = ExcelOperation.ConvertBoolean(dr, "多人"),
                                remarks = dr["备注"].ToString(),
                                UploadTime = DateTime.Now,
                                UploadUser = User.Identity.Name
                            };
                            offlineDB.Off_SalesInfo_Daily.Add(dailyinfo);
                            messageList.Add(new Excel_DataMessage(i, "数据类型验证成功", false));
                        }
                    }
                    catch (Exception e)
                    {
                        result_flag = false;
                        messageList.Add(new Excel_DataMessage(i, "表格格式错误," + e.ToString(), true));
                    }
                }
                if (result_flag)
                {
                    offlineDB.SaveChanges();
                    messageList.Add(new Excel_DataMessage(0, "保存成功", false));
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

        public PartialViewResult Ajax_EditDailyInfo(int id)
        {
            var item = offlineDB.Off_SalesInfo_Daily.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                var sellerlist = from m in offlineDB.Off_Seller
                                 where m.StoreId == item.StoreId
                                 select m;
                ViewBag.Sellerlist = new SelectList(sellerlist, "Id", "Name");
                List<Object> attendance = new List<Object>();
                attendance.Add(new { Key = 0, Value = "全勤" });
                attendance.Add(new { Key = 1, Value = "迟到" });
                attendance.Add(new { Key = 2, Value = "早退" });
                attendance.Add(new { Key = 3, Value = "旷工" });
                ViewBag.Attendancelist = new SelectList(attendance, "Key", "Value", item.Attendance);
                return PartialView(item);
            }
            else
            {
                return PartialView("Error");
            }
        }
        [HttpPost]
        public ActionResult Ajax_EditDailyInfo(int id, FormCollection form)
        {
            var item = new Off_SalesInfo_Daily();
            if (TryUpdateModel(item))
            {
                item.UploadTime = DateTime.Now;
                item.UploadUser = User.Identity.Name;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Content("SUCCESS");
            }
            else
            {
                ModelState.AddModelError("", "错误");
                var sellerlist = from m in offlineDB.Off_Seller
                                 where m.StoreId == item.StoreId
                                 select m;
                ViewBag.Sellerlist = new SelectList(sellerlist, "Id", "Name");
                List<Object> attendance = new List<Object>();
                attendance.Add(new { Key = 0, Value = "全勤" });
                attendance.Add(new { Key = 1, Value = "迟到" });
                attendance.Add(new { Key = 2, Value = "早退" });
                attendance.Add(new { Key = 3, Value = "旷工" });
                ViewBag.Attendance = new SelectList(attendance, "Key", "Value", item.Attendance);
                return PartialView(item);
            }
        }
        #endregion

        #region 上传月度门店销售表
        public ActionResult UploadMonthInfo()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UploadMonthInfo(FormCollection form)
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
                    List<Excel_DataMessage> result = analyseExcel_MonthInfoTable(filename, messageList);
                }
            }
            else
            {
                messageList.Add(new Excel_DataMessage(0, "文件上传错误", true));
            }
            return View("UploadResult", messageList);
        }
        public List<Excel_DataMessage> analyseExcel_MonthInfoTable(string filename, List<Excel_DataMessage> messageList)
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
                        // 判断是否存在店铺
                        string storename = dr["店铺名称"].ToString();
                        var exist_store = offlineDB.Off_Store.SingleOrDefault(m => m.StoreName == storename);
                        if (exist_store == null)
                        {
                            messageList.Add(new Excel_DataMessage(i, "店铺不存在", true));
                            result_flag = false;
                            continue;

                        }
                        // 判断是否含已有数据
                        DateTime info_date = Convert.ToDateTime(dr["月份"]);
                        info_date = new DateTime(info_date.Year, info_date.Month, 1);
                        var exist_dailyinfo = offlineDB.Off_SalesInfo_Daily.SingleOrDefault(m => m.Date.Year == info_date.Year && m.Date.Month == info_date.Month && m.StoreId == exist_store.Id);
                        if (exist_dailyinfo != null)
                        {
                            messageList.Add(new Excel_DataMessage(i, "当月数据已存在", true));
                            result_flag = false;
                            continue;
                        }
                        else
                        {
                            Off_SalesInfo_Month monthinfo = new Off_SalesInfo_Month()
                            {
                                StoreId = exist_store.Id,
                                Date = info_date,
                                Item_Brown = ExcelOperation.ConvertInt(dr, "红糖姜茶"),
                                Item_Black = ExcelOperation.ConvertInt(dr, "黑糖姜茶"),
                                Item_Lemon = ExcelOperation.ConvertInt(dr, "柠檬姜茶"),
                                Item_Honey = ExcelOperation.ConvertInt(dr, "蜂蜜姜茶"),
                                Item_Dates = ExcelOperation.ConvertInt(dr, "红枣姜茶"),
                                UploadTime = DateTime.Now,
                                UploadUser = User.Identity.Name
                            };
                            offlineDB.Off_SalesInfo_Month.Add(monthinfo);
                            messageList.Add(new Excel_DataMessage(i, "数据类型验证成功", false));
                        }
                    }
                    catch (Exception e)
                    {
                        result_flag = false;
                        messageList.Add(new Excel_DataMessage(i, "表格格式错误," + e.ToString(), true));
                    }

                }
                if (result_flag)
                {
                    offlineDB.SaveChanges();
                    messageList.Add(new Excel_DataMessage(0, "保存成功", false));
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
        public PartialViewResult Ajax_EditMonthInfo(int id)
        {
            var item = offlineDB.Off_SalesInfo_Month.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                return PartialView(item);
            }
            else
            {
                return PartialView("Error");
            }
        }
        [HttpPost]
        public ActionResult Ajax_EditMonthInfo(int id, FormCollection form)
        {
            var item = new Off_SalesInfo_Month();
            if (TryUpdateModel(item))
            {
                item.UploadTime = DateTime.Now;
                item.UploadUser = User.Identity.Name;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Content("SUCCESS");
            }
            else
            {
                ModelState.AddModelError("", "错误");

                return PartialView(item);
            }
        }
        #endregion

        #region 上传促销人员信息
        /*---------- 上传促销人员信息 ----------*/
        public ActionResult UploadSeller()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UploadSeller(FormCollection form)
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
                    List<Excel_DataMessage> result = analyseExcel_SellerTable(filename, messageList);
                }
            }
            else
            {
                messageList.Add(new Excel_DataMessage(0, "文件上传错误", true));
            }
            return View("UploadResult", messageList);
        }
        public List<Excel_DataMessage> analyseExcel_SellerTable(string filename, List<Excel_DataMessage> messageList)
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
                        // 判断是否存在店铺
                        string storename = dr["店铺名称"].ToString();
                        var exist_store = offlineDB.Off_Store.SingleOrDefault(m => m.StoreName == storename);
                        if (exist_store == null)
                        {
                            messageList.Add(new Excel_DataMessage(i, "店铺不存在", true));
                            result_flag = false;
                            continue;
                        }
                        // 判断是否含已有数据
                        string info_name = dr["姓名"].ToString();
                        //var exist_dailyinfo = offlineDB.Off_SalesInfo_Daily.SingleOrDefault(m => m.Date == info_date && m.StoreId == exist_store.Id);
                        var exist_seller = offlineDB.Off_Seller.SingleOrDefault(m => m.Name == info_name);
                        if (exist_seller != null)
                        {
                            messageList.Add(new Excel_DataMessage(i, "销售员信息已存在", true));
                            result_flag = false;
                            continue;
                        }
                        else
                        {
                            Off_Seller seller = new Off_Seller()
                            {
                                StoreId = exist_store.Id,
                                Name = info_name,
                                Mobile = dr["联系方式"].ToString(),
                                IdNumber = dr["身份证"].ToString(),
                                CardName = dr["开户行"].ToString(),
                                CardNo = dr["银行卡号"].ToString(),
                                UploadTime = DateTime.Now,
                                UploadUser = User.Identity.Name
                            };
                            offlineDB.Off_Seller.Add(seller);
                        }
                        messageList.Add(new Excel_DataMessage(i, "数据类型验证成功", false));
                    }
                    catch (Exception e)
                    {
                        messageList.Add(new Excel_DataMessage(i, "表格格式错误," + e.ToString(), true));
                        result_flag = false;
                    }
                }
                if (result_flag)
                {
                    offlineDB.SaveChanges();
                    messageList.Add(new Excel_DataMessage(0, "保存成功", false));
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
        public PartialViewResult Ajax_EditSeller(int id)
        {
            var item = offlineDB.Off_Seller.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                return PartialView(item);
            }
            else
            {
                return PartialView("Error");
            }
        }
        [HttpPost]
        public ActionResult Ajax_EditSeller(int id, FormCollection form)
        {
            var item = new Off_Seller();
            if (TryUpdateModel(item))
            {
                item.UploadTime = DateTime.Now;
                item.UploadUser = User.Identity.Name;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Content("SUCCESS");
            }
            else
            {
                ModelState.AddModelError("", "错误");
                return PartialView(item);
            }
        }
        public PartialViewResult Ajax_AddSeller()
        {
            var storelist = offlineDB.Off_Store.OrderBy(m => m.StoreName);
            ViewBag.Storelist = new SelectList(storelist, "Id", "StoreName");
            Off_Seller seller = new Off_Seller();
            return PartialView(seller);
        }
        [HttpPost]
        public ActionResult Ajax_AddSeller(FormCollection form)
        {
            var item = new Off_Seller();
            if (TryUpdateModel(item))
            {
                item.UploadUser = User.Identity.Name;
                item.UploadTime = DateTime.Now;
                offlineDB.Off_Seller.Add(item);
                offlineDB.SaveChanges();
                return Content("SUCCESS");
            }
            else
            {
                ModelState.AddModelError("", "错误");
                var storelist = offlineDB.Off_Store.OrderBy(m => m.StoreName);
                ViewBag.Storelist = new SelectList(storelist, "Id", "StoreName");
                return PartialView(item);
            }
        }
        #endregion

        #region 上传进场费用信息
        public ActionResult UploadStoreSystemCosts()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UploadStoreSystemCosts(FormCollection form)
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
                    List<Excel_DataMessage> result = analyseExcel_StoreSystemCostsTable(filename, messageList);
                }
            }
            else
            {
                messageList.Add(new Excel_DataMessage(0, "文件上传错误", true));
            }
            return View("UploadResult", messageList);
        }
        public List<Excel_DataMessage> analyseExcel_StoreSystemCostsTable(string filename, List<Excel_DataMessage> messageList)
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
                        decimal _totalfee = ExcelOperation.ConvertDecimal(dr, "总费用") == null ? 0 : Convert.ToDecimal(ExcelOperation.ConvertDecimal(dr, "总费用"));
                        decimal _cash = ExcelOperation.ConvertDecimal(dr, "已付现金") == null ? 0 : Convert.ToDecimal(ExcelOperation.ConvertDecimal(dr, "已付现金"));
                        decimal _mortagegegoods = ExcelOperation.ConvertDecimal(dr, "已货抵") == null ? 0 : Convert.ToDecimal(ExcelOperation.ConvertDecimal(dr, "已货抵"));
                        if (_totalfee == 0)
                        {
                            result_flag = false;
                            messageList.Add(new Excel_DataMessage(i, "总费用不能为空", true));
                            continue;
                        }
                        if (_totalfee < _cash + _mortagegegoods)
                        {
                            result_flag = false;
                            messageList.Add(new Excel_DataMessage(i, "总费用不得小于现金+货抵", true));
                            continue;
                        }

                        Off_StoreSystem_Costs cost = new Off_StoreSystem_Costs()
                        {
                            Distributor = dr["经销商"].ToString(),
                            ApplicationDate = ExcelOperation.ConvertDateTime(dr, "申请日期") ?? DateTime.Now,
                            StoreSystem = dr["渠道"].ToString(),
                            TotalFee = _totalfee,
                            Cash = _cash,
                            MortgageGoods = _mortagegegoods,
                            Warrant = dr["收款人"].ToString(),
                            Checked = ExcelOperation.ConvertBoolean(dr, "审核"),
                            Completed = ExcelOperation.ConvertBoolean(dr, "完成"),
                            Canceled = ExcelOperation.ConvertBoolean(dr, "作废"),
                            UploadTime = DateTime.Now,
                            UploadUser = User.Identity.Name
                        };
                        offlineDB.Off_StoreSystem_Costs.Add(cost);
                        messageList.Add(new Excel_DataMessage(i, "数据类型验证成功", false));
                    }
                    catch (Exception e)
                    {
                        messageList.Add(new Excel_DataMessage(i, "表格格式错误," + e.ToString(), true));
                        result_flag = false;
                    }
                }
                if (result_flag)
                {
                    offlineDB.SaveChanges();
                    messageList.Add(new Excel_DataMessage(0, "保存成功", false));
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
        public PartialViewResult Ajax_EditStoreSystemCosts(int id)
        {
            var item = offlineDB.Off_StoreSystem_Costs.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                List<Object> warrant = new List<Object>();
                warrant.Add(new { Key = "经销商", Value = "经销商" });
                warrant.Add(new { Key = "门店", Value = "门店" });
                ViewBag.Warrantlist = new SelectList(warrant, "Key", "Value", item.Warrant);
                return PartialView(item);
            }
            else
            {
                return PartialView("Error");
            }
        }
        [HttpPost]
        public ActionResult Ajax_EditStoreSystemCosts(int id, FormCollection form)
        {
            var item = new Off_StoreSystem_Costs();
            if (TryUpdateModel(item))
            {
                item.UploadTime = DateTime.Now;
                item.UploadUser = User.Identity.Name;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Content("SUCCESS");
            }
            else
            {
                List<Object> warrantlist = new List<Object>();
                warrantlist.Add(new { Key = "经销商", Value = "经销商" });
                warrantlist.Add(new { Key = "门店", Value = "门店" });
                ViewBag.Warrant = new SelectList(warrantlist, "Key", "Value", item.Warrant);
                return PartialView(item);
            }
        }
        [HttpPost]
        public JsonResult Ajax_CheckStoreSystemCosts(int id)
        {
            try
            {
                var item = offlineDB.Off_StoreSystem_Costs.SingleOrDefault(m => m.Id == id);
                if (item != null)
                {
                    if (item.Checked)
                        item.Checked = false;
                    else
                        item.Checked = true;
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
            return Json(new { result = "FAIL" });
        }
        [HttpPost]
        public JsonResult Ajax_CompletedStoreSystemCosts(int id)
        {
            try
            {
                var item = offlineDB.Off_StoreSystem_Costs.SingleOrDefault(m => m.Id == id);
                if (item != null)
                {
                    if (item.Completed)
                        item.Completed = false;
                    else
                        item.Completed = true;
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
            return Json(new { result = "FAIL" });
        }
        [HttpPost]
        public JsonResult Ajax_CancelStoreSystemCosts(int id)
        {
            try
            {
                var item = offlineDB.Off_StoreSystem_Costs.SingleOrDefault(m => m.Id == id);
                if (item != null)
                {
                    if (item.Canceled)
                        item.Canceled = false;
                    else
                        item.Canceled = true;
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
            return Json(new { result = "FAIL" });
        }
        #endregion

        #region 上传活动费用信息
        public ActionResult UploadEventCosts()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UploadEventCosts(FormCollection form)
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
                    List<Excel_DataMessage> result = analyseExcel_EventCostsTable(filename, messageList);
                }
            }
            else
            {
                messageList.Add(new Excel_DataMessage(0, "文件上传错误", true));
            }
            return View("UploadResult", messageList);
        }
        public List<Excel_DataMessage> analyseExcel_EventCostsTable(string filename, List<Excel_DataMessage> messageList)
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
                        // 判断是否存在店铺
                        string storename = dr["店铺名称"].ToString();
                        var exist_store = offlineDB.Off_Store.SingleOrDefault(m => m.StoreName == storename);
                        if (exist_store == null)
                        {
                            messageList.Add(new Excel_DataMessage(i, "店铺不存在", true));
                            result_flag = false;
                            continue;
                        }
                        decimal _totalfee = ExcelOperation.ConvertDecimal(dr, "总费用") == null ? 0 : Convert.ToDecimal(ExcelOperation.ConvertDecimal(dr, "总费用"));
                        decimal _cash = ExcelOperation.ConvertDecimal(dr, "已付现金") == null ? 0 : Convert.ToDecimal(ExcelOperation.ConvertDecimal(dr, "已付现金"));
                        decimal _mortagegegoods = ExcelOperation.ConvertDecimal(dr, "已货抵") == null ? 0 : Convert.ToDecimal(ExcelOperation.ConvertDecimal(dr, "已货抵"));
                        if (_totalfee == 0)
                        {
                            result_flag = false;
                            messageList.Add(new Excel_DataMessage(i, "总费用不能为空", true));
                            continue;
                        }
                        if (_totalfee < _cash + _mortagegegoods)
                        {
                            result_flag = false;
                            messageList.Add(new Excel_DataMessage(i, "总费用不得小于现金+货抵", true));
                            continue;
                        }
                        Off_Costs costs = new Off_Costs()
                        {
                            ApplicationDate = ExcelOperation.ConvertDateTime(dr, "申请日期") ?? DateTime.Now,
                            StoreId = exist_store.Id,
                            StartDate = ExcelOperation.ConvertDateTime(dr, "开始时间"),
                            EndDate = ExcelOperation.ConvertDateTime(dr, "结束时间"),
                            Event_HB = ExcelOperation.ConvertBoolean(dr, "海报"),
                            Event_DT = ExcelOperation.ConvertBoolean(dr, "堆头"),
                            Event_TG = ExcelOperation.ConvertBoolean(dr, "TG"),
                            Event_DJ = ExcelOperation.ConvertBoolean(dr, "端架"),
                            Event_DL = ExcelOperation.ConvertBoolean(dr, "叠篮"),
                            Event_SY = ExcelOperation.ConvertBoolean(dr, "试饮"),
                            Event_TJ = ExcelOperation.ConvertBoolean(dr, "特价"),
                            Event_Other = ExcelOperation.ConvertBoolean(dr, "其他"),
                            TotalFee = _totalfee,
                            Cash = _cash,
                            MortgageGoods = _mortagegegoods,
                            Warrant = dr["收款方"].ToString(),
                            Checked = ExcelOperation.ConvertBoolean(dr, "审核"),
                            Completed = ExcelOperation.ConvertBoolean(dr, "完成"),
                            Canceled = ExcelOperation.ConvertBoolean(dr, "作废"),
                            UploadTime = DateTime.Now,
                            UploadUser = User.Identity.Name
                        };
                        offlineDB.Off_Costs.Add(costs);
                        messageList.Add(new Excel_DataMessage(i, "数据类型验证成功", false));
                    }
                    catch (Exception e)
                    {
                        messageList.Add(new Excel_DataMessage(i, "表格格式错误," + e.ToString(), true));
                        result_flag = false;
                    }
                }
                if (result_flag)
                {
                    offlineDB.SaveChanges();
                    messageList.Add(new Excel_DataMessage(0, "保存成功", false));
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
        public PartialViewResult Ajax_EditCosts(int id)
        {
            var item = offlineDB.Off_Costs.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                List<Object> warrant = new List<Object>();
                warrant.Add(new { Key = "经销商", Value = "经销商" });
                warrant.Add(new { Key = "门店", Value = "门店" });
                ViewBag.Warrant = new SelectList(warrant, "Key", "Value", item.Warrant);
                return PartialView(item);
            }
            else
            {
                return PartialView("Error");
            }
        }
        [HttpPost]
        public ActionResult Ajax_EditCosts(int id, FormCollection form)
        {
            var item = new Off_Costs();
            if (TryUpdateModel(item))
            {
                item.UploadTime = DateTime.Now;
                item.UploadUser = User.Identity.Name;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Content("SUCCESS");
            }
            else
            {
                List<Object> warrant = new List<Object>();
                warrant.Add(new { Key = "经销商", Value = "经销商" });
                warrant.Add(new { Key = "门店", Value = "门店" });
                ViewBag.Warrantlist = new SelectList(warrant, "Key", "Value", item.Warrant);
                return PartialView(item);
            }
        }
        [HttpPost]
        public JsonResult Ajax_CheckCosts(int id)
        {
            try
            {
                var item = offlineDB.Off_Costs.SingleOrDefault(m => m.Id == id);
                if (item != null)
                {
                    if (item.Checked)
                        item.Checked = false;
                    else
                        item.Checked = true;
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
            return Json(new { result = "FAIL" });
        }
        [HttpPost]
        public JsonResult Ajax_CompletedCosts(int id)
        {
            try
            {
                var item = offlineDB.Off_Costs.SingleOrDefault(m => m.Id == id);
                if (item != null)
                {
                    if (item.Completed)
                        item.Completed = false;
                    else
                        item.Completed = true;
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
            return Json(new { result = "FAIL" });
        }
        [HttpPost]
        public JsonResult Ajax_CancelCosts(int id)
        {
            try
            {
                var item = offlineDB.Off_Costs.SingleOrDefault(m => m.Id == id);
                if (item != null)
                {
                    if (item.Canceled)
                        item.Canceled = false;
                    else
                        item.Canceled = true;
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return Json(new { result = "SUCCESS" });
                }
            }
            catch
            {
                return Json(new { result = "FAIL" });
            }
            return Json(new { result = "FAIL" });
        }
        #endregion

        /* -----费用表----- */
        public ActionResult Off_Expenses_List()
        {
            return View();
        }
        public ActionResult Off_Expenses_Add()
        {
            List<Object> attendance = new List<Object>();
            attendance.Add(new { Key = 0, Value = "进场费" });
            attendance.Add(new { Key = 1, Value = "活动费" });
            ViewBag.PayType = new SelectList(attendance, "Key", "Value");
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Off_Expenses_Add(FormCollection form)
        {
            var item = new Off_Expenses();
            if (TryUpdateModel(item))
            {
                item.Status = 0;
                item.UploadTime = DateTime.Now;
                item.UploadUser = User.Identity.Name;
                offlineDB.Off_Expenses.Add(item);
                offlineDB.SaveChanges();
            }
            return RedirectToAction("Off_Expenses_List");
        }
        public ActionResult Off_Expenses_Edit(int id)
        {
            var item = offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                List<Object> attendance = new List<Object>();
                attendance.Add(new { Key = 0, Value = "进场费" });
                attendance.Add(new { Key = 1, Value = "活动费" });
                ViewBag.PayType = new SelectList(attendance, "Key", "Value", item.Status);
                return View(item);
            }
            else
                return View("Error");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Off_Expenses_Edit(int id, FormCollection form)
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
                        offlineDB.Off_Expenses_Details.Add(detailtemp);
                    }
                    else
                    {
                        int d_id = Convert.ToInt32(form.GetValues("detailid")[i]);
                        Off_Expenses_Details detailstemp = offlineDB.Off_Expenses_Details.SingleOrDefault(m => m.Id == d_id);
                        detailstemp.DetailsFee = Convert.ToDecimal(form.GetValues("detailfee")[i].ToString());
                        detailstemp.DetailsName = form.GetValues("detaillist")[i].ToString();
                        detailstemp.Remarks = form.GetValues("detailremarks")[i].ToString();
                        detailstemp.UploadTime = DateTime.Now;
                        detailstemp.UploadUser = User.Identity.Name;
                        offlineDB.Entry(detailstemp).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                item.UploadTime = DateTime.Now;
                item.UploadUser = User.Identity.Name;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
            }
            return RedirectToAction("Off_Expenses_List");
        }
        public ActionResult Off_Expenses_Balance(int id)
        {
            var item = offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                List<Object> attendance = new List<Object>();
                attendance.Add(new { Key = 0, Value = "进场费" });
                attendance.Add(new { Key = 1, Value = "活动费" });
                ViewBag.PayType = new SelectList(attendance, "Key", "Value", item.PaymentType);
                return View(item);
            }
            else
                return View("Error");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Off_Expenses_Balance(int id, FormCollection form)
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
                        offlineDB.Off_Expenses_Details.Add(detailtemp);
                    }
                    else
                    {
                        int d_id = Convert.ToInt32(form.GetValues("detailid")[i]);
                        Off_Expenses_Details detailstemp = offlineDB.Off_Expenses_Details.SingleOrDefault(m => m.Id == d_id);
                        detailstemp.DetailsFee = Convert.ToDecimal(form.GetValues("detailfee")[i].ToString());
                        detailstemp.DetailsName = form.GetValues("detaillist")[i].ToString();
                        detailstemp.Remarks = form.GetValues("detailremarks")[i].ToString();
                        detailstemp.UploadTime = DateTime.Now;
                        detailstemp.UploadUser = User.Identity.Name;
                        offlineDB.Entry(detailstemp).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                item.UploadTime = DateTime.Now;
                item.UploadUser = User.Identity.Name;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return RedirectToAction("Off_Expenses_List");
            }
            return View("Error");
        }
        public ActionResult Off_Expenses_VerifyCost(int id)
        {
            var item = offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                List<Object> attendance = new List<Object>();
                attendance.Add(new { Key = 0, Value = "进场费" });
                attendance.Add(new { Key = 1, Value = "活动费" });
                ViewBag.PayType = new SelectList(attendance, "Key", "Value", item.PaymentType);
                return View(item);
            }
            else
                return View("Error");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Off_Expenses_VerifyCost(FormCollection form)
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
                        offlineDB.Off_Expenses_Payment.Add(detailtemp);
                    }
                    else
                    {
                        int d_id = Convert.ToInt32(form.GetValues("detailid")[i]);
                        Off_Expenses_Payment detailstemp = offlineDB.Off_Expenses_Payment.SingleOrDefault(m => m.Id == d_id);
                        detailstemp.VerifyCost = Convert.ToDecimal(form.GetValues("detailfee")[i].ToString());
                        detailstemp.VerifyType = Convert.ToInt32(form.GetValues("detaillist")[i].ToString());
                        detailstemp.ApplicationDate = Convert.ToDateTime(form.GetValues("apdate")[i]);
                        detailstemp.Remarks = form.GetValues("detailremarks")[i].ToString();
                        detailstemp.UploadTime = DateTime.Now;
                        detailstemp.UploadUser = User.Identity.Name;
                        offlineDB.Entry(detailstemp).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                item.UploadTime = DateTime.Now;
                item.UploadUser = User.Identity.Name;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return RedirectToAction("Off_Expenses_List");
            }
            return View("Error");
        }
        [HttpPost]
        public JsonResult Off_Expenses_Details_Del(int id)
        {
            if (id == 0)
            {
                return Json(new { result = "SUCCESS" });
            }
            var item = offlineDB.Off_Expenses_Details.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                offlineDB.Off_Expenses_Details.Remove(item);
                offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }
        [HttpPost]
        public JsonResult Off_Expenses_Payment_Del(int id)
        {
            if (id == 0)
            {
                return Json(new { result = "SUCCESS" });
            }
            var item = offlineDB.Off_Expenses_Payment.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                offlineDB.Off_Expenses_Payment.Remove(item);
                offlineDB.SaveChanges();
                return Json(new { result = "SUCCESS" });
            }
            else
            {
                return Json(new { result = "FAIL" });
            }
        }
        public ActionResult Off_Expenses_Check(int id)
        {
            var item = offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                item.CheckTime = DateTime.Now;
                item.Status = 1;
                offlineDB.SaveChanges();
            }
            return RedirectToAction("Off_Expenses_List");
        }
        public ActionResult Off_Expenses_Verify_Submit(int id)
        {
            var item = offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                item.CheckTime = DateTime.Now;
                item.Status = 3;
                offlineDB.SaveChanges();
            }
            return RedirectToAction("Off_Expenses_List");
        }
        public ActionResult Off_Expenses_Details(int id)
        {
            var item = offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                List<Object> attendance = new List<Object>();
                attendance.Add(new { Key = 0, Value = "进场费" });
                attendance.Add(new { Key = 1, Value = "活动费" });
                ViewBag.PayType = new SelectList(attendance, "Key", "Value", item.PaymentType);
                List<Object> status = new List<Object>();
                status.Add(new { Key = 0, Value = "未审核" });
                status.Add(new { Key = 1, Value = "已审核" });
                status.Add(new { Key = 2, Value = "已结算" });
                status.Add(new { Key = 3, Value = "已核销" });
                status.Add(new { Key = -1, Value = "作废" });
                ViewBag.ExpensesStatus = new SelectList(status, "Key", "Value", item.Status);
                return View(item);
            }
            else
                return View("Error");

        }
        public ActionResult Off_Expenses_AjaxList(int? page)
        {
            int _page = page ?? 1;
            var list = (from m in offlineDB.Off_Expenses
                        where m.Status>=0
                        orderby m.Id descending
                        select m).ToPagedList(_page, 50);
            return PartialView(list);
        }
        public ActionResult Off_Expenses_Balance_Submit(int id)
        {
            var item = offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                item.BalanceTime = DateTime.Now;
                item.Status = 2;
                offlineDB.SaveChanges();
            }
            return RedirectToAction("Off_Expenses_List");
        }
        public ActionResult Off_Expenses_Cancel(int id)
        {
            var item = offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                item.UploadTime = DateTime.Now;
                item.UploadUser = User.Identity.Name;
                item.Status = -1;
                offlineDB.SaveChanges();
            }
            return RedirectToAction("Off_Expenses_List");
        }
        public ActionResult Off_Expenses_Details_Edit(int id)
        {
            var item = offlineDB.Off_Expenses.SingleOrDefault(m => m.Id == id);
            if (item != null)
                return View(item);
            else
                return View("Error");
        }
        [HttpPost]
        public ActionResult Off_Expenses_Details_Edit(FormCollection form)
        {
            var listcontent = "";
            int i = 0;
            foreach(var item in form.GetValues("list"))
            {
                listcontent += item.ToString();
                i++;
            }
            return Content(form["ExpensesId"].ToString() + "," + i + "<BR />" + listcontent);
        }


        public ActionResult UploadResult()
        {
            return View();
        }


        public ActionResult StoreMap()
        {
            return View();
        }

        [HttpPost]
        public JsonResult ajax_StoreSystem()
        {
            var storesystem = from m in offlineDB.Off_Store
                              group m by m.StoreSystem into g
                              select g.Key;
            return Json(new { storesystem = storesystem });
        }
        public JsonResult JsonStoreList(string storesystem)
        {
            if (storesystem == null || storesystem == "")
            {
                var list = from m in offlineDB.Off_Store
                           where m.Latitude != "" && m.Longitude != ""
                           orderby m.Id descending
                           select new { StoreName = m.StoreName, StoreSystem = m.StoreSystem, Address = m.Address, Longitude = m.Longitude, Latitude = m.Latitude };
                return Json(new { result = "SUCCESS", list = list }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                string[] systems = storesystem.Split(',');
                var list = from m in offlineDB.Off_Store
                           where systems.Contains(m.StoreSystem)
                           orderby m.Id descending
                           select new { StoreName = m.StoreName, StoreSystem = m.StoreSystem, Address = m.Address, Longitude = m.Longitude, Latitude = m.Latitude };
                return Json(new { result = "SUCCESS", list = list }, JsonRequestBehavior.AllowGet);
            }
        }
        public FileResult Ajax_downloadSalary(DateTime start, DateTime end)
        {
            var list = from m in offlineDB.Off_SalesInfo_Daily
                       where m.Date >= start && m.Date <= end
                       group m by m.Off_Seller into g
                       select new
                       {
                           Name = g.Key.Name,
                           StoreName = g.Key.Off_Store.StoreName,
                           Distributor= g.Key.Off_Store.Distributor,
                           Mobile = g.Key.Mobile,
                           IdNumber = g.Key.IdNumber,
                           CardName = g.Key.CardName,
                           CardNo = g.Key.CardNo,
                           Salary = g.Sum(m => m.Salary) == null ? 0 : g.Sum(m => m.Salary),
                           Bonus = g.Sum(m => m.Bonus) == null ? 0 : g.Sum(m => m.Bonus),
                           Debit =g.Sum(m=>m.Debit) == null? 0 : g.Sum(m=>m.Debit),
                           AccountName = g.Key.AccountName,
                           All = g.Count(m => m.Attendance == 0),
                           Delay = g.Count(m => m.Attendance == 1),
                           Leave = g.Count(m=> m.Attendance ==2),
                           Absence = g.Count(m=>m.Attendance ==3)
                       };
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            CsvWriter csv = new CsvWriter(writer);
            //string[] columname = new string[] {"店铺名称", "经销商", "姓名", "电话号码", "身份证号码", "开户行", "银行卡号", "工资", "奖金", "全勤天数", "迟到天数" };
            csv.WriteField("店铺名称");
            csv.WriteField("经销商");
            csv.WriteField("姓名");
            csv.WriteField("电话号码");
            csv.WriteField("身份证号码");
            csv.WriteField("开户行");
            csv.WriteField("开户人姓名");
            csv.WriteField("银行卡号");
            csv.WriteField("工资");
            csv.WriteField("奖金");
            csv.WriteField("扣款");
            csv.WriteField("全勤天数");
            csv.WriteField("迟到天数");
            csv.WriteField("早退天数");
            csv.WriteField("旷工天数");
            csv.NextRecord();
            foreach (var item in list)
            {
                csv.WriteField(item.StoreName);
                csv.WriteField(item.Distributor);
                csv.WriteField(item.Name);
                csv.WriteField("'" + item.Mobile);
                csv.WriteField("'" + item.IdNumber);
                csv.WriteField(item.CardName);
                csv.WriteField(item.AccountName);
                csv.WriteField("'" + item.CardNo);
                csv.WriteField(item.Salary);
                csv.WriteField(item.Bonus);
                csv.WriteField(item.Debit);
                csv.WriteField(item.All);
                csv.WriteField(item.Delay);
                csv.WriteField(item.Leave);
                csv.WriteField(item.Absence);
                csv.NextRecord();
            }
            //csv.WriteRecords(list);
            writer.Flush();
            writer.Close();
            return File(convertCSV(stream.ToArray()), "@text/csv", "工资信息" + start.ToShortDateString() + "-" + end.ToShortDateString() + ".csv");
        }
        #region 绑定促销员
        public ActionResult Off_BindSeller_List(string query)
        {
            if (query==null)
            {
                var list = offlineDB.Off_Membership_Bind.OrderByDescending(m => m.ApplicationDate);
                return View(list);
            }
            else
            {
                var list = from m in offlineDB.Off_Membership_Bind
                           where (m.NickName.Contains(query) || m.Off_Seller.Off_Store.StoreName.Contains(query))
                           select m;
                return View(list);
            }
        }
        public ActionResult Off_BindSeller(int id)
        {
            var item = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Id == id);
            return PartialView(item);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Off_BindSeller(int id, FormCollection form)
        {
            Off_Membership_Bind item = new Off_Membership_Bind();
            if (TryUpdateModel(item))
            {
                item.Bind = true;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Content("SUCCESS");
            }
            else
            {
                return View("Error");
            }
        }
        #endregion

        #region 绑定促销门店
        public ActionResult Off_ScheduleList(bool? history)
        {
            bool _history = history ?? false;
            var currentTime = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            if (_history)
            {
                var list = from m in offlineDB.Off_Checkin_Schedule
                           where m.Subscribe<currentTime
                           group m by m.Subscribe into g
                           orderby g.Key descending
                           select new ScheduleList { Subscribe = g.Key, Count = g.Count(), Unfinished = g.Count(m => m.Off_Checkin.Any(p => p.Status >= 3)) };
                return View(list);
            }
            else
            {
                var list = from m in offlineDB.Off_Checkin_Schedule
                           where m.Subscribe >= currentTime
                           group m by m.Subscribe into g
                           orderby g.Key
                           select new ScheduleList { Subscribe = g.Key, Count = g.Count(), Unfinished = g.Count(m => m.Off_Checkin.Any(p => p.Status >= 3)) };
                return View(list);
            }
            
        }
        public ActionResult Off_ScheduleDetails(string date)
        {
            DateTime day = DateTime.Parse(date);
            var list = from m in offlineDB.Off_Checkin_Schedule
                       where m.Subscribe == day
                       select m;
            return View(list);
        }
        public ActionResult Ajax_EditSchedule(int id)
        {
            var item = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == id);
            ViewBag.StoreName = item.Off_Store.StoreName;
            return PartialView(item);
        }
        [ValidateAntiForgeryToken, HttpPost]
        public ActionResult Ajax_EditSchedule(Off_Checkin_Schedule model)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin_Schedule item = new Off_Checkin_Schedule();
                if (TryUpdateModel(item))
                {
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return View("Error");
            }
            else
            {
                ModelState.AddModelError("", "请重试");
                ViewBag.StoreName = offlineDB.Off_Store.SingleOrDefault(m => m.Id == model.Off_Store_Id).StoreName;
                return PartialView(model);
            }
        }
        [HttpPost]
        public ActionResult Ajax_DeleteSchedule(int id)
        {
            var item = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == id);
            offlineDB.Off_Checkin_Schedule.Remove(item);
            offlineDB.SaveChanges();
            return Content("SUCCESS");
        }
        public ActionResult Off_Add_Schedule()
        {
            var storesystem = from m in offlineDB.Off_Store
                              group m by m.StoreSystem into g
                              orderby g.Key
                              select new { Key = g.Key, Value = g.Key };
            ViewBag.SystemList = new SelectList(storesystem, "Key", "Value", storesystem.FirstOrDefault().Value);
            return View();
        }
        [HttpPost]
        public ActionResult Off_Add_Schedule(FormCollection form)
        {
            StoreSchedule_ViewModel model = new StoreSchedule_ViewModel();
            if (TryUpdateModel(model))
            {
                if (ModelState.IsValid)
                {
                    if(model.StartDate > model.EndDate)
                    {
                        var storesystem = from m in offlineDB.Off_Store
                                          group m by m.StoreSystem into g
                                          orderby g.Key
                                          select new { Key = g.Key, Value = g.Key };
                        ViewBag.SystemList = new SelectList(storesystem, "Key", "Value", storesystem.FirstOrDefault().Value);
                        ModelState.AddModelError("", "开始日期不得大于结束日期");
                        return View(model);
                    }
                    if(form["StoreList"].ToString().Trim() == "")
                    {
                        var storesystem = from m in offlineDB.Off_Store
                                          group m by m.StoreSystem into g
                                          orderby g.Key
                                          select new { Key = g.Key, Value = g.Key };
                        ViewBag.SystemList = new SelectList(storesystem, "Key", "Value", storesystem.FirstOrDefault().Value);
                        ModelState.AddModelError("", "请至少选择一个店铺");
                        return View(model);
                    }
                    var datelength = Convert.ToInt32(model.EndDate.Subtract(model.StartDate).TotalDays);
                    // 每天循环
                    for (int i=0; i <= datelength; i++)
                    {
                        
                        string[] storelist = form["StoreList"].ToString().Split(',');
                        for (int j = 0; j < storelist.Length;j++)
                        {
                            string[] begintime = model.BeginTime.Split(':');
                            string[] finishtime = model.FinishTime.Split(':');
                            int year = model.StartDate.AddDays(i).Year;
                            int month = model.StartDate.AddDays(i).Month;
                            int day = model.StartDate.AddDays(i).Day;
                            Off_Checkin_Schedule schedule = new Off_Checkin_Schedule()
                            {
                                Off_Store_Id = Convert.ToInt32(storelist[j]),
                                Subscribe = model.StartDate.AddDays(i),
                                Standard_CheckIn = new DateTime(year, month, day, Convert.ToInt32(begintime[0]), Convert.ToInt32(begintime[1]), 0),
                                Standard_CheckOut = new DateTime(year, month, day, Convert.ToInt32(finishtime[0]), Convert.ToInt32(finishtime[1]), 0),
                                Standard_Salary = model.Salary
                            };
                            offlineDB.Off_Checkin_Schedule.Add(schedule);
                        }
                    }
                    offlineDB.SaveChanges();
                    return RedirectToAction("Off_ScheduleList");
                }
                else
                {
                    var storesystem = from m in offlineDB.Off_Store
                                      group m by m.StoreSystem into g
                                      orderby g.Key
                                      select new { Key = g.Key, Value = g.Key };
                    ViewBag.SystemList = new SelectList(storesystem, "Key", "Value", storesystem.FirstOrDefault().Value);
                    return View(model);
                }
            }
            return RedirectToAction("Off_ScheduleList");
        }
        [HttpPost]
        public JsonResult Off_Add_Schedule_StoreList(string storesystem)
        {
            var list = from m in offlineDB.Off_Store
                       where m.StoreSystem == storesystem
                       select new { ID = m.Id, StoreName = m.StoreName };
            return Json(new { StoreList = list });
        }
        #endregion

        #region 确认销售金额
        public ActionResult Off_ConfirmCheckIn(int CheckinId)
        {
            var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == CheckinId);
            bool attendance_late = false;
            bool attendance_early = false;
            if (item.CheckinTime >= item.Off_Checkin_Schedule.Standard_CheckIn)
                attendance_late = true;
            if (item.CheckoutTime <= item.Off_Checkin_Schedule.Standard_CheckOut)
                attendance_early = true;
            int attendance = 0;
            int debits = 0;
            if (attendance_early && attendance_late)
            {
                attendance = 3;
                debits = -40;
            }
            else if (attendance_early)
            {
                attendance = 2;
                debits = -20;
            }
            else if (attendance_late)
            {
                attendance = 1;
                debits = -20;
            }
            List<Object> attendance_list = new List<Object>();
            attendance_list.Add(new { Key = 0, Value = "全勤" });
            attendance_list.Add(new { Key = 1, Value = "迟到" });
            attendance_list.Add(new { Key = 2, Value = "早退" });
            attendance_list.Add(new { Key = 3, Value = "旷工" });
            ViewBag.Attendancelist = new SelectList(attendance_list, "Key", "Value", attendance);
            ConfirmCheckIn_ViewModel model = new ConfirmCheckIn_ViewModel()
            {
                CheckIn_Id = CheckinId,
                AttendanceStatus = attendance,
                Salary = item.Off_Checkin_Schedule.Standard_Salary,
                Bonus = item.Bonus,
                Debits = debits,
                Rep_Brown = item.Rep_Brown,
                Rep_Black = item.Rep_Black,
                Rep_Dates = item.Rep_Dates,
                Rep_Honey = item.Rep_Honey,
                Rep_Lemon = item.Rep_Lemon,
                Rep_Other = item.Rep_Other,
                Remark = item.Remark,
                Bonus_Remark = item.Bonus_Remark,
                Confirm_Remark = item.Confirm_Remark
            };
            ViewBag.CheckItem = item;
            return View(model);
        }
        [ValidateAntiForgeryToken, HttpPost]
        public ActionResult Off_ConfirmCheckIn(FormCollection form)
        {
            ConfirmCheckIn_ViewModel model = new ConfirmCheckIn_ViewModel();
            if (TryUpdateModel(model))
            {
                var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == model.CheckIn_Id);
                if (item.Status == 4)
                {
                    Off_SalesInfo_Daily info = new Off_SalesInfo_Daily()
                    {
                        Attendance = model.AttendanceStatus,
                        Date = item.Off_Checkin_Schedule.Subscribe,
                        Bonus = model.Bonus,
                        Debit = model.Debits,
                        isMultiple = false,
                        Item_Black = item.Rep_Black,
                        Item_Brown = item.Rep_Brown,
                        Item_Dates = item.Rep_Dates,
                        Item_Honey = item.Rep_Honey,
                        Item_Lemon = item.Rep_Lemon,
                        remarks = model.Remark,
                        SellerId = item.Off_Seller_Id,
                        Salary = model.Salary,
                        StoreId = item.Off_Checkin_Schedule.Off_Store_Id,
                        UploadTime = DateTime.Now,
                        UploadUser = User.Identity.Name
                    };
                    offlineDB.Off_SalesInfo_Daily.Add(info);
                    item.Status = 5;
                    item.Bonus = model.Bonus;
                    item.Bonus_Remark = model.Bonus_Remark;
                    item.SubmitTime = DateTime.Now;
                    item.SubmitUser = User.Identity.Name;
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return RedirectToAction("Off_CheckIn_List");
                }
                else if(item.Status == 3)
                {
                    item.Status = 4;
                    item.Rep_Lemon = model.Rep_Lemon;
                    item.Rep_Brown = model.Rep_Brown;
                    item.Rep_Black = model.Rep_Black;
                    item.Rep_Honey = model.Rep_Honey;
                    item.Rep_Dates = model.Rep_Dates;
                    item.Rep_Other = model.Rep_Other;
                    item.Confirm_Remark = model.Confirm_Remark;
                    item.ConfirmUser = User.Identity.Name;
                    item.ConfirmTime = DateTime.Now;
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return RedirectToAction("Off_CheckIn_List");
                }
                return View("Error");
            }
            else
            {
                return View("Error");
            }
        }
        public ActionResult Off_CreateCheckIn(int scheduleId)
        {

            Off_Checkin item = new Off_Checkin();
            item.Status = 3;
            item.Off_Schedule_Id = scheduleId;
            var schedule = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == scheduleId);
            ViewBag.StoreName = schedule.Off_Store.StoreName;
            ViewBag.Subscribe = schedule.Subscribe.ToString("yyyy-MM-dd");

            var sellerlist = from m in offlineDB.Off_Seller
                             where m.StoreId == schedule.Off_Store_Id
                             select m;
            ViewBag.Sellerlist = new SelectList(sellerlist, "Id", "Name");
            return View(item);
        }
        [ValidateAntiForgeryToken, HttpPost]
        public ActionResult Off_CreateCheckIn(Off_Checkin model)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin item = new Off_Checkin();
                if (TryUpdateModel(item))
                {
                    var schedule = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == item.Off_Schedule_Id);
                    item.CheckinLocation = "N/A";
                    item.CheckinTime = null;
                    item.CheckoutTime = null;
                    item.CheckoutLocation = "N/A";
                    item.ConfirmTime = DateTime.Now;
                    item.ConfirmUser = User.Identity.Name;
                    item.Status = 4;
                    offlineDB.Off_Checkin.Add(item);
                    //offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return RedirectToAction("Off_CheckIn_List");
                }
            }
            return View(model);
        }
        public ActionResult Off_ProxyCheckIn(int checkid)
        {
            Off_Checkin item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == checkid);
            return View(item);
        }
        [ValidateAntiForgeryToken, HttpPost]
        public ActionResult Off_ProxyCheckIn(Off_Checkin model)
        {
            if (ModelState.IsValid)
            {
                Off_Checkin item = new Off_Checkin();
                if (TryUpdateModel(item))
                {
                    //var schedule = offlineDB.Off_Checkin_Schedule.SingleOrDefault(m => m.Id == item.Off_Schedule_Id);
                    item.CheckinLocation = "N/A";
                    item.CheckoutLocation = "N/A";
                    item.CheckinTime = null;
                    item.CheckoutTime = null;
                    item.ConfirmTime = DateTime.Now;
                    item.ConfirmUser = User.Identity.Name;
                    item.Status = 4;
                    offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    offlineDB.SaveChanges();
                    return RedirectToAction("Off_CheckIn_List");
                }
            }
            return View(model);
        }
        [HttpPost]
        public ActionResult Off_CreateCheckIn_FileUpload(FormCollection form)
        {
            var  files = Request.Files;
            string msg = string.Empty;
            string error = string.Empty;
            string imgurl;
            if (files.Count > 0)
            {
                string filename = DateTime.Now.ToFileTime().ToString() + ".jpg";
                files[0].SaveAs(Server.MapPath("/Content/checkin-img/") + filename);
                msg = "成功! 文件大小为:" + files[0].ContentLength;
                imgurl = filename;
                string res = "{ error:'" + error + "', msg:'" + msg + "',imgurl:'" + imgurl + "'}";
                return Content(res);
            }
            string err_res = "{ error:'" + error + "', msg:'" + msg + "',imgurl:''}";
            return Content(err_res);
        }

        public ActionResult Off_ConfirmCheckIn_Map(bool trans, string lbs)
        {
            ViewBag.LBS = lbs;
            ViewBag.Trans = trans;
            return PartialView();
        }
        [HttpPost]
        public ActionResult Off_CancelCheckIn(int id)
        {
            var item = offlineDB.Off_Checkin.SingleOrDefault(m => m.Id == id);
            if(item.Status>=0 && item.Status < 5)
            {
                item.Status = -1;
                offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                offlineDB.SaveChanges();
                return Content("SUCCESS");
            }
            else
            {
                return Content("FAIL");
            }
        }
        public ActionResult Off_CheckIn_List()
        {
            return View();
        }
        public PartialViewResult Off_CheckIn_List_ajax(int? status, int? page, string query)
        {
            int _page = page ?? 1;
            int _status = status ?? 4;
            // 按照活动日期排序
            if (query==null)
            {
                var list = (from m in offlineDB.Off_Checkin
                            where m.Status == _status
                            orderby m.Off_Checkin_Schedule.Subscribe descending
                            select m).ToPagedList(_page, 50);
                return PartialView(list);
            }
            else
            {
                var list = (from m in offlineDB.Off_Checkin
                            where m.Status == _status
                            && (m.Off_Checkin_Schedule.Off_Store.StoreName.Contains(query) || m.Off_Seller.Name.Contains(query))
                            orderby m.Off_Checkin_Schedule.Subscribe descending
                            select m).ToPagedList(_page, 50);
                return PartialView(list);
            }
        }
        #endregion
        public ActionResult Off_UpdateManager(int id)
        {
            var item = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Id == id);
            var user = UserManager.FindByName(item.UserName);
            UserManager.RemoveFromRole(user.Id, "Seller");
            UserManager.AddToRole(user.Id, "Manager");
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.UserName == user.UserName);
            if (manager == null)
            {
                manager = new Off_StoreManager()
                {
                    UserName = user.UserName,
                    NickName = user.NickName,
                    Mobile = user.UserName,
                    Status = 1
                };
                offlineDB.Off_StoreManager.Add(manager);
            }
            item.Bind = false;
            item.Off_Seller_Id = null;
            //offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
            offlineDB.Off_Membership_Bind.Remove(item);
            offlineDB.SaveChanges();
            return Content("SUCCESS");
        }

        

        public ActionResult Off_CreateStore()
        {
            var store = new Off_Store();
            return PartialView(store);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Off_CreateStore(Off_Store model)
        {
            if (ModelState.IsValid)
            {
                Off_Store item = new Off_Store();
                if (TryUpdateModel(item))
                {
                    item.UploadTime = DateTime.Now;
                    item.UploadUser = User.Identity.Name;
                    offlineDB.Off_Store.Add(item);
                    offlineDB.SaveChanges();
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
        //0308
        [HttpPost]
        public ActionResult Off_DeleteStore(int id)
        {
            var item = offlineDB.Off_Store.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                try { offlineDB.Off_Store.Remove(item);
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                catch
                {
                    return Content("FAIL");
                }
            }
            return Content("FAIL");
        }
        public ActionResult Off_CreateSalesDaily()
        {
            var item = new Off_SalesInfo_Daily();
            var storelist = from m in offlineDB.Off_Store
                            orderby m.StoreName
                            select new { Key = m.Id, Value = m.StoreName };
            ViewBag.StoreDropDown = new SelectList(storelist, "Key", "Value");
            return PartialView(item);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Off_CreateSalesDaily(Off_SalesInfo_Daily model)
        {
            if (ModelState.IsValid)
            {
                Off_SalesInfo_Daily item = new Off_SalesInfo_Daily();
                if (TryUpdateModel(item))
                {
                    item.UploadTime = DateTime.Now;
                    item.UploadUser = User.Identity.Name;
                    offlineDB.Off_SalesInfo_Daily.Add(item);
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                var storelist = from m in offlineDB.Off_Store
                                orderby m.StoreName
                                select new { Key = m.Id, Value = m.StoreName };
                ViewBag.StoreDropDown = new SelectList(storelist, "Key", "Value");
                return PartialView(model);
            }
        }
        // 0308
        [HttpPost]
        public ActionResult Off_DeleteSalesDaily(int id)
        {
            var item = offlineDB.Off_SalesInfo_Daily.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                try
                {
                    offlineDB.Off_SalesInfo_Daily.Remove(item);
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                catch
                {
                    return Content("FAIL");
                }
            }
            return Content("FAIL");
        }

        public ActionResult Off_CreateSalesMonth()
        {
            var item = new Off_SalesInfo_Month();
            var storelist = from m in offlineDB.Off_Store
                            orderby m.StoreName
                            select new { Key = m.Id, Value = m.StoreName };
            ViewBag.StoreDropDown = new SelectList(storelist, "Key", "Value");
            return PartialView(item);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Off_CreateSalesMonth(Off_SalesInfo_Month model)
        {
            if (ModelState.IsValid)
            {
                Off_SalesInfo_Month item = new Off_SalesInfo_Month();
                if (TryUpdateModel(item))
                {
                    item.UploadTime = DateTime.Now;
                    item.UploadUser = User.Identity.Name;
                    offlineDB.Off_SalesInfo_Month.Add(item);
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                return Content("FAIL");
            }
            else
            {
                ModelState.AddModelError("", "发生错误");
                var storelist = from m in offlineDB.Off_Store
                                orderby m.StoreName
                                select new { Key = m.Id, Value = m.StoreName };
                ViewBag.StoreDropDown = new SelectList(storelist, "Key", "Value");
                return PartialView(model);
            }
        }
        // 0308
        [HttpPost]
        public ActionResult Off_DeleteSalesMonth(int id)
        {
            var item = offlineDB.Off_SalesInfo_Month.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                try
                {
                    offlineDB.Off_SalesInfo_Month.Remove(item);
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                catch
                {
                    return Content("FAIL");
                }
            }
            return Content("FAIL");
        }

        // 0309 删除促销员信息 /OfflineSales/Off_DeleteSeller
        [HttpPost]
        public ActionResult Off_DeleteSeller(int id)
        {
            var item = offlineDB.Off_Seller.SingleOrDefault(m => m.Id == id);
            if (item != null)
            {
                try
                {
                    offlineDB.Off_Seller.Remove(item);
                    offlineDB.SaveChanges();
                    return Content("SUCCESS");
                }
                catch
                {
                    return Content("FAIL");
                }
            }
            return Content("FAIL");
        }
        // 0309 查询促销签到记录 /OfflineSales/Off_QueryCheckIn
        public ActionResult Off_QueryCheckIn(DateTime? start, DateTime? end, string query)
        {
            DateTime _end = end ?? Convert.ToDateTime(DateTime.Now.ToShortDateString());
            DateTime _start = start ?? _end.AddDays(-1);
            var list = from m in offlineDB.Off_Checkin
                       where m.Off_Checkin_Schedule.Subscribe >= _start &&
                       m.Off_Checkin_Schedule.Subscribe <= _end &&
                       m.Off_Checkin_Schedule.Off_Store.StoreName.Contains(query)
                       select m;
            return View(list);
        }

        // 0310 管理员列表
        public ActionResult Off_Manager_List()
        {
            var list = from m in offlineDB.Off_StoreManager
                       select m;
            return View(list);
        }

        // 0310 升级为超级管理员
        public ActionResult Off_Manager_UpdateSenior(int id)
        {
            var item = offlineDB.Off_StoreManager.SingleOrDefault(m => m.Id == id);
            var user = UserManager.FindByName(item.UserName);
            UserManager.AddToRole(user.Id, "Senior");
            item.Status = 2;
            offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
            offlineDB.SaveChanges();
            return Content("SUCCESS");
        }

        public ActionResult Off_Manager_AddStore(int id)
        {
            //var item = offlineDB.Off_Membership_Bind.SingleOrDefault(m => m.Id == id);
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.Id == id);
            if (manager != null)
            {
                ViewBag.StoreList = manager.Off_Store.OrderBy(m => m.StoreName);
                var storesystem = from m in offlineDB.Off_Store
                                  group m by m.StoreSystem into g
                                  orderby g.Key
                                  select new { Key = g.Key, Value = g.Key };
                ViewBag.SystemList = new SelectList(storesystem, "Key", "Value", storesystem.FirstOrDefault().Value);
                ViewBag.Name = manager.NickName;
                ViewBag.ManagerId = manager.Id;
                return View();
            }
            else
            {
                return View();
            }
        }
        public JsonResult Off_GetStoreName(int storeid)
        {
            var item = offlineDB.Off_Store.SingleOrDefault(m => m.Id == storeid).StoreName;
            return Json(new { id = storeid, name = item }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Off_Manager_AjaxAddStore(int managerId, string arr_list)
        {
            var manager = offlineDB.Off_StoreManager.SingleOrDefault(m => m.Id == managerId);
            var currentlist = manager.Off_Store.Select(m => m.Id);
            string[] arr_temp = arr_list.Split(',');
            List<int> arr_int = new List<int>();
            foreach (var s in arr_temp)
            {
                arr_int.Add(Convert.ToInt32(s));
            }
            var select_list = (from m in offlineDB.Off_Store
                               where arr_int.Contains(m.Id)
                               select m).Select(m => m.Id);
            var removelist = currentlist.Except(select_list);
            var addlist = select_list.Except(currentlist);
            foreach (var item1 in removelist)
            {
                manager.Off_Store.Remove(offlineDB.Off_Store.SingleOrDefault(m => m.Id == item1));
            }
            foreach (var item2 in addlist)
            {
                manager.Off_Store.Add(offlineDB.Off_Store.SingleOrDefault(m => m.Id == item2));
            }
            offlineDB.Entry(manager).State = System.Data.Entity.EntityState.Modified;
            offlineDB.SaveChanges();
            return Content("SUCCESS");
        }
        // 0314 降级为普通管理员
        public ActionResult Off_Manager_ReduceManager(int id)
        {
            var item = offlineDB.Off_StoreManager.SingleOrDefault(m => m.Id == id);
            var user = UserManager.FindByName(item.UserName);
            UserManager.RemoveFromRole(user.Id, "Senior");
            item.Status = 1;
            offlineDB.Entry(item).State = System.Data.Entity.EntityState.Modified;
            offlineDB.SaveChanges();
            return Content("SUCCESS");
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

    public class Form_Product_Details
    {
        public string Product_Code { get; set; }
        public int? ReportNum { get; set; }
        public int? CheckNum { get; set; }
        public string Product_Name { get; set; }
        public Form_Product_Details()
        {

        }
    }
    public class Form_Product_Sales_Month
    {
        public string Product_Code { get; set; }
        public int? Sales_Count { get; set; }
        public decimal? Sales_Amount { get; set; }
        public string Product_Name { get; set; }
        public Form_Product_Sales_Month()
        {

        }
    }
    public class StoreView
    {
        public string StoreName { get; set; }
        public string StoreSystem
        {
            get; set;
        }
        public string Address
        {
            get; set;
        }
        public string Longitude
        {
            get; set;
        }
        public string Latitude
        {
            get; set;
        }
    }
    public class ScheduleList
    {
        public DateTime Subscribe { get; set; }
        public int Count { get; set; }
        public int Unfinished { get; set; }
    }
    public class Excel_DataMessage
    {
        public int line;
        public string message;
        public bool error;
        public Excel_DataMessage(int line, string message, bool error)
        {
            this.line = line;
            this.message = message;
            this.error = error;
        }
    }
    public enum Expenses_Name
    {
        [Display(Name = "进场费")]
        进场费,
        [Display(Name = "促销员工资")]
        促销员工资,
        [Display(Name = "促销员奖金")]
        促销员奖金,
        [Display(Name = "海报费")]
        海报费,
        [Display(Name = "端架费用")]
        端架费用,
        [Display(Name = "TG费用")]
        TG费用,
        [Display(Name = "地堆费用")]
        地堆费用,
        [Display(Name = "运输费")]
        运输费,
        [Display(Name = "试吃物料")]
        试吃物料,
        [Display(Name = "其他赠品")]
        其他赠品,
        [Display(Name = "公司赠品")]
        公司赠品,
        [Display(Name = "POSM")]
        POSM,
        [Display(Name = "其他费用")]
        其他费用
    }
}
