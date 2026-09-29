using UnityEngine;

public class ZombieIdleState : StateMachineBehaviour
{
    private float timer;
    public float idleTime = 0f;
    private Transform player;
    public float detectionAreaRadius = 18f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer = 0f;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
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

        // Transition to Patrol State
        timer += Time.deltaTime;
        if (timer > idleTime)
        {
            animator.SetBool("isPatroling", true);
        }

        // Transition to Chase State
        if (player != null)
        {
            float distanceFromPlayer = Vector3.Distance(player.position, animator.transform.position);
            if (distanceFromPlayer < detectionAreaRadius)
            {
                animator.SetBool("isChasing", true);
            }
        }
    }
}
