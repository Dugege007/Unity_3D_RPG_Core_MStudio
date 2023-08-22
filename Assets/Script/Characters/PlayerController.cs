using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;

    private CharacterStats characterStats;

    private GameObject attackTarget;
    private float lastAttackTime;

    private bool isDead;

    private float stopDistance;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();

        stopDistance = agent.stoppingDistance;
    }

    private void OnEnable()    //人物在场景中启用时
    {
        //在OnMouseClicked中添加一个方法，添加方式为 +=
        MouseManager.Instance.OnMouseClicked += MoveToTarget;
        MouseManager.Instance.OnEnemyClicked += EventAttack;
        GameManager.Instance.RigisterPlayer(characterStats);
    }

    private void Start()
    {
        //将player的数据注册到GameManager中
        SaveManager.Instance.LoadPlayerData();

        //初始数值，可直接在PlayerData中修改
        //characterStats.MaxHealth = 100;
        //characterStats.CurrentHealth = 100;
        //characterStats.BaseDefence = 5;
        //characterStats.CurrentDefence = 5;
    }

    private void OnDisable()    //人物在场景中销毁时
    {
        //在场景切换销毁物体时，取消订阅
        MouseManager.Instance.OnMouseClicked -= MoveToTarget;
        MouseManager.Instance.OnEnemyClicked -= EventAttack;
    }

    private void Update()
    {
        //判断当前血量是否为0，如果为0，则给isDead赋值true
        isDead = characterStats.CurrentHealth == 0;

        if (isDead)
            GameManager.Instance.NotifyObservers();
        SwitchAnimation();

        //攻击冷却衰减
        lastAttackTime -= Time.deltaTime;
    }

    private void SwitchAnimation()
    {
        anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
        anim.SetBool("Death", isDead);
    }

    /// <summary>
    /// 移动到目标点
    /// </summary>
    /// <param name="target">目标点</param>
    public void MoveToTarget(Vector3 target)    //添加的参数与定义的事件保持一致
    {
        //确保其他Coroutine停止，以打断攻击状态
        StopAllCoroutines();

        //判断玩家是否死亡，如果死亡直接return
        if (isDead) return;

        agent.stoppingDistance = stopDistance;

        //确保人物没有在停止状态
        agent.isStopped = false;
        agent.destination = target;
    }

    /// <summary>
    /// 攻击目标
    /// </summary>
    /// <param name="target">目标GameObject</param>
    private void EventAttack(GameObject target)
    {
        if (isDead) return;

        if (target != null)
        {
            attackTarget = target;
            characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.criticalChance;
            //开始协程
            StartCoroutine(MoveToAttackTarget());
        }
    }

    IEnumerator MoveToAttackTarget()
    {
        //确保人物没有在停止状态
        agent.isStopped = false;
        agent.stoppingDistance = characterStats.attackData.attackRange;
        //面向目标
        transform.LookAt(attackTarget.transform);

        //当与目标距离大于指定距离时，持续向目标移动
        //TODO:修改攻击范围参数
        while (Vector3.Distance(attackTarget.transform.position, transform.position) > characterStats.attackData.attackRange)
        {
            agent.destination = attackTarget.transform.position;
            yield return null;//如果到这里，那么下一帧会继续执行循环
        }

        //让人物停下
        agent.isStopped = true;
        //攻击
        if (lastAttackTime < 0)
        {
            anim.SetBool("Critical", characterStats.isCritical);
            anim.SetTrigger("Attack");
            //重置冷却时间
            lastAttackTime = characterStats.attackData.coolDown;
        }
    }

    //Animation Event
    private void Hit()
    {
        if (attackTarget.CompareTag("Attackable"))
        {
            if (attackTarget.GetComponent<Rock>())
            {
                attackTarget.GetComponent<Rock>().rockStates = Rock.RockStates.HitEnemy;
                attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;
                attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward * 20, ForceMode.Impulse);
            }
        }
        else
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();

            targetStats.TakeDamage(characterStats, targetStats);
        }

    }
}
