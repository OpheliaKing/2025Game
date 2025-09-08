using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shin;

[System.Serializable]
public class StageDataItem
{
    [Header("TID")]

    public string StageTid;

    [Header("Stage Information")]
    public StageData stageData = new StageData();
    
    [Header("Prefab Reference")]
    public string prefabPath = "";
    
    [Header("Additional Settings")]
    public bool isUnlocked = true;
    
    public StageDataItem()
    {
        stageData = new StageData();
        prefabPath = "";
        isUnlocked = true;
    }
    
    public StageDataItem(StageData data, string path, bool unlocked, int level)
    {
        stageData = data;
        prefabPath = path;
        isUnlocked = unlocked;
    }
}

[CreateAssetMenu(fileName = "New Stage Data Collection", menuName = "Game/Stage Data Collection")]
public class StageDataSO : ScriptableObject
{
    [Header("Stage Data List")]
    [SerializeField]
    private List<StageDataItem> stageDataList = new List<StageDataItem>();
    
    [Header("Settings")]
    [SerializeField]
    private bool autoInitialize = true;
    
    [SerializeField]
    private bool showDebugInfo = true;

    #region Properties
    
    /// <summary>
    /// 스테이지 데이터 리스트
    /// </summary>
    public List<StageDataItem> StageDataList => stageDataList;
    
    /// <summary>
    /// 스테이지 개수
    /// </summary>
    public int StageCount => stageDataList.Count;
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// stageId로 StageDataItem 찾기
    /// </summary>
    /// <param name="stageId">스테이지 ID</param>
    /// <returns>해당하는 StageDataItem, 없으면 null</returns>
    public StageDataItem FindStageById(string stageId)
    {
        if (string.IsNullOrEmpty(stageId))
        {
            Debug.LogWarning("stageId가 비어있습니다.");
            return null;
        }
        
        return stageDataList.Find(item => item.stageData.stageId == stageId);
    }
    
    /// <summary>
    /// 인덱스로 StageDataItem 가져오기
    /// </summary>
    /// <param name="index">인덱스</param>
    /// <returns>해당하는 StageDataItem, 없으면 null</returns>
    public StageDataItem GetStageByIndex(int index)
    {
        if (index < 0 || index >= stageDataList.Count)
        {
            Debug.LogWarning($"인덱스가 범위를 벗어났습니다: {index}");
            return null;
        }
        
        return stageDataList[index];
    }
    
    /// <summary>
    /// stageId로 StageData 찾기
    /// </summary>
    /// <param name="stageId">스테이지 ID</param>
    /// <returns>해당하는 StageData, 없으면 null</returns>
    public StageData FindStageDataById(string stageId)
    {
        StageDataItem item = FindStageById(stageId);
        return item?.stageData;
    }
    
    /// <summary>
    /// stageId로 동기화용 StageData 찾기
    /// </summary>
    /// <param name="stageId">스테이지 ID</param>
    /// <returns>동기화용 StageData, 없으면 null</returns>
    public StageData FindStageDataForSyncById(string stageId)
    {
        StageDataItem item = FindStageById(stageId);
        if (item == null) return null;
        
        return new StageData(
            item.stageData.stageId,
            item.stageData.stageName,
            item.stageData.stageLevel,
            item.stageData.stageDescription
        );
    }
    
    /// <summary>
    /// 잠금 해제된 스테이지들만 가져오기
    /// </summary>
    /// <returns>잠금 해제된 StageDataItem 리스트</returns>
    public List<StageDataItem> GetUnlockedStages()
    {
        return stageDataList.FindAll(item => item.isUnlocked);
    }
    
    
    /// <summary>
    /// 스테이지 존재 여부 확인
    /// </summary>
    /// <param name="stageId">스테이지 ID</param>
    /// <returns>존재 여부</returns>
    public bool HasStage(string stageId)
    {
        return FindStageById(stageId) != null;
    }
    
    /// <summary>
    /// 스테이지 추가
    /// </summary>
    /// <param name="stageDataItem">추가할 StageDataItem</param>
    public void AddStage(StageDataItem stageDataItem)
    {
        if (stageDataItem == null)
        {
            Debug.LogWarning("추가할 StageDataItem이 null입니다.");
            return;
        }
        
        if (HasStage(stageDataItem.stageData.stageId))
        {
            Debug.LogWarning($"이미 존재하는 스테이지 ID입니다: {stageDataItem.stageData.stageId}");
            return;
        }
        
        stageDataList.Add(stageDataItem);
        
        if (showDebugInfo)
        {
            Debug.Log($"스테이지 추가됨: {stageDataItem.stageData.stageName} (ID: {stageDataItem.stageData.stageId})");
        }
    }
    
