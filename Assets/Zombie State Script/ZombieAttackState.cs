using UnityEngine;
using UnityEngine.AI;

public class ZombieAttackState : StateMachineBehaviour
{
    private Transform player;
    private NavMeshAgent agent;
    public float stopAttackingDistance = 2.5f;
    private ZombieAudio zombieAudio;
    private float attackAudioTimer = 0f;
    public float attackAudioInterval = 1.2f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        agent = animator.GetComponent<NavMeshAgent>();
        zombieAudio = animator.GetComponent<ZombieAudio>();
        attackAudioTimer = attackAudioInterval; // trigger attack sound immediately
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        attackAudioTimer += Time.deltaTime;
        if (attackAudioTimer >= attackAudioInterval)
        {
            attackAudioTimer = 0f;
            if (zombieAudio != null)
            {
                zombieAudio.PlayAttack();
            }
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        if (player != null && agent != null)
        {
            LookAtPlayer(animator);
            float distanceFromPlayer = Vector3.Distance(player.position, animator.transform.position);
            if (distanceFromPlayer > stopAttackingDistance)
            {
                animator.SetBool("isAttacking", false);
            }
        }
    }

    private void LookAtPlayer(Animator animator)
    {
        if (player == null) return;
        Vector3 direction = player.position - animator.transform.position;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            animator.transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }
}
