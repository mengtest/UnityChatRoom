using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class Program : MonoBehaviour
{
    //服务器设置输入
    public InputField hostInput;
    public InputField portInput;

    //消息输入
    public InputField messageInput;

    //面板消息
    public Text clientText;
    public Text messageTextPrefab;
    public RectTransform messageContent;

    //最大显示消息数
    public int maxMessageCount = 20;

    //消息间隔
    public float messageSpace = 5.0f;

    //数据缓冲区长度,默认为1024字节
    private const int BUFFER_SIZE = 1024;

    //套接字
    private Socket socket;

    //消息预设
    private List<Text> messagesText;

    //数据缓冲区
    private byte[] readBuffer = new byte[BUFFER_SIZE];

    //接收消息
    private string receiveMessage;

    //是否重连
    private bool isReconnection;

    private void Start()
    {
        messagesText = new List<Text>();
        Connection();
    }

    private void Update()
    {
        //重连服务器
        if (isReconnection)
        {
            Connection();
            isReconnection = false;
        }

        //更新消息面板
        if (receiveMessage != null)
        {
            lock (receiveMessage)
            {
                CreateMessage(receiveMessage);
                receiveMessage = null;
            }
        }

        //连接时重置消息面板
        if (socket != null)
        {
            if (socket.Connected)
            {
                clientText.text = socket.LocalEndPoint.ToString();
            }
            else
            {
                clientText.text = "正在连接中...";
            }
        }
    }

    private void OnDestroy()
    {
        //游戏结束关闭套接字
        if (socket != null && socket.Connected)
        {
            socket.Close();
        }
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    public void Connection()
    {
        if (socket != null && socket.Connected)
            return;

        //创建套接字
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        string host = hostInput.text;
        int port = int.Parse(portInput.text);

        //开始异步连接服务器
        socket.BeginConnect(host, port, ConnectCallBack, socket);

        //启动超时重连
        StartCoroutine(AsyncConnectTimeout(1000));

        //学习测试
        /*string message = "Hello Unity!";
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        socket.Send(bytes);

        int count = socket.Receive(readBuffer);
        message = Encoding.UTF8.GetString(readBuffer, 0, count);
        receiveText.text = message;

        socket.Close();*/
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    public void Send()
    {
        string message = messageInput.text;
        messageInput.text = "";
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        try
        {
            socket.Send(bytes);
        }
        catch { }
    }

    /// <summary>
    /// 异步连接服务器回调
    /// </summary>
    /// <param name="ar"></param>
    private void ConnectCallBack(IAsyncResult ar)
    {
        socket = (Socket)ar.AsyncState;
        socket.EndConnect(ar);

        MessageListClear();

        //开始异步接收数据
        socket.BeginReceive(readBuffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallBack, socket);
    }

    /// <summary>
    /// 异步接收数据回调
    /// </summary>
    /// <param name="ar"></param>
    private void ReceiveCallBack(IAsyncResult ar)
    {
        try
        {
            //检测套接字是否断开
            if (!socket.Connected)
                return;

            int count = socket.EndReceive(ar);

            //服务器断开启动重连
            if (count <= 0)
            {
                socket.Close();
                isReconnection = true;
                return;
            }

            //数据解析
            string message = Encoding.UTF8.GetString(readBuffer, 0, count);
            receiveMessage = message;

            //循环异步开始接收数据
            socket.BeginReceive(readBuffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallBack, null);
        }
        catch (Exception e)
        {
            //套接字异常断开启动重连
            Debug.Log(e);
            socket.Close();
            isReconnection = true;
        }
    }

    private void MessageListClear()
    {
        if (messagesText.Count > 0)
        {
            for (int i = messagesText.Count - 1; i > 0; i--)
            {
                Destroy(messagesText[i].gameObject);
                messagesText.RemoveAt(i);
            }
        }
    }

    private void CreateMessage(string message)
    {
        if (messageContent == null || messageTextPrefab == null)
            return;

        Text messageText = Instantiate(messageTextPrefab);
        messageText.text = message;

        RectTransform messageRect = messageText.rectTransform;
        messageRect.SetParent(messageContent);
        messageRect.offsetMin = new Vector2(0, messageRect.offsetMin.y);
        messageRect.offsetMax = new Vector2(0, messageRect.offsetMax.y);
        messageRect.anchoredPosition = new Vector2(messageRect.anchoredPosition.x, 0);
        messageRect.sizeDelta = new Vector2(messageRect.sizeDelta.x, messageText.preferredHeight);

        if (messagesText.Count >= maxMessageCount)
            messagesText.RemoveAt(0);

        messagesText.Add(messageText);

        for (int i = 0; i < messagesText.Count - 1; i++)
        {
            float height = 0;
            for (int j = i + 1; j < messagesText.Count; j++)
            {
                height += messagesText[j].preferredHeight;
            }
            messagesText[i].rectTransform.anchoredPosition = new Vector2(messagesText[i].rectTransform.anchoredPosition.x, height);
        }

        float totalHeight = 0;
        for (int i = 0; i < messagesText.Count; i++)
        {
            totalHeight += messagesText[i].preferredHeight + messageSpace;
        }
        messageContent.sizeDelta = new Vector2(messageContent.sizeDelta.x, totalHeight);
    }

    /// <summary>
    /// 连接服务器超时重连机制(ms)
    /// </summary>
    /// <param name="timeoutLength"></param>
    /// <returns></returns>
    private IEnumerator AsyncConnectTimeout(int timeoutLength)
    {
        //每帧检测套接字是否连接
        DateTime currentTime = DateTime.Now;
        while ((int)(DateTime.Now - currentTime).TotalMilliseconds < timeoutLength)
        {
            if (socket.Connected)
                break;
            else
                yield return null;
        }

        //超时后套接字未连接则循环重连
        if (!socket.Connected)
        {
            socket.Close();
            isReconnection = true;
        }
    }
}
