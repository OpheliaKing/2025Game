using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//해당 스크립트는 삭제

namespace Shin
{
    public class InGameManagerInput : MonoBehaviour
    {
        private Vector2 _moveInput;

        public void SetPlayerMoveVector(Vector2 vector)
        {
            _moveInput = vector;
        }

        // public void ActiveAttack()
        // {
        //     if (PlayerUnit.IsAiState)
        //     {
        //         return;
        //     }
        //     PlayerUnit.ActiveAttack();
        // }

        // public void ActiveJump()
        // {
        //     if (PlayerUnit.IsAiState)
        //     {
        //         return;
        //     }
        //     PlayerUnit.ActiveJump();
        // }

        // public void ActiveAbilityA()
        // {
        //     PlayerUnit.ActiveAbilityA();
        // }

        // public void ActiveAbilityB()
        // {
        //     PlayerUnit.ActiveAbilityB();
        // }

        // public void ActiveAbilityC()
        // {
        //     PlayerUnit.ActiveAbilityC();
        // }

        // private void MoveInputUpdate()
        // {
        //     if (PlayerUnit == null)
        //     {
        //         return;
        //     }

        //     if (PlayerUnit.IsAiState)
        //     {
        //         return;
        //     }

        //     PlayerUnit.Move(_moveInput);
        // }
    }
}

