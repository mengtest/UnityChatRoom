using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetServer
{
    public class Server
    {
        /// <summary>
        /// 监听套接字
        /// </summary>
        public Socket listen;

        /// <summary>
        /// 连接池
        /// </summary>
        public Connection[] connections;

        /// <summary>
        /// 最大连接数
        /// </summary>
        public int maxConnection = 50;

        /// <summary>
        /// 数据库连接
        /// </summary>
        private MySqlDBAccessHelper database;

        ~Server()
        {
            for (int i = 0; i < connections.Length; i++)
            {
                if (connections[i].isUse)
                {
                    connections[i].Close();
                }
            }
        }

        /// <summary>
        /// 获取连接池索引，返回负数表示获取失败
        /// </summary>
        public int GetIndex()
        {
            if (connections == null)
                return -1;

            for (int i = 0; i < connections.Length; i++)
            {
                if (connections[i] == null)
                {
                    connections[i] = new Connection();
                    return i;
                }
                else if (!connections[i].isUse)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 启动数据库连接
        /// </summary>
        public void StartDatabase(string dbAddress, int dbPort, string dbName, string userName, string password)
        {
            database = new MySqlDBAccessHelper(dbAddress, dbPort, dbName, userName, password);
        }

        /// <summary>
        /// 启动服务器
        /// </summary>
        public void StartServer(string host, int port)
        {
            connections = new Connection[maxConnection];
            for (int i = 0; i < connections.Length; i++)
            {
                connections[i] = new Connection();
            }

            listen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddress = IPAddress.Parse(host);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
            listen.Bind(ipEndPoint);
            listen.Listen(maxConnection);
            listen.BeginAccept(AcceptCallBack, null);
            Console.WriteLine("服务器启动成功！");
        }

        /// <summary>
        /// 广播消息
        /// </summary>
        public void Broadcast(byte[] bytes)
        {
            for (int i = 0; i < connections.Length; i++)
            {
                if (connections[i] == null || !connections[i].isUse)
                    continue;

                Console.WriteLine("将消息转发给 {0}", connections[i].GetAddress());
                connections[i].socket.Send(bytes);
            }
        }

        /// <summary>
        /// 转发广播消息,消息将带上时间和地址
        /// </summary>
        public void ForwardBroadcast(string endPoint, string message, DateTime time)
        {
            string _time = "[" + time.ToString("yyyy-MM-dd HH:mm:ss") + "] ";
            string ip = endPoint + "：";
            string processMessage = string.Format("{0}{1}\r\n{2}", _time, ip, message);
            byte[] bytes = Encoding.UTF8.GetBytes(processMessage);
            Broadcast(bytes);
        }

        /// <summary>
        /// 异步建立客户端连接回调
        /// </summary>
        private void AcceptCallBack(IAsyncResult ar)
        {
            try
            {
                Socket socket = listen.EndAccept(ar);
                int index = GetIndex();

                if (index < 0)
                {
                    socket.Close();
                    Console.WriteLine("警告：连接已满！");
                }
                else
                {
                    //广播该用户连接消息
                    ForwardBroadcast(socket.LocalEndPoint.ToString(), "上线", DateTime.Now);

                    //创建会话
                    Connection connection = connections[index];
                    connection.Initialize(socket);
                    string address = connection.GetAddress();
                    Console.WriteLine("客户端连接 [{0}] 连接池ID：{1}", address, index);

                    connection.socket.BeginReceive(connection.readBuffer, connection.readBufferCount, connection.BuffRemain(), SocketFlags.None, ReceiveCallBack, connection);
                }

                listen.BeginAccept(AcceptCallBack, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("异步建立客户端连接失败：" + e.Message);
            }
        }

        /// <summary>
        /// 异步接收数据回调
        /// </summary>
        private void ReceiveCallBack(IAsyncResult ar)
        {
            Connection connection = (Connection)ar.AsyncState;
            try
            {
                int count = connection.socket.EndReceive(ar);
                if (count <= 0)
                {
                    string connAddress = connection.GetAddress();

                    Console.WriteLine("收到 [{0}] 断开连接", connection.GetAddress());
                    connection.Close();

                    //广播该用户断开连接消息
                    ForwardBroadcast(connAddress, "下线", DateTime.Now);
                    return;
                }

                string message = Encoding.UTF8.GetString(connection.readBuffer, connection.readBufferCount, count);
                Console.WriteLine("收到 [{0}] 数据：{1}", connection.GetAddress(), message);

                string address = connection.GetAddress();
                DateTime now = DateTime.Now;
                RecordLog(address, message, now);
                ForwardBroadcast(address, message, now);

                connection.socket.BeginReceive(connection.readBuffer, connection.readBufferCount, connection.BuffRemain(), SocketFlags.None, ReceiveCallBack, connection);
            }
            catch (Exception e)
            {
                string connAddress = connection.GetAddress();

                Console.WriteLine("收到 {0} 断开连接", connection.GetAddress());
                connection.Close();

                //广播该用户断开连接消息
                ForwardBroadcast(connAddress, "下线", DateTime.Now);
            }
        }

        /// <summary>
        /// 记录消息日志
        /// </summary>
        private void RecordLog(string endPoint, string message, DateTime time)
        {
            if (database == null)
                return;

            string[] columNames = new string[] { "ip", "message", "time" };
            string[] values = new string[] { string.Format("'{0}'", endPoint), message, string.Format("'{0}'", time.ToString("yyyy-MM-dd HH:mm:ss")) };
            database.Insert("messagelog", columNames, values);
        }
    }
}
