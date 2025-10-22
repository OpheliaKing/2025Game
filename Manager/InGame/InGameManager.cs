using System;
using UnityEngine;

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

        private void Update()
        {
            //MoveInputUpdate();
        }

        public void StartGame(StageData data,Action onComplete = null)
        {
            //StageInit("Stage_0001",onComplete);            
        }

        public void StageInit(string stageTid,Action onComplete = null)
        {
            StageInfo.LoadMapPrefab(stageTid,onComplete);
            SpawnCharacter("PlayerBase");
        }

        private void SpawnCharacter(string characterTid)
        {
            PlayerInfo.LoadPlayerPrefab(characterTid);            
        }
    }
}