using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using StockNotification.Common;
using StockNotification.Database.Interface;
using StockNotification.Service.Entity;

namespace StockNotification.Service.CheckPoint
{
    class StockAnalyzer
    {
        private readonly Stock _stock;
        private readonly IStore _store;
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

        public StockAnalyzer(Stock stock, IStore store)
        {
            _stock = stock;
            _store = store;
        }

        public IList<string> Analyse()
        {
            GetInstitutionOwn();
            var sessions = GetSessions();
            return CheckPoints(sessions);
        }

        private void GetInstitutionOwn()
        {
            var content = GetInstituionPageContent();
            string regex = @"机构持股：<span>(\w*\.\w*)%</span>";
            const RegexOptions options =
                ((RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline) | RegexOptions.IgnoreCase);
            var reg = new Regex(regex, options);
            Match match = reg.Match(content);
            if (match.Success && match.Groups.Count > 1)
            {
                var value = match.Groups[1].Captures[0].Value;
                float rate = 0;
                if (float.TryParse(value, out rate))
                {
                    var time = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                    var own = new InstitutionOwn {Rate = rate, TimeStamp = time};
                    if (_stock.InstOwnList.Count == 0
                        || _stock.InstOwnList[0].TimeStamp != own.TimeStamp)
                    {
                        _stock.InstOwnList.Insert(0, own);
                        _store.SaveStockInstOwn(_stock.Symbol, rate, time);
                    }
                }

            }
        }

        private string GetInstituionPageContent()
        {
            const string urlPattern = "http://xueqiu.com/S/{0}";
            var url = string.Format(urlPattern, _stock.Symbol);
            var ms = new MemoryStream();

            try
            {
                var request = (HttpWebRequest) WebRequest.Create(url);
                var response = (HttpWebResponse) request.GetResponse();

                using (var st = response.GetResponseStream())
                {
                    if (st == null)
                    {
                        return string.Empty;
                    }

                    var totalDownloadedByte = 0;
                    var buffer = new byte[1024];
                    int osize = st.Read(buffer, 0, buffer.Length);
                    while (osize > 0)
                    {
                        totalDownloadedByte = osize + totalDownloadedByte;
                        ms.Write(buffer, 0, osize);

                        osize = st.Read(buffer, 0, buffer.Length);
                        Thread.Sleep(TimeSpan.FromMilliseconds(1));
                    }
                    st.Close();
                    ms.Seek(0, SeekOrigin.Begin);
                }

                return Encoding.GetEncoding("utf-8").GetString(ms.ToArray());
            }
            catch (Exception e)
            {
                LogManager.WriteLog(LogFileType.Error, url + "。" + e);
                return string.Empty;
            }
            finally
            {
                ms.Close();
            }
        }

