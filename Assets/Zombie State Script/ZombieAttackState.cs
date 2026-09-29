using UnityEngine;
using UnityEngine.AI;

public class ZombieAttackState : StateMachineBehaviour
{
    private Transform player;
    private NavMeshAgent agent;
    public float stopAttackingDistance = 2.5f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        agent = animator.GetComponent<NavMeshAgent>();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (SoundManager.Instance != null && SoundManager.Instance.zombieChannel != null && SoundManager.Instance.zombieAttack != null)
        {
            if (!SoundManager.Instance.zombieChannel.isPlaying)
            {
                SoundManager.Instance.zombieChannel.PlayOneShot(SoundManager.Instance.zombieAttack);
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
        if (SoundManager.Instance != null && SoundManager.Instance.zombieChannel != null)
        {
            SoundManager.Instance.zombieChannel.Stop();
        }
    }
}
