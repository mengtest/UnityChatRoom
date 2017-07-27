using UnityEngine;
using System;
using System.Net.Sockets;

namespace NetServer.Session
{
    /// <summary>
    /// 会话状态
    /// </summary>
    public enum SessionState
    {
        None,
        Connect,
        Run,
        Close
    }

    /// <summary>
    /// 会话客户端类
    /// </summary>
    public class SessionClient
    {
        /// <summary>
        /// 缓冲区大小
        /// </summary>
        private const int BUFFER_SIZE = 1024;

        /// <summary>
        /// 读数据缓冲区大小
        /// </summary>
        private byte[] readBuffer = new byte[BUFFER_SIZE];

        /// <summary>
        /// 套接字
        /// </summary>
        private Socket socket;

        /// <summary>
        /// 动态缓冲区
        /// </summary>
        private DynamicBuffer dynamicBuffer = new DynamicBuffer(BUFFER_SIZE);

        /// <summary>
        /// 请求处理
        /// </summary>
        private ActionHandler actionHandler;

        /// <summary>
        /// 是否使用
        /// </summary>
        public bool isUse = false;

        public ActionHandler Action { get { return actionHandler; } }
        public SessionState State { get; set; }
        public int ReceiveTimeout { set { socket.ReceiveTimeout = value; } }
        public int SendTimeout { set { socket.SendTimeout = value; } }

        /// <summary>
        /// 初始化处理对象
        /// </summary>
        public void InitializeAction()
        {
            if (actionHandler == null)
            {
                GameObject obj = new GameObject("ActionHandler");
                actionHandler = obj.AddComponent<ActionHandler>();
                actionHandler.hideFlags = HideFlags.HideInHierarchy & HideFlags.HideInInspector;
                GameObject.DontDestroyOnLoad(obj);
            }
        }

        /// <summary>
        /// 启动套接字异步接收
        /// </summary>
        /// <param name="socket"></param>
        public void AsyncReceive(Socket socket)
        {
            State = SessionState.Run;
            this.socket = socket;
            isUse = true;
            socket.BeginReceive(readBuffer, 0, readBuffer.Length, SocketFlags.None, ReceiveCallBack, null);
        }

        /// <summary>
        /// 启动套接字异步连接
        /// </summary>
        public void AsyncConnect(Socket socket, string host, int port)
        {
            State = SessionState.Connect;
            this.socket = socket;
            socket.BeginConnect(host, port, ConnectCallBack, this.socket);
        }

        /// <summary>
        /// 发送数据包
        /// </summary>
        public void Send(DataPackage packet)
        {
            try
            {
                socket.Send(packet.Pack());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 发送指定类型处理消息
        /// </summary>
        public void SendAction(int actionType, ActionParameter parameter)
        {
            DataPackage packet = Action.SendProcess(actionType, parameter);
            if (packet != null)
                Send(packet);
        }

        /// <summary>
        /// 获取远程套接字地址
        /// </summary>
        public string GetRemoteAddress()
        {
            if (!isUse)
                return null;

            return socket.RemoteEndPoint.ToString();
        }

        /// <summary>
        /// 获取本地套接字地址
        /// </summary>
        public string GetLocalAddress()
        {
            if (!isUse)
                return null;

            return socket.LocalEndPoint.ToString();
        }

        /// <summary>
        /// 关闭套接字
        /// </summary>
        public void Close()
        {
            if (!isUse)
                return;

            State = SessionState.Close;
            socket.Close();
            dynamicBuffer.Clear();
            isUse = false;
        }

        /// <summary>
        /// 异步连接回调
        /// </summary>
        private void ConnectCallBack(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);

            //开始异步接收数据
            AsyncReceive(socket);
        }

        /// <summary>
        /// 异步接收回调
        /// </summary>
        private void ReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                int count = socket.EndReceive(ar);
                if (count <= 0)
                {
                    Debug.Log("服务器断开连接");
                    Close();
                    return;
                }

                dynamicBuffer.WriteBytes(readBuffer, count);

                //数据解析
                DataPackage packet;
                while ((packet = dynamicBuffer.UnPack()) != null)
                {
                    Action.DisposeProcess(packet);
                }

                socket.BeginReceive(readBuffer, 0, readBuffer.Length, SocketFlags.None, ReceiveCallBack, null);
            }
            catch (Exception)
            {
                Close();
            }
        }
    }
}
