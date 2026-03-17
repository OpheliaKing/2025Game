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

        private Animator _animator;

        private Animator Animator
        {
            get
            {
                if (_animator == null)
                {
                    _animator = GetComponent<Animator>();
                }

                return _animator;
            }
        }

        private IEnumerator _doorMoveCo;

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
            //_doorObject.SetActive(false);

            _doorMoveCo = DoorMoveCo(true);
            StartCoroutine(_doorMoveCo);
        }



        private void CloseDoor()
        {
            _isOpen = false;
            //_doorObject.SetActive(true);
            _doorMoveCo = DoorMoveCo(false);
            StartCoroutine(_doorMoveCo);
        }

        private IEnumerator DoorMoveCo(bool isOpen)
        {
            var animString = isOpen ? "Map_Move_Up" : "Map_Move_Down";
            Animator.Play(animString);
            yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).IsName("DoorOpen") && Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);

            _isOpen = isOpen;
        }
    }
}
