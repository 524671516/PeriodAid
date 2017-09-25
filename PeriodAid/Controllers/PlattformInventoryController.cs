using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using PeriodAid.UnityTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PeriodAid.Controllers
{
    public class PlattformInventoryController : Controller
    {
        // GET: PlattformInventory
        public ActionResult Index()
        {           
            return View();
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
                //file.SaveAs(savePath);
                //stus = ExcelHelper.ReadExcelToEntityList<Student>(savePath);
                //ViewBag.Data = stus;
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