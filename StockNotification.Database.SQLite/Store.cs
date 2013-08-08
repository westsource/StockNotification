using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using StockNotification.Database.Interface;

namespace StockNotification.Database.SQLite
{
    public class Store: IStore
    {
        public User GetUser(string userName)
        {
            const string sql = "select * from user where username=$username";
            var table = DatabaseHelper.Instance.GetDataTable(sql, new object[] {userName});
            var list = BuildUserList(table);
            return list.Count > 0 ? list[0] : null;
        }

        public IList<User> GetUser()
        {
            const string sql = "select * from user order by username";
            var table = DatabaseHelper.Instance.GetDataTable(sql);
            return BuildUserList(table);
        }

        public IList<Stock> GetStockOfUser(string userId)
        {
            var table = DatabaseHelper.Instance.GetDataTable(
                        "select s.* from stock s "
                        + " join userstock us on s.id=us.stockid and us.userid=$userid"
                        + " order by stock",
                        new object[] { userId }
                        );
            return BuildStockList(table);
        }

        public Stock GetStock(string symbol)
        {
            symbol = symbol.ToUpper();

            var table = DatabaseHelper.Instance.GetDataTable(
                "select * from stock where stock=$symbol",
                new object[] {symbol});
            var list = BuildStockList(table);

            if (list.Count > 0)
            {
                return list[0];
            }

            var stock = new Stock
                {
                    Id = Guid.NewGuid().ToString(),
                    Symbol = symbol
                };

            DatabaseHelper.Instance.ExecuteSql(
                "insert into stock(id,stock)values($id,$stock)",
                new object[]
                    {
                        stock.Id,
                        stock.Symbol
                    });

            return stock;
        }

        public IList<Stock> GetStock()
        {
            var table = DatabaseHelper.Instance.GetDataTable(
                "select * from stock order by stock");
            return BuildStockList(table);
        }

        public User SaveUser(User user)
        {
            var u = GetUser(user.Name);
            if (null == u)
            {
                user.Id = Guid.NewGuid().ToString();
                DatabaseHelper.Instance.ExecuteSql(
                    "insert into user(userid,username,email)values($userid,$username,$email)",
                    new object[]
                        {
                            user.Id,
                            user.Name,
                            user.Email
                        });
            }
            else
            {
                DatabaseHelper.Instance.ExecuteSql(
                    "update user set email=$email where username=$username",
                    new object[]
                        {
                            user.Email,
                            user.Name
                        });
                u.Email = user.Email;
            }

            return u;

        }

        public int ClearOverdueStock()
        {
            int effectStock = DatabaseHelper.Instance.ExecuteSql(
                "delete from stock where id not in "
                + " ("
                + " select distinct stockid from userstock"
                + " )");
            return effectStock;
        }

        public void SaveStockForUser(string userId, string symbol)
        {
            symbol = symbol.ToUpper();

            var list = GetStockOfUser(userId);
            var selected = (from Stock s in list where s.Symbol == symbol select s);
            if (selected.Count() != 0)
            {
                return;
            }

            var stock = GetStock(symbol);
            DatabaseHelper.Instance.ExecuteSql(
                "insert into userstock(id,userid,stockid)values($id,$userid,$stockid)",
                new object[]
                    {
                        Guid.NewGuid().ToString(),
                        userId, 
                        stock.Id
                    });
        }

        public int ClearStockOfUser(string userId)
        {
            return DatabaseHelper.Instance.ExecuteSql("delete from userstock where userid=$userid",
                                               new object[]
                                                   {
                                                       userId
                                                   });
        }

        public bool IsRelated(string userId, string stockId)
        {
            var countObj = DatabaseHelper.Instance.ExecuteScalar(
                "select count(1) from userstock where userid=$userid and stockid=$stockid",
                new object[] {userId, stockId}
                );
            return int.Parse(countObj.ToString()) > 0;
        }

        private IList<Stock> BuildStockList(DataTable table)
        {
            var list = (from DataRow r in table.Rows
                        select new Stock
                            {
                                Id = r["id"].ToString(),
                                Symbol = r["stock"].ToString()
                            }
                       ).ToList();
            foreach (var stock in list)
            {
                GetInstOwnInfo(stock);
            }

            return list;
        }

        private void GetInstOwnInfo(Stock stock)
        {
            var table = DatabaseHelper.Instance.GetDataTable(
                "select * from stock_inst_own si where si.stockid=$stockid"
                + " order by si.time desc",
                new object[] {stock.Id}
                );

            foreach (DataRow r in table.Rows)
            {
                var own = new InstitutionOwn
                    {
                        Rate = float.Parse(r["instown"].ToString()),
                        TimeStamp = r["time"].ToString()
                    };
                stock.InstOwnList.Add(own);
            }
        }

        private static IList<User> BuildUserList(DataTable table)
        {
            var list = (from DataRow r in table.Rows
                        select new User
                            {
                                Id = r["userid"].ToString(),
                                Name = r["username"].ToString(),
                                Email = r["email"].ToString()
                            }).ToList();
            return list;
        }


        public void SaveStockInstOwn(string symbol, float rate, string time)
        {
            var stock = GetStock(symbol);
            if (stock == null)
            {
                return;
            }

            var effected = DatabaseHelper.Instance.ExecuteSql(
                "update stock_inst_own set instown=$rate where stockid=$stockid and time=$time",
                new object[]
                    {
                        rate,
                        stock.Id,
                        time
                    });

            if (effected == 0)
            {
                DatabaseHelper.Instance.ExecuteSql(
                "insert into stock_inst_own(id,stockid,instown,time)values($id,$stockid,$rate,$time)",
                new object[]
                    {
                        Guid.NewGuid().ToString(),
                        stock.Id,
                        rate,
                        time
                    });
            }
        }
    }
}
