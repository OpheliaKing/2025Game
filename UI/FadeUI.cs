using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Shin
{
    public class FadeUI : UIBase
    {
        private Animator _animator;
        private Coroutine _fadeCoroutine;

        private Animator Animator
        {
            get
            {
                if (_animator == null)
                {
                    _animator = GetComponent<Animator>();
                }
                return _animator;
            }
        }

        private CanvasGroup _canvasGroup;
        private CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                {
                    _canvasGroup = GetComponent<CanvasGroup>();
                    if (_canvasGroup == null)
                    {
                        _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                    }
                }
                
                return _canvasGroup;
            }
        }

        public void FadeIn(float duration, Action onComplete = null)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            _fadeCoroutine = StartCoroutine(FadeToCo(0f, duration, onComplete));
        }

        public void FadeOut(float duration, Action onComplete = null)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            _fadeCoroutine = StartCoroutine(FadeToCo(1f, duration, onComplete));
        }

        private IEnumerator FadeToCo(float targetAlpha, float duration, Action onComplete = null)
        {
            float startAlpha = CanvasGroup.alpha;

            if (duration <= 0f)
            {
                CanvasGroup.alpha = targetAlpha;
                CanvasGroup.blocksRaycasts = targetAlpha > 0f;
                CanvasGroup.interactable = targetAlpha > 0f;

                _fadeCoroutine = null;
                onComplete?.Invoke();
                yield break;
            }

            CanvasGroup.blocksRaycasts = true;
            CanvasGroup.interactable = false;

            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / duration);
                CanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, u);
                yield return null;
            }

            CanvasGroup.alpha = targetAlpha;
            CanvasGroup.blocksRaycasts = targetAlpha > 0f;
            CanvasGroup.interactable = targetAlpha > 0f;

            _fadeCoroutine = null;
            onComplete?.Invoke();
        }

        private void OnDisable()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
        }
    }
}
