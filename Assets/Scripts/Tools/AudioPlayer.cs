using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] clips;

    public void Play(int clipIndex)
    {
        if (clips == null || clips.Length == 0)
        {
            return;
        }

        if (clipIndex < 0 || clipIndex >= clips.Length)
        {
            return;
        }
        
        var clip = clips[clipIndex];
        audioSource.PlayOneShot(clip);
    }
}
