# StageDataSO 리스트 시스템 사용 가이드

`StageDataSO`는 하나의 ScriptableObject 안에서 여러 스테이지 데이터를 리스트 형태로 관리하는 시스템입니다. Unity Inspector에서 리스트로 보이고 편집할 수 있어 직관적인 스테이지 데이터 관리가 가능합니다.

## 주요 구조

### 📋 StageDataItem 클래스
```csharp
[System.Serializable]
public class StageDataItem
{
    [Header("Stage Information")]
    public StageData stageData = new StageData();
    
    [Header("Prefab Reference")]
    public string prefabPath = "";
    
    [Header("Additional Settings")]
    public bool isUnlocked = true;
    public int requiredLevel = 1;
}
```

### 📊 StageDataSO 클래스
- **List<StageDataItem>**: Unity Inspector에서 리스트로 표시
- **인덱스 기반 접근**: 0부터 시작하는 인덱스로 스테이지 접근
- **ID 기반 검색**: stageId로 특정 스테이지 찾기
- **동적 관리**: 런타임에서 스테이지 추가/제거 가능

## 설정 방법

### 1. StageDataSO 생성
1. Unity 에디터에서 우클릭
2. `Create > Game > Stage Data Collection` 선택
3. Inspector에서 `Stage Data List`에 StageDataItem들을 추가

### 2. Unity Inspector에서 리스트 관리
```
Stage Data List (Size: 3)
├── Element 0
│   ├── Stage Information
│   │   ├── Stage Data
│   │   │   ├── Stage Id: "stage_forest"
│   │   │   ├── Stage Name: "숲의 전장"
│   │   │   ├── Stage Level: 1
│   │   │   └── Stage Description: "고대 숲에서 벌어지는 전투"
│   │   ├── Prefab Path: "Prefabs/Stages/ForestStage"
│   │   ├── Is Unlocked: true
│   │   └── Required Level: 1
├── Element 1
│   └── ... (사막 스테이지 데이터)
└── Element 2
    └── ... (눈 스테이지 데이터)
```

## 사용법

### 1. 기본 사용법
```csharp
// StageDataSO 참조
[SerializeField] private StageDataSO stageDataSO;

// stageId로 StageDataItem 찾기
StageDataItem stageItem = stageDataSO.FindStageById("stage_forest");

// 인덱스로 StageDataItem 가져오기
StageDataItem stageItem = stageDataSO.GetStageByIndex(0);

// 리스트 직접 접근
List<StageDataItem> stageList = stageDataSO.StageDataList;
```

### 2. 스테이지 선택 시스템
```csharp
public class StageSelectUI : MonoBehaviour
{
    [SerializeField] private StageDataSO stageDataSO;
    [SerializeField] private int playerLevel = 5;
    
    public void OnStageButtonClicked(string stageId)
    {
        // StageDataItem 가져오기
        StageDataItem stageItem = stageDataSO.FindStageById(stageId);
        
        if (stageItem == null)
        {
            Debug.LogError($"스테이지를 찾을 수 없습니다: {stageId}");
            return;
        }
        
        // 잠금 해제 확인
        if (stageItem.requiredLevel > playerLevel)
        {
            Debug.Log("레벨이 부족합니다!");
            return;
        }
        
        // 스테이지 선택 및 동기화
        StageData syncData = stageDataSO.FindStageDataForSyncById(stageId);
        inGameManager.SetLocalGameData(syncData, characterData);
    }
}
```

### 3. 인덱스 기반 스테이지 선택
```csharp
public void OnStageButtonClickedByIndex(int stageIndex)
{
    // 인덱스로 StageDataItem 가져오기
    StageDataItem stageItem = stageDataSO.GetStageByIndex(stageIndex);
    
    if (stageItem == null)
    {
        Debug.LogError($"스테이지 인덱스가 범위를 벗어났습니다: {stageIndex}");
        return;
    }
    
    // 스테이지 선택 및 동기화
    StageData syncData = new StageData(
        stageItem.stageData.stageId,
        stageItem.stageData.stageName,
        stageItem.stageData.stageLevel,
        stageItem.stageData.stageDescription
    );
    
    inGameManager.SetLocalGameData(syncData, characterData);
}
```

### 4. 스테이지 목록 표시
```csharp
public void DisplayAvailableStages()
{
    // 잠금 해제된 스테이지들 가져오기
    List<StageDataItem> unlockedStages = stageDataSO.GetUnlockedStages();
    
    for (int i = 0; i < unlockedStages.Count; i++)
    {
        var stage = unlockedStages[i];
        Debug.Log($"{i + 1}. {stage.stageData.stageName} (ID: {stage.stageData.stageId})");
    }
}
```

