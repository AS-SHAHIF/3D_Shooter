using UnityEngine;
using static Weapon;

public class SoundManager : MonoBehaviour
{
    public AudioSource ShootingChannel;
    

    public AudioClip m16Shot;
    public AudioClip pistolShot;

    public AudioSource reloadingSoundpistol;
    public AudioSource reloadingSoundM16;

    public AudioSource empty_pistol_sound;

    public AudioSource throwableChannel;
    public AudioClip grenadeSound;

    public AudioClip zombieWalking;
    public AudioClip zombieChase;
    public AudioClip zombieAttack;
    public AudioClip zombieHurt;
    public AudioClip zombieDeath;
    public AudioSource zombieChannel;
    public AudioSource zombieChannel2;

    public AudioSource playerChannel;
    public AudioClip playerHurt;
    public AudioClip playerDie;
    public AudioClip gameOverMusic;

    public static SoundManager Instance { get; set; }
    // public GameObject bulletImpactEffectPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }


    public void PlayShootingSound(WeaponModel weapon)
    {
        switch (weapon)
        {
            case WeaponModel.pistol:
                ShootingChannel.PlayOneShot(pistolShot);
                break;
            case WeaponModel.m16:
                ShootingChannel.PlayOneShot(m16Shot);
                break;

        }
    }

    public void PlayReloadSound(WeaponModel weapon)
    {
        switch (weapon)
        {
            case WeaponModel.pistol:
                reloadingSoundpistol.Play();
                break;
            case WeaponModel.m16:
                reloadingSoundM16.Play();
                break;
        }
    }
}
