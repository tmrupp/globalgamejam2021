using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXPlayer : MonoBehaviour
{
    [SerializeField] AudioClip KillSound = null;
    [SerializeField] AudioClip Success = null;
    [SerializeField] AudioClip Failure = null;

    static Dictionary<string, AudioSource> soundNames;

    private void Awake()
    {
        soundNames = new Dictionary<string, AudioSource>();

        //Kill Sound
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = KillSound;
        audioSource.volume = 0.5f;
        soundNames["Kill"] = audioSource;

        //Success Sound
        AudioSource audioSource2 = gameObject.AddComponent<AudioSource>();
        audioSource2.clip = Success;
        audioSource2.volume = 1f;
        soundNames["Success"] = audioSource2;

        //Failure Sound
        AudioSource audioSource3 = gameObject.AddComponent<AudioSource>();
        audioSource3.clip = Failure;
        audioSource3.volume = 1f;
        soundNames["Failure"] = audioSource3;
    }

    public static void PlaySound(string name)
    {
        soundNames[name].Play();
    }
}
