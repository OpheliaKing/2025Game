using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Shin
{
    //PhotonObject
    public partial class InGamePlayerInfo : NetworkBehaviour
    {
        [SerializeField]
        private CharacterUnit _playerUnit;

        public CharacterUnit PlayerUnit
        {
            get { return _playerUnit; }
        }

        public bool IsMyCharacter
        {
            get
            {
                return PlayerUnit.NetworkObject.HasInputAuthority;
            }
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
            MoveInputUpdate();
        }

        #region Game Data Sync Methods


        /// <summary>
        /// 다른 플레이어로부터 게임 데이터를 받는 RPC 메서드
        /// </summary>
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void ReceiveGameDataRPC(string gameDataJson)
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
        /// 방에 입장했을 때 기존 플레이어들의 데이터 요청
        /// </summary>
        // Fusion에서는 INetworkRunnerCallbacks.OnPlayerJoined 등에서 처리합니다.

        /// <summary>
        /// 다른 플레이어가 방에 입장했을 때 데이터 요청을 받는 RPC
        /// </summary>
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RequestGameDataRPC()
        {
            // 요청한 플레이어에게 현재 데이터 전송
            if (_localGameData.selectedStage.stageId != "" && _localGameData.selectedCharacter.characterId != "")
            {
                ReceiveGameDataRPC(JsonUtility.ToJson(_localGameData));
            }
        }

        /// <summary>
        /// 플레이어가 방을 떠났을 때 해당 플레이어의 데이터 제거
        /// </summary>
        // 플레이어 퇴장 처리는 INetworkRunnerCallbacks.OnPlayerLeft에서 PlayerRef 기반으로 구현하세요.


        #endregion

        /// <summary>
        /// 캐릭터 유닛 초기화 (기존 메서드 오버로드)
        /// </summary>
        private void CharacterUnitInit()
        {
            // 기본 캐릭터 초기화
        }
        public void LoadPlayerPrefab(string playerTid)
        {
            var resourceManager = GameManager.Instance.ResourceManager;
            var runner = FindObjectOfType<NetworkRunner>();
            if (runner == null || !runner.IsRunning)
            {
                Debug.LogWarning("NetworkRunner가 실행 중이 아닙니다. 스폰을 진행할 수 없습니다.");
                return;
            }

            // 리소스에서 프리팹 자산 조회 (존재 확인용)
            var obj = resourceManager.LoadPrefab<CharacterBase>(playerTid, resourceManager.CharacterPrefabPath);
            if (obj == null)
            {
                Debug.LogError($"캐릭터 프리팹 로드 실패: {playerTid}");
                return;
            }

            // Fusion 스폰 (프리팹은 NetworkObject 필요)
            var spawned = runner.Spawn(obj.NetworkObject, Vector3.zero, Quaternion.identity, runner.LocalPlayer);
            if (spawned == null)
            {
                Debug.LogError("Fusion 스폰 실패");
                return;
            }

            // 생성된 객체에서 캐릭터 유닛 참조 캐싱
            GameObject networkObj = spawned.gameObject;
            _playerUnit = networkObj.GetComponent<CharacterUnit>();
            if (_playerUnit == null)
            {
                Debug.LogWarning("생성된 객체에서 CharacterUnit 컴포넌트를 찾지 못했습니다.");
            }

            Debug.Log($"{networkObj.name} 네트워크 생성 완료 (Fusion)");
        }
    }
}