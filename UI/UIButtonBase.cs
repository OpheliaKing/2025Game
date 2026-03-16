using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
    public class UIButtonBase : MonoBehaviour
    {
        [SerializeField]
        protected string _clickSound = "Sound_Click_001";

        public virtual void OnClick()
        {
            GameManager.Instance.SoundManager.Play(SOUND_TYPE.SE, _clickSound);
        }
    }

}

