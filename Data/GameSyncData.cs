using System;
using UnityEngine;

namespace Shin
{
    [Serializable]
    public class StageData
    {
        public string stageId;
        public string stageName;
        public int stageLevel;
        public string stageDescription;
        
        public StageData(string id, string name, int level, string description)
        {
            stageId = id;
            stageName = name;
            stageLevel = level;
            stageDescription = description;
        }
        
        public StageData()
        {
            stageId = "";
            stageName = "";
            stageLevel = 0;
            stageDescription = "";
        }
    }  
}
