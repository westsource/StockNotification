﻿using System.Threading;
using StockNotification.Database.Interface;
using StockNotification.Service.CheckPoint;
using System;
using System.Collections.Generic;
using System.ServiceProcess;
using StockNotification.Common;

namespace StockNotification.Service
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
            string sessionDate = DateTime.Now.ToString("yyyy-MM-dd");
            foreach (var stock in stocks)
            {
                var analyzer = new StockAnalyzer(stock, store);
                dictionary[stock] = analyzer.Analyse();
                sessionDate = analyzer.LastSessionDate;
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
                    //const string prefix = "上一交易日股票提醒:";
                    const string prefixPattern = "交易日股票提醒:";
                    var prefix = string.Format("{0}{1}", sessionDate, prefixPattern);
                    string subject = prefix + string.Join(",", userSymbols);
                    string body = prefix + "<br /><br />" + string.Join("<br />", userMessages);
                    body += "<br/><br/>" + DateTime.Now;

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
