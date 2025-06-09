using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterAttackData", menuName = "Scriptable Object/CharacterAttackData", order = int.MaxValue)]
public class CharacterAttackSO : ScriptableObject
{
    [SerializeField] private List<CharacterAttackSOData> NormalAttackData;

}

[Serializable]
public class CharacterAttackSOData
{
    public float Damage;
    /// <summary>
    /// 공격의 위치 값
    /// </summary>
    public Vector2 Position;
    /// <summary>
    /// 공격의 범위값
    /// </summary>
    public Vector2 Range = new Vector2(1f, 1f);

    public GameObject Effect;
    public GameObject Sound;
}