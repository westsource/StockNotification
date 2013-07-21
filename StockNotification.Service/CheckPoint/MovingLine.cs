using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockNotification.Service.Entity;

namespace StockNotification.Service.CheckPoint
{
    class MovingLine
    {
        //private readonly IList<Session> _sessions;
        private readonly int _scalar;
        private readonly double _volume;
        private readonly double _price;

        public int Scalar
        {
            get { return _scalar; }
        }

        public double Volume
        {
            get { return _volume; }
        }

        public double Price
        {
            get { return _price; }
        }

        public MovingLine(int scalar, IList<Session> sessions)
        {
            //_sessions = sessions;
            _scalar = scalar >= 1 ? scalar : 1;

            var volumes = (from Session s in sessions select s.Volume).ToList();
            _volume = GetMovingLineValue(volumes);
            var prices = (from Session s in sessions select s.Close).ToList();
            _price = GetMovingLineValue(prices);
        }

        private double GetMovingLineValue(IList<double> values)
        {
            var count = values.Count >= Scalar ? Scalar : values.Count;
            decimal sum = 0;
            for (int i = 0; i < count; i++)
            {
                sum += Convert.ToDecimal(values[i]);
            }

            return Convert.ToDouble(sum / count);
        }
    }
}
