using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class Golem : EnemyController
{
    [Header("Skill")]
    public float kickForce = 25;
    public GameObject rockProfab;
    public Transform handTrans;

    //Animation Event
    public void KickOff()
    {
        if (attackTarget!=null)
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();

            Vector3 direction = (attackTarget.transform.position - transform.position).normalized;
            //direction.Normalize();

            targetStats.GetComponent<NavMeshAgent>().isStopped = true;
            targetStats.GetComponent<NavMeshAgent>().autoBraking = false;

            targetStats.GetComponent<NavMeshAgent>().velocity = direction * kickForce;
            targetStats.GetComponent<Animator>().SetTrigger("Dizzy");
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    //Animation Event
    public void ThrowRock()
    {
        if (attackTarget != null)
        {
            CreateRock();
        }
        else
        {
            attackTarget = FindObjectOfType<PlayerController>().gameObject;
            CreateRock();
        }
    }

    public void CreateRock()
    {
        var rock = Instantiate(rockProfab, handTrans.position, Quaternion.identity);
        rock.GetComponent<Rock>().target = attackTarget;
    }
}
