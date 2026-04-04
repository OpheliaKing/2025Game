using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
    public class MapSelectPopupUI : UIBase
    {
        public override void Show()
        {
            base.Show();

            var mapSO = GameManager.Instance.ResourceManager.LoadSO<StageDataSO>("StageData", GameManager.Instance.ResourceManager.SOPath);

            if(mapSO == null)
            {
                Debug.LogError("Map SO not found");
                return;
            }

            for (int i = 0; i < mapSO.StageDataList.Count; i++)
            {
                
            }

            var mapData = mapSO.StageDataList;
        }
    }
}