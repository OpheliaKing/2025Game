using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.AI;

namespace Shin
{
    public class PlayerEmotion : MonoBehaviour
    {
        private SpriteRenderer _sprite;
        public SpriteRenderer Sprite
        {
            get
            {
                if (_sprite == null)
                {
                    _sprite = GetComponent<SpriteRenderer>();
                }
                return _sprite;
            }
        }

        public SerializableDictionary<EMOETION_TYPE, Sprite> EmotionSprites;

        private Animator _anim;
        public Animator Anim
        {
            get
            {
                if (_anim == null)
                {
                    _anim = GetComponent<Animator>();
                }
                return _anim;
            }
        }

        private IEnumerator _showCo;

        [SerializeField]
        private float _showTime = 3f;

        public void ShowEmotion(EMOETION_TYPE emotionType)
        {
            if (EmotionSprites.ContainsKey(emotionType))
            {
                gameObject.SetActive(true);
                Sprite.sprite = EmotionSprites[emotionType];
                if (_showCo != null)
                {
                    StopCoroutine(_showCo);
                }
                _showCo = ShowCo();
                StartCoroutine(_showCo);
            }
            else
            {
                Debug.LogError("Emotion sprite not found for type: " + emotionType);
            }
        }

        private IEnumerator ShowCo()
        {
            Anim.Rebind();    
            Anim.Play("Object_Emotion_Show");

            yield return new WaitForSeconds(_showTime);
            gameObject.SetActive(false);
            _showCo = null;
        }
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