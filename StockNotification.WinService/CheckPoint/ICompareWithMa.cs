using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockNotification.WinService.Entity;

namespace StockNotification.WinService.CheckPoint
{
    interface ICompareWithMa
    {
        bool Compare(Session last, Session lastSecond, double ma);
        string BehaviourName { get; }
    }

    class CategoryBreakOutOf:ICompareWithMa
    {

        public bool Compare(Session current, Session last, double ma)
        {
            return last.Close <= ma && current.Close > ma;
        }

        public string BehaviourName { get { return "向上突破"; } }
    }

    internal class CategoryBelowOf : ICompareWithMa
    {

        public bool Compare(Session last, Session lastSecond, double ma)
        {
            return lastSecond.Close >= ma && last.Close < ma;
        }

        public string BehaviourName
        {
            get { return "跌破"; }
        }
    }
}
