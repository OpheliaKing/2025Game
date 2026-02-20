using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shin
{
    //해당 클래스를 InGameNetworkManager 로 변경후 stageInfo도 합병해야됨
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

        /// <summary>
        /// 현재 방 안에 있는 플레이어 캐릭터 목록
        /// </summary>
        private Dictionary<string, CharacterUnit> _characterUnitList = new Dictionary<string, CharacterUnit>();

        public Dictionary<string, CharacterUnit> CharacterUnitList
        {
            get { return _characterUnitList; }
        }

        private void Update()
        {
            MoveInputUpdate();
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
                RpcRequestSpawnPlayerPrefab(playerTid);
            }
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
            var createPlayerUnit = networkObj.GetComponent<CharacterUnit>();
            if (createPlayerUnit == null)
            {
                Debug.LogWarning("생성된 객체에서 CharacterUnit 컴포넌트를 찾지 못했습니다.");
            }

            RpcGrantCharacterControll(targetPlayer, createPlayerUnit.GetNetworkId());
            Debug.Log($"{networkObj.name} 네트워크 생성 완료 (Fusion), InputAuthority: {targetPlayer}");

            if (Object.HasStateAuthority)
            {
                createPlayerUnit.MasterPlayerId = targetPlayer.PlayerId.ToString();
            }

            createPlayerUnit.CharacterInit();
        }

        /// <summary>
        /// 클라이언트가 서버에게 플레이어 프리팹 스폰을 요청하는 RPC
        /// </summary>
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RpcRequestSpawnPlayerPrefab(string playerTid, RpcInfo info = default)
        {
            Debug.Log($"서버가 플레이어 스폰 요청 받음: {playerTid}, 요청 플레이어: {info.Source}");
            LoadPlayerPrefabInternal(playerTid, info.Source);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RpcGrantCharacterControll(PlayerRef playerRef, NetworkId charUuid)
        {
            Debug.Log($"character uuid: {charUuid} \n Local Player Check = {playerRef == Runner.LocalPlayer}");
            //TestDebug("Target Player: " + playerRef.PlayerId.ToString());
            if (playerRef == Runner.LocalPlayer)
            {
                var findChar = GameObject.FindObjectsOfType<CharacterUnit>().FirstOrDefault(x => x.GetNetworkId() == charUuid);
                if (findChar == null)
                {
                    Debug.LogError("CharacterUnit을 찾을 수 없습니다.");
                    return;
                }
                _playerUnit = findChar;
                Debug.Log($"findChar: {findChar.name}");

                // State Authority를 가진 클라이언트(서버/호스트)에게 MasterPlayerId 설정 요청
                //RpcSetMasterPlayerId(charUuid, playerRef.PlayerId.ToString());
            }
        }

        /// <summary>
        /// MasterPlayerId를 설정하는 RPC
        /// State Authority를 가진 클라이언트에서만 실제로 값을 변경하고, 모든 클라이언트에 동기화됩니다.
        /// </summary>
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RpcSetMasterPlayerId(NetworkId charUuid, string masterPlayerId, RpcInfo info = default)
        {
            // State Authority를 가진 클라이언트에서만 실행됨
            var findChar = GameObject.FindObjectsOfType<CharacterUnit>().FirstOrDefault(x => x.GetNetworkId() == charUuid);
            if (findChar == null)
            {
                Debug.LogError($"CharacterUnit을 찾을 수 없습니다. NetworkId: {charUuid}");
                return;
            }

            // State Authority에서 [Networked] 속성 변경 → 자동으로 모든 클라이언트에 동기화됨
            findChar.MasterPlayerId = masterPlayerId;
            Debug.Log($"[StateAuthority] Master Player Id 설정: {findChar.MasterPlayerId}, Character: {findChar.name}");
        }

        public void InitCharacterUnitList()
        {
            _characterUnitList.Clear();
        }

        public void AddCharacterUnit(string userId, CharacterUnit characterUnit)
        {
            if (_characterUnitList.ContainsKey(userId))
            {
                Debug.LogError($"이미 캐릭터 유닛이 존재합니다. userId: {userId}");
                return;
            }

            _characterUnitList.Add(userId, characterUnit);
            Debug.Log($"캐릭터 유닛 추가: userId: {userId}, characterUnit: {characterUnit.name}");
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RpcPopupMessage(string message, RpcInfo info = default)
        {
            InGameManager.Instance.InGameUIManager.ShowTextPopup(message);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RpcGameClear()
        {
            GameManager.Instance.InputManager.SetInputMode(INPUT_MODE.UISelect);
            InGameManager.Instance.InGameUIManager.ShowUI("GameClearUI");

        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RpcGameClearInputCheck()
        {
            GameManager.Instance.UImanager.SetActiveCanvas(true);

            if (Object.HasStateAuthority)
            {
                GameManager.Instance.NetworkManager.SceneLoad("StartScene", LoadSceneMode.Single, () =>
                {
                    //Event
                });
            }
        }
    }
}