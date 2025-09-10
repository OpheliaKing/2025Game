using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shin;
using UnityEngine.Rendering;

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

        private Transform _tr;

        public Transform Tr
        {
            get
            {
                if (_tr == null)
                {
                    _tr = GetComponent<Transform>();
                }

                return _tr;
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
            AnimUnitInit();
            MoveUnitInit();
            AttackUnitInit();
            AIStateInit();
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
            UpdateAIState();
            Test();
        }

        private void Test()
        {
            if (photonView.IsMine)
            {
                if (Input.GetKey(KeyCode.T))
                {
                    var speed = UnityEngine.Random.Range(0.001f, 0.005f);
                    transform.position = new Vector3(Tr.position.x + speed, Tr.position.y, Tr.position.z);
                }

            }
        }

        //FSM

        public void SetCharacterState(PublicVariable.CharacterState state)
        {
            // 죽은 상태에서는 상태 변경 불가
            if (_characterState == PublicVariable.CharacterState.DIE)
            {
                return;
            }

            // 상태 전환 검증 및 수정
            switch (state)
            {
                case PublicVariable.CharacterState.IDLE:
                    if (!_isGrounded)
                    {
                        state = PublicVariable.CharacterState.AIR;
                    }
                    break;

                case PublicVariable.CharacterState.MOVE:
                    if (!_isGrounded)
                    {
                        state = PublicVariable.CharacterState.MOVE_AIR;
                    }
                    break;

                case PublicVariable.CharacterState.MOVE_AIR:
                    if (_isGrounded)
                    {
                        state = PublicVariable.CharacterState.MOVE;
                    }
                    break;
            }

            // 상태가 실제로 변경된 경우에만 애니메이션 변경
            if (_characterState != state)
            {
                AnimationChange(state);
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