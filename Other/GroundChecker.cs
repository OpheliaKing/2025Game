using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    public Action<bool> OnGroundEvent;
    public Action<bool> OnOutGroundEvent;

    private int GroundLayer
    {
        get
        {
            return LayerMask.NameToLayer("Ground");
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Ground Checker");

        if (other.gameObject.layer == GroundLayer)
        {
            OnGroundEvent?.Invoke(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == GroundLayer)
        {
            OnOutGroundEvent?.Invoke(false);
        }
    }
}
