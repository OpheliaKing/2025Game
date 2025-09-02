using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shin
{
    public class InputManager : ManagerBase
    {
        private PlayerInput _playerInput;
        public PlayerInput PlayerInput
        {
            get
            {
                if (_playerInput == null)
                {
                    _playerInput = GetComponentInChildren<PlayerInput>();
                }
                return _playerInput;
            }
        }

        private INPUT_MODE _inputMode;
        public INPUT_MODE InputMode
        {
            get { return _inputMode; }
        }

        public override void ManagerInit()
        {
            base.ManagerInit();

            SetInputMode(INPUT_MODE.UISelect);
        }


        public void SetInputMode(INPUT_MODE mode)
        {
            _inputMode = mode;
            UpdateInputMode();
        }

        public void UpdateInputMode()
        {
            PlayerInput.SwitchCurrentActionMap(_inputMode.ToString());
        }
        
    }

    public enum INPUT_MODE
    {
        None,
        Player,
        UISelect,
    }

}

