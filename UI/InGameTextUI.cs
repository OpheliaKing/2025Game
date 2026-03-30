using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shin
{
    public class InGameTextUI : UIBase
    {
        private TextMeshProUGUI _text;
        [SerializeField]
        private Transform _target;

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

        private Camera _camera;
        private Camera Camera
        {
            get
            {
                if (_camera == null)
                {
                     _camera = InGameManager.Instance.PlayerInfo.PlayerUnit.Camera;
                }
                return _camera;
            }
        }

        [SerializeField]
        private Vector3 _offset = new Vector3(0, 1.5f, 0);

        private void Awake()
        {
            base.Awake();
        }

        public void Show(string text, Vector3 position)
        {
            // 기존 시그니처는 "position"을 월드 좌표로 해석해서 화면 위치에 표시하도록 변경합니다.
            Show(text, position, null);
        }

        public void Show(string text, Vector3 worldPosition, Camera worldCamera)
        {
            base.Show();
            Text.SetText(text);

            _target = null;
        }

        public void Show(string text, Transform target, Camera worldCamera)
        {
            base.Show();
            Text.SetText(text);

            _target = target;
        }

        private void LateUpdate()
        {
            UpdateTargerPosition();
        }

        private void UpdateTargerPosition()
        {
            if (_target == null) return;
            transform.position = Camera.WorldToScreenPoint(_target.transform.position + _offset);
        }
    }
}