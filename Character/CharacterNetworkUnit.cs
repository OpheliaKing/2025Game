using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
    public partial class CharacterUnit
    {
        public override void Spawned()
        {
            base.Spawned();
            SpawnedEnd();
        }

        public void SpawnedEnd()
        {
            Debug.Log($"Spawned 호출");

            StartCoroutine(SpawnedEndCO());
        }
        
        private IEnumerator SpawnedEndCO()
        {
            yield return new WaitUntil(() => MasterPlayerId != "");
           InGameManager.Instance.PlayerInfo.AddCharacterUnit(MasterPlayerId, this);
        }
    }
}
