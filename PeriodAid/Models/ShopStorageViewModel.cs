using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PeriodAid.Models
{
    public class ShopStorageViewModel
    {
    }

    public class CalcStorageParmsViewModel
    {
        public string system_code { get; set; }

        public decimal params_rate { get; set; }
    }

    public class CalcStorageViewModel
    {
        public SS_Product Product { get; set; }

        public int Sales_Count { get; set; }

        public int Storage_Count { get; set; }

        public double Sales_Avg { get; set; }
    }

    public class ProductStatisticViewModel
    {
        public DateTime salesdate { get; set; }

        public int salescount { get; set; }
    }

    // 京东送货单导入
    public class StorageOrder
    {
        public int OrderId { get; set; }

        public string SystemCode { get; set; }

        public string ProductName { get; set; }

        public string StorageName { get; set; }

        public string SubStoName { get; set; }

        public int CartonSpec { get; set; }

        public int OrderCount { get; set; }

        public int CartonCount { get; set; }
    }

    // 天猫调货单
    public class TM_TransferringOrder
    {
        public string StorageCode { get; set; }

        public string SystemCode { get; set; }

        public string ItemName { get; set; }

        public int CommitCount { get; set; }
        
        public int BarCode { get; set; }
    }



    // ViewModel
    public class Product_SummaryViewModel
    {
        public SS_Product Product { get; set; }

        public int Sales_Sum { get; set; }

        public int Inventory_Sum { get; set; }

        public decimal Pay_Sum { get; set; }

        public decimal SubAccount_Sum { get; set; }
    }
}