using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
    public partial class CharacterUnit
    {
        private Animator _anim;

        public Animator Anim
        {
            get
            {
                if (_anim == null)
                {
                    _anim = GetComponentInChildren<Animator>();
                }
                
                return _anim;
            }
        }

        [SerializeField]
        private SpriteRenderer _modelSprite;

        public SpriteRenderer ModelSprite
        {
            get
            {
                return _modelSprite;
            }
        }

        private void AnimUnitInit()
        {
            OnChangeGroundState += UpdateGroundedAnimParamerters;
        }
        
        public void AnimationChange(PublicVariable.CharacterState state)
        {
            switch (state)
            {
                case PublicVariable.CharacterState.IDLE :
                    Anim.SetFloat("Move",0f);
                    break;
                case PublicVariable.CharacterState.MOVE :
                    Anim.SetFloat("Move",1f);
                    break;
                case PublicVariable.CharacterState.JUMP :
                    Anim.Play("Jump");
                    break;
            }
        }

        public void AnimationChange()
        {
            
        }

        public void UpdateGroundedAnimParamerters(bool isGrounded)
        {
            if (isGrounded && _isJump)
            {
                Anim.SetTrigger("GroundTrigger");
            }
        }
    }
}

