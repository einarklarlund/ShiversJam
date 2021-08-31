using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyDontDestroyOnLoad : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var instance = GameObject.Find(name);
        if(instance && instance.GetInstanceID() != gameObject.GetInstanceID())
        {
            Debug.Log("found instance of " + name);
            Destroy(gameObject);
        }

        DontDestroyOnLoad(this);
    }
}
