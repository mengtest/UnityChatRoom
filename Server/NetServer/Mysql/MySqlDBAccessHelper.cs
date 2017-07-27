using System;
using MySql.Data.MySqlClient;

namespace NetServer.Mysql
{
    public class MySqlDBAccessHelper
    {
        MySqlConnection dbConnection;
        MySqlDataReader dbReader;
        MySqlCommand dbCommand;

        public MySqlDBAccessHelper(string address, int port, string databaseName, string userName, string password)
        {
            string connectString = "Data Source={0};Port={1};Database={2};UserId={3};Password={4};";
            connectString = string.Format(connectString, address, port.ToString(), databaseName, userName, password);
            OpenDb(connectString);
        }

        /// <summary>
        /// 关闭数据库
        /// </summary>
        public void CloseDb()
        {
            if (dbConnection != null)
            {
                dbConnection.Close();
                dbConnection = null;
            }
            if (dbReader != null)
            {
                dbReader.Close();
                dbReader = null;
            }
            if (dbCommand != null)
            {
                dbCommand.Dispose();
                dbCommand = null;
            }
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        public MySqlDataReader ExecuteCommand(string commandString)
        {
            try
            {
                dbCommand = dbConnection.CreateCommand();
                dbCommand.CommandText = commandString;
                if (dbReader != null && !dbReader.IsClosed)
                    dbReader.Close();
                dbReader = dbCommand.ExecuteReader();
            }
            catch (Exception e)
            {
                Console.WriteLine("【数据库】执行命令异常：{0}", e.Message);
                return null;
            }
            return dbReader;
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        public bool Insert(string tableName, string[] colNames, string[] values)
        {
            string commandString = "INSERT INTO {0} ({1}) VALUES ({2});";
            string insertColums = colNames[0];
            string insertValues = values[0];
            for (int i = 1; i < colNames.Length; i++)
            {
                insertColums += "," + colNames[i];
                insertValues += "," + values[i];
            }
            commandString = string.Format(commandString, tableName, insertColums, insertValues);

            return ExecuteCommand(commandString) != null;
        }

        public bool Insert(string tableName, string colName, string value)
        {
            return Insert(tableName, new string[] { colName }, new string[] { value });
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        public MySqlDataReader Select(string tableName, string[] selectName, string[] whereNames, string[] operations, string[] values, string order)
        {
            //拼接select部分命令
            string commandString = "SELECT {0} FROM {1}";
            string selectString = selectName[0];
            for (int i = 1; i < selectName.Length; i++)
            {
                selectString += "," + selectName[i];
            }
            commandString = string.Format(commandString, selectString, tableName);

            //拼接where部分命令
            if (whereNames != null)
            {
                string whereString = "{0} WHERE {1}";
                string conditionsFormat = " OR {0}{1}{2}";
                string conditionsString = whereNames[0] + operations[0] + values[0];
                for (int i = 1; i < whereNames.Length; i++)
                {
                    conditionsString += string.Format(conditionsFormat, whereNames[i], operations[i], values[i]);
                }
                commandString = string.Format(whereString, commandString, conditionsString);
            }

            //拼接order部分命令
            if (order != null)
            {
                string orderString = "{0} ORDER {1}";
                commandString = string.Format(orderString, commandString, order);
            }
            return ExecuteCommand(commandString);
        }

        public MySqlDataReader Select(string tableName, string[] selectName)
        {
            return Select(tableName, selectName, null, null, null, null);
        }

        public MySqlDataReader Select(string tableName, string[] selectName, string order)
        {
            return Select(tableName, selectName, null, null, null, order);
        }

        public MySqlDataReader Select(string tableName, string[] selectName, string[] whereNames, string[] operations, string[] values)
        {
            return Select(tableName, selectName, whereNames, operations, values, null);
        }

        /// <summary>
        /// 打开数据库
        /// </summary>
        private void OpenDb(string connectString)
        {
            dbConnection = new MySqlConnection(connectString);
            try
            {
                dbConnection.Open();
                Console.WriteLine("数据库启动成功!");
            }
            catch (Exception e)
            {
                Console.WriteLine("【数据库】连接失败 {0}", e.Message);
                return;
            }
        }
    }
}
