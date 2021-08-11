using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheFirstPerson;

public class PeriscopeMouseLook : MonoBehaviour
{
    public bool mouseLookEnabled;

    FPSController fpsController;

    // Start is called before the first frame update
    void Start()
    {
        fpsController = FindObjectOfType<FPSController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (mouseLookEnabled)
        {
            fpsController.mouseLocked = true;

            float horizontalLook = transform.localEulerAngles.y;
            var xMouse = Input.GetAxis("Mouse X");

            horizontalLook += xMouse * fpsController.sensitivity;

            horizontalLook = Mathf.Clamp(horizontalLook, 0, 180);

            transform.localEulerAngles = new Vector3(0, horizontalLook, 0);
        }
    }
}
