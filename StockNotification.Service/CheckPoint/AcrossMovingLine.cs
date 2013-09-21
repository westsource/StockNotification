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
        public string Check(IList<Session> sessions, int scalar, double volumeBenchmark)
        {
            var current = sessions[0];
            var last = sessions[1];
            var currentMovingLine = new MovingLine(scalar, sessions);
            var lastSessions = StockAnalyzer.Copy(sessions);
            lastSessions.RemoveAt(0);
            var lastMovingLine = new MovingLine(scalar, lastSessions);

            if (Compares(current, last, currentMovingLine, lastMovingLine)
                && (100 * current.Volume / currentMovingLine.Volume) > volumeBenchmark)
            {
                return string.Format("{3}{0}日均线{1:F2}%，成交量为{0}日均值的{2:F2}%",
                                     currentMovingLine.Scalar,
                                     100 * (current.Close - currentMovingLine.Price) / currentMovingLine.Price,
                                     100 * current.Volume / currentMovingLine.Volume,
                                     Behaviour);
            }

            return string.Empty;
        }

        protected abstract bool Compares(Session current,
                                        Session last,
                                        MovingLine currentMovingLine,
                                        MovingLine lastMovingLine);
        protected abstract string Behaviour { get; }
    }
    
    class BreakOutOfMovingLine: AcrossMovingLine
    {
        protected override bool Compares(Session current, 
            Session last, 
            MovingLine currentMovingLine, 
            MovingLine lastMovingLine)
        {
            return current.Close > currentMovingLine.Price && last.Close < lastMovingLine.Price;
        }

        protected override string Behaviour
        {
            get { return "向上突破"; }
        }
    }

    class BelowOfMovingLine: AcrossMovingLine
    {
        protected override bool Compares(Session current,
            Session last,
            MovingLine currentMovingLine,
            MovingLine lastMovingLine)
        {
            return current.Close < currentMovingLine.Price && last.Close > lastMovingLine.Price;
        }

        protected override string Behaviour
        {
            get { return "向下跌破"; }
        }
    }
}
