using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Photon.Pun;
using Photon.Realtime;

namespace Shin
{
    public class InGamePlayerInfo : MonoBehaviourPunCallbacks
    {
        [SerializeField]
        private CharacterUnit _playerUnit;

        public CharacterUnit PlayerUnit
        {
            get { return _playerUnit; }
        }

        [Header("Stage & Character Settings")]
        [SerializeField]
        private string _selectCharacterTid;

        // 동기화 관련 필드들
        [Header("Game Sync Data")]
        [SerializeField]
        private GameSyncData _localGameData = new GameSyncData();

        private Dictionary<string, GameSyncData> _playerGameData = new Dictionary<string, GameSyncData>();



        // 이벤트
        public System.Action<GameSyncData> OnGameDataReceived;
        public System.Action<string, GameSyncData> OnPlayerDataUpdated;

        private void Update()
        {
            //MoveInputUpdate();
        }

        #region Game Data Sync Methods

        /// <summary>
        /// 로컬 게임 데이터를 설정하고 다른 플레이어들에게 동기화
        /// </summary>
        public void SetLocalGameData(StageData stageData, CharacterData characterData)
        {
            _localGameData.selectedStage = stageData;
            _localGameData.selectedCharacter = characterData;
            _localGameData.playerId = PhotonNetwork.LocalPlayer.UserId;
            _localGameData.playerNickname = PhotonNetwork.LocalPlayer.NickName;
            _localGameData.timestamp = Time.time;

            // RPC로 다른 플레이어들에게 데이터 전송
            photonView.RPC("ReceiveGameData", RpcTarget.Others, JsonUtility.ToJson(_localGameData));

            Debug.Log($"로컬 게임 데이터 설정 완료 - 스테이지: {stageData.stageName}, 캐릭터: {characterData.characterName}");
        }

        /// <summary>
        /// 다른 플레이어로부터 게임 데이터를 받는 RPC 메서드
        /// </summary>
        [PunRPC]
        private void ReceiveGameData(string gameDataJson)
        {
            try
            {
                GameSyncData receivedData = JsonUtility.FromJson<GameSyncData>(gameDataJson);
                // 플레이어 데이터 딕셔너리에 저장
                _playerGameData[receivedData.playerId] = receivedData;

                // 이벤트 호출
                OnGameDataReceived?.Invoke(receivedData);
                OnPlayerDataUpdated?.Invoke(receivedData.playerId, receivedData);

                Debug.Log($"게임 데이터 수신 완료 - 플레이어: {receivedData.playerNickname}, 스테이지: {receivedData.selectedStage.stageName}, 캐릭터: {receivedData.selectedCharacter.characterName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"게임 데이터 파싱 오류: {e.Message}");
            }
        }

        /// <summary>
        /// 특정 플레이어의 게임 데이터를 가져오기
        /// </summary>
        public GameSyncData GetPlayerGameData(string playerId)
        {
            return _playerGameData.ContainsKey(playerId) ? _playerGameData[playerId] : null;
        }

        /// <summary>
        /// 모든 플레이어의 게임 데이터를 가져오기
        /// </summary>
        public Dictionary<string, GameSyncData> GetAllPlayerGameData()
        {
            return new Dictionary<string, GameSyncData>(_playerGameData);
        }

        /// <summary>
        /// 로컬 게임 데이터를 가져오기
        /// </summary>
        public GameSyncData GetLocalGameData()
        {
            return _localGameData;
        }

        /// <summary>
        /// 방에 입장했을 때 기존 플레이어들의 데이터 요청
        /// </summary>
        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            // 기존 플레이어들에게 데이터 요청
            if (PhotonNetwork.PlayerList.Length > 1)
            {
                photonView.RPC("RequestGameData", RpcTarget.Others);
            }
        }

        /// <summary>
        /// 다른 플레이어가 방에 입장했을 때 데이터 요청을 받는 RPC
        /// </summary>
        [PunRPC]
        private void RequestGameData()
        {
            // 요청한 플레이어에게 현재 데이터 전송
            if (_localGameData.selectedStage.stageId != "" && _localGameData.selectedCharacter.characterId != "")
            {
                photonView.RPC("ReceiveGameData", RpcTarget.Others, JsonUtility.ToJson(_localGameData));
            }
        }

        /// <summary>
        /// 플레이어가 방을 떠났을 때 해당 플레이어의 데이터 제거
        /// </summary>
        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);

