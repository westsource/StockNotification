using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockNotification.WinService.CheckPoint
{
    class BreakOutOfMa200:CompareWithMaBase
    {
        protected override int DayCount
        {
            get { return 200; }
        }
    }
}
