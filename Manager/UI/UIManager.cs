using System.Collections.Generic;
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

        public void ShowUI(string uiName)
        {
            if (string.IsNullOrEmpty(uiName))
            {
                Debug.LogError("ShowUI called with null or empty uiName");
                return;
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
                    return;
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
        }

        public void Push(UIBase ui)
        {
            if (ui == null)
            {
                return;
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
