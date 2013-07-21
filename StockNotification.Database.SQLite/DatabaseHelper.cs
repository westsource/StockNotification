using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Data.SQLite;

namespace StockNotification.Database.SQLite
{
    public class DatabaseHelper
    {
        private static string ConnectionString
        {
            get
            {
                var cnn = string.Format("Data Source={0}{1}",
                                        AppDomain.CurrentDomain.BaseDirectory,
                                        ConfigurationManager.ConnectionStrings["database"].ConnectionString);
                return cnn;
            }
        }

        private static readonly DatabaseHelper instance = new DatabaseHelper();

        public static DatabaseHelper Instance
        {
            get { return instance; }
        }


        private int ExecuteSql(string sql, object[] values, 
            SQLiteConnection connection, SQLiteTransaction transaction)
        {
            var cmd = new SQLiteCommand(sql, connection);
            if (transaction != null)
            {
                cmd.Transaction = transaction;
            }
            SetParameters(cmd, sql, values);
            return cmd.ExecuteNonQuery();
        }

        private void SetParameters(SQLiteCommand cmd, string sql, object[] values)
        {
            if (values.Length > 0)
            {
                var names = ParseParameters(sql);
                for (int i = 0; i < values.Length; ++i)
                {
                    cmd.Parameters.AddWithValue(names[i], values[i]);
                }
            }
        }

        private List<string> ParseParameters(string sql)
        {
            var list = new List<String>();
            const RegexOptions options = ((RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline)
                                          | RegexOptions.IgnoreCase);
            //MySql
            //var regex = new Regex("\\?(\\w*)", options);
            //SQLite
            var regex = new Regex("\\$(\\w*)", options);
            var m = regex.Match(sql);
            while (m.Success)
            {
                list.Add(m.Value);
                m = m.NextMatch();
            }
            return list;
        }

        public int ExecuteSql(string sql)
        {
            return ExecuteSql(sql, new object[] {});
        }

        public int ExecuteSql(string sql, object[] values)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                try
                {
                    return ExecuteSql(sql, values, connection, null);
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public int ExecuteSql(string sql, List<object[]> valuesList)
        {
            int effect = 0;
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                var tran = connection.BeginTransaction();
                try
                {
                    effect += valuesList.Sum(values => ExecuteSql(sql, values, connection, tran));
                    tran.Commit();
                    return effect;
                }
                catch (Exception)
                {
                    tran.Rollback();
                    throw;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public DataTable GetDataTable(string sql)
        {
            return GetDataTable(sql, new object[] {});
        }

        public DataTable GetDataTable(string sql, object[] values)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                var cmd = new SQLiteCommand(sql, connection);
                SetParameters(cmd, sql, values);
                var adapter = new SQLiteDataAdapter(cmd);
                var table = new DataTable("data");
                adapter.Fill(table);
                return table;
            }
        }

        public object ExecuteScalar(string sql)
        {
            return ExecuteScalar(sql, new object[] {});
        }

        public object ExecuteScalar(string sql, object[] values)
        {
            var table = GetDataTable(sql, values);
            if (table.Rows.Count > 0)
            {
                return table.Rows[0][0];
            }

            throw new Exception("找不到任何数据");
        }
    }
}
