using UnityEngine;

public class MainMenuMusic : MonoBehaviour
{
    public AudioSource playingTrack;            // Reference to the main music track
    public AudioSource playingTrackOverlay;     // Reference to the overlay music track
    public AudioClip[] musicTracks_Main;        // Array of main music tracks
    public AudioClip[] musicTracks_Overlay;     // Array of overlay music tracks

    ///////////////////

    void Update()
    {
        // IF the main music track is not playing
        if (!playingTrack.isPlaying)
        {
            // Select a random main music track from the array and play it
            playingTrack.clip = musicTracks_Main[UnityEngine.Random.Range(0, musicTracks_Main.Length)];
            playingTrack.Play();
        }
        // IF the overlay music track is not playing
        if (!playingTrackOverlay.isPlaying)
        {
            // Select a random overlay music track from the array and play it
            playingTrackOverlay.clip = musicTracks_Overlay[UnityEngine.Random.Range(0, musicTracks_Overlay.Length)];
            playingTrackOverlay.Play();
        }
    }
}