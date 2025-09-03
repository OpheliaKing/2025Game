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


        private readonly Stack<UIBase> _uiStack = new Stack<UIBase>();

		public UIBase Current => _uiStack.Count > 0 ? _uiStack.Peek() : null;

		public override void ManagerInit()
		{
			base.ManagerInit();
		}

        public void ShowUI(string uiName)
        {
            var reManager = GameManager.Instance.ResourceManager;
            var uiPrefab = reManager.InstantiatePrefab<UIBase>(uiName,_canvas, reManager.UIPrefabPath);

            if (uiPrefab == null)
            {
                Debug.LogError($"Not Found Prefab!!! {uiName} ");
                return;
            }

            Push(uiPrefab);
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

		public bool Contains(UIBase ui)
		{
			return _uiStack.Contains(ui);
		}
	}
}
