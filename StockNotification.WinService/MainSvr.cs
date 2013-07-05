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
        private readonly IStore store;

        public MainSvr()
        {
            store = ApplicationEntry.Instance.GetService<IStore>();
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ThreadPool.QueueUserWorkItem(delegate(object state)
                {
                    while (true)
                    {
                        StartProcessing();
                        Thread.Sleep(TimeSpan.FromMinutes(30));
                    }
                });
        }

        public void StartProcessing()
        {
            var dictionary = new Dictionary<Stock, IList<string>>();
            var stocks = store.GetStock();
            foreach (var stock in stocks)
            {
                var analyzer = new StockAnalyzer(stock);
                dictionary[stock] = analyzer.Analyse();
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            var users = store.GetUser();
            foreach (var u in users)
            {
                var userSymbols = new List<string>();
                var userMessages = new List<string>();
                foreach (var pair in dictionary)
                {
                    if ((pair.Value.Count > 0) && store.IsRelated(u.Id, pair.Key.Id))
                    {
                        userSymbols.Add(pair.Key.Symbol);
                        userMessages.AddRange(pair.Value);
                    }
                }

                if (userMessages.Count > 0)
                {
                    const string prefixPattern = "交易日股票提醒:";
                    var prefix = string.Format("{0}{1}", StockAnalyzer.LastSessionDate, prefixPattern);
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

        protected override void OnStop()
        {
        }
    }
}
