using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
    public partial class InGamePlayerInfo
    {
        private Vector2 _moveInput;

        public void SetPlayerMoveVector(Vector2 vector)
        {
            _moveInput = vector;
        }

        public void ActiveAttack()
        {
            if (PlayerUnit.IsAiState)
            {
                return;
            }
            PlayerUnit.ActiveAttack();
        }

        public void ActiveJump()
        {
            if (PlayerUnit.IsAiState)
            {
                return;
            }
            PlayerUnit.ActiveJump();
        }

        public void ActiveAbilityA()
        {
            PlayerUnit.ActiveAbilityA();
        }

        public void ActiveAbilityB()
        {
            PlayerUnit.ActiveAbilityB();
        }

        public void ActiveAbilityC()
        {
            PlayerUnit.ActiveAbilityC();
        }

        public void ActiveInteraction()
        {
            PlayerUnit.ActiveInteraction();
        }

        public void ActiveEmotion()
        {
            if (PlayerUnit == null)
            {
                Debug.Log("Player NULL");
                return;
            }
            PlayerUnit.PlayerEmotionUI.Show();
        }

        private void MoveInputUpdate()
        {
            if (PlayerUnit == null)
            {
                return;
            }

            if (PlayerUnit.IsAiState)
            {
                return;
            }

            PlayerUnit.Move(_moveInput);
        }
    }
}