### 5. 레벨별 스테이지 필터링
```csharp
public void DisplayStagesForPlayerLevel(int playerLevel)
{
    // 플레이어 레벨에 맞는 스테이지들 가져오기
    List<StageDataItem> unlockableStages = stageDataSO.GetUnlockableStages(playerLevel);
    
    Debug.Log($"레벨 {playerLevel}에서 플레이 가능한 스테이지: {unlockableStages.Count}개");
    
    for (int i = 0; i < unlockableStages.Count; i++)
    {
        var stage = unlockableStages[i];
        Debug.Log($"{i + 1}. {stage.stageData.stageName} (필요 레벨: {stage.requiredLevel})");
    }
}
```

### 6. 리스트 직접 접근
```csharp
public void AccessListDirectly()
{
    List<StageDataItem> stageList = stageDataSO.StageDataList;
    
    // for 루프로 모든 스테이지 순회
    for (int i = 0; i < stageList.Count; i++)
    {
        var stage = stageList[i];
        Debug.Log($"Index: {i}, Name: {stage.stageData.stageName}, ID: {stage.stageData.stageId}");
    }
    
    // foreach로 모든 스테이지 순회
    foreach (var stage in stageList)
    {
        Debug.Log($"Name: {stage.stageData.stageName}, ID: {stage.stageData.stageId}");
    }
}
```

## InGameManager 연동

### stageId로 스테이지 설정
```csharp
// InGameManager의 새로운 메서드 사용
inGameManager.SetStageDataFromSO(stageDataSO, "stage_forest", characterData);
```

### 인덱스로 스테이지 설정
```csharp
// 인덱스 기반 스테이지 설정
inGameManager.SetStageDataFromSOByIndex(stageDataSO, 0, characterData);
```

## 런타임 스테이지 관리

### 스테이지 추가
```csharp
public void AddNewStage()
{
    // 새로운 StageDataItem 생성
    StageData newStageData = new StageData("stage_new", "새 스테이지", 1, "새로운 스테이지입니다.");
    StageDataItem newStageItem = new StageDataItem(newStageData, "Prefabs/Stages/NewStage", true, 1);
    
    // 스테이지 추가
    stageDataSO.AddStage(newStageItem);
}
```

### 스테이지 제거
```csharp
public void RemoveStage(string stageId)
{
    bool removed = stageDataSO.RemoveStage(stageId);
    
    if (removed)
    {
        Debug.Log($"스테이지 제거됨: {stageId}");
    }
}
```

### 프리팹 로드
```csharp
public void LoadStagePrefab(string stageId)
{
    GameObject prefab = stageDataSO.LoadPrefab(stageId);
    if (prefab != null)
    {
        GameObject instance = Instantiate(prefab);
        // 스테이지 초기화 로직
    }
}
```

## Unity Inspector 활용

### 리스트 편집
1. **추가**: `+` 버튼으로 새 요소 추가
2. **제거**: `-` 버튼으로 요소 제거
3. **재정렬**: 드래그 앤 드롭으로 순서 변경
4. **크기 조정**: `Size` 필드로 리스트 크기 조정

### 디버그 기능
- **Print All Stage Info**: 모든 스테이지 정보 출력
- **중복 ID 검사**: OnValidate에서 자동으로 중복된 stageId 확인

## 장점

1. **단일 파일 관리**: 하나의 ScriptableObject에서 모든 스테이지 관리
2. **직관적인 UI**: Unity Inspector에서 리스트로 보기 편함
3. **유연한 관리**: 런타임에서 동적으로 스테이지 추가/제거 가능
4. **인덱스 접근**: 순서가 중요한 경우 인덱스로 접근 가능
5. **ID 검색**: stageId로 빠른 검색 가능
6. **에러 방지**: null 체크 및 범위 검사 자동화

## 주의사항

1. **인덱스 범위**: 인덱스 접근 시 범위 확인 필요
2. **중복 ID**: 같은 stageId를 가진 스테이지가 있으면 경고
3. **프리팹 경로**: Resources 폴더 기준으로 상대 경로 입력
4. **메모리 관리**: 불필요한 스테이지는 적절히 제거

## 테스트

`StageDataSOListExample.cs` 스크립트를 사용하여 다음 기능들을 테스트할 수 있습니다:
- L키: 리스트 기본 동작 테스트
- P키: 모든 스테이지 정보 출력
- G키: 특정 스테이지 데이터 가져오기 테스트
- I키: 인덱스로 스테이지 데이터 가져오기 테스트
- U키: 잠금 해제된 스테이지들 테스트
- O키: 잠금 해제 가능한 스테이지들 테스트
- E키: 스테이지 존재 여부 확인 테스트
- R키: 리스트 직접 접근 테스트
- F키: 프리팹 로드 테스트
- S키: 스테이지 선택 및 동기화 테스트 (ID 기반)
- X키: 스테이지 선택 및 동기화 테스트 (인덱스 기반)
