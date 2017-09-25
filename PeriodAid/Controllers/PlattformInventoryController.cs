﻿using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using PeriodAid.UnityTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CsvHelper;
using System.IO;
using System.Text;
using Microsoft.AspNet.Identity.Owin;
using PeriodAid.Models;

namespace PeriodAid.Controllers
{
    public class PlattformInventoryController : Controller
    {
        // GET: PlattformInventory
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ShopStorageModel _db;

        public PlattformInventoryController()
        {
            _db = new ShopStorageModel();
        }

        public PlattformInventoryController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        public ActionResult Read_InsertFile()
        {
            string folder = HttpContext.Server.MapPath("~/Content/xlsx/");
            StreamReader reader = new StreamReader(folder + "201709221540.csv", System.Text.Encoding.GetEncoding("GB2312"), false);
            CsvReader csv_reader = new CsvReader(reader);
            StringBuilder sb = new StringBuilder();
            //string s = String.Join(",", csv_reader.FieldHeaders);
            int row_count = 0;
            List<string> headers = new List<string>();
            var storage_list = from m in _db.SS_Storage
                               where m.Plattform_Id == 1
                               select m;
            int sales_field = 0;
            int inventory_field = 0;
            while (csv_reader.Read())
            {
                try
                {
                    string system_code = csv_reader.GetField<string>("商品编号");
                    var product = _db.SS_Product.SingleOrDefault(m => m.System_Code == system_code);
                    if(product != null)
                    {
                        foreach(var inventory in storage_list)
                        {
                            SS_SalesRecord record = new SS_SalesRecord()
                            {
                                Product_Id = product.Id,
                                SalesRecord_Date = DateTime.Now.Date,
                                Storage_Id = inventory.Id,
                                Sales_Count = csv_reader.TryGetField<int>(inventory.Sales_Header, out sales_field) ? sales_field : 0,
                                Storage_Count = csv_reader.TryGetField<int>(inventory.Inventory_Header, out inventory_field) ? inventory_field : 0
                            };
                            _db.SS_SalesRecord.Add(record);
                        }
                    }
                    row_count++;
                }catch(Exception e)
                {
                    sb.Append(e.Message);
                    row_count++;
                }
            }
            _db.SaveChanges();
            return Content(sb.ToString());
        }

        public ActionResult LeadingIn() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HandleError(View = "~/Views/Shared/Error.cshtml")]
        public ActionResult Browse(HttpPostedFileBase file)
        {

            if (string.Empty.Equals(file.FileName) || ".xlsx" != Path.GetExtension(file.FileName))
            {
                throw new ArgumentException("当前文件格式不正确,请确保正确的Excel文件格式!");
            }

            var severPath = this.Server.MapPath("/files/"); //获取当前虚拟文件路径

            var savePath = Path.Combine(severPath, file.FileName); //拼接保存文件路径

            try
            {
                file.SaveAs(savePath);
                stus = ExcelHelper.ReadExcelToEntityList<Student>(savePath);
                ViewBag.Data = stus;
                return View("Index");
            }
            finally
            {
                System.IO.File.Delete(savePath);//每次上传完毕删除文件
            }

        }

        [HandleError(View = "~/Views/Shared/Error.cshtml")]
        public ActionResult Upload() {
            return View("UploadSuccess");
        }


    }
    
}