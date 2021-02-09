using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // Static Properties
    public static SoundManager Instance;
    
    // Serialized Properties
    public AudioClip dialogAccept;
    public AudioClip dialogStart;
    public AudioClip pageTurn;
    public GameObject audioListener;

    
    // Private Properties
    private AudioSource _overworldSource;
    private bool _isMuted;

    private void Awake()
    {
        Instance = this;
    }

    public void PlaySound(AudioClip clip, float volume = 1.0f, bool shouldLoop = false)
    {
        if (_isMuted || clip == null)
        {
            return;
        }

        var source = audioListener.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.Play();
        source.loop = shouldLoop;

        if (!shouldLoop)
        {
            StartCoroutine(_DestroyAfterPlay(source));
        }
    }

    private IEnumerator _DestroyAfterPlay(AudioSource source)
    {
        yield return new WaitForSeconds(source.clip.length);
        Destroy(source);
    }
}
