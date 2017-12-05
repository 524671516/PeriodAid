using PagedList;
using PeriodAid.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PeriodAid.Controllers
{
    [Authorize]
    public class SalesProcessController : Controller
    {
        // GET: SalesProcess
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private SalesProcessModel _db;
        public SalesProcessController()
        {
            _db = new SalesProcessModel();
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ProductList(int plattformId)
        {
            ViewBag.PlattformId = plattformId;
            return View();
        }

        public ActionResult ProductListPartial(int plattformId, int? page, string query)
        {
            int _page = page ?? 1;
            if (query != null)
            {
                if (query != "")
                {
                    var product = (from m in _db.SP_Product
                                   where m.Plattform_Id == plattformId
                                   select m);
                    var SearchResult = (from m in product
                                        where m.Item_Name.Contains(query) || m.Item_Code.Contains(query) || m.System_Code.Contains(query)
                                        orderby m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
                else
                {
                    var SearchResult = (from m in _db.SP_Product
                                        where m.Plattform_Id == plattformId
                                        orderby m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }

            }
            else
            {
                var productlist = (from m in _db.SP_Product
                                   where m.Plattform_Id == plattformId
                                   orderby m.Id descending
                                   select m).ToPagedList(_page, 15);
                return PartialView(productlist);
            }
        }

        public ActionResult CustomerList(int plattformId)
        {
            ViewBag.PlattformId = plattformId;
            var trafficName = from m in _db.SP_TrafficPlattform
                                where m.Plattform_Id == plattformId
                                select m;
            ViewBag.trafficName = trafficName;
            return View();
        }

        public ActionResult CustomerListPartial(int plattformId, int? page, string query, int trafficPlattformId)
        {
            int _page = page ?? 1;
            if(trafficPlattformId == 0)
            {
                if (query != "")
                {
                    var customer = (from m in _db.SP_Customer
                                    where m.SP_TrafficPlattform.Plattform_Id == plattformId
                                    select m);
                    var SearchResult = (from m in customer
                                        where m.Customer_Name.Contains(query) || m.Customer_Mobile.Contains(query) || m.SP_Seller.Seller_Name.Contains(query)
                                        orderby m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
                else
                {
                    var SearchResult = (from m in _db.SP_Customer
                                        where m.SP_TrafficPlattform.Plattform_Id == plattformId
                                        orderby m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
            }
            else
            {
                if (query != "")
                {
                    var customer = (from m in _db.SP_Customer
                                    where m.SP_TrafficPlattform.Plattform_Id == plattformId && m.TrafficPlattform_Id == trafficPlattformId
                                    select m);
                    var SearchResult = (from m in customer
                                        where m.Customer_Name.Contains(query) || m.Customer_Mobile.Contains(query) || m.SP_Seller.Seller_Name.Contains(query)
                                        orderby m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
                else
                {
                    var SearchResult = (from m in _db.SP_Customer
                                        where m.SP_TrafficPlattform.Plattform_Id == plattformId && m.TrafficPlattform_Id == trafficPlattformId
                                        orderby m.Id descending
                                        select m).ToPagedList(_page, 15);
                    return PartialView(SearchResult);
                }
            }
        }

        public ActionResult EditCustomerInfo(int customerId)
        {
            var item = _db.SP_Customer.SingleOrDefault(m => m.Id == customerId);
            //List<Object> selectvalue = new List<Object>();
            //selectvalue.Add(new { Text = "潜在客户", Value = -1 });
            //selectvalue.Add(new { Text = "正常客户", Value = 0 });
            //selectvalue.Add(new { Text = "经常购买客户", Value = 1 });
            //ViewBag.SelectList = new SelectList(selectvalue, "Value", "Text", item.Customer_Type);

            //List<Object> sellervalue = new List<Object>();
            //sellervalue.Add(new { Text = "孙楠楠", Value = 1 });
            //sellervalue.Add(new { Text = "杨丽萌", Value = 2 });
            //ViewBag.SellerList = new SelectList(sellervalue, "Value", "Text");

            //List<Object> trafficvalue = new List<Object>();
            //trafficvalue.Add(new { Text = "分销", Value = 1 });
            //trafficvalue.Add(new { Text = "代销", Value = 2 });
            //trafficvalue.Add(new { Text = "代发货", Value = 3 });
            //ViewBag.TrafficList = new SelectList(trafficvalue, "Value", "Text", item.TrafficPlattform_Id);
            return PartialView(item);
        }

        //[HttpPost]
        //public ActionResult EditCustomerInfo(SP_Customer model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        SP_Customer item = new SP_Customer();
        //        if (TryUpdateModel(item))
        //        {
        //            _db.Entry(item).State = System.Data.Entity.EntityState.Modified;
        //            _db.SaveChanges();
        //            return Json(new { result = "SUCCESS" });
        //        }
        //    }
        //    return Json(new { result = "FAIL" });
        //}
    }
}