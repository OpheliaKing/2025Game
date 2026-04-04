using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shin;

[System.Serializable]
public class StageDataItem
{
    [Header("TID")]

    public string StageTid;
    public string stageName;
    public int stageLevel;
    public string stageDescription;

    public string prefabPath = "";
    public string mapImagePath = "";
    public bool isUnlocked = true;

    public StageDataItem()
    {
        StageTid = "";
        prefabPath = "";
        isUnlocked = true;
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
    /// StageTid로 StageDataItem 찾기
    /// </summary>
    /// <param name="stageTid">스테이지 TID</param>
    /// <returns>해당하는 StageDataItem, 없으면 null</returns>
    public StageDataItem FindStageById(string stageTid)
    {
        if (string.IsNullOrEmpty(stageTid))
        {
            Debug.LogWarning("StageTid가 비어있습니다.");
            return null;
        }

        return stageDataList.Find(item => item.StageTid == stageTid);
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
    /// StageTid로 StageData 찾기
    /// </summary>
    /// <param name="stageTid">스테이지 TID</param>
    /// <returns>해당하는 StageData, 없으면 null</returns>
    public StageData FindStageDataById(string stageTid)
    {
        StageDataItem item = FindStageById(stageTid);
        if (item == null) return null;

        return new StageData(
            item.StageTid,
            item.stageName,
            item.stageLevel,
            item.stageDescription
        );
    }

    /// <summary>
    /// StageTid로 동기화용 StageData 찾기
    /// </summary>
    /// <param name="stageTid">스테이지 TID</param>
    /// <returns>동기화용 StageData, 없으면 null</returns>
    public StageData FindStageDataForSyncById(string stageTid)
    {
        return FindStageDataById(stageTid);
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
    /// <param name="stageTid">스테이지 TID</param>
    /// <returns>존재 여부</returns>
    public bool HasStage(string stageTid)
    {
        return FindStageById(stageTid) != null;
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

        if (HasStage(stageDataItem.StageTid))
        {
            Debug.LogWarning($"이미 존재하는 스테이지 TID입니다: {stageDataItem.StageTid}");
            return;
        }

        stageDataList.Add(stageDataItem);

        if (showDebugInfo)
        {
            Debug.Log($"스테이지 추가됨: {stageDataItem.stageName} (TID: {stageDataItem.StageTid})");
        }
    }

    /// <summary>
    /// 스테이지 제거
    /// </summary>
    /// <param name="stageTid">제거할 스테이지 TID</param>
    /// <returns>제거 성공 여부</returns>
    public bool RemoveStage(string stageTid)
    {
        StageDataItem itemToRemove = FindStageById(stageTid);
        if (itemToRemove == null)
        {
            Debug.LogWarning($"제거할 스테이지를 찾을 수 없습니다: {stageTid}");
            return false;
        }

        bool removed = stageDataList.Remove(itemToRemove);

        if (removed && showDebugInfo)
        {
            Debug.Log($"스테이지 제거됨: {itemToRemove.stageName} (TID: {stageTid})");
        }

        return removed;
    }

    /// <summary>
    /// 프리팹 로드
    /// </summary>
    /// <param name="stageTid">스테이지 TID</param>
    /// <returns>로드된 프리팹</returns>
    public GameObject LoadPrefab(string stageTid)
    {
        StageDataItem item = FindStageById(stageTid);
        if (item == null)
        {
            Debug.LogWarning($"스테이지를 찾을 수 없습니다: {stageTid}");
            return null;
        }

        if (string.IsNullOrEmpty(item.prefabPath))
        {
            Debug.LogWarning($"StageDataItem '{stageTid}'의 프리팹 경로가 설정되지 않았습니다.");
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
            Debug.Log($"{i + 1}. {item.stageName} (TID: {item.StageTid}) - Unlocked: {item.isUnlocked}");
        }
    }

    /// <summary>
    /// 특정 스테이지 정보 출력 (디버그용)
    /// </summary>
    /// <param name="stageTid">스테이지 TID</param>
    public void PrintStageInfo(string stageTid)
    {
        StageDataItem item = FindStageById(stageTid);
        if (item != null)
        {
            Debug.Log($"=== Stage Info: {stageTid} ===");
            Debug.Log($"TID: {item.StageTid}");
            Debug.Log($"Name: {item.stageName}");
            Debug.Log($"Level: {item.stageLevel}");
            Debug.Log($"Description: {item.stageDescription}");
            Debug.Log($"Prefab Path: {item.prefabPath}");
            Debug.Log($"Is Unlocked: {item.isUnlocked}");
        }
        else
        {
            Debug.LogWarning($"스테이지를 찾을 수 없습니다: {stageTid}");
        }
    }

    #endregion

    #region Unity Editor

    private void OnValidate()
    {
        // 에디터에서 값이 변경될 때 유효성 검사
        if (stageDataList != null)
        {
            // 중복된 StageTid 확인
            var stageTids = new HashSet<string>();
            for (int i = 0; i < stageDataList.Count; i++)
            {
                if (stageDataList[i] != null)
                {
                    string tid = stageDataList[i].StageTid;
                    if (!string.IsNullOrEmpty(tid))
                    {
                        if (stageTids.Contains(tid))
                        {
                            Debug.LogWarning($"중복된 StageTid가 발견되었습니다: {tid} (인덱스: {i})");
                        }
                        else
                        {
                            stageTids.Add(tid);
                        }
                    }
                }
            }
        }
    }

    #endregion
}
