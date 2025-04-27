using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event_TransformRotate : MonoBehaviour
{
    [SerializeField]
    protected float speed = 1.0f;

    [SerializeField]
    protected float degrees = 360.0f;

    [SerializeField]
    protected bool loop = true;

    [SerializeField]
    protected bool pause = true;

    [SerializeField]
    protected float pauseDuration = 1.0f;

    private float localDegrees = 0.0f;
    private float localPauseTime = 0.0f;
    private bool isPlaying = false;
    private bool isPaused = false;
    private Vector3 cacheRotation;

    private void Awake()
    {
        cacheRotation = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z);
    }

    private void OnEnable()
    {
        ResetThis();
    }

    private void OnDisable()
    {
        isPlaying = false;
    }

    private void Update()
    {
        if (!isPlaying) return;

        if (localDegrees < degrees)
        {
            transform.Rotate(0, 0, -(Time.deltaTime * speed) * 10);

            localDegrees += (Time.deltaTime * speed) * 10;
        }
        else
        {
            if (loop)
            {
                if (pause && !isPaused)
                {
                    isPaused = true;
                }
            }
        }

        if (isPaused)
        {
            if (localPauseTime < pauseDuration)
            {
                localPauseTime += Time.deltaTime;
            }
            else
            {
                isPlaying = false;
                ResetThis();
            }
        }
    }

    private void ResetThis()
    {
        localDegrees = 0.0f;
        localPauseTime = 0.0f;

        isPaused = false;

        transform.localEulerAngles = cacheRotation;

        isPlaying = true;
    }
}
