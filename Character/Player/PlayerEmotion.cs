using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.PlayerLoop;

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

        public Dictionary<EMOETION_TYPE, Sprite> EmotionSprites = new Dictionary<EMOETION_TYPE, Sprite>();

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

        public void Init()
        {
            var reManager = GameManager.Instance.ResourceManager;
            var sprite = GameManager.Instance.ResourceManager.LoadSprites("Emotion_Sprite", reManager.SpritePath);

            var count = System.Enum.GetValues(typeof(EMOETION_TYPE)).Length;

            EmotionSprites.Clear();
            for (int i = 0; i < count; i++)
            {
                EmotionSprites.Add((EMOETION_TYPE)i, sprite.First(x=>x.name == ((EMOETION_TYPE)i).ToFileName()));
            }
        }

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