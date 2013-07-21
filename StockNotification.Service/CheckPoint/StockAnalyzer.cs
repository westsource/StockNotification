using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using StockNotification.Common;
using StockNotification.Database.Interface;
using StockNotification.Service.Entity;

namespace StockNotification.Service.CheckPoint
{
    class StockAnalyzer
    {
        private readonly Stock _stock;
        private readonly List<string> _messages = new List<string>();

        private string _lastSessionDate = string.Empty;

        public string LastSessionDate
        {
            get { return _lastSessionDate; }
        }

        //public MovingLineOfPrice PriceMovingLine;
        //public MovingLineOfVolume VolumeMovingLine;

        private string DataPath
        {
            get
            {
                var path = AppDomain.CurrentDomain.BaseDirectory + "\\Stocks\\" + _stock.Symbol;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        public Stock Stock
        {
            get { return _stock; }
        }

        public StockAnalyzer(Stock stock)
        {
            _stock = stock;
        }

        public IList<string> Analyse()
        {
            var file = DownloadSessionHistory();
            return CheckPoints(file);
        }

        private string DownloadSessionHistory()
        {
            //形如 http://ichart.finance.yahoo.com/table.csv?s=MA&a=04&b=25&c=2006&d=03&e=13&f=2013&g=d&ignore=.csv
            const string urlMask =
                "http://ichart.finance.yahoo.com/table.csv?s={6}&a={0}&b={1}&c={2}&d={3}&e={4}&f={5}&g=d&ignore=.csv";

            var begin = DateTime.Now.Date.AddYears(-1);
            string beginMonth = begin.ToString("MM");
            string beginDay = begin.ToString("dd");
            string beginYear = begin.ToString("yyyy");

            var end = DateTime.Now.Date;
            var endMonth = end.ToString("MM");
            var endDay = end.ToString("dd");
            var endYear = end.ToString("yyyy");

            var url = string.Format(urlMask,
                                    beginMonth,
                                    beginDay,
                                    beginYear,
                                    endMonth,
                                    endDay,
                                    endYear,
                                    _stock.Symbol);
            try
            {
                var files = Directory.GetFiles(DataPath, "*.*");
                //删除旧的交易记录文件
                foreach (var f in files)
                {
                    File.Delete(f);
                }

                using (var ms = new MemoryStream())
                {
                    DownloadHistorySession(url, ms);
                    var dataFileName = string.Format("{0}\\{1}.csv",
                                                     DataPath,
                                                     DateTime.Now.ToString("yyyy-MM-dd"));
                    using (var fs = new FileStream(dataFileName, FileMode.Create))
                    {
                        var buffer = ms.ToArray();
                        fs.Write(buffer, 0, buffer.Length);
                        fs.Flush();
                        fs.Close();
                    }
                    ms.Close();
                    return dataFileName;
                }

            }
            catch (Exception e)
            {
                LogManager.WriteLog(LogFileType.Error, e.Message + ":" + url);
                return string.Empty;
            }
        }

        private static void DownloadHistorySession(string url, Stream outStream)
        {
            var request = (System.Net.HttpWebRequest) System.Net.WebRequest.Create(url);
            var response = (System.Net.HttpWebResponse) request.GetResponse();

            using (var st = response.GetResponseStream())
            {
                if (st == null)
                {
                    return ;
                }

                var totalDownloadedByte = 0;
                var buffer = new byte[1024];
                int osize = st.Read(buffer, 0, buffer.Length);
                while (osize > 0)
                {
                    totalDownloadedByte = osize + totalDownloadedByte;
                    outStream.Write(buffer, 0, osize);

                    osize = st.Read(buffer, 0, buffer.Length);
                    Thread.Sleep(TimeSpan.FromMilliseconds(1));
                }
                st.Close();
                outStream.Seek(0, SeekOrigin.Begin);
            }
        }

        private IList<string> CheckPoints(string file)
        {
            var sessions = GetSessions(file);
            if (sessions.Count < 2)
            {
                return new List<string>();
            }

            if (sessions.Count > 0)
            {
                _lastSessionDate = sessions[0].Date;
            }

            var current = sessions[0];
            var last = sessions[1];

            const double cPositiveBenchmark = 1.0;
            const double cNegativeBenchmark = -1.0;
            const double cVolumeBenchmark = 110;//150%以上

            var priceDelta = 100*(current.Close - last.Close)/last.Close;
            double volumeDelta;
            int volumeDeltaScalar;
            if (CompareWithMovingLine(sessions, cVolumeBenchmark, out volumeDelta, out volumeDeltaScalar)
                ||Stock.Symbol.StartsWith("^") 
                || priceDelta >= cPositiveBenchmark 
                || priceDelta <= cNegativeBenchmark)
            {
                var msg = string.Format(">{1}{2}{3:f2}%，收盘价{6}，{7}成交量为{4}日均值的{5:f0}%，上交易日的{0:f0}%",
                                        100*(current.Volume/last.Volume),
                                        Stock.Symbol,
                                        priceDelta > 0 ? "上涨" : "下跌",
                                        priceDelta,
                                        volumeDeltaScalar,
                                        volumeDelta,
                                        current.Close,
                                        IsNewHigh(current, sessions)?"创52周新高，":"");
                _messages.Add(msg);

                const double cMovingLineVolumeBenchmark = 10; //10%以上
                AppendToMessages((new BreakOutOfMovingLine()).Check(sessions,
                                                                    new MovingLine(10, sessions),
                                                                    cMovingLineVolumeBenchmark));
                AppendToMessages((new BreakOutOfMovingLine()).Check(sessions,
                                                                    new MovingLine(50, sessions),
                                                                    cMovingLineVolumeBenchmark));
                AppendToMessages((new BreakOutOfMovingLine()).Check(sessions,
                                                                    new MovingLine(200, sessions),
                                                                    cMovingLineVolumeBenchmark));
                AppendToMessages((new BelowOfMovingLine()).Check(sessions,
                                                                 new MovingLine(10, sessions),
                                                                 cMovingLineVolumeBenchmark));
                AppendToMessages((new BelowOfMovingLine()).Check(sessions,
                                                                 new MovingLine(50, sessions),
                                                                 cMovingLineVolumeBenchmark));
                AppendToMessages((new BelowOfMovingLine()).Check(sessions,
                                                                 new MovingLine(200, sessions),
                                                                 cMovingLineVolumeBenchmark));
            }

            return _messages;
        }

        private bool IsNewHigh(Session current, IList<Session> sessions)
        {
            double high = sessions.Select(s => s.Close).Concat(new double[] {0}).Max();
            return current.Close >= high;
        }

        private bool CompareWithMovingLine(List<Session> sessions,
                                           double volumeBenchmark,
                                           out double volumeDeltaRate,
                                           out int volumeDeltaScalar)
        {
            volumeDeltaScalar = 5;
            var current = sessions[0];
            var ma10 = new MovingLine(volumeDeltaScalar, sessions);
            volumeDeltaRate = 100 * current.Volume / ma10.Volume;
            if (volumeDeltaRate < volumeBenchmark)
            {
                return false;
            }

            return true;
        }

        private void AppendToMessages(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                _messages.Add("&nbsp;&nbsp;&nbsp;&nbsp;"+msg);
            }
        }

        private List<Session> GetSessions(string file)
        {
            var list = new List<Session>();
            if (!string.IsNullOrEmpty(file) && File.Exists(file))
            {
                var reader = new StreamReader(file);
                reader.ReadLine(); //第一行是表头，丢弃

                var line = reader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    try
                    {
                        var trade = ParseToSession(line);
                        trade.Id = Guid.NewGuid().ToString();
                        trade.StockId = _stock.Id;

                        list.Add(trade);
                    }
                    catch (Exception e)
                    {
                        LogManager.WriteLog(LogFileType.Error, e.ToString());
                    }
                    line = reader.ReadLine();
                }
            }

            return list;
        }

        private Session ParseToSession(string line)
        {
            const int wordCounts = 7;
            var words = line.Split(new[] { ',' });
            if (words.Length != wordCounts)
                throw new Exception("行格式不正确：" + line);

            return new Session
            {
                Date = words[0],
                Open = double.Parse(words[1]),
                High = double.Parse(words[2]),
                Low = double.Parse(words[3]),
                Close = double.Parse(words[4]),
                Volume = double.Parse(words[5]),
                AdjClose = double.Parse(words[6])
            };
        }
    }
}
