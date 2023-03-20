using UnityEngine.Audio;
using UnityEngine;

// Serializable class for storing information about a sound clip
[System.Serializable]
public class SoundClip
{
    public string name;                             // Name of the sound clip

    public AudioClip audioClip;                     // Audio clip associated with the sound clip
    public AudioMixerGroup mixerGroup;              // AudioMixerGroup to which the sound clip belongs
    [HideInInspector] public AudioSource source;    // AudioSource component used to play the sound clip

    [Range(0f, 1f)] public float volume;            // Volume of the sound clip (range: 0 to 1)
    [Range(0.1f, 3f)] public float pitch;           // Pitch of the sound clip (range: 0.1 to 3)
}