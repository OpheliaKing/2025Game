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
    
    [Serializable]
    public class CharacterData
    {
        public string characterId;
        public string characterName;
        public int characterLevel;
        public float health;
        public float maxHealth;
        public float attackPower;
        public float defense;
        public string[] abilities;
        
        public CharacterData(string id, string name, int level, float hp, float maxHp, float attack, float defense, string[] abilities)
        {
            characterId = id;
            characterName = name;
            characterLevel = level;
            health = hp;
            maxHealth = maxHp;
            attackPower = attack;
            this.defense = defense;
            this.abilities = abilities ?? new string[0];
        }
        
        public CharacterData()
        {
            characterId = "";
            characterName = "";
            characterLevel = 0;
            health = 100f;
            maxHealth = 100f;
            attackPower = 10f;
            defense = 5f;
            abilities = new string[0];
        }
    }
    
    [Serializable]
    public class GameSyncData
    {
        public StageData selectedStage;
        public CharacterData selectedCharacter;
        public string playerId;
        public string playerNickname;
        public float timestamp;
        
        public GameSyncData()
        {
            selectedStage = new StageData();
            selectedCharacter = new CharacterData();
            playerId = "";
            playerNickname = "";
            //timestamp = Time.time;
        }
        
        public GameSyncData(StageData stage, CharacterData character, string playerId, string nickname)
        {
            selectedStage = stage;
            selectedCharacter = character;
            this.playerId = playerId;
            playerNickname = nickname;
            //timestamp = Time.time;
        }
    }
}
