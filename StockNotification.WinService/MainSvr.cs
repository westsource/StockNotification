using System.Data;
using System.Linq;
using System.Threading;
using StockNotification.Database.Interface;
using StockNotification.WinService.CheckPoint;
using StockNotification.WinService.Entity;
using System;
using System.Collections.Generic;
using System.ServiceProcess;
using StockNotification.Common;

namespace StockNotification.WinService
{
    public partial class MainSvr : ServiceBase
    {
        public MainSvr()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //ThreadPool.QueueUserWorkItem(delegate(object state)
            //    {
            //        while (true)
            //        {
            //            StartProcessing();
            //            Thread.Sleep(TimeSpan.FromMinutes(30));
            //        }
            //    });

            if (StartMySQL())
            {
                StartProcessing();
                StopMySQL();
            }
        }

        public  static bool StartMySQL()
        {
            try
            {
                var control = new ServiceController("mysql");
                control.Start();
                Thread.Sleep(TimeSpan.FromSeconds(10));
                return true;
            }
            catch (Exception e)
            {
                LogManager.WriteLog(LogFileType.Error, "启动MySQL失败" + e);
                return false;
            }
        }

        public static void StopMySQL()
        {
            try
            {
                var control = new ServiceController("mysql");
                if (control.CanStop)
                {
                    control.Stop();
                }
            }
            catch (Exception e)
            {
                LogManager.WriteLog(LogFileType.Error, "停止MySQL失败" + e);
            } 
        }

        public void StartProcessing()
        {
            var dictionary = new Dictionary<Stock, IList<string>>();
            var stocks = GetStocks();
            foreach (var stock in stocks)
            {
                var analyzer = new StockAnalyzer(stock);
                dictionary[stock] = analyzer.Analyse();
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            var users = GetUsers();
            foreach (var u in users)
            {
                var userSymbols = new List<string>();
                var userMessages = new List<string>();
                foreach (var pair in dictionary)
                {
                    if ((pair.Value.Count > 0) && Related(u, pair.Key))
                    {
                        userSymbols.Add(pair.Key.Symbol);
                        userMessages.AddRange(pair.Value);
                    }
                }

                if (userMessages.Count > 0)
                {
                    const string prefix = "上一交易日股票提醒:";
                    string subject = prefix + string.Join(",", userSymbols);
                    string body = prefix + "<br />" + string.Join("<br />", userMessages);
                    body += "<br/>" + DateTime.Now;

                    var sender = new MailSender(u.Email,
                                                "56472190@qq.com",
                                                body,
                                                subject,
                                                "56472190",
                                                "indian-2011");
                    sender.Send();
                }
            }
        }

        private bool Related(User user, Stock stock)
        {
            var countObj = DatabaseHelper.Instance.ExecuteScalar(
                "select count(1) from userstock where userid=?userid and stockid=?stockid",
                new object[]{user.Id, stock.Id}
                );
            return int.Parse(countObj.ToString()) > 0;
        }

        private IEnumerable<User> GetUsers()
        {
            var dt = DatabaseHelper.Instance.GetDataTable("select * from user");
            return (from DataRow r in dt.Rows
                    select new User
                        {
                            Id = r["userid"].ToString(),
                            Email = r["email"].ToString(),
                            Name = r["username"].ToString()
                        }).ToList();
        }

        private IEnumerable<Stock> GetStocks()
        {
            var dt = DatabaseHelper.Instance.GetDataTable("select * from stock");
            return (from DataRow r in dt.Rows
                    select new Stock
                        {
                            Id = r["id"].ToString(),
                            Symbol = r["stock"].ToString()
                        }).ToList();
        }

        protected override void OnStop()
        {
        }
    }
}
