using System;
using UnityEngine;

namespace Shin
{
	public class UIBase : MonoBehaviour
	{
		[SerializeField]
		private bool _startsHidden = true;

		public bool IsVisible => gameObject.activeSelf;
		public bool IsFocused => _isFocused;
		private bool _isFocused;

        protected virtual void Awake()
        {
            if (_startsHidden)
            {
                Hide();
            }
		}

        protected virtual void Init(){ }

		public virtual void OnPush() { }
		public virtual void OnPop() { }
		public virtual void OnFocus() { _isFocused = true; }
		public virtual void OnUnfocus() { _isFocused = false; }

		// Input handlers: 4-direction, confirm, cancel (guarded by focus state)
		public void OnUp() { if (!IsFocused) return; OnUpImpl(); }
		public void OnDown() { if (!IsFocused) return; OnDownImpl(); }
		public void OnLeft() { if (!IsFocused) return; OnLeftImpl(); }
		public void OnRight() { if (!IsFocused) return; OnRightImpl(); }
		public void OnConfirm() { if (!IsFocused) return; OnConfirmImpl(); }
		public void OnCancel() { if (!IsFocused) return; OnCancelImpl(); }

		// Overridables
		protected virtual void OnUpImpl() { }
		protected virtual void OnDownImpl() { }
		protected virtual void OnLeftImpl() { }
		protected virtual void OnRightImpl() { }
		protected virtual void OnConfirmImpl() { }
		protected virtual void OnCancelImpl() { }

        public virtual void Show()
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            Init();
		}

		public virtual void Hide()
		{
			if (gameObject.activeSelf)
			{
				gameObject.SetActive(false);
			}
		}
	}
}

