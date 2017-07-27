using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using NetServer.Session;
using UnityEngine.SceneManagement;

public class Connect : MonoBehaviour
{
    //服务器设置输入
    public InputField hostInput;
    public InputField portInput;

    public Text tipText;
    public Button button;

    private void Update()
    {
        if (NetClient.Instance.Session.State == SessionState.Run)
        {
            SceneManager.LoadScene("Main");
            enabled = false;
        }
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    public void ConnectToServer()
    {
        if (NetClient.Instance.Session.State == SessionState.Connect)
            return;

        tipText.text = "正在连接服务器...";
        button.gameObject.SetActive(false);
        NetClient.Instance.Host = hostInput.text;
        NetClient.Instance.Port = int.Parse(portInput.text);
        NetClient.Instance.Connect();
    }
}
