using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockNotification.WinService.Entity;

namespace StockNotification.WinService.CheckPoint
{
    abstract class MovingHeavyBase
    {
        public string Check(string symbol, List<Entity.Session> histories)
        {
            if (histories.Count == 0)
            {
                return string.Empty;
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
                v90 = (decimal)sum / histories.Count;
            }

            var current = histories[0];
            var last = histories[1];

            if (Compare(current, last) && (last.Volume / v90 > (decimal)1.1))
            {
                var change = (current.Close - last.Close) * 100 / last.Close;
                return string.Format("{0}比前交易日{1}{2:F2}%，成交量为90日平均值的{3:F2}倍",
                                     symbol,
                                     change > 0 ? "上涨" : "下跌",
                                     change,
                                     last.Volume / v90);
            }

            return string.Empty;
        }

        protected abstract bool Compare(Session current, Session last);
    }

    class MovingDownHeavy: MovingHeavyBase
    {
        protected override bool Compare(Session current, Session last)
        {
            return (current.Close - last.Close)*100/last.Close > 0.8;
        }
    }

    class MovingUpHeavy: MovingHeavyBase
    {

        protected override bool Compare(Session current, Session last)
        {
            return (current.Close - last.Close) * 100 / last.Close < -0.8;
        }
    }
}
