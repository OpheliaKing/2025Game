using System;
using System.Collections;
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
        public Transform Canvas
        {
            get
            {
                return _canvas;
            }
        }

        //추후 리소스 매니저에서 가져오는 방식으로 수정해야됨

        [SerializeField]
        private List<UIBase> _uiPrefabList;

        private List<UIBase> _cashedUI;

        private readonly Stack<UIBase> _uiStack = new Stack<UIBase>();

        [Header("InGameText Pool (Object Pooling)")]
        [SerializeField]
        private string _inGameTextPrefabName = "InGameTextUI";

        [SerializeField]
        private int _inGameTextPrewarmCount = 10;

        [SerializeField]
        private int _inGameTextMaxPoolSize = 50;

        private readonly Queue<InGameTextUI> _inGameTextPool = new Queue<InGameTextUI>();
        private readonly List<InGameTextUI> _inGameTextAll = new List<InGameTextUI>();

        // 인스턴스별로 "자동 반환" 코루틴을 하나만 유지하기 위함
        private readonly Dictionary<InGameTextUI, Coroutine> _inGameTextReleaseCoroutines = new Dictionary<InGameTextUI, Coroutine>();

        public UIBase Current => _uiStack.Count > 0 ? _uiStack.Peek() : null;

        public override void ManagerInit()
        {
            base.ManagerInit();
            if (_cashedUI == null)
            {
                _cashedUI = new List<UIBase>();
            }
            // 인게임 진입 전/후와 무관하게 풀을 즉시 프리워밍하도록 원복
            //PrewarmInGameTextPool();
        }

        public void PrewarmInGameTextPool()
        {
            if (_canvas == null) return;
            var reManager = GameManager.Instance.ResourceManager;
            if (reManager == null) return;

            // 너무 많이 만들지 않도록 방어
            int target = Mathf.Clamp(_inGameTextPrewarmCount, 0, _inGameTextMaxPoolSize);
            for (int i = 0; i < target; i++)
            {
                var ui = CreateInGameTextUI(reManager);
                if (ui == null) break;
                ui.Hide(); // UIBase.Hide: 비활성화
                _inGameTextPool.Enqueue(ui);
            }
        }

        private InGameTextUI CreateInGameTextUI(ResourceManager reManager)
        {
            if (reManager == null || _canvas == null) return null;

            // Resources 경로: Prefab/UI/InGameTextUI.prefab
            var ui = reManager.InstantiatePrefab<InGameTextUI>(_inGameTextPrefabName, _canvas, reManager.UIPrefabPath);
            if (ui == null) return null;

            // 이름은 Show/Pool 추적에 도움됨
            ui.name = _inGameTextPrefabName;
            return ui;
        }

        private InGameTextUI AcquireInGameTextUI()
        {
            if (_inGameTextPool.Count > 0)
            {
                return _inGameTextPool.Dequeue();
            }

            if (_inGameTextAll.Count < _inGameTextMaxPoolSize)
            {
                var reManager = GameManager.Instance.ResourceManager;
                var created = CreateInGameTextUI(reManager);
                if (created == null) return null;
                _inGameTextAll.Add(created);
                return created;
            }

            // 풀 상한 도달: 남아있는 것 중 아무거나 재사용
            // (동시 표시량이 많으면 오래된 것부터 덮어쓰는 동작이 될 수 있음)
            return _inGameTextAll.Count > 0 ? _inGameTextAll[0] : null;
        }

        private void ReleaseInGameTextUI(InGameTextUI ui)
        {
            if (ui == null) return;
            ui.Hide();
            _inGameTextPool.Enqueue(ui);
        }

        private IEnumerator ReleaseInGameTextUIAfter(InGameTextUI ui, float duration)
        {
            yield return new WaitForSeconds(duration);
            ReleaseInGameTextUI(ui);

            // 코루틴 참조 해제
            if (ui != null && _inGameTextReleaseCoroutines.ContainsKey(ui))
            {
                _inGameTextReleaseCoroutines.Remove(ui);
            }
        }

        /// <summary>
        /// 월드 좌표 위치에 인게임 텍스트를 띄우고, 시간이 지나면 풀로 반환합니다.
        /// </summary>
        public InGameTextUI ShowInGameText(string text, Vector3 worldPosition, float duration = 1.0f, Camera worldCamera = null)
        {
            var ui = AcquireInGameTextUI();
            if (ui == null) return null;

            // 이미 해당 ui에 대해 예약된 "자동 반환"이 있으면 중복 실행 방지
            if (_inGameTextReleaseCoroutines.TryGetValue(ui, out var running))
            {
                StopCoroutine(running);
                _inGameTextReleaseCoroutines.Remove(ui);
            }

            ui.Show(text, worldPosition, worldCamera);

            if (duration > 0f)
            {
                _inGameTextReleaseCoroutines[ui] = StartCoroutine(ReleaseInGameTextUIAfter(ui, duration));
            }

            return ui;
        }

        /// <summary>
        /// 타겟 Transform을 따라가며 인게임 텍스트를 띄우고, 시간이 지나면 풀로 반환합니다.
        /// </summary>
        public InGameTextUI ShowInGameText(string text, Transform target, float duration = 1.0f, Camera worldCamera = null)
        {
            var ui = AcquireInGameTextUI();
            if (ui == null) return null;

            if (_inGameTextReleaseCoroutines.TryGetValue(ui, out var running))
            {
                StopCoroutine(running);
                _inGameTextReleaseCoroutines.Remove(ui);
            }

            ui.Show(text, target, worldCamera);

            if (duration > 0f)
            {
                _inGameTextReleaseCoroutines[ui] = StartCoroutine(ReleaseInGameTextUIAfter(ui, duration));
            }

            return ui;
        }

        /// <summary>
        /// 특정 InGameTextUI를 즉시 풀로 반환합니다.
        /// </summary>
        public void HideInGameText(InGameTextUI ui)
        {
            if (ui == null) return;

            if (_inGameTextReleaseCoroutines.TryGetValue(ui, out var running))
            {
                StopCoroutine(running);
                _inGameTextReleaseCoroutines.Remove(ui);
            }

            ReleaseInGameTextUI(ui);
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
