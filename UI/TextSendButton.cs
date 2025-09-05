using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextSendButton : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _text;

    public TextSnedButtonType Type;

    private Action<string> _callback;

    public void SetCallback(Action<string> callback)
    {
        _callback = callback;
    }

    public void OnCallback()
    {
        Debug.Log("Test Callback");
        _callback?.Invoke(_text.text);
    }
}

public enum TextSnedButtonType
{
    CREATE_ROOM,
    JOIN_ROOM
}
