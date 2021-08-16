using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SelectiveAudioSourceGroup : MonoBehaviour
{
    [HideInInspector]
    public SelectiveAudioSourceController[] selectiveAudioSources;
    public float fadeInDuration = 1.5f;
    public float fadeOutDuration = 3.0f;

    List<SelectiveAudioSourceGroup> _otherAudioSourceGroups;



    void Start()
    {
        _otherAudioSourceGroups = FindObjectsOfType<SelectiveAudioSourceGroup>()
            .ToList<SelectiveAudioSourceGroup>();

        _otherAudioSourceGroups.Remove(this);

        selectiveAudioSources = GetComponentsInChildren<SelectiveAudioSourceController>();
    }

    public void SetAudioSourcesEnabled(bool enabled, float fadeDuration = 0)
    {
        foreach(var selectiveAudioSource in selectiveAudioSources)
        {
            if(enabled)
            {
                if(selectiveAudioSource.canOnlyBeHeardInGroupCollider)
                {
                    selectiveAudioSource.Play(fadeDuration);
                }
            }
            else
            {
                if(selectiveAudioSource.canOnlyBeHeardInGroupCollider)
                {
                    selectiveAudioSource.Stop(fadeDuration);
                }
            }
        }
    }

    public void SetOtherAudioSourceGroupsEnabled(bool enabled)
    {
        foreach(var audioSourceGroup in _otherAudioSourceGroups)
        {
            audioSourceGroup.SetAudioSourcesEnabled(enabled, fadeOutDuration);
        }
    }
}
