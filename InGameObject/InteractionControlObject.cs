using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
    /// <summary>
    /// InteractionObject에 의해 컨트롤 되는 오브젝트
    /// </summary>

    public class InteractionControlObject : MonoBehaviour
    {

        [SerializeField]
        private string _objectId;
        public string ObjectId => _objectId;

        [SerializeField]
        private OBJECT_CONTROL_TYPE _controlType;

        [SerializeField]
        private GameObject _doorObject;

        private bool _isOpen = false;

        public void ControlObject()
        {
            switch (_controlType)
            {
                case OBJECT_CONTROL_TYPE.OPEN_DOOR:
                    OpenDoor();
                    break;
                case OBJECT_CONTROL_TYPE.CLOSE_DOOR:
                    CloseDoor();
                    break;
                case OBJECT_CONTROL_TYPE.TOGGLE_DOOR:
                    _isOpen = !_isOpen;
                    if (_isOpen)
                    {
                        OpenDoor();
                    }
                    else
                    {
                        CloseDoor();
                    }
                    break;
            }
        }

        private void OpenDoor()
        {
            _isOpen = true;
            _doorObject.SetActive(false);
        }

        private void CloseDoor()
        {
            _isOpen = false;
            _doorObject.SetActive(true);
        }
    }
}
