using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Data",menuName ="Character Stats/Data")]//在Create菜单中创建子集菜单
public class CharacterData_SO : ScriptableObject
{
    [Header("Stats Info")]
    public int maxHealth;
    public int currentHealth;
    public int baseDefence;
    public int currentDefence;
    //攻击数值单独一个CharacterData_SO存储

    [Header("Kill")]
    public int killPoint;

    [Header("Level")]
    public int currentLevel;
    public int maxLevel;
    public int baseExp;
    public int currentExp;
    public float levelBuff;
    public float LevelMultiplier
    {
        get { return 1 + (currentLevel - 1) + levelBuff; }
    }

    public void UpdateExp(int point)
    {
        currentExp += point;
        if (currentExp>=baseExp)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        //所有需要提升数据的方法都在这里
        currentLevel = Mathf.Clamp(currentLevel + 1, 0, maxLevel);    // Mathf.Clamp()保证currentLevel + 1 在 0~20之间
        baseExp += (int)(maxHealth * LevelMultiplier);

        maxHealth = (int)(maxHealth * LevelMultiplier);
        currentHealth = maxHealth;

        Debug.Log("Level Up! " + currentLevel + "\nMax Health: " + maxHealth);
    }
}