        private string DownloadSessionHistory(DateTime begin)
        {
            //形如 http://ichart.finance.yahoo.com/table.csv?s=MA&a=04&b=25&c=2006&d=03&e=13&f=2013&g=d&ignore=.csv
            const string urlMask =
                "http://ichart.finance.yahoo.com/table.csv?s={6}&a={0}&b={1}&c={2}&d={3}&e={4}&f={5}&g=d&ignore=.csv";

            string beginMonth = begin.ToString("MM");
            string beginDay = begin.ToString("dd");
            string beginYear = begin.ToString("yyyy");

            var end = DateTime.Now.Date.AddDays(-1);
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
                using (var ms = new MemoryStream())
                {
                    DownloadHistorySession(url, ms);
                    _lastSessionDate = GetLastSesssionDate(ms);
                    var dataFileName = string.Format("{0}\\{1}.csv",
                                                     DataPath,
                                                     _lastSessionDate);
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

        private static string GetLastSesssionDate(MemoryStream source)
        {
            source.Seek(0, SeekOrigin.Begin);
            try
            {
                using (Stream ms = new MemoryStream(source.ToArray()))
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(ms))
                    {
                        reader.ReadLine();
                        var line = reader.ReadLine();
                        var session = ParseToSession(line);
                        return session.Date;
                    }
                }
            }
            finally
            {
                source.Seek(0, SeekOrigin.Begin);
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

        private IList<string> CheckPoints(IList<Session> sessions)
        {
            if (sessions.Count < 2)
            {
                return new List<string>();
            }

            var current = sessions[0];
            var last = sessions[1];

            const double cPositiveBenchmark = 1.2;
            const double cNegativeBenchmark = -1.2;
            const double cVolumeBenchmark = 120;//120%以上

            var priceDelta = 100*(current.Close - last.Close)/last.Close;
            double volumeDelta;
            int volumeDeltaScalar;
            if (CompareWithMovingLine(sessions, cVolumeBenchmark, out volumeDelta, out volumeDeltaScalar)
                ||Stock.Symbol.StartsWith("^") 
                || priceDelta >= cPositiveBenchmark 
                || priceDelta <= cNegativeBenchmark)
            {
                _messages.Add(">" + Stock.Symbol);

                var msg = string.Format("收盘价{6}，{2}{3:f2}%，{7}成交量为上交易日的{0:f0}%，{4}日均值的{5:f0}%",
                                        100*(current.Volume/last.Volume),
                                        Stock.Symbol,
                                        priceDelta > 0 ? "+" : "",
                                        priceDelta,
                                        volumeDeltaScalar,
                                        volumeDelta,
                                        current.Close,
                                        IsNewHigh(current, sessions) ? "创历史新高，" : "");
                _messages.Add(msg);

                const double cMovingLineVolumeBenchmark = 10; //10%以上
                AppendToMessages((new BreakOutOfMovingLine()).Check(sessions, 50, cMovingLineVolumeBenchmark));
                AppendToMessages((new BreakOutOfMovingLine()).Check(sessions, 200, cMovingLineVolumeBenchmark));
                AppendToMessages((new BelowOfMovingLine()).Check(sessions, 50, cMovingLineVolumeBenchmark));
                AppendToMessages((new BelowOfMovingLine()).Check(sessions, 200, cMovingLineVolumeBenchmark));
                //收盘价位置
                AppendToMessages(PositionChecker.Check(current, 0.2));
                //均线缠绕
                AppendToMessages(MovingLineTwine.Check(5, 10, sessions));
                //在20日、50日、200日均线上获得支撑
                //AppendToMessages(SupportByMovingLine.Check(20, sessions));
                AppendToMessages(SupportByMovingLine.Check(50, sessions));
                AppendToMessages(SupportByMovingLine.Check(200, sessions));
                //在均线上反弹
                //AppendToMessages(ReboundByMovingLine.Check(20, sessions));
                AppendToMessages(ReboundByMovingLine.Check(50, sessions));
                AppendToMessages(ReboundByMovingLine.Check(200, sessions));
                //机构持股率发生变化
                AppendToMessages(InstitutionOwnChange.Check(_stock.InstOwnList));

                var movingLineCurrent = new MovingLine(50, sessions);
                var temps = Copy(sessions);
                temps.RemoveAt(0);
                var movingLineYesterday = new MovingLine(50, temps);
                temps.RemoveAt(0);
                var movingLineBeforeYesterday = new MovingLine(50, temps);
                AppendToMessages(MovingLineTrendChange.Check(new[]
                    {
                        movingLineCurrent,
                        movingLineYesterday,
                        movingLineBeforeYesterday
                    }));
            }

            return _messages;
        }

        public static IList<Session> Copy(IList<Session> sessions)
        {
            var list = new List<Session>();
            list.AddRange(sessions);
            return list;
        }

        private bool IsNewHigh(Session current, IList<Session> sessions)
        {
            double high = sessions.Select(s => s.Close).Concat(new double[] {0}).Max();
            return current.Close >= high;
        }

        private bool CompareWithMovingLine(IList<Session> sessions,
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
                //_messages.Add("&nbsp;&nbsp;&nbsp;&nbsp;"+msg);
                _messages.Add(msg);
            }
        }

        private List<Session> GetSessions()
        {
            var sessions = new List<Session>();
            var files = Directory.GetFiles(DataPath).ToList();
            if (files.Count > 0)
            {
                files.Sort();
                foreach (var f in files)
                {
                    var temps = GetSessions(f);
                    sessions.AddRange(temps);
                }
            }

            if (sessions.Count > 0
                && sessions[0].Date == DateTime.Now.Date.AddDays(-1).ToString("yyyy-MM-dd"))
            {
                return sessions;
            }

            //yahoo的股价，获取的天数太少会有问题，所以一把全获取了
            sessions.Clear();
            //删除旧的交易记录文件
            foreach (var f in files)
            {
                File.Delete(f);
            }

            DateTime begin = sessions.Count > 0
                                 ? DateTime.Parse(sessions[0].Date).AddDays(1)
                                 : DateTime.Now.Date.AddYears(-100);

            var file = DownloadSessionHistory(begin);
            var lasts = GetSessions(file);
            sessions.AddRange(lasts);
            return sessions;
        }

        private IList<Session> GetSessions(string file)
        {
            var list = new List<Session>();
            if (!string.IsNullOrEmpty(file) && File.Exists(file))
            {
                using (var reader = new StreamReader(file))
                {
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
            }

            return list;
        }

        private static Session ParseToSession(string line)
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
