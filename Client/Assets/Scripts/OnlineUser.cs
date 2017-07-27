using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class OnlineUser : MonoBehaviour
{
    public RectTransform onlinePanel;
    public Text onlineListUI;

    void Start()
    {
        NetClient.Instance.Session.Action.AddListener(100, UserList);
        StartCoroutine(SyncOnlineListTask());
    }

    void Update()
    {
        if (NetClient.Instance.Session.State != NetServer.Session.SessionState.Run && onlinePanel.gameObject.activeSelf)
        {
            onlinePanel.gameObject.SetActive(false);
        }
        else if (NetClient.Instance.Session.State != NetServer.Session.SessionState.Run && !onlinePanel.gameObject.activeSelf)
        {
            onlinePanel.gameObject.SetActive(true);
        }
    }

    private void UserList(ActionParameter parameter)
    {
        List<string> onlineList = parameter.GetValue<List<string>>("onlineList");
        onlineListUI.text = onlineList[0];
        for (int i = 1; i < onlineList.Count; i++)
        {
            onlineListUI.text += "\r\n" + onlineList[i];
        }

        RectTransform onlineRect = onlineListUI.rectTransform;
        onlineRect.sizeDelta = new Vector2(onlineRect.sizeDelta.x, onlineListUI.preferredHeight);
    }

    private IEnumerator SyncOnlineListTask()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            if (NetClient.Instance.Session.State == NetServer.Session.SessionState.Run)
            {
                try
                {
                    NetClient.Instance.Session.SendAction(100, null);
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }
        }
    }
}
