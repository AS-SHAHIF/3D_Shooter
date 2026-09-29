using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombiePatrolingState : StateMachineBehaviour
{
    private float timer = 0f;
    public float patrolingTime = 10f;
    private Transform player;
    private NavMeshAgent agent;
    public float detectionAreaRadius = 18f;
    public float patrolSpeed = 2f;
    private readonly List<Transform> waypointsList = new List<Transform>();

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer = 0;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        agent = animator.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = patrolSpeed;
        }

        waypointsList.Clear();
        GameObject waypointCluster = GameObject.FindGameObjectWithTag("Waypoints");
        if (waypointCluster != null)
        {
            foreach (Transform t in waypointCluster.transform)
            {
                waypointsList.Add(t);
            }
        }

        if (agent != null && agent.isOnNavMesh && waypointsList.Count > 0)
        {
            Vector3 nextPosition = waypointsList[Random.Range(0, waypointsList.Count)].position;
            agent.SetDestination(nextPosition);
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Sound check with full null safety
        if (SoundManager.Instance != null && SoundManager.Instance.zombieChannel != null && SoundManager.Instance.zombieWalking != null)
        {
            if (!SoundManager.Instance.zombieChannel.isPlaying)
            {
                SoundManager.Instance.zombieChannel.clip = SoundManager.Instance.zombieWalking;
                SoundManager.Instance.zombieChannel.PlayDelayed(1f);
            }
        }

        // Move to next waypoint if arrived
        if (agent != null && agent.isOnNavMesh && waypointsList.Count > 0)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                agent.SetDestination(waypointsList[Random.Range(0, waypointsList.Count)].position);
            }
        }

        // Transition to Idle State
        timer += Time.deltaTime;
        if (timer > patrolingTime)
        {
            animator.SetBool("isPatroling", false);
        }

        // Transition to Chase State
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
            float distanceFromPlayer = Vector3.Distance(player.position, animator.transform.position);
            if (distanceFromPlayer < detectionAreaRadius)
            {
                animator.SetBool("isChasing", true);
            }
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(agent.transform.position);
        }

        if (SoundManager.Instance != null && SoundManager.Instance.zombieChannel != null)
        {
            SoundManager.Instance.zombieChannel.Stop();
        }
    }
}
