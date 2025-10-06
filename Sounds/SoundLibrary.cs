using UnityEngine;

[System.Serializable]
public struct SoundEffect
{
    public string groupID;
    public AudioClip[] clips;
    public bool loop; // Added loop flag
}

public class SoundLibrary : MonoBehaviour
{
    public SoundEffect[] soundEffects;

    public AudioClip GetClipFromName(string name, out bool shouldLoop)
    {
        foreach (var soundEffect in soundEffects)
        {
            if (soundEffect.groupID == name)
            {
                shouldLoop = soundEffect.loop;
                return soundEffect.clips[Random.Range(0, soundEffect.clips.Length)];
            }
        }
        shouldLoop = false;
        return null;
    }
}
