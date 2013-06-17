using System;
using System.Data;
using System.Windows.Forms;
using StockNotification.Common;
using StockNotification.WinService.Entity;

namespace StockNotification.Gui
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                var userId = ProcessUserInfo();
                UpdateUserStocks(userId);
                ClearOverdueStock();

                MessageBox.Show("导入完成");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }

        private void ClearOverdueStock()
        {
            int effectStock = DatabaseHelper.Instance.ExecuteSql(
                "delete from stock where id not in "
                + " ("
                + " select distinct stockid from userstock"
                + " )");

            LogManager.WriteLog(LogFileType.Trace,
                                string.Format("删除{0}条股票代码", effectStock));
        }

        private void UpdateUserStocks(string userId)
        {
            ClearStockOfUser(userId);

            var symbols = tbStocks.Text.Split(new[] {",", "\r\n"},
                                              StringSplitOptions.RemoveEmptyEntries);
            foreach (var symbol in symbols)
            {
                if (string.IsNullOrEmpty(symbol))
                {
                    continue;
                }

                AppendUserStock(userId, symbol);
            }
        }

        private void ClearStockOfUser(string userId)
        {
            //清理userstock表中相关用户的数据
            DatabaseHelper.Instance.ExecuteSql(
                "delete from userstock where userid=?userid",
                new object[] {userId});
        }

        private void AppendUserStock(string userId, string symbol)
        {
            string stockId = GetStockId(symbol);
            var count = int.Parse(DatabaseHelper.Instance.ExecuteScalar(
                "select count(1) from userstock where userid=?userid and stockid=?stockid",
                new object[]
                    {
                        userId, stockId
                    }).ToString());
            if (count != 0)
                return;

            DatabaseHelper.Instance.ExecuteSql(
                "insert into userstock(id,userid,stockid)values(?id,?userid,?stockid)",
                new object[]
                    {
                        Guid.NewGuid().ToString(),
                        userId, stockId
                    });
        }

        /// <summary>
        /// 获取股票标识，如存在此股票则返回其标识，否则插入数据库后，返回其标识
        /// </summary>
        /// <param name="symbol">股票代码</param>
        /// <returns>股票标识</returns>
        private string GetStockId(string symbol)
        {
            //数据库中的所有股票代码都是大写
            symbol = symbol.Trim().ToUpper();

            var count = int.Parse(DatabaseHelper.Instance.ExecuteScalar(
                "select count(1) from stock where stock=?symbol",
                new object[] {symbol}).ToString());

            if (count == 0)
            {
                DatabaseHelper.Instance.ExecuteSql(
                    "insert into stock(id,stock)values(?id,?stock)",
                    new object[]
                        {
                            Guid.NewGuid().ToString(),
                            symbol
                        });
            }

            return DatabaseHelper.Instance.ExecuteScalar(
                "select id from stock where stock=?symbol",
                new object[] {symbol}).ToString();
        }

        /// <summary>
        /// 处理用户信息
        /// </summary>
        /// <returns>UserId</returns>
        private string ProcessUserInfo()
        {
            var user = new User
                {
                    Name = tbUserName.Text.Trim().ToLower(),
                    Email = tbEmail.Text.Trim()
                };

            var count = int.Parse(DatabaseHelper.Instance.ExecuteScalar(
                "select count(1) from user where username=?username",
                new object[] {tbUserName.Text.Trim().ToLower()}).ToString());

            //如无此用户则添加，否则更新
            if (count == 0)
            {
                DatabaseHelper.Instance.ExecuteSql(
                    "insert into user(userid,username,email)values(?userid,?username,?email)",
                    new object[]
                        {
                            Guid.NewGuid().ToString(),
                            user.Name,
                            user.Email
                        });
            }
            else
            {
                DatabaseHelper.Instance.ExecuteSql(
                    "update user set email=?email where username=?username",
                    new object[]
                        {
                            user.Email,
                            user.Name
                        });
            }

            return DatabaseHelper.Instance.ExecuteScalar(
                "select userid from user where username=?username",
                new object[] {tbUserName.Text.Trim().ToLower()}).ToString();
        }

        private bool ValidateInput()
        {
            foreach (var c in Controls)
            {
                if ((c is Label) && (c as Label).Name.StartsWith("lbv"))
                {
                    (c as Label).Visible = false;
                }
            }

            return ValidateInput(tbUserName, lbvUserName)
                   && ValidateInput(tbEmail, lbvEmail)
                   && ValidateInput(tbStocks, lbvStocks);
        }

        private bool ValidateInput(TextBox input, Label indicator)
        {
            if (string.IsNullOrEmpty(input.Text.Trim()))
            {
                indicator.Visible = true;
                return false;
            }

            return true;
        }

        private void tbUserName_Leave(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(tbUserName.Text.Trim()) && string.IsNullOrEmpty(tbStocks.Text.Trim()))
                {
                    var userName = tbUserName.Text.Trim().ToLower();
                    tbEmail.Text = DatabaseHelper.Instance.ExecuteScalar(
                        "select email from user where username=?username",
                        new object[] {userName}).ToString();

                    var userId = DatabaseHelper.Instance.ExecuteScalar(
                        "select userid from user where username=?username",
                        new object[] {userName}).ToString();

                    var rows = DatabaseHelper.Instance.GetDataTable(
                        "select s.stock from stock s "
                        + " join userstock us on s.id=us.stockid and us.userid=?userid"
                        + " order by stock",
                        new object[] {userId}
                        ).Rows;
                    foreach (DataRow r in rows)
                    {
                        tbStocks.Text += r[0] + "\r\n";
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }
    }
}
