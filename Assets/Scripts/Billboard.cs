using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour
{
    public bool completelyFreeMovement = false; 
	
    void Start ()
    {
        if (cam == null)
        {
            cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Transform>();
        }
    }

    public static Transform cam;

    void LateUpdate()
    {
        // this.transform.LookAt(cam);
        // eangles = transform.eulerAngles;
        // eangles.x *= freeRotation.x;
        // eangles.y *= freeRotation.y;
        // eangles.z *= freeRotation.z;
        // Debug.Log(new Vector3(cam.transform.eulerAngles.x, cam.transform.eulerAngles.y, cam.transform.eulerAngles.z));
        if(!completelyFreeMovement)
            transform.localEulerAngles = 
                new Vector3(0, cam.transform.eulerAngles.y - 180,0);
        else
            transform.localEulerAngles = 
                new Vector3(cam.transform.eulerAngles.x, cam.transform.eulerAngles.y, cam.transform.eulerAngles.z);

    }
}
