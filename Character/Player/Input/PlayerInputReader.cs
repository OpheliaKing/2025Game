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
        [SerializeField]
        private InGameManager _inGameManager;
    
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
            var realValue = value.Get<Vector2>();
            _moveInput = realValue;

            Debug.Log($"Current :{PlayerInput.currentActionMap}");


            if (_inGameManager == null)
            {
                return;
            }            

            _inGameManager.SetPlayerMoveVector(_moveInput);
        }

        void OnAttack(InputValue value)
        {
             if (_inGameManager == null)
            {
                return;
            }
            _inGameManager.ActiveAttack();
        }

        void OnJump(InputValue value)
        {
            if (_inGameManager == null)
            {
                return;
            }
            
            _inGameManager.ActiveJump();
        }

        void OnAbilityA(InputValue value)
        {
             if (_inGameManager == null)
            {
                return;
            }
            _inGameManager.ActiveAbilityA();
        }

        void OnAbilityB(InputValue value)
        {
            if (_inGameManager == null)
            {
                return;
            }
            _inGameManager.ActiveAbilityB();
        }
        
        void OnAbilityC(InputValue value)
        {
                        
         if (_inGameManager == null)
            {
                return;
            }
            
            _inGameManager.ActiveAbilityC();
        }
    }
}