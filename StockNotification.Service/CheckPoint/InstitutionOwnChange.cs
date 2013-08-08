using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockNotification.Database.Interface;

namespace StockNotification.Service.CheckPoint
{
    static class InstitutionOwnChange
    {
        public static string Check(IList<InstitutionOwn> list)
        {
            if (list.Count >= 2)
            {
                var current = list[0];
                var last = list[1];

                if (current.Rate != last.Rate)
                {
                    return string.Format("机构持股为{2}%，{0}{1:F2}%</font>",
                                         current.Rate > last.Rate ? "<font color=\"green\">+" : "<font color=\"red\">",
                                         current.Rate - last.Rate,
                                         current.Rate);
                }

                return string.Empty;
            }

            return string.Empty;
        }

    }
}
