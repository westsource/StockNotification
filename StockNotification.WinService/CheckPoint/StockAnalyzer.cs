using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using StockNotification.Common;
using StockNotification.WinService.Entity;

namespace StockNotification.WinService.CheckPoint
{
    class StockAnalyzer
    {
        private readonly Stock _stock;
        private readonly List<string> _messages; 

        private string LastSessionDate
        {
            get
            {
                var date = DateTime.Now.AddDays(-1);
                while (date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday)
                {
                    date = date.AddDays(-1);
                }

                return date.ToString("yyyy-MM-dd");
            }
        }

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

        private string DataFileName
        {
            get
            {
                return string.Format("{0}\\{1}.csv", DataPath, LastSessionDate);
            }
        }

        public Stock Stock
        {
            get { return _stock; }
        }

        public StockAnalyzer(Stock stock)
        {
            _stock = stock;
            _messages = new List<string>();
        }

        public IList<string> Analyse()
        {
            GetHistoryPrice();
            return CheckPoints();
        }

        private void GetHistoryPrice()
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
                var files = Directory.GetFiles(DataPath, "*.csv");
                //如果已经存在最新的交易记录文件，则不需要再下载
                if (files.Length == 0 || !files[0].Equals(DataFileName))
                {
                    //删除旧的交易记录文件
                    foreach (var f in files)
                    {
                        File.Delete(f);
                    }

                    using (var ms = new MemoryStream())
                    {
                        DownloadHistorySession(url, ms);
                        using (var fs = new FileStream(DataFileName, FileMode.Create))
                        {
                            var buffer = ms.ToArray();
                            fs.Write(buffer, 0, buffer.Length);
                            fs.Flush();
                            fs.Close();
                        }

                        ms.Close();
                    }
                }
            }
            catch (Exception e)
            {
                LogManager.WriteLog(LogFileType.Error, e.Message + ":" + url);
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
                    return;
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

        private IList<string> CheckPoints()
        {
            var tick = DataFileName + ".log";
            if (!File.Exists(tick))
            {
                var sessions = GetSessions();

                AppendToMessageToList((new BreakOutOfMa50()).Check(_stock.Symbol, sessions, new CategoryBelowOf()));
                AppendToMessageToList((new BreakOutOfMa200()).Check(_stock.Symbol, sessions, new CategoryBelowOf()));
                AppendToMessageToList((new BreakOutOfMa50()).Check(_stock.Symbol, sessions, new CategoryBreakOutOf()));
                AppendToMessageToList((new BreakOutOfMa200()).Check(_stock.Symbol, sessions, new CategoryBreakOutOf()));
                AppendToMessageToList((new MovingDownHeavy()).Check(_stock.Symbol, sessions));
                AppendToMessageToList((new MovingUpHeavy()).Check(_stock.Symbol, sessions));

                using (var fs = File.Create(tick))
                {
                    var writer = new StreamWriter(fs);
                    foreach (var msg in _messages)
                    {
                        writer.WriteLine(msg);
                    }

                    fs.Flush();
                    fs.Close();
                }
            }

            return _messages;
        }

        private void AppendToMessageToList(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                _messages.Add(msg);
            }
        }

        private List<Session> GetSessions()
        {
            var list = new List<Session>();
            if (File.Exists(DataFileName))
            {
                var reader = new StreamReader(DataFileName);
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
                Volume = ulong.Parse(words[5]),
                AdjClose = double.Parse(words[6])
            };
        }


    }
}
