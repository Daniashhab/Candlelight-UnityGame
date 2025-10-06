using UnityEngine;

public class StoneGolemSoundManager : MonoBehaviour
{
    [SerializeField]
    private SoundLibrary sfxLibrary;
    [SerializeField]
    private AudioSource sfxSource;

    public void PlaySound3D(AudioClip clip, Vector3 pos)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, pos);
        }
    }

    public void PlaySound3D(string soundName, Vector3 pos)
    {
        bool shouldLoop;
        AudioClip clip = sfxLibrary.GetClipFromName(soundName, out shouldLoop);
        PlaySound3D(clip, pos);
    }

    public void PlaySound(string soundName)
    {
        bool shouldLoop;
        AudioClip clip = sfxLibrary.GetClipFromName(soundName, out shouldLoop);

        if (clip != null)
        {
            if (shouldLoop)
            {
                if (sfxSource.clip != clip)
                {
                    sfxSource.clip = clip;
                    sfxSource.loop = true;
                    sfxSource.Play();
                }
            }
        }
    }

    public void StopSound()
    {
        if (sfxSource.isPlaying)
        {
            sfxSource.Stop();
            sfxSource.clip = null;
        }
    }
}
