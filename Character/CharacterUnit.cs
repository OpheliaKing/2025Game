using System;
using Fusion;
using UnityEngine;

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

        [Networked]
        public string MasterPlayerId { get; set; }

        [SerializeField]
        public string _testMasterPlayerId;

        /// <summary>
        /// MasterPlayerId가 변경될 때 호출되는 콜백
        /// 네트워크를 통해 동기화된 값이 변경될 때 모든 클라이언트에서 호출됩니다.
        /// </summary>
        private void MasterPlayerIdChanged()
        {
            Debug.Log($"[CharacterUnit] MasterPlayerId 변경됨: {MasterPlayerId}, Object: {gameObject.name}, HasInputAuthority: {Object.HasInputAuthority}");
        }

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


        private void FixedUpdate()
        {
            MoveUnitUpdate();
            UpdateAIState();
            UpdateName();
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
                RpcAnimationChange(state);
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
            Debug.Log($"Ground Check ::: Is Ground : {isGround}");

            if (_isGrounded != isGround)
            {
                Debug.Log($"Ground Check ::: Is Ground Invoke : {isGround}");

                OnChangeGroundState?.Invoke(isGround);
            }

            _isGrounded = isGround;
        }

        public void UpdateName()
        {
            if (Object.HasStateAuthority)
            {
                Debug.Log($"Object Name ::: {gameObject.name} UpdateName ::: HasStateAuthority: {Object.HasStateAuthority}");
                MasterPlayerId = _testMasterPlayerId;
            }
            else
            {
                Debug.Log($"Object Name ::: {gameObject.name} UpdateName ::: No HasStateAuthority : {Object.HasStateAuthority}");
            }
        }
    }
}