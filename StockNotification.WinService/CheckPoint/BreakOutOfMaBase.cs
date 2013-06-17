using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockNotification.WinService.Entity;

namespace StockNotification.WinService.CheckPoint
{
    abstract class BreakOutOfMaBase: ICheckPoint
    {
        protected abstract int DayCount { get; }
        
        public string Check(string symbol, List<Session> histories)
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
            double v90 = 0;
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
                v90 = histories.Average(trade => trade.Volume);
            }

            var last = histories[0];
            var lastSecond = histories[1];
            if (lastSecond.Close <= ma
                && last.Close > ma
                && (last.Volume / v90 > 1.1))
            {
                return string.Format("{0}上涨{1:F2}%，向上突破{2}日均线{3:F2}%，成交量为90日平均值的{4:F2}倍",
                                     symbol,
                                     (last.Close - lastSecond.Close) * 100 / lastSecond.Close,
                                     DayCount,
                                     (last.Close - ma) * 100 / ma,
                                     last.Volume / v90);
            }

            return string.Empty;
        }
    }
}
