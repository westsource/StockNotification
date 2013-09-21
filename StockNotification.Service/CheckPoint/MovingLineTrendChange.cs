using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockNotification.Service.CheckPoint
{
    static class MovingLineTrendChange
    {
        public static string Check(MovingLine[] lines)
        {
            if (lines[0].Price < lines[1].Price && lines[1].Price > lines[2].Price)
            {
                return string.Format("<font color=\"red\">{0}日均线转跌</font>", lines[0].Scalar);
            }
            
            if (lines[0].Price > lines[1].Price && lines[1].Price < lines[2].Price)
            {
                return string.Format("<font color=\"green\">{0}日均线转涨</font>", lines[0].Scalar);
            }

            return string.Empty;
        }
    }
}
