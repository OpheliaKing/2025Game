using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shin
{
    public class SummonPlayer : PlayerUnit
    {
        public override void ActiveAbilityA()
        {
            ChangePartnerUI(-1);
        }

        public override void ActiveAbilityB()
        {
            ActivePartnerUI();
        }

        public override void ActiveAbilityC()
        {
            ChangePartnerUI(1);
        }

         public void ActivePartnerUI()
        {
            PlayerUI.instance.PlayerPartnerUI.ToggleSelectUI();
        }

        public void ChangePartnerUI(int value)
        {
            PlayerUI.instance.PlayerPartnerUI.ChangeSelectUI(value);
        }
    }
}
