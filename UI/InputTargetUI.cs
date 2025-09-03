using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
	public class InputTargetUI : MonoBehaviour
	{
		[SerializeField]
		private List<InputTargetButton> _buttons = new List<InputTargetButton>();

		[SerializeField]
		private int _selectedIndex = -1;

		public int SelectedIndex => _selectedIndex;
		public InputTargetButton SelectedButton =>
			_selectedIndex >= 0 && _selectedIndex < _buttons.Count ? _buttons[_selectedIndex] : null;

		public event Action Up;
		public event Action Down;
		public event Action Left;
		public event Action Right;
		public event Action Confirm;
		public event Action Cancel;

		public void InvokeUp() { Up?.Invoke(); MoveSelection(-1); }
		public void InvokeDown() { Down?.Invoke(); MoveSelection(1); }
		public void InvokeLeft() { Left?.Invoke(); MoveSelection(-1); }
		public void InvokeRight() { Right?.Invoke(); MoveSelection(1); }
		public void InvokeConfirm() { Confirm?.Invoke(); SelectedButton?.InvokeConfirm(); }
		public void InvokeCancel() { Cancel?.Invoke(); }

		public void SetButtons(List<InputTargetButton> buttons)
		{
			_buttons = buttons ?? new List<InputTargetButton>();
			ClampAndApplySelection(_selectedIndex < 0 && _buttons.Count > 0 ? 0 : _selectedIndex);
		}

		public void SetSelectedIndex(int index)
		{
			ClampAndApplySelection(index);
		}

		public void ResetSelection()
		{
			if (_buttons != null)
			{
				for (int i = 0; i < _buttons.Count; i++)
				{
					_buttons[i]?.SetSelected(false);
				}
			}
			_selectedIndex = -1;
		}

		private void MoveSelection(int delta)
		{
			if (_buttons == null || _buttons.Count == 0)
            {
                return;
            }

			int next = _selectedIndex;
			if (next < 0)
			{
				next = 0;
			}
			else
			{
				next += delta;
			}

			next = WrapIndex(next);
			ApplySelection(next);
		}

		private void ClampAndApplySelection(int index)
		{
			if (_buttons == null || _buttons.Count == 0)
			{
				_selectedIndex = -1;
				return;
			}
			index = WrapIndex(index);
			ApplySelection(index);
            
		}

		private int WrapIndex(int index)
		{
			int count = _buttons.Count;
			if (count <= 0)
			{
				return -1;
			}
			int wrapped = index % count;
			if (wrapped < 0) wrapped += count;
			return wrapped;
		}

		private void ApplySelection(int newIndex)
		{
			if (newIndex == _selectedIndex)
            {
                return;
            }

			if (_selectedIndex >= 0 && _selectedIndex < _buttons.Count)
			{
				_buttons[_selectedIndex]?.SetSelected(false);
			}

			_selectedIndex = newIndex;

			if (_selectedIndex >= 0 && _selectedIndex < _buttons.Count)
			{
				_buttons[_selectedIndex]?.SetSelected(true);
			}
		}
	}
}
