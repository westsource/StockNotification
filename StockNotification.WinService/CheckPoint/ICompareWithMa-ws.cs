using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockNotification.WinService.CheckPoint
{
    interface ICompareWithMa
    {
        bool Compare(double last, double lastSecond, );
    }
}
