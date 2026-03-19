using System.Collections;
using TMPro;
using UnityEngine;

namespace Shin
{
    public class GameClearUI : UIBase
    {
        [SerializeField]
        private TextMeshProUGUI _clearTimeText;

        [SerializeField]
        private TextMeshProUGUI _clearRankText;

        [SerializeField]
        private CanvasGroup _canvasGroup;
        private CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                {
                    _canvasGroup = GetComponent<CanvasGroup>();
                }
                return _canvasGroup;
            }
        }

        private bool _inputAble = false;

        private IEnumerator _clearSequenceCo;
        override protected void OnConfirmImpl()
        {
            base.OnConfirmImpl();

            if (InGameManager.Instance.PlayerInfo.Object.HasStateAuthority && _inputAble)
            {
                InGameManager.Instance.PlayerInfo.RpcGameClearInputCheck();
            }
        }

        public override void Show()
        {
            base.Show();
            Init();


            _clearSequenceCo = ClearSequenceCo();
            StartCoroutine(_clearSequenceCo);
        }

        private void Init()
        {
            if (_clearSequenceCo != null)
            {
                StopCoroutine(_clearSequenceCo);
            }
            _clearTimeText.SetText("");
            _clearRankText.SetText("");
            CanvasGroup.alpha = 0.0f;
            _inputAble = false;
        }

        private IEnumerator ClearSequenceCo()
        {
            var duration = 1.0f;
            var currentFadeTime = 0f;
            var clearTime = -1f;

            var playerInfo = InGameManager.Instance.PlayerInfo;

            InGameManager.Instance.PlayerInfo.OnGameClearTimeUpdate = (gameClearTime) =>
                {
                    clearTime = gameClearTime;
                };

            if (playerInfo.Object.HasStateAuthority)
            {
                playerInfo.RpcUpdateGameClearTime((Time.time - playerInfo.GameStartTime) % 60f);
            }



            yield return new WaitUntil(() => clearTime != -1f);

            while (currentFadeTime < duration)
            {
                currentFadeTime += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(currentFadeTime / duration);
                CanvasGroup.alpha = Mathf.Lerp(0.0f, 1.0f, u);
                yield return null;
            }

            var clearTimeDuration = 2f;
            var curCleartimeDuration = 0f;

            while (curCleartimeDuration < clearTimeDuration)
            {
                curCleartimeDuration += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(curCleartimeDuration / clearTimeDuration);
                float curClearTimeValue = Mathf.Lerp(0f, clearTime, t);
                _clearTimeText.SetText($"{curClearTimeValue:F1} Sec");
                yield return null;
            }

            _clearTimeText.SetText($"{clearTime:F1} Sec");

            yield return new WaitForSeconds(1.5f);

            //클리어랭크에 애니메이션 추가할지 고민
            _clearRankText.SetText(GetClearRank(clearTime));
            _inputAble = true;
        }

        private string GetClearRank(float clearTime)
        {
            switch (clearTime)
            {
                case <= 20f:
                    return "S";
                case <= 30f:
                    return "A";
                case <= 40f:
                    return "B";
                default:
                    return "F";
            }
        }
    }
}
