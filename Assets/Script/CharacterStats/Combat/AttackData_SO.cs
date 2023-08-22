using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Attack",menuName ="Attack")]
public class AttackData_SO : ScriptableObject
{
    //普通攻击距离
    public float attackRange;
    //技能攻击距离
    public float skillRange;
    //CD时间
    public float coolDown;
    //最小攻击数值
    public int minDamage;
    //最大攻击数值
    public int maxDamage;

    //暴击加成百分比
    public float criticalMultiplier;
    //暴击率
    public float criticalChance;
}
