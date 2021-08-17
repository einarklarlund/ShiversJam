using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public float flickerPeriod;
    public float randomDeviation;
    
    // the light cone meshes must be set in the inspector so that the
    // script knows which meshes to flicker on/off (it shouldn't flicker
    // the mesh of the lightbulb/metal cage)
    [Tooltip("The meshes of the light cone that will flicker")]
    public MeshRenderer[] lightConeMeshes;

    [HideInInspector]
    public Light[] lights;

    // Start is called before the first frame update
    void Start()
    {
        lights = GetComponentsInChildren<Light>();

        StartCoroutine(WaitForFlicker());
    }

    IEnumerator WaitForFlicker()
    {
        yield return new WaitForSeconds(flickerPeriod + 2 * (Random.value - 0.5f) * randomDeviation);

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

        foreach(var mesh in lightConeMeshes)
        {
            mesh.enabled = active;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
