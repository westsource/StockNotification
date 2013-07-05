using System;
using System.Data;
using System.Windows.Forms;
using StockNotification.Common;
using StockNotification.Database.Interface;
using StockNotification.WinService.Entity;

namespace StockNotification.Gui
{
    public partial class frmMain : Form
    {
        private readonly IStore store;

        public frmMain()
        {
            store = ApplicationEntry.Instance.GetService<IStore>();
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
                store.ClearOverdueStock();

                MessageBox.Show("导入完成");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }

        private void UpdateUserStocks(string userId)
        {
            var effectStock = store.ClearStockOfUser(userId);
            LogManager.WriteLog(LogFileType.Trace,
                                string.Format("删除{0}条股票代码", effectStock));

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

        private void AppendUserStock(string userId, string symbol)
        {
            string stockId = GetStockId(symbol);
            store.SaveStockForUser(userId, symbol);
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
            var stock = store.GetStock(symbol);
            return stock.Id;
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

            var u = store.SaveUser(user);
            return u.Id;
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
                var needRefresh = !string.IsNullOrEmpty(tbUserName.Text.Trim());
                if (sender.GetType() != this.GetType())
                {
                    needRefresh = needRefresh && string.IsNullOrEmpty(tbStocks.Text.Trim());
                }

                if (needRefresh)
                {
                    var userName = tbUserName.Text.Trim().ToLower();
                    var user = store.GetUser(userName);
                    if (null == user)
                    {
                        return;
                    }

                    tbEmail.Text = user.Email;

                    var stocks = store.GetStockOfUser(user.Id);
                    tbStocks.Text = "";
                    foreach (var s in stocks)
                    {
                        tbStocks.Text += s.Symbol + "\r\n";
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            tbUserName_Leave(this, null);
        }
    }
}
