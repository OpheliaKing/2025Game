using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Shin
{
    public class MapSelectPopupUI : UIBase
    {
        [SerializeField]
        private Transform _dataUIParent;

        private ScrollRect _scrollRect;
        public ScrollRect ScrollRect
        {
            get
            {
                if (_scrollRect == null)
                {
                    _scrollRect = GetComponentInChildren<ScrollRect>();
                }
                return _scrollRect;
            }
        }

        private bool _isDataInit = false;
        public override void Show()
        {
            base.Show();

            if (!_isDataInit)
            {
                DataInit();
            }

            ScrollRect.verticalNormalizedPosition = 1f;
        }

        private void DataInit()
        {
            var mapSO = GameManager.Instance.ResourceManager.LoadSO<StageDataSO>("StageData", GameManager.Instance.ResourceManager.SOPath);
            var dataUIPrefab = GameManager.Instance.ResourceManager.LoadPrefab("MapDataUI", GameManager.Instance.ResourceManager.UIPrefabPath);

            if (mapSO == null)
            {
                Debug.LogError("Map SO not found");
                return;
            }

            for (int i = 0; i < mapSO.StageDataList.Count; i++)
            {
                var dataUI = Instantiate(dataUIPrefab, _dataUIParent).GetComponent<MapDataUI>();
                if (dataUI == null)
                {
                    Debug.LogError("Map Data UI not found");
                    return;
                }
                dataUI.UpdateUI(mapSO.StageDataList[i], SelectMap);
            }
            _isDataInit = true;
        }

        private void SelectMap(StageDataItem mapDataUI)
        {
            GameManager.Instance.UImanager.Pop();
            NetworkManager.RpcUpdateMapData(GameManager.Instance.NetworkManager.Runner, mapDataUI.StageTid);
        }
    }
}