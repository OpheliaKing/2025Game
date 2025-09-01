using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shin
{
    public static class ExtentionMethod
    {
        public static string ToString(this INPUT_MODE mode)
        {
            var result = "";
            switch (mode)
            {
                case INPUT_MODE.Player:
                    result = "Player";
                    break;
                case INPUT_MODE.UISelect:
                    result = "UISelect";
                    break;
            }
            
            return result;
        }
    }

}
