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

        public static string ToFileName(this EMOETION_TYPE emotionType)
        {
            switch (emotionType)
            {
                case EMOETION_TYPE.LOVE:
                    return "02_Heart_C";
                case EMOETION_TYPE.QUESTION:
                    return "03_Question_C";
                case EMOETION_TYPE.SURPRISED:
                    return "04_Surprised_C";
                case EMOETION_TYPE.ANGRY:
                    return "21_Very angry_C";
                case EMOETION_TYPE.ERROR:
                    return "15_Error_C";
                case EMOETION_TYPE.AGREE:
                    return "14_Correct_C";
                case EMOETION_TYPE.SILENCE:
                    return "01_Ellipsis_C";
                default:
                    return "";
            }

        }
    }
}