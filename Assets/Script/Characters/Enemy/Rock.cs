using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Rock : MonoBehaviour
{
    public enum RockStates
    {
        HitPlayer,
        HitEnemy,
        HitNothing
    }
    public RockStates rockStates;

    private Rigidbody rb;

    [Header("Basic Settings")]
    public float force;
    public int damage;
    public GameObject target;
    public GameObject breakEffect;
    private Vector3 direction;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        //出场时设置速度，避免直接进入RockStates.HitNothing
        rb.velocity = Vector3.one;

        rockStates = RockStates.HitPlayer;
        FlyToTarget();
    }

    private void FixedUpdate()
    {
        if (rb.velocity.sqrMagnitude<1)
        {
            rockStates = RockStates.HitNothing;
        }
    }

    public void FlyToTarget()
    {
        direction = (target.transform.position - transform.position + Vector3.up * 1.5f).normalized;
        rb.AddForce(direction * force, ForceMode.Impulse);  //ForceMode.Impulse 冲击力
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (rockStates)
        {
            case RockStates.HitPlayer:
                if (collision.gameObject.CompareTag("Player"))
                {
                    collision.gameObject.GetComponent<NavMeshAgent>().isStopped = true;
                    collision.gameObject.GetComponent<NavMeshAgent>().autoBraking = false;
                    collision.gameObject.GetComponent<NavMeshAgent>().velocity = direction * force * 0.3f;
                    collision.gameObject.GetComponent<Animator>().SetTrigger("Dizzy");
                    collision.gameObject.GetComponent<CharacterStats>().TakeDamage(damage, collision.gameObject.GetComponent<CharacterStats>());

                    rockStates = RockStates.HitNothing;
                }
                break;

            case RockStates.HitEnemy:
                if (collision.gameObject.CompareTag("Enemy"))
                {
                    var collisionStats = collision.gameObject.GetComponent<CharacterStats>();
                    collisionStats.TakeDamage(damage, collisionStats);
                    Instantiate(breakEffect, transform.position, Quaternion.identity);

                    Destroy(gameObject);
                }
                break;

            case RockStates.HitNothing:

                break;
        }
    }
}
