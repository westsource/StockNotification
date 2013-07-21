using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockNotification.Database.Interface;
using StockNotification.Service.Entity;

namespace StockNotification.Service.CheckPoint
{
    abstract class AcrossMovingLine
    {
        public string Check(IList<Session> sessions, MovingLine movingLine, double volumeBenchmark)
        {
            var current = sessions[0];
            var last = sessions[1];

            if (Compares(current, last, movingLine)
                && (100 * current.Volume / movingLine.Volume) > volumeBenchmark)
            {
                return string.Format("{3}{0}日均线{1:F2}%，成交量为{0}日均值的{2:F2}%",
                                     movingLine.Scalar,
                                     100 * (current.Close - movingLine.Price) / movingLine.Price,
                                     100 * current.Volume / movingLine.Volume,
                                     Behaviour);
            }

            return string.Empty;
        }

        protected abstract bool Compares(Session current, Session last, MovingLine movingLine);
        protected abstract string Behaviour { get; }
    }
    
    class BreakOutOfMovingLine: AcrossMovingLine
    {
        protected override bool Compares(Session current, Session last, MovingLine movingLine)
        {
            return current.Close > movingLine.Price && last.Close < movingLine.Price;
        }

        protected override string Behaviour
        {
            get { return "向上突破"; }
        }
    }

    class BelowOfMovingLine: AcrossMovingLine
    {
        protected override bool Compares(Session current, Session last, MovingLine movingLine)
        {
            return current.Close < movingLine.Price && last.Close > movingLine.Price;
        }

        protected override string Behaviour
        {
            get { return "向下跌破"; }
        }
    }
}
