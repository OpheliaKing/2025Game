using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Shin
{
    public class RoomManager : MonoBehaviour
    {
        [Header("Events")]
        public UnityEvent<List<PlayerRef>> onPlayersUpdated;

        private readonly List<PlayerRef> _currentPlayerNicknames = new List<PlayerRef>();

        public IReadOnlyList<PlayerRef> CurrentPlayerNicknames => _currentPlayerNicknames;

        [Header("Player INfo UI")]

        [SerializeField]
        private Transform _playerInfoUIParent;
        private List<RoomUIPlayerInfo> _playerInfoUIList = new List<RoomUIPlayerInfo>();


        [SerializeField]
        private TextMeshProUGUI _textMeshProUGUI;

        // Fusion 호환: 닉네임 컬렉션을 받아 UI/로직 갱신
        public void UpdateRoomPlayers(IEnumerable<PlayerRef> playerNicknames)
        {
            if (!gameObject.activeSelf)
            {
                SetActive(true);
            }

            Debug.Log("Test 12345");

            if (playerNicknames != null)
            {
                Debug.Log($"123Test {playerNicknames.Count(x => x != null)}");
            }

            _currentPlayerNicknames.Clear();
            if (playerNicknames != null)
            {
                _currentPlayerNicknames.AddRange(playerNicknames);
            }
            onPlayersUpdated?.Invoke(new List<PlayerRef>(_currentPlayerNicknames));

            UpdatePlayerUI();
        }

        private void InitPlayerInfoUI()
        {
            for (int i = 0; i < _playerInfoUIList.Count; i++)
            {
                _playerInfoUIList[i].SetActive(false);
            }
        }

        private void CreatePlayerInfoUI(int createCount)
        {
            for (int i = 0; i < createCount; i++)
            {
                var reManager = GameManager.Instance.ResourceManager;
                var uiInstance = reManager.InstantiatePrefab<RoomUIPlayerInfo>("RoomUIPlayerInfo", _playerInfoUIParent, $"{reManager.UIPrefabPath}/Room");
                _playerInfoUIList.Add(uiInstance);
            }
        }

        public void UpdatePlayerUI()
        {
            InitPlayerInfoUI();

            var createCount = CurrentPlayerNicknames.Count - _playerInfoUIList.Count;

            Debug.Log($"Create Count {createCount}");

            if (createCount > 0)
            {
                CreatePlayerInfoUI(createCount);
            }

            for (int i = 0; i < CurrentPlayerNicknames.Count; i++)
            {
                _playerInfoUIList[i].UpdateInfo(CurrentPlayerNicknames[i].PlayerId.ToString());
            }
        }

        // 필요 시 수동 갱신: Fusion 기준으로 구현 지점
        public void RefreshFromFusion()
        {
            UpdateRoomPlayers(null);
        }

        public void GameStart()
        {
            var runner = GameManager.Instance.NetworkManager.Runner;
            if (runner != null && runner.IsRunning)
            {
                GameManager.Instance.NetworkManager.Test();
                NetworkManager.RpcGameStart(runner);
            }
            else
            {
                Debug.LogError("NetworkRunner가 실행 중이 아닙니다.");
            }
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                GameManager.Instance.NetworkManager.Test();
            }
        }
        
        public void TestPush()
        {
            _textMeshProUGUI.text = "Test Push";
        }
    }
}
