using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockNotification.Service.Entity;

namespace StockNotification.Service.CheckPoint
{
    static class ReboundByMovingLine
    {
        public static string Check(int scalar, IList<Session> sessions)
        {
            var line = new MovingLine(scalar, sessions);
            var current = sessions[0];
            var last = sessions[1];
            if (current.Low < line.Price
                && current.Close > line.Price
                && current.Close > last.Close)
            {
                return string.Format("在{0}日均线上反弹，收于均线{1:F2}%上方",
                                     scalar,
                                     100 * (current.Close - line.Price) / line.Price);
            }

            return string.Empty;
        }

    }
}
