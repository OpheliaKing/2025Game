using System.Collections;
using System.Collections.Generic;
using Shin;
using TMPro;
using UnityEngine;

namespace Shin
{
    public class TextPopupUI : UIBase
    {
        [SerializeField]
        private TextMeshProUGUI _text;

        public override void Show()
        {
            base.Show();
        }

        public void SetText(string text)
        {
            _text.SetText(text);
        }
    }
}
