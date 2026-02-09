using System.Collections;
using System.Collections.Generic;
using Shin;
using UnityEngine;

public class StageInfo : MonoBehaviour
{
    [SerializeField]
    private List<InteractionObject> _interactiveObjectList = new List<InteractionObject>();
    public List<InteractionObject> InteractiveObjectList => _interactiveObjectList;

    [SerializeField]
    private List<InteractionControlObject> _interactionControlObjectList = new List<InteractionControlObject>();
    public List<InteractionControlObject> InteractionControlObjectList => _interactionControlObjectList;


}
