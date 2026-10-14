using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int HP = 100;
    [SerializeField] private ZombieHealthBar healthBar;
    private ZombieAudio zombieAudio;
    private int maxHP;
    private Animator animator;
    private NavMeshAgent navAgent;
    public bool isDead;

    private void Start()
    {
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        zombieAudio = GetComponent<ZombieAudio>();
        maxHP = HP;
        if (healthBar != null) healthBar.SetHealth(HP, maxHP);
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        HP -= damageAmount;

        if (healthBar != null) healthBar.SetHealth(Mathf.Max(0, HP), maxHP);
        if (HP <= 0)
        {
            isDead = true;
            if (healthBar != null) healthBar.Hide();

                // Roll loot drops (ammo, crystals)
            GetComponent<ZombieLootDropper>()?.TryDrop();

            // Award Run XP per zombie kill
            XPSystem.AddRunXP(10);

            int randomValue = Random.Range(0, 2);
            if (animator != null)
            {
                if (randomValue == 0)
                {
                    animator.SetTrigger("DIE1");
                }
                else
                {
                    animator.SetTrigger("DIE2");
                }
            }

            // Stop and disable NavMeshAgent so dead zombie stops moving
            if (navAgent != null && navAgent.isActiveAndEnabled && navAgent.isOnNavMesh)
            {
                navAgent.isStopped = true;
                navAgent.enabled = false;
            }

            // Disable colliders so bullets and player don't get blocked by dead body
            foreach (Collider col in GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }

            // if (SoundManager.Instance != null && SoundManager.Instance.zombieChannel2 != null && SoundManager.Instance.zombieDeath != null)
            // {
            //     SoundManager.Instance.zombieChannel2.PlayOneShot(SoundManager.Instance.zombieDeath);
            // }
            if (zombieAudio != null)
            {
                zombieAudio.StopLoop();   // stop walking/chase sound
                zombieAudio.PlayDeath();
            }

            // Destroy body after death animation finishes
            Destroy(gameObject, 5f);
        }
        else
        {
            if (animator != null)
            {
                animator.SetTrigger("DAMAGE");
            }
            // if (SoundManager.Instance != null && SoundManager.Instance.zombieChannel2 != null && SoundManager.Instance.zombieHurt != null)
            // {
            //     SoundManager.Instance.zombieChannel2.PlayOneShot(SoundManager.Instance.zombieHurt);
            // }
            if (zombieAudio != null) zombieAudio.PlayHurt();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 2.5f); // Attacking

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 18f); // Detection - Start Chasing

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 21f); // Stop Chasing
    }
}
