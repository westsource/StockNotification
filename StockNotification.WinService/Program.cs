using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using StockNotification.WinService.Common;

namespace StockNotification.WinService
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main(string[] args)
        {
            var ServicesToRun = new ServiceBase[] 
                { 
                    new MainSvr() 
                };

            if (args.Length > 0 && args[0].ToLower() == "debug")
            {
                if (MainSvr.StartMySQL())
                {
                    (ServicesToRun[0] as MainSvr).StartProcessing();
                    MainSvr.StopMySQL();
                }
            }
            else
            {
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
