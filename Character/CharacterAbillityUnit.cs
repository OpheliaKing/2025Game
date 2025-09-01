using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
    public partial class CharacterUnit
    {
        // Start is called before the first frame update
        public virtual void ActiveAbilityA()
        {
            Debug.Log("ActiveAbilityA");
        }
        
        public virtual void ActiveAbilityB()
        {
            Debug.Log("ActiveAbilityB");
        }

        public virtual void ActiveAbilityC()
        {
            Debug.Log("ActiveAbilityC");
        }
    }
}