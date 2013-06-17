using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockNotification.WinService.Entity;

namespace StockNotification.WinService.CheckPoint
{
    class BreakOutOfMa50:CompareWithMaBase
    {
        protected override int DayCount
        {
            get { return 50; }
        }
    }
}
