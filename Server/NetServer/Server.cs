using System;
using System.Net;
using System.Net.Sockets;
using NetServer.Action;
using NetServer.Session;

namespace NetServer
{
    /// <summary>
    /// 服务器类
    /// </summary>
    public class Server
    {
        /// <summary>
        /// 监听套接字
        /// </summary>
        public Socket listen;

        /// <summary>
        /// 最大连接数
        /// </summary>
        public int maxSessionClient = 50;

        //会话端池
        private SessionClientPool pool = SessionClientPool.Instance;

        /// <summary>
        /// 启动服务器
        /// </summary>
        public void StartServer(string host, int port)
        {
            pool.SetMaxSessionClient(maxSessionClient);
            listen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddress = IPAddress.Parse(host);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
            listen.Bind(ipEndPoint);
            listen.Listen(maxSessionClient);
            listen.BeginAccept(AcceptCallBack, null);
            Console.WriteLine("服务器启动成功！");
        }

        /// <summary>
        /// 异步建立客户端连接回调
        /// </summary>
        private void AcceptCallBack(IAsyncResult ar)
        {
            try
            {
                Socket socket = listen.EndAccept(ar);
                SessionClient session = pool.GetSessionClient();

                if (session == null)
                {
                    socket.Close();
                    Console.WriteLine("警告：连接已满！");
                }
                else
                {
                    session.Initialize(socket);
                    string address = session.GetRemoteAddress();
                    Console.WriteLine("客户端连接 [{0}]", address);
                }

                listen.BeginAccept(AcceptCallBack, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("异步建立客户端连接失败：" + e.Message);
            }
        }
    }
}
