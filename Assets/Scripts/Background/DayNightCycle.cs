using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightCycle : MonoBehaviour
{
    public TextMeshProUGUI timeDisplay;             // Text object displaying in-game time
    public TextMeshProUGUI dayDisplay;              // Text object displaying current in-game day
    public TextMeshProUGUI daysSurvivedDisplay;     // Text object displaying the number of days survived
    public Material stars;                          // Material used for the stars
    public Light2D sunlight;                        // 2D light representing the sun
    public GameObject sunMoon;                      // GameObject representing the sun and moon
    private float sunMoonRotation;                  // Current rotation angle for the sun/moon object

    public int sunRise = 5;                         // In-game hour when the sun rises
    public int sunSet = 16;                         // In-game hour when the sun sets
    public int starsAppear = 18;                    // In-game hour when the stars appear
    public int starsDisappear = 3;                  // In-game hour when the stars disappear

    public enum GameSpeed                           // Enumeration for different game speeds
    {
        Normal,
        Fast,
        SuperFast
    };
    public GameSpeed gameSpeed;                     // Current game speed

    private float tick;                             // Tick rate for the game speed
    private float rotation;                         // Rotation angle for the sun/moon object

    public float realSeconds = 60;                  // Real-time seconds per in-game second
    public float seconds = 0;                       // In-game seconds
    public int minutes = 0;                         // In-game minutes
    public int hours = 10;                          // In-game hours
    public int days = 0;                            // In-game days
    public int daysSurvived;                        // Number of in-game days survived
    public bool dayTime;                            // Flag for whether it is currently daytime

    ///////////////////

    void Start()
    {
        // Set the tick and rotation values based on the game speed
        if (gameSpeed == GameSpeed.Normal)
        {
            tick = 60;
            rotation = 360;
        }
        if (gameSpeed == GameSpeed.Fast)
        {
            tick = 600;
            rotation = 360;
        }
        if (gameSpeed == GameSpeed.SuperFast)
        {
            tick = 6000;
            rotation = 180;
        }
        realSeconds = 60;

        // Calculate the starting rotation angle based on the current time
        float secondsPerDay = 60 * 60 * 24;
        float secondsPerInGameDay = (Time.fixedDeltaTime * tick) / secondsPerDay;
        float startingSeconds = (seconds + minutes * 60 + hours * 60 * 60) / (Time.fixedDeltaTime * tick);
        float startingRotation = (secondsPerInGameDay * 360) * startingSeconds;
        sunMoon.transform.Rotate(0, 0, startingRotation + 180);// Add 180 so sun can be set to noon in scene view

        // Get the Light2D component attached to this GameObject
        sunlight = gameObject.GetComponent<Light2D>();

        // Set the stars to be transparent at the start
        stars.color = new Color(stars.color.r, stars.color.g, stars.color.b, 0);

        // Initialize the daysSurvived variable
        daysSurvived = 0;
    }

    void FixedUpdate()
    {
        CalcTime();                             // Update in-game time
        DisplayTime();                          // Display time and day number
        DaysSurvived();                         // Update days survived and adjust display transparency
        RotateSunMoon();                        // Rotate the sun/moon object based on time of day
        ControlSunlightIntesity();              // Adjust the intensity of the sun based on time of day
        ControlStars();                         // Adjust the opacity of stars based on time of day
    }

    public void CalcTime()
    {
        // Increase the real time counter by fixed delta time
        realSeconds += Time.fixedDeltaTime;
        // Increase the in-game time counter by fixed delta time multiplied by the current tick speed
        seconds += Time.fixedDeltaTime * tick;
        // If the seconds counter reaches 60, reset the seconds counter and increase the minutes counter
        if (seconds >= 60) 
        {
            // 
            seconds = 0;
            // 
            minutes++; 
        }
        // If the minutes counter reaches 60, reset the minutes counter and increase the hours counter
        if (minutes >= 60) 
        {
            minutes = 0;
            hours++; 
        }
        // If the hours counter reaches 24, reset the hours counter and increase the days counter
        // and increase the days survived counter
        if (hours >= 24) 
        {
            hours = 0;
            days++;
            daysSurvived++;
        }
    }
    public void DisplayTime()
    {
        // Update the time display text with the current hours and minutes in 00:00 format
        timeDisplay.text = (hours.ToString("00:") + minutes.ToString("00")); //+ seconds.ToString("00"));
        // Update the day display text with the current day number
        dayDisplay.text = "Day: " + days;

        // If the current time is between sunrise and sunset + 2 hours, set the dayTime boolean to TRUE
        // Otherwise, set dayTime boolean to FALSE
        if (hours > sunRise && hours < sunSet + 2)
        {
            dayTime = true;
        }
        else
        {
            dayTime = false;
        }
    }

    public void DaysSurvived()
    {
        // If the number of days is equal to 1 update the days survived display text with singular grammar
        // Otherwise, update the days survived display text with plural grammar
        if (days == 1)
            daysSurvivedDisplay.text = "SURVIVED " + daysSurvived + " DAY";
        else
            daysSurvivedDisplay.text = "SURVIVED " + daysSurvived + " DAYS";

        // If the current time is one hour after sunrise, reset the real time counter
        if (hours == sunRise + 1 && minutes == 0)
        {
            realSeconds = 0;
        }

        // If the real time counter is less than 5 seconds
        if (realSeconds < 5)
        {
            // Update the days survived display text color with alpha value based on the real time counter value (fading in)
            daysSurvivedDisplay.color = new Color
            (daysSurvivedDisplay.color.r, daysSurvivedDisplay.color.g, daysSurvivedDisplay.color.b, ((float)realSeconds) / 5);
        }

        // If the real time counter is between 5 and 10 seconds
        if (realSeconds < 10 && realSeconds > 5)
        {
            // Update the days survived display text color with alpha value based on the real time counter value (fading out)
            daysSurvivedDisplay.color = new Color
            (daysSurvivedDisplay.color.r, daysSurvivedDisplay.color.g, daysSurvivedDisplay.color.b, 1 - ((float)realSeconds -5 ) / 5);
        }
    }

    public void RotateSunMoon()
    {
        // Calculate the number of seconds per day and seconds per in-game day
        float secondsPerDay = 60 * 60 * 24;
        float secondsPerInGameDay = (Time.fixedDeltaTime * tick) / secondsPerDay;

        // Calculate the rotation angle based on the number of seconds per in-game day and the rotation value
        sunMoonRotation = secondsPerInGameDay * rotation;

        // Rotate the sun/moon object around its z-axis by the calculated rotation angle
        sunMoon.transform.Rotate(0, 0, sunMoonRotation);
    }

    public void ControlSunlightIntesity()
    {
        // IF hours at sunSet, gradually decrease the intensity of the sun over the course of 3 hours. Night time begins
        if (hours >= sunSet && hours < sunSet + 3)
        {
            sunlight.intensity = Mathf.Clamp((1 - ((float)minutes + ((hours - sunSet) * 60)) / 180), 0.1f, 1f);
        }
        // IF hours at sunRise, gradually increase the intensity of the sun over the course of 3 hours. Day time begins
        else if (hours >= sunRise && hours < sunRise + 3)
        {
            sunlight.intensity = Mathf.Clamp((((float)minutes + ((hours - sunRise) * 60)) / 180), 0.1f, 1f);
        }
    }

    public void ControlStars()
    {
        // IF hours at starsAppear, gradually increase the opacity of the stars over the course of 3 hours
        if (hours >= starsAppear && hours < starsAppear + 3)
        {
            stars.color = new Color(stars.color.r, stars.color.g, stars.color.b, ((float)minutes + ((hours - starsAppear) * 60)) / 180);
        }
        // IF hours at starsAppear, gradually decrease the opacity of the stars over the course of 3 hours
        else if (hours >= starsDisappear || hours < starsDisappear + 3)
        {
            stars.color = new Color(stars.color.r, stars.color.g, stars.color.b, 1 - ((float)minutes + ((hours - starsDisappear) * 60)) / 180);
        }
    }
}

