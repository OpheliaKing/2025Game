using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
    public class SingletonObject<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _isDestroyed = false;

        public static T Instance
        {
            get
            {
                if (_isDestroyed)
                {
                    return null;
                }

                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T).ToString();
                        
                        DontDestroyOnLoad(singletonObject);
                    }
                }
                
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _isDestroyed = true;
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _isDestroyed = true;
        }
    }
}

