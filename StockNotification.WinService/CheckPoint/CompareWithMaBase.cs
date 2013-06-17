using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockNotification.WinService.Entity;

namespace StockNotification.WinService.CheckPoint
{
    abstract class CompareWithMaBase : ICheckPoint
    {
        protected abstract int DayCount { get; }
        
        public string Check(string symbol, List<Session> histories, ICompareWithMa comparer)
        {
            if (histories.Count == 0)
            {
                return string.Empty;
            }

            double ma = 0;
            if (histories.Count > DayCount)
            {
                for (int i = 0; i < DayCount; i++)
                {
                    ma += histories[i].Close;
                }
                ma = ma / DayCount;
            }
            else
            {
                ma = histories.Average(trade => trade.Close);
            }

            const int volumeDayCount = 90;
            decimal v90 = 0;
            if (histories.Count > volumeDayCount)
            {
                for (int i = 0; i < volumeDayCount; i++)
                {
                    v90 += histories[i].Volume;
                }
                v90 = v90 / volumeDayCount;
            }
            else
            {
                ulong sum = histories.Aggregate<Session, ulong>(0,
                                                                (temp, session) => temp + session.Volume);
                v90 = (decimal)sum/histories.Count;
            }

            var current = histories[0];
            var last = histories[1];
            if (comparer.Compare(current, last, ma)
                && (current.Volume / v90 > (decimal)1.1))
            {
                var change = (current.Close - last.Close)*100/last.Close;
                return string.Format("{0}{1}{2:F2}%，{3}{4}日均线{5:F2}%，成交量为90日平均值的{6:F2}倍",
                                     symbol,
                                     change > 0? "上涨":"下跌",
                                     change,
                                     comparer.BehaviourName,
                                     DayCount,
                                     (current.Close - ma) * 100 / ma,
                                     current.Volume / v90);
            }

            return string.Empty;
        }
    }
}
