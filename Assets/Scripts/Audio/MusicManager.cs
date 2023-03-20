using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public PlayerController player;
    public float playerPositionY;
    public DayNightCycle dayNight;
    public AudioSource surfaceTrackOverlay;
    public AudioSource playingSurfaceTrack;
    [Range(0f, 1f)] public float surfaceTrackVolume;
    public AudioSource playingCaveTrack;
    [Range(0f, 1f)] public float caveTrackVolume;
    public AudioClip[] musicTracks_Day;
    public AudioClip[] musicTracks_Night;
    public AudioClip[] musicTracks_Cave;

    ///////////////////

    void Start()
    {
        player = PlayerController.FindObjectOfType<PlayerController>();
        dayNight = DayNightCycle.FindObjectOfType<DayNightCycle>();
    }

    void Update()
    {
        // IF there is no surfaceTrack currently playing
        if (!playingSurfaceTrack.isPlaying)
        {
            // IF it's day time in game
            if (dayNight.dayTime == true)
            {
                // Select a random track from the array of day time tracks
                playingSurfaceTrack.clip = musicTracks_Day[UnityEngine.Random.Range(0, musicTracks_Day.Length)];
                // Play the random track
                playingSurfaceTrack.Play();
            }
            // ELSE it's night time in game
            else
            {
                // Select a random track from the array of night time tracks
                playingSurfaceTrack.clip = musicTracks_Night[UnityEngine.Random.Range(0, musicTracks_Night.Length)];
                // Play the random track
                playingSurfaceTrack.Play();
            }
        }
        // IF there is no caveTrack currently playing
        if (!playingCaveTrack.isPlaying)
        {
            // Select a random track from the array of cave tracks
            playingCaveTrack.clip = musicTracks_Cave[UnityEngine.Random.Range(0, musicTracks_Cave.Length)];
            // Play the random track
            playingCaveTrack.Play();
        }

        // Get Player's Y position in world
        playerPositionY = player.transform.position.y;

        // IF Player is more than 100 tiles high
        if (playerPositionY > 100)
        {
            // surfaceTrackVolume at max
            surfaceTrackVolume = 0.5f;
        }
        // ELSE IF Player is between tiles 90 and 100
        else if (playerPositionY >= 90f && playerPositionY <= 100)
        {
            // Adjust surfaceTrackVolume based on Player's Y position in world
            surfaceTrackVolume = (playerPositionY - 90) / 20;
        }
        // ELSE IF Player is less than 90 tiles high
        else if (playerPositionY < 90)
        {
            // surfaceTrackVolume at lowest / muted
            surfaceTrackVolume = 0.0f;
        }

        // IF Player is more than 95 tiles high
        if (playerPositionY > 95)
        {
            // caveTrackVolume at lowest / muted
            caveTrackVolume = 0.0f;
        }
        // ELSE IF Player is between tiles 85 and 95
        else if (playerPositionY >= 85f && playerPositionY <= 95)
        {
            // Adjust caveTrackVolume based on the Player's Y position in world
            caveTrackVolume = 0.5f - (playerPositionY - 85) / 20;
        }
        // ELSE IF Player is less than 85 tiles high
        else if (playerPositionY < 85)
        {
            // caveTrackVolume at max
            caveTrackVolume = 0.5f;
        }

        // Assign volume level from proper IF statement to the volumes of the Audio Sources playing music
        playingSurfaceTrack.volume = surfaceTrackVolume;
        // Ambient volume will always be 1/4 the volume of the surfaceTrackVolume
        surfaceTrackOverlay.volume = surfaceTrackVolume / 4;
        // Assign volume level from proper IF statement to the volumes of the Audio Sources playing music
        playingCaveTrack.volume = caveTrackVolume;
    }
}