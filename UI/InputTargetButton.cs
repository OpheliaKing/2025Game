using System;
using UnityEngine;

namespace Shin
{
	public class InputTargetButton : MonoBehaviour
	{
		[SerializeField]
		private bool _isSelected;

		public bool IsSelected => _isSelected;

		public event Action Confirm;

		//UI
		[SerializeField]
		private GameObject _selectUI;

		public void SetSelected(bool selected)
		{
			_isSelected = selected;
			OnSelectionChanged(selected);
		}

		protected virtual void OnSelectionChanged(bool selected)
		{
			// 구현체에서 비주얼 갱신
			if (_selectUI != null)
			{
				_selectUI.SetActive(selected);
			}
		}

		public void InvokeConfirm()
		{
			Confirm?.Invoke();
		}
	}
}