using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;

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



    }
}