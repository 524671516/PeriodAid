using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using PeriodAid.Models;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace PeriodAid.DAL
{
    public class OfflineSalesUtilities
    {
        // Origin: 
        private OfflineSales _offlineDb;
        public OfflineSalesUtilities()
        {
            _offlineDb = new OfflineSales();
        }
        public async Task<bool> UpdateDailySalesAvg(int storeid, int dow)
        {
            try
            {
                var item = _offlineDb.Off_AVG_Info.SingleOrDefault(m => m.DayOfWeek == dow && m.StoreId == storeid);
                if (item != null)
                {
                    //修改
                    string sql = "update Off_AVG_Info set AVG_SalesData = (Select cast(cast(T5.SalesCount as decimal(18, 2)) / T2.Count as decimal(18, 2)) as AVG_SalesData from(SELECT StoreId, DATEPART(DW, T1.[Date]) as DayOfWeek, COUNT(T1.Id) as Count FROM Off_SalesInfo_Daily as T1" +
                        " where T1.StoreId = " + storeid + " and DATEPART(DW, T1.[Date]) = " + dow + " group by T1.StoreId, DATEPART(DW, T1.[Date])) as T2  left join (select T3.StoreId, DATEPART(DW, T3.Date) as DayOfWeek, SUM(T4.SalesCount) as SalesCount, SUM(T4.SalesAmount) as SalesAmount, SUM(T4.StorageCount) as StorageCount" +
                        " FROM Off_SalesInfo_Daily as T3 left join Off_Daily_Product as T4 on T3.Id = T4.DailyId  where T3.StoreId = " + storeid + " and DATEPART(DW, T3.[Date]) = " + dow + " group by T3.StoreId, DATEPART(DW, T3.Date) ) as T5 on T2.StoreId = T5.StoreId and T2.DayOfWeek = T5.DayOfWeek), " +
                        " AVG_AmountData = ( Select cast(cast(T5.SalesAmount as decimal(18, 2)) / T2.Count as decimal(18,2)) as AVG_AmountData from(SELECT StoreId, DATEPART(DW, T1.[Date]) as DayOfWeek, COUNT(T1.Id) as Count FROM Off_SalesInfo_Daily as T1 where T1.StoreId = " + storeid + " and DATEPART(DW, T1.[Date]) = " + dow +
                        " group by T1.StoreId, DATEPART(DW, T1.[Date])) as T2  left join (select T3.StoreId, DATEPART(DW, T3.Date) as DayOfWeek, SUM(T4.SalesCount) as SalesCount, SUM(T4.SalesAmount) as SalesAmount, SUM(T4.StorageCount) as StorageCount" +
                        " FROM Off_SalesInfo_Daily as T3 left join Off_Daily_Product as T4 on T3.Id = T4.DailyId where T3.StoreId = " + storeid + " and DATEPART(DW, T3.[Date]) = " + dow + " group by T3.StoreId, DATEPART(DW, T3.Date) ) as T5 on T2.StoreId = T5.StoreId and T2.DayOfWeek = T5.DayOfWeek)" +
                        " where StoreId = " + storeid + " and DayOfWeek = " + dow;
                    _offlineDb.Database.ExecuteSqlCommand(sql);
                    await _offlineDb.SaveChangesAsync();
                }
                else
                {
                    //新增
                    string sql = "INSERT INTO dbo.Off_AVG_Info ([StoreId] ,[DayOfWeek] ,[AVG_SalesData],[AVG_AmountData])" +
                        " Select T2.StoreId, T2.DayOfWeek, cast(cast(T5.SalesCount as decimal(18,2))/T2.Count as decimal(18,2)) as AVG_SalesData, cast(cast(T5.SalesAmount as decimal(18, 2)) / T2.Count as decimal(18,2)) as AVG_AmountData from(SELECT StoreId, DATEPART(DW, T1.[Date]) as DayOfWeek, COUNT(T1.Id) as Count" +
                        " FROM Off_SalesInfo_Daily as T1 where T1.StoreId = " + storeid + " and DATEPART(DW, T1.[Date]) = " + dow +
                        " group by T1.StoreId, DATEPART(DW, T1.[Date])) as T2  left join (select T3.StoreId, DATEPART(DW, T3.Date) as DayOfWeek, SUM(T4.SalesCount) as SalesCount, SUM(T4.SalesAmount) as SalesAmount, SUM(T4.StorageCount) as StorageCount" +
                        " FROM Off_SalesInfo_Daily as T3 left join Off_Daily_Product as T4 on T3.Id = T4.DailyId" +
                        " where T3.StoreId = " + storeid + " and DATEPART(DW, T3.[Date]) = " + dow + " group by T3.StoreId, DATEPART(DW, T3.Date) ) as T5 on T2.StoreId = T5.StoreId and T2.DayOfWeek = T5.DayOfWeek";
                    _offlineDb.Database.ExecuteSqlCommand(sql);
                    await _offlineDb.SaveChangesAsync();
                }
                return true;
            }
            catch
            {
                return false;
            }
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