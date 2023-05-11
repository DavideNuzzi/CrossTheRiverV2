using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class SoundInstance : MonoBehaviour
{
    public AudioClip clip;
    private AudioSource source;

    void Start()
    {
        source = GetComponent<AudioSource>();
        source.clip = clip;
        source.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (source.isPlaying == false)
        {
            Destroy(gameObject);
        }
    }
}
