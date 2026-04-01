using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
    public class PlayerEmotion : MonoBehaviour
    {
        private SpriteRenderer _sprite;
    }
}


public enum EMOETION_TYPE
{
    SILENCE,
    LOVE,
    QUESTION,
    SURPRISED,
    ANGRY,
    ERROR,
    AGREE,
}