using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Shin
{
    public class UIManager : ManagerBase
    {
        //Inspecter
        [SerializeField]
        private Transform _canvas;

        //추후 리소스 매니저에서 가져오는 방식으로 수정해야됨

        [SerializeField]
        private List<UIBase> _uiPrefabList;

        private List<UIBase> _cashedUI;

        private readonly Stack<UIBase> _uiStack = new Stack<UIBase>();

        public UIBase Current => _uiStack.Count > 0 ? _uiStack.Peek() : null;

        public override void ManagerInit()
        {
            base.ManagerInit();
            if (_cashedUI == null)
            {
                _cashedUI = new List<UIBase>();
            }
        }

        public UIBase ShowUI(string uiName)
        {
            if (string.IsNullOrEmpty(uiName))
            {
                Debug.LogError("ShowUI called with null or empty uiName");
                return null;
            }

            UIBase uiInstance = null;
            if (_cashedUI != null)
            {
                uiInstance = _cashedUI.Find(u => u != null && u.name == uiName);
            }

            if (uiInstance == null)
            {
                var reManager = GameManager.Instance.ResourceManager;
                uiInstance = reManager.InstantiatePrefab<UIBase>(uiName, _canvas, reManager.UIPrefabPath);
                if (uiInstance == null)
                {
                    Debug.LogError($"Not Found Prefab!!! {uiName} ");
                    return null;
                }

                uiInstance.name = uiName;
                if (_cashedUI == null)
                {
                    _cashedUI = new List<UIBase>();
                }
                _cashedUI.Add(uiInstance);
            }
            else
            {
                if (uiInstance.transform.parent != _canvas)
                {
                    uiInstance.transform.SetParent(_canvas, false);
                }
            }

            Push(uiInstance);

            return uiInstance;
        }

        public void ShowSystemMessage(string message, float duration = 3.0f)
        {
            UIBase uiInstance = ShowUI("UISystemPopup");
            if (uiInstance is UISystemMessage systemMessageUI)
            {
                systemMessageUI.Show(message, duration);
            }
        }

        public void ShowTextPopup(string text)
        {
            UIBase uiInstance = ShowUI("TextPopupUI");
            if (uiInstance is TextPopupUI textPopupUI)
            {
                textPopupUI.Show(text);
            }
        }

        public void FadeIn(float duration, Action onComplete = null)
        {
            UIBase uiInstance = ShowUI("FadeUI");
            if (uiInstance is FadeUI fadeUI)
            {
                fadeUI.FadeIn(duration, onComplete);
            }
        }

        public void FadeOut(float duration, Action onComplete = null)
        {
            UIBase uiInstance = ShowUI("FadeUI");
            if (uiInstance is FadeUI fadeUI)
            {
                fadeUI.FadeOut(duration, onComplete);
            }
        }

        public void ShowNetworkConnectPopup()
        {
            UIBase uiInstance = ShowUI("NetworkConnectPopup");
            if (uiInstance is NetworkConnectPopup networkConnectPopup)
            {
                networkConnectPopup.Show();
            }
        }

        public void HideNetworkConnectPopup()
        {
            UIBase uiInstance = ShowUI("NetworkConnectPopup");
            if (uiInstance is NetworkConnectPopup networkConnectPopup)
            {
                networkConnectPopup.Hide();
            }
        }
        public void Push(UIBase ui)
        {
            if (ui == null)
            {
                return;
            }

            // UI_TYPE이 NONE인 UI는 스택 관리에서 제외한다.
            // (Current 전환/OnPush/OnFocus 호출 없이 화면만 표시)
            var uiTypeField = typeof(UIBase).GetField("_uiType", BindingFlags.NonPublic | BindingFlags.Instance);
            if (uiTypeField != null)
            {
                var uiTypeObj = uiTypeField.GetValue(ui);
                if (uiTypeObj != null && uiTypeObj.Equals(UI_TYPE.NONE))
                {
                    ui.Show();
                    return;
                }
            }

            if (Current != null)
            {
                Current.OnUnfocus();
            }

            _uiStack.Push(ui);
            ui.Show();
            ui.OnPush();
            ui.OnFocus();
        }

        public UIBase Pop()
        {
            if (_uiStack.Count == 0)
            {
                return null;
            }

            UIBase top = _uiStack.Pop();
            top.OnUnfocus();
            top.OnPop();
            top.Hide();

            if (Current != null)
            {
                Current.Show();
                Current.OnFocus();
            }

            return top;
        }

        public void PopUntil(UIBase target)
        {
            if (target == null)
            {
                return;
            }

            while (_uiStack.Count > 0 && Current != target)
            {
                Pop();
            }
        }

        public void Clear()
        {
            while (_uiStack.Count > 0)
            {
                Pop();
            }
        }

        public void SetActiveCanvas(bool isActive)
        {
            _canvas.gameObject.SetActive(isActive);
        }

        public bool Contains(UIBase ui)
        {
            return _uiStack.Contains(ui);
        }
    }
}

public enum UI_TYPE
{
    MAIN,
    POPUP,
    NONE
}
