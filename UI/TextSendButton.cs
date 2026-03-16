using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shin
{
    public class TextSendButton : UIButtonBase
    {
        [SerializeField]
        private TextMeshProUGUI _text;

        public TextSnedButtonType Type;

        private Action<string> _callback;

        public void SetCallback(Action<string> callback)
        {
            _callback = callback;
        }

        public override void OnClick()
        {
            base.OnClick();
            _callback?.Invoke(_text.text);
        }
    }

    public enum TextSnedButtonType
    {
        CREATE_ROOM,
        JOIN_ROOM
    }

}
