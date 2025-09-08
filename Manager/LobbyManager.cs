using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;

namespace Shin
{
    public class LobbyManager : MonoBehaviourPunCallbacks
    {
        [Header("Options")]
        [SerializeField] private byte maxPlayersPerRoom = 2;
        [SerializeField] private bool autoJoinLobby = true;

        [Header("Events")]
        public UnityEvent onConnectedToMaster;
        public UnityEvent onJoinedLobby;
        public UnityEvent onLeftLobby;
        public UnityEvent onJoinedRoom;
        public UnityEvent onLeftRoom;
        public UnityEvent<string> onError;
        public UnityEvent<List<RoomInfo>> onRoomListUpdatedEvent;
        public UnityEvent<List<string>> onPlayerListUpdated;

        private readonly Dictionary<string, RoomInfo> roomNameToInfo = new Dictionary<string, RoomInfo>();

        public bool IsConnected => PhotonNetwork.IsConnected;
        public bool InRoom => PhotonNetwork.InRoom;

        [Header("UI")]

        [SerializeField]
        private List<TextSendButton> _textSendButtonList;

        [Header("Managers")]
        [SerializeField]
        private RoomManager roomManager;

        private void EnsureRoomManager()
        {
            if (roomManager == null)
            {
                roomManager = GameObject.FindObjectOfType<RoomManager>();
            }
        }

        private void PushPlayersToRoomManager()
        {
            EnsureRoomManager();
            if (roomManager != null)
            {
                roomManager.UpdateRoomPlayers(PhotonNetwork.InRoom ? PhotonNetwork.PlayerList : null);
            }
        }

        private void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetLobbyManager(this);
            }
        }

        private void Start()
        {
            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
            }

            InitTextSendButton();
            EnsureRoomManager();
            roomManager.SetActive(false);
        }

        private void InitTextSendButton()
        {
            for (int i = 0; i < _textSendButtonList.Count; i++)
            {
                switch (_textSendButtonList[i].Type)
                {
                    case TextSnedButtonType.CREATE_ROOM:
                        _textSendButtonList[i].SetCallback(CreateRoom);
                        break;
                    case TextSnedButtonType.JOIN_ROOM:
                        _textSendButtonList[i].SetCallback(JoinRoom);
                        break;
                }
            }
        }


        public override void OnConnectedToMaster()
        {
            onConnectedToMaster?.Invoke();
            if (autoJoinLobby)
            {
                PhotonNetwork.JoinLobby();
            }
        }

        public override void OnJoinedLobby()
        {
            onJoinedLobby?.Invoke();
        }

        public override void OnLeftLobby()
        {
            onLeftLobby?.Invoke();
        }

        public void CreateRoom(string roomName)
        {
            if (string.IsNullOrWhiteSpace(roomName))
            {
                onError?.Invoke("방 이름이 비어있습니다.");
                return;
            }

            RoomOptions options = new RoomOptions
            {
                MaxPlayers = maxPlayersPerRoom,
                IsVisible = true,
                IsOpen = true
            };

            if (!PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default))
            {
                onError?.Invoke("방 생성 요청에 실패했습니다.");
            }
            else
            {
                Debug.Log($"{roomName} 방 생성 성공");
            }
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            onError?.Invoke($"방 생성 실패 ({returnCode}): {message}");
        }

        public void JoinRoom(string roomName)
        {
            if (string.IsNullOrWhiteSpace(roomName))
            {
                onError?.Invoke("방 이름이 비어있습니다.");
                return;
            }
            if (!PhotonNetwork.JoinRoom(roomName))
            {
                onError?.Invoke("방 접속 요청에 실패했습니다.");
            }
        }

        public void JoinRandomRoom()
        {
            if (!PhotonNetwork.JoinRandomRoom())
            {
                onError?.Invoke("랜덤 방 접속 요청에 실패했습니다.");
            }
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            onError?.Invoke($"랜덤 접속 실패 ({returnCode}): {message}");
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            onError?.Invoke($"방 접속 실패 ({returnCode}): {message}");
        }

        public override void OnJoinedRoom()
        {
            onJoinedRoom?.Invoke();
            UpdatePlayerNicknameList();
            // 방 인원 제한 도달 시 방 닫기 처리 (마스터 전용)
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom != null)
            {
                if (PhotonNetwork.CurrentRoom.PlayerCount >= maxPlayersPerRoom)
                {
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    PhotonNetwork.CurrentRoom.IsVisible = false;
                }
            }
            PushPlayersToRoomManager();
        }

        public override void OnLeftRoom()
        {
            onLeftRoom?.Invoke();
            PushPlayersToRoomManager();
        }

        public void LeaveRoom()
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
                EnsureRoomManager();
                if (roomManager != null)
                {
                    roomManager.SetActive(false);
                }
            }
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            foreach (var info in roomList)
            {
                if (info.RemovedFromList || info.PlayerCount == 0)
                {
                    roomNameToInfo.Remove(info.Name);
                }
                else
                {
                    roomNameToInfo[info.Name] = info;
                }
            }
            onRoomListUpdatedEvent?.Invoke(new List<RoomInfo>(roomNameToInfo.Values));
        }

        public IReadOnlyList<RoomInfo> GetCachedRoomList()
        {
            return new List<RoomInfo>(roomNameToInfo.Values);
        }

        public void SetLocalNickname(string nickname)
        {
            if (string.IsNullOrWhiteSpace(nickname))
            {
                onError?.Invoke("닉네임이 비어있습니다.");
                return;
            }
            PhotonNetwork.NickName = nickname;
        }

        private void UpdatePlayerNicknameList()
        {
            if (!PhotonNetwork.InRoom)
            {
                onPlayerListUpdated?.Invoke(new List<string>());
                return;
            }

            List<string> nicknames = new List<string>();
            foreach (var player in PhotonNetwork.PlayerList)
            {
                nicknames.Add(string.IsNullOrEmpty(player.NickName) ? $"Player_{player.ActorNumber}" : player.NickName);
            }
            onPlayerListUpdated?.Invoke(nicknames);
        }

        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            UpdatePlayerNicknameList();
            // 최대 인원 도달 시 방 닫기 (마스터만 처리)
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom != null)
            {
                if (PhotonNetwork.CurrentRoom.PlayerCount >= maxPlayersPerRoom)
                {
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    PhotonNetwork.CurrentRoom.IsVisible = false;
                }
            }
            PushPlayersToRoomManager();
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            UpdatePlayerNicknameList();
            // 인원 감소 시 방 다시 열기 (마스터만 처리)
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom != null)
            {
                if (PhotonNetwork.CurrentRoom.PlayerCount < maxPlayersPerRoom)
                {
                    PhotonNetwork.CurrentRoom.IsOpen = true;
                    PhotonNetwork.CurrentRoom.IsVisible = true;
                }
            }
            PushPlayersToRoomManager();
        }

        public void GameStart()
        {
            GameManager.Instance.NetworkManager.GameStart();
        }
    }
}


