using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStats", menuName = "Battle/Character Stats")]
public class CharacterStats : ScriptableObject
{
    [Header("ステータス")]
    public int maxHP = 100;
    public int maxSP = 5;
    
    [Header("スキル")]
    public int reflectSkillSPCost = 1;
    public int reflectSuccessRate = 80;
    public int reflectMaxCount = 3;
    
    [Header("攻撃")]
    public int attackDamage = 10;
}