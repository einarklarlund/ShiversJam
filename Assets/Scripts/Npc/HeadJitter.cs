using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadJitter : MonoBehaviour
{
    public float displacement = 0;
    public int framesBeforeUpdate = 2;

    Vector3 _initialPosition;
    int _framesWaited = 0;

    void Start()
    {
        _initialPosition = transform.localPosition;
    }

    void Update()
    {
        if(++_framesWaited < framesBeforeUpdate)
            return;

        Vector3 randomDisplacement = new Vector3((Random.value - 0.5f) * displacement, (Random.value - 0.5f) * displacement, 0f);
        transform.localPosition = _initialPosition + randomDisplacement;

        _framesWaited = 0;
    }
}
