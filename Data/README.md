# 게임 데이터 동기화 시스템

이 폴더는 Photon을 사용한 멀티플레이어 게임에서 스테이지와 캐릭터 데이터를 동기화하기 위한 데이터 구조를 포함합니다.

## 파일 설명

### GameSyncData.cs
- `StageData`: 스테이지 정보를 담는 클래스
- `CharacterData`: 캐릭터 정보를 담는 클래스  
- `GameSyncData`: 전체 게임 동기화 데이터를 담는 클래스

## 사용법

1. 스테이지와 캐릭터 데이터를 생성합니다.
2. `InGameManager.SetLocalGameData()` 메서드를 호출하여 데이터를 동기화합니다.
3. 다른 플레이어들이 자동으로 데이터를 받아 처리합니다.

## 예시

```csharp
// 스테이지 데이터 생성
StageData stageData = new StageData("stage_forest", "숲의 전장", 1, "고대 숲에서 벌어지는 전투");

// 캐릭터 데이터 생성
CharacterData characterData = new CharacterData(
    "char_warrior", 
    "전사", 
    5, 
    150f, 
    150f, 
    25f, 
    10f, 
    new string[] { "강타", "방어막", "돌진" }
);

// 데이터 동기화
inGameManager.SetLocalGameData(stageData, characterData);
```
