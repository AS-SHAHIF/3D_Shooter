using UnityEngine;
using UnityEngine.Audio;
using static Weapon;

public class SoundManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Music")]
    public AudioSource musicChannel;
    public AudioClip gameplayMusic;

    [Header("Shooting & Reloading")]
    public AudioSource ShootingChannel;
    public AudioClip m16Shot;
    public AudioClip pistolShot;

    public AudioSource reloadingSoundpistol;
    public AudioSource reloadingSoundM16;
    public AudioSource empty_pistol_sound;

    [Header("Throwables")]
    public AudioSource throwableChannel;
    public AudioClip grenadeSound;

    [Header("Zombies")]
    public AudioClip zombieWalking;
    public AudioClip zombieChase;
    public AudioClip zombieAttack;
    public AudioClip zombieHurt;
    public AudioClip zombieDeath;
    public AudioSource zombieChannel;
    public AudioSource zombieChannel2;

    [Header("Player")]
    public AudioSource playerChannel;
    public AudioClip playerHurt;
    public AudioClip playerDie;
    public AudioClip gameOverMusic;

    [Header("Pickups")]
    public AudioClip crystalPickupSound;
    public AudioClip ammoPickupSound;

    public static SoundManager Instance { get; set; }

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

        AssignMixerGroups();
    }

    private void Start()
    {
        PlayGameplayMusic();
    }

    public void AssignMixerGroups()
    {
        if (audioMixer == null)
        {
            audioMixer = Resources.Load<AudioMixer>("MainAudioMixer");
        }

        if (audioMixer != null)
        {
            AudioMixerGroup[] sfxGroups = audioMixer.FindMatchingGroups("sfx");
            AudioMixerGroup[] musicGroups = audioMixer.FindMatchingGroups("music");

            AudioMixerGroup sfxGroup = (sfxGroups != null && sfxGroups.Length > 0) ? sfxGroups[0] : null;
            AudioMixerGroup musicGroup = (musicGroups != null && musicGroups.Length > 0) ? musicGroups[0] : null;

            if (sfxGroup != null)
            {
                if (ShootingChannel != null && ShootingChannel.outputAudioMixerGroup == null) ShootingChannel.outputAudioMixerGroup = sfxGroup;
                if (reloadingSoundpistol != null && reloadingSoundpistol.outputAudioMixerGroup == null) reloadingSoundpistol.outputAudioMixerGroup = sfxGroup;
                if (reloadingSoundM16 != null && reloadingSoundM16.outputAudioMixerGroup == null) reloadingSoundM16.outputAudioMixerGroup = sfxGroup;
                if (empty_pistol_sound != null && empty_pistol_sound.outputAudioMixerGroup == null) empty_pistol_sound.outputAudioMixerGroup = sfxGroup;
                if (throwableChannel != null && throwableChannel.outputAudioMixerGroup == null) throwableChannel.outputAudioMixerGroup = sfxGroup;
                if (zombieChannel != null && zombieChannel.outputAudioMixerGroup == null) zombieChannel.outputAudioMixerGroup = sfxGroup;
                if (zombieChannel2 != null && zombieChannel2.outputAudioMixerGroup == null) zombieChannel2.outputAudioMixerGroup = sfxGroup;
                if (playerChannel != null && playerChannel.outputAudioMixerGroup == null) playerChannel.outputAudioMixerGroup = sfxGroup;
            }

            if (musicGroup != null)
            {
                if (musicChannel != null && musicChannel.outputAudioMixerGroup == null) musicChannel.outputAudioMixerGroup = musicGroup;
            }
        }
    }

    public void PlayGameplayMusic()
    {
        if (musicChannel != null && gameplayMusic != null)
        {
            musicChannel.clip = gameplayMusic;
            musicChannel.loop = true;
            if (!musicChannel.isPlaying)
            {
                musicChannel.Play();
            }
        }
    }

    public void StopGameplayMusic()
    {
        if (musicChannel != null)
        {
            musicChannel.Stop();
        }
    }

    public void PlayShootingSound(WeaponModel weapon)
    {
        if (ShootingChannel == null) return;

        switch (weapon)
        {
            case WeaponModel.pistol:
                if (pistolShot != null) ShootingChannel.PlayOneShot(pistolShot);
                break;
            case WeaponModel.m16:
                if (m16Shot != null) ShootingChannel.PlayOneShot(m16Shot);
                break;
        }
    }

    public void PlayReloadSound(WeaponModel weapon)
    {
        switch (weapon)
        {
            case WeaponModel.pistol:
                if (reloadingSoundpistol != null) reloadingSoundpistol.Play();
                break;
            case WeaponModel.m16:
                if (reloadingSoundM16 != null) reloadingSoundM16.Play();
                break;
        }
    }

    public void PlayCrystalPickupSound(AudioClip customClip = null)
    {
        AudioClip clipToPlay = customClip != null ? customClip : crystalPickupSound;
        if (clipToPlay != null && playerChannel != null)
        {
            playerChannel.PlayOneShot(clipToPlay);
        }
    }

    public void PlayAmmoPickupSound(AudioClip customClip = null)
    {
        AudioClip clipToPlay = customClip != null ? customClip : ammoPickupSound;
        if (clipToPlay != null && playerChannel != null)
        {
            playerChannel.PlayOneShot(clipToPlay);
        }
    }
}
