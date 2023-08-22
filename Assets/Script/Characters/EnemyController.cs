using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates
{
    GUARD,
    PATROL,
    CHASE,
    DEAD
}

[RequireComponent(typeof(NavMeshAgent))]    //确保NavMeshAgent组件添加上
[RequireComponent(typeof(CharacterStats))]
[RequireComponent(typeof(HealthBarUI))]

public class EnemyController : MonoBehaviour, IEndGameObserver
{
    private EnemyStates enemyStates;
    private NavMeshAgent agent;
    private Animator anim;
    private Collider coll;
    protected CharacterStats characterStats;

    [Header("Basic Settings")]
    public float sightRadius;
    public bool isGuard;
    public float lookAtTime;
    protected GameObject attackTarget;
    private float speed;
    private float remainLookAtTime;
    private float lastAttackTime;

    private Quaternion guardRotation;

    [Header("Patrol State")]
    public float patrolRange;
    private Vector3 wayPoint;
    private Vector3 guardPosition;

    //配合动画转换
    private bool isWalk;
    private bool isChase;
    private bool isFollow;
    private bool isDead;
    private bool playerDead;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        coll = GetComponent<Collider>();

        speed = agent.speed;
        guardPosition = transform.position;
        guardRotation = transform.rotation;
        remainLookAtTime = lookAtTime;
    }

    private void Start()
    {
        SwitchEnemyGuardOrPatrol();

        GameManager.Instance.AddObserver(this);
    }

    //切换场景时启用
    //private void OnEnable()
    //{
    //    GameManager.Instance.AddObserver(this);
    //}

    private void OnDisable()    //在销毁完成之后执行
    {
        if (!GameManager.IsInitialzed) return;
        GameManager.Instance.RemoveObserver(this);
    }

    private void Update()
    {
        if (characterStats.CurrentHealth == 0)
            isDead = true;

        if (!playerDead)
        {
            SwitchStates();
            SwitchAnimation();
        }

        //攻击计时
        lastAttackTime -= Time.deltaTime;
    }

    private void SwitchAnimation()
    {
        anim.SetBool("Walk", isWalk);
        anim.SetBool("Chase", isChase);
        anim.SetBool("Follow", isFollow);
        anim.SetBool("Critical", characterStats.isCritical);
        anim.SetBool("Death", isDead);
    }

    private void SwitchStates()
    {
        if (isDead)
            enemyStates = EnemyStates.DEAD;
        //如果发现Player，切换到CHASE追击状态
        else if (FoundPlayer())
        {
            enemyStates = EnemyStates.CHASE;
            Debug.Log("找到Player");
        }

        switch (enemyStates)
        {
            case EnemyStates.GUARD:
                isChase = false;
                if (Vector3.SqrMagnitude(guardPosition - transform.position) >= agent.stoppingDistance)
                {
                    isWalk = true;
                    agent.isStopped = false;
                    agent.speed = speed * 0.3f;
                    agent.destination = guardPosition;
                    if (Vector3.SqrMagnitude(guardPosition - transform.position) <= agent.stoppingDistance)//Vector3.SqrMagnitude()判断两点差值
                    {
                        isWalk = false;
                        //朝向归位
                        transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation, 0.01f);
                    }
                }
                break;
            case EnemyStates.PATROL:
                isChase = false;
                agent.speed = speed * 0.5f;

                //判断是否到了随机巡逻点
                if (Vector3.Distance(wayPoint, transform.position) <= agent.stoppingDistance)//Vector3.Distance()某些情况比Vector3.SqrMagnitude()性能开销大，推荐用Vector3.SqrMagnitude()
                {
                    isWalk = false;
                    if (remainLookAtTime > 0)
                        remainLookAtTime -= Time.deltaTime;
                    else
                        GetNewWayPoint();
                }
                else
                {
                    isWalk = true;
                    agent.destination = wayPoint;
                }

                break;
            case EnemyStates.CHASE:     //追击
                //TODO:追Player
                //TODO:配合动画
                isWalk = false;
                isChase = true;

                agent.speed = speed;

                if (!FoundPlayer())
                {
                    //脱战
                    //TODO:拉脱回到上一个状态
                    isFollow = false;

                    if (remainLookAtTime > 0)
                    {
                        agent.destination = transform.position;
                        remainLookAtTime -= Time.deltaTime;
                    }
                    else
                        //归位
                        SwitchEnemyGuardOrPatrol();

                    Debug.Log("丢失目标");
                }
                else
                {
                    //追击
                    isFollow = true;
                    agent.isStopped = false;
                    agent.destination = attackTarget.transform.position;
                }

                //TODO:在攻击范围内，则攻击
                if (TargetInAttackRange() || TargetInSkillRange())
                {
                    isFollow = false;
                    isWalk = false;
                    agent.isStopped = true;

                    //攻击冷却判断
                    if (lastAttackTime < 0)
                    {
                        lastAttackTime = characterStats.attackData.coolDown;

                        //暴击判断
                        characterStats.isCritical = Random.value < characterStats.attackData.criticalChance;//Random.value可以返回0到1之间的随机数
                        //执行攻击
                        Attack();
                    }
                }

                break;
            case EnemyStates.DEAD:
                //关闭碰撞组件
                coll.enabled = false;
                //关闭代理组件
                agent.radius = 0;
                Destroy(gameObject, 3f);
                break;
        }
    }

    /// <summary>
    /// 攻击动画
    /// </summary>
    private void Attack()
    {
        transform.LookAt(attackTarget.transform);
        if (TargetInAttackRange())
            //普通攻击动画
            anim.SetTrigger("Attack");
        if (TargetInSkillRange())
            //技能攻击动画
            anim.SetTrigger("Skill");
    }

    private bool TargetInAttackRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.attackRange;
        else
            return false;
    }

    private bool TargetInSkillRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.skillRange;
        else
            return false;
    }

    /// <summary>
    /// 发现玩家
    /// </summary>
    /// <returns>是否发现</returns>
    private bool FoundPlayer()
    {
        var colliders = Physics.OverlapSphere(transform.position, sightRadius);

        foreach (var target in colliders)
        {
            if (target.CompareTag("Player"))
            {
                attackTarget = target.gameObject;
                return true;
            }
        }
        attackTarget = null;
        return false;
    }

    /// <summary>
    /// 获取随机巡逻点
    /// </summary>
    private void GetNewWayPoint()
    {
        remainLookAtTime = lookAtTime;

        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);

        Vector3 randomPoint = new Vector3(guardPosition.x + randomX, transform.position.y, guardPosition.z + randomZ);
        //FIXME:可能出现的问题
        //wayPoint = randomPoint;
        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1) ? hit.position : transform.position;
    }

    /// <summary>
    /// 选择敌人 站桩/巡逻 状态
    /// </summary>
    private void SwitchEnemyGuardOrPatrol()
    {
        if (isGuard)
            enemyStates = EnemyStates.GUARD;
        else
        {
            enemyStates = EnemyStates.PATROL;
            GetNewWayPoint();
        }
    }

    /// <summary>
    /// 显示视野范围Gizmos
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }

    //Animation Event
    /// <summary>
    /// 攻击造成伤害
    /// </summary>
    private void Hit()
    {
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();

            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    /// <summary>
    /// 结束的广播
    /// </summary>
    public void EndNotify()
    {
        playerDead = true;

        //播放获胜动画
        //停止所有动作
        //停止所有Agent
        anim.SetBool("Win", true);

        isChase = false;
        isWalk = false;
        attackTarget = null;


    }
}
