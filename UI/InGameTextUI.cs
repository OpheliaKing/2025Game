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

        // 화면(UI) 상에서 위치를 잡기 위한 RectTransform/Canvas 캐싱
        private RectTransform _rectTransform;
        private Canvas _canvas;
        private RectTransform _canvasRectTransform;

        // 월드 좌표를 스크린 좌표로 바꾸는데 사용할 카메라
        private Camera _worldCamera;

        // 따라다닐 대상(Transform 방식) / 고정 위치(월드 Vector 방식)
        private Transform _target;
        private Vector3 _staticWorldPosition;

        [Header("Floating Options")]
        [SerializeField]
        private Vector3 _worldOffset = Vector3.up * 1f;

        [SerializeField]
        private bool _hideWhenOffScreen = true;

        private Color _originalTextColor;
        private bool _isFollowing;
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

        private void Awake()
        {
            CacheReferences();
            base.Awake();
        }

        private void CacheReferences()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            if (_canvas != null)
            {
                _canvasRectTransform = _canvas.GetComponent<RectTransform>();

                // Canvas.renderMode가 Screen Space - Camera / World Space 인 경우, UI 변환에 사용할 카메라가 있을 수 있음.
                // 다만 "월드 -> 스크린" 변환은 항상 _worldCamera로 처리함(아래 UpdateScreenPosition에서 사용).
            }
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
            _staticWorldPosition = worldPosition;
            _worldCamera = worldCamera != null ? worldCamera : Camera.main;
            _isFollowing = true;

            _originalTextColor = Text.color;
            UpdateScreenPosition();
        }

        public void Show(string text, Transform target, Camera worldCamera)
        {
            base.Show();
            Text.SetText(text);

            _target = target;
            _staticWorldPosition = target != null ? target.position : Vector3.zero;
            _worldCamera = worldCamera != null ? worldCamera : Camera.main;
            _isFollowing = true;

            _originalTextColor = Text.color;
            UpdateScreenPosition();
        }

        private void LateUpdate()
        {
            if (!_isFollowing || !gameObject.activeSelf) return;
            UpdateScreenPosition();
        }

        private void UpdateScreenPosition()
        {
            // 월드 -> 스크린 변환/캔버스 좌표 변환에 필요한 최소 요소들이 없으면 기존 동작처럼 월드 좌표에 둠.
            if (_worldCamera == null || _rectTransform == null || _canvasRectTransform == null)
            {
                transform.position = (_target != null ? _target.position : _staticWorldPosition) + _worldOffset;
                return;
            }

            Vector3 worldPos = (_target != null ? _target.position : _staticWorldPosition) + _worldOffset;
            Vector3 screenPos = _worldCamera.WorldToScreenPoint(worldPos);

            // 카메라 뒤에 있으면 offscreen 처리
            bool isInFront = screenPos.z > 0f;
            bool isInViewport = screenPos.x >= 0f && screenPos.x <= Screen.width && screenPos.y >= 0f && screenPos.y <= Screen.height;

            if (_hideWhenOffScreen && (!isInFront || !isInViewport))
            {
                if (Text != null)
                {
                    Text.color = new Color(_originalTextColor.r, _originalTextColor.g, _originalTextColor.b, 0f);
                }
                return;
            }

            if (_hideWhenOffScreen && Text != null)
            {
                Text.color = _originalTextColor;
            }

            // Canvas 좌표계로 변환(anchoredPosition)
            Vector2 localPoint;
            // Canvas.renderMode에 따라 UI 좌표 변환에 필요한 카메라가 달라질 수 있음.
            Camera uiCamera = null;
            if (_canvas != null)
            {
                if (_canvas.renderMode == RenderMode.ScreenSpaceCamera || _canvas.renderMode == RenderMode.WorldSpace)
                {
                    uiCamera = _canvas.worldCamera;
                }
            }
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRectTransform,
                screenPos,
                uiCamera,
                out localPoint
            );

            _rectTransform.anchoredPosition = localPoint;
        }
    }
}