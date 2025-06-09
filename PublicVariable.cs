using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
    public class PublicVariable
    {
        public enum CharacterState
        {
            IDLE,
            MOVE,
            JUMP,
            MOVE_AIR,
            AIR,
            ATTACK,
            DIE,
        }

        public enum CharacterAIState
        {
            CHASE,
            STAND_BY,
            ATTACK,
            MOVE,
        }

        public enum CharacterAICommandState
        {
            PATROL,//일정 위치를 순찰
            STAND_BY,//대기
            FOLLOW,
        }
    }
}

