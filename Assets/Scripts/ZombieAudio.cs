using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ZombieAudio : MonoBehaviour
{
    private AudioSource source;
    private AudioSource loopSource;

    [Header("3D Audio Range Settings")]
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 20f; // Sounds can only be heard within 20 meters!

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        ConfigureSource(source);

        // Second source so a loop is never cut off by a hurt/attack one-shot
        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.playOnAwake = false;
        loopSource.loop = true;
        ConfigureSource(loopSource);

        // Reuse the sfx mixer group so volume settings still apply
        if (SoundManager.Instance != null && SoundManager.Instance.zombieChannel2 != null)
        {
            var group = SoundManager.Instance.zombieChannel2.outputAudioMixerGroup;
            source.outputAudioMixerGroup = group;
            loopSource.outputAudioMixerGroup = group;
        }
    }

    private void ConfigureSource(AudioSource src)
    {
        if (src == null) return;
        src.spatialBlend = 1f; // 100% 3D sound
        src.rolloffMode = AudioRolloffMode.Linear;
        src.minDistance = minDistance;
        src.maxDistance = maxDistance;
        src.dopplerLevel = 0f;
    }

    public void PlayWalking()
    {
        if (SoundManager.Instance != null && SoundManager.Instance.zombieWalking != null)
        {
            PlayLoop(SoundManager.Instance.zombieWalking);
        }
    }

    public void PlayChase()
    {
        if (SoundManager.Instance != null && SoundManager.Instance.zombieChase != null)
        {
            PlayLoop(SoundManager.Instance.zombieChase);
        }
    }

    public void PlayHurt()
    {
        PlayOneShot(SoundManager.Instance != null ? SoundManager.Instance.zombieHurt : null);
    }

    public void PlayDeath()
    {
        StopLoop();
        PlayOneShot(SoundManager.Instance != null ? SoundManager.Instance.zombieDeath : null);
    }

    public void PlayAttack()
    {
        PlayOneShot(SoundManager.Instance != null ? SoundManager.Instance.zombieAttack : null);
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip != null && source != null) source.PlayOneShot(clip);
    }

    public void PlayLoop(AudioClip clip)
    {
        if (loopSource == null || clip == null) return;
        if (loopSource.clip == clip && loopSource.isPlaying) return;
        loopSource.clip = clip;
        loopSource.Play();
    }

    public void StopLoop()
    {
        if (loopSource != null) loopSource.Stop();
    }
}
