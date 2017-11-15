using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
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

    public class EventStatisticViewModel
    {
        public int productId { get; set; }

        public DateTime salesdate { get; set; }

        public int salescount { get; set; }

        public decimal Pay_Sum { get; set; }

        public decimal SubAccount_Sum { get; set; }

        public decimal Sales_Price { get; set; }
    }

    // 渠道销量
    public class TrafficStatisticViewModel
    {
        public DateTime salesdate { get; set; }

        public int ordercount { get; set; }
    }
    // 京东送货单导入
    public class StorageOrder
    {
        public string OrderId { get; set; }

        public string SystemCode { get; set; }

        public string ProductName { get; set; }

        public string StorageName { get; set; }

        public string SubStoName { get; set; }

        public int CartonSpec { get; set; }

        public int OrderCount { get; set; }

        public int CartonCount { get; set; }
    }
    
    // ViewModel
    public class Product_SummaryViewModel
    {
        public SS_Product Product { get; set; }

        public int Sales_Sum { get; set; }

        public int Inventory_Sum { get; set; }

        public decimal Pay_Sum { get; set; }

        public decimal SubAccount_Sum { get; set; }

        public decimal Settlement { get; set; }
    }
    
    class StreamDataSource : IStaticDataSource
    {
        public byte[] bytes { get; set; }
        public StreamDataSource(MemoryStream ms)
        {
            bytes = ms.GetBuffer();
        }

        public Stream GetSource()
        {
            Stream s = new MemoryStream(bytes);
            return s;
        }
    }
}