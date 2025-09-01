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
    
        private PlayerInput inputHandle;
        private Vector2 _moveInput;
    
        [SerializeField] private CharacterUnit _unit;
    
        public event UnityAction<Vector2> CallMove = delegate { };
    
        void OnMove(InputValue value)
        {
            var realValue = value.Get<Vector2>();
            _moveInput = realValue;
            _inGameManager.SetPlayerMoveVector(_moveInput);
        }

        void OnAttack(InputValue value)
        {
            _inGameManager.ActiveAttack();
        }

        void OnJump(InputValue value)
        {
            _inGameManager.ActiveJump();
        }

        void OnAbilityA(InputValue value)
        {
            _inGameManager.ActiveAbilityA();
        }

        void OnAbilityB(InputValue value)
        {
            _inGameManager.ActiveAbilityB();
        }
        
        void OnAbilityC(InputValue value)
        {
            _inGameManager.ActiveAbilityC();
        }
    }
}