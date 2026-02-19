using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shin
{
    public class InGameManager : SingletonObject<InGameManager>
    {
        [SerializeField]
        private CharacterUnit _playerUnit;

        public CharacterUnit PlayerUnit
        {
            get { return _playerUnit; }
        }

        private InGamePlayerInfo _playerInfo;

        public InGamePlayerInfo PlayerInfo
        {
            get
            {
                if (_playerInfo == null)
                {
                    _playerInfo = FindObjectOfType<InGamePlayerInfo>();
                }
                return _playerInfo;
            }
        }

        private InGameStageInfo _stageInfo;

        public InGameStageInfo StageInfo
        {
            get
            {
                if (_stageInfo == null)
                {
                    _stageInfo = FindObjectOfType<InGameStageInfo>();
                }
                return _stageInfo;
            }
        }


        private UIManager _inGameUIManager;
        public UIManager InGameUIManager
        {
            get
            {
                if (_inGameUIManager == null || !_inGameUIManager.gameObject.activeSelf)
                {
                    _inGameUIManager = GameObject.Find("InGameUIManager").GetComponent<UIManager>();
                }
                return _inGameUIManager;
            }
        }

        public void StartGame(StageData data, Action onComplete = null)
        {
            //StageInit("Stage_0001",onComplete);            
        }

        public void StageInit(string stageTid, Action<StageInfo> onComplete = null)
        {
            Debug.Log($"StageInfo: {StageInfo}\n stageTid: {stageTid}");

            StartCoroutine(StageInitCO(stageTid, onComplete));
        }

        private IEnumerator StageInitCO(string stageTid, Action<StageInfo> onComplete = null)
        {
            yield return new WaitUntil(() => StageInfo != null);

            StageInfo.LoadMapPrefab(stageTid, onComplete);
            SpawnCharacter("PlayerBase");
        }

        private void SpawnCharacter(string characterTid)
        {
            PlayerInfo.LoadPlayerPrefab(characterTid);
        }

        public void GameClear()
        {
            Debug.Log("Test Game Clear");
            GameManager.Instance.InputManager.SetInputMode(INPUT_MODE.UISelect);
            InGameUIManager.ShowUI("GameClearUI");
        }
    }
}