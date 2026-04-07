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
        [Header("Player INfo UI")]

        [SerializeField]
        private Transform _playerInfoUIParent;
        private List<RoomUIPlayerInfo> _playerInfoUIList = new List<RoomUIPlayerInfo>();

        [SerializeField]
        private TextMeshProUGUI _mapNameText;

        [SerializeField]
        private GameObject _mapSelectButton;

        private bool _isInit = false;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            GameManager.Instance.NetworkManager.OnChangeMapTidCallback += OnChangeMapTid;
        }


        public void Show()
        {
            SetActive(true);
            if (!_isInit)
            {
                Init();
                _isInit = true;
            }
        }

        // Fusion 호환: 닉네임 컬렉션을 받아 UI/로직 갱신
        public void UpdateRoomPlayers()
        {
            if (!gameObject.activeSelf)
            {
                SetActive(true);
            }

            Debug.Log("서버 인지 확인 : " + GameManager.Instance.NetworkManager.Runner.IsServer);

            ActiveMapSelectButton(GameManager.Instance.NetworkManager.Runner.IsServer);
            UpdatePlayerUI();
        }

        public void InitPlayerInfoUI()
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

            var curRoomPlayerInfo = GameManager.Instance.NetworkManager.RoomPlayerInfo;
            var createCount = curRoomPlayerInfo.Count - _playerInfoUIList.Count;

            Debug.Log($"Create Count {createCount}");

            if (createCount > 0)
            {
                CreatePlayerInfoUI(createCount);
            }

            var index = 0;

            foreach (var player in curRoomPlayerInfo)
            {
                _playerInfoUIList[index].UpdateInfo(player.Key);
                index++;
            }
        }

        public void GameStart()
        {
            var mapName = GameManager.Instance.NetworkManager.SelectedMapTid;

            var nm = GameManager.Instance.NetworkManager;
            switch (GameManager.Instance.NetworkManager.Runner.GameMode)
            {
                case GameMode.Host:
                    if (nm.IsAllPlayerReady())
                    {
                        var runner = GameManager.Instance.NetworkManager.Runner;
                        if (runner != null && runner.IsRunning)
                        {
                            NetworkManager.RpcGameStart(runner, mapName);
                        }
                        else
                        {
                            Debug.LogError("NetworkRunner가 실행 중이 아닙니다.");
                        }
                    }
                    else
                    {
                        Debug.Log("Not All Player Ready");
                        //팝업으로 띄워줄지 고민
                    }
                    break;
                case GameMode.Client:

                    var localPlayer = nm.Runner.LocalPlayer;
                    var currentReady = nm.RoomPlayerInfo.TryGetValue(localPlayer, out var roomInfo) && roomInfo.IsReady;
                    NetworkManager.RpcRoomReady(nm.Runner, localPlayer, !currentReady);
                    break;
            }
        }

        public void HostLeft()
        {
            SetActive(false);
        }

        public void ActiveMapSelectButton(bool isActive)
        {
            _mapSelectButton.SetActive(isActive);
        }

        public void OnClickShowMapSelectPopup()
        {
            GameManager.Instance.UImanager.ShowUI("MapSelectPopupUI");
        }

        public void OnChangeMapTid(string mapTid)
        {
            var mapData = GameManager.Instance.ResourceManager.LoadSO<StageDataSO>("StageData", GameManager.Instance.ResourceManager.SOPath);
            if (mapData == null)
            {
                Debug.LogError("Map Data not found");
                return;
            }
            var mapName = mapData.StageDataList.Find(x => x.StageTid == mapTid)?.StageName;
            _mapNameText.text = mapName;
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public void OnClickLeaveRoom()
        {
            GameManager.Instance.NetworkManager.LeaveRoom();
        }
    }
}
