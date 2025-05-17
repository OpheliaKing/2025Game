using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shin;

namespace Shin
{
    public partial class CharacterUnit : CharacterBase
    {
        private Rigidbody2D _rb;

        protected Rigidbody2D Rb
        {
            get
            {
                if (_rb == null)
                {
                    _rb = GetComponent<Rigidbody2D>();
                }

                return _rb;
            }
        }


        //State
        [SerializeField] private PublicVariable.CharacterState _characterState;

        public PublicVariable.CharacterState CharacterState
        {
            get { return _characterState; }
        }


        //Ground

        private GroundChecker _groundChecker;

        public GroundChecker GroundChecker
        {
            get
            {
                if (_groundChecker == null)
                {
                    _groundChecker = GetComponentInChildren<GroundChecker>();
                }

                return _groundChecker;
            }
        }

        [SerializeField] private bool _isGrounded;

        public bool IsGrounded
        {
            get { return _isGrounded; }
        }

        /// <summary>
        /// True일시 False => True 로 변경된 상태
        /// </summary>
        public Action<bool> OnChangeGroundState;

        private void Awake()
        {
            CharacterInit();
        }

        #region Init

        private void CharacterInit()
        {
            EventInit();

            MoveUnitInit();
        }

        private void EventInit()
        {
            GroundChecker.OnGroundEvent = UpdateCheckGround;
            GroundChecker.OnOutGroundEvent = UpdateCheckGround;
        }

        #endregion


        private void Update()
        {
            MoveUnitUpdate();
        }

        //FSM

        public void SetCharacterState(PublicVariable.CharacterState state)
        {
            //값이 오면 상황에 맞게 변경해주는 기능 필요

            if (_characterState == PublicVariable.CharacterState.DIE)
            {
                return;
            }

            switch (state)
            {
                case PublicVariable.CharacterState.IDLE:
                    if (!_isGrounded)
                    {
                        state = PublicVariable.CharacterState.AIR;
                    }
                    break;

                case PublicVariable.CharacterState.MOVE_AIR:
                    if (_isGrounded)
                    {
                        state = PublicVariable.CharacterState.MOVE;
                    }

                    break;
            }

            _characterState = state;
        }

        /// <summary>
        /// 캐릭터가 공중에 있는 상태(점프중 이동, 떨어짐, 점프)를 리턴
        /// </summary>
        /// <returns></returns>
        public bool IsCharacterAirState()
        {
            return !_isGrounded;
        }

        private void UpdateCheckGround(bool isGround)
        {
            if (_isGrounded != isGround)
            {
                OnChangeGroundState?.Invoke(isGround);
            }

            _isGrounded = isGround;
        }
    }
}