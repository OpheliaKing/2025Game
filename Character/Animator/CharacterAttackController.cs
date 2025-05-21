using System;
using System.Collections;
using System.Collections.Generic;
using Shin;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;

public class CharacterAttackController : StateMachineBehaviour
{
    private CharacterUnit _master;

    public int AttackIndex;
    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        if (_master == null)
        {
            _master =  animator.GetComponentInParent<CharacterUnit>();
        }

        if (_master != null)
        {
            _master.AttackAnimationStart(AttackIndex);
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        if (_master != null)
        {
            _master.AttackAnimationEnd(AttackIndex);
        }
    }
}