    /// <summary>
    /// 스테이지 제거
    /// </summary>
    /// <param name="stageId">제거할 스테이지 ID</param>
    /// <returns>제거 성공 여부</returns>
    public bool RemoveStage(string stageId)
    {
        StageDataItem itemToRemove = FindStageById(stageId);
        if (itemToRemove == null)
        {
            Debug.LogWarning($"제거할 스테이지를 찾을 수 없습니다: {stageId}");
            return false;
        }
        
        bool removed = stageDataList.Remove(itemToRemove);
        
        if (removed && showDebugInfo)
        {
            Debug.Log($"스테이지 제거됨: {itemToRemove.stageData.stageName} (ID: {stageId})");
        }
        
        return removed;
    }
    
    /// <summary>
    /// 프리팹 로드
    /// </summary>
    /// <param name="stageId">스테이지 ID</param>
    /// <returns>로드된 프리팹</returns>
    public GameObject LoadPrefab(string stageId)
    {
        StageDataItem item = FindStageById(stageId);
        if (item == null)
        {
            Debug.LogWarning($"스테이지를 찾을 수 없습니다: {stageId}");
            return null;
        }
        
        if (string.IsNullOrEmpty(item.prefabPath))
        {
            Debug.LogWarning($"StageDataItem '{stageId}'의 프리팹 경로가 설정되지 않았습니다.");
            return null;
        }
        
        GameObject prefab = Resources.Load<GameObject>(item.prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"프리팹을 찾을 수 없습니다: {item.prefabPath}");
        }
        
        return prefab;
    }
    
    /// <summary>
    /// 모든 스테이지 정보 출력 (디버그용)
    /// </summary>
    [ContextMenu("Print All Stage Info")]
    public void PrintAllStageInfo()
    {
        Debug.Log($"=== 모든 스테이지 정보: {name} ===");
        Debug.Log($"총 스테이지 개수: {stageDataList.Count}");
        
        for (int i = 0; i < stageDataList.Count; i++)
        {
            var item = stageDataList[i];
            Debug.Log($"{i + 1}. {item.stageData.stageName} (ID: {item.stageData.stageId}) - Unlocked: {item.isUnlocked}");
        }
    }
    
    /// <summary>
    /// 특정 스테이지 정보 출력 (디버그용)
    /// </summary>
    /// <param name="stageId">스테이지 ID</param>
    public void PrintStageInfo(string stageId)
    {
        StageDataItem item = FindStageById(stageId);
        if (item != null)
        {
            Debug.Log($"=== Stage Info: {stageId} ===");
            Debug.Log($"ID: {item.stageData.stageId}");
            Debug.Log($"Name: {item.stageData.stageName}");
            Debug.Log($"Level: {item.stageData.stageLevel}");
            Debug.Log($"Description: {item.stageData.stageDescription}");
            Debug.Log($"Prefab Path: {item.prefabPath}");
            Debug.Log($"Is Unlocked: {item.isUnlocked}");
        }
        else
        {
            Debug.LogWarning($"스테이지를 찾을 수 없습니다: {stageId}");
        }
    }
    
    #endregion
    
    #region Unity Editor
    
    private void OnValidate()
    {
        // 에디터에서 값이 변경될 때 유효성 검사
        if (stageDataList != null)
        {
            // 중복된 stageId 확인
            var stageIds = new HashSet<string>();
            for (int i = 0; i < stageDataList.Count; i++)
            {
                if (stageDataList[i] != null)
                {
                    string stageId = stageDataList[i].stageData.stageId;
                    if (!string.IsNullOrEmpty(stageId))
                    {
                        if (stageIds.Contains(stageId))
                        {
                            Debug.LogWarning($"중복된 stageId가 발견되었습니다: {stageId} (인덱스: {i})");
                        }
                        else
                        {
                            stageIds.Add(stageId);
                        }
                    }
                }
            }
        }
    }
    
    #endregion
}
