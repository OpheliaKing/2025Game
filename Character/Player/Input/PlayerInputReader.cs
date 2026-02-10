using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Shin
{
    public class PlayerInputReader : MonoBehaviour
    {

        private UIManager _uiManager;

        private UIManager UIManager
        {
            get
            {
                if (!GameManager.Instance.IsInGame)
                {
                    return GameManager.Instance.UImanager;
                }
                else
                {
                    return InGameManager.Instance.InGameUIManager;
                }
            }
        }

        private InputManager _inputManager;

        private InputManager InputManager
        {
            get
            {
                if (_inputManager == null)
                {
                    _inputManager = GameManager.Instance.InputManager;
                }

                return _inputManager;
            }
        }

        private InGameManager _inGameManager;

        public InGameManager InGameManager
        {
            get
            {
                if (_inGameManager == null)
                {
                    _inGameManager = InGameManager.Instance;
                }
                return _inGameManager;
            }
        }

        private PlayerInput _inputHandle;

        public PlayerInput PlayerInput
        {
            get
            {
                if (_inputHandle == null)
                {
                    _inputHandle = GetComponent<PlayerInput>();
                }

                return _inputHandle;
            }

        }

        private Vector2 _moveInput;

        [SerializeField] private CharacterUnit _unit;

        public event UnityAction<Vector2> CallMove = delegate { };




        void OnMove(InputValue value)
        {
            Debug.Log("Move Test 1");
            var realValue = value.Get<Vector2>();
            _moveInput = realValue;

            switch (InputManager.InputMode)
            {
                case INPUT_MODE.Player:
                    if (InGameManager == null)
                    {
                        return;
                    }

                    if (InGameManager.PlayerInfo == null)
                    {
                        return;
                    }

                    Debug.Log("Move Test 3");
                    InGameManager.PlayerInfo.SetPlayerMoveVector(_moveInput);
                    break;
                case INPUT_MODE.UISelect:

                    if (_moveInput.x == 0)
                    {
                        break;
                    }
                    if (_moveInput.x > 0)
                    {
                        UIManager.Current.OnRight();
                    }
                    else
                    {
                        UIManager.Current.OnLeft();
                    }

                    break;
            }
        }

        #region Player Input

        public bool CheckPlayerInputAble()
        {
            if (InGameManager == null || InputManager.InputMode != INPUT_MODE.Player || !InGameManager.PlayerInfo.IsMyCharacter)
            {
                return false;
            }
            return true;
        }

        void OnAttack(InputValue value)
        {
            if (!CheckPlayerInputAble())
            {
                return;
            }
            InGameManager.PlayerInfo.ActiveInteraction();
        }

        void OnJump(InputValue value)
        {
            if (!CheckPlayerInputAble())
            {
                return;
            }

            InGameManager.PlayerInfo.ActiveJump();
        }

        void OnAbilityA(InputValue value)
        {
            if (!CheckPlayerInputAble())
            {
                return;
            }
            InGameManager.PlayerInfo.ActiveAbilityA();
        }

        void OnAbilityB(InputValue value)
        {
            if (!CheckPlayerInputAble())
            {
                return;
            }
            InGameManager.PlayerInfo.ActiveAbilityB();
        }

        void OnAbilityC(InputValue value)
        {

            if (!CheckPlayerInputAble())
            {
                return;
            }

            InGameManager.PlayerInfo.ActiveAbilityC();
        }
        #endregion

        #region UISelect

        public bool CheckUIInputAble()
        {
            if (InputManager.InputMode != INPUT_MODE.UISelect)
            {
                return false;
            }
            return true;
        }

        void OnSelect(InputValue value)
        {
            if (!CheckUIInputAble())
            {
                return;
            }

            UIManager.Current.OnConfirm();
        }

        void OnCancel(InputValue value)
        {

            if (!CheckUIInputAble())
            {
                return;
            }

            UIManager.Current.OnCancel();
        }


        #endregion
    }
}