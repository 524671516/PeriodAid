using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PeriodAid.Models;

namespace PeriodAid.DAL
{
    public class PeriodAidUtilities
    {
        public PeriodAidUtilities()
        {
        }

        // 通过月份，获取生理期数据
        public PeriodResult getPeriodResult(int year, int month, DateTime mc_date, int mc_days, int mc_cycle, PeriodResult result)
        {
            // 当月第一天
            DateTime first_day = new DateTime(year, month, 1);
            // 当月最后一天
            DateTime last_day = first_day.AddMonths(1);
            int difference_first = Convert.ToInt32(first_day.Subtract(mc_date).TotalDays);
            int difference_last = Convert.ToInt32(last_day.Subtract(mc_date).TotalDays);
            // 计算其中间隔多少个周期

            int period_count_f = difference_first / mc_cycle;
            int period_count_l = difference_last / mc_cycle;

            //PeriodResult result = new PeriodResult();

            for (int i = period_count_f; i <= period_count_l; i++)
            {
                DateTime period = mc_date.AddDays(i * mc_cycle);
                // 渲染当月月经数据
                if (period > mc_date)
                {
                    result = RenderPeriod(year, month, mc_date, period, mc_days, mc_cycle, result);
                }
            }
            return result;
        }


        // 渲染预测月经数据
        public PeriodResult RenderPeriod(int year, int month, DateTime last_period, DateTime period, int days, int cycle, PeriodResult result)
        {
            DateTime first_day = new DateTime(year, month, 1);
            DateTime last_day = first_day.AddMonths(1);

            // 填充预计月经期
            for (int i = 0; i < days; i++)
            {
                var item = period.AddDays(i);
                if (item >= first_day && item < last_day)
                {
                    result.prep_days.Add(item.Day);
                }
            }
            var safe_day_last = period.AddDays(cycle - 3);
            var safe_day_first = period.AddDays(days + 4);
            var danger_day = (safe_day_last.Subtract(safe_day_first).Days) / 2 - 2;
            for (int i = 0; i < 5; i++)
            {
                // 填充易孕期
                var item = safe_day_first.AddDays(danger_day + i);
                if (item >= first_day && item < last_day)
                {
                    result.e_days.Add(item.Day);
                }
            }
            return result;
        }

        // 渲染历史月经数据
        public PeriodResult RenderPeriodHistory(int year, int month, PeriodData data, PeriodResult result)
        {
            //var peroid_list = from m in peroid
            DateTime first_day = new DateTime(year, month, 1);
            DateTime last_day = first_day.AddMonths(1);
            DateTime period = data.MC_Begin;
            // 填充历史经期
            for (int i = 0; i < data.MC_Days; i++)
            {
                var item = period.AddDays(i);
                if (item >= first_day && item < last_day)
                {
                    result.p_days.Add(item.Day);
                }
            }
            var safe_day_last = period.AddDays(data.MC_Cycle - 3);
            var safe_day_first = period.AddDays(data.MC_Days + 4);
            var danger_day = (safe_day_last.Subtract(safe_day_first).Days) / 2 - 2;
            for (int i = 0; i < 5; i++)
            {
                // 填充易孕期
                var item = safe_day_first.AddDays(danger_day + i);
                if (item >= first_day && item < last_day)
                {
                    result.e_days.Add(item.Day);
                }
            }
            return result;
        }
    }

    public class PeriodResult
    {
        // 生理期
        public List<int> p_days { get; set; }

        // 排卵期
        public List<int> e_days { get; set; }

        // 预测生理期
        public List<int> prep_days { get; set; }

        public PeriodResult()
        {
            prep_days = new List<int>();
            e_days = new List<int>();
            p_days = new List<int>();
        }
    }

    public class PeriodInfo
    {
        /// <summary>
        /// 经期首日时间
        /// </summary>
        public DateTime MC_Begin { get; set; }

        /// <summary>
        /// 经期时间
        /// </summary>
        public int MC_Days { get; set; }

        /// <summary>
        /// 经期周期
        /// </summary>
        public int MC_Cycle { get; set; }

    }

}