using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DayNightCycle : MonoBehaviour
{
    public TextMeshProUGUI timeDisplay;
    public TextMeshProUGUI dayDisplay;
    public TextMeshProUGUI daysSurvivedDisplay;
    public Material stars;
    public Light2D sunlight;
    public GameObject sunMoon;
    private float sunMoonRotation;

    public int sunRise = 5;
    public int sunSet = 16;

    public int starsAppear = 18;
    public int starsDisappear = 3;

    public enum GameSpeed
    {
        Normal,
        Fast,
        SuperFast
    };
    public GameSpeed gameSpeed;

    private float tick;
    private float rotation;

    public float realSeconds = 60;
    public float seconds = 0;
    public int minutes = 0;
    public int hours = 10;
    public int days = 0;
    public int daysSurvived;

    public bool dayTime;

    ///////////////////

    void Start()
    {
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

        float secondsPerDay = 60 * 60 * 24;
        float secondsPerInGameDay = (Time.fixedDeltaTime * tick) / secondsPerDay;
        float startingSeconds = (seconds + minutes * 60 + hours * 60 * 60) / (Time.fixedDeltaTime * tick);
        float startingRotation = (secondsPerInGameDay * 360) * startingSeconds;
        sunMoon.transform.Rotate(0, 0, startingRotation + 180); //add 180 so sun can be set to noon in scene view

        sunlight = gameObject.GetComponent<Light2D>();

        stars.color = new Color(stars.color.r, stars.color.g, stars.color.b, 0);
        daysSurvived = 0;
    }

    void FixedUpdate()
    {
        CalcTime();
        DisplayTime();
        DaysSurvived();
        RotateSunMoon();
        ControlSunlightIntesity();
        ControlStars();
    }

    public void CalcTime()
    {
        realSeconds += Time.fixedDeltaTime;
        seconds += Time.fixedDeltaTime * tick;
        if (seconds >= 60)
        {
            seconds = 0;
            minutes++;
        }
        if (minutes >= 60)
        {
            minutes = 0;
            hours++;
        }
        if (hours >= 24)
        {
            hours = 0;
            days++;
            daysSurvived++;
        }
    }
    public void DisplayTime()
    {
        timeDisplay.text = (hours.ToString("00:") + minutes.ToString("00")); //+ seconds.ToString("00"));
        dayDisplay.text = "Day: " + days;

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
        if (days == 1)
            daysSurvivedDisplay.text = "SURVIVED " + daysSurvived + " DAY";
        else
            daysSurvivedDisplay.text = "SURVIVED " + daysSurvived + " DAYS";

        if (hours == sunRise + 1 && minutes == 0)
        {
            realSeconds = 0;
        }

        if (realSeconds < 5)
        {
            daysSurvivedDisplay.color = new Color(daysSurvivedDisplay.color.r, daysSurvivedDisplay.color.g, daysSurvivedDisplay.color.b, ((float)realSeconds) / 5);
        }

        if (realSeconds < 10 && realSeconds > 5)
        {
            daysSurvivedDisplay.color = new Color(daysSurvivedDisplay.color.r, daysSurvivedDisplay.color.g, daysSurvivedDisplay.color.b, 1 - ((float)realSeconds -5 ) / 5);
        }
    }

    public void RotateSunMoon()
    {
        float secondsPerDay = 60 * 60 * 24;
        float secondsPerInGameDay = (Time.fixedDeltaTime * tick) / secondsPerDay;
        sunMoonRotation = secondsPerInGameDay * rotation;
        sunMoon.transform.Rotate(0,0, sunMoonRotation);
    }

    public void ControlSunlightIntesity()
    {
        //night cycle
        if (hours >= sunSet && hours < sunSet +1) //dusk begins at public sunSet var and takes 3 hours to complete
        {
            sunlight.intensity = Mathf.Clamp((1 - (float)minutes / 180), 0.1f, 1f); //minutes = 60, slowly raises value. divide by 180 3 times for slow transition. need to add 60 each time.
        }
        if (hours >= sunSet+1 && hours < sunSet+2)
        {
            sunlight.intensity = Mathf.Clamp((1 - ((float)minutes + 60) / 180), 0.1f, 1f);
        }
        if (hours >= sunSet+2 && hours < sunSet+3)
        {
            sunlight.intensity = Mathf.Clamp((1 - ((float)minutes + 120) / 180), 0.1f, 1f);
        }

        //morning cycle
        if (hours >= sunRise && hours < sunRise + 1) //dawn begins at public sunRise var and takes 3 hours to complete
        {
            sunlight.intensity = Mathf.Clamp(((float)minutes / 180), 0.1f, 1f);
        }
        if (hours >= sunRise + 1 && hours < sunRise + 2)
        {
            sunlight.intensity = Mathf.Clamp((((float)minutes + 60) / 180), 0.1f, 1f);
        }
        if (hours >= sunRise + 2 && hours < sunRise + 3)
        {
            sunlight.intensity = Mathf.Clamp((((float)minutes + 120) / 180), 0.1f, 1f);
        }
    }

    public void ControlStars()
    {
        //stars appear
        if (hours >= starsAppear && hours < starsAppear + 1)
        {
            stars.color = new Color(stars.color.r, stars.color.g, stars.color.b, (float)minutes / 180);
        }
        if (hours >= starsAppear + 1 && hours < starsAppear + 2)
        {
            stars.color = new Color(stars.color.r, stars.color.g, stars.color.b, ((float)minutes + 60) / 180);
        }
        if (hours >= starsAppear + 2 && hours < starsAppear + 3)
        {
            stars.color = new Color(stars.color.r, stars.color.g, stars.color.b, ((float)minutes + 120) / 180);
        }
        //stars disapear
        if (hours >= starsDisappear && hours < starsDisappear + 1)
        {
            stars.color = new Color(stars.color.r, stars.color.g, stars.color.b, 1 - (float)minutes / 180);
        }
        if (hours >= starsDisappear + 1 && hours < starsDisappear + 2)
        {
            stars.color = new Color(stars.color.r, stars.color.g, stars.color.b, 1 - ((float)minutes + 60) / 180);
        }
        if (hours >= starsDisappear + 2 && hours < starsDisappear + 3)
        {
            stars.color = new Color(stars.color.r, stars.color.g, stars.color.b, 1 - ((float)minutes + 120) / 180);
        }
    }

    //public Volume postExposVolume; postExposVolume = gameObject.GetComponent<Volume>();
    //public SpriteRenderer[] stars;

    /*public void ControlPPV()
    {
        //night cycle
        if (hours >= sunSet && hours < sunSet + 1) //dusk begins at public sunSet var and takes 3 hours to complete
        {
            postExposVolume.weight = (float)minutes / 180; //minutes = 60, slowly raises value. divide by 180 3 times for slow transition. need to add 60 each time.
        }
        if (hours >= sunSet + 1 && hours < sunSet + 2)
        {
            postExposVolume.weight = ((float)minutes + 60) / 180;
        }
        if (hours >= sunSet + 2 && hours < sunSet + 3)
        {
            postExposVolume.weight = ((float)minutes + 120) / 180;
        }

        //morning cycle
        if (hours >= sunRise && hours < sunRise + 1) //dawn begins at public sunRise var and takes 3 hours to complete
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
}