// Code below was previously used to control daylight and starlight intensities but had flaws

//public Volume postExposVolume; postExposVolume = gameObject.GetComponent<Volume>();
//public SpriteRenderer[] stars;

/*public void ControlPPV()
{
    if (hours >= sunSet && hours < sunSet + 1)
    {
        postExposVolume.weight = (float)minutes / 180; 
    }
    if (hours >= sunSet + 1 && hours < sunSet + 2)
    {
        postExposVolume.weight = ((float)minutes + 60) / 180;
    }
    if (hours >= sunSet + 2 && hours < sunSet + 3)
    {
        postExposVolume.weight = ((float)minutes + 120) / 180;
    }

    if (hours >= sunRise && hours < sunRise + 1)
    {
        postExposVolume.weight = 1 - (float)minutes / 180;
    }
    if (hours >= sunRise + 1 && hours < sunRise + 2)
    {
        postExposVolume.weight = 1 - ((float)minutes + 60) / 180;
    }
    if (hours >= sunRise + 2 && hours < sunRise + 3)
    {
        postExposVolume.weight = 1 - ((float)minutes + 120) / 180;
    }
}*/

/*public void ControlStarAlpha()
{
    //stars appear
    if (hours >= starsAppear && hours < starsAppear + 1)
    {
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].color = new Color(stars[i].color.r, stars[i].color.g, stars[i].color.b, (float)minutes / 180);
        }
    }
    if (hours >= starsAppear + 1 && hours < starsAppear + 2)
    {
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].color = new Color(stars[i].color.r, stars[i].color.g, stars[i].color.b, ((float)minutes + 60) / 180);
        }
    }
    if (hours >= starsAppear + 2 && hours < starsAppear + 3)
    {
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].color = new Color(stars[i].color.r, stars[i].color.g, stars[i].color.b, ((float)minutes + 120) / 180);
        }
    }

    //stars disapear
    if (hours >= starsDisappear && hours < starsDisappear + 1)
    {
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].color = new Color(stars[i].color.r, stars[i].color.g, stars[i].color.b, 1 - (float)minutes / 180);
        }
    }
    if (hours >= starsDisappear + 1 && hours < starsDisappear + 2)
    {
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].color = new Color(stars[i].color.r, stars[i].color.g, stars[i].color.b, 1 - ((float)minutes + 60) / 180);
        }
    }
    if (hours >= starsDisappear + 2 && hours < starsDisappear + 3)
    {
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].color = new Color(stars[i].color.r, stars[i].color.g, stars[i].color.b, 1 - ((float)minutes + 120) / 180);
        }
    }
}*/