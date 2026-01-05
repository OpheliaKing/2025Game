using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        /// <summary>
        /// 플레이어 프리팹 로드 및 스폰 요청
        /// 서버에서만 스폰할 수 있으므로, 클라이언트는 RPC로 요청
        /// </summary>
        public void LoadPlayerPrefab(string playerTid)
        {
            Debug.Log($"LoadPlayerPrefab: {playerTid}");

            var runner = GameManager.Instance.NetworkManager.Runner;
            if (runner == null || !runner.IsRunning)
            {
                Debug.LogWarning("NetworkRunner가 실행 중이 아닙니다. 스폰을 진행할 수 없습니다.");
                return;
            }

            Debug.Log("플레이어 프리팹 스폰 준비");

            // 서버(호스트)인 경우 직접 스폰
            if (runner.IsServer)
            {
                Debug.Log("플레이어 프리팹 서버 스폰");
                LoadPlayerPrefabInternal(playerTid);
            }
            else
            {
                // 클라이언트인 경우 서버에게 RPC로 스폰 요청
                Debug.Log("플레이어 프리팹 클라이언트 스폰");

                StartCoroutine(TestCO(playerTid));
                //RpcRequestSpawnPlayerPrefab(playerTid);
            }
        }

        private IEnumerator TestCO(string playerTid)
        {
            yield return new WaitForSeconds(3f);
            RpcRequestSpawnPlayerPrefab(playerTid);
        }

        /// <summary>
        /// 서버에서 플레이어 프리팹을 실제로 스폰하는 내부 메서드
        /// </summary>
        /// <param name="playerTid">플레이어 프리팹 ID</param>
        /// <param name="inputAuthority">InputAuthority를 가질 플레이어 (null이면 LocalPlayer)</param>
        private void LoadPlayerPrefabInternal(string playerTid, PlayerRef? inputAuthority = null)
        {
            Debug.Log($"{playerTid} 플레이어 프리팹 스폰 준비");

            var resourceManager = GameManager.Instance.ResourceManager;

            // 항상 NetworkManager의 Runner만 사용
            var runner = GameManager.Instance?.NetworkManager?.Runner;

            if (runner == null)
            {
                Debug.LogError("NetworkRunner를 찾을 수 없습니다.");
                return;
            }

            // 리소스에서 프리팹 자산 조회 (존재 확인용)
            var obj = resourceManager.LoadPrefab<CharacterBase>(playerTid, resourceManager.CharacterPrefabPath);
            if (obj == null)
            {
                Debug.LogError($"캐릭터 프리팹 로드 실패: {playerTid}");
                return;
            }

            //여 아래 부분은 직접 수정해야됨
            //InvalidOperationException: Behaviour not initialized: Object not set. 해당 버그 발생


            // InputAuthority 설정 (RPC 요청인 경우 요청한 플레이어, 아니면 LocalPlayer)
            var targetPlayer = inputAuthority ?? runner.LocalPlayer;

            // Fusion 스폰 (서버만 가능)
            var spawned = runner.Spawn(obj.NetworkObject, Vector3.zero, Quaternion.identity, targetPlayer);
            if (spawned == null)
            {
                Debug.LogError("Fusion 스폰 실패");
                return;
            }

            // 생성된 객체에서 캐릭터 유닛 참조 캐싱 (스폰한 플레이어의 인스턴스에서만)
            // Spawned() 콜백에서도 처리할 수 있음

            Debug.Log($"Test Player Spawned {spawned.name}");

            GameObject networkObj = spawned.gameObject;

            //여기 부분 주석 처리 후 문제 해결 시 아래 코드 삭제
            //_playerUnit = networkObj.GetComponent<CharacterUnit>();
            if (_playerUnit == null)
            {
                Debug.LogWarning("생성된 객체에서 CharacterUnit 컴포넌트를 찾지 못했습니다.");
            }

            RpcGrantCharacterControll(targetPlayer, _playerUnit.GetNetworkId());

            Debug.Log($"{networkObj.name} 네트워크 생성 완료 (Fusion), InputAuthority: {targetPlayer}");
        }

        /// <summary>
        /// 클라이언트가 서버에게 플레이어 프리팹 스폰을 요청하는 RPC
        /// </summary>
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RpcRequestSpawnPlayerPrefab(string playerTid, RpcInfo info = default)
        {
            Debug.Log($"서버가 플레이어 스폰 요청 받음: {playerTid}, 요청 플레이어: {info.Source}");

            // 서버에서 스폰 (요청한 플레이어를 InputAuthority로 설정)
            // LoadPlayerPrefabInternal은 항상 NetworkManager의 Runner를 사용
            LoadPlayerPrefabInternal(playerTid, info.Source);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RpcGrantCharacterControll(PlayerRef playerRef,NetworkId charUuid)
        {
            if (playerRef == Runner.LocalPlayer)
            {
                var findChar = GameObject.FindObjectsOfType<CharacterUnit>().FirstOrDefault(x => x.GetNetworkId() == charUuid);
                _playerUnit = findChar;
            }
        }


        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RpcTest()
        {
            Debug.Log("RpcTest 호출");
        }
    }
}