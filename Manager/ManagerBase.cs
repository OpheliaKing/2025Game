using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
    public class ManagerBase<T> : SingletonObject<T> where T : MonoBehaviour
    {
        public virtual void ManagerInit()
        {
            
        }
    }
}

