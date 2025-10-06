using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField]
    private SoundLibrary sfxLibrary;
    [SerializeField]
    private AudioSource sfx2DSource;  // For looping sounds (walking)
    [SerializeField]
    private AudioSource sfxOneShotSource; // For one-time sounds (jumping)

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

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

    public void PlaySound2D(string soundName)
    {
        bool shouldLoop;
        AudioClip clip = sfxLibrary.GetClipFromName(soundName, out shouldLoop);

        if (clip != null)
        {
            if (shouldLoop)
            {
                // Play looping sounds (e.g., walking)
                if (sfx2DSource.clip != clip) // Avoid restarting the same sound
                {
                    sfx2DSource.clip = clip;
                    sfx2DSource.loop = true;
                    sfx2DSource.Play();
                }
            }
            else
            {
                // Play one-time sounds (e.g., jumping)
                sfxOneShotSource.PlayOneShot(clip);
            }
        }
    }

    public void StopSound2D()
    {
        if (sfx2DSource.isPlaying)
        {
            sfx2DSource.Stop();
            sfx2DSource.clip = null; // Ensure it resets properly
        }
    }
}