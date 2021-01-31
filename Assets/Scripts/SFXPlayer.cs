using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXPlayer : MonoBehaviour
{
    [SerializeField] AudioClip KillSound;

    static Dictionary<string, AudioSource> soundNames;

    private void Awake()
    {
        soundNames = new Dictionary<string, AudioSource>();

        //Kill Sound
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = KillSound;
        audioSource.volume = 0.5f;
        soundNames["Kill"] = audioSource;
    }

    public static void PlaySound(string name)
    {
        soundNames[name].Play();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            PlaySound("Kill");
        }
    }
}
