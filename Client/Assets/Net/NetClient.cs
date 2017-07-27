using UnityEngine;
using System;
using System.Collections;
using System.Net.Sockets;
using NetServer.Action;
using NetServer.Session;

/// <summary>
/// 网络客户端
/// </summary>
public class NetClient : MonoBehaviour
{
    //数据缓冲区长度,默认为1024字节
    private const int BUFFER_SIZE = 1024;

    //单例
    private static NetClient instance;

    //会话对象
    private SessionClient session = new SessionClient();

    //数据缓冲区
    private byte[] readBuffer = new byte[BUFFER_SIZE];

    //重连协程
    private Coroutine reconnectCoroutine;

    //单例全局访问接口
    public static NetClient Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("NetClient");
                instance = obj.AddComponent<NetClient>();
            }
            return instance;
        }
    }

    public SessionClient Session { get { return session; } }

    //主机地址
    public string Host { get; set; }

    //主机端口
    public int Port { get; set; }

    //连接超时毫秒
    public int Timeout { get; set; }

    private void Awake()
    {
        hideFlags = HideFlags.HideInHierarchy & HideFlags.HideInInspector;
        DontDestroyOnLoad(gameObject);

        Timeout = 3000;

        Session.InitializeAction();
    }

    private void Update()
    {
        //检测服务器连接断开重连
        if (reconnectCoroutine == null && session.State == SessionState.Close)
            Connect();
    }

    private void OnDestroy()
    {
        //游戏结束关闭套接字
        if (session != null && session.isUse)
        {
            session.Close();
        }
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    public void Connect()
    {
        if (session != null && session.isUse || string.IsNullOrEmpty(Host) || Port == 0)
            return;

        //创建套接字
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        session.AsyncConnect(socket, Host, Port);

        //启动超时重连
        if (reconnectCoroutine != null)
            StopCoroutine(reconnectCoroutine);
        reconnectCoroutine = StartCoroutine(AsyncConnectTimeout());
    }

    /// <summary>
    /// 异步连接服务器回调
    /// </summary>
    /// <param name="ar"></param>
    private void ConnectCallBack(IAsyncResult ar)
    {
        Socket socket = (Socket)ar.AsyncState;
        socket.EndConnect(ar);

        session = new SessionClient();
        session.AsyncReceive(socket);
    }

    /// <summary>
    /// 连接服务器超时重连机制(ms)
    /// </summary>
    /// <param name="timeoutLength"></param>
    /// <returns></returns>
    private IEnumerator AsyncConnectTimeout()
    {
        //每帧检测套接字是否连接
        DateTime currentTime = DateTime.Now;
        while ((int)(DateTime.Now - currentTime).TotalMilliseconds < Timeout)
        {
            if (session != null && session.isUse)
                break;
            else
                yield return null;
        }

        //超时后套接字未连接则循环重连
        if (!session.isUse)
        {
            session.Close();
            Connect();
        }

        reconnectCoroutine = null;
    }
}
