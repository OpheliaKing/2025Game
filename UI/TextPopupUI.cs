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
        private IEnumerator _hideTimeCo;
        private float _hideTime = 1.0f;

        public override void Show()
        {
            base.Show();

            if(_hideTimeCo != null)
            {
                StopCoroutine(_hideTimeCo);
            }
            _hideTimeCo = HideTimeCo();
            StartCoroutine(_hideTimeCo);
        }

        public void Show(string text)
        {
             base.Show();
             SetText(text);
        }

        public void SetText(string text)
        {
            _text.SetText(text);
        }

        private IEnumerator HideTimeCo()
        {
            yield return new WaitForSeconds(_hideTime);
            Hide();
            _hideTimeCo = null;
        }
    }
}
