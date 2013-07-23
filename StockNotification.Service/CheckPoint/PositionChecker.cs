using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockNotification.Service.Entity;

namespace StockNotification.Service.CheckPoint
{
    static class PositionChecker
    {
        public static string Check(Session session, double benchmark)
        {
            var range = session.High - session.Low;
            var position = session.Close - session.Low;
            double positionRate = 0.0 == range?1:position/range;

            if (positionRate < benchmark)
            {
                return string.Format("收于当日价格范围的{0:F2}位置（顶部为1）", positionRate);
            }

            return string.Empty;
        }
    }
}
