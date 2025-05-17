using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
    public partial class CharacterUnit
    {
        protected float _moveSpeed = 1f;
        protected float _maxMoveSpeed = 5f;
        
        [SerializeField]
        protected float _jumpSpeed = 8f;

        protected int _maxJumpCount = 2;
        protected int _currentJumpCount = 0;

        private void MoveUnitInit()
        {
            OnChangeGroundState += (isGround) =>
            {
                if (isGround)
                {
                    JumpEnd();
                }
            };
        }
        
        public void Move(Vector2 vec)
        {
            var horizontal = vec.x;
            var vertical = vec.y;
    
            if (horizontal == 0)
            {
                Rb.velocity = new Vector2(horizontal, Rb.velocity.y);
                SetCharacterState(PublicVariable.CharacterState.IDLE);
                return;
            }
            
            Rb.AddForce(Vector2.right * (horizontal * _moveSpeed), ForceMode2D.Impulse);

            if (IsCharacterAirState())
            {
                SetCharacterState(PublicVariable.CharacterState.MOVE_AIR);
            }
            else
            {
                SetCharacterState(PublicVariable.CharacterState.MOVE);
            }
        }

        public void ActiveJump()
        {
            if (!CheckJumpAble())
            {
                return;
            }

            Jump();
        }

        protected bool CheckJumpAble()
        {
            if (_maxJumpCount == _currentJumpCount)
            {
                return false;
            }

            return true;
        }
        
        public void Jump()
        {
            Debug.Log("Jump");
            
            Rb.velocity = new Vector2(Rb.velocity.y, 0);
            
            Rb.AddForce(Vector2.up * _jumpSpeed, ForceMode2D.Impulse);

            _currentJumpCount++;
            SetCharacterState(PublicVariable.CharacterState.JUMP);
        }

        protected void JumpEnd()
        {
            _currentJumpCount = 0;
            SetCharacterState(PublicVariable.CharacterState.IDLE);
        }
        
        public void MoveUnitUpdate()
        {
            MoveSpeedControll();
        }
    
        private void MoveSpeedControll()
        {
            var moveSpeed = Mathf.Abs(Rb.velocity.x);
            if (moveSpeed > _maxMoveSpeed)
            {
                var vec = 1;
                
                if (Rb.velocity.x < 0)
                {
                    vec = -1;
                }
                
                Rb.velocity = new Vector2(_maxMoveSpeed * vec,_rb.velocity.y);
            }
        }
    }
}



