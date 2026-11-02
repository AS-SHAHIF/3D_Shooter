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
    public AudioClip sniperShot;

    public AudioSource reloadingSoundpistol;
    public AudioSource reloadingSoundM16;
    public AudioSource reloadingSoundSniper;
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
    public AudioClip playerWalking;
    public AudioClip gameOverMusic;
    [Tooltip("Playback speed / pitch of footsteps when walking")]
    public float walkFootstepPitch = 1.0f;
    [Tooltip("Playback speed / pitch of footsteps when running")]
    public float runFootstepPitch = 1.5f;

    [Header("Pickups")]
    public AudioClip crystalPickupSound;
    public AudioClip ammoPickupSound;

    [Header("UI / Popup")]
    public AudioSource uiChannel;                // 2D sound for UI/popup/button clicks
    public AudioClip popupOpenSound;
    public AudioClip popupCloseSound;
    public AudioClip buttonClickSound;

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

        // Ensure a dedicated 2D channel exists for UI / popup / button sounds so
        // they are audible even while the game is paused (popups, death, menus).
        if (uiChannel == null)
        {
            uiChannel = gameObject.AddComponent<AudioSource>();
            uiChannel.playOnAwake = false;
            uiChannel.loop = false;
            uiChannel.spatialBlend = 0f;
        }
        uiChannel.ignoreListenerPause = true;

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
            // Auto-detect mixer from existing channels if already assigned in Inspector
            if (ShootingChannel != null && ShootingChannel.outputAudioMixerGroup != null)
            {
                audioMixer = ShootingChannel.outputAudioMixerGroup.audioMixer;
            }
            else
            {
                audioMixer = Resources.Load<AudioMixer>("MainAudioMixer");
            }
        }

        if (audioMixer != null)
        {
            // Case-insensitive search for both sfx and music mixer groups
            AudioMixerGroup[] sfxGroups = audioMixer.FindMatchingGroups("sfx");
            if (sfxGroups == null || sfxGroups.Length == 0) sfxGroups = audioMixer.FindMatchingGroups("SFX");

            AudioMixerGroup[] musicGroups = audioMixer.FindMatchingGroups("Music");
            if (musicGroups == null || musicGroups.Length == 0) musicGroups = audioMixer.FindMatchingGroups("music");

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
                if (uiChannel != null && uiChannel.outputAudioMixerGroup == null) uiChannel.outputAudioMixerGroup = sfxGroup;
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
            case WeaponModel.sniper:
                AudioClip sClip = sniperShot != null ? sniperShot : m16Shot;
                if (sClip != null) ShootingChannel.PlayOneShot(sClip);
                break;
        }
    }

    public void PlayReloadSound(WeaponModel weapon, float pitch = 1.0f)
    {
        switch (weapon)
        {
            case WeaponModel.pistol:
                if (reloadingSoundpistol != null)
                {
                    reloadingSoundpistol.pitch = pitch;
                    reloadingSoundpistol.Play();
                }
                break;
            case WeaponModel.m16:
                if (reloadingSoundM16 != null)
                {
                    reloadingSoundM16.pitch = pitch;
                    reloadingSoundM16.Play();
                }
                break;
            case WeaponModel.sniper:
                AudioSource sRel = reloadingSoundSniper != null ? reloadingSoundSniper : reloadingSoundM16;
                if (sRel != null)
                {
                    sRel.pitch = pitch;
                    sRel.Play();
                }
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

    public void PlayPlayerWalking(bool isWalking, bool isRunning = false)
    {
        if (playerChannel == null || playerWalking == null) return;

        if (isWalking)
        {
            // Adjust pitch/speed dynamically based on walking vs running
            playerChannel.pitch = isRunning ? runFootstepPitch : walkFootstepPitch;

            // Start the walking loop (restart if a different clip was playing).
            if (!playerChannel.isPlaying || playerChannel.clip != playerWalking)
            {
                playerChannel.clip = playerWalking;
                playerChannel.loop = true;
                playerChannel.Play();
            }
        }
        else
        {
            // Stop the walking loop only if the walking clip is what's playing.
            if (playerChannel.isPlaying && playerChannel.clip == playerWalking)
            {
                playerChannel.Stop();
                playerChannel.loop = false;
                playerChannel.clip = null;
                playerChannel.pitch = 1.0f;
            }
        }
    }

    public void StopPlayerWalking()
    {
        if (playerChannel != null && playerChannel.isPlaying && playerChannel.clip == playerWalking)
        {
            playerChannel.Stop();
            playerChannel.loop = false;
            playerChannel.clip = null;
            playerChannel.pitch = 1.0f;
        }
    }

    public void PlayPopupOpen()
    {
        PlayUISound(uiChannel, popupOpenSound);
    }

    public void PlayPopupClose()
    {
        PlayUISound(uiChannel, popupCloseSound);
    }

    public void PlayButtonClick()
    {
        PlayUISound(uiChannel, buttonClickSound);
    }

    private void PlayUISound(AudioSource source, AudioClip clip)
    {
        if (source != null && clip != null)
        {
            source.PlayOneShot(clip);
        }
    }
}
