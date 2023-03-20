using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float length;
    public GameObject cam;
    public float startPosX;
    public bool startPosXEnable;
    public float yHeight;
    public bool yFollowCam;
    [SerializeField] private float parallaxEffect;

    ///////////////////

    void Start()
    {
        if (startPosXEnable)
            startPosX = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        float distance = (cam.transform.position.x * parallaxEffect);
        float temporary = (cam.transform.position.x * (1 - parallaxEffect));

        if (yFollowCam == true)
            transform.position = new Vector3(startPosX + distance, cam.transform.position.y, transform.position.z);
        else
            transform.position = new Vector3(startPosX + distance, yHeight, transform.position.z);

        if (temporary > startPosX + length)
        {
            startPosX += length;
        }
        else if (temporary < startPosX - length)
        {
            startPosX -= length;
        }
    }
}
