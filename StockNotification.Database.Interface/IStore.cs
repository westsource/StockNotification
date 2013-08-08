using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockNotification.Database.Interface
{
    public interface IStore
    {
        User GetUser(string userId);
        IList<User> GetUser();
        IList<Stock> GetStockOfUser(string userId);
        Stock GetStock(string symbol);
        IList<Stock> GetStock();
        User SaveUser(User user);
        int ClearOverdueStock();
        void SaveStockForUser(string userId, string symbol);
        int ClearStockOfUser(string userId);
        bool IsRelated(string userId, string stockId);
        void SaveStockInstOwn(string symbol, float rate, string time);
    }
}
