using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicSync : MonoBehaviour
{
    public static MusicSync sync = null;

    public GameObject sfx;

    [HideInInspector] public AudioSource audio;
    AudioSource newSound;

    // Start is called before the first frame update
    void Start()
    {
        audio = GetComponent<AudioSource>();
        if (sync == null)
        {
            sync = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); }
    }

    // Update is called once per frame
    void Update()
    {
        //print(audio.time);
    }

    public void PlayMusic(AudioClip music)
    {
        if (music != audio.clip)
        {
            audio.clip = music;
            audio.Play();
        }
    }

    public void PlaySound(AudioClip sound)
    {
        newSound = Instantiate(sfx).GetComponent<AudioSource>();
        newSound.clip = sound;
        newSound.Play();
    }

    public void ToggleMute() { audio.mute =  !audio.mute; }
}