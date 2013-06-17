using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockNotification.WinService.CheckPoint
{
    class SupportByMaBase : ICheckPoint
    {
        public string Check(string symbol, List<Entity.Session> histories, ICompareWithMa comparer)
        {
            return "";
        }
    }
}
