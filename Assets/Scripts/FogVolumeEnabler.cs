using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using HauntedPSX.RenderPipelines.PSX.Runtime;

public class FogVolumeEnabler : MonoBehaviour
{
    public void SetFogEnabled(bool enabled)
    {
        var volume = this.FindComponent<Volume>();
        FogVolume fogVolume;

        // get reference to the sky volume and store it 
        var profile = volume.profile;
        profile.TryGet<FogVolume>(out fogVolume);

        fogVolume.isEnabled.overrideState = true;
        Debug.Log($"setting fog enabled to {enabled}");
        fogVolume.isEnabled.value = enabled;
    }
}
