using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public float flickerPeriod;
    public float randomDeviation;

    [HideInInspector]
    public Light[] lights;
    
    [HideInInspector]
    public MeshRenderer[] meshes;    

    // Start is called before the first frame update
    void Start()
    {
        lights = GetComponentsInChildren<Light>();
        meshes = GetComponentsInChildren<MeshRenderer>();

        StartCoroutine(WaitForFlicker());
    }

    IEnumerator WaitForFlicker()
    {
        yield return new WaitForSeconds(flickerPeriod + Random.value * randomDeviation);

        StartCoroutine(Flicker());
    }

    IEnumerator Flicker()
    {
        int numFlickers = Random.Range(4, 8);
        
        for(int i = 0; i < numFlickers; i++)
        {
            SetLightsActive(false);
            yield return new WaitForSeconds(Random.value * 0.1f);
            
            SetLightsActive(true);
            yield return new WaitForSeconds(Random.value * 0.1f);
        }

        StartCoroutine(WaitForFlicker());
    }

    void SetLightsActive(bool active)
    {
        foreach(var light in lights)
        {
            light.enabled = active;
        }

        foreach(var mesh in meshes)
        {
            mesh.enabled = active;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
