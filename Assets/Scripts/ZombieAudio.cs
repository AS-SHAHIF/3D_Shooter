using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ZombieAudio : MonoBehaviour
{
    private AudioSource source;
    private AudioSource loopSource;

    private void Awake()
    {
        source = GetComponent<AudioSource>();

        // Second source so a loop is never cut off by a hurt/attack one-shot
        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.playOnAwake = false;
        loopSource.loop = true;
        loopSource.spatialBlend = source.spatialBlend;
        loopSource.rolloffMode = source.rolloffMode;
        loopSource.minDistance = source.minDistance;
        loopSource.maxDistance = source.maxDistance;
        loopSource.dopplerLevel = source.dopplerLevel;

        // Reuse the sfx mixer group so volume settings still apply
        if (SoundManager.Instance != null && SoundManager.Instance.zombieChannel2 != null)
        {
            var group = SoundManager.Instance.zombieChannel2.outputAudioMixerGroup;
            source.outputAudioMixerGroup = group;
            loopSource.outputAudioMixerGroup = group;
        }
    }

    public void PlayHurt() { PlayOneShot(SoundManager.Instance != null ? SoundManager.Instance.zombieHurt : null); }
    public void PlayDeath() { PlayOneShot(SoundManager.Instance != null ? SoundManager.Instance.zombieDeath : null); }
    public void PlayAttack() { PlayOneShot(SoundManager.Instance != null ? SoundManager.Instance.zombieAttack : null); }

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
