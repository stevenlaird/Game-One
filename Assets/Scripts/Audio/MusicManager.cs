using UnityEngine;

public class MusicManager : MonoBehaviour
{
    // References to PlayerController and DayNightCycle scripts
    private PlayerController player;
    private DayNightCycle dayNight;

    // Field to store the Player's tile height in the world
    public float playerPositionY;

    // Audio sources and volume for the music tracks
    public AudioSource surfaceTrackOverlay;
    public AudioSource playingSurfaceTrack;
    [Range(0f, 1f)] public float surfaceTrackVolume;
    public AudioSource playingCaveTrack;
    [Range(0f, 1f)] public float caveTrackVolume;

    // Arrays of music tracks for different times and places in the game
    public AudioClip[] musicTracks_Day;
    public AudioClip[] musicTracks_Night;
    public AudioClip[] musicTracks_Cave;

    ///////////////////

    void Start()
    {
        // Find and assign the PlayerController and DayNightCycle objects in the scene
        player = PlayerController.FindObjectOfType<PlayerController>();
        dayNight = DayNightCycle.FindObjectOfType<DayNightCycle>();
    }

    void Update()
    {
        
        if (!playingSurfaceTrack.isPlaying)                         // IF there is no surfaceTrack currently playing
        {
            if (dayNight.dayTime == true)                                   // IF it's day time in game
            {
                // Select a random track from the array of day time tracks
                playingSurfaceTrack.clip = musicTracks_Day[UnityEngine.Random.Range(0, musicTracks_Day.Length)];
                // Play the random track
                playingSurfaceTrack.Play();
            }
            else                                                            // ELSE it's night time in game
            {
                // Select a random track from the array of night time tracks
                playingSurfaceTrack.clip = musicTracks_Night[UnityEngine.Random.Range(0, musicTracks_Night.Length)];
                // Play the random track
                playingSurfaceTrack.Play();
            }
        }
        if (!playingCaveTrack.isPlaying)                            // IF there is no caveTrack currently playing
        {
            // Select a random track from the array of cave tracks
            playingCaveTrack.clip = musicTracks_Cave[UnityEngine.Random.Range(0, musicTracks_Cave.Length)];
            // Play the random track
            playingCaveTrack.Play();
        }

        
        playerPositionY = player.transform.position.y;              // Get Player's Y position in world

        if (playerPositionY > 100)                                  // IF Player is more than 100 tiles high
        {            
            surfaceTrackVolume = 0.5f;                                      // Set surfaceTrackVolume to max
        }        
        else if (playerPositionY >= 90f && playerPositionY <= 100)  // ELSE IF Player is between tiles 90 and 100
        {            
            surfaceTrackVolume = (playerPositionY - 90) / 20;               // Adjust surfaceTrackVolume based on Player's Y position in world
        }        
        else if (playerPositionY < 90)                              // ELSE IF Player is less than 90 tiles high
        {            
            surfaceTrackVolume = 0.0f;                                      // Set surfaceTrackVolume to lowest / muted
        }

        if (playerPositionY > 95)                                   // IF Player is more than 95 tiles high
        {
            caveTrackVolume = 0.0f;                                         // Set caveTrackVolume to lowest / muted
        }
        else if (playerPositionY >= 85f && playerPositionY <= 95)   // ELSE IF Player is between tiles 85 and 95
        {
            caveTrackVolume = 0.5f - (playerPositionY - 85) / 20;           // Adjust caveTrackVolume based on the Player's Y position in world
        }
        else if (playerPositionY < 85)                              // ELSE IF Player is less than 85 tiles high
        {
            caveTrackVolume = 0.5f;                                         // Set caveTrackVolume to max
        }

        // Assign volume level from proper IF statement to the volumes of the Audio Sources playing music
        playingSurfaceTrack.volume = surfaceTrackVolume;
        // Ambient volume will always be 1/4 the volume of the surfaceTrackVolume
        surfaceTrackOverlay.volume = surfaceTrackVolume / 4;
        // Assign volume level from proper IF statement to the volumes of the Audio Sources playing music
        playingCaveTrack.volume = caveTrackVolume;
    }
}