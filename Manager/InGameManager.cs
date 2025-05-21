using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Shin
{
    public class InGameManager : MonoBehaviour
    {
        [SerializeField]
        private CharacterUnit _playerUnit;

        public CharacterUnit PlayerUnit
        {
            get { return _playerUnit; }
        }

        private Vector2 _moveInput;


        #region  Input

        public void SetPlayerMoveVector(Vector2 vector)
        {
            
            Debug.Log($"Test Inpuit {vector}");
            _moveInput = vector;
        }

        public void ActiveAttack()
        {
            PlayerUnit.ActiveAttack();
        }

        public void ActiveJump()
        {
            PlayerUnit.ActiveJump();
        }

        #endregion


        private void Update()
        {
            MoveInputUpdate();
        }

        private void MoveInputUpdate()
        {
            if (PlayerUnit == null)
            {
                return;
            }
        
            PlayerUnit.Move(_moveInput);
        }
    }
}