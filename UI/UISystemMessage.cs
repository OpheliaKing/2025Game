using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Shin
{
    public class UISystemMessage : UIBase
    {
        private TextMeshProUGUI _text;

        private TextMeshProUGUI Text
        {
            get
            {
                if (_text == null)
                {
                    _text = GetComponentInChildren<TextMeshProUGUI>();
                }

                return _text;
            }
        }

        private Animator _anim;
        private Animator Anim
        {
            get
            {
                if (_anim == null)
                {
                    _anim = GetComponentInChildren<Animator>();
                }
                return _anim;
            }
        }

        private IEnumerator _hideTimeCo;
        private float _hideTime = 5.0f;



        public void Show(string message, float duration = 5.0f)
        {
            base.Show();
            _hideTime = duration;
            Text.SetText(message);

            if (_hideTimeCo != null)
            {
                StopCoroutine(_hideTimeCo);
            }
            _hideTimeCo = HideTimeCo();
            StartCoroutine(_hideTimeCo);
        }


        private IEnumerator HideTimeCo()
        {
            Anim.SetBool("Hide", false);
            Anim.Play("UI_Show_Size_Up");

            yield return new WaitForSeconds(_hideTime);

            Anim.Play("UI_Show_Size_Down");

            yield return new WaitUntil(() => Anim.GetCurrentAnimatorStateInfo(0).IsName("UI_Show_Size_Down")
            && Anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);

            Hide();
            _hideTimeCo = null;
        }
    }
}