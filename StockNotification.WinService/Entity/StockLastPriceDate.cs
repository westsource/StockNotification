using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockNotification.WinService.Entity
{
    public class StockLastPriceDate
    {
        public string Id { get; set; }
        public string Symbol { get; set; }
        public string TimeStamp { get; set; } 
    }
}
