using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TorchLightGlow : MonoBehaviour
{
    private Light2D torchLight;
    public float intensityCounter;
    public float radiusCounter;
    private bool intensityMax;
    private bool radiusMax;

    void Start()
    {
        torchLight= GetComponent<Light2D>();

        intensityCounter = 80;
        radiusCounter = 90;

        intensityMax = false;
        radiusMax = false;
    }

    void Update()
    {
        if (intensityCounter <= 100 && intensityMax == false)
        {
            intensityCounter += intensityCounter * Time.fixedDeltaTime;
        }
        if (radiusCounter <= 100 && radiusMax == false)
        {
            radiusCounter += radiusCounter * Time.fixedDeltaTime;
        }
        if (intensityCounter >= 100)
        {
            intensityMax = true;
        }
        if (radiusCounter >= 100)
        {
            radiusMax = true;
        }

        if (intensityCounter <= 102 && intensityMax == true)
        {
            intensityCounter -= intensityCounter * Time.fixedDeltaTime;
        }
        if (radiusCounter <= 102 && radiusMax == true)
        {
            radiusCounter -= radiusCounter * Time.fixedDeltaTime;
        }
        if (intensityCounter <= 80)
        {
            intensityMax = false;
        }
        if (radiusCounter <= 90)
        {
            radiusMax = false;
        }

        
        //torchLight.intensity = intensityCounter * 0.01f;
        torchLight.pointLightOuterRadius = radiusCounter * 0.1f;
    }
}
