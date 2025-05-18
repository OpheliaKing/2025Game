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

        private bool _isJump;
        
        protected int _maxJumpCount = 2;
        protected int _currentJumpCount = 0;

        private int _horizontalLookVec = 0;
        
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

            SetHorizontalLookVector(horizontal);///// = horizontal;
        }

        public void SetHorizontalLookVector(float vec)
        {
            if (vec == 0)
            {
                return;
            }
            
            var look = vec > 0 ? 1 : -1;

            if (_horizontalLookVec == look)
            {
                return;
            }
            _horizontalLookVec = look;
            Tr.localScale = new Vector3(Mathf.Abs(Tr.localScale.x) * _horizontalLookVec, Tr.localScale.y, Tr.localScale.z);
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
            Rb.velocity = new Vector2(Rb.velocity.y, 0);
            
            Rb.AddForce(Vector2.up * _jumpSpeed, ForceMode2D.Impulse);

            _currentJumpCount++;
            _isJump = true;
            SetCharacterState(PublicVariable.CharacterState.JUMP);
        }

        protected void JumpEnd()
        {
            _currentJumpCount = 0;
            _isJump = false;
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



