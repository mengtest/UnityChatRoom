using System;
using System.Net;
using System.Net.Sockets;

namespace NetServer
{
    public class Connection
    {
        /// <summary>
        /// 缓冲区大小
        /// </summary>
        public const int BUFFER_SIZE = 1024;

        /// <summary>
        /// 套接字
        /// </summary>
        public Socket socket;

        /// <summary>
        /// 读数据缓冲区大小
        /// </summary>
        public byte[] readBuffer = new byte[BUFFER_SIZE];

        /// <summary>
        /// 接收超时时间
        /// </summary>
        public int receiveTimeout = 3000;

        /// <summary>
        /// 当前读缓冲区长度
        /// </summary>
        public int readBufferCount = 0;

        /// <summary>
        /// 是否使用
        /// </summary>
        public bool isUse = false;

        public Connection()
        {
            readBuffer = new byte[BUFFER_SIZE];
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(Socket socket)
        {
            this.socket = socket;
            this.socket.ReceiveTimeout = receiveTimeout;
            isUse = true;
            readBufferCount = 0;
        }

        /// <summary>
        /// 读缓冲区剩余字节数
        /// </summary>
        public int BuffRemain()
        {
            return BUFFER_SIZE - readBufferCount;
        }

        /// <summary>
        /// 获取套接字地址
        /// </summary>
        public string GetAddress()
        {
            if (!isUse)
                return null;
            return socket.RemoteEndPoint.ToString();
        }

        /// <summary>
        /// 关闭套接字
        /// </summary>
        public void Close()
        {
            if (!isUse)
                return;
            socket.Close();
            isUse = false;
        }
    }
}
