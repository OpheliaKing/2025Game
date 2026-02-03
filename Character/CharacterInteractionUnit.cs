using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shin
{
    public partial class CharacterUnit
    {

        private InteractionObject _curInteractionObject;

        private HashSet<InteractionObject> _interactionObjectList = new HashSet<InteractionObject>();

        public InteractionObject CurInteractionObject
        {
            get { return _curInteractionObject; }
        }

        protected void InteractionUnitInit()
        {

        }
        public void SetInteractionObject(InteractionObject interactionObject)
        {
            if (_curInteractionObject == null)
            {
                SetCurInteractionObject(interactionObject);
                _interactionObjectList.Add(interactionObject);
            }
            else
            {
                _interactionObjectList.Add(interactionObject);
            }
        }

        private void SetCurInteractionObject(InteractionObject interactionObject)
        {
            _curInteractionObject = interactionObject;
            Debug.Log($"SetCurInteractionObject: {interactionObject.name}");
        }

        public void RemoveInteractionObject(InteractionObject interactionObject)
        {
            if (_interactionObjectList.Contains(interactionObject))
            {
                _interactionObjectList.Remove(interactionObject);
            }

            if (_curInteractionObject == interactionObject)
            {
                _curInteractionObject = null;
            }

            if (_interactionObjectList.Count != 0)
            {
                SetCurInteractionObject(_interactionObjectList.First());
            }
        }


        public void ClearCurInteractionObject()
        {
            _curInteractionObject = null;
        }

        public void ActiveInteraction()
        {
            if (_curInteractionObject == null)
            {
                return;
            }
            _curInteractionObject.ActiveInteraction();
        }

    }
}
