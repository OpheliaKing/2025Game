using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  Shin
{
    public partial class CharacterUnit
    {
        protected bool _isAiState = false;

        /// <summary>
        /// 현재 AI로 움직이는지 리턴
        /// </summary>
        public bool IsAiState
        {
            get
            {
                return _isAiState;
            }
        }
        
        /// <summary>
        /// 명령을 위해 행동하는 값
        /// </summary>
        protected PublicVariable.CharacterAIState _characterAIState;
        
        /// <summary>
        /// AI가 명령받은 값
        /// </summary>
        protected PublicVariable.CharacterAICommandState _characterAICommandState;

        [SerializeField]
        protected CharacterUnit _targetUnit;

        [SerializeField]
        protected float _targetDistance;

        protected void AIStateInit()
        {
            _characterAIState = PublicVariable.CharacterAIState.STAND_BY;
            _characterAICommandState = PublicVariable.CharacterAICommandState.STAND_BY;
        }
        
        public void SetAiState(PublicVariable.CharacterAICommandState aistate)
        {
            _characterAICommandState = aistate;
        }

        public void SetTarget(CharacterUnit targetUnit, float targetDistance = 0)
        {
            _targetUnit = targetUnit;
            _targetDistance = targetDistance;
        }

        public void TestTaget()
        {
            if (_characterAICommandState == PublicVariable.CharacterAICommandState.FOLLOW)
            {
                SetAiState(PublicVariable.CharacterAICommandState.STAND_BY);
                _isAiState = false;
            }
            else
            {
                SetAiState(PublicVariable.CharacterAICommandState.FOLLOW);
                _isAiState = true;
            }
            Debug.Log($"Test Target On State == {_characterAICommandState}");
            UpdateAIState();
        }
        
        public void UpdateAIState()
        {
            // AI가 비활성화된 경우 업데이트하지 않음
            if (!_isAiState)
            {
                return;
            }

            switch (_characterAICommandState)
            {
                case PublicVariable.CharacterAICommandState.PATROL:
                    PatrolAction();
                    break;
                case PublicVariable.CharacterAICommandState.STAND_BY:
                    StandByAction();
                    break;
                case PublicVariable.CharacterAICommandState.FOLLOW:
                    if (_targetUnit == null)
                    {
                        SetAiState(PublicVariable.CharacterAICommandState.STAND_BY);
                        return;
                    }

                    FollowAction();
                    break;
            }
        }

        private void PatrolAction()
        {
            // 순찰 로직 구현 (현재는 대기 상태로 전환)
            SetAiState(PublicVariable.CharacterAICommandState.STAND_BY);
        }

        private void StandByAction()
        {
            // 대기 상태에서는 이동 중지
            Move(Vector2.zero);
        }

        private void FollowAction()
        {
            if (_targetUnit == null)
            {
                return;
            }
            
            var distance = Vector3.Distance(transform.position, _targetUnit.transform.position);

            if (_targetDistance < distance)
            {
                //x축 이동
                var targetPos = _targetUnit.transform.position;
                var moveVec = (targetPos - transform.position).x >0 ? Vector2.right : Vector2.left;
                Move(moveVec);
            }
            else
            {
                Move(Vector2.zero);
            }
        }
    }
}

