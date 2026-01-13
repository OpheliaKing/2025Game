using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace Shin
{
    public partial class CharacterUnit
    {

        //Test

        /// <summary>
        /// 0이면 이동 가능 0보다 큰 숫자일 경우 이동 불가능
        /// </summary>
        private int _moveAbleStack = 0;

        public bool MoveAble
        {
            get
            {
                return _moveAbleStack == 0;
            }
        }
        
        protected float _moveSpeed = 1f;
        protected float _maxMoveSpeed = 5f;
        
        [SerializeField]
        protected float _jumpSpeed = 8f;

        private bool _isJump;
        
        protected int _maxJumpCount = 2;
        protected int _currentJumpCount = 0;

        private int _horizontalLookVec = 0;

        private float _maxFalldownSpeed = 10;
        
        private void MoveUnitInit()
        {
            OnChangeGroundState += (isGround) =>
            {
                if (isGround)
                {
                    JumpEnd();
                }
            };

            _moveAbleStack = 0;
        }

        public void AddMoveAbleStack()
        {
            _moveAbleStack++;
        }

        public void RemoveMoveAbleStack()
        {
            _moveAbleStack--;
        }
        
        public void Move(Vector2 vec)
        {
            if (!MoveAble)
            {
                return;
            }
            
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
            Rb.velocity = new Vector2(Rb.velocity.x, 0);
            
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
            FalldownSpeedControll();
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
                
                Rb.velocity = new Vector2(_maxMoveSpeed * vec, Rb.velocity.y);
            }
        }

        private void FalldownSpeedControll()
        {
            var falldownSpeed = Mathf.Abs(Rb.velocity.y);
            if (falldownSpeed > _maxFalldownSpeed)
            {
                var vec = 1;
                
                if (Rb.velocity.y < 0)
                {
                    vec = -1;
                }
                
                Rb.velocity = new Vector2(Rb.velocity.x,_maxFalldownSpeed * vec);
            }
        }
    }
}



