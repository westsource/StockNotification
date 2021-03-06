﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockNotification.Database.Interface
{
    public class Stock
    {
        public string Id { get; set; }
        public string Symbol { get; set; }
        public readonly IList<InstitutionOwn> InstOwnList = new List<InstitutionOwn>();
    }

    public class InstitutionOwn
    {
        public string TimeStamp { get; set; }
        public float Rate { get; set; }
    }
}
