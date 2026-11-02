using UnityEngine;
using UnityEngine.AI;

public class ZombieChaseState : StateMachineBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    public float chaseSpeed = 6f;
    public float stopChasingDistance = 21f;
    public float attackingDiatance = 2.5f;
    private ZombieAudio zombieAudio;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        agent = animator.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = chaseSpeed;
        }

        zombieAudio = animator.GetComponent<ZombieAudio>();
        if (zombieAudio != null)
        {
            zombieAudio.PlayChase();
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        if (player != null)
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.SetDestination(player.position);
            }

            Vector3 lookDirection = player.position - animator.transform.position;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                animator.transform.rotation = Quaternion.LookRotation(lookDirection);
            }

            float distanceFromPlayer = Vector3.Distance(player.position, animator.transform.position);
            if (distanceFromPlayer > stopChasingDistance)
            {
                animator.SetBool("isChasing", false);
            }
            if (distanceFromPlayer < attackingDiatance)
            {
                animator.SetBool("isAttacking", true);
            }
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(animator.transform.position);
        }

        if (zombieAudio != null)
        {
            zombieAudio.StopLoop();
        }
    }
}