            if (_playerGameData.ContainsKey(otherPlayer.UserId))
            {
                _playerGameData.Remove(otherPlayer.UserId);
                Debug.Log($"플레이어 {otherPlayer.NickName}의 게임 데이터 제거됨");
            }
        }


        #endregion

        public void StartGame()
        {
            var stageData = new StageData("test", "test", 0, "test");
            //StageInit(stageData);
        }

        /// <summary>
        /// StageDataSO에서 stageId로 스테이지 데이터 설정 및 동기화
        /// </summary>
        /// <param name="stageDataSO">StageDataSO 참조</param>
        /// <param name="stageId">스테이지 ID</param>
        /// <param name="characterData">캐릭터 데이터</param>
        public void SetStageDataFromSO(StageDataSO stageDataSO, string stageId, CharacterData characterData)
        {
            if (stageDataSO == null)
            {
                Debug.LogError("StageDataSO가 null입니다.");
                return;
            }

            StageData stageData = stageDataSO.FindStageDataForSyncById(stageId);
            if (stageData == null)
            {
                Debug.LogError($"스테이지를 찾을 수 없습니다: {stageId}");
                return;
            }

            SetLocalGameData(stageData, characterData);
        }

        /// <summary>
        /// StageDataSO에서 인덱스로 스테이지 데이터 설정 및 동기화
        /// </summary>
        /// <param name="stageDataSO">StageDataSO 참조</param>
        /// <param name="stageIndex">스테이지 인덱스</param>
        /// <param name="characterData">캐릭터 데이터</param>
        public void SetStageDataFromSOByIndex(StageDataSO stageDataSO, int stageIndex, CharacterData characterData)
        {
            if (stageDataSO == null)
            {
                Debug.LogError("StageDataSO가 null입니다.");
                return;
            }

            StageDataItem stageItem = stageDataSO.GetStageByIndex(stageIndex);
            if (stageItem == null)
            {
                Debug.LogError($"스테이지 인덱스가 범위를 벗어났습니다: {stageIndex}");
                return;
            }

            StageData stageData = new StageData(
                stageItem.stageData.stageId,
                stageItem.stageData.stageName,
                stageItem.stageData.stageLevel,
                stageItem.stageData.stageDescription
            );

            SetLocalGameData(stageData, characterData);
        }

        /// <summary>
        /// 캐릭터 유닛 초기화 (기존 메서드 오버로드)
        /// </summary>
        private void CharacterUnitInit()
        {
            // 기본 캐릭터 초기화
        }

        private void ProcessCharacterData(CharacterData characterData, string playerId)
        {
            if (string.IsNullOrEmpty(characterData.characterId))
                return;

            Debug.Log($"캐릭터 설정: {characterData.characterName} (레벨: {characterData.characterLevel})");

            // 캐릭터별 초기화 로직
            CharacterUnitInit(characterData, playerId);
        }

        /// <summary>
        /// 캐릭터 유닛 초기화 (데이터 기반)
        /// </summary>
        private void CharacterUnitInit(CharacterData characterData, string playerId)
        {
            _selectCharacterTid = characterData.characterId;

            // 로컬 플레이어인 경우에만 실제 캐릭터 유닛 설정
            if (playerId == PhotonNetwork.LocalPlayer.UserId && _playerUnit != null)
            {
                ApplyCharacterDataToUnit(_playerUnit, characterData);
            }

            // 캐릭터별 특수 능력 설정
            SetupCharacterAbilities(characterData);
        }

        /// <summary>
        /// 캐릭터 데이터를 유닛에 적용
        /// </summary>
        private void ApplyCharacterDataToUnit(CharacterUnit unit, CharacterData characterData)
        {
            // 캐릭터 스탯 적용
            // unit.SetHealth(characterData.health);
            // unit.SetMaxHealth(characterData.maxHealth);
            // unit.SetAttackPower(characterData.attackPower);
            // unit.SetDefense(characterData.defense);

            Debug.Log($"캐릭터 {characterData.characterName}의 스탯이 적용되었습니다.");
        }

        /// <summary>
        /// 캐릭터별 특수 능력 설정
        /// </summary>
        private void SetupCharacterAbilities(CharacterData characterData)
        {
            if (characterData.abilities == null || characterData.abilities.Length == 0)
                return;

            foreach (string ability in characterData.abilities)
            {
                Debug.Log($"캐릭터 {characterData.characterName}의 능력: {ability}");
                // 능력별 설정 로직
            }
        }

        private void LoadPlayerPrafab(string playerTid)
        {

        }

    }
}