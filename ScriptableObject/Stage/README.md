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

### 3. InGameManager 연동
```csharp
// stageId로 스테이지 설정
inGameManager.SetStageDataFromSO(stageDataSO, "stage_forest", characterData);

// 인덱스로 스테이지 설정
inGameManager.SetStageDataFromSOByIndex(stageDataSO, 0, characterData);
```

### 4. 프리팹 로드
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

## 주요 메서드

### 검색 메서드
- `FindStageById(string stageId)`: stageId로 StageDataItem 찾기
- `GetStageByIndex(int index)`: 인덱스로 StageDataItem 가져오기
- `FindStageDataById(string stageId)`: stageId로 StageData 찾기
- `FindStageDataForSyncById(string stageId)`: 동기화용 StageData 찾기

### 필터링 메서드
- `GetUnlockedStages()`: 잠금 해제된 스테이지들 반환
- `GetUnlockableStages(int playerLevel)`: 레벨 기준 잠금 해제 가능한 스테이지들 반환
- `HasStage(string stageId)`: 스테이지 존재 여부 확인

### 관리 메서드
- `AddStage(StageDataItem)`: 스테이지 추가
- `RemoveStage(string stageId)`: 스테이지 제거
- `LoadPrefab(string stageId)`: 프리팹 로드

## 장점

1. **단일 파일 관리**: 하나의 ScriptableObject에서 모든 스테이지 관리
2. **직관적인 UI**: Unity Inspector에서 리스트로 보기 편함
3. **유연한 관리**: 런타임에서 동적으로 스테이지 추가/제거 가능
4. **인덱스 접근**: 순서가 중요한 경우 인덱스로 접근 가능
5. **ID 검색**: stageId로 빠른 검색 가능
6. **Photon 동기화**: 기존 동기화 시스템과 완벽 호환

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