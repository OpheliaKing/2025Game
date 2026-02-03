using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Shin
{
    public class TeleportObject : InteractionObject
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                Debug.Log("Player entered the teleport area");
            }
        }
    }
}
