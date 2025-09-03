using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
    public class InputCheckUI : UIBase
    {
        [SerializeField]
        private InputTargetUI _target;

        [SerializeField]
        private int _defaultIndex = -1;
        protected override void Init()
        {
            base.Init();


            if (_target != null)
            {
                _target.ResetSelection();
                _target.SetSelectedIndex(_defaultIndex);
            }
        }

        public void SetTarget(InputTargetUI target)
        {
            _target = target;
        }

        protected override void OnUpImpl()
        {
            _target?.InvokeUp();
        }

        protected override void OnDownImpl()
        {
            _target?.InvokeDown();
        }

        protected override void OnLeftImpl()
        {
            _target?.InvokeLeft();
        }

        protected override void OnRightImpl()
        {
            _target?.InvokeRight();
        }

        protected override void OnConfirmImpl()
        {
            _target?.InvokeConfirm();
        }

        protected override void OnCancelImpl()
        {
            _target?.InvokeCancel();
        }
    }
}

