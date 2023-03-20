using UnityEngine.Audio;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public SoundClip[] soundClips;  // Array of SoundClips that holds all sounds available in the game

    ///////////////////

    // This method assigns the sound settings specified in the Inspector to each SoundClip in the array
    void Awake()
    {
        foreach (SoundClip s in soundClips)
        {
            
            s.source = gameObject.AddComponent<AudioSource>();  // Add an AudioSource component to the game object and assign it to the current SoundClip
            s.source.clip = s.audioClip;                        // Assign the AudioClip to the AudioSource of the current SoundClip
            s.source.outputAudioMixerGroup = s.mixerGroup;      // Assign the AudioMixerGroup to the AudioSource of the current SoundClip
            s.source.volume = s.volume;                         // Assign the volume to the AudioSource of the current SoundClip
            s.source.pitch = s.pitch;                           // Assign the pitch to the AudioSource of the current SoundClip
        }
    }

    // This method plays a sound by finding the SoundClip in the array that matches the given name
    public void Play(string name)
    {
        // Find the SoundClip with the given name in the array of SoundClips
        SoundClip s = Array.Find(soundClips, soundclip => soundclip.name == name);

        // If there is no SoundClip with the given name in the array, return without playing anything
        if (s == null)
            return;

        // Play the AudioClip of the SoundClip found
        s.source.Play();
    }

    // Usage: FindObjectOfType<AudioManager>().Play("name_of_sound");
}