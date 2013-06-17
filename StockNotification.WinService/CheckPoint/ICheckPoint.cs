using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockNotification.WinService.Entity;

namespace StockNotification.WinService.CheckPoint
{
    interface ICheckPoint
    {
        string Check(string symbol, List<Session> histories, ICompareWithMa comparer);
    }
}
