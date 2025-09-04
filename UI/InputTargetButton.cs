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

		[SerializeField]
		private BUTTON_CONFIRM_TYPE _confirm_Type;

		[SerializeField]
		private string _confirmValue;
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
			Debug.Log("Test 2");

			Confirm?.Invoke();

			switch (_confirm_Type)
			{
				case BUTTON_CONFIRM_TYPE.MOVE_SCENE:
					GameManager.Instance.UImanager.ShowUI(_confirmValue);
					break;
				case BUTTON_CONFIRM_TYPE.BACK_SCENE:
					GameManager.Instance.UImanager.Pop();
					break;
			}
		}
	}

	public enum BUTTON_CONFIRM_TYPE
	{
		NONE,
		MOVE_SCENE,
		BACK_SCENE,
	}
}