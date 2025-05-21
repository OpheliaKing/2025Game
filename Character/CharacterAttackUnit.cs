using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
    public partial class CharacterUnit
    {
        private int _currentAttackIndex;
        private int _maxAttackIndex = 0;
        
        
        //해당 값은 테스트 용으로 가져옴 => 추후 테이블 등에서 공격의 최대 인덱스를 가져와야됨
        private int _testAttackMaxCount = 3;

        protected bool _isAttack;
        protected bool _ableToMoveAttack = false;

        //공격으로 인해 moveAble 스택이 올라갔는지 확인용
        private int _moveAbleStackToAttack = 0;
        

        private void AttackUnitInit()
        {
            _maxAttackIndex = _testAttackMaxCount;
        }
        
        //예외처리 등등 추가해야됨
        public void ActiveAttack()
        {
            //어택 이름 받아올수 있도록 수정
            //아래 스크립트는 테스트 용
            
            if (_currentAttackIndex >= _maxAttackIndex)
            {
                return;
            }
            //공격 호출시

            //아래에 있는 조건 및 벨로시트 초기화 등은 함수등으로 묶어서 관리
            
            Rb.velocity = new Vector2(0, Rb.velocity.y);
            
            if (!_ableToMoveAttack && _moveAbleStackToAttack == 0)
            {
                _moveAbleStackToAttack++;
                AddMoveAbleStack();
            }

            if (_currentAttackIndex == 0)
            {
                PlayAnim($"Red_Attack_0{_currentAttackIndex +1}");
            }
            else
            {
                PlayBoolAnim($"IsAttack",true);
            }
            
            _currentAttackIndex++;
            SetCharacterState(PublicVariable.CharacterState.ATTACK);
        }

        public void AttackAnimationStart(int index)
        {
            if (_currentAttackIndex != index)
            {
                return;
            }
            PlayBoolAnim($"IsAttack",false);
        }
        
        public void AttackAnimationEnd(int index)
        {
            if (index != _currentAttackIndex)
            {
                return;
            }

            AttackEnd();
        }

        private void AttackEnd()
        {
            PlayBoolAnim($"IsAttack",false);
            _currentAttackIndex = 0;
            
            if (_moveAbleStackToAttack > 0)
            {
                _moveAbleStackToAttack = 0;
                RemoveMoveAbleStack();
            }
        }
    }
}