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
        Vector3 target = Quaternion.LookRotation(cam.transform.forward).eulerAngles;
        if(!completelyFreeMovement)
            transform.eulerAngles = 
                new Vector3(0, target.y,0);
        else
            transform.eulerAngles = target;
    }
}
