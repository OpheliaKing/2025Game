// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Rendering.Universal;
// using Photon.Pun;
// using Photon.Pun.Demo.Cockpit;
// using Unity.VisualScripting;

// namespace Shin
// {
//     public partial class InGameManager
//     {
//         [Header("Stage & Character Settings")]
//         [SerializeField]
//         private string _selectCharacterTid;
        
//         [SerializeField]
//         private string _selectStageTid;

//         private StageDataSO _stageSO;

//         private StageDataSO StageSO
//         {
//             get
//             {
//                 if (_stageSO == null)
//                 {
//                     LoadSO();
//                 }

//                 return _stageSO;
//             }
//         }

//         private void Start()
//         {
//             // 동기화 이벤트 구독
//             OnGameDataReceived += OnGameDataReceivedHandler;
//             OnPlayerDataUpdated += OnPlayerDataUpdatedHandler;
//         }

//         private void OnDestroy()
//         {
//             // 이벤트 구독 해제
//             OnGameDataReceived -= OnGameDataReceivedHandler;
//             OnPlayerDataUpdated -= OnPlayerDataUpdatedHandler;
//         }

//         private void LoadSO()
//         {
//             var resourceManager = GameManager.Instance.ResourceManager;
//             var so = resourceManager.LoadSO<StageDataSO>("StageData", resourceManager.SOPath);
//             if (so == null)
//             {
//                 Debug.Log("Not Found Stage Data SO");
//             }
//             _stageSO = so;
//         }

//         /// <summary>
//         /// 게임 데이터를 받았을 때 호출되는 핸들러
//         /// </summary>
//         private void OnGameDataReceivedHandler(GameSyncData gameData)
//         {
//             Debug.Log($"게임 데이터 수신: {gameData.playerNickname}의 데이터 처리 중...");

//             // 스테이지 데이터 처리
//             ProcessStageData(gameData.selectedStage);

//             // 캐릭터 데이터 처리
//             ProcessCharacterData(gameData.selectedCharacter, gameData.playerId);
//         }

//         /// <summary>
//         /// 플레이어 데이터가 업데이트되었을 때 호출되는 핸들러
//         /// </summary>
//         private void OnPlayerDataUpdatedHandler(string playerId, GameSyncData gameData)
//         {
//             Debug.Log($"플레이어 {gameData.playerNickname}의 데이터 업데이트됨");
            
//             // UI 업데이트나 다른 처리 로직 추가 가능
//             UpdatePlayerUI(playerId, gameData);
//         }

//         /// <summary>
//         /// 스테이지 데이터 처리
//         /// </summary>
//         private void ProcessStageData(StageData stageData)
//         {
//             if (string.IsNullOrEmpty(stageData.stageId))
//                 return;

//             Debug.Log($"스테이지 설정: {stageData.stageName} (레벨: {stageData.stageLevel})");
            
//             // 스테이지별 초기화 로직
//             StageInit(stageData);
//         }

//         /// <summary>
//         /// 캐릭터 데이터 처리
//         /// </summary>
//         private void ProcessCharacterData(CharacterData characterData, string playerId)
//         {
//             if (string.IsNullOrEmpty(characterData.characterId))
//                 return;

//             Debug.Log($"캐릭터 설정: {characterData.characterName} (레벨: {characterData.characterLevel})");
            
//             // 캐릭터별 초기화 로직
//             CharacterUnitInit(characterData, playerId);
//         }

//         /// <summary>
//         /// 플레이어 UI 업데이트
//         /// </summary>
//         private void UpdatePlayerUI(string playerId, GameSyncData gameData)
//         {
//             // 플레이어 정보 UI 업데이트 로직
//             // 예: 플레이어 리스트에서 해당 플레이어의 스테이지/캐릭터 정보 표시
//         }

//         /// <summary>
//         /// 스테이지 초기화 (기존 메서드 오버로드)
//         /// </summary>
//         private void StageInit()
//         {
//             // 기본 스테이지 초기화
//         }

//         /// <summary>
//         /// 스테이지 초기화 (데이터 기반)
//         /// </summary>
//         private void StageInit(StageData stageData)
//         {
//             _selectStageTid = stageData.stageId;
            
//             // 스테이지별 환경 설정
//             switch (stageData.stageId)
//             {
//                 case "stage_forest":
//                     // 숲 스테이지 설정
//                     break;
//                 case "stage_desert":
//                     // 사막 스테이지 설정
//                     break;
//                 case "stage_snow":
//                     // 눈 스테이지 설정
//                     break;
//                 default:
//                     Debug.LogWarning($"알 수 없는 스테이지 ID: {stageData.stageId}");
//                     break;
//             }
//         }

//         /// <summary>
//         /// 캐릭터 유닛 초기화 (기존 메서드 오버로드)
//         /// </summary>
//         private void CharacterUnitInit()
//         {
//             // 기본 캐릭터 초기화
//         }

//         /// <summary>
//         /// 캐릭터 유닛 초기화 (데이터 기반)
//         /// </summary>
//         private void CharacterUnitInit(CharacterData characterData, string playerId)
//         {
//             _selectCharacterTid = characterData.characterId;
            
//             // 로컬 플레이어인 경우에만 실제 캐릭터 유닛 설정
//             if (playerId == PhotonNetwork.LocalPlayer.UserId && _playerUnit != null)
//             {
//                 ApplyCharacterDataToUnit(_playerUnit, characterData);
//             }
            
//             // 캐릭터별 특수 능력 설정
//             SetupCharacterAbilities(characterData);
//         }

//         /// <summary>
//         /// 캐릭터 데이터를 유닛에 적용
//         /// </summary>
//         private void ApplyCharacterDataToUnit(CharacterUnit unit, CharacterData characterData)
//         {
//             // 캐릭터 스탯 적용
//             // unit.SetHealth(characterData.health);
//             // unit.SetMaxHealth(characterData.maxHealth);
//             // unit.SetAttackPower(characterData.attackPower);
//             // unit.SetDefense(characterData.defense);
            
//             Debug.Log($"캐릭터 {characterData.characterName}의 스탯이 적용되었습니다.");
//         }

//         /// <summary>
//         /// 캐릭터별 특수 능력 설정
//         /// </summary>
//         private void SetupCharacterAbilities(CharacterData characterData)
//         {
//             if (characterData.abilities == null || characterData.abilities.Length == 0)
//                 return;

//             foreach (string ability in characterData.abilities)
//             {
//                 Debug.Log($"캐릭터 {characterData.characterName}의 능력: {ability}");
//                 // 능력별 설정 로직
//             }
//         }

//         /// <summary>
//         /// 테스트용 메서드 - 스테이지와 캐릭터 데이터 설정
//         /// </summary>
//         [ContextMenu("Test Set Game Data")]
//         public void TestSetGameData()
//         {
//             // 테스트용 스테이지 데이터
//             StageData testStage = new StageData("stage_forest", "숲의 전장", 1, "고대 숲에서 벌어지는 전투");
            
//             // 테스트용 캐릭터 데이터
//             CharacterData testCharacter = new CharacterData(
//                 "char_warrior", 
//                 "전사", 
//                 5, 
//                 150f, 
//                 150f, 
//                 25f, 
//                 10f, 
//                 new string[] { "강타", "방어막", "돌진" }
//             );
            
//             SetLocalGameData(testStage, testCharacter);
//         }
//     }
// }

