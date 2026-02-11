using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Unity.VisualScripting;
using System;

namespace Shin
{
    public class InGameStageInfo : MonoBehaviour
    {
        [Header("Stage & Character Settings")]
        [SerializeField]
        private string _selectCharacterTid;

        [SerializeField]
        private string _selectStageTid;

        private StageDataSO _stageSO;

        private StageDataSO StageSO
        {
            get
            {
                if (_stageSO == null)
                {
                    LoadSO();
                }

                return _stageSO;
            }
        }

        private void LoadSO()
        {
            var resourceManager = GameManager.Instance.ResourceManager;
            var so = resourceManager.LoadSO<StageDataSO>("StageData", resourceManager.SOPath);
            if (so == null)
            {
                Debug.Log("Not Found Stage Data SO");
            }
            _stageSO = so;
        }
        /// <summary>
        /// 스테이지 초기화 (기존 메서드 오버로드)
        /// </summary>
        private void StageInit()
        {
            // 기본 스테이지 초기화
        }

        public void LoadMapPrefab(string mapTid, Action<StageInfo> onComplete = null)
        {
            var data = StageSO.FindStageById(mapTid);

            if (data == null)
            {
                Debug.LogError($"Not Found Data {mapTid}");
            }
            var resourceManager = GameManager.Instance.ResourceManager;
            var obj = resourceManager.InstantiatePrefab<GameObject>(data.prefabPath, null, resourceManager.PrefabBasePath);
            var mapData = obj.GetComponent<StageInfo>();
            Debug.Log($"{obj.gameObject.name} 생성 완료");
            onComplete?.Invoke(mapData);
        }
    }
}

